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
using System.Windows.Media.Animation;


namespace SIV_Servidor
{
    public partial class MainWindow : Window
    {
        ///Variables Globales
        List<articuloClass> mArticulos;
        List<itemVenta> mItemsVenta;
        int mPestana;   //pestana venta activa
        List<itemCaja> mArticulosCaja;
        Storyboard sb;
        Storyboard sb2;
        Storyboard sbAyuda;
        Storyboard sbListVentas;
        Storyboard sbSlideVentas;
        Storyboard sbSlideCaja;
        Storyboard sbListFiltroMostrar;
        Storyboard sbSlideGrid;
        bool mBuscarArticuloPorCodigo = false;  //al apretar enter end escripcion, si empieza por un numero, busca el articulo
        bool seEditoDescripcionDesdeElPrograma = false;

        public double gridXTo { get; set; }
        public static double gridXFrom { get; set; }
        double mAnchoPantalla;




        //Storyboard sbGridVentasHolaIzquierda;
        //Storyboard sbGridVentasChauIzquierda;
        //Storyboard sbGridCajaHolaDerecha;
        //Storyboard sbGridCajaChauDerecha;
        //static SQLiteConnection conexion;



        ///Shortcuts
        //Navigate Forward/Backward Ctrl+–/Ctrl+Shift+–
        //Peek Definition Alt+F12
        //Comment Code Block Ctrl+K+C/Ctrl+K+U

        ///varios:
        //animacion correr hacia los costados
        //listArticulos alineada con los textboxes
        //ctrl+1 seleccionar pestaña

        //FOCO en control al cambiar pestaña
        //color de seleccion sin gradiente????
        //editar template del listVenta (hacer copia)


        ///controles static
        public static Label statAyuda1;
        public static Label statAyuda2;
        public static Storyboard statSbAyuda;
        
        private void SistemaVentas_Loaded(object sender, RoutedEventArgs e)
        {
            /// referencia a los controles static
            statAyuda1 = labelAyuda;
            statAyuda2 = labelAyuda2;
            statSbAyuda = sbAyuda;
        }


