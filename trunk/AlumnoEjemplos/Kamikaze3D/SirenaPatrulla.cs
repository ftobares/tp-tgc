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
    public class SirenaPatrulla: TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcScene scene;
        TgcArrow arrow;
        Effect effect;        

        // Shadow map
        readonly int SHADOWMAP_SIZE = 1024; 
        Texture g_pShadowMap;    // Texture to which the shadow map is rendered
        Surface g_pDSShadow;     // Depth-stencil buffer for rendering to shadow map
        Matrix g_mShadowProj;    // Projection matrix for shadow map
        Vector3 g_LightPos;						// posicion de la luz actual (la que estoy analizando)
        Vector3 g_LightDir;						// direccion de la luz actual
        Matrix g_LightView;						// matriz de view del light
        float near_plane = 2f; 
        float far_plane = 1500f;         
        float time;
        float cont;

        #region Configuracion
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Sirena Patrulla";
        }

        public override string getDescription()
        {
            return "Sirena Patrulla";
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

            // ------------------------------------------------------------
            //Cargar la escena
            scene = loader.loadSceneFromFile(MyMediaDir
                    + "escenarioPrueba-TgcScene.xml");            

            GuiController.Instance.RotCamera.CameraDistance = 600;
            
            //Cargar Shader
            string compilationErrors; //PhongShading.fx --> ShadowMap.fx 
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "ShadowMap.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //effect.Technique = "RenderScene";
            // le asigno el efecto a las mallas 
            foreach (MyMesh T in scene.Meshes)
            {
                T.Scale = new Vector3(2f, 2f, 2f);
                T.effect = effect;
            }
            
            //--------------------------------------------------------------------------------------
            // Format.R32F
            // Format.X8R8G8B8
            g_pShadowMap = new Texture(d3dDevice, SHADOWMAP_SIZE, SHADOWMAP_SIZE,
                                        1, Usage.RenderTarget, Format.R32F,
                                        Pool.Default);

            // tengo que crear un stencilbuffer para el shadowmap manualmente
            // para asegurarme que tenga la el mismo tamaño que el shadowmap, y que no tenga 
            // multisample, etc etc.
            g_pDSShadow = d3dDevice.CreateDepthStencilSurface(SHADOWMAP_SIZE,
                                                             SHADOWMAP_SIZE,
                                                             DepthFormat.D24S8,
                                                             MultiSampleType.None,
                                                             0,
                                                             true);
            // por ultimo necesito una matriz de proyeccion para el shadowmap, ya 
            // que voy a dibujar desde el pto de vista de la luz.
            // El angulo tiene que ser mayor a 45 para que la sombra no falle en los extremos del cono de luz
            // de hecho, un valor mayor a 90 todavia es mejor, porque hasta con 90 grados es muy dificil
            // lograr que los objetos del borde generen sombras
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;
            g_mShadowProj = Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(80),
                aspectRatio, 10, 500);
            d3dDevice.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                aspectRatio, near_plane, far_plane);

            arrow = new TgcArrow();
            arrow.Thickness = 1f;
            arrow.HeadSize = new Vector2(2f, 2f);
            arrow.BodyColor = Color.Blue;
            cont = 0;            

            GuiController.Instance.RotCamera.targetObject(scene.Meshes[0].BoundingBox);
            float K = 100;
            GuiController.Instance.Modifiers.addVertex3f("LightLookFrom", new Vector3(-K, -K, -K), new Vector3(K, K, K), new Vector3(0, 100, 0));
            GuiController.Instance.Modifiers.addVertex3f("LightLookAt", new Vector3(-K, -K, -K), new Vector3(K, K, K), new Vector3(0, 100, 0));
            //GuiController.Instance.Modifiers.addFloat("Ambient", 0, 1, 0.5f);
            //GuiController.Instance.Modifiers.addFloat("Diffuse", 0, 1, 0.6f);
            //GuiController.Instance.Modifiers.addFloat("Specular", 0, 1, 0.5f);
            //GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);
        }


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;
            time += elapsedTime;
            
            g_LightPos = (Vector3)GuiController.Instance.Modifiers["LightLookFrom"];

            cont = cont + elapsedTime * Geometry.DegreeToRadian(100.0f);            
            g_LightPos.X = g_LightPos.X + (5f * (float)Math.Cos(cont));
            g_LightPos.Y = g_LightPos.Y + (1f * (float)Math.Sin(cont));
            g_LightPos.Z = g_LightPos.Z + (10f * (float)Math.Sin(cont));

            g_LightDir = (Vector3)GuiController.Instance.Modifiers["LightLookAt"] - g_LightPos;
            g_LightDir.Normalize();

            arrow.PStart = g_LightPos;
            arrow.PEnd = g_LightPos + g_LightDir * 20;

            // Shadow maps:
            device.EndScene();      // termino el thread anterior

            GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 100;            
            GuiController.Instance.CurrentCamera.updateCamera();
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Genero el shadow map
            RenderShadowMap();

            device.BeginScene();
            // dibujo la escena pp dicha
            effect.Technique = "RenderScene";
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            RenderScene(false);

            //Cargar valores de la flecha
            arrow.render();


        }

        public void RenderShadowMap()
        {
            Device device = GuiController.Instance.D3dDevice;
            // Seteo colores e intencidad            
            //effect.SetValue("k_la", (float)GuiController.Instance.Modifiers["Ambient"]);
            //effect.SetValue("k_ld", (float)GuiController.Instance.Modifiers["Diffuse"]);
            //effect.SetValue("k_ls", (float)GuiController.Instance.Modifiers["Specular"]);
            //effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);
            //effect.SetValue("PixScene",
            // Calculo la matriz de view de la luz
            effect.SetValue("g_vLightPos", new Vector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
            effect.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
            g_LightView = Matrix.LookAtLH(g_LightPos, g_LightPos + g_LightDir, new Vector3(0, 0, 1));

            arrow.PStart = g_LightPos;
            arrow.PEnd = g_LightPos + g_LightDir * 20f;
            arrow.updateValues();
  
            // inicializacion standard: 
            effect.SetValue("g_mProjLight", g_mShadowProj);
            effect.SetValue("g_mViewLightProj", g_LightView * g_mShadowProj);
            // Seteo la tecnica: estoy generando la sombra o estoy dibujando la escena
            effect.Technique = "RenderShadow";            

            // Primero genero el shadow map, para ello dibujo desde el pto de vista de luz
            // a una textura, con el VS y PS que generan un mapa de profundidades. 
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pShadowSurf = g_pShadowMap.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pShadowSurf);
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDSShadow;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            // Hago el render de la escena pp dicha
            effect.SetValue("g_txShadow", g_pShadowMap);
            RenderScene(true);

            // Termino 
            device.EndScene();           

            // restuaro el render target y el stencil
            device.DepthStencilSurface = pOldDS;
            device.SetRenderTarget(0, pOldRT);

        }

        public void RenderScene(bool shadow)
        {
            foreach (MyMesh T in scene.Meshes)
                T.render();            
        }        

        public override void close()
        {
            effect.Dispose();
            scene.disposeAll();            
            g_pShadowMap.Dispose();
            g_pDSShadow.Dispose();
        }            
    }

}
