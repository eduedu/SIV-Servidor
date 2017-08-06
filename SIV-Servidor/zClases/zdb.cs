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
using System.Threading;
using System.Windows.Threading;
using System.Windows;

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

        public static void grabarConfig(string parametro, string valor)
        {
            /// Trabaja con DATOS.DB; guarda "valor" en el registro cuyo campo parametro sea igual a "parametro"

            ///definir variables
            string archivoDB = "datos.db";
            string tabla = "datos";
            string campo = "valor";
            string valorAnterior = "";
            string comando = "";

            ///comprobar si ya existia el campo
            valorAnterior = leerConfig(parametro);
            if (valorAnterior == "registroInexistente")
            {
                ///crear nuevo registro
                string parametrosInsert = "(parametro, valor) VALUES ('" + parametro + "','" + valor + "')";
                InsertDB(archivoDB, tabla, parametrosInsert);

            }
            else
            {
                ///modificar el valor del registro
                comando = "UPDATE  " + tabla + " SET " + campo + "='" + valor + "' WHERE parametro='" + parametro + "'";
                //new Thread(new ThreadStart(delegate
                //{
                    ejecutarComandoSql(archivoDB, comando);
                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    //}), DispatcherPriority.Normal, null);
                //})).Start();

            }

        }

        public static string leerConfig(string parametro)
        {
            /// Trabaja con DATOS.DB: Devuelve el valor del parametro guardado con anterioridad. Si no existe, devuelve la cadena "registroInexistente".

            ///definir parametros y variables
            string valor = "registroInexistente";
            string archivoDB = "datos.db";
            string tabla = "datos";

            ///abrir conexion DB
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivoDB + ";Version=3;New=False;Compress=True;");
            conexion.Open();

            ///realizar CONSULTA
            //string consulta = "select *, MAX(id) from articulos";
            //string consulta = "select MAX(" + campo + ") from " + tabla;
            string consulta = "SELECT * FROM " + tabla + " WHERE parametro='" + parametro + "'";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///cierro base de datos
            conexion.Close();

            ///// retornar valor maximo de idventa (error=-1)
            //int resultado = -1;
            //Int32.TryParse(dataTable.Rows[0].ItemArray.GetValue(0).ToString(), out resultado);

            if (dataTable.Rows.Count > 0)
            {
                valor = dataTable.Rows[0].ItemArray.GetValue(2).ToString();
            }

            return valor;
        }

        public static void InsertDB(string archivoDB, string tabla, string parametrosInsert)
        {
            ///parametros
            //FORMATO DE PARAMETROS INSERT: " (nombre, direccion, telefono, cuit) VALUES (?,?,?,?)"

            ///abrir conexion DB
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivoDB + ";Version=3;New=False;Compress=True;");
            conexion.Open();

            ///comando SQL a ejecutar
            SQLiteCommand insertSQL;
            //insertSQL = new SQLiteCommand("INSERT INTO " + tabla + " (nombre, direccion, telefono, cuit) VALUES (?,?,?,?)", conexion);
            insertSQL = new SQLiteCommand("INSERT INTO " + tabla + " " + parametrosInsert, conexion);


            ///ejecutar comando SQL
            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            ///Cerrar conexion
            conexion.Close();
        }

        ///entradas y salidas
        //public static void sumarAtotalEntradasBD(string monto)
        //{
        //    editarEntradasSalidas(monto, true);
        //}
        //public static void sumarAtotalSalidasBD(string monto)
        //{
        //    editarEntradasSalidas(monto, false);
        //}
        //public static void editarEntradasSalidas(string monto, bool entradas)
        //{
        //    string nombreValor = "totalEntradas";
        //    ///al final decidi usar el mismo registro en datos.db para alojar tanto entradas, como salidas
        //    //if (entradas)
        //    //{
        //    //    nombreValor = "totalEntradas";
        //    //} else
        //    //{
        //    //    nombreValor = "totalSalidas";
        //    //}

        //    monto = monto.Replace("$", "").Trim();
        //    float fMonto = zfun.toFloat(monto);
        //    float montoAnterior = zfun.toFloat(zdb.leerConfig(nombreValor));
        //    float nuevoMonto = montoAnterior + fMonto;
        //    zdb.grabarConfig(nombreValor, nuevoMonto.ToString());
        //}
        public static void balanceCajaDB(string monto)
        {
            string nombreValor = "balanceCaja";

            monto = monto.Replace("$", "").Trim();
            float fMonto = zfun.toFloat(monto);
            float montoAnterior = zfun.toFloat(zdb.leerConfig(nombreValor));
            float nuevoMonto = montoAnterior + fMonto;
            zdb.grabarConfig(nombreValor, nuevoMonto.ToString());
        }
    }
}
