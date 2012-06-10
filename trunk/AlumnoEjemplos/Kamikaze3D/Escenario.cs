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

namespace AlumnoEjemplos.Kamikaze3D
{
    /// <summary>
    /// Kamikaze3D
    /// </summary>
    public class Escenario
    {

        private int cuadraTamanno = 2000;
        private int cuadrasCantidad = 4;
        private Cuadra[] cuadras;
        private Vector3 lightPosition = new Vector3(0, 0, 0);
        private Vector3 camaraPosition = new Vector3(0, 0, 0);
        
        public Escenario()
        {
            this.cuadras = new Cuadra[this.cuadrasCantidad];
        }

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// </summary>
        public void init()
        {
            GuiController.Instance.BackgroundColor = Color.Black;

            /*GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-140f, 40f, -50f), new Vector3(-140f, 40f, -120f));
            GuiController.Instance.FpsCamera.MovementSpeed = 200f;
            GuiController.Instance.FpsCamera.JumpSpeed = 200f;*/

            //Agregar Valores para la Niebla
            GuiController.Instance.Modifiers.addBoolean("Enabled", "Enabled", true);
            GuiController.Instance.Modifiers.addFloat("startDistance", 1, 2000, 700);
            GuiController.Instance.Modifiers.addFloat("endDistance", 1, 4000, 1200);
            GuiController.Instance.Modifiers.addFloat("density", 0, 10, 1);
            GuiController.Instance.Modifiers.addColor("color", Color.Gray);

            this.crearCuadras();

        }

        /// <summary>
        /// Método que crea y posiciona todas las cuadras del mapa
        /// </summary>
        public void crearCuadras()
        {

            //Inicializar cuadras
            for (int i = 0; i < this.cuadras.Length; i++)
            {
                this.cuadras[i] = new Cuadra();
                this.cuadras[i].init();
            }

            double max = Math.Sqrt(this.cuadrasCantidad);
            int cuadra = 0;

            //posicionar cuadras en forma de cuadrilátero
            for(int i = 0; i < max; i++)
                for (int j = 0; j < max; j++)
                {
                    if (cuadra < this.cuadrasCantidad)
                    {
                        this.cuadras[cuadra].position(i * this.cuadraTamanno, 0, j * this.cuadraTamanno);
                    }

                    //this.cuadras[cuadra].updateShader(this.lightPosition, this.camaraPosition);
                    cuadra++;
                }


        }


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public void render(float elapsedTime, Camara camara, Explosion explosion)
        {
            for (int i = 0; i < this.cuadras.Length; i++)
            {
                this.cuadras[i].render(elapsedTime, camara);
            }

            //Cargar los valores de la Niebla
            if (explosion.estaEjecutandose())
                GuiController.Instance.Fog.Enabled = false;
            else
                GuiController.Instance.Fog.Enabled = (bool)GuiController.Instance.Modifiers["Enabled"];
            GuiController.Instance.Fog.StartDistance = (float)GuiController.Instance.Modifiers["startDistance"];
            GuiController.Instance.Fog.EndDistance = (float)GuiController.Instance.Modifiers["endDistance"];
            GuiController.Instance.Fog.Density = (float)GuiController.Instance.Modifiers["density"];
            GuiController.Instance.Fog.Color = (Color)GuiController.Instance.Modifiers["color"];

            //Actualizar valores de la Niebla
            GuiController.Instance.Fog.updateValues();
        }

        /// <summary>
        /// Devuelve un listado de BoundingBox de los objetos del escenario
        /// </summary>
        public List<TgcBoundingBox> getObjetosColisionables()
        {
            List<TgcBoundingBox> lista = new List<TgcBoundingBox> { };

            for (int i = 0; i < this.cuadras.Length; i++)
                foreach (TgcMesh mesh in this.cuadras[i].getMeshes())
                {
                    if (
                            //String.Compare(mesh.Name, 0, "Estructura", 0, 9) == 0 ||
                            //String.Compare(mesh.Name, 0, "Semaforo", 0, 8) == 0 ||
                            //String.Compare(mesh.Name, 0, "fuente", 0, 6) == 0
                            String.Compare(mesh.Name, 0, "Box", 0, 3) == 0
                        )
                    lista.Add(mesh.BoundingBox);
                   
                }

            return lista;

        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public void close()
        {
            for (int i = 0; i < this.cuadras.Length; i++)
                this.cuadras[i].close();
        }

        public TgcBoundingBox getBoundingBox()
        {
            Vector3 pMin = cuadras[0].getScene().BoundingBox.PMin;
            Vector3 pMax = cuadras[0].getScene().BoundingBox.PMax;

            for (int i = 0; i < this.cuadras.Length; i++)
            {
                foreach (TgcMesh mesh in cuadras[i].getMeshes())
                {
                    if (mesh.BoundingBox.PMin.X < pMin.X &&
                        mesh.BoundingBox.PMin.Y < pMin.Y &&
                        mesh.BoundingBox.PMin.Z < pMin.Z)
                        pMin = mesh.BoundingBox.PMin;

                    if (mesh.BoundingBox.PMax.X > pMax.X &&
                        mesh.BoundingBox.PMax.Y > pMax.Y &&
                        mesh.BoundingBox.PMax.Z > pMax.Z)
                        pMax = mesh.BoundingBox.PMax;
                }
            }
            return new TgcBoundingBox(pMin, pMax);
        }

    }
}
