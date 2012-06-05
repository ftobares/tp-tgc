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

        private Effect effect;

        private bool detonada = false;

        public void init(Camara camara)
        {
            this.camara = camara;
            
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();
            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();
            this.scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Explosion\\Explosion-TgcScene.xml");

            //Cargar shader
            Device d3dDevice = GuiController.Instance.D3dDevice;
            string compilationErrors;
            this.effect = Effect.FromFile(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Shaders\\Explosion.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (this.effect == null)
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
           
            this.effect.Technique = "DefaultTechnique";

            foreach (MyMesh mesh in this.scene.Meshes)
            {
                mesh.Scale = new Vector3(0, 0, 0);
                mesh.effect = this.effect;
            }
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

                Vector3 position = this.camara.getPosition();

                if (scale <= this.maxScale)
                {

                    position = position - new Vector3(0, position.Y, -100);

                    foreach (TgcMesh mesh in this.scene.Meshes)
                    {
                        mesh.Scale = new Vector3(scale, scale, scale);
                        mesh.Position = position;
                    }

                }

                //Configurar shader
                this.effect.Technique = "DefaultTechnique";
                this.effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(position));
                this.effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(position));
                this.effect.SetValue("k_la", 0.5f);
                this.effect.SetValue("k_ld", 0.6f);
                this.effect.SetValue("k_ls", 0.5f);
                this.effect.SetValue("fSpecularPower", 16f);

                Device device = GuiController.Instance.D3dDevice;
                //device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (MyMesh m in this.scene.Meshes)
                {
                    m.effect = effect;
                    m.render();
                }

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
            this.effect.Dispose();
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
