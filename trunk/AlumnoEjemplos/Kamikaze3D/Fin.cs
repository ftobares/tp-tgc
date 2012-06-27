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
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils._2D;
using System.Windows.Forms;

namespace AlumnoEjemplos.Kamikaze3D
{
    public class Fin
    {

        TgcSprite sangre;
        TgcSprite texto;

        public void render()
        {
            GuiController.Instance.Drawer2D.beginDrawSprite();
            sangre.render();
            texto.render();
            GuiController.Instance.Drawer2D.endDrawSprite();
        }

        public void init()
        {
            //Crear Sprite de sangre y texto
            sangre = new TgcSprite();
            sangre.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Kamikaze3D\\sangre.png");
            texto = new TgcSprite();
            texto.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Kamikaze3D\\objetivoCumplido.png");

            Size screenSize = GuiController.Instance.Panel3d.Size;

            Control focusWindows = GuiController.Instance.D3dDevice.CreationParameters.FocusWindow;
            sangre.Position = new Vector2(screenSize.Width / 2 - sangre.Texture.Size.Width / 2, screenSize.Height / 2 - sangre.Texture.Size.Height / 2);
            texto.Position = new Vector2(screenSize.Width / 2 - texto.Texture.Size.Width / 2, screenSize.Height / 2 - texto.Texture.Size.Height / 2);
            Console.WriteLine(sangre.Texture.Size.Width / screenSize.Width);
        }

        public void close()
        {
            this.sangre.dispose();
            this.texto.dispose();
        }

    }
}
