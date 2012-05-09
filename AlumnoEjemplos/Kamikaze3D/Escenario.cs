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

            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-140f, 40f, -50f), new Vector3(-140f, 40f, -120f));
            GuiController.Instance.FpsCamera.MovementSpeed = 200f;
            GuiController.Instance.FpsCamera.JumpSpeed = 200f;

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

                    this.cuadras[cuadra].updateShader(this.lightPosition, this.camaraPosition);
                    cuadra++;
                }


        }


        /// <summary>
        /// Método que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aquí todo el código referido al renderizado.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public void render(float elapsedTime)
        {
            for (int i = 0; i < this.cuadras.Length; i++)
            {
                this.cuadras[i].updateShader(this.lightPosition, this.camaraPosition);
                this.cuadras[i].render(elapsedTime);
            }
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

    }
}