        ///MAIN
        //////sasas
        public MainWindow()
        {
            InitializeComponent();


            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mArticulos = new List<articuloClass>();
            mItemsVenta = new List<itemVenta>();
            mArticulosCaja = new List<itemCaja>();

            mAnchoPantalla = SistemaVentas.Width;
            gridXTo = -500;
            gridXFrom = 0;


            ///animacion
            sb = this.FindResource("Storyboard1") as Storyboard;
            sb2 = this.FindResource("Storyboard2") as Storyboard;
            sbAyuda = this.FindResource("sbAyuda") as Storyboard;
            sbListVentas = this.FindResource("sbListVentas") as Storyboard;
            sbSlideCaja = this.FindResource("sbSlideCaja") as Storyboard;
            sbSlideVentas = this.FindResource("sbSlideVentas") as Storyboard;
            sbListFiltroMostrar = this.FindResource("sbListFiltroMostrar") as Storyboard;
            sbSlideGrid = this.FindResource("sbSlideGrid") as Storyboard;
            //sbGridVentasHolaIzquierda = this.FindResource("sbGridVentasHolaIzquierda") as Storyboard;
            //sbGridVentasChauIzquierda = this.FindResource("sbGridVentasChauIzquierda") as Storyboard;
            //sbGridCajaHolaDerecha = this.FindResource("sbGridCajaHolaDerecha") as Storyboard;
            //sbGridCajaChauDerecha = this.FindResource("sbGridCajaChauDerecha") as Storyboard;
            sb.Completed += (s, e2) =>
            {
                ///FOCO en tbDescripcion
                tbDescripcion.Focus();
                //Keyboard.Focus(tbDescripcion);
                //FocusManager.SetFocusedElement(this, tbDescripcion);
                //consola("0");
            };

            sb2.Completed += (s, e2) =>
            {
                ///FOCO en listCaja
                //FocusManager.SetFocusedElement(this, listVenta);
                if (ucCaja.listCaja.SelectedIndex == -1)
                {
                    ucCaja.listCaja.SelectedIndex = 0;
                }
                var item = ucCaja.listCaja.ItemContainerGenerator.ContainerFromIndex(ucCaja.listCaja.SelectedIndex) as ListBoxItem;
                if (item != null)
                {
                    item.Focus();
                }
                //Keyboard.Focus(listCaja);
                //listCaja.Focus();
                //listCaja.SelectedItem = 1;
                //Keyboard.Focus(listCaja.SelectedIndex);
                //consola("1");
                //consola(gridVentas.RenderTransformOrigin.X.ToString());
            };

            sbSlideCaja.Completed += (s, e2) =>
             {
                 tbDescripcion.Focus();

                 //consola(gridVentas.RenderTransform.Value.OffsetX.ToString());
             };
            sbSlideVentas.Completed += (s, e2) =>
            {
                if (ucCaja.listCaja.SelectedIndex == -1)
                {
                    ucCaja.listCaja.SelectedIndex = 0;
                }
                var item = ucCaja.listCaja.ItemContainerGenerator.ContainerFromIndex(ucCaja.listCaja.SelectedIndex) as ListBoxItem;
                if (item != null)
                {
                    item.Focus();
                }
                //consola(gridVentas.Margin.Left.ToString());
                //consola(gridVentas.RenderTransform.Value.OffsetX.ToString());
            };


            ///SETEAR CONTROLES
            //labelAyuda.Content = "";
            tip();
            ayuda();

            listFiltro.Visibility = Visibility.Hidden;
            //listFiltro.Margin = new Thickness(tabMain.Margin.Left + tbCodigo.Margin.Left + 2, tabMain.Margin.Top + gridTabItemVEntasHeader.Height + tbDescripcion.Margin.Top + tbDescripcion.Height + 10, 0, 0);
            listFiltro.Margin = new Thickness(tbCodigo.Margin.Left + 2, tbDescripcion.Margin.Top + tbDescripcion.Height + 0 + 2, 0, 0);
            //tbBuscar.Focus();


            ///FUNCIONES DE INICIO
            //iniciarOSC();
            //gridFiltroSQL();
            seleccionarPestana(0);
            cargarListaDeArticulos();
            calcularTotal();
            tbDescripcion.Focus();
            CargarDBCaja();
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

            ///borrar todos los elementos de mArticulos
            if (mArticulos != null)
            {
                mArticulos.Clear();
            }

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {
                float precio = toFloat(registro["precio"].ToString());
                float costo = toFloat(registro["costo"].ToString());

                articuloClass tempArticulo = new articuloClass();
                tempArticulo.id = (long)registro["id"];
                tempArticulo.codigo = registro["codigo"].ToString();
                tempArticulo.codigopro = registro["codigopro"].ToString();
                tempArticulo.descripcion = registro["descripcion"].ToString();
                tempArticulo.precio = precio;
                tempArticulo.costo = costo;
                tempArticulo.tags = registro["tags"].ToString();
                tempArticulo.stock = registro["stock"].ToString();
                //tempArticulo.stock = "1";

                mArticulos.Add(tempArticulo);
            }


            ///cerrar conexion
            conexion.Close();

            ///asignar datos al list
            //gridFiltro.ItemsSource = mArticulos;
            listFiltro.ItemsSource = mArticulos;

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
        private void agregarItemALista()
        {
            ///definir variables y obtener valores de los textbox
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
            if (tbCantidad.Tag != null)
                costo = tbCantidad.Tag.ToString();

            ///crear itemVenta
            itemVenta itemTemp = new itemVenta
            {
                codigo = codigo,
                descripcion = descripcion,
                cantidad = cantidad,
                precio = precio,
                subtotal = subtotal,
                costo = costo
            };

            ///agregar item a la tabla temporal
            string tabla = "listventa" + mPestana.ToString().Trim();
            asentarDBItemVenta(itemTemp, tabla);

            ///agregar item al listbox
            //listVenta.Items.Add(itemTemp);

            ///actualizar listVenta
            mostrarListVentaX();

            ///Calcular total
            calcularTotal();

            tbDescripcion.Focus();
        }
        private void asentarDBItemVenta(itemVenta item, string tabla, int idMax = -1)
        {
            ///abrir conexion DB
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=caja.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            SQLiteCommand insertSQL;
            if (idMax == -1)
            {
                ///asentar en tabla temporal, sin IDVENTA y sin FECHA, pero con SUBTOTAL
                insertSQL = new SQLiteCommand("INSERT INTO " + tabla + " (codigo, descripcion, cantidad, precio, subtotal, costo) VALUES (?,?,?,?,?,?)", conexion);
            }
            else
            {
                ///asentar en tabla "caja", con IDVENTA
                insertSQL = new SQLiteCommand("INSERT INTO " + tabla + " (idventa, fecha, codigo, descripcion, cantidad, precio, costo) VALUES (?,DATETIME('NOW'),?,?,?,?,?)", conexion);
                insertSQL.Parameters.AddWithValue("idventa", idMax.ToString());
            }

            //insertSQL.Parameters.AddWithValue("fecha", "DATETIME('NOW')");
            insertSQL.Parameters.AddWithValue("codigo", item.codigo);
            insertSQL.Parameters.AddWithValue("descripcion", item.descripcion);
            insertSQL.Parameters.AddWithValue("cantidad", item.cantidad);
            insertSQL.Parameters.AddWithValue("precio", item.precio);
            if (idMax == -1)
            {
                insertSQL.Parameters.AddWithValue("subtotal", item.subtotal);
            }
            insertSQL.Parameters.AddWithValue("costo", item.costo);
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
        private void asentarVenta()
        {
            ///ejecutar si la lista no está vacía
            if (listVenta.Items.Count > 0)
            {
                ///obtener idVentaMax
                int idVentaMax = obtenerIdVentaMax() + 1;
                //consola(idVentaMax.ToString());

                /////abrir conexion DB
                //SQLiteConnection conexion;
                //conexion = new SQLiteConnection("Data Source=caja.db;Version=3;New=False;Compress=True;");
                //conexion.Open();

                ///recorrer la lista (listVenta) e ir asentando fila por fila
                foreach (itemVenta item in listVenta.Items)
                {
                    //asentarDBItemVenta(item, "caja", conexion, idVentaMax);
                    asentarDBItemVenta(item, "caja", idVentaMax);
                }

                /////Cerrar conexion
                //conexion.Close();

                ///resetear list y recalcular valores
                resetListVenta();
                resetTb();
                calcularTotal();
                CargarDBCaja();

            }
            else
            {
                consola("No hay articulos en la lista.");
            }

            tbDescripcion.Focus();
        }
        private void mostrarListVentaX()
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=caja.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            string tabla = "listventa" + mPestana.ToString().Trim();
            string consulta = "select * from " + tabla;

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///borrar todos los elementos de mItemsVenta
            if (mItemsVenta != null)
            {
                mItemsVenta.Clear();

            }

            //consola("Registros: " + dataTable.Rows.Count.ToString());

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {

                //float cantidad = toFloat(registro["cantidad"].ToString());
                //float precio = toFloat(registro["precio"].ToString());
                //float subtotal= toFloat(registro["subtotal"].ToString());
                //float costo = toFloat(registro["costo"].ToString());

                itemVenta tempItem = new itemVenta();

                tempItem.codigo = registro["codigo"].ToString();
                tempItem.descripcion = registro["descripcion"].ToString();
                tempItem.cantidad = registro["cantidad"].ToString();
                tempItem.precio = registro["precio"].ToString();
                tempItem.subtotal = registro["subtotal"].ToString();
                tempItem.costo = registro["costo"].ToString();
                //tempArticulo.stock = "1";

                mItemsVenta.Add(tempItem);
            }

            ///cerrar conexion
            conexion.Close();

            ///asignar datos al list
            listVenta.ItemsSource = null;
            listVenta.ItemsSource = mItemsVenta;

        }
        private void resetBDventas()
        {
            ///tabla (actual), se borrara el contenido
            string tabla = "listventa" + mPestana.ToString().Trim();

            ///abrir conexion DB
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=caja.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            ///establecer comando SQL y ejecutar
            SQLiteCommand comando = new SQLiteCommand("DELETE FROM " + tabla, conexion);
            try
            {
                comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            ///Cerrar conexion
            conexion.Close();

        }
        private void resetListVenta()
        {
            ///Borrar items de la base de datos
            resetBDventas();


            ///borrar itemsVenta
            if (mItemsVenta != null)
            {
                mItemsVenta.Clear();

            }

            ///des-asignar listVenta a datos
            listVenta.ItemsSource = null;
        }
        private void buscarArticuloPorCodigo()
        {
            string codigoTemp = tbDescripcion.Text.Trim();
            var listaTotal = from registro in mArticulos
                             where registro.codigo.ToLower().Trim().Equals(codigoTemp)
                             select registro;

            int articulosEncontrados = listaTotal.Count();

            ///si encuentra 1 articulo con codigo=codigoTemp, lo muestra, sino sale mensaje de art no encontrado
            if (articulosEncontrados < 1)
            {
                tip("Artículo no encontrado", tbDescripcion);
            }
            else
            {
                articuloClass articulo;
                articulo = listaTotal.ElementAt(0);

                //consola("Res: " + tempArticulo.descripcion.ToString());
                string codigopro = ((articulo.codigo == "" || articulo.codigo == null) ? "" : articulo.codigo.ToString());
                string descripcion = articulo.descripcion.ToString();
                string precio = articulo.precio.ToString("0.00");

                tbCodigo.Text = codigopro;
                seEditoDescripcionDesdeElPrograma = true;
                tbDescripcion.Text = descripcion;
                tbPrecio.Text = precio;
                tbPrecio.Tag = precio;
                tbCantidad.Tag = articulo.costo;

                tbCantidad.Text = "1";
                tbCantidad.SelectAll();
                tbCantidad.Focus();
                listFiltro.Visibility = Visibility.Hidden;

            }


        }

        ///FUNCIONES GENERALES
        /*private void VentanaCaja()
        {
            winCaja ventana = new winCaja();
            //ventana.Show();

            ventana.Owner = this;
            ventana.ShowDialog();
        }
        */
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
            tip();
            //ayuda();
        }
        public static void consola(string texto)
        {
            Console.WriteLine(texto);
            //labelAyuda.Content = texto;
        }
        public static void ayuda2(string texto = "", string texto2 = "")
        {
                if (statAyuda1.Content.ToString() != texto)
                {
                    statAyuda1.Content = texto;
                    statAyuda2.Content = texto2;

                    statSbAyuda.Begin();
                }

        }
        public void ayuda(string texto = "", string texto2 = "")
        {
            //Console.WriteLine(texto);
            //consola(labelAyuda.Content.ToString());

            if (labelAyuda.Content.ToString() != texto)
            {
                labelAyuda.Content = texto;
                labelAyuda2.Content = texto2;

                sbAyuda.Begin();
            }
        }
        private void tip(string texto = "", object sender = null)
        {
            if (texto == "")
            {
                labelTip.Visibility = Visibility.Hidden;
            }
            else
            {
                labelTip.Content = texto;

                if (sender is TextBox)
                {
                    var control = sender as TextBox;
                    ///si es tbDescripcion, acomodo arriba del control, sino abajo
                    if (control.Name == "tbDescripcion")
                    {
                        labelTip.Margin = new Thickness(control.Margin.Left + 135, control.Margin.Top - 16, 0, 0);
                    }
                    else
                    {
                        labelTip.Margin = new Thickness(control.Margin.Left + 2, control.Margin.Top + control.Height + 2, 0, 0);

                    }
                    //consola(control.GetType().ToString());
                }
                labelTip.Visibility = Visibility.Visible;
            }
            //Console.WriteLine(texto);

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



        ///FUNCIONES CAJA
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
                float cantidad = toFloat(registro["cantidad"].ToString());
                float precio = toFloat(registro["precio"].ToString());
                float tmpSubtotal = cantidad * precio;

                //consola(registro["precio"].ToString());

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
                tempArticulo.precio = precio.ToString("0.00");
                tempArticulo.costo = registro["costo"].ToString();
                tempArticulo.subtotal = tmpSubtotal.ToString("0.00");
                tempArticulo.total = "";
                if (dataTable.Rows.Count != index + 1)
                {
                    String tmpSiguienteId = dataTable.Rows[index + 1]["idventa"].ToString();
                    //consola(registro["idventa"].ToString() + "-" + tmpSiguienteId);
                    if (registro["idventa"].ToString() != tmpSiguienteId)
                    {
                        tempArticulo.total = "$" + tmpTotal.ToString("0.00");
                    }
                }
                else
                {
                    tempArticulo.total = tmpTotal.ToString();
                }
                mArticulosCaja.Add(tempArticulo);
                index++;
            }


            ///asigno la lista al control listCaja
            ucCaja.listCaja.ItemsSource = mArticulosCaja;
            //mArticulosCaja.Reverse();

            ///cerrar conexion
            conexion.Close();
        }

