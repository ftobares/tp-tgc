using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

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

        private bool detonada = false;

        public void init(Camara camara)
        {
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();
            this.scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Explosion\\Explosion-TgcScene.xml");

            this.camara = camara;

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

                if (scale <= this.maxScale)
                {

                    Vector3 position = this.camara.getPosition();
                    position = position - new Vector3(0, position.Y, -100);

                    foreach (TgcMesh mesh in this.scene.Meshes)
                    {
                        mesh.Scale = new Vector3(scale, scale, scale);
                        mesh.Position = position;
                    }

                }
                this.scene.renderAll();

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
