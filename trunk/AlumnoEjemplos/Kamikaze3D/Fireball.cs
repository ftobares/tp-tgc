using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Kamikaze3D
{
    class Fireball
    {

        TgcAnimatedSprite animatedSprite;

        public void init()
        {
            //Crear Sprite animado
            this.animatedSprite = new TgcAnimatedSprite(
                GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Animations\\explosion2.png",
                new Size(64, 64), //Tamaño de un frame
                25, //Cantidad de frames
                10 //Velocidad de animacion, en cuadros x segundo
            );

            //Ubicarlo centrado en la pantalla
            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = this.animatedSprite.Sprite.Texture.Size;
            this.animatedSprite.Position = new Vector2(screenSize.Width / 2 - textureSize.Width / 2, screenSize.Height / 2 - textureSize.Height / 2);
            this.animatedSprite.setFrameRate((float)10);

        }

        public void render()
        {

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            //Actualizamos el estado de la animacion y renderizamos
            this.animatedSprite.updateAndRender();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

        }

        public void dispose()
        {
            this.animatedSprite.dispose();
        }

    }
}
