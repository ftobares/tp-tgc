using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;

namespace TgcViewer.Utils.Render
{
    /// <summary>
    /// Herramienta para administrar todos los elementos que se quieren renderizar
    /// a traves de una cola de renderizado.
    /// Interfaz genérica para implementar la cola de renderizado.
    /// </summary>
    public interface TgcRenderQueue
    {
        /// <summary>
        /// Limpiar toda la cola de renderizado
        /// </summary>
        void clearAll();

        /// <summary>
        /// Agregar un elemento a renderizar a la cola de renderizado
        /// </summary>
        /// <param name="element">Elemento a renderizar</param>
        /// <param name="hasAlpa">True si tiene translucidez</param>
        void add(IRenderQueueElement element, bool hasAlpa);

        /// <summary>
        /// Agregar un elemento opaco a renderizar a la cola de renderizado
        /// </summary>
        /// <param name="element">Elemento opaco a renderizar</param>
        void addOpaque(IRenderQueueElement element);

        /// <summary>
        /// Agregar un elemento con translucidez a renderizar a la cola de renderizado
        /// </summary>
        /// <param name="element">Elemento translucido a renderizar</param>
        void addAlpha(IRenderQueueElement element);

        /// <summary>
        /// Renderizar todos los elementos de la cola de renderizado
        /// </summary>
        void renderAll();


    }

    /// <summary>
    /// Elemento a ser renderizado en una cola de renderizado
    /// </summary>
    public interface IRenderQueueElement
    {
        /// <summary>
        /// Ejecutar el renderizado físico final del elemento
        /// </summary>
        void executeRender();

        /// <summary>
        /// Posicion absoluta del elemento en el universo.
        /// Tiene que indicar de alguna forma donde se encuentra el elemento.
        /// Puede ser el centro del objeto o algún punto relevante del elemento.
        /// </summary>
        Vector3 Position
        {
            get;
        }

    }

}
