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

    public class PatrullaPhong
    {
        string MyMediaDir;
        string MyShaderDir;
        string MyObjectsDir;
        TgcScene scene,scene2;
        MyMesh mesh;
        MyMesh patrulla;
        Effect effect;
        TgcBox lightBox;
        Viewport ViewF;
        bool vista_unica = true;
        //float cont;
        int r, t;

        /*#region Configuracion
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
        #endregion*/

        public /*override*/ void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            //MyMediaDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\EscenarioPrueba\\";
            MyObjectsDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Patrulla\\";
            MyShaderDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Shaders\\";
            
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();            

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            // Cargo la escena del cornell box.            
            //scene = loader.loadSceneFromFile(MyMediaDir + "escenarioPrueba-TgcScene.xml");

            //Carga la patrulla
            scene2 = loader.loadSceneFromFile(MyObjectsDir + "autoPolicia-TgcScene.xml");
            patrulla = (MyMesh)scene2.Meshes[0];

            patrulla.Scale = new Vector3(1f, 1f, 1f);
            patrulla.Position = new Vector3(0f, 1f, 0f);

            //mesh = (MyMesh)scene.Meshes[0];

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir  + "PhongShading.fx", null, null, ShaderFlags.None, null, out compilationErrors);            
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }             
            // configurar la tecnica 
            effect.Technique = "DefaultTechnique";
            // le asigno el efecto a las mallas 
            //mesh.effect = effect;
            patrulla.effect = effect;

            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-100f, -100f, -100f), new Vector3(100f, 100f, 100f), new Vector3(0f, 40f, -10f));
            GuiController.Instance.Modifiers.addFloat("Ambient", 0, 1, 0.5f);
            GuiController.Instance.Modifiers.addFloat("Diffuse", 0, 1, 0.6f);
            GuiController.Instance.Modifiers.addFloat("Specular", 0, 1, 0.5f);            
            //GuiController.Instance.Modifiers.addColor("AmbientColor", Color.Gray);
            //GuiController.Instance.Modifiers.addColor("DiffuseColor", Color.Gray);
            //GuiController.Instance.Modifiers.addColor("SpecularColor", Color.Blue);
            GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);

            t = 0;

            //Crear caja para indicar ubicacion de la luz
            lightBox = TgcBox.fromSize(new Vector3(5, 5, 5), Color.Yellow);
            
            // Creo el viewport, para la iluminacion dinamica            
            GuiController.Instance.RotCamera.setCamera(new Vector3(-10f, 40f, -10f), 300);
            ViewF = d3dDevice.Viewport;

            // Creo la luz para el fixed pipeline
            d3dDevice.Lights[0].Type = LightType.Point;
            d3dDevice.Lights[0].Diffuse = Color.FromArgb(255, 255, 255, 255);
            d3dDevice.Lights[0].Specular = Color.FromArgb(255, 255, 255, 255);
            d3dDevice.Lights[0].Attenuation0 = 0.0f;
            d3dDevice.Lights[0].Range = 50000.0f;
            d3dDevice.Lights[0].Enabled = true;
            //d3dDevice.Lights[0].AmbientColor = ColorValue.FromColor((Color)GuiController.Instance.Modifiers["AmbientColor"]);
            //d3dDevice.Lights[0].DiffuseColor = ColorValue.FromColor((Color)GuiController.Instance.Modifiers["DiffuseColor"]);
            //d3dDevice.Lights[0].SpecularColor = ColorValue.FromColor((Color)GuiController.Instance.Modifiers["SpecularColor"]);
        }
        
        public /*override*/ void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space ))
                vista_unica = !vista_unica;

            Vector3 lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];
            
            //Modifico parametros para que la luz gire en circulos
            /*cont = cont + elapsedTime * Geometry.DegreeToRadian(120.0f);
            lightPosition.X = lightPosition.X + (10f * (float)Math.Cos(cont));
            lightPosition.Y = lightPosition.Y + (10f * (float)Math.Sin(cont));
            lightPosition.Z = lightPosition.Z + (50f * (float)Math.Sin(cont));*/

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
            
            //Cambio los valores del Specular Power de la luz, para simular el parpadeo de la sirena
            r = t % 10;
            if (r == 0)
            {
                effect.SetValue("fSpecularPower", (float)20);
            }
            else {
                effect.SetValue("fSpecularPower", (float)90);
            }
            if (t == 2147483647)
            {
                t = 0;
            }
            else 
            {
                t = t + 1;
            }
             
            //Mover mesh que representa la luz
            lightBox.Position = lightPosition;
                       
            // solo una vista
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.Viewport = ViewF;
            /*foreach (MyMesh m in scene.Meshes)
            {
                if (m.Name != "escenarioPrueba")
                {
                        m.effect = effect;
                }
            m.render();*/
            patrulla.render();
            lightBox.render();            
            //}                 
            
        }

        public /*override*/ void close()
        {
            effect.Dispose();
            //scene.disposeAll();
            scene2.disposeAll();
            lightBox.dispose();
        }
    }

}
