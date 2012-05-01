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
        VoidMap Map;
        Vector3 LightPosition;
        #endregion

        public EjemploAlumno()
        {
            /*Map = new VoidMap();
            LightPosition = new Vector3(-4000, 8000, -1000);*/
        }

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// </summary>
        public override void init()
        {
            /*Map.iniciar();
            Map.setCamaraPosition = GuiController.Instance.ThirdPersonCamera.Position;
            Map.setLightPosition = LightPosition;
            Map.moveDefaultPosition();*/
        }


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {
            /*TgcFrustum frustum = GuiController.Instance.Frustum;

            //Crea una lista de los bloques a renderizar y se la mando al mapa para que se encarge de actualizarle valores internos y luego renderizar
            List<VoidBloque> CuadrasARenderizar = new List<VoidBloque>();

            foreach (VoidBloque Cuadra in Map.blocks)
            {
                //Agrega las cuadras que colisionan contra el frustum
                TgcCollisionUtils.FrustumResult Result = TgcCollisionUtils.classifyFrustumAABB(frustum, Cuadra.structure[0].BoundingBox);
                if (Result != TgcCollisionUtils.FrustumResult.OUTSIDE)
                {
                    CuadrasARenderizar.Add(Cuadra);
                }
            }

            //Mando la lista
            Map.renderList(ref CuadrasARenderizar);*/

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            //Carga el archivo del bloque
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Block\\Block-TgcScene.xml");
            scene.renderAll();


        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {

        }

    }
}
