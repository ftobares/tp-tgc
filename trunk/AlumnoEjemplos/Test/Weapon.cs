using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using TgcViewer;
using Microsoft.DirectX.Direct3D;

namespace AlumnoEjemplos.Test
{
    class Weapon
    {
        private List<TgcMesh> listMesh;
        public bool visible;

        public Weapon(Vector3 initPosition)
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene aScene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir 
                                + "Kamikaze3D\\AK47\\AK47-TgcScene.xml");

            listMesh = aScene.Meshes;

            float dv = 30;
            foreach (TgcMesh m in listMesh)
            {
                m.Scale = new Vector3(2f, 2f, 2f);
                m.Position = new Vector3(initPosition.X + dv, initPosition.Y + dv, initPosition.Z + dv);
                m.rotateY(Geometry.DegreeToRadian(180));
            }
            visible = true;
        }


        public void move(Vector3 v)
        {
            foreach (TgcMesh m in listMesh)
                m.move(v);
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
