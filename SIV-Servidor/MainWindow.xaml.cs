using System;
using System.Collections.Generic;
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

using Bespoke.Common;
using Bespoke.Common.Osc;
using System.Windows.Threading;
using System.Net;

//using System;
using System.IO;
using System.Data.SQLite; //Utilizamos la DLL
using System.Data;
using System.Globalization;

namespace SIV_Servidor
{
    public partial class MainWindow : Window
    {
        ///Variables Globales
        List<articuloClass> mArticulos;
        int mPestana;

        //static SQLiteConnection conexion;

        ///MAIN
        public MainWindow()
        {
            InitializeComponent();
            mArticulos = new List<articuloClass>();

            ///SETEAR CONTROLES
            LTemp.Content = "";
            gridFiltro.Visibility = Visibility.Hidden;
            listFiltro.Visibility = Visibility.Hidden;
            listFiltro.Margin = new Thickness(tbDescripcion.Margin.Left, tbDescripcion.Margin.Top + tbDescripcion.Height + 2, 0, 0);
            //tbBuscar.Focus();


            ///FUNCIONES DE INICIO
            //iniciarOSC();
            //gridFiltroSQL();
            seleccionarPestana(0);
            cargarListaDeArticulos();
            calcularTotal();
            tbDescripcion.Focus();
        }
        ///---------------------------------------------------------------------------

