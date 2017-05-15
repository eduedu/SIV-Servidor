using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SIV_Servidor
{
    class funciones
    {
        //private static int i=0;
        public static void consola(string texto)
        {
            Console.WriteLine(texto);
            //labelAyuda.Content = texto;
        }
        public static float toFloat(string cadena)
        {
            //i++;
            //consola(i.ToString());
            float resultado = 0;
            float.TryParse(cadena, out resultado);
            return resultado;
        }
        public static bool esDecimal(Key key)
        {
            bool respuesta = false;

            if ((key >= Key.D0) && (key <= Key.D9))
            {
                respuesta = true;
            }
            if ((key >= Key.NumPad0) && (key <= Key.NumPad9))
            {
                respuesta = true;
            }
            if ((key == Key.OemPeriod) || (key == Key.Decimal) || (key == Key.OemComma))
            {
                respuesta = true;
            }

            if ((key == Key.Up) || (key == Key.Down) || (key == Key.Right) || (key == Key.Left))
            {
                respuesta = true;
            }
            if ((key == Key.Delete) || (key == Key.Back))
            {
                respuesta = true;
            }

            return respuesta;
        }
        public static string toMoneda(string cadena)
        {
            string resultado = "";

            resultado = cadena.Replace(".", "");
            resultado = cadena.Replace(",", ".");
            //consola(cadena);
            //consola(resultado);
            return resultado;
        }
    }
}
