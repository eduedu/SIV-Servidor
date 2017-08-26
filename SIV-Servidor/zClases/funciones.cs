using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SIV_Servidor
{
    class zfun
    {
        //private static int i=0;
        public static void consola(string texto)
        {
            Console.WriteLine(texto);
            //labelAyuda.Content = texto;
        }
        public static int toInt(string cadena)
        {
            //i++;
            //consola(i.ToString());
            int resultado = 0;
            int.TryParse(cadena, out resultado);
            return resultado;
        }
        public static double toDouble(string cadena)
        {
            //i++;
            //consola(i.ToString());
            double resultado = 0;
            cadena.Replace(",", ".");
            double.TryParse(cadena, out resultado);
            return resultado;
        }

        public static long toLong(string cadena)
        {
            //i++;
            //consola(i.ToString());
            long resultado = -1;
            long.TryParse(cadena, out resultado);
            return resultado;
        }
        public static float toFloat(string cadena)
        {
            //i++;
            //consola(i.ToString());
            float resultado = -1;
            cadena.Replace(",", ".");
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
        public static string toFechaMostrar(string registro)
        {
            DateTime fecha = DateTime.ParseExact(registro, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string resultado = fecha.ToString("dd/MM/yy HH:mm");

            return resultado;
        }
        
        public static string getFechaNow()
        {
            //DateTime fecha = DateTime.ParseExact(registro, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            //string resultado = fecha.ToString("dd/MM/yy HH:mm");
            DateTime nowGMT = DateTime.Now;
            TimeSpan menos3horas = new TimeSpan(0, 0, 0, 0);
            DateTime nowMenos3horas = nowGMT.Subtract(menos3horas);
            //DateTime fecha = DateTime.ParseExact(registro, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string now = nowMenos3horas.ToString("yyyy-MM-dd HH:mm:ss");

            string resultado="DATETIME('"+ now + "')";
            return resultado;
        }
    }
}