        ///OSC 
        public void iniciarOSC()
        {
            //Console.WriteLine("hola");

            OscServer oscServer;

            //DemoType demoType = GetDemoType();
            DemoType demoType = DemoType.Udp;
            IPAddress miIp;
            //miIp=IPAddress.Loopback;
            //miIp= IP DEL SERVER:
            miIp = IPAddress.Parse("192.168.0.9");

            oscServer = new OscServer(TransportType.Udp, miIp, MiOSC.Port);
            //oscServer = new OscServer(miIp, Port);
            //oscServer = new OscServer(IPAddress.Parse("224.25.26.27"), Port);

            /*
			switch (demoType)
			{
					case DemoType.Udp:
							oscServer = new OscServer(TransportType.Udp, miIp, Port);
							break;

					case DemoType.Tcp:
							oscServer = new OscServer(TransportType.Tcp, IPAddress.Loopback, Port);
							break;

					case DemoType.Multicast:
							oscServer = new OscServer(IPAddress.Parse("224.25.26.27"), Port);
							break;

					default:
							throw new Exception("Unsupported receiver type.");
			}
			*/
            //Console.WriteLine("IP ADDRESSSSSSSSSSSSSS:" + IPAddress.Loopback);

            oscServer.FilterRegisteredMethods = false;
            oscServer.RegisterMethod(MiOSC.AliveMethod);
            oscServer.RegisterMethod(MiOSC.TestMethod);
            oscServer.BundleReceived += new EventHandler<OscBundleReceivedEventArgs>(MiOSC.oscServer_BundleReceived);
            //oscServer.MessageReceived += new EventHandler<OscMessageReceivedEventArgs>(MiOSC.oscServer_MessageReceived);
            oscServer.MessageReceived += new EventHandler<OscMessageReceivedEventArgs>(NuevoMensajeOSC);
            oscServer.ReceiveErrored += new EventHandler<Bespoke.Common.ExceptionEventArgs>(MiOSC.oscServer_ReceiveErrored);
            oscServer.ConsumeParsingExceptions = false;

            oscServer.Start();


            Console.WriteLine("Osc Receiver: " + demoType.ToString());
            Console.WriteLine("Press any key to exit.");
            //Console.ReadKey();
            //oscServer.Stop();
        }
        public void NuevoMensajeOSC(object sender, OscMessageReceivedEventArgs e)
        {
            MiOSC.sMessagesReceivedCount++;

            OscMessage message = e.Message;

            Console.WriteLine(string.Format("\nMessage Received [{0}]: {1}", message.SourceEndPoint.Address, message.Address));
            Console.WriteLine(string.Format("Message contains {0} objects.", message.Data.Count));

            //ACCION LAYOUT
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Label1.Content = "";
                //txtUrlCompartirTitulo.Text = mCambioUrlCompartirTitulo;
            }), DispatcherPriority.Normal, null);


            for (int i = 0; i < message.Data.Count; i++)
            {
                string dataString;

                if (message.Data[i] == null)
                {
                    dataString = "Nil";
                }
                else
                {
                    dataString = (message.Data[i] is byte[] ? BitConverter.ToString((byte[])message.Data[i]) : message.Data[i].ToString());
                }
                Console.WriteLine(string.Format("[{0}]: {1}", i, dataString));

                //ACCION LAYOUT
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Label1.Content = Label1.Content + "Mensaje:" + dataString;
                }), DispatcherPriority.Normal, null);

            }

            Console.WriteLine("Total Messages Received: {0}", MiOSC.sMessagesReceivedCount);
        }

        ///FUNCIONES BD
        private void gridFiltroSQL(string filtro = "")
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=lista.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            // Lanzamos la consulta y preparamos la estructura para leer datos
            string consulta = "select * from lista where descripcion like '%" + filtro + "%' ";

            // Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);

            DataSet ds = new DataSet();
            ds.Reset();

            DataTable dt = new DataTable();
            db.Fill(ds);

            //Asigna al DataTable la primer tabla (ciudades) 
            // y la mostramos en el DataGridView
            dt = ds.Tables[0];

            //dataGrid.DataSource = dt;
            //dataGrid.DataContext = dt.DefaultView;  //esto anda
            gridFiltro.ItemsSource = dt.DefaultView;
            listFiltro.ItemsSource = dt.DefaultView;

            // Y ya podemos cerrar la conexion
            conexion.Close();


        }
        private void cargarListaDeArticulos()
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=articulos.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            string consulta = "select * from articulos";

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
            if (mArticulos != null)
            {
                mArticulos.Clear();
            }

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {
                //Console.WriteLine(registro.ItemArray.GetValue(0));
                //Console.WriteLine(registro["descripcion"]);
                //Console.WriteLine(registro["id"]+"-"+registro["id"].GetType());

                //string tmpStringPrecio = registro["precio"].ToString();
                //float tmpPrecio = 0;
                //float.TryParse(tmpStringPrecio, out tmpPrecio);

                float precio = toFloat(registro["precio"].ToString());
                float costo = toFloat(registro["costo"].ToString());

                articuloClass tempArticulo = new articuloClass();
                tempArticulo.id = (long)registro["id"];
                tempArticulo.codigo = registro["codigo"].ToString();
                tempArticulo.codigopro = registro["codigopro"].ToString();
                tempArticulo.descripcion = registro["descripcion"].ToString();
                tempArticulo.precio = precio;
                tempArticulo.costo = costo;

                mArticulos.Add(tempArticulo);
                /*
				mArticulos.Add(new articuloClass() {
					id = (long)registro["id"],
					codigopro = registro["codigopro"].ToString(),
					descripcion = registro["descripcion"].ToString(),
					precio = tmpPrecio,
				});
				*/
            }

            //popular lista mArticulos con los datos de todos los registros (se puede??)
            //List<articuloClass> tempArticulos = new List<articuloClass>();
            //tempArticulos.AddRange(dataTable.Rows);


            //cerrar conexion
            gridFiltro.ItemsSource = mArticulos;
            listFiltro.ItemsSource = mArticulos;
            conexion.Close();

        }
        private void filtroArticulos(string filtro = "")
        {
            var listaTotal = from registro in mArticulos
                             where registro.descripcion.ToLower().Contains(filtro.ToLower())
                             select registro;
            listFiltro.ItemsSource = listaTotal;
            /*
			foreach (var reg in listaTotal) {
				Console.WriteLine("Id = {0} , Descripcion = {1}",
													reg.id, reg.descripcion);
			}
			*/
        }
        private int obtenerIdVentaMax()
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=caja.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            //string consulta = "select *, MAX(id) from articulos";
            string consulta = "select MAX(idventa) from caja";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            /// retornar valor maximo de idventa
            int resultado = -1;
            Int32.TryParse(dataTable.Rows[0].ItemArray.GetValue(0).ToString(), out resultado);

            conexion.Close();
            return resultado;
        }
        private void asentarVenta()
        {
            ///ejecutar si la lista no está vacía
            if (listVenta.Items.Count > 0)
            {
                ///obtener idVentaMax
                int idVentaMax = obtenerIdVentaMax() + 1;
                //consola(idVentaMax.ToString());

                ///abrir conexion DB
                SQLiteConnection conexion;
                conexion = new SQLiteConnection("Data Source=caja.db;Version=3;New=False;Compress=True;");
                conexion.Open();

                ///recorrer la lista (listVenta)
                foreach (itemVenta item in listVenta.Items)
                {
                    asentarItem(item, idVentaMax, conexion);
                }

                ///Cerrar conexion
                conexion.Close();
            }
            else
            {
                consola("No hay articulos en la lista.");
            }

            tbDescripcion.Focus();
        }
        private void asentarItem(itemVenta item, int idMax, SQLiteConnection conexion)
        {
            SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO caja (idventa, fecha, codigo, descripcion, cantidad, precio, costo) VALUES (?,DATETIME('NOW'),?,?,?,?,?)", conexion);
            /*insertSQL.Parameters.Add(idMax.ToString());
            insertSQL.Parameters.Add("1");
            insertSQL.Parameters.Add(item.codigo);
            insertSQL.Parameters.Add(item.descripcion);
            insertSQL.Parameters.Add(item.cantidad);
            insertSQL.Parameters.Add(item.precio);
            */
            insertSQL.Parameters.AddWithValue("idventa", idMax.ToString());
            //insertSQL.Parameters.AddWithValue("fecha", "DATETIME('NOW')");
            insertSQL.Parameters.AddWithValue("codigo", item.codigo);
            insertSQL.Parameters.AddWithValue("descripcion", item.descripcion);
            insertSQL.Parameters.AddWithValue("cantidad", item.cantidad);
            insertSQL.Parameters.AddWithValue("precio", item.precio);
            insertSQL.Parameters.AddWithValue("costo", item.costo);
            try
            {
                insertSQL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        ///FUNCIONES GENERALES
        private void seleccionarPestana(int pestana)
        {
            mPestana = pestana;

            //LTemp.Content = mPestana;
        }
        private void resetTb()
        {
            tbCodigo.Text = "";
            tbDescripcion.Text = "";
            tbCantidad.Text = "";
            tbPrecio.Text = "";
            tbSubtotal.Text = "";
        }
        private void consola(string texto)
        {
            Console.WriteLine(texto);
            LTemp.Content = texto;
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
        private void calcularSubtotal()
        {

            tbCantidad.Text = tbCantidad.Text.Replace('.', ',');
            tbPrecio.Text = tbPrecio.Text.Replace('.', ',');

            string resultado = "";
            float precio = 0;
            float cantidad = 0;
            float subtotal = 0;
            //NumberStyles style;
            //NumberFormatInfo culture;
            //culture = CultureInfo.InvariantCulture.NumberFormat;
            //style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
            float.TryParse(tbCantidad.Text, out cantidad);
            float.TryParse(tbPrecio.Text, out precio);

            subtotal = cantidad * precio;
            resultado = subtotal.ToString();
            //consola(resultado);

            //tbSubtotal.Text = resultado;
            tbSubtotal.Text = subtotal.ToString("0.00");
        }
        private float toFloat(string cadena)
        {
            float resultado = 0;
            float.TryParse(cadena, out resultado);
            return resultado;
        }
        private void calcularTotal()
        {
            float total = 0;
            float subtotal = 0;
            if (listVenta.Items.Count > 0)
            {
                foreach (itemVenta item in listVenta.Items)
                {
                    //consola(item.descripcion);
                    subtotal = 0;
                    float.TryParse(item.subtotal, out subtotal);
                    total = total + subtotal;
                }
                tbTotal.Text = "$ " + total.ToString("0.00");
            }
            else
            {
                tbTotal.Text = "$ 0.00";
            }
        }
        private void agregarItemALista()
        {
            string codigo = "";
            string descripcion = "";
            string cantidad = "";
            string precio = "";
            string subtotal = "";
            string costo = "";

            codigo = tbCodigo.Text;
            descripcion = tbDescripcion.Text;
            cantidad = tbCantidad.Text;
            precio = tbPrecio.Text;
            subtotal = tbSubtotal.Text;
            costo = tbPrecio.Tag.ToString();

            ///agregar item al listbox
            //float tmpCantidad = 0;
            //float.TryParse(cantidad, out tmpCantidad);
            //float tmpPrecio = 0;
            //float.TryParse(precio, out tmpPrecio);
            listVenta.Items.Add(new itemVenta
            {
                codigo = codigo,
                descripcion = descripcion,
                cantidad = cantidad,
                precio = precio,
                subtotal = subtotal,
                costo = costo
            });

            ///agregar item a la tabla temporal



            //string resultado = "";
            //resultado = "Codigo:" + codigo + "-Descripcion:" + descripcion + "-Cantidad:" + cantidad + "-Precio:" + precio + "-Subtotal:" + subtotal;
            //LTemp.Content = resultado;
            //resetTb();
            //tbBuscar.Focus();
            tbDescripcion.Focus();


            ///Calcular total
            calcularTotal();
        }

        ///CONTROLES
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                var selected = grid.SelectedItem;
                // ... Set Title to selected names.
                //this.Title = string.Join(", ", names);

                /*var item = selected as usuarioClass;
				if (item != null) {
					//labelid.Content = item.id.ToString();
					lNombre.Content = item.nya.ToString();
					string tmpN = item.num.ToString();
					if (tmpN.Length > 4) {
						tmpN = tmpN.Substring(0, 3) + "-" + tmpN.Substring(3, tmpN.Length - 3);
					}
					lNumero.Content = tmpN;
				}
				*/
                string tmpTexto = "";
                //Console.WriteLine(selected);
                //consola(selected.ToString());
                if (selected.ToString() == "System.Data.DataRowView")
                {
                    DataRowView drv = (DataRowView)grid.SelectedItem;

                    String valor = drv[3].ToString();

                    tmpTexto = valor;
                    //int columna = grid.CurrentColumn.DisplayIndex;
                    //valor = columna.ToString();
                    tmpTexto = valor;
                    LTemp.Content = tmpTexto;
                }
            }
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 120));
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            if (textBox.Name == "tbDescripcion")
            {

            }
        }
        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            tbDescripcion.Focus();
        }

        private void Button_GotFocus(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            boton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 120));
        }
        private void Button_LostFocus(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            boton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        }


        private void btnAsentar_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //consola(e.Key.ToString());
            if (e.Key == Key.Left)
            {
                //tbBuscar.Focus();
            }
            if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Up)
            {
                e.Handled = true;
            }
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                asentarVenta();
            }
        }
        private void btnAsentar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            asentarVenta();
        }

        private void listFiltro_KeyDown(object sender, KeyEventArgs e)
        {
            //consola(e.Key.ToString());
            if (e.Key == Key.Escape)
            {
                //Console.WriteLine("ESCAPE");
                //gridFiltro.SelectedItem = 0;
                //gridFiltro.Focus();
                //tbBuscar.MoveFocus(gridFiltro);
                //tbBuscar.Focus();
                tbDescripcion.Focus();
            }
            if (e.Key == Key.Enter)
            {
                //Console.WriteLine("ENTER");
                var list = sender as ListView;
                /*
				DataRowView fila = (DataRowView)list.SelectedItem;
				String codigo = fila["codigo"].ToString();
				String descripcion = fila["descripcion"].ToString();
				String precio = fila["precio"].ToString();
				*/

                var selected = list.SelectedItem;
                var fila = selected as articuloClass;

                string codigopro = ((fila.codigo == "" || fila.codigo == null) ? "" : fila.codigo.ToString());
                string descripcion = fila.descripcion.ToString();
                string precio = fila.precio.ToString("0.00");

                tbCodigo.Text = codigopro;
                tbDescripcion.Text = descripcion;
                tbPrecio.Text = precio;
                //consola("costo:" + fila.costo.ToString());
                tbPrecio.Tag = fila.costo;
                //consola("tbPrecio.Tag:"+tbPrecio.Tag.ToString());

                //tbBuscar.Text = "";
                //tbDescripcion.Text = "";
                //tbBuscar.Focus();
                tbCantidad.Text = "1";
                tbCantidad.SelectAll();
                tbCantidad.Focus();
                listFiltro.Visibility = Visibility.Hidden;
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = sender as TabControl;
            int selected = tab.SelectedIndex;
            seleccionarPestana(selected);
            //consola(selected.ToString());
            //tbDescripcion.Focus();
        }

        private void tbDescripcion_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string filtro = textBox.Text.Trim();
            //consola(listFiltro.Items.Count.ToString());
            if (filtro != "" && textBox.IsFocused)
            {
                ///aplicar filtro
                //gridFiltroSQL(filtro);
                filtroArticulos(filtro);

                if (listFiltro.Items.Count > 0)
                {
                    listFiltro.Visibility = Visibility.Visible;
                }
                else
                {
                    ///ocultar control si no hay resultados
                    listFiltro.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                ///ocultar control si el text esta vacio
                listFiltro.Visibility = Visibility.Hidden;
            }
        }
        private void tbDescripcion_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                listFiltro.Focus();
            }
            if (e.Key == Key.Right)
            {
                //btnNuevo.Focus();
                //e.Handled = true;

            }
            if (e.Key == Key.Escape)
            {
                tbDescripcion.Text = "";
                resetTb();
            }
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (tbDescripcion.Text.Trim() == "")
                {
                    if (listVenta.Items.Count > 0)
                    {
                        tbPagaCon.Focus();
                    }
                }
                else
                {
                    tbCantidad.Text = "1";
                    tbCantidad.SelectAll();
                    tbCantidad.Focus();
                    listFiltro.Visibility = Visibility.Hidden;
                }
            }
        }
        private void tbCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            calcularSubtotal();
        }
        private void tbCantidad_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (esDecimal(e.Key) == false)
            {
                e.Handled = true;
            }

            if (e.Key == Key.Left)
            {
                tbDescripcion.Focus();
            }
            if (e.Key == Key.Right || e.Key == Key.Enter)
            {
                tbPrecio.SelectAll();
                tbPrecio.Focus();
            }
            if (e.Key == Key.Up || e.Key == Key.Escape)
            {
                resetTb();
                //tbBuscar.Focus();
                tbDescripcion.Focus();
            }

        }
        private void tbPrecio_TextChanged(object sender, TextChangedEventArgs e)
        {
            calcularSubtotal();

        }
        private void tbPrecio_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (esDecimal(e.Key) == false)
            {
                e.Handled = true;
            }

            if (e.Key == Key.Left)
            {
                tbCantidad.SelectAll();
                tbCantidad.Focus();
            }
            if (e.Key == Key.Up || e.Key == Key.Escape)
            {
                resetTb();
                tbDescripcion.Focus();

            }

            if (e.Key == Key.Enter)
            {
                agregarItemALista();
                resetTb();
                tbDescripcion.Focus();
            }
        }

        private void tbPagaCon_TextChanged(object sender, TextChangedEventArgs e)
        {
            string total = tbTotal.Text.Replace("$", "");
            float vuelto = toFloat(tbPagaCon.Text) - toFloat(total);
            tbVuelto.Text = vuelto.ToString();

        }

        private void tbPagaCon_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnAsentar.Focus();
            }
        }


        ///-------------------------------------------------------------------------------------------



    }
}

