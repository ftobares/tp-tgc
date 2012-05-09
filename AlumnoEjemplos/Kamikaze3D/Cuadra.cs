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

        #region Variables Globales
        TgcScene scene;
        TgcFrustum frustum;
        int sceneSize = 2000;
        #endregion

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

        }

        /// <summary>
        /// Método que posiciona las cuadras una al lado de la otra
        /// </summary>
        /// <param name="index">Número de cuadra</param>
        public void position(int index)
        {
            //Primer cuadra, no mover
            if(index == 0)
                return ;

            foreach(TgcMesh mesh in this.scene.Meshes)
                mesh.move(this.sceneSize * index+1, 0, 0);
        }

        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public void render(float elapsedTime)
        {

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
