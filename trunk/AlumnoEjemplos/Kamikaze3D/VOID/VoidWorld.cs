using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.VOID
{
    public class VoidBloque
    {
        //Estructura para almacenar los objetos del bloque
        public VoidMeshShader[] structure = new VoidMeshShader[23];

        //Estrucutra para almacenar los objetos logicos que simulan la forma real de los edificios visibles
        public TgcBoundingBox[] structureBB = new TgcBoundingBox[69];

        /// <summary>
        /// Carga en la estructura de VoidMeshShader[] los objetos de la cuadra
        /// </summary>
        public void cargarBloque()
        {
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            //Carga el archivo del bloque
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Block\\Block-TgcScene.xml");

            #region Array structure[]

            //Variables banderas
            int numeroObjeto = 0;

            //Cargo el bloque de la cuadra del scene
            structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("Bloque");
            numeroObjeto++;

            //Cargo la calle o vereda del scene
            structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("Vereda");
            numeroObjeto++;

            //Cargo los semaforos del scene
            foreach (TgcMesh edificio in scene.Meshes)
            {
                if (String.Compare(edificio.Name, 0, "Semaforo", 0, 8) == 0)
                {
                    structure[numeroObjeto] = (VoidMeshShader)edificio;
                    numeroObjeto++;
                }
            }

            //Cargo los objetos edificios del scene
            foreach (TgcMesh edificio in scene.Meshes)
            {
                if (String.Compare(edificio.Name, 0, "Estructura", 0, 9) == 0)
                {
                    structure[numeroObjeto] = (VoidMeshShader)edificio;
                    numeroObjeto++;
                }
            }

            //Cargar los autos
            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("taxi1");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("taxi2");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("autoVioleta");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("audi");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("autoSport");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("autoPolicia");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("camioneta");
            //numeroObjeto++;

            //Cargar los puestos de diarios
            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("diarios1");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("diarios2");
            //numeroObjeto++;

            //Cargar los parquimetros
            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("pkmeter");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("pkmeter2");
            //numeroObjeto++;

            //Cargar Hidrante para Bomberos
            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("hidrante1");
            //numeroObjeto++;

            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("hidrante2");
            //numeroObjeto++;

            //Cargo la fuente
            //structure[numeroObjeto] = (VoidMeshShader)scene.getMeshByName("fuente");
            //numeroObjeto++;

            #endregion

            #region Array structureBB[]

            //Variables banderas
            numeroObjeto = 0;

            //Bloque
            structureBB[numeroObjeto] = (scene.getMeshByName("Bloque")).BoundingBox;
            numeroObjeto++;

            //Vereda
            structureBB[numeroObjeto] = (scene.getMeshByName("Vereda")).BoundingBox;
            numeroObjeto++;

            //BBlogicos
            foreach (TgcMesh BBstructure in scene.Meshes)
            {
                if (String.Compare(BBstructure.Name, 0, "Box", 0, 3) == 0)
                {
                    structureBB[numeroObjeto] = BBstructure.BoundingBox;
                    numeroObjeto++;
                }
            }

            #endregion

        }

        /// <summary>
        /// Carga el shader para cada objeto de la cuadra e inicializa sus valores estaticos
        /// </summary>
        public void cargarShader()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            string compilationErrors;

            //Carga el shader
            foreach (VoidMeshShader structureActual in structure)
            {
                structureActual.Effect = Microsoft.DirectX.Direct3D.Effect.FromFile(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "VOID\\Shader\\VoidShader3.2.fx", null, null, ShaderFlags.None, null, out compilationErrors);
                if (structureActual.Effect == null)
                {
                    throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
                }

                //Definir tecnica por default
                structureActual.Effect.Technique = "VoidFullTransformationTechnique";

                //Colorea las zonas por defecto del Modelo
                structureActual.setColor(Color.White);

                //Cargar variables constantes del shader
                structureActual.Effect.SetValue("fvAmbient", ColorValue.FromColor(DaylightSystem.ambientColor));
                structureActual.Effect.SetValue("fvDiffuse", ColorValue.FromColor(DaylightSystem.diffuseColor));
                structureActual.Effect.SetValue("fvSpecular", ColorValue.FromColor(Color.FromArgb(190, 190, 190)));
                if (structureActual.Name == "Bloque")
                    structureActual.Effect.SetValue("fvSpecularIntensity", 0.28f);
                else
                    structureActual.Effect.SetValue("fvSpecularIntensity", 0.95f);
            }
        }

        /// <summary>
        /// Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        public void move(Vector3 translate)
        {
            foreach (VoidMeshShader structureActual in structure)
            {
                structureActual.move(translate.X, translate.Y, translate.Z);
            }

            foreach (TgcBoundingBox BB in structureBB)
            {
                BB.move(translate);
            }
            //structure[0].BoundingBox.move(new Vector3(0, structure[1].BoundingBox.Position.Y - structure[0].BoundingBox.Position.Y, 0));
        }

        /// <summary>
        /// Desplaza toda la cuadra al origen
        /// </summary>
        public void moveOrigen()
        {
            foreach (VoidMeshShader structureActual in structure)
            {
                structureActual.moveAbsolute(new Vector3());
            }

            foreach (TgcBoundingBox BB in structureBB)
            {
                BB.move(new Vector3());
            }
        }

        /// <summary>
        /// Renderiza todos los objetos del bloque
        /// </summary>
        public void render()
        {
            foreach (VoidMeshShader structureActual in structure)
            {
                structureActual.render();
            }
        }

        /// <summary>
        /// Renderiza solo los objetos que estan dentro del frustum (por fuerza bruta)
        /// </summary>
        public void renderInsideFrustum()
        {
            //Instancia del frustum
            TgcFrustum frustum = GuiController.Instance.Frustum;
            //Analiza por cada objeto si esta dentro del frustum, si lo esta lo renderiza
            foreach (VoidMeshShader objeto in structure)
            {
                TgcCollisionUtils.FrustumResult Result = TgcCollisionUtils.classifyFrustumAABB(frustum, objeto.BoundingBox);
                if (Result != TgcCollisionUtils.FrustumResult.OUTSIDE)
                {
                    objeto.render();
                }
            }
        }

        /// <summary>
        /// Libera la memoria de los objetos de la cuadra
        /// </summary>
        public void dispose()
        {
            foreach (VoidMeshShader structureActual in structure)
            {
                structureActual.dispose();
                structureActual.Effect.Dispose();
            }
        }

        /// <summary>
        /// Establece los valores dinamicos del shader
        /// </summary>
        public void actualizarShader(Vector3 lightPosition, Vector3 camaraPosition, Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            foreach (VoidMeshShader structureActual in structure)
            {
                structureActual.Effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
                structureActual.Effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(camaraPosition));
                structureActual.Effect.SetValue("matView", d3dDevice.Transform.View);
                structureActual.Effect.SetValue("matProjection", d3dDevice.Transform.Projection);
                structureActual.Effect.SetValue("matWorld", structureActual.Transform);

                //Si hubo cambios en el modifier del Daylight, actualiza el color de las iluminaciones
                if (DaylightSystem.dirtyValues)
                {
                    structureActual.Effect.SetValue("fvAmbient", ColorValue.FromColor(DaylightSystem.ambientColor));
                    structureActual.Effect.SetValue("fvDiffuse", ColorValue.FromColor(DaylightSystem.diffuseColor));
                }
            }
        }

        /// <summary>
        /// Establece todos los valores del shader
        /// </summary>
        public void actualizarShaderTodo(Vector3 lightPosition, Vector3 camaraPosition, Microsoft.DirectX.Direct3D.Device d3dDevice, ColorValue ambient, ColorValue diffuse, ColorValue specular, float specularIntensity)
        {
            foreach (VoidMeshShader structureActual in structure)
            {
                structureActual.Effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
                structureActual.Effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(camaraPosition));
                structureActual.Effect.SetValue("matView", d3dDevice.Transform.View);
                structureActual.Effect.SetValue("matProjection", d3dDevice.Transform.Projection);
                structureActual.Effect.SetValue("matWorld", structureActual.Transform);

                structureActual.Effect.SetValue("fvAmbient", ambient);
                structureActual.Effect.SetValue("fvDiffuse", diffuse);
                structureActual.Effect.SetValue("fvSpecular", specular);
                if (structureActual.Name != "Bloque")
                    structureActual.Effect.SetValue("fvSpecularIntensity", specularIntensity);
            }
        }
    }

    public class VoidMap
    {
        //Tipo de valor que devuelve la función RevisarColisionPiso
        public enum TipoColision
        {
            COL_NADA,
            COL_SUELO,
            COL_VEREDA
        };

        //Caja con el mismo color que la niebla
        private TgcBox background = new TgcBox();

        //Estructura para almacenar los bloques
        public VoidBloque[] blocks;

        //Estructura que almacena la posicion de los bloques
        private int[,] blocksMatrix = new int[3, 3];

        Vector3 lightPosition = new Vector3(0, 0, 0);
        /// <summary>
        /// Posiciona la fuente de luz en el escenario para calcular las iluminaciones del shader
        /// </summary>
        public Vector3 setLightPosition
        {
            set { lightPosition = value; }
        }

        Vector3 camaraPosition = new Vector3(0, 0, 0);
        /// <summary>
        /// Posiciona la camara en el escenario para calcular las iluminaciones del shader
        /// </summary>
        public Vector3 setCamaraPosition
        {
            set { camaraPosition = value; }
        }

        /// <summary>
        /// Iniciliza el Mapa (carga las cuadras y sus shaders)
        /// </summary>
        public void iniciar()
        {
            //Crea el background
            background = TgcBox.fromSize(new Vector3(4000, 4000, 4000), Color.FromArgb(45, 45, 45));

            blocks = new VoidBloque[9];
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i] = new VoidBloque();
                blocks[i].cargarBloque();
            }

            //Cargar Shader
            foreach (VoidBloque Block in blocks)
            {
                Block.cargarShader();
            }

            //Ubica y Escala los bloques
            ajustarBloques();
        }

        /// <summary>
        /// Ubica los bloques entre ellos
        /// </summary>
        private void ajustarBloques()
        {
            int nroBlock = 0, fila = 0, columna = 0;
            for (int y = 2000; y >= -2000; y -= 2000)
            {
                columna = 0;
                for (int x = -2000; x <= 2000; x += 2000)
                {
                    blocks[nroBlock].move(new Vector3(x, 0, y));
                    blocksMatrix[fila, columna] = nroBlock;

                    nroBlock++;
                    columna++;
                }
                fila++;
            }
        }

        /// <summary>
        /// Mueve todos los bloques respecto de su posicion actual y revisa si hace falta reposicionar alguno
        /// </summary>
        public void move(Vector3 posicion)
        {
            foreach (VoidBloque Block in blocks)
            {
                Block.move(posicion);
            }
            checkPosiciones();
        }

        /// <summary>
        /// Mueve todos los bloques al origen
        /// </summary>
        public void moveDefaultPosition()
        {
            //Mueve todas las cuadras al origen
            foreach (VoidBloque Block in blocks)
            {
                Block.moveOrigen();
            }

            //Reubica las cuadras
            ajustarBloques();
        }

        /// <summary>
        /// Revisa las posiciones de los bloques y reubica los lejanos hacia donde uno se dezplaza
        /// </summary>
        private void checkPosiciones()
        {
            Vector2 posicionBloqueCentral = new Vector2(blocks[blocksMatrix[1, 1]].structure[0].Position.X, blocks[blocksMatrix[1, 1]].structure[0].Position.Z);

            int[,] matrixTemp = new int[3, 3];

            if (posicionBloqueCentral.X > 1000)
            {
                for (int i = 0; i < 3; i++)
                {
                    blocks[blocksMatrix[i, 2]].move(new Vector3(-6000, 0, 0));
                    matrixTemp[i, 0] = blocksMatrix[i, 2];
                    matrixTemp[i, 1] = blocksMatrix[i, 0];
                    matrixTemp[i, 2] = blocksMatrix[i, 1];
                }
                blocksMatrix = matrixTemp;
            }

            if (posicionBloqueCentral.X < -1000)
            {
                for (int i = 0; i < 3; i++)
                {
                    blocks[blocksMatrix[i, 0]].move(new Vector3(6000, 0, 0));
                    matrixTemp[i, 0] = blocksMatrix[i, 1];
                    matrixTemp[i, 1] = blocksMatrix[i, 2];
                    matrixTemp[i, 2] = blocksMatrix[i, 0];
                }
                blocksMatrix = matrixTemp;
            }


            if (posicionBloqueCentral.Y > 1000)
            {
                for (int i = 0; i < 3; i++)
                {
                    blocks[blocksMatrix[0, i]].move(new Vector3(0, 0, -6000));
                    matrixTemp[0, i] = blocksMatrix[1, i];
                    matrixTemp[1, i] = blocksMatrix[2, i];
                    matrixTemp[2, i] = blocksMatrix[0, i];
                }
                blocksMatrix = matrixTemp;
            }

            if (posicionBloqueCentral.Y < -1000)
            {
                for (int i = 0; i < 3; i++)
                {
                    blocks[blocksMatrix[2, i]].move(new Vector3(0, 0, 6000));
                    matrixTemp[0, i] = blocksMatrix[2, i];
                    matrixTemp[1, i] = blocksMatrix[0, i];
                    matrixTemp[2, i] = blocksMatrix[1, i];
                }
                blocksMatrix = matrixTemp;
            }
        }

        /// <summary>
        /// Renderiza cada cuadra sin ningun metodo de optimizacion y actualiza las variables del shader para el mapa
        /// </summary>
        public void renderAll()
        {
            actualizarShaderAll();

            foreach (VoidBloque Block in blocks)
            {
                Block.render();
            }

            background.render();
        }

        /// <summary>
        /// Renderiza una lista de cuadras y de cada cuadra si analiza que objetos renderizar por fuerza bruta. Tambien actualiza las variables del shader para el mapa
        /// </summary>
        public void renderList(ref List<VoidBloque> Cuadras)
        {
            //actualizarShaderList(ref Cuadras);
            actualizarShaderAll();

            foreach (VoidBloque Block in Cuadras)
            {
                Block.renderInsideFrustum();
            }

            background.render();
        }

        /// <summary>
        /// Libera la memoria de los objetos de las cuadras
        /// </summary>
        public void dispose()
        {
            if (blocks != null)
            {
                foreach (VoidBloque block in blocks)
                {
                    foreach (VoidMeshShader structureActual in block.structure)
                    {
                        structureActual.dispose();
                        structureActual.Effect.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Actualiza las variables del shader para cada bloque. Las posiciones son absolutas, no relativas respecto a los bloques
        /// </summary>
        private void actualizarShaderAll()
        {
            //Instancia del gpu
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            foreach (VoidBloque Block in blocks)
            {
                //Actualiza las variables del shader
                Block.actualizarShader(lightPosition, camaraPosition, d3dDevice);
            }
        }

        /// <summary>
        /// Actualiza las variables del shader para ciertos bloque. Las posiciones son absolutas, no relativas respecto a los bloques
        /// </summary>
        private void actualizarShaderList(ref List<VoidBloque> Cuadras)
        {
            //Instancia del gpu
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            foreach (VoidBloque Block in Cuadras)
            {
                //Actualiza las variables del shader
                Block.actualizarShader(lightPosition, camaraPosition, d3dDevice);
            }
        }
  
    }

    public class VoidMapLowPoly
    {
        //Estructura para almacenar los bloques
        VoidMeshShader[] blocks = new VoidMeshShader[9];

        //Estructura que almacena la posicion de los bloques
        private int[,] blocksMatrix = new int[3, 3];

        Vector3 lightPosition = new Vector3(0, 0, 0);
        /// <summary>
        /// Posiciona la fuente de luz en el escenario para calcular las iluminaciones del shader
        /// </summary>
        public Vector3 setLightPosition
        {
            set { lightPosition = value; }
        }

        Vector3 camaraPosition = new Vector3(0, 0, 0);
        /// <summary>
        /// Posiciona la camara en el escenario para calcular las iluminaciones del shader
        /// </summary>
        public Vector3 setCamaraPosition
        {
            set { camaraPosition = value; }
        }

        /// <summary>
        /// Iniciliza el Mapa
        /// </summary>
        /// <param name="angle">Handle al dispositivo 3d</param>
        public void iniciar(Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            for (int i = 0; i < blocks.Length; i++)
            {
                TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "BlockLowPoly\\Block-TgcScene.xml");
                blocks[i] = (VoidMeshShader)scene.Meshes[0];
            }

            //Cargar Shader
            string compilationErrors;
            foreach (VoidMeshShader Block in blocks)
            {
                Block.Effect = Microsoft.DirectX.Direct3D.Effect.FromFile(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "Shader\\VoidShader3.2.fx", null, null, ShaderFlags.None, null, out compilationErrors);
                if (Block.Effect == null)
                {
                    throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
                }
                //Definir tecnica por default
                Block.Effect.Technique = "VoidFullTransformationTechnique";

                //Colorea las zonas por defecto del Modelo
                Color defaultColor = Color.White;
                Block.setColor(defaultColor);

                //Cargar variables constantes del shader
                Block.Effect.SetValue("fvAmbient", ColorValue.FromColor((Color)Color.FromArgb(80, 80, 80)));
                Block.Effect.SetValue("fvDiffuse", ColorValue.FromColor((Color)Color.FromArgb(220, 220, 220)));
                Block.Effect.SetValue("fvSpecular", ColorValue.FromColor((Color)Color.White));
                Block.Effect.SetValue("fvSpecularIntensity", 0.09f);
            }

            //Ubica y Escala los bloques
            AjustarBloques();
        }

        /// <summary>
        /// Ubica los bloques entre ellos
        /// </summary>
        private void AjustarBloques()
        {
            int nroBlock = 0, fila = 0, columna = 0;
            for (int y = 2000; y >= -2000; y -= 2000)
            {
                columna = 0;
                for (int x = -2000; x <= 2000; x += 2000)
                {
                    blocks[nroBlock].move(new Vector3(x, 0, y));
                    blocksMatrix[fila, columna] = nroBlock;

                    nroBlock++;
                    columna++;
                }
                fila++;
            }

            move(new Vector3(-900, 0, 0));
        }

        /// <summary>
        /// Mueve todos los bloques respecto de su posicion actual y revisa si hace falta reposicionar alguno
        /// </summary>
        public void move(Vector3 posicion)
        {
            foreach (VoidMeshShader Block in blocks)
            {
                Block.move(posicion);
            }
            checkPosiciones();
        }

        /// <summary>
        /// Revisa las posiciones de los bloques y reubica los lejanos hacia donde uno se dezplaza
        /// </summary>
        private void checkPosiciones()
        {
            Vector2 posicionBloqueCentral = new Vector2(blocks[blocksMatrix[1, 1]].Position.X, blocks[blocksMatrix[1, 1]].Position.Z);

            int[,] matrixTemp = new int[3, 3];

            if (posicionBloqueCentral.X > 1000)
            {
                for (int i = 0; i < 3; i++)
                {
                    blocks[blocksMatrix[i, 2]].move(new Vector3(-6000, 0, 0));
                    matrixTemp[i, 0] = blocksMatrix[i, 2];
                    matrixTemp[i, 1] = blocksMatrix[i, 0];
                    matrixTemp[i, 2] = blocksMatrix[i, 1];
                }
                blocksMatrix = matrixTemp;
            }

            if (posicionBloqueCentral.X < -1000)
            {
                for (int i = 0; i < 3; i++)
                {
                    blocks[blocksMatrix[i, 0]].move(new Vector3(6000, 0, 0));
                    matrixTemp[i, 0] = blocksMatrix[i, 1];
                    matrixTemp[i, 1] = blocksMatrix[i, 2];
                    matrixTemp[i, 2] = blocksMatrix[i, 0];
                }
                blocksMatrix = matrixTemp;
            }


            if (posicionBloqueCentral.Y > 1000)
            {
                for (int i = 0; i < 3; i++)
                {
                    blocks[blocksMatrix[0, i]].move(new Vector3(0, 0, -6000));
                    matrixTemp[0, i] = blocksMatrix[1, i];
                    matrixTemp[1, i] = blocksMatrix[2, i];
                    matrixTemp[2, i] = blocksMatrix[0, i];
                }
                blocksMatrix = matrixTemp;
            }

            if (posicionBloqueCentral.Y < -1000)
            {
                for (int i = 0; i < 3; i++)
                {
                    blocks[blocksMatrix[2, i]].move(new Vector3(0, 0, 6000));
                    matrixTemp[0, i] = blocksMatrix[2, i];
                    matrixTemp[1, i] = blocksMatrix[0, i];
                    matrixTemp[2, i] = blocksMatrix[1, i];
                }
                blocksMatrix = matrixTemp;
            }
        }

        /// <summary>
        /// Rota todos los bloques respecto al (0,0,0)
        /// </summary>
        public void absoluteRotateY(float angle)
        {
            foreach (VoidMeshShader Block in blocks)
            {
                Block.absoluteRotateY(angle);
            }
        }

        public void render()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            actualizarVariablesShader(d3dDevice);

            foreach (VoidMeshShader Block in blocks)
            {
                Block.render();
            }
        }

        private void actualizarVariablesShader(Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            foreach (VoidMeshShader Block in blocks)
            {
                //Actualiza las variables del shader
                Block.Effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
                Block.Effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(camaraPosition));
                Block.Effect.SetValue("matView", d3dDevice.Transform.View);
                Block.Effect.SetValue("matProjection", d3dDevice.Transform.Projection);
                Block.Effect.SetValue("matWorld", Block.Transform);
            }

        }

        public bool RevisarColision(TgcBoundingBox delorean, ref TgcBoundingBox colisionado)
        {
            foreach (VoidMeshShader obstaculo in blocks)
            {

                Vector3 menor = obstaculo.BoundingBox.PMin;
                menor.X += 140;
                menor.Z += 140;
                Vector3 mayor = obstaculo.BoundingBox.PMax;
                mayor.X -= 140;
                mayor.Z -= 140;
                //obstaculo.BoundingBox.setExtremes(menor, mayor);
                //  obstaculo.BoundingBox.executeRender();
                TgcBoundingBox bbox = new TgcBoundingBox(menor, mayor);
                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(delorean, bbox);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    colisionado = bbox;
                    return true;
                }
            }
            return false;
        }

    }
}