using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;


namespace AlumnoEjemplos.Kamikaze3D
{
    public class Personaje
    {
        TgcSkeletalMesh personaje;
        Weapon weapon;
        SphereCollisionManager collisionManager;
        TgcBoundingSphere characterSphere;
        TgcArrow directionArrow;
        List<TgcBoundingBox> objetosColisionables = new List<TgcBoundingBox>();
        List<Vector3> personajesColisionables = new List<Vector3>();
        TgcThirdPersonCamera camara;
        Explosion explosion;
        int life = 100;
        bool dead = false;
        public bool canExplode = false;
        bool destroy = false;
        TgcSprite mira;
        List<Proyectil>  balas = new List<Proyectil>();
        int countDeads = 0;

        //variables de pruebas
        bool renderDirectionArrow = false;

        public void setObjetosColisionables(List<TgcBoundingBox> cList,List<TgcBoundingBox> bList) {            
            cList.AddRange(bList);
            this.objetosColisionables = cList;            
        }

        public void setPersonajesColisionables(List<Vector3> aList)
        {
            this.personajesColisionables = aList;
        }

        public void clearPersonajesColisionables()
        {
            this.personajesColisionables.Clear();
        }
        
        public void addObjetosColisionables(Vector3 position)
        {
            this.personajesColisionables.Add(position);
        }

        public Personaje(TgcThirdPersonCamera camaraParametro, Explosion explosion)
        {

            //Crear personaje
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
            this.personaje = skeletalLoader.loadMeshAndAnimationsFromFile(
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\" + "CS_Arctic-TgcSkeletalMesh.xml",
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\",
                new string[] { 
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Walk-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "StandBy-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Jump-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Run-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "HighKick-TgcSkeletalAnim.xml",                    
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Talk-TgcSkeletalAnim.xml",                                        
                    GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Animations\\WeaponPos-TgcSkeletalAnim.xml",
                    GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\Animations\\Muerte-TgcSkeletalAnim.xml",

                });

            //Configurar animacion inicial
            this.personaje.playAnimation("StandBy", true);
            
            //Escalarlo y posicionarlo
            this.personaje.Position = new Vector3(-100f, 3.5f, -340f);
            this.personaje.Scale = new Vector3(0.7f, 0.7f, 0.7f);

            //Rotarlo 180° porque esta mirando para el otro lado
            this.personaje.rotateY(Geometry.DegreeToRadian(180f));

            this.personaje.AutoUpdateBoundingBox = false;

            //Crear manejador de colisiones
            this.collisionManager = new SphereCollisionManager();
            this.collisionManager.GravityEnabled = false;
            this.characterSphere = new TgcBoundingSphere(personaje.BoundingBox.calculateBoxCenter(), personaje.BoundingBox.calculateBoxRadius());

            //Crear linea para mostrar la direccion del movimiento del personaje
            this.directionArrow = new TgcArrow();
            this.directionArrow.BodyColor = Color.Red;
            this.directionArrow.HeadColor = Color.Green;
            this.directionArrow.Thickness = 1;
            this.directionArrow.HeadSize = new Vector2(10, 20);

            this.camara = camaraParametro;

            this.weapon = new Weapon(personaje.Position);
            
            //Agregar el arma al personaje
            this.addWeapon(personaje, weapon);

            this.explosion = explosion;
           
        }

        public void init()
        {   
            //Modifiers para desplazamiento del personaje
            GuiController.Instance.Modifiers.addFloat("VelocidadCaminar", 0, 3f, 1.5f);
            GuiController.Instance.Modifiers.addFloat("VelocidadRotacion", 40f, 100f, 70f);
            //GuiController.Instance.Modifiers.addBoolean("HabilitarGravedad", "Habilitar Gravedad", true);
            //GuiController.Instance.Modifiers.addVertex3f("Gravedad", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, -10, 0));
            GuiController.Instance.Modifiers.addFloat("SlideFactor", 0f, 2f, 0.5f);

            GuiController.Instance.UserVars.addVar("Movement");
            
            //Crear Sprite de mira
            mira = new TgcSprite();
            mira.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "\\Kamikaze3D\\mira2.png");
            Control focusWindows = GuiController.Instance.D3dDevice.CreationParameters.FocusWindow;
            mira.Position = new Vector2(focusWindows.Width * 0.4f, focusWindows.Height * 0.18f);
            
        }

        public TgcSkeletalMesh getPersonaje()
        {
            return personaje;
        }

        public void addWeapon(TgcSkeletalMesh personaje, Weapon aWeapon)
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