        ///CONTROLES
        private void SistemaVentas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                tabMain.SelectedIndex = 0;
                resetTb();
                //tbBuscar.Focus();
                tbDescripcion.Focus();
            }
            if (e.Key == Key.F1)
            {
                tabMain.SelectedIndex = 0;
                /*
                Storyboard sb = this.FindResource("Storyboard1") as Storyboard;
                //Storyboard.SetTarget(sb, this.btn);
                sb.Begin();
                tbDescripcion.Focus();
                FocusManager.SetFocusedElement(this, tbDescripcion);
                Keyboard.Focus(tbDescripcion);
                tbDescripcion.Focus();
                */
                e.Handled = true;

            }
            if (e.Key == Key.F2)
            {

                tabMain.SelectedIndex = 1;
                /*
                Storyboard sb = this.FindResource("Storyboard2") as Storyboard;
                //Storyboard.SetTarget(sb, this.btn);
                sb.Begin();
                listVenta.Focus();
                */
                e.Handled = true;
            }
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }
            if (e.Key == Key.D1 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                tabVentas.SelectedIndex = 0;
            }
            if (e.Key == Key.D2 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                tabVentas.SelectedIndex = 1;
            }
            if (e.Key == Key.D3 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                tabVentas.SelectedIndex = 2;
            }
            if (e.Key == Key.D4 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                tabVentas.SelectedIndex = 3;
            }
            if (e.Key == Key.D5 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                tabVentas.SelectedIndex = 4;
            }

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //ayuda();
            var textBox = sender as TextBox;
            //textBox.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 120));
            //textBox.Background = this.Resources["confoco1"] as SolidColorBrush;
            textBox.Background = App.Current.Resources["confoco"] as SolidColorBrush;

            if (textBox.Name == "tbPrecio")
            {
                ayuda(zAyuda.precio1a, zAyuda.precio1b);
            }
            else if (textBox.Name == "tbDescripcion")
            {
                //tip(mensajeDescripcion, sender.GetType().ToString());
                //ayuda(zAyuda.ayudaDescripcion1);
                string filtro = textBox.Text;
                ///si no esta vacio el texto del filtro
                if (filtro != "")
                {
                    ///comprobar si empieza por un numero o letra
                    string primerLetra = filtro.Substring(0, 1);
                    int valor;
                    if (int.TryParse(primerLetra, out valor))
                    ///si es un numero
                    {
                        ayuda(zAyuda.descripcion4);
                    }
                    else
                    {
                        ///si es una letra
                        ///aplicar filtro
                        //filtroArticulos(filtro);

                        if (listFiltro.Items.Count > 0)
                        {
                            //listFiltro.Visibility = Visibility.Visible;
                            ayuda(zAyuda.descripcion2a, zAyuda.descripcion2b);
                        }
                        else
                        {
                            ayuda(zAyuda.descripcion3);
                            ///ocultar control si no hay resultados
                            //listFiltro.Visibility = Visibility.Hidden;
                        }
                    }
                }
                else
                {
                    ///si esta vacio el texto del filtro
                    ayuda(zAyuda.descripcion1);
                    ///ocultar control si el text esta vacio
                    //listFiltro.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                ayuda();
            }

        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //ayuda();
            var textBox = sender as TextBox;
            //textBox.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            textBox.Background = App.Current.Resources["sinfoco"] as SolidColorBrush;
            if (textBox.Name == "tbDescripcion")
            {
                if (labelTip.Content.ToString() == zAyuda.nuevoArticulo)
                {
                    tip();
                }
            }
        }

        private void Button_GotFocus(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            //boton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 120));
            boton.Background = App.Current.Resources["confoco"] as SolidColorBrush;
        }
        private void Button_LostFocus(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            //boton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            boton.Background = App.Current.Resources["boton"] as SolidColorBrush;
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tab = sender as TabControl;
                int selected = tab.SelectedIndex;
                //consola(e.Source.ToString());

                if (selected == 0)
                {
                    ///color texto pestaña
                    tbPestanaVentas.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                    tbPestanaCaja.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;

                    ///animacion
                    //Storyboard sb = this.FindResource("Storyboard1") as Storyboard;
                    //Storyboard.SetTarget(sb, this.btn);


                    //sb.Begin();
                    //sbSlideVentas.Begin();





                    //gridXFrom=gridVentas.RenderTransform.Transform.trans
                    //sbSlideGrid.Begin();

                    //sbGridCajaChauDerecha.Begin();
                    //sbGridVentasHolaIzquierda.Begin();
                    //sb.Completed -= Sb_Completed;

                    /*
                    sb.Completed += (s, e2) =>
                    {

                        ///FOCO en tbDescripcion
                        tbDescripcion.Focus();
                        Keyboard.Focus(tbDescripcion);
                        FocusManager.SetFocusedElement(this, tbDescripcion);
                        consola("0 - i=" + i.ToString());
                        i++;
                        //sb.Completed -= HandleCustomEvent;
                    };
                    */


                    ///FOCO en tbDescripcion
                    /*
                    tbDescripcion.Focus();
                    Keyboard.Focus(tbDescripcion);
                    FocusManager.SetFocusedElement(this,tbDescripcion);
                    consola("0");
                    */
                }
                if (selected == 1)
                {
                    ///color texto pestaña
                    tbPestanaVentas.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                    tbPestanaCaja.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;

                    ///animacion
                    //Storyboard sb2 = this.FindResource("Storyboard2") as Storyboard;
                    //Storyboard.SetTarget(sb, this.btn);

                    //sb2.Begin();
                    //sbSlideCaja.Begin();

                    //sbGridVentasChauIzquierda.Begin();
                    //sbGridCajaHolaDerecha.Begin();
                    /*
                    sb2.Completed += (s, e2) =>
                    {
                        ///FOCO en listVenta
                        listVenta.Focus();
                        Keyboard.Focus(listVenta);
                        //FocusManager.SetFocusedElement(this, listVenta);
                        //listVenta.SelectedIndex = 0;
                        consola("1");
                        e.Handled = true;
                    };
                    */

                    /*
                    listVenta.Focus();
                    Keyboard.Focus(listVenta);
                    FocusManager.SetFocusedElement(this, listVenta);
                    consola("1");
                    */
                }

                //consola(gridVentas.RenderTransform.Value.OffsetX.ToString());

                //consola("from:" + gridXFrom);
                //consola("to:" + gridXTo);

                //sbSlideGrid.Begin();
                //gridVentas.Margin = new Thickness(mAnchoPantalla * selected * -1, gridVentas.Margin.Top, gridVentas.Margin.Right, gridVentas.Margin.Bottom);


                /// animacion
                gridXFrom = gridMain.RenderTransform.Value.OffsetX;
                gridXTo = (double)(mAnchoPantalla * selected * -1);

                ///easing
                CircleEase easing = new CircleEase();  // or whatever easing class you want
                easing.EasingMode = EasingMode.EaseInOut;
                DoubleAnimation scrollQueue = new DoubleAnimation();
                scrollQueue.By = -1;   //este valor invente yo
                scrollQueue.EasingFunction = easing;
                //scrollQueue.Duration = new Duration(TimeSpan.FromSeconds(0.25));
                //gridVentas.BeginAnimation(Grid.MarginProperty, scrollQueue);

                ///animacion en sí
                ThicknessAnimation ta = new ThicknessAnimation();
                ta.From = gridMain.Margin;
                ta.To = new Thickness(gridXTo, ta.From.Value.Top, ta.From.Value.Right, ta.From.Value.Bottom);
                ta.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                ta.EasingFunction = easing;
                ta.Completed += (s, e2) =>
                {
                    ///pestana VENTAS
                    if (selected == 0)
                    {
                        ///FOCO en tbDescripcion
                        tbDescripcion.Focus();
                    }
                    ///pestana CAJA
                    if (selected == 1)
                    {
                        ///FOCO en listCaja
                        //FocusManager.SetFocusedElement(this, listVenta);
                        if (ucCaja.listCaja.SelectedIndex == -1)
                        {
                            ucCaja.listCaja.SelectedIndex = 0;
                        }
                        var item = ucCaja.listCaja.ItemContainerGenerator.ContainerFromIndex(ucCaja.listCaja.SelectedIndex) as ListBoxItem;
                        if (item != null)
                        {
                            item.Focus();
                        }
                    }

                };

                //dont need to use story board but if you want pause,stop etc use story board
                gridMain.BeginAnimation(Grid.MarginProperty, ta);


            }
            e.Handled = true;
        }
        private void tabVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = sender as TabControl;
            int selected = tab.SelectedIndex;
            seleccionarPestana(selected);
            mostrarListVentaX();

            sbListVentas.Begin();
            //consola(tab.Name);
            //consola(selected.ToString());
            //tbDescripcion.Focus();
            e.Handled = true;
        }
        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            tbDescripcion.Focus();
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
                seEditoDescripcionDesdeElPrograma = true;
                tbDescripcion.Text = descripcion;
                tbPrecio.Text = precio;
                tbPrecio.Tag = precio;
                //consola("costo:" + fila.costo.ToString());
                tbCantidad.Tag = fila.costo;
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
        private void listFiltro_GotFocus(object sender, RoutedEventArgs e)
        {
            ayuda(zAyuda.listFiltro1a, zAyuda.listFiltro1a);

        }
        private void listFiltro_LostFocus(object sender, RoutedEventArgs e)
        {
            //ayuda();
        }


        private void tbDescripcion_TextChanged(object sender, TextChangedEventArgs e)
        {
            mBuscarArticuloPorCodigo = false;

            ///si anteriormente se busco un articulo y no se encontro, borrar mensaje al editer tbDescripcion
            if (labelTip.Content.ToString() == zAyuda.articuloNoEncontrado)
            {
                tip();
            }

            ///borrar el tbCodigo ya que si se edita la descripcion, deja de ser ESE articulo
            //consola(seEditoDescripcionDesdeElPrograma.ToString());
            if (seEditoDescripcionDesdeElPrograma)
            {
                seEditoDescripcionDesdeElPrograma = false;
                e.Handled = true;
            }
            else
            {
                tbCodigo.Text = "";

            }


            var textBox = sender as TextBox;
            string filtro = textBox.Text.Trim();
            //consola(listFiltro.Items.Count.ToString());
            if (filtro != "" && textBox.IsFocused)
            {
                ///texto filtro NO esta vacio

                ///comprobar si empieza por un numero o letra
                string primerLetra = filtro.Substring(0, 1);
                int valor;
                if (int.TryParse(primerLetra, out valor))
                {
                    ///si es un numero
                    ayuda(zAyuda.descripcion4);
                    mBuscarArticuloPorCodigo = true;
                }
                else
                {
                    ///si es una letra

                    ///aplicar filtro
                    //gridFiltroSQL(filtro);
                    filtroArticulos(filtro);

                    ///mostrar tip paa agregar nuevo articulo
                    if (tbDescripcion.IsFocused)
                    {
                        tip(zAyuda.nuevoArticulo, tbDescripcion);
                    }

                    ///mostrar list si hay resultados
                    if (listFiltro.Items.Count > 0)
                    {
                        listFiltro.Visibility = Visibility.Visible;
                        ayuda(zAyuda.descripcion2a, zAyuda.descripcion2b);
                    }
                    else
                    {
                        ayuda(zAyuda.descripcion3);
                        ///ocultar control si no hay resultados
                        listFiltro.Visibility = Visibility.Hidden;
                    }
                }
            }
            else
            {
                ///texto filtro vacio
                ayuda(zAyuda.descripcion1);
                ///ocultar control si el text esta vacio
                listFiltro.Visibility = Visibility.Hidden;
            }
        }
        private void tbDescripcion_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                //listFiltro.Focus();
                if (listFiltro.Visibility == Visibility.Visible)
                {
                    //consola("down-"+ listFiltro.SelectedIndex.ToString());
                    //if (listFiltro.SelectedIndex == -1)
                    //{
                    //    listFiltro.SelectedIndex = 0;
                    //}
                    listFiltro.SelectedIndex = 0;
                    var item = listFiltro.ItemContainerGenerator.ContainerFromIndex(listFiltro.SelectedIndex) as ListBoxItem;
                    if (item != null)
                    {
                        //consola(listFiltro.SelectedItem.ToString());
                        item.Focus();
                        //listFiltro.ScrollIntoView(listFiltro.SelectedItem);
                    }
                    e.Handled = true;

                }
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
                ///si esta vacio tbDescipcion, ir a tbPagaCon
                if (tbDescripcion.Text.Trim() == "")
                {
                    if (listVenta.Items.Count > 0)
                    {
                        tbPagaCon.Focus();
                    }
                }
                else
                {
                    ///comprobar si es un numero (para buscar articulo por codigo)
                    if (mBuscarArticuloPorCodigo)
                    {
                        buscarArticuloPorCodigo();
                        e.Handled = true;
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
            var textBox = sender as TextBox;
            if (textBox.IsFocused)
            {
                string texto = textBox.Text.Trim();
                string tag = "";
                if (textBox.Tag != null)
                {
                    tag = textBox.Tag.ToString();
                }
                if (texto != tag)
                {
                    ayuda(zAyuda.precio2a, zAyuda.precio2b);
                    tip("<ENTER> Actualizar precio en Base de Datos\n<ESC> Cancelar", sender);
                }
            }

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

        private void listFiltro_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //consola(listFiltro.IsVisible.ToString());
            //consola("ok");
            sbListFiltroMostrar.Begin();
        }




        ///-------------------------------------------------------------------------------------------



    }
}


