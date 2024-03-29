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
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils._2D;

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
            return "WASD - Movimiento\r\nMouse - Camara\r\nClick derecho - Apuntar\r\nL - mostrar/ocultar cursor";
        }

        #endregion

        #region Variables Globales
        Escenario escenario;
        Personaje personaje;
        TgcThirdPersonCamera camara;
        Vector3 llegada;
        Explosion explosion;
        #endregion

        private Police police;
        private TgcText2d distanceTargetText;
        private TgcText2d onTargetPosition;
        private TgcText2d lifeText;
        private Quadtree quadtree;
        private const int MIN_DISTANCE_TO_EXPLODE = 100;
        private bool showingCursor = true;
        private PatrullaPhong patrulla;
        private Fin fin;

        public EjemploAlumno()
        {
            this.escenario = new Escenario();
            this.camara = new TgcThirdPersonCamera();
            this.llegada = new Vector3(1800,0,1700);
            this.explosion = new Explosion();
            this.personaje = new Personaje(this.camara, this.explosion);
            this.patrulla = new PatrullaPhong();
            this.fin = new Fin();

            distanceTargetText = new TgcText2d();            
            distanceTargetText.Position = new Point(0, 40);
            distanceTargetText.Color = Color.White;
            onTargetPosition = new TgcText2d();
            onTargetPosition.Position = new Point(0, 60);
            onTargetPosition.Color = Color.Red;
            onTargetPosition.Text = "Presione la tecla K";
            lifeText = new TgcText2d();
            lifeText.Position = new Point(-400, 400);
            try
            {
                lifeText.changeFont(new System.Drawing.Font(new FontFamily("Asrock7Segment"), 25));
            }
            catch  {
                lifeText.changeFont(new System.Drawing.Font(lifeText.D3dFont.Description.FaceName, 25));
            }
            lifeText.Color = Color.Yellow;
        }

        private void optimizar()
        {
            //Crear Quadtree
            quadtree = new Quadtree();
            quadtree.create(this.police.getInstances(), escenario.getBoundingBox());
            //quadtree.createDebugQuadtreeMeshes();
            return;
        }

        /// <summary>
        /// M�todo que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, modifiers, uservars, etc.
        /// </summary>
        public override void init()
        {
            this.camara.Enable = true;
            this.camara.setCamera(this.personaje.getPersonaje().Position, 25, -100);

            this.escenario.init();
            this.personaje.init();
            this.patrulla.init();
            this.personaje.setObjetosColisionables(this.escenario.getObjetosColisionables(),this.patrulla.getObjetosColisionables());            
            this.explosion.init(this.camara, this.personaje);          
            this.police = new Police(300, this.escenario.getObjetosColisionables(), escenario.getBoundingBox());
            List<Vector3> personajesColisionables = new List<Vector3>();
            foreach (TgcSkeletalMesh skm in this.police.getInstances())
            {
                personajesColisionables.Add(skm.Position);
            }
            this.personaje.setPersonajesColisionables(personajesColisionables);
            this.optimizar();
            GuiController.Instance.Mp3Player.closeFile();
            GuiController.Instance.Mp3Player.FileName = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\musica\\Mission Impossible.mp3";
            GuiController.Instance.Mp3Player.play(true);

            this.fin.init();
        }

        public int getDistanceToTarget()
        {
            float distance = FastMath.Pow2(this.personaje.getPersonaje().Position.X - llegada.X) +
                             FastMath.Pow2(this.personaje.getPersonaje().Position.Z - llegada.Z);
            distance = FastMath.Sqrt(distance);
            if (distance < MIN_DISTANCE_TO_EXPLODE)
                distanceTargetText.Color = Color.Red;
            else
                distanceTargetText.Color = Color.White;
            return Convert.ToInt32(distance);
        }

        /// <summary>
        /// M�todo que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aqu� todo el c�digo referido al renderizado.
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el �ltimo frame</param>
        public override void render(float elapsedTime)
        {
            if (!this.explosion.finalizada())
            {

                this.camara.Target = this.personaje.getPersonaje().Position;
                this.escenario.render(elapsedTime, this.camara);

                if (this.personaje.alive())
                {
                    this.personaje.render(elapsedTime);
                    lifeText.Text = "+" + Convert.ToString(this.personaje.getLife());
                    if (this.personaje.getLife() < 30)
                        lifeText.Color = Color.Red;
                    lifeText.render();
                }
                else
                    this.personaje.renderDeading();

                this.explosion.render(elapsedTime);
                this.patrulla.render(elapsedTime, this.personaje, this.llegada, this.explosion);
                this.quadtree.render(GuiController.Instance.Frustum, this.personaje, false);
                int distance = getDistanceToTarget();
                this.personaje.canExplode = false;
                if (distance < MIN_DISTANCE_TO_EXPLODE)
                {
                    onTargetPosition.render();
                    this.personaje.canExplode = true;
                }
                distanceTargetText.Text = "Distancia a objetivo: " + Convert.ToString(distance);
                distanceTargetText.render();

                if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.L))
                {
                    if (showingCursor)
                        System.Windows.Forms.Cursor.Hide();
                    else
                        System.Windows.Forms.Cursor.Show();
                    showingCursor = !showingCursor;
                }

            }
            else
            {
                this.fin.render();
            }
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
            this.patrulla.close();
            this.fin.close();
            System.Windows.Forms.Cursor.Show(); 
        }

    }
}