        public void render(float elapsedTime)
        {
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            //Calcular proxima posicion de personaje segun Input
            float moveForward = 0f;
            float moveSide = 0f;
            float rotateY = 0;
            //float rotateX = 0;
            float jump = 0;
            bool moving = false;
            bool rotating = false;
            bool rotatingY = false;
           // bool rotatingX = false;
            bool running = false;
            bool jumping = false;            
            string animationAction = "StandBy";

            //Ubicarlo centrado en la pantalla
            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = mira.Texture.Size;

            mira.Enabled = false;
            
            TgcText2d hitCantTextY = new TgcText2d();
            hitCantTextY.Position = new Point(0, 0);
            hitCantTextY.Color = Color.White;

          /*  TgcText2d hitCantTextX = new TgcText2d();
            hitCantTextX.Position = new Point(0, 20);
            hitCantTextX.Color = Color.White;
            */
            if(destroy)
                goto Rendering;

            //obtener velocidades de Modifiers
            float velocidadCaminar = (float)GuiController.Instance.Modifiers["VelocidadCaminar"];
            float velocidadRotacion = (float)GuiController.Instance.Modifiers.getValue("VelocidadRotacion");

            //Detonar
            if (this.canExplode && d3dInput.keyDown(Key.K))
            {
                animationAction = "Talk";
                this.explosion.detonar();
                destroy = true;
            }

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                if (!moving)
                {
                    moveForward = -velocidadCaminar;
                }
                else
                {
                    moveForward = -velocidadCaminar / 2;
                }
                moving = true;
            }


            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                if (!moving)
                {
                    moveForward = velocidadCaminar;
                }
                else
                {
                    moveForward = velocidadCaminar / 2;
                }
                moving = true;
            }

            //Derecha
            if (d3dInput.XposRelative > 0 || d3dInput.keyDown(Key.RightArrow))
            {
                if (d3dInput.XposRelative > 0){ rotateY = (velocidadRotacion / 2); }
                else { rotateY = velocidadRotacion; }
               
                rotating = true;
                rotatingY = true;
            }

            //Izquierda
            if (d3dInput.XposRelative < 0 || d3dInput.keyDown(Key.LeftArrow))
            {

                if (d3dInput.XposRelative < 0) { rotateY = -(velocidadRotacion / 2); }
                else { rotateY = -velocidadRotacion; }

                rotating = true;
                rotatingY = true;
            }

            //Mover Derecha
            if (d3dInput.keyDown(Key.D))
            {
                if (!moving)
                {
                    moveSide = -velocidadCaminar;
                }
                else
                {
                    moveSide = -velocidadCaminar / 2;
                }
                moving = true;
            }
            

            //Mover Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                if (!moving)
                {
                    moveSide = velocidadCaminar;
                }
                else
                {
                    moveSide = velocidadCaminar / 2;
                }
                moving = true;
            }

