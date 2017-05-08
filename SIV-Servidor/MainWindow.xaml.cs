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

using System.IO;
using System.Data.SQLite; //Utilizamos la DLL
using System.Data;
using System.Globalization;
using System.Windows.Media.Animation;

//using System;
using Bespoke.Common;
using Bespoke.Common.Osc;
using System.Windows.Threading;
using System.Net;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SIV_Servidor
{
    public partial class MainWindow : INotifyPropertyChanged
    {

        ///Variables Globales
        public static List<itemCaja> mArticulosCaja = new List<itemCaja>();

        
        ///ver qué tanto lio seria hacer el binding de datos (masque nada para aprender)
        ///pero en todo caso, podria intentar armar funciones de actuaqlizacion,
        ///o una mezcla de tecnicoas
        ///Pero lo más importante es terminar esa parte rápido.

        Storyboard sbAyuda;

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
        public static ucCaja statUcCaja;
        public static ucVentas statUcVentas;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SistemaVentas_Loaded(object sender, RoutedEventArgs e)
        {
            /// referencia a los controles static
            statAyuda1 = labelAyuda;
            statAyuda2 = labelAyuda2;
            statSbAyuda = sbAyuda;
            statUcCaja = ucCaja;
            statUcVentas = ucVentas;
            statUcVentas.tbDescripcion.Focus();

            CargarDBCaja();
        }


        ///MAIN
        //////sasas
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            mAnchoPantalla = SistemaVentas.Width;
            gridXTo = -500;
            gridXFrom = 0;

            //mArticulosCaja = new List<itemCaja>();

            ///animacion
            //sb = this.FindResource("Storyboard1") as Storyboard;
            //sb2 = this.FindResource("Storyboard2") as Storyboard;
            sbAyuda = this.FindResource("sbAyuda") as Storyboard;

            //sbSlideCaja = this.FindResource("sbSlideCaja") as Storyboard;
            //sbSlideVentas = this.FindResource("sbSlideVentas") as Storyboard;
            //sbSlideGrid = this.FindResource("sbSlideGrid") as Storyboard;
            //sbGridVentasHolaIzquierda = this.FindResource("sbGridVentasHolaIzquierda") as Storyboard;
            //sbGridVentasChauIzquierda = this.FindResource("sbGridVentasChauIzquierda") as Storyboard;
            //sbGridCajaHolaDerecha = this.FindResource("sbGridCajaHolaDerecha") as Storyboard;
            //sbGridCajaChauDerecha = this.FindResource("sbGridCajaChauDerecha") as Storyboard;




            ///SETEAR CONTROLES
            ayuda();


            ///FUNCIONES DE INICIO
            
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



        ///FUNCIONES GENERALES


        public static float toFloat(string cadena)
        {
            float resultado = 0;
            float.TryParse(cadena, out resultado);
            return resultado;
        }
        public static void consola(string texto)
        {
            Console.WriteLine(texto);
            //labelAyuda.Content = texto;
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
        public static void ayuda2(string texto = "", string texto2 = "")
        {
            if (statAyuda1 != null)
            {
                if (statAyuda1.Content.ToString() != texto)
                {
                    statAyuda1.Content = texto;
                    statAyuda2.Content = texto2;

                    statSbAyuda.Begin();
                }
            }


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




        ///CONTROLES
        private void SistemaVentas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                tabMain.SelectedIndex = 0;
                statUcVentas.resetTb();
                //tbBuscar.Focus();
                ucVentas.tbDescripcion.Focus();
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
                ucVentas.tabVentas.SelectedIndex = 0;
            }
            if (e.Key == Key.D2 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucVentas.tabVentas.SelectedIndex = 1;
            }
            if (e.Key == Key.D3 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucVentas.tabVentas.SelectedIndex = 2;
            }
            if (e.Key == Key.D4 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucVentas.tabVentas.SelectedIndex = 3;
            }
            if (e.Key == Key.D5 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucVentas.tabVentas.SelectedIndex = 4;
            }

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
                        ucVentas.tbDescripcion.Focus();
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



        ///Funciones caja
        public static void CargarDBCaja()
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
            if (MainWindow.mArticulosCaja != null)
            {
                MainWindow.mArticulosCaja.Clear();
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
                MainWindow.mArticulosCaja.Add(tempArticulo);
                index++;
            }


            ///asigno la lista al control listCaja
            
            ///cargaError
            //if (MainWindow.statUcCaja != null)
            //{
                statUcCaja.listCaja.ItemsSource = mArticulosCaja;

            //}
            mArticulosCaja.Reverse();

            ///cerrar conexion
            conexion.Close();
        }

        ///-------------------------------------------------------------------------------------------



    }
}


