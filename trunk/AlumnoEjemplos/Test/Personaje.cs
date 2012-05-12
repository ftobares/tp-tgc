using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.Test
{
    class Personaje : TgcSkeletalMesh
    {
        public static void addWeapon(TgcSkeletalMesh personaje, Weapon aWeapon)
        {
            List<TgcMesh> listMesh = aWeapon.getListMesh();

            foreach (TgcMesh m in listMesh)
            {
                TgcSkeletalBoneAttach attachment = new TgcSkeletalBoneAttach();
                attachment.Mesh = m;           
                attachment.Bone = personaje.getBoneByName("Bip01 R Hand");
                //attachment.Offset = Matrix.Translation(0, -10, -15);

                Matrix resultMatrix = Matrix.Translation(-5, -10, -15);
                resultMatrix = Matrix.Multiply(resultMatrix, Matrix.Scaling(0.5f, 0.5f, 0.5f));
                resultMatrix = Matrix.Multiply(resultMatrix, Matrix.RotationY(Geometry.RadianToDegree(90)));
                //resultMatrix = Matrix.Multiply(resultMatrix, Matrix.Translation(0, 10, 0));
                resultMatrix = Matrix.Multiply(resultMatrix, Matrix.Translation(5, 0, 0));
                //resultMatrix = Matrix.Multiply(resultMatrix, Matrix.RotationZ(Geometry.DegreeToRadian(30)));
                //resultMatrix = Matrix.Multiply(resultMatrix, Matrix.RotationX(Geometry.RadianToDegree(20)));
                attachment.Offset = resultMatrix;
                //attachment.Offset = Matrix.Multiply(Matrix.Multiply( 
                //                                        Matrix.Translation(0, -10, -15), 
                //                                        Matrix.Scaling(0.5f, 0.5f, 0.5f)),
                //                                    Matrix.RotationY(Geometry.RadianToDegree(90)));
                attachment.updateValues();
                personaje.Attachments.Add(attachment);
            }
        }
    }
}
