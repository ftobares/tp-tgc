using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.TgcKeyFrameLoader;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Kamikaze3D
{
    public class Proyectil
    {
        TgcMesh bala;
        private int alcance = 50;
        private bool impacto = false;
        private Personaje personaje;
        private static float VELOCIDAD_BALA = 6f;

        public Proyectil(Personaje persona)
        {
            personaje = persona;
        }

        public TgcMesh getMesh()
        {
            return bala;
        }

        public void dispose()
        {
            bala.dispose();
        }

        public TgcBoundingBox getBoundingBox()
        {
            return bala.BoundingBox;
        }

        protected void cargarMesh()
        {
            //Cargar modelo estatico Box
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml",
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\");
            bala = scene.Meshes[0];
        }

        private void dispararProyectil()
        {

            Vector3 v = new Vector3(0f, 20f, 0f);
            
            bala.move(personaje.getPosicionBala() + v);

            vistaDelPersonaje(personaje.getPersonaje().Position);
        }

        public void inicializar()
        {
            cargarMesh();
            dispararProyectil();
        }
        public void inicializar(Vector3 v3PosicionInicial)
        {
            cargarMesh();
            bala.Position=v3PosicionInicial;
            dispararProyectil();
        }

        public Vector3 getVectorHaciaPersonaje(Vector3 posicionPersonaje)
        {
            return (bala.Position - posicionPersonaje);
        }

        public void vistaDelPersonaje(Vector3 posicionPersonaje)
        {
            Vector3 vec = getVectorHaciaPersonaje(posicionPersonaje);
            double anguloFinal = Math.Atan2(vec.X, vec.Z);

            bala.rotateY(-bala.Rotation.Y);
            bala.rotateY((float)anguloFinal);
            bala.rotateY((float)Math.PI);
            bala.rotateY((float)Geometry.DegreeToRadian(-10));
        }

        public void mePegaste()
        {
            impacto = true;
        }

        protected bool mover(float t)
        {
            //bala.moveOrientedY((float)GuiController.Instance.Modifiers.getValue("VelocidadBala"));
            bala.moveOrientedY(VELOCIDAD_BALA);
            return true;
        }

        public bool renderizar()
        {
            if (alcance > 0 && !impacto)
            {
                Vector3 v = new Vector3(0.05f, 0.05f, 0.05f);
                bala.Scale = v;
                mover(0f);
                //bala.render();
                this.alcance--;
                return true;
            }
            else
                return false;
        }
    }
}
