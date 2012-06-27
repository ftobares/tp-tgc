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
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils._2D;
using System.Windows.Forms;

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
        private TgcThirdPersonCamera camara;
        private Personaje personaje;
        TgcSprite sangre;
        TgcSprite texto;

        private bool detonada = false;

        public void init(TgcThirdPersonCamera camara, Personaje personaje)
        {
            this.camara = camara;
            this.personaje = personaje;
            
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();
            this.scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Explosion\\Explosion-TgcScene.xml");

            foreach (TgcMesh mesh in this.scene.Meshes)
                mesh.Scale = new Vector3(0, 0, 0);

            //Crear Sprite de sangre y texto
            sangre = new TgcSprite();
            sangre.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Kamikaze3D\\sangre.png");
            texto = new TgcSprite();
            texto.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Kamikaze3D\\objetivoCumplido.png");
            Control focusWindows = GuiController.Instance.D3dDevice.CreationParameters.FocusWindow;
            sangre.Position = new Vector2(focusWindows.Width * 0.05f, focusWindows.Height * 0.5f);
            texto.Position = new Vector2(focusWindows.Width * 0.05f, focusWindows.Height * 0.5f);

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
                {
                    GuiController.Instance.Mp3Player.stop();
                    this.loadMP3();
                } 

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
               /* else
                {
                    TgcStaticSound tiro = new TgcStaticSound();
                    tiro.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\AK47\\gunshot.wav");
                    tiro.play();
                    sangre.render();

                    System.Threading.Thread.Sleep(1000);

                    tiro.play();
                    texto.render();
                }*/

                foreach (TgcMesh m in this.scene.Meshes)
                    m.render();


            }

        }

        public void loadMP3()
        {
            //GuiController.Instance.Mp3Player.closeFile();
            //GuiController.Instance.Mp3Player.FileName = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Explosion\\fx.mp3";
            //GuiController.Instance.Mp3Player.play(false);

            TgcStaticSound sonidoBomba = new TgcStaticSound();
            sonidoBomba.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Explosion\\fx.wav");
            sonidoBomba.play();

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