/*            //Arriba
            if (d3dInput.YposRelative < 0)
            {
                //if (camara.RotationX > -(Math.PI / 3))
                //{
                    rotateX = -velocidadRotacion;
                    rotating = true;
                    rotatingX = true;
                //}
            }

            //Abajo
            if (d3dInput.YposRelative > 0)
            {
                //if (camara.RotationX < (Math.PI / 3))
                //{
                    rotateX = velocidadRotacion;
                    rotating = true;
                    rotatingX = true;
                //}
            }
            */
            //Jump
            /*if (d3dInput.keyDown(Key.Space))
            {
                jump = 30;                
                moving = true;                
            }*/

            //Run
            if (d3dInput.keyDown(Key.LeftShift))
            {
                running = true;
                moveForward *= 1.5f;
            }

            if (d3dInput.keyDown(Key.E)|| d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                mira.Enabled = true;
                animationAction = "WeaponPos";
                if (d3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    this.fire();
                }
            }

            //Si hubo rotacion
            if (rotating)
            {
                  if (rotatingY)
                {
                    //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                    
                    float rotAngleY = Geometry.DegreeToRadian(rotateY * elapsedTime);
                    this.personaje.rotateY(rotAngleY);
                    this.camara.rotateY(rotAngleY);
                }
                /*  if (rotatingX)
                {
                    //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                    float rotAngleX = Geometry.DegreeToRadian(rotateX * elapsedTime);
                    this.camara.rotateX(rotAngleX);
                    
                }*/
                
            }


            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                if (running)
                    this.personaje.playAnimation("Run", true);
                else if(jumping)
                    this.personaje.playAnimation("Jump", true);
                else
                    this.personaje.playAnimation("Walk", true);
            }

            //Si no se esta moviendo, activar animationAction
            else
            {
                this.personaje.playAnimation(animationAction, true);
            }

            if(life <= 0)
                this.personaje.playAnimation("Muerte", true);


            //Vector de movimiento
            Vector3 movementVector = Vector3.Empty;
            if (moving)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                //Grupo Kamikaze3D :: Se agrega también al desplazamiento sobre el eje x y eje z, el valor de desplazamiento lateral
                movementVector = new Vector3(
                    (FastMath.Sin(this.personaje.Rotation.Y) * moveForward) + (FastMath.Cos(this.personaje.Rotation.Y) * moveSide),
                    jump,
                    (FastMath.Cos(this.personaje.Rotation.Y) * moveForward) + (-FastMath.Sin(this.personaje.Rotation.Y) * moveSide) 
                    );
            }


           //Mover personaje con detección de colisiones, sliding y gravedad
            Vector3 realMovement;
            if (collisionWithCharacters(movementVector))
                realMovement = Vector3.Empty;
            else    
                realMovement = collisionManager.moveCharacter(this.characterSphere, movementVector, this.objetosColisionables);
            this.personaje.move(realMovement);

            //Actualizar valores de la linea de movimiento
            this.directionArrow.PStart = characterSphere.Center;
            this.directionArrow.PEnd = characterSphere.Center + Vector3.Multiply(movementVector, 50);
            this.directionArrow.updateValues();

            //Caargar desplazamiento realizar en UserVar
            GuiController.Instance.UserVars.setValue("Movement", TgcParserUtils.printVector3(realMovement));

        Rendering:

            //Render linea
            if (renderDirectionArrow)
                this.directionArrow.render();

            this.renderBullets();
            //Render personaje
            this.personaje.animateAndRender();

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            mira.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

            hitCantTextY.Text = "Y: " + camara.RotationY;
            //hitCantTextX.Text = "X: " + camara.RotationX;
            hitCantTextY.render();
            //hitCantTextX.render();

        }

        private bool collisionWithCharacters(Vector3 movVector)
        {
            float collisionDistance = 200;
            float posX = this.personaje.Position.X + movVector.X;
            float posZ = this.personaje.Position.Z + movVector.Z;

            foreach (Vector3 posicion in personajesColisionables)
                if(FastMath.Pow2(posicion.X - posX) +
                   FastMath.Pow2(posicion.Z - posZ) 
                    < collisionDistance)
                    return true;
            return false;
        }

        public bool kill(TgcSkeletalMesh enemy)
        {
            bool result = false;
            if (mira.Enabled)
            {
                foreach (Proyectil p in balas)
                {
                    if (TgcCollisionUtils.testAABBAABB(p.getBoundingBox(), enemy.BoundingBox))
                    {
                        result = true;
                        break;
                    }
                }
            }
            if (result)
            {
                balas.RemoveAll(x => true);
                countDeads++;
            }
            return result;
        }

        public int getLife()
        {
            return life;
        }

        int countDamage = 0;
        public void damage(int damage)
        {
            countDamage++;
            if (countDamage == 500)
            {
                life -= damage;
                countDamage = 0;

                if (life == 0)
                    this.die();
            }
        }

        public void die() 
        {
           // GuiController.Instance.Mp3Player.closeFile();
           // GuiController.Instance.Mp3Player.FileName = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\AK47\\death.mp3";
           // GuiController.Instance.Mp3Player.play(false);

            TgcStaticSound morir = new TgcStaticSound();
            morir.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\AK47\\death.wav");
            morir.play();

            this.dead = true;
        }

        public bool alive()
        {
            return !this.dead;
        }

        public Vector3 getPosicionBala()
        {
            Vector3 lastPosition = this.personaje.Position;
            Vector3 Ans;

            this.personaje.moveOrientedY(1f);
            Ans = this.personaje.Position;
            this.personaje.Position = lastPosition;

            return Ans;
        }

        private void fire()
        {
            //Codigo de disparar
            Proyectil bala = new Proyectil(this);
            bala.inicializar();
            balas.Add(bala);

            //GuiController.Instance.Mp3Player.closeFile();
           // GuiController.Instance.Mp3Player.FileName = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\AK47\\gunshot.mp3";
            //GuiController.Instance.Mp3Player.play(false);

            TgcStaticSound sonidoTiro = new TgcStaticSound();
            sonidoTiro.loadSound(GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\AK47\\gunshot.wav");
            sonidoTiro.play();

        }

        private void renderBullets()
        {
            foreach (Proyectil p in balas) //saco las balas que no estan mas
                if (p == null)
                    balas.Remove(p);

            /*Entes.renderizarElConjuntoDe(balas, elapsedTime);*/

            List<Proyectil> elementosBuffer = new List<Proyectil>();
            elementosBuffer.Clear();

            foreach (Proyectil elemento in balas)
            {
                if (elemento.renderizar())
                    elementosBuffer.Add(elemento);
                else
                    elemento.dispose();
            }
            balas.Clear();
            foreach (Proyectil elemento in elementosBuffer)
            {
                balas.Add(elemento);
            }
        }

        private bool finishAnimationDead = false;
        public void renderDeading()
        {
            if (finishAnimationDead) return;
            this.personaje.playAnimation("Muerte", false);
            this.personaje.animateAndRender();
            if (!this.personaje.IsAnimating)
                finishAnimationDead = true;
        }
        
        public void close()
        {
            this.personaje.dispose();
        }
    }
}
