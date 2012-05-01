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
        PersonajeController personajeController;
        List<TgcBoundingBox> objetosColisionables = new List<TgcBoundingBox>();
        TgcBoundingSphere characterSphere;
        TgcArrow directionArrow;
        TgcScene escenario;
        SphereCollisionManager collisionManager;
        List<TgcMesh> objectsBehind = new List<TgcMesh>();
        List<TgcMesh> objectsInFront = new List<TgcMesh>();
        TgcSkyBox skyBox;        

        bool renderDirectionArrow;
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
            renderDirectionArrow = false; //ver la linea de direccion
        }

        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            setConfigVars();

            //Cargar escenario específico para este ejemplo
            TgcSceneLoader loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(GuiController.Instance.ExamplesDir + "\\Collision\\SphereCollision\\PatioDeJuegos\\PatioDeJuegos-TgcScene.xml");

            personajeController = new PersonajeController();

            //BoundingSphere que va a usar el personaje
            characterSphere = new TgcBoundingSphere(personajeController.getPersonaje().BoundingBox.calculateBoxCenter(), personajeController.getPersonaje().BoundingBox.calculateBoxRadius());


            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                objetosColisionables.Add(mesh.BoundingBox);
            }


            //Crear linea para mostrar la direccion del movimiento del personaje
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 1;
            directionArrow.HeadSize = new Vector2(10, 20);

            //Crear manejador de colisiones
            collisionManager = new SphereCollisionManager();
            collisionManager.GravityEnabled = true;


            //Configurar camara en Tercer Persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(personajeController.getPersonaje().Position, 100, -400);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 100, 0);

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


            //Calcular proxima posicion de personaje segun Input
            float moveForward = 0f;
            float rotate = 0;
            float jump = 0;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            bool moving = false;
            bool rotating = false;
            bool running = false;                        
            string animationAction = "StandBy";

            TgcSkeletalMesh personaje = personajeController.getPersonaje();

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (d3dInput.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }

            //Jump
            if (d3dInput.keyDown(Key.Space))
            {
                jump = 30;
                moving = true;
            }
            //Run
            if (d3dInput.keyDown(Key.LeftShift))
            {
                running = true;
                moveForward *= 1.5f;
            }

            if (d3dInput.keyDown(Key.E))
            {
                animationAction = "Talk";
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                float rotAngle = Geometry.DegreeToRadian(rotate * elapsedTime);
                personaje.rotateY(rotAngle);
                GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                if (running)
                    personaje.playAnimation("Run", true);
                else
                    personaje.playAnimation("Walk", true);
            }

            //Si no se esta moviendo, activar animationAction
            else
            {
                personaje.playAnimation(animationAction, true);
            }



            //Vector de movimiento
            Vector3 movementVector = Vector3.Empty;
            if (moving)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new Vector3(
                    FastMath.Sin(personaje.Rotation.Y) * moveForward,
                    jump,
                    FastMath.Cos(personaje.Rotation.Y) * moveForward
                    );
            }


            //Actualizar valores de gravedad
            collisionManager.GravityEnabled = (bool)GuiController.Instance.Modifiers["HabilitarGravedad"];
            collisionManager.GravityForce = (Vector3)GuiController.Instance.Modifiers["Gravedad"];
            collisionManager.SlideFactor = (float)GuiController.Instance.Modifiers["SlideFactor"];


            //Mover personaje con detección de colisiones, sliding y gravedad
            Vector3 realMovement = collisionManager.moveCharacter(characterSphere, movementVector, objetosColisionables);
            personaje.move(realMovement);

             //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = personaje.Position;


            //Actualizar valores de la linea de movimiento
            directionArrow.PStart = characterSphere.Center;
            directionArrow.PEnd = characterSphere.Center + Vector3.Multiply(movementVector, 50);
            directionArrow.updateValues();

            //Cargar desplazamiento realizar en UserVar
            GuiController.Instance.UserVars.setValue("Movement", TgcParserUtils.printVector3(realMovement));




            //Ver cual de las mallas se interponen en la visión de la cámara en 3ra persona.
            objectsBehind.Clear();
            objectsInFront.Clear();
            TgcThirdPersonCamera camera = GuiController.Instance.ThirdPersonCamera;
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



            //Render personaje
            personaje.animateAndRender();
            if (showBB)
            {
                characterSphere.render();
            }

            //Render linea
            if(renderDirectionArrow)
                directionArrow.render();

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
