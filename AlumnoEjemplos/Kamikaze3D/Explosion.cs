using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Kamikaze3D
{
    public class Explosion
    {

        private TgcScene scene;
        private float camTime = 0;
        private float expTime = 0;
        private float speed = 0.01F;
        private float acceleration = 10;
        private int maxScale = 28;

        private int cameraDistance = 2000;
        private float cameraSpeed = 0.1F;
        private float cameraAcceleration = 0.5F;
        private Camara camara;
        private Personaje personaje;

        private bool detonada = false;

        public void init(Camara camara, Personaje personaje)
        {
            this.camara = camara;
            this.personaje = personaje;
            
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();
            this.scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Explosion\\Explosion-TgcScene.xml");

            foreach (TgcMesh mesh in this.scene.Meshes)
                mesh.Scale = new Vector3(0, 0, 0);
        }

        public void render(float elapsedTime)
        {

            if (!detonada)
                return;

            //Alejar cámara primero
            if (this.camara.OffsetHeight <= this.cameraDistance)
            {
                this.camTime += elapsedTime;
                this.camara.OffsetHeight += (float)(this.camTime * this.cameraSpeed + (Math.Pow(this.camTime, 2) * this.cameraAcceleration));
            }
            else
            {

                if (this.expTime == 0)
                    this.loadMP3();

                this.expTime += elapsedTime;

                //Generar explosión escalando scene
                float scale = (float)(0.1 * (this.expTime * this.speed + Math.Pow(this.expTime, 2) * this.acceleration));

                //Vector3 position = this.camara.getPosition();
                Vector3 position = new Vector3();
                this.personaje.getPersonaje().getPosition(position);

                if (scale <= this.maxScale)
                {

                    position = position - new Vector3(-1770, position.Y, -1720);

                    foreach (TgcMesh mesh in this.scene.Meshes)
                    {
                        mesh.Scale = new Vector3(scale, scale, scale);
                        mesh.Position = position;
                    }

                }

                foreach (TgcMesh m in this.scene.Meshes)
                    m.render();

            }

        }

        public void loadMP3()
        {
            GuiController.Instance.Mp3Player.FileName = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Explosion\\fx.mp3";
            GuiController.Instance.Mp3Player.play(false);
        }

        public void close()
        {
            this.scene.disposeAll();
        }

        public bool estaEjecutandose()
        {
            return this.detonada;
        }

        public void detonar()
        {
            this.detonada = true;
        }

    }
}
