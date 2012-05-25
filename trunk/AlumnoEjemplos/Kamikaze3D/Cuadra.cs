using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using AlumnoEjemplos.VOID;

namespace AlumnoEjemplos.Kamikaze3D
{
    /// <summary>
    /// Kamikaze3D
    /// </summary>
    public class Cuadra
    {

        private TgcScene scene;
        private TgcFrustum frustum;
        private VoidMeshShader[] objects = new VoidMeshShader[23];
        private Color ambientColor = Color.Black;
        private Color diffuseColor = Color.FromArgb(160, 160, 88);
        
        public Cuadra()
        {
            this.frustum = GuiController.Instance.Frustum;
        }

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// </summary>
        public void init()
        {

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            //Carga el archivo del bloque
            this.scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Block\\Block-TgcScene.xml");
            //this.scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "_Scene Completa Mesh Unidos\\Block-TgcScene.xml");

            this.loadObjects();
        }

        /// <summary>
        /// Método que posiciona las cuadras una al lado de la otra
        /// </summary>
        /// <param name="index">Número de cuadra</param>
        public void position(float x, float y, float z)
        {
            //Primer cuadra, no mover
            if(x == 0 && y == 0 && z == 0)
                return ;

            foreach(TgcMesh mesh in this.scene.Meshes)
                mesh.move(x, y, z);
        }


        /// <summary>
        /// Accede a los distintos objetos de la cuadra para poder aplicarles shaders posteriormente
        /// </summary>
        private void loadObjects()
        {
            int numeroObjeto = 0;

            //Cargo el bloque de la cuadra del scene
            this.objects[numeroObjeto] = (VoidMeshShader)this.scene.getMeshByName("Bloque");
            numeroObjeto++;

            //Cargo la calle o vereda del scene
            this.objects[numeroObjeto] = (VoidMeshShader)this.scene.getMeshByName("Vereda");
            numeroObjeto++;

            //Cargo los semaforos del scene
            foreach (TgcMesh edificio in this.scene.Meshes)
            {
                if (String.Compare(edificio.Name, 0, "Semaforo", 0, 8) == 0)
                {
                    this.objects[numeroObjeto] = (VoidMeshShader)edificio;
                    numeroObjeto++;
                }
            }

            //Cargo los objetos edificios del scene
            foreach (TgcMesh edificio in this.scene.Meshes)
            {
                if (String.Compare(edificio.Name, 0, "Estructura", 0, 9) == 0)
                {
                    this.objects[numeroObjeto] = (VoidMeshShader)edificio;
                    numeroObjeto++;
                }
            }

            this.loadShader();

        }

        /// <summary>
        /// Accede a los meshes de la cuadra
        /// </summary>
        public List<TgcMesh> getMeshes()
        {
            return this.scene.Meshes;
        }

        /// <summary>
        /// Carga el shader para cada objeto de la cuadra e inicializa sus valores estaticos
        /// </summary>
        private void loadShader()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            string compilationErrors;

            //Carga el shader
            foreach (VoidMeshShader obj in this.objects)
            {
                obj.Effect = Microsoft.DirectX.Direct3D.Effect.FromFile(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Shader\\VoidShader3.2.fx", null, null, ShaderFlags.None, null, out compilationErrors);
                if (obj.Effect == null)
                {
                    throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
                }

                //Definir tecnica por default
                obj.Effect.Technique = "VoidFullTransformationTechnique";

                //Colorea las zonas por defecto del Modelo
                obj.setColor(Color.White);

                //Cargar variables constantes del shader
                obj.Effect.SetValue("fvAmbient", ColorValue.FromColor(this.ambientColor));
                obj.Effect.SetValue("fvDiffuse", ColorValue.FromColor(this.diffuseColor));
                obj.Effect.SetValue("fvSpecular", ColorValue.FromColor(Color.FromArgb(190, 190, 190)));
                if (obj.Name == "Bloque")
                    obj.Effect.SetValue("fvSpecularIntensity", 0.28f);
                else
                    obj.Effect.SetValue("fvSpecularIntensity", 0.95f);
            }
        }

        /// <summary>
        /// Establece los valores dinamicos del shader
        /// </summary>
        public void updateShader(Vector3 lightPosition, Vector3 camaraPosition)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            foreach (VoidMeshShader obj in this.objects)
            {
                obj.Effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
                obj.Effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(camaraPosition));
                obj.Effect.SetValue("matView", d3dDevice.Transform.View);
                obj.Effect.SetValue("matProjection", d3dDevice.Transform.Projection);
                obj.Effect.SetValue("matWorld", obj.Transform);
            }
        }

        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public void render(float elapsedTime, Camara camara)
        {

            //Ver cual de las mallas se interponen en la visión de la cámara en 3ra persona.
            List<TgcMesh> objectsInFront = new List<TgcMesh>();

            foreach (TgcMesh mesh in this.scene.Meshes)
            {
                Vector3 q;
                if (mesh.Name == "Vereda" || mesh.Name == "Bloque") //Vereda y calle siempre visible
                    objectsInFront.Add(mesh);
                else if (!TgcCollisionUtils.intersectSegmentAABB(camara.Position, camara.Target, mesh.BoundingBox, out q))
                    if (String.Compare(mesh.Name, 0, "Box", 0, 3) != 0) //No renderizar bloques azules
                        objectsInFront.Add(mesh);
            }

            //Render mallas que no se interponen
            foreach (TgcMesh mesh in objectsInFront)
                mesh.render();

            /*foreach (TgcMesh mesh in this.scene.Meshes)
            {
                //Nos ocupamos solo de las mallas habilitadas
                if (mesh.Enabled)
                {
                    //Solo mostrar la malla si colisiona contra el Frustum
                    TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(this.frustum, mesh.BoundingBox);
                    if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        mesh.render();
                }
            }*/

        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public void close()
        {
            this.scene.disposeAll();
        }

    }
}
