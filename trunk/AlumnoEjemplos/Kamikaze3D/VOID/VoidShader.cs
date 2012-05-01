using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Render;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils;

namespace AlumnoEjemplos.VOID
{
    public class VoidMeshShader : TgcMesh, IRenderQueueElement
    {
        /// <summary>
        /// Desplaza la malla la distancia especificada, respecto al 0,0,0
        /// </summary>
        public void moveAbsolute(Vector3 v)
        {
            this.translation.X += v.X;
            this.translation.Y += v.Y;
            this.translation.Z += v.Z;

            updateBoundingBox();
        }

        Effect effect;
        /// <summary>
        /// Shader
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected new MeshRenderType renderType;
        /// <summary>
        /// Tipo de formato de Render de esta malla
        /// </summary>
        public new MeshRenderType RenderType
        {
            get { return renderType; }
            set { renderType = value; }
        }

        /// <summary>
        /// Tipos de de renderizado de mallas
        /// </summary>
        public new enum MeshRenderType
        {
            /// <summary>
            /// Solo un canal de textura en DiffuseMap
            /// </summary>
            DIFFUSE_MAP,
            /// <summary>
            /// Un canal de textura en DiffuseMap y otro para Lightmap,
            /// utilizando Multitexture
            /// </summary>
            DIFFUSE_MAP_BASIC,
            /// <summary>
            /// Solo colores por vertice
            /// </summary>
            VERTEX_COLOR,
        };

        /// <summary>
        /// Se llama antes de iniciar el shader.
        /// </summary>
        /// <param name="mesh">Mesh</param>
        public delegate void ShaderBeginHandler(VoidMeshShader mesh);

        /// <summary>
        /// Evento que se llama antes de iniciar el shader.
        /// </summary>
        public event ShaderBeginHandler ShaderBegin;

        /// <summary>
        /// Se llama antes de iniciar una pasada del shader.
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="pass">Número de pasada del shader</param>
        public delegate void ShaderPassBeginHandler(VoidMeshShader mesh, int pass);

        /// <summary>
        /// Evento que se llama antes de iniciar una pasada del shader.
        /// </summary>
        public event ShaderPassBeginHandler ShaderPassBegin;

        public VoidMeshShader(Mesh mesh, string name, MeshRenderType renderType)
            : base(mesh, name, (TgcViewer.Utils.TgcSceneLoader.TgcMesh.MeshRenderType)renderType)
        {
        }

        public VoidMeshShader(string name, TgcMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
            : base(name, parentInstance, translation, rotation, scale)
        {
        }

        Vector3 lastTranslation = new Vector3();
        float absoluteRotationY = 0f;
        /// <summary>
        /// Rota la malla respecto del eje Y con pivote en el (0,0,0)
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void absoluteRotateY(float angle)
        {
            absoluteRotationY += angle;

            //Cambio el signo del angulo para que gire en sentido horario
            angle = -angle;

            //Almacena la posision inicial
            double posX = Position.X;
            double posZ = Position.Z;

            //Calcula la nueva posicion
            posX = (posX * FastMath.Cos(angle)) - (posZ * FastMath.Sin(angle));
            posZ = (posZ * FastMath.Cos(angle)) + (posX * FastMath.Sin(angle));

            //Actualiza la posicion
            this.Position = new Vector3((float)posX, Position.Y, (float)posZ);

            //Rota el modelo en su eje
            this.rotateY(-angle);
        }

        /// <summary>
        /// Se redefine este método para agregar shaders.
        /// Es el mismo código del executeRender() pero con la sección de "MeshRenderType.DIFFUSE_MAP" ampliada
        /// para Shaders.
        /// </summary>
        public new void executeRender()
        {
            Device device = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Aplicar transformacion de malla
            if (autoTransformEnable)
            {
                this.transform = Matrix.Identity
                    * Matrix.Scaling(scale)
                    * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                    * Matrix.Translation(translation);
            }
            device.Transform.World = this.transform;

            //Cargar VertexDeclaration
            device.VertexDeclaration = vertexDeclaration;

            //Renderizar segun el tipo de render de la malla
            switch (renderType)
            {
                case MeshRenderType.DIFFUSE_MAP:

                    //Hacer reset de Lightmap
                    texturesManager.clear(1);

                    //Llamar evento para configurar inicio del shader
                    if (ShaderBegin != null)
                    {
                        ShaderBegin.Invoke(this);
                    }

                    //Iniciar Shader e iterar sobre sus Render Passes
                    int numPasses = effect.Begin(0);
                    for (int n = 0; n < numPasses; n++)
                    {
                        //Llamar evento para configurar inicio de la pasada del shader
                        if (ShaderPassBegin != null)
                        {
                            ShaderPassBegin.Invoke(this, n);
                        }

                        //Iniciar pasada de shader
                        effect.BeginPass(n);

                        //Habilito el fog en la primer pasada


                        //Dibujar cada subset con su Material y DiffuseMap correspondiente
                        for (int i = 0; i < materials.Length; i++)
                        {
                            device.Material = materials[i];
                            texturesManager.set(0, diffuseMaps[i]);
                            d3dMesh.DrawSubset(i);
                        }

                        //Finalizar pasada
                        effect.EndPass();
                    }

                    //Finalizar shader
                    effect.End();

                    break;

                case MeshRenderType.DIFFUSE_MAP_BASIC:

                    //Hacer reset de Lightmap
                    texturesManager.clear(1);

                    //Dibujar cada subset con su Material y DiffuseMap correspondiente
                    for (int i = 0; i < materials.Length; i++)
                    {
                        device.Material = materials[i];
                        texturesManager.set(0, diffuseMaps[i]);
                        d3dMesh.DrawSubset(i);
                    }
                    break;

                case MeshRenderType.VERTEX_COLOR:

                    //Hacer reset de texturas y materiales
                    texturesManager.clear(0);
                    texturesManager.clear(1);
                    device.Material = TgcD3dDevice.DEFAULT_MATERIAL;

                    //Dibujar mesh
                    d3dMesh.DrawSubset(0);
                    break;
            }
        }

    }

    /// <summary>
    /// Factory customizado para poder crear clase VoidMeshShader
    /// </summary>
    public class MyCustomMeshFactory : TgcSceneLoader.IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new VoidMeshShader(d3dMesh, meshName, (AlumnoEjemplos.VOID.VoidMeshShader.MeshRenderType)renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            return new VoidMeshShader(meshName, originalMesh, translation, rotation, scale);
        }
    }
}