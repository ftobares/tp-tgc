using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Input;
using AlumnoEjemplos.Kamikaze3D;

namespace AlumnoEjemplos.Kamikaze3D
{

    public class SirenaShadingPhong: TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcScene scene;
        MyMesh mesh;
        Effect effect;
        TgcBox lightBox;
        Viewport View1,View2,View3,ViewF;
        bool vista_unica = true;
        float cont;

        #region Configuracion
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "SirenaShadingPhong";
        }

        public override string getDescription()
        {
            return "SirenaShadingPhong";
        }
        #endregion

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MyMediaDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\EscenarioPrueba\\";
            MyShaderDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Shaders\\";
            
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            // Cargo la escena del cornell box.
            //scene = loader.loadSceneFromFile(MyMediaDir + "EscenarioPortal-TgcScene.xml");
            scene = loader.loadSceneFromFile(MyMediaDir + "escenarioPrueba-TgcScene.xml");

            mesh = (MyMesh)scene.Meshes[0];

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir  + "PhongShading.fx", null, null, ShaderFlags.None, null, out compilationErrors);            
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            // Pasos standard: 
            // configurar la tecnica 
            effect.Technique = "DefaultTechnique";
            // le asigno el efecto a la malla 
            mesh.effect = effect;

            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-100f, -100f, -100f), new Vector3(100f, 100f, 100f), new Vector3(0f, 40f, 0f));
            GuiController.Instance.Modifiers.addFloat("Ambient", 0, 1, 0.5f);
            GuiController.Instance.Modifiers.addFloat("Diffuse", 0, 1, 0.6f);
            GuiController.Instance.Modifiers.addFloat("Specular", 0, 1, 0.5f);            
            //GuiController.Instance.Modifiers.addColor("AmbientColor", Color.Gray);
            //GuiController.Instance.Modifiers.addColor("DiffuseColor", Color.Gray);
            //GuiController.Instance.Modifiers.addColor("SpecularColor", Color.Red);
            GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);

            cont = 0;

            //Crear caja para indicar ubicacion de la luz
            //lightBox = TgcBox.fromSize(new Vector3(5, 5, 5), Color.Yellow);


            // Creo 3 viewport, para mostrar una comparativa entre los metodos de iluminacion
            //GuiController.Instance.RotCamera.Enable = true;
            //GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            //GuiController.Instance.RotCamera.CameraDistance = 150;
            //GuiController.Instance.setCamera(new Vector3(-10f, 40f, -10f), new Vector3(-10f, 40f, -10f));
            GuiController.Instance.RotCamera.setCamera(new Vector3(-10f, 40f, -10f), 300);
            View1 = new Viewport();
            View1.X = 0;
            View1.Y = 0;
            View1.Width = 400;
            View1.Height = 250;
            View1.MinZ = 0;
            View1.MaxZ = 1;


            View2 = new Viewport();
            View2.X = 0;
            View2.Y = 250;
            View2.Width = 400;
            View2.Height = 250;
            View2.MinZ = 0;
            View2.MaxZ = 1;

            View3 = new Viewport();
            View3.X = 400;
            View3.Y = 0;
            View3.Width = 400;
            View3.Height = 250;
            View3.MinZ = 0;
            View3.MaxZ = 1;

            ViewF = d3dDevice.Viewport;

            // Creo la luz para el fixed pipeline
            d3dDevice.Lights[0].Type = LightType.Point;
            d3dDevice.Lights[0].Diffuse = Color.FromArgb(255, 255, 255, 255);
            d3dDevice.Lights[0].Specular = Color.FromArgb(255, 255, 255, 255);
            d3dDevice.Lights[0].Attenuation0 = 0.0f;
            d3dDevice.Lights[0].Range = 50000.0f;
            d3dDevice.Lights[0].Enabled = true;  
        }
        
        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space ))
                vista_unica = !vista_unica;

            Vector3 lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];
                       
            cont = cont + elapsedTime * Geometry.DegreeToRadian(120.0f);
            lightPosition.X = lightPosition.X + (10f * (float)Math.Cos(cont));
            lightPosition.Y = lightPosition.Y + (10f * (float)Math.Sin(cont));
            lightPosition.Z = lightPosition.Z + (50f * (float)Math.Sin(cont));
                        
            //avion.Position = new Vector3(80f * (float)Math.Cos(alfa), 40 - 20 * (float)Math.Sin(alfa), 80f * (float)Math.Sin(alfa));
            //dir_avion = new Vector3(-(float)Math.Sin(alfa), 0, (float)Math.Cos(alfa));
            //avion.Transform = CalcularMatriz(avion.Position, avion.Scale, dir_avion);

            effect.Technique = "DefaultTechnique";
            
            //Cargar variables de shader
            effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            effect.SetValue("k_la", (float)GuiController.Instance.Modifiers["Ambient"]);
            effect.SetValue("k_ld", (float)GuiController.Instance.Modifiers["Diffuse"]);
            effect.SetValue("k_ls", (float)GuiController.Instance.Modifiers["Specular"]);
            //effect.SetValue("fvAmbient", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["AmbientColor"]));
            //effect.SetValue("fvDiffuse", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["DiffuseColor"]));
            //effect.SetValue("fvSpecular", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["SpecularColor"]));
            effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);

            //Mover mesh que representa la luz
            //lightBox.Position = lightPosition;

            if (vista_unica)
            {
                // solo una vista
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                device.Viewport = ViewF;
                foreach (MyMesh m in scene.Meshes)
                {
                    if (m.Name != "escenarioPrueba")
                    {
                        m.effect = effect;                        
                    }
                    m.render();
                }                
                //lightBox.render();
            }                 
            
        }

        public override void close()
        {
            effect.Dispose();
            scene.disposeAll();
            //lightBox.dispose();
        }
    }

}
