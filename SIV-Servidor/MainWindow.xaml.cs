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
using System.Collections.ObjectModel;

namespace SIV_Servidor
{
    public partial class MainWindow : Window
    {
        ///ver qué tanto lio seria hacer el binding de datos (masque nada para aprender)
        ///pero en todo caso, podria intentar armar funciones de actuaqlizacion,
        ///o una mezcla de tecnicoas
        ///Pero lo más importante es terminar esa parte rápido.

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


        ///Variables Globales
        //public static List<itemCaja> mArticulosCaja = new List<itemCaja>();
        //public static ObservableCollection<itemCaja> mArticulosCaja = new ObservableCollection<itemCaja>();
        Storyboard sbAyuda;
        public double gridXTo { get; set; }
        public double gridXFrom { get; set; }
        public static int mPestanaMain { get; set; }
        double mAnchoPantalla;


        ///controles static
        public static Label statAyuda1;
        public static Label statAyuda2;
        public static Storyboard statSbAyuda;
        public static ucCaja statUcCaja;
        public static ucVentas statUcVentas;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
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

            //CargarDBCaja();
        }


        ///MAIN
        //////sasas
        public MainWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            mAnchoPantalla = SistemaVentas.Width;
            gridXTo = -500;
            gridXFrom = 0;


            ///animacion
            sbAyuda = this.FindResource("sbAyuda") as Storyboard;

            ///SETEAR CONTROLES
            ayuda();


            ///FUNCIONES DE INICIO
            //MessageBox.Show("Armar esquema de todo lo q falta y priorizar");
        }

        //------------------------------------------------------------------------------------------
        ///-----------------------------------------------------------------------------------------

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


        ///CONTROLES
        private void SistemaVentas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Escape)
            //{
            //    tabMain.SelectedIndex = 0;
            //    statUcVentas.resetTb();
            //    //tbBuscar.Focus();
            //    ucVentas.tbDescripcion.Focus();
            //}
            if (e.Key == Key.F1)
            {
                tabMain.SelectedIndex = 0;
                e.Handled = true;

            }
            if (e.Key == Key.F2)
            {
                tabMain.SelectedIndex = 1;
                e.Handled = true;
            }
            //if (e.Key == Key.F3)
            //{
            //    tabMain.SelectedIndex = 2;
            //    e.Handled = true;
            //}

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

                /// animacion
                gridXFrom = gridMain.RenderTransform.Value.OffsetX;
                gridXTo = (double)(mAnchoPantalla * selected * -1);
                //consola("from:" + gridXFrom);
                //consola("to:" + gridXTo);

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
                ta.Duration = new Duration(TimeSpan.FromSeconds((double)App.Current.Resources["TiempoAnimacion"]));
                ta.EasingFunction = easing;
                ta.Completed += (s, e2) =>
                {
                    //consola(selected.ToString());

                    ///pestana VENTAS
                    if (selected == 0)
                    {
                        ///FOCO en tbDescripcion
                        ucVentas.tbDescripcion.Focus();
                    }
                    ///pestana CAJA
                    if (selected == 1)
                    {
                        //FOCO en listCaja
                        //int tempSeleccion = ucCaja.tabCaja.SelectedIndex;
                        //ucCaja.tabCaja.SelectedIndex = -1;
                        //ucCaja.tabCaja.SelectedIndex = tempSeleccion;

                        /// FOCO en listCaja
                        if (ucCaja.mPestanaCaja == 0)
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
                        }
                        else
                        /// FOCO en PESTANA IMPRIMIR
                        {


                            /// si el boton tiene el style de selecionado, hace foco en el textbox tbNombre, sino en el boton mBotonSelected

                            if (ucCaja.mBotones[ucCaja.mBotonSelected].Style == ucCaja.styleBotonSelected)
                            {
                                ucCaja.tbNombre.Focus();
                            }
                            else
                            {
                                ucCaja.mBotones[ucCaja.mBotonSelected].Focus();
                            }
                        }
                    }

                };

                //dont need to use story board but if you want pause,stop etc use story board
                gridMain.BeginAnimation(Grid.MarginProperty, ta);


                ///color textblock de las pestañas en tabMain

                tbPestanaCaja.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 0)
                {
                    tbPestanaVentas.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }

                tbPestanaVentas.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 1)
                {
                    tbPestanaCaja.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }

                //tbPestanaOpciones.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                //if (selected == 2)
                //{
                //    tbPestanaOpciones.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                //}

                ///Index Pestana MAIN seleecionada
                mPestanaMain = selected;
            }
            e.Handled = true;
        }


        ///extensiones
        private float toFloat(string cadena)
        {
            return zfun.toFloat(cadena);
        }
        private bool esDecimal(Key key)
        {
            return zfun.esDecimal(key);
        }
        private void consola(string texto)
        {
            zfun.consola(texto);
        }



        ///-------------------------------------------------------------------------------------------



    }
}


