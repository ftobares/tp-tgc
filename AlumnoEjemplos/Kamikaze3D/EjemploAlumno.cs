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
    public class EjemploAlumno : TgcExample
    {

        #region Configuración
        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Kamikaze3D";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "MiIdea - Descripcion de la idea";
        }

        #endregion

        #region Variables Globales
        //VoidMap map;
        //Vector3 lightPosition;
        //List<VoidBloque> cuadras;
        TgcScene scene;
        TgcFrustum frustum;
        #endregion

        public EjemploAlumno()
        {
            //this.map = new VoidMap();
            this.frustum = GuiController.Instance.Frustum;
            //LightPosition = new Vector3(-4000, 8000, -1000);
        }

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// </summary>
        public override void init()
        {
            /*
            this.map.iniciar();
            this.map.setCamaraPosition = GuiController.Instance.ThirdPersonCamera.Position;
            this.map.setLightPosition = this.lightPosition;
            this.map.moveDefaultPosition();
             
            //Crea una lista de los bloques a renderizar y se la mando al mapa para que se encarge de actualizarle valores internos y luego renderizar
            this.cuadras = new List<VoidBloque>();

            foreach (VoidBloque Cuadra in this.map.blocks)
            {
                //Agrega las cuadras que colisionan contra el frustum
                TgcCollisionUtils.FrustumResult Result = TgcCollisionUtils.classifyFrustumAABB(this.frustum, Cuadra.structure[0].BoundingBox);
                if (Result != TgcCollisionUtils.FrustumResult.OUTSIDE)
                {
                    this.cuadras.Add(Cuadra);
                    GuiController.Instance.UserVars.setValue("Meshes renderizadas", this.cuadras.Count);
                }
            }
            */

            GuiController.Instance.UserVars.addVar("Meshes renderizadas");
            GuiController.Instance.UserVars.addVar("Meshes no renderizadas");

            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-140f, 40f, -50f), new Vector3(-140f, 40f, -120f));
            GuiController.Instance.FpsCamera.MovementSpeed = 200f;
            GuiController.Instance.FpsCamera.JumpSpeed = 200f;
            
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            //Carga el archivo del bloque
            this.scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Block\\Block-TgcScene.xml");
        
        }


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {

            //this.map.renderList(ref this.cuadras);

            int render = 0;
            int noRender = 0;
            foreach (TgcMesh mesh in this.scene.Meshes)
            {
                //Nos ocupamos solo de las mallas habilitadas
                if (mesh.Enabled)
                {
                    //Solo mostrar la malla si colisiona contra el Frustum
                    TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(this.frustum, mesh.BoundingBox);
                    if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                    {
                        mesh.render();
                        render++;
                    }
                    else
                    {
                        noRender++;
                    }
                }
            }
            GuiController.Instance.UserVars.setValue("Meshes renderizadas", render);
            GuiController.Instance.UserVars.setValue("Meshes no renderizadas", noRender);



        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            this.scene.disposeAll();
        }

    }
}
