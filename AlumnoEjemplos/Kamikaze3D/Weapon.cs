using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using TgcViewer;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.Kamikaze3D
{
    class Weapon
    {
        private List<TgcMesh> listMesh;
        public bool visible;
        private float dx, dy, dz;

        public List<TgcMesh> getListMesh()
        {
            return listMesh;
        }

        public Weapon(Vector3 initPosition)
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene aScene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir 
                                + "Kamikaze3D\\AK47\\AK47-TgcScene.xml");

            listMesh = aScene.Meshes;

            float dv = 30;
            dx = dy = dz = dv;
            dy += 45; //altura
            dx += 0;
            foreach (TgcMesh m in listMesh)
            {
                m.Scale = new Vector3(2f, 2f, 2f);                                
                m.Position = new Vector3(initPosition.X + dx, initPosition.Y + dy, initPosition.Z + dz);
                m.rotateX(Geometry.DegreeToRadian(-20));
                m.rotateY(Geometry.DegreeToRadian(150));
                //m.rotateZ(Geometry.DegreeToRadian(-20));
            }
            //dx = listMesh[0].Position.X - initPosition.X;
            //dy = listMesh[0].Position.Y - initPosition.Y;
            //dz = listMesh[0].Position.Z - initPosition.Z;
            visible = true;

        }


        public void move(Vector3 v)
        {
            Vector3 newPos = new Vector3(v.X + dx, v.Y + dy, v.Z + dz);
            foreach (TgcMesh m in listMesh)
            {
                m.Position = newPos;
            }
        }

        public void render()
        {
            if (!visible) return;
            foreach (TgcMesh m in listMesh)
                m.render();
        }

        public void rotateY(float rotAngle)
        {
            foreach (TgcMesh m in listMesh)
                m.rotateY(rotAngle);
        }

    }
}
