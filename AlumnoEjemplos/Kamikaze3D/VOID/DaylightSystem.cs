using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Render;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.VOID
{

    public static class DaylightSystem
    {
        //Inicializador
        public static void Inicializar()
        {
            //Instancia los objetos
            ambientColor = new Color();
            diffuseColor = new Color();
            dirtyValues = false;
            actualDaylight = "Madrugada";
            changeNow = false;
        }

        //Color de la iluminacion segun la hora del dia
        public static Color ambientColor;
        public static Color diffuseColor;

        //Variable que permite dejar un ciclo con el dirtyValues en true;
        private static bool changeNow;

        //Daylight actual
        public static string actualDaylight;

        //Bandera que indica si hubo una actualizacion
        public static bool dirtyValues;

        //Revisa si se cambio el modifier y actualiza
        public static void checkModifier()
        {
            string valorModifier = (string)GuiController.Instance.Modifiers["Daylight"];

            //Si no coincide con el actual
            if (valorModifier != actualDaylight)
            {
                //Setea que hubo cambio de variables
                dirtyValues = true;

                if (!changeNow)
                {
                    changeNow = true;

                }
                else
                {
                    dirtyValues = false;
                    changeNow = false;
                    actualDaylight = valorModifier;
                    return;
                }

                if (valorModifier == "Noche")
                {
                    ambientColor = Color.Black;
                    diffuseColor = Color.FromArgb(160, 160, 88);
                }
                else

                    if (valorModifier == "Madrugada")
                    {
                        ambientColor = Color.FromArgb(20, 20, 20);
                        diffuseColor = Color.FromArgb(225, 225, 130);
                    }

                    else

                        if (valorModifier == "Dia")
                        {
                            ambientColor = Color.Black;
                            diffuseColor = Color.White;
                        }
            }
        }
    }

}