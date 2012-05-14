using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.Input;
using Microsoft.DirectX;
using TgcViewer;

namespace AlumnoEjemplos.Kamikaze3D
{
    /// <summary>
    /// Camara en tercera persona que sigue a un objeto a un determinada distancia.
    /// </summary>
    public class Camara : TgcCamera
    {

        static readonly Vector3 UP_VECTOR = new Vector3(0, 1, 0);
        float xMin = -0.1f;
        float xMax = 0.45f;

        float offsetHeight;
        /// <summary>
        /// Desplazamiento en altura de la camara respecto del target
        /// </summary>
        public float OffsetHeight
        {
            get { return offsetHeight; }
            set { offsetHeight = value; }
        }

        float offsetForward;
        /// <summary>
        /// Desplazamiento hacia adelante o atras de la camara repecto del target.
        /// Para que sea hacia atras tiene que ser negativo.
        /// </summary>
        public float OffsetForward
        {
            get { return offsetForward; }
            set { offsetForward = value; }
        }

        Vector3 targetDisplacement;
        /// <summary>
        /// Desplazamiento final que se le hace al target para acomodar la camara en un cierto
        /// rincon de la pantalla
        /// </summary>
        public Vector3 TargetDisplacement
        {
            get { return targetDisplacement; }
            set { targetDisplacement = value; }
        }

        float rotationY;
        /// <summary>
        /// Rotacion absoluta en Y de la camara
        /// </summary>
        public float RotationY
        {
            get { return rotationY; }
            set { rotationY = value; }
        }

       float rotationX;
        /// <summary>
        /// Rotacion absoluta en X de la camara
        /// </summary>
        public float RotationX
        {
            get { return rotationX; }
            set { rotationX = value; }
        }

        Vector3 target;
        /// <summary>
        /// Objetivo al cual la camara tiene que apuntar
        /// </summary>
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        Vector3 position;
        /// <summary>
        /// Posicion del ojo de la camara
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        bool enable;
        /// <summary>
        /// Habilita o no el uso de la camara
        /// </summary>
        public bool Enable
        {
            get { return enable; }
            set
            {
                enable = value;

                //Si se habilito la camara, cargar como la cámara actual
                if (value)
                {
                    GuiController.Instance.CurrentCamera = this;
                }
            }
        }

        Matrix viewMatrix;


        /// <summary>
        /// Crear una nueva camara
        /// </summary>
        public Camara()
        {
            resetValues();
        }

        /// <summary>
        /// Carga los valores default de la camara y limpia todos los cálculos intermedios
        /// </summary>
        public void resetValues()
        {
            offsetHeight = 20;
            offsetForward = -120;
            rotationY = 0;
            rotationX = 0;
            targetDisplacement = Vector3.Empty;
            target = Vector3.Empty;
            position = Vector3.Empty;
            viewMatrix = Matrix.Identity;
        }

        /// <summary>
        /// Configura los valores iniciales de la cámara
        /// </summary>
        /// <param name="target">Objetivo al cual la camara tiene que apuntar</param>
        /// <param name="offsetHeight">Desplazamiento en altura de la camara respecto del target</param>
        /// <param name="offsetForward">Desplazamiento hacia adelante o atras de la camara repecto del target.</param>
        public void setCamera(Vector3 target, float offsetHeight, float offsetForward)
        {
            this.target = target;
            this.offsetHeight = offsetHeight;
            this.offsetForward = offsetForward;
        }

        /// <summary>
        /// Genera la proxima matriz de view, sin actualizar aun los valores internos
        /// </summary>
        /// <param name="pos">Futura posicion de camara generada</param>
        /// <param name="pos">Futuro centro de camara a generada</param>
        /// <returns>Futura matriz de view generada</returns>
        public Matrix generateViewMatrix(out Vector3 pos, out Vector3 targetCenter)
        {
            //alejarse, luego rotar y lueg ubicar camara en el centro deseado
            targetCenter = Vector3.Add(target, targetDisplacement);
            Matrix m = Matrix.Translation(0, offsetHeight, offsetForward) * Matrix.RotationY(rotationY) * Matrix.RotationX(rotationX) * Matrix.Translation(targetCenter);

            //Extraer la posicion final de la matriz de transformacion
            pos.X = m.M41;
            pos.Y = m.M42;
            pos.Z = m.M43;

            //Obtener ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
            return Matrix.LookAtLH(pos, targetCenter, UP_VECTOR);
        }

        public void updateCamera()
        {
            Vector3 targetCenter;
            viewMatrix = generateViewMatrix(out position, out targetCenter);
        }

        /// <summary>
        /// Rotar la camara respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateY(float angle)
        {
            rotationY += angle;
        }

        /// <summary>
        /// Rotar la camara respecto del eje X
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateX(float angle)
        {
            float temp = rotationX + angle;
            if (temp > this.xMin && temp < this.xMax)
                rotationX = temp;
        }

        public Vector3 getPosition()
        {
            return position;
        }

        public Vector3 getLookAt()
        {
            return target;
        }


        public void updateViewMatrix(Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            if (!enable)
            {
                return;
            }

            d3dDevice.Transform.View = viewMatrix;
        }

    }
}
