using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Shaders
{
    /// <summary>
    /// Ejemplo EjemploBumpMapping:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "SceneLoader/CustomMesh" y luego "Shaders/EjemploShaderTgcMesh".
    /// Muestra como aplicar el efecto de Bump Mapping. No se aplica en toda su plenitud, porque se evita
    /// utilizar Tangentes y Bitangentes. Pero sirve para una aproximación sencilla del efecto.
    /// Bump Mapping toma una textura denominada NormalMap para generar efectos de ondulaciones y protuberancias
    /// en la superficie del modelo
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploBumpMapping: TgcExample
    {

        TgcMeshBumpMapping mesh;
        TgcBox ligtBox;
        TgcTexture normalMap;


        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "BumpMapping";
        }

        public override string getDescription()
        {
            return "BumpMapping";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactoryBumpMapping();

            //Cargar mesh
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\Box-TgcScene.xml");
            mesh = (TgcMeshBumpMapping)scene.Meshes[0];

            //Cambiar textura del mesh
            TgcTexture diffuseMap = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_DiffuseMap.jpg");
            mesh.changeDiffuseMaps(new TgcTexture[] { diffuseMap });
            
            //Cargar textura de BumpMapping
            mesh.NormalMap = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_NormalMap.jpg");

            //Cargar Shader
            string compilationErrors;
            mesh.Effect = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMappingNoTangent.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (mesh.Effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique
            mesh.Effect.Technique = "DefaultTechnique";
            

            //Modifier para variables de shader
            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-100, -100, -100), new Vector3(100, 100, 100), new Vector3(0, 50, 0));
            GuiController.Instance.Modifiers.addColor("AmbientColor", Color.Gray);
            GuiController.Instance.Modifiers.addColor("DiffuseColor", Color.Gray);
            GuiController.Instance.Modifiers.addColor("SpecularColor", Color.Gray);
            GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);            
            

            //Crear caja para indicar ubicacion de la luz
            ligtBox = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Yellow);
            

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
        }


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;

            Vector3 lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];

            //Cargar variables de shader
            mesh.Effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            mesh.Effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            mesh.Effect.SetValue("fvAmbient", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["AmbientColor"]));
            mesh.Effect.SetValue("fvDiffuse", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["DiffuseColor"]));
            mesh.Effect.SetValue("fvSpecular", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["SpecularColor"]));
            mesh.Effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);
            
            //Mover mesh que representa la luz
            ligtBox.Position = lightPosition;
            
            mesh.render();
            ligtBox.render();
        }

        public override void close()
        {
            mesh.dispose();
            mesh.Effect.Dispose();
            ligtBox.dispose();
        }

    }

    /// <summary>
    /// Extendemos de TgcMesh para poder redefinir el método executeRender() y agregar renderizado de Shaders. 
    /// 
    /// </summary>
    public class TgcMeshBumpMapping : TgcMesh
    {
        Effect effect;
        /// <summary>
        /// Shader
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        TgcTexture normalMap;
        /// <summary>
        /// NormalMap para BumpMapping
        /// </summary>
        public TgcTexture NormalMap
        {
            get { return normalMap; }
            set { normalMap = value; }
        }

        public TgcMeshBumpMapping(Mesh mesh, string name, MeshRenderType renderType)
            : base(mesh, name, renderType)
        {
        }

        public TgcMeshBumpMapping(string name, TgcMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
            : base(name, parentInstance, translation, rotation, scale)
        {
        }

        /// <summary>
        /// Se redefine este método para agregar shaders.
        /// Es el mismo código del render() pero con la sección de "MeshRenderType.DIFFUSE_MAP" ampliada
        /// para Shaders.
        /// </summary>
        public new void render()
        {
            if (!enabled)
                return;

            Device device = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Aplicar transformacion de malla
            if (autoTransformEnable)
            {
                this.transform = Matrix.Identity
                    * Matrix.Scaling(scale)
                    * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                    * Matrix.Translation(translation);
            }
            device.Transform.World = this.transform;

            //Cargar VertexDeclaration
            device.VertexDeclaration = vertexDeclaration;


            //RenderType harcodeado para utilizar BumpMapping
            this.effect.SetValue("matWorld", device.Transform.World);
            this.effect.SetValue("matWorldViewProjection", device.Transform.World * device.Transform.View * device.Transform.Projection);

            
            //Cargar NormalMap
            //texturesManager.set(1, normalMap);
            effect.SetValue("bump_Tex", normalMap.D3dTexture);


            //Iniciar Shader e iterar sobre sus Render Passes
            int numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                //Iniciar pasada de shader
                effect.BeginPass(n);

                //Dibujar cada subset con su Material y DiffuseMap correspondiente
                for (int i = 0; i < materials.Length; i++)
                {
                    device.Material = materials[i];
                    
                    
                    //texturesManager.set(0, diffuseMaps[i]);
                    effect.SetValue("base_Tex", diffuseMaps[i].D3dTexture);
                    
                    d3dMesh.DrawSubset(i);
                }

                //Finalizar pasada
                effect.EndPass();
            }

            //Finalizar shader
            effect.End();


        }

    }

    /// <summary>
    /// Factory customizado para poder crear clase TgcMeshShader
    /// </summary>
    public class MyCustomMeshFactoryBumpMapping : TgcSceneLoader.IMeshFactory
    {
        public TgcMesh createNewMesh(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {
            return new TgcMeshBumpMapping(d3dMesh, meshName, renderType);
        }

        public TgcMesh createNewMeshInstance(string meshName, TgcMesh originalMesh, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            return new TgcMeshBumpMapping(meshName, originalMesh, translation, rotation, scale);
        }
    }

    

}
