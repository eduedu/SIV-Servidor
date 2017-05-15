using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data.SQLite; //Utilizamos la DLL
using System.Data;
using System.Globalization;
using System.Windows.Media.Animation;

namespace SIV_Servidor
{
    class zdb
    {
        public static int valorMaxDB(string archivoDB, string tabla, string campo)
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivoDB + ";Version=3;New=False;Compress=True;");
            conexion.Open();

            //string consulta = "select *, MAX(id) from articulos";
            string consulta = "select MAX(" + campo + ") from " + tabla;

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///cierro base de datos
            conexion.Close();

            /// retornar valor maximo de idventa (error=-1)
            int resultado = -1;
            Int32.TryParse(dataTable.Rows[0].ItemArray.GetValue(0).ToString(), out resultado);


            return resultado;
        }

        public static void EliminarRegistroDB(string archivoDB, string tabla, long index)
        {
            /// elimina el registro con id="index" de la "tabla" en el archivo "archivoDB"

            string comando = "DELETE FROM " + tabla + " WHERE id=" + index;
            ejecutarComandoSql(archivoDB, comando);

        }
        public static void modificarRegistroDB(string archivoDB, string tabla, string index, string campo, string valor = "", bool CampoEsNumerico = false)
        {
            /// MODIFICA el registro con id="index" de la "tabla" en el archivo "archivoDB", campo=valor.
            string comando = "";
            // UPDATE COMPANY SET ADDRESS = 'Texas' WHERE ID = 6;
            // UPDATE COMPANY SET ADDRESS = 'Texas', SALARY = 20000.00;
            if (CampoEsNumerico)
            {
                comando = "UPDATE  " + tabla + " SET " + campo + "=" + valor + " WHERE id=" + index.ToString();
            }
            else
            {
                comando = "UPDATE  " + tabla + " SET " + campo + "='" + valor + "' WHERE id=" + index.ToString();
            }

            ejecutarComandoSql(archivoDB, comando);
        }
        public static void ejecutarComandoSql(string archivoDb, string comando)
        {
            ///EJECUTA -comando- en la base de datos -archivoDB-


            ///abrir conexion DB
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivoDb + ";Version=3;New=False;Compress=True;");
            conexion.Open();

            
            ///definir comando
            SQLiteCommand comandoSQL;
            //comandoSQL = new SQLiteCommand(comando + " " + tabla + " " + parametros, conexion);
            comandoSQL = new SQLiteCommand(comando, conexion);


            ///ejecutar comando
            try
            {
                comandoSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            ///Cerrar conexion
            conexion.Close();

        }

    }
}
