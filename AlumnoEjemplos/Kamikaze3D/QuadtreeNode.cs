using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace AlumnoEjemplos.Kamikaze3D
{
    /// <summary>
    /// Nodo del árbol Quadtree
    /// </summary>
    class QuadtreeNode
    {
        public QuadtreeNode[] children;
        public TgcSkeletalMesh[] models;

        public bool isLeaf()
        {
            return children == null;
        }
    }
}
