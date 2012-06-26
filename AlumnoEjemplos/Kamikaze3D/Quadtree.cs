using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer;

namespace AlumnoEjemplos.Kamikaze3D
{
    /// <summary>
    /// Herramienta para crear y utilizar un Quadtree para renderizar por Frustum Culling
    /// </summary>
    public class Quadtree
    {

        QuadtreeNode quadtreeRootNode;
        List<TgcSkeletalMesh> modelos;
        List<TgcSkeletalMesh> deads = new List<TgcSkeletalMesh>();
        Dictionary<int, int> controllerDeads = new Dictionary<int, int>();
        TgcBoundingBox sceneBounds;
        QuadtreeBuilder builder;
        List<TgcDebugBox> debugQuadtreeBoxes;

        public Quadtree()
        {
            builder = new QuadtreeBuilder();
        }

        public List<TgcSkeletalMesh> getModelos() {
            return modelos;
        }

        /// <summary>
        /// Crear nuevo Quadtree
        /// </summary>
        /// <param name="modelos">Modelos a optimizar</param>
        /// <param name="sceneBounds">Límites del escenario</param>
        public void create(List<TgcSkeletalMesh> modelos, TgcBoundingBox sceneBounds)
        {
            this.modelos = modelos;
            this.sceneBounds = sceneBounds;

            //Crear Quadtree
            this.quadtreeRootNode = builder.crearQuadtree(modelos, sceneBounds);

            //Deshabilitar todos los mesh inicialmente
            foreach (TgcSkeletalMesh mesh in modelos)
            {
                mesh.Enabled = false;
            }
        }

        /// <summary>
        /// Crear meshes para debug
        /// </summary>
        public void createDebugQuadtreeMeshes()
        {
            debugQuadtreeBoxes = builder.createDebugQuadtreeMeshes(quadtreeRootNode, sceneBounds);
        }

        /// <summary>
        /// Renderizar en forma optimizado utilizando el Quadtree para hacer FrustumCulling
        /// </summary>
        public void render(TgcFrustum frustum, Personaje mainPJ, bool debugEnabled)
        {
            Vector3 pMax = sceneBounds.PMax;
            Vector3 pMin = sceneBounds.PMin;
            findVisibleMeshes(frustum, quadtreeRootNode,
                pMin.X, pMin.Y, pMin.Z,
                pMax.X, pMax.Y, pMax.Z);

            //Renderizar
            float pjX = mainPJ.getPersonaje().Position.X;
            float pjZ = mainPJ.getPersonaje().Position.Z;
            foreach (TgcSkeletalMesh mesh in modelos)
            {
                if (mesh.Enabled && FastMath.Abs(mesh.Position.X - pjX) < 1000 && FastMath.Abs(mesh.Position.Z - pjZ) < 1000)
                {
                    Vector3 vec = mesh.Position - mainPJ.getPersonaje().Position;
                    double anguloFinal = Math.Atan2(vec.X, vec.Z);
                    mesh.rotateY(-mesh.Rotation.Y);
                    mesh.rotateY((float)anguloFinal);
                    mesh.render();
                    mesh.Enabled = false;
                    mainPJ.damage(1);
                    if (mainPJ.kill(mesh))
                    {
                        GuiController.Instance.Mp3Player.closeFile();
                        GuiController.Instance.Mp3Player.FileName = GuiController.Instance.AlumnoEjemplosMediaDir + "Kamikaze3D\\AK47\\pain.mp3";
                        GuiController.Instance.Mp3Player.play(false);

                        deads.Add(mesh);
                        break;
                    }
                }
            }
            
            if (deads.Count > 0)
            {
                List<TgcSkeletalMesh> auxList = new List<TgcSkeletalMesh>();
                foreach (TgcSkeletalMesh mesh in deads)
                {
                    modelos.Remove(mesh);
                    //mesh.playAnimation("Muerte", false);
                    //mesh.animateAndRender();
                    //if (mesh.CurrentAnimation.Name.Contains("Muerte") && !mesh.IsAnimating)
                        auxList.Add(mesh);
                }
                foreach (TgcSkeletalMesh m in auxList)
                    deads.Remove(m);
            }

            if (debugEnabled)
            {
                foreach (TgcDebugBox debugBox in debugQuadtreeBoxes)
                {
                    debugBox.render();
                }
            }
        }


        /// <summary>
        /// Recorrer recursivamente el Quadtree para encontrar los nodos visibles
        /// </summary>
        private void findVisibleMeshes(TgcFrustum frustum, QuadtreeNode node,
            float boxLowerX, float boxLowerY, float boxLowerZ,
            float boxUpperX, float boxUpperY, float boxUpperZ)
        {
            QuadtreeNode[] children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);
            }

            //recursividad sobre hijos
            else
            {
                float midX = FastMath.Abs((boxUpperX - boxLowerX) / 2);
                float midZ = FastMath.Abs((boxUpperZ - boxLowerZ) / 2);

                //00
                testChildVisibility(frustum, children[0], boxLowerX + midX, boxLowerY, boxLowerZ + midZ, boxUpperX, boxUpperY, boxUpperZ);

                //01
                testChildVisibility(frustum, children[1], boxLowerX + midX, boxLowerY, boxLowerZ, boxUpperX, boxUpperY, boxUpperZ - midZ);

                //10
                testChildVisibility(frustum, children[2], boxLowerX, boxLowerY, boxLowerZ + midZ, boxUpperX - midX, boxUpperY, boxUpperZ);
                
                //11
                testChildVisibility(frustum, children[3], boxLowerX, boxLowerY, boxLowerZ, boxUpperX - midX, boxUpperY, boxUpperZ - midZ);


            }
        }


        /// <summary>
        /// Hacer visible las meshes de un nodo si es visible por el Frustum
        /// </summary>
        private void testChildVisibility(TgcFrustum frustum, QuadtreeNode childNode,
                float boxLowerX, float boxLowerY, float boxLowerZ, float boxUpperX, float boxUpperY, float boxUpperZ)
        {

            //test frustum-box intersection
            TgcBoundingBox caja = new TgcBoundingBox(
                new Vector3(boxLowerX, boxLowerY, boxLowerZ),
                new Vector3(boxUpperX, boxUpperY, boxUpperZ));
            TgcCollisionUtils.FrustumResult c = TgcCollisionUtils.classifyFrustumAABB(frustum, caja);

            //complementamente adentro: cargar todos los hijos directamente, sin testeos
            if (c == TgcCollisionUtils.FrustumResult.INSIDE)
            {
                addAllLeafMeshes(childNode);
            }

            //parte adentro: seguir haciendo testeos con hijos
            else if (c == TgcCollisionUtils.FrustumResult.INTERSECT)
            {
                findVisibleMeshes(frustum, childNode, boxLowerX, boxLowerY, boxLowerZ, boxUpperX, boxUpperY, boxUpperZ);
            }
        }

        /// <summary>
        /// Hacer visibles todas las meshes de un nodo, buscando recursivamente sus hojas
        /// </summary>
        private void addAllLeafMeshes(QuadtreeNode node)
        {
            QuadtreeNode[] children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);
            }
            //pedir hojas a hijos
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    addAllLeafMeshes(children[i]);
                }
            }
        }


        /// <summary>
        /// Hacer visibles todas las meshes de un nodo
        /// </summary>
        private void selectLeafMeshes(QuadtreeNode node)
        {
            TgcSkeletalMesh[] models = node.models;
            foreach (TgcSkeletalMesh m in models)
            {
                m.Enabled = true;
            }
        }






    }
}
