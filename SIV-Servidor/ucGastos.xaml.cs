using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SIV_Servidor
{
    /// <summary>
    /// Interaction logic for ucGastos.xaml
    /// </summary>
    public partial class ucGastos : UserControl
    {
        ///Globales 
        public static ObservableCollection<itemGastos> mItemsGastos =
            new ObservableCollection<itemGastos>();   //Lista los articulos de la caja

        ///MAIN
        public ucGastos()
        {
            InitializeComponent();

            ///FUNCIONES DE INICIO
            resetTextos();
        }

        ///extensiones
        private void ayuda(string texto = "", string texto2 = "")
        {
            MainWindow.ayuda2(texto, texto2);
        }
        private float toFloat(string cadena)
        {
            return zfun.toFloat(cadena);
        }
        private int toInt(string cadena)
        {
            return zfun.toInt(cadena);
        }
        private bool esDecimal(Key key)
        {
            return zfun.esDecimal(key);
        }
        private void consola(string texto)
        {
            zfun.consola(texto);
        }

        ///FUNCIONES VARIAS
        private void resetTextos()
        {
            tbDescripcion.Text = "";
            tbCantidad.Text = "";
            tbMonto.Text = "";
            tbTotal.Text = "";
            tbFiltrar.Text = "";
            tbDescripcion.Focus();
        }
        private void calcularSubtotal()
        {
            tbCantidad.Text = tbCantidad.Text.Replace('.', ',');
            //tbPrecio.Text = tbPrecio.Text.Replace('.', ',');

            //string resultado = "";
            int cantidad = toInt(tbCantidad.Text);
            float monto = toFloat(tbMonto.Text.Replace("$", ""));

            float total = cantidad * monto;
            //resultado = total.ToString();
            //consola(resultado);
            tbTotal.Text = "";
            tbTotal.Text = "$ " + total.ToString("0.00");
            //consola("$ " + total.ToString("0.00"));
        }

        ///CONTROLES
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            string tbName = tb.Name.ToString();
            string tbText = tb.Text.ToString().Trim();


            ///ENTER
            if (e.Key == Key.Enter && tbText != "")
            {
                switch (tbName)
                {
                    case "tbDescripcion":
                        tbCantidad.Text = "1";
                        tbCantidad.SelectAll();
                        tbCantidad.Focus();
                        break;
                    case "tbCantidad":
                        tbMonto.Focus();
                        break;
                    case "tbMonto":
                        asentarGasto();
                        break;

                }
            }

            ///Saltear teclas no numericas
            if (tbName == "tbCantidad" || tbName == "tbMonto")
            {
                if (esDecimal(e.Key) == false)
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        private void tbMonto_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbMonto.Text = tbMonto.Text.Replace('.', ',');
            tbMonto.CaretIndex = tbMonto.Text.Length;   //poner el cursor al final

            calcularSubtotal();
        }

        ///FUNCIONES DB
        private void asentarGasto()
        {
            ///declaracion variables a usar
            string fecha;
            string descripcion;
            int cantidad;
            float monto;
            string montoStr;
            float total;
            string totalStr;

            ///asignacion y pre-asignacion
            fecha = zfun.getFechaNow();
            descripcion = tbDescripcion.Text.ToString().Trim();
            cantidad = toInt(tbCantidad.Text);
            monto = toFloat(tbMonto.Text.Replace("$", ""));
            montoStr = monto.ToString().Replace(",", ".");
            total = toFloat(tbTotal.Text.Replace("$", ""));
            totalStr = total.ToString().Replace(",", ".");

            ///conexion SQL y ejecucion de comando
            string archivo = "gastos.db";
            string tabla = "gastos";
            string parametros;
            parametros = "(fecha, descripcion, cantidad, monto, total) VALUES" +
                   "(" + fecha + ",'" + descripcion + "'," + cantidad + "," + montoStr + "," + totalStr + ")";
            zdb.InsertDB(archivo, tabla, parametros);

            ///fin
            resetTextos();
        }
        private void cargarListaDeClientes()
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=gastos.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            string consulta = "select * from gastos";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///borrar todos los elementos de mTotalArticulos
            if (mItemsGastos!= null)
            {
                mItemsGastos.Clear();
            }

            /////Loop por todos los registros de la tabla
            //foreach (DataRow registro in dataTable.Rows)
            //{

            //    itemGastos tempItem = new itemGastos();
            //    tempItem.id = (long)registro["id"];
            //    tempItem.descripcion= registro["nombre"].ToString();
            //    tempItem.cantidad = registro["direccion"].ToString();
            //    tempItem.monto = registro["telefono"].ToString();
            //    tempItem.total = registro["cuit"].ToString();

            //    mItemsGastos.Add(tempArticulo);
            //}


            /////cerrar conexion
            //conexion.Close();

            /////ordenar lista (coleccion)
            ////mTotalClientes = new ObservableCollection<itemCliente>(mTotalClientes.OrderBy(i => i));

            /////asignar datos al list
            //listFiltroClientes.ItemsSource = mTotalClientes;

            

        }
    }
}
