using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Terrain;
using AlumnoEjemplos.Test;
using AlumnoEjemplos.Kamikaze3D;

namespace Examples.Collision.SphereCollision
{
    /// <summary>
    /// Ejemplo SphereCollision 
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - SkyBox
    ///     # Unidad 6 - Detección de Colisiones - Estrategia Integral
    /// 
    /// Ejemplo de nivel avanzado.
    /// Se recomienda leer primero EjemploColisionesThirdPerson que posee el mismo espiritu de ejemplo pero mas sencillo.
    /// 
    /// Muestra una posible implementación de la Estrategia Integral de colisiones de una esfera con gravedad y sliding,
    /// explicada en el apunte de Detección de Colisiones.
    /// Utiliza la clase SphereCollisionManager para encapsular la estrategia de colisión. Esta clase se basa
    /// en el paper: http://www.peroxide.dk/papers/collision/collision.pdf.
    /// El paper no ha sido implementado en su totalidad y aún existen muchos puntos por mejorar.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo 
    /// 
    /// </summary>
    public class SphereCollision : TgcExample
    {
        MovimientoController movimientoController;
        List<TgcBoundingBox> objetosColisionables = new List<TgcBoundingBox>();
        TgcScene escenario;        
        List<TgcMesh> objectsBehind = new List<TgcMesh>();
        List<TgcMesh> objectsInFront = new List<TgcMesh>();
        TgcSkyBox skyBox;
        Camara camara;
        bool verLasEsferas;

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Movimientos del personaje";
        }

        public override string getDescription()
        {
            return "";
        }

        private void setConfigVars()
        {
            verLasEsferas = false; //ver las esferas amarillas
        }

        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            setConfigVars();

            //Cargar escenario específico para este ejemplo
            TgcSceneLoader loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(GuiController.Instance.ExamplesDir + "\\Collision\\SphereCollision\\PatioDeJuegos\\PatioDeJuegos-TgcScene.xml");
 
             //Configurar camara en Tercer Persona
            camara = new Camara();
            camara.Enable = true;
            
            camara.TargetDisplacement = new Vector3(0, 100, 0);

            movimientoController = new MovimientoController(camara);

            camara.setCamera(movimientoController.getPersonaje().Position, 100, -400);

            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                objetosColisionables.Add(mesh.BoundingBox);
            }

            movimientoController.setObjetosColisionables(objetosColisionables);

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox3\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "Up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "Down.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "Left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "Right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "Back.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "Front.jpg");
            skyBox.updateValues();


            //Modifier para ver BoundingBox
            GuiController.Instance.Modifiers.addBoolean("showBoundingBox", "Bouding Box", verLasEsferas);

            //Modifiers para desplazamiento del personaje
            GuiController.Instance.Modifiers.addFloat("VelocidadCaminar", 0, 100, 5);
            GuiController.Instance.Modifiers.addFloat("VelocidadRotacion", 1f, 360f, 150f);
            GuiController.Instance.Modifiers.addBoolean("HabilitarGravedad", "Habilitar Gravedad", true);
            GuiController.Instance.Modifiers.addVertex3f("Gravedad", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, -10, 0));
            GuiController.Instance.Modifiers.addFloat("SlideFactor", 1f, 2f, 1.3f);

            GuiController.Instance.UserVars.addVar("Movement");
        }

        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Obtener boolean para saber si hay que mostrar Bounding Box
            bool showBB = (bool)GuiController.Instance.Modifiers.getValue("showBoundingBox");


            //obtener velocidades de Modifiers
            float velocidadCaminar = (float)GuiController.Instance.Modifiers.getValue("VelocidadCaminar");
            float velocidadRotacion = (float)GuiController.Instance.Modifiers.getValue("VelocidadRotacion");


            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
              
            movimientoController.render(elapsedTime);

             //Hacer que la camara siga al personaje en su nueva posicion
            camara.Target = movimientoController.getPersonaje().Position;

            //Ver cual de las mallas se interponen en la visión de la cámara en 3ra persona.
            objectsBehind.Clear();
            objectsInFront.Clear();
            Camara camera = camara;
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                Vector3 q;
                if (TgcCollisionUtils.intersectSegmentAABB(camera.Position, camera.Target, mesh.BoundingBox, out q))
                {
                    objectsBehind.Add(mesh);
                }
                else
                {
                    objectsInFront.Add(mesh);
                }
            }

            //Render mallas que no se interponen
            foreach (TgcMesh mesh in objectsInFront)
            {
                mesh.render();
                if (showBB)
                {
                    mesh.BoundingBox.render();
                }
            }

            //Para las mallas que se interponen a la cámara, solo renderizar su BoundingBox
            foreach (TgcMesh mesh in objectsBehind)
            {
                mesh.BoundingBox.render();
            }



            //Render SkyBox
            skyBox.render();
        }






        public override void close()
        {
            escenario.disposeAll();
            skyBox.dispose();
        }

    }
}
