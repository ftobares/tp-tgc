using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Examples.Collision.SphereCollision;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.TgcGeometry;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using System.Windows.Forms;
using AlumnoEjemplos.Kamikaze3D;

namespace AlumnoEjemplos.Test
{
    class PersonajeController
    {
        TgcSkeletalMesh personaje;
        SphereCollisionManager collisionManager;
        TgcBoundingSphere characterSphere;
        TgcArrow directionArrow;
        List<TgcBoundingBox> objetosColisionables = new List<TgcBoundingBox>();

        //variables de pruebas
        bool renderDirectionArrow = false;

        public void setObjetosColisionables(List<TgcBoundingBox> aList) {
            objetosColisionables = aList;
        }

        public PersonajeController()
        {
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
            personaje = skeletalLoader.loadMeshAndAnimationsFromFile(
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\" + "CS_Arctic-TgcSkeletalMesh.xml",
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\",
                new string[] { 
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Walk-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "StandBy-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Jump-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Run-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "HighKick-TgcSkeletalAnim.xml",                    
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Talk-TgcSkeletalAnim.xml",                                        
                });
            //Configurar animacion inicial
            personaje.playAnimation("StandBy", true);
            //Escalarlo para que sea mas grande
            personaje.Position = new Vector3(0, 500, -100);
            personaje.Scale = new Vector3(3f, 3f, 3f);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));

            personaje.AutoUpdateBoundingBox = false;

            //Crear manejador de colisiones
            collisionManager = new SphereCollisionManager();
            collisionManager.GravityEnabled = true;

            characterSphere = new TgcBoundingSphere(personaje.BoundingBox.calculateBoxCenter(), personaje.BoundingBox.calculateBoxRadius());

            //Crear linea para mostrar la direccion del movimiento del personaje
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 1;
            directionArrow.HeadSize = new Vector2(10, 20);

            //Cursor.Hide();
        }

        public TgcSkeletalMesh getPersonaje()
        {
            return personaje;
        }

        public void render(float elapsedTime)
        {
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            //Calcular proxima posicion de personaje segun Input
            float moveForward = 0f;
            float rotate = 0;
            float jump = 0;
            bool moving = false;
            bool rotating = false;
            bool running = false;
            bool jumping = false;
            string animationAction = "StandBy";

            //obtener velocidades de Modifiers (no deberia estar porque es fijo)
            float velocidadCaminar = (float)GuiController.Instance.Modifiers.getValue("VelocidadCaminar");
            float velocidadRotacion = (float)GuiController.Instance.Modifiers.getValue("VelocidadRotacion");
            //Obtener boolean para saber si hay que mostrar Bounding Box (tampoco deberia estar)
            bool showBB = (bool)GuiController.Instance.Modifiers.getValue("showBoundingBox");

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
            if (d3dInput.keyDown(Key.D))// || d3dInput.XposRelative > 0)
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A))// || d3dInput.XposRelative < 0)
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
                else if(jumping)
                    personaje.playAnimation("Jump", true);
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

            //Actualizar valores de la linea de movimiento
            directionArrow.PStart = characterSphere.Center;
            directionArrow.PEnd = characterSphere.Center + Vector3.Multiply(movementVector, 50);
            directionArrow.updateValues();

            //Cargar desplazamiento realizar en UserVar
            GuiController.Instance.UserVars.setValue("Movement", TgcParserUtils.printVector3(realMovement));
            
            //Render linea
            if (renderDirectionArrow)
                directionArrow.render();


            //Render personaje
            personaje.animateAndRender();
            if (showBB)
            {
                characterSphere.render();
            }

        }
    }
}
