using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using AlumnoEjemplos.Kamikaze3D;
using TgcViewer.Utils.Input;

namespace Examples.SkeletalAnimation
{
    /// <summary>
    /// Ejemplo EjemploMeshInstance:
    /// Unidades Involucradas:
    ///     # Unidad 5 - Animación - Skeletal Animation
    ///     # Unidad 7 - Técnicas de Optimización - Instancias de Modelos
    /// 
    /// Muestra como crear instancias de modelos animados con Skeletal Animation.
    /// Al crear instancias de un único modelo original se reutiliza toda su información
    /// gráfica (animaciones, vértices, texturas, etc.)
    /// 
    /// 
    /// Autor: Leandro Barbagallo, Matías Leone
    /// 
    /// </summary>
    public class EjPersonajes : TgcExample
    {
        TgcBox suelo;
        TgcBox box1;
        TgcBox box2;
        TgcBox box3;
        TgcSkeletalMesh original;
        List<TgcSkeletalMesh> instances;
        TgcThirdPersonCamera camara;
        Personaje personaje;
        Quadtree quadtree; 

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Muchos personajes";
        }

        public override string getDescription()
        {
            return "Muestra como crear instancias de modelos animados con Skeletal Animation.";
        }

        public override void init()
        {
            camara = new TgcThirdPersonCamera();
            personaje = new Personaje(camara, new Explosion());

            this.camara.Enable = true;
            this.personaje.getPersonaje().Position = new Vector3(100,0,100);
            this.camara.setCamera(this.personaje.getPersonaje().Position, 50, -100);
            this.personaje.init();

            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear suelo
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack2\\rock_floor1.jpg");
            suelo = TgcBox.fromSize(new Vector3(500, 0, 500), new Vector3(4000, 0, 4000), pisoTexture);
            box1 = TgcBox.fromSize(new Vector3(500, 0, 500), new Vector3(550, 550, 550), pisoTexture);
            box2 = TgcBox.fromSize(new Vector3(100, 0, 100), new Vector3(550, 550, 550), pisoTexture);
            box3 = TgcBox.fromSize(new Vector3(1400, 0, 900), new Vector3(550, 550, 550), pisoTexture);

            //Cargar malla original
            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\" + "CS_Gign-TgcSkeletalMesh.xml";
            string mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\";
            original = loader.loadMeshFromFile(pathMesh, mediaPath);

            string posicionStr = "Disparar";
            //Agregar animación a original
            loader.loadAnimationFromFile(original, GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Animations\\" + posicionStr + "-TgcSkeletalAnim.xml");
            //loader.loadAnimationFromFile(original, mediaPath + "Animations\\" + posicionStr + "-TgcSkeletalAnim.xml");

            //Agregar attachment a original
            TgcSkeletalBoneAttach attachment = new TgcSkeletalBoneAttach();
            TgcBox attachmentBox = TgcBox.fromSize(new Vector3(1, 10, 1), Color.Black);
            
            Matrix offsetMatrix = Matrix.RotationY(Geometry.DegreeToRadian(190));
            offsetMatrix = Matrix.Multiply(offsetMatrix, Matrix.RotationZ(Geometry.DegreeToRadian(80)));
            offsetMatrix = Matrix.Multiply(offsetMatrix, Matrix.Translation(10, 0, 0));
            offsetMatrix = Matrix.Multiply(offsetMatrix, Matrix.RotationX(Geometry.DegreeToRadian(300)));
            offsetMatrix = Matrix.Multiply(offsetMatrix, Matrix.Translation(-2.5f, -1.5f, 0));

            attachment.Mesh = attachmentBox.toMesh("attachment");
            attachment.Bone = original.getBoneByName("Bip01 L Hand");
            attachment.Offset = offsetMatrix; 
            attachment.updateValues();
            original.Scale = new Vector3(0.7f, 0.7f, 0.7f);
            original.Attachments.Add(attachment);


            //Crear 9 instancias mas de este modelo, pero sin volver a cargar el modelo entero cada vez
            float offset = 200;
            int cantInstancias = 150;
            instances = new List<TgcSkeletalMesh>();
            for (int i = 0; i < cantInstancias; i++)
			{
                TgcSkeletalMesh instance = original.createMeshInstance(original.Name + i);
                instance.move((i % 20)* offset, 0, (i / 20) * offset);
                instance.Scale = original.Scale;
                instances.Add(instance);
			}


            //Especificar la animación actual para todos los modelos
            original.playAnimation(posicionStr);
            foreach (TgcSkeletalMesh instance in instances)
            {
                instance.playAnimation(posicionStr);
            }

            //Crear quadtree
            quadtree = new Quadtree();
            //como el bounding box del suelo no tiene altura se la agrego
            Vector3 Pmax = suelo.BoundingBox.PMax;
            Pmax.Y = Pmax.Y + 500;
            TgcBoundingBox bb = new TgcBoundingBox(suelo.BoundingBox.PMin, Pmax);
            quadtree.create(instances, bb);
            quadtree.createDebugQuadtreeMeshes();

            return;
            //Camara en primera persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400;
            GuiController.Instance.FpsCamera.JumpSpeed = 400;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(293.201f, 291.0797f, -604.6647f), new Vector3(299.1028f, -63.9185f, 330.1836f));
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            this.camara.Target = this.personaje.getPersonaje().Position;
            this.personaje.render(elapsedTime);

            //Renderizar suelo
            suelo.render();

            //Renderizar original e instancias
            original.animateAndRender();
            //Renderizar el quadtree que tiene las instancias 
            quadtree.render(GuiController.Instance.Frustum, this.personaje, false);
            //foreach (TgcSkeletalMesh instance in instances)
            //{
            //    instance.animateAndRender();
            //}
            //Renderizar cajas
            box1.render();
            box2.render();
            box3.render();

        }

        public override void close()
        {
            suelo.dispose();

            //Al hacer dispose del original, se hace dispose automáticamente de todas las instancias
            original.dispose();
        }

    }
}
