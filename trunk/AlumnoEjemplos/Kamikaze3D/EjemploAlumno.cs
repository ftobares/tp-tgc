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
using TgcViewer.Utils.Input;

namespace AlumnoEjemplos.Kamikaze3D
{
    /// <summary>
    /// Kamikaze3D
    /// </summary>
    public class EjemploAlumno : TgcExample
    {

        #region Configuraci�n
        /// <summary>
        /// Categor�a a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el �rbol de la derecha de la pantalla.
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
        /// Completar con la descripci�n del TP
        /// </summary>
        public override string getDescription()
        {
            return "MiIdea - Descripcion de la idea";
        }

        #endregion

        #region Variables Globales
        Escenario escenario;
        Personaje personaje;
        Camara camara;
        Vector3 llegada;
        Explosion explosion;
        #endregion

        public EjemploAlumno()
        {
            this.escenario = new Escenario();
            this.camara = new Camara();
            this.llegada = new Vector3();
            this.explosion = new Explosion();
            this.personaje = new Personaje(this.camara, this.explosion);
        }

        /// <summary>
        /// M�todo que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, modifiers, uservars, etc.
        /// </summary>
        public override void init()
        {
            this.camara.Enable = true;
            this.camara.setCamera(this.personaje.getPersonaje().Position, 50, -100);

            this.escenario.init();
            this.personaje.init();
            this.personaje.setObjetosColisionables(this.escenario.getObjetosColisionables());
            this.explosion.init(this.camara);
        }

        /// <summary>
        /// M�todo que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aqu� todo el c�digo referido al renderizado.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el �ltimo frame</param>
        public override void render(float elapsedTime)
        {
            this.camara.Target = this.personaje.getPersonaje().Position;
            this.escenario.render(elapsedTime, this.camara, this.explosion);
            this.personaje.render(elapsedTime);
            this.explosion.render(elapsedTime);
        }

        /// <summary>
        /// M�todo que se llama cuando termina la ejecuci�n del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            this.escenario.close();
            this.personaje.close();
            this.explosion.close();
        }

    }
}
