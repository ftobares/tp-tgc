using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.Kamikaze3D
{
    class Police
    {
        private TgcSkeletalMesh original;
        private List<TgcSkeletalMesh> instances;

        public Police(int numberOfInstances, List<TgcBoundingBox> listObjColisionables, TgcBoundingBox scene)
        {
            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\" + "CS_Gign-TgcSkeletalMesh.xml";
            string mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\";
            original = loader.loadMeshFromFile(pathMesh, mediaPath);
            loader.loadAnimationFromFile(original, GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Animations\\Disparar-TgcSkeletalAnim.xml");
            original.Scale = new Vector3(0.7f, 0.7f, 0.7f);
            addWeapon();

            //Crear la cantidad de instancias de este modelo, pero sin volver a cargar el modelo entero cada vez
            float offset = 200;
            int cantInstancias = numberOfInstances;
            instances = new List<TgcSkeletalMesh>();
            for (int i = 0; i < cantInstancias; i++)
            {
                TgcSkeletalMesh instance = original.createMeshInstance(original.Name + i);
                instance.move((i % 20) * offset, 3.5f, (i / 20) * offset);
                instance.Scale = original.Scale;

                if (TgcCollisionUtils.classifyBoxBox(instance.BoundingBox, scene) != TgcCollisionUtils.BoxBoxResult.Encerrando)
                    continue; //si esta afuera del escenario lo descarto

                bool colisionando = false;
                foreach (TgcBoundingBox bb in listObjColisionables)
                {
                    if (TgcCollisionUtils.classifyBoxBox(instance.BoundingBox, bb) != TgcCollisionUtils.BoxBoxResult.Afuera)
                    {
                        colisionando = true;
                        break;
                    }
                }
                if (colisionando) continue;//Si colisiona con algo no se debe agregar
                instances.Add(instance);
            }

            //Especificar la animación actual para todos los modelos
            original.playAnimation("Disparar");
            //foreach (TgcSkeletalMesh instance in instances)
            //{
            //    instance.playAnimation("Disparar");
            //}         

        }

        private void addWeapon()
        {
            //Agregar attachment a original
            TgcSkeletalBoneAttach attachment = new TgcSkeletalBoneAttach();
            TgcBox attachmentBox = TgcBox.fromSize(new Vector3(1, 10, 1), Color.Black);
            attachment.Mesh = attachmentBox.toMesh("attachment");
            attachment.Bone = original.getBoneByName("Bip01 L Hand");

            Matrix offsetMatrix = Matrix.RotationY(Geometry.DegreeToRadian(190));
            offsetMatrix = Matrix.Multiply(offsetMatrix, Matrix.RotationZ(Geometry.DegreeToRadian(80)));
            offsetMatrix = Matrix.Multiply(offsetMatrix, Matrix.Translation(10, 0, 0));
            offsetMatrix = Matrix.Multiply(offsetMatrix, Matrix.RotationX(Geometry.DegreeToRadian(300)));
            offsetMatrix = Matrix.Multiply(offsetMatrix, Matrix.Translation(-2.5f, -1.5f, 0));
            
            attachment.Offset = offsetMatrix;
            attachment.updateValues();
            original.Attachments.Add(attachment);
        }

        public List<TgcSkeletalMesh> getInstances()
        {
            return instances;
        }

    }
}
