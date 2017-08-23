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


            ///INICIO Y ASIGNACIONES
            resetTextos();
            listGastos.ItemsSource = mItemsGastos;
            cargarListaGastos();
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
            tbMonto.Text = "";
            tbFiltrar.Text = "";
            tbDescripcion.Focus();
        }

        ///FUNCIONES DB
        private void asentarGasto()
        {
            ///declaracion variables a usar
            string fecha;
            string descripcion;
            float monto;
            string montoStr;

            ///asignacion y pre-asignacion
            fecha = zfun.getFechaNow();
            descripcion = tbDescripcion.Text.ToString().Trim();
            monto = toFloat(tbMonto.Text.Replace("$", ""));
            montoStr = monto.ToString().Replace(",", ".");

            ///conexion SQL y ejecucion de comando
            string archivo = "gastos.db";
            string tabla = "gastos";
            string parametros;
            parametros = "(fecha, descripcion, monto) VALUES" +
                   "(" + fecha + ",'" + descripcion + "'," + montoStr + ")";
            zdb.InsertDB(archivo, tabla, parametros);

            ///agregar al registro de 'balanceCaja' en datos.db
            //string monto = tbTotal.Text.ToString().Replace("$", "").Trim();
            zdb.balanceCajaDB(montoStr, true);
            ///actualizar balance
            MainWindow.statucInicio.calcularTotalBalance();


            ///fin
            cargarListaGastos();
            resetTextos();
        }
        private void cargarListaGastos()
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=gastos.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            string consulta = "select id, datetime(fecha), descripcion, monto from gastos";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///borrar todos los elementos de mTotalArticulos
            if (mItemsGastos != null)
            {
                mItemsGastos.Clear();
            }

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {

                itemGastos tempItem = new itemGastos();
                tempItem.id = (long)registro["id"];
                tempItem.fecha = zfun.toFechaMostrar(registro[1].ToString());
                tempItem.descripcion = registro["descripcion"].ToString();
                tempItem.monto = zfun.toFloat(registro["monto"].ToString());

                mItemsGastos.Add(tempItem);
            }


            ///cerrar conexion
            conexion.Close();

            ///ordenar lista (coleccion)
            mItemsGastos = new ObservableCollection<itemGastos>(mItemsGastos.OrderByDescending(i => i.fecha));
            listGastos.ItemsSource = mItemsGastos;
            //mItemsGastos.Reverse();

            ///asignar datos al list
            //listGastos.ItemsSource = mItemsGastos;



        }

        ///CONTROLES
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            string tbName = tb.Name.ToString();

            if (tbName == "tbMonto")
            {
                ayuda(zAyuda.gastos_tbMonto);
            }
            if (tbName == "tbDescripcion")
            {
                ayuda(zAyuda.gastos_tbDescripcion);
            }
            if (tbName == "tbFiltrar")
            {
                ayuda(zAyuda.gastos_tbFiltrar, zAyuda.gastos_tbFiltrar2);
            }
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            string tbName = tb.Name.ToString();
            string tbText = tb.Text.ToString().Trim();


            ///ENTER
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                switch (tbName)
                {
                    case "tbDescripcion":
                        if (tbText != "")
                        {
                            tbMonto.Focus();
                        }
                        else
                        {
                            tbFiltrar.Focus();
                        }
                        break;

                    case "tbMonto":
                        if (tbText != "")
                        {
                            asentarGasto();
                        }
                        break;

                    case "tbFiltrar":
                        if (listGastos.Items.Count > -1)
                        {
                            listGastos.SelectedIndex = 0;
                            var item = listGastos.ItemContainerGenerator.ContainerFromIndex(listGastos.SelectedIndex) as ListBoxItem;
                            if (item != null)
                            {
                                item.Focus();
                            }
                        }
                        break;

                }
            }

            ///ESCAPE
            if (e.Key == Key.Escape)
            {
                switch (tbName)
                {
                    case "tbDescripcion":
                        resetTextos();
                        break;
                    case "tbMonto":
                        resetTextos();
                        break;
                    case "tbFiltrar":
                        resetTextos();
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

        }
        private void tbFiltrar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = tbFiltrar.Text.ToString().Trim();
            var itemsFiltrados = from registro in mItemsGastos
                                 where registro.descripcion.ToLower().Contains(filtro.ToLower())
                                 orderby registro.fecha descending
                                 select registro;

            listGastos.ItemsSource = itemsFiltrados;
        }

        private void listGastos_GotFocus(object sender, RoutedEventArgs e)
        {
            ayuda(zAyuda.gastos_listGastos);
        }
        private void listGastos_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var list = sender as ListView;
            var item = list.SelectedItem as itemGastos;
            if (e.Key == Key.Escape)
            {
                tbDescripcion.Focus();
            }
            if (e.Key == Key.Delete)
            {
                string descripcion = item.descripcion;
                float monto = item.monto;
                string montoStr = monto.ToString();

                ///preguntar si eliminar el item
                bool eliminar = false;
                string mensaje = "¿Eliminar '" + descripcion + "' ($ " + montoStr + ")?";
                MessageBoxResult result = MessageBox.Show(mensaje, "Eliminar Registro", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        eliminar = true;
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                }

                if (eliminar)
                {
                    ///eliminar item de gastos.db
                    string archivo = "gastos.db";
                    string tabla = "gastos";
                    long id = item.id;
                    zdb.EliminarRegistroDB(archivo, tabla, id);

                    ///sumar a balanceCaja
                    zdb.balanceCajaDB(montoStr);
                    ///actualizar balance
                    MainWindow.statucInicio.calcularTotalBalance();

                    ///actualizar vista
                    cargarListaGastos();
                    tbDescripcion.Focus();
                    //consola("Borrar: " + item.id.ToString() + "-" + item.descripcion);
                }
            }
        }

    }
}
