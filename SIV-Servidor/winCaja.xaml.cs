using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SIV_Servidor
{

    public partial class winCaja : Window
    {
        ///Variables Globales
        List<itemCaja> mArticulosCaja;

        ///MAIN
        public winCaja()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            mArticulosCaja = new List<itemCaja>();

            ///INICIO
            CargarDBCaja();
        }

        ///FUNCIONES
        private void CargarDBCaja()
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=caja.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            string consulta = "select * from caja ORDER BY id DESC";
            //string consulta = "select * from caja";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];
            //dataGrid.DataSource = dt;
            //dataGrid.DataContext = dt.DefaultView;  //esto anda

            //gridFiltro.ItemsSource = dataTable.DefaultView;
            //listFiltro.ItemsSource = dataTable.DefaultView;

            ///borrar todos los elementos de mArticulos
            if (mArticulosCaja != null)
            {
                mArticulosCaja.Clear();
            }

            ///Loop por todos los registros de la tabla
            int idventaMostrar = -1;
            float tmpTotal = 0;
            int index = 0;
            foreach (DataRow registro in dataTable.Rows)
            {
                //Console.WriteLine(registro.ItemArray.GetValue(0));
                //Console.WriteLine(registro["descripcion"]);
                //Console.WriteLine(registro["id"]+"-"+registro["id"].GetType());

                //string tmpStringPrecio = registro["precio"].ToString();
                //float tmpPrecio = 0;
                //float.TryParse(tmpStringPrecio, out tmpPrecio);

                float cantidad = toFloat(registro["cantidad"].ToString());
                float precio = toFloat(registro["precio"].ToString());
                float tmpSubtotal = cantidad * precio;

                //float costo = toFloat(registro["costo"].ToString());
                //float cantidad = toFloat(registro["cantidad"].ToString());
                consola(registro["precio"].ToString());

                int tmpidventa = 0;
                int.TryParse(registro["idventa"].ToString(), out tmpidventa);


                itemCaja tempArticulo = new itemCaja();
                if (tmpidventa != idventaMostrar)
                {
                    idventaMostrar = tmpidventa;
                    tempArticulo.idventamostrar = idventaMostrar.ToString();
                    tmpTotal = tmpSubtotal;
                }
                else
                {
                    tempArticulo.idventamostrar = "";
                    tmpTotal = tmpTotal + tmpSubtotal;
                }
                if ((tmpidventa % 2) == 0)
                {
                    tempArticulo.color = 0;
                }
                else
                {
                    tempArticulo.color = 1;
                }
                tempArticulo.idventa = tmpidventa;
                tempArticulo.codigo = registro["codigo"].ToString();
                tempArticulo.descripcion = registro["descripcion"].ToString();
                tempArticulo.cantidad = cantidad.ToString();
                tempArticulo.precio = precio.ToString();
                tempArticulo.costo = registro["costo"].ToString();
                tempArticulo.subtotal = tmpSubtotal.ToString();
                tempArticulo.total = "";
                if (dataTable.Rows.Count != index + 1)
                {
                    String tmpSiguienteId = dataTable.Rows[index + 1]["idventa"].ToString();
                    //consola(registro["idventa"].ToString() + "-" + tmpSiguienteId);
                    if (registro["idventa"].ToString() != tmpSiguienteId)
                    {
                        tempArticulo.total = tmpTotal.ToString();
                    }
                }
                else
                {
                    tempArticulo.total = tmpTotal.ToString();
                }
                mArticulosCaja.Add(tempArticulo);
                /*
				mArticulos.Add(new articuloClass() {
					id = (long)registro["id"],
					codigopro = registro["codigopro"].ToString(),
					descripcion = registro["descripcion"].ToString(),
					precio = tmpPrecio,
				});
				*/
                index++;
            }

            //popular lista mArticulos con los datos de todos los registros (se puede??)
            //List<articuloClass> tempArticulos = new List<articuloClass>();
            //tempArticulos.AddRange(dataTable.Rows);


            //cerrar conexion
            //gridFiltro.ItemsSource = mArticulos;
            //mArticulosCaja.Reverse();
            listCaja.ItemsSource = mArticulosCaja;

            /*
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listCaja.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("idventa");
            view.GroupDescriptions.Add(groupDescription);
            */

            conexion.Close();
        }
        private float toFloat(string cadena)
        {
            float resultado = 0;
            float.TryParse(cadena, out resultado);
            return resultado;
        }
        private void consola(string texto)
        {
            Console.WriteLine(texto);
            //LTemp.Content = texto;
        }
    }
}
