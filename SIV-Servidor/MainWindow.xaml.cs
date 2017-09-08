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

        ///Shortcuts

        //Navigate Forward/Backward Ctrl+–/Ctrl+Shift+–

        ///Variables Globales

        //public static List<itemCaja> mArticulosCaja = new List<itemCaja>();
        //public static ObservableCollection<itemCaja> mArticulosCaja = new ObservableCollection<itemCaja>();
        Storyboard sbAyuda;
        //public double gridXFrom { get; set; }
        double mAnchoPantalla;

        ///controles static
        public static Label statAyuda1;
        public static Label statAyuda2;
        public static Storyboard statSbAyuda;
        public static ucImpresiones statucImpresiones;
        public static ucInicio statucInicio;
        public static ucConsultas statUcConsultas;
        public static TextBox statBalanceCaja;
        public static Label statLabBalanceCaja;
        public static ProgressBar statPBar;
        public static Button statCortinaNegra;
        //public static uczImprimirConsulta statImprimirConsulta;

        ///propiedades
        public double gridXTo { get; set; }
        public static int mPestanaMain { get; set; }

        ///se usa para las propiedades get/set
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        ///LOADED
        private void SistemaVentas_Loaded(object sender, RoutedEventArgs e)
        {
            /// referencia a los controles static
            statAyuda1 = labelAyuda;
            statAyuda2 = labelAyuda2;
            statSbAyuda = sbAyuda;
            statucImpresiones = ucImpresiones;
            statucInicio = ucInicio;
            statucInicio.tbDescripcion.Focus();
            statUcConsultas = ucConsultas;
            //CargarDBCaja();
            statPBar = pBar;
            statBalanceCaja = tbBalanceCaja;
            statLabBalanceCaja = labBalanceCaja;
            statCortinaNegra = gridCortinaNegra;
            //statImprimirConsulta = uczImprimirConsulta;

            ///actualizar 'tbBalanceCaja' en MainWindow desde ucInicio
            if (tbBalanceCaja != null)
            {
                tbBalanceCaja.Text = statucInicio.tbBalanceCaja.Text;
                tbBalanceCaja.Background = statucInicio.tbBalanceCaja.Background;
                //string tempLabBalCaja = statucInicio.labBalanceCaja.Content.ToString().Substring(0, 1);
                //if (tempLabBalCaja == "S" || tempLabBalCaja == "F")
                //{
                //    labBalanceCaja.Content = tempLabBalCaja;
                //} else
                //{
                //    labBalanceCaja.Content = "";
                //}
                labBalanceCaja.Content = statucInicio.labBalanceCaja.Content;
            }
        }


        ///MAIN
        //////sasas
        public MainWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            mAnchoPantalla = SistemaVentas.Width;
            //gridXTo = -500;
            //gridXFrom = 0;


            ///animacion
            sbAyuda = this.FindResource("sbAyuda") as Storyboard;

            ///SETEAR CONTROLES
            pBar.Visibility = Visibility.Hidden;
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

            if (labelAyuda.Content.ToString() != texto || labelAyuda2.Content.ToString() != texto2)
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
                if (statAyuda1.Content.ToString() != texto || statAyuda2.Content.ToString() != texto2)
                {
                    statAyuda1.Content = texto;
                    statAyuda2.Content = texto2;

                    statSbAyuda.Begin();
                }
            }


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

        ///CONTROLES
        private void SistemaVentas_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                consola("maximize");
                consola("w:" + this.ActualWidth.ToString() + " h:" + this.ActualHeight.ToString());
                consola("---------");
            }
        }
        private void SistemaVentas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            consola("w:" + this.Width.ToString() + " h:" + this.Height.ToString());
        }
        private void SistemaVentas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            /// ATENCION: SI ESTA VISIBLE LA 'CORTINA NEGRA' ENTONCES IGNORO TODO ESTO
            if (gridCortinaNegra.IsVisible)
            {
                return;
            }

            //if (e.Key == Key.Escape)
            //{
            //    tabMain.SelectedIndex = 0;
            //    statucInicio.resetTb();
            //    //tbBuscar.Focus();
            //    ucInicio.tbDescripcion.Focus();
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
            if (e.Key == Key.F3)
            {
                ///si NO esta seleccionada la pestaña 'consultas', seleccionarla
                if (tabMain.SelectedIndex != 2)
                {
                    tabMain.SelectedIndex = 2;
                    e.Handled = true;

                }
                ///si no, si esta seleccionada la pestaña consultas, alternar entre los 3 tipos de consultas
                else
                {
                    if (ucConsultas.tabConsultas.SelectedIndex == 0)
                    {

                        ucConsultas.tabConsultas.SelectedIndex = 1;
                        e.Handled = true;
                    }
                    else if (ucConsultas.tabConsultas.SelectedIndex == 1)
                    {
                        ucConsultas.tabConsultas.SelectedIndex = 2;
                        e.Handled = true;
                    }
                    else if (ucConsultas.tabConsultas.SelectedIndex == 2)
                    {
                        ucConsultas.tabConsultas.SelectedIndex = 0;
                        e.Handled = true;
                    }

                }
            }
            if (e.Key == Key.F4)
            {
                tabMain.SelectedIndex = 3;
                e.Handled = true;
            }

            if (e.Key == Key.F5)
            {
                ///si esta seleccionada la pestana 'inicio'
                if (tabMain.SelectedIndex == 0)
                {
                    ///si btnCaja esta 'presionado'
                    if (ucInicio.btnCaja.IsChecked == true)
                    {
                        bool focoEnCaja = false;
                        if (ucInicio.tbCaja01.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja02.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja03.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja04.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja05.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja06.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja07.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja08.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja09.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja10.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja11.IsFocused == true)
                            focoEnCaja = true;
                        if (ucInicio.tbCaja12.IsFocused == true)
                            focoEnCaja = true;

                        ///si el foco esta en alguno de los tbCaja
                        if (focoEnCaja)
                        {
                            ucInicio.MostrarCaja(false);

                            //ucInicio.tbDescripcion.Focus();
                        }
                        else
                        {
                            ucInicio.tbCaja06.Focus();
                        }
                    }
                    else
                    {
                        ucInicio.MostrarCaja(true);
                    }
                    e.Handled = true;
                }
            }

            //if (tabMain.SelectedIndex == 2)
            //{
            //    if (e.Key == Key.F5)
            //    {
            //        ucConsultas.tabConsultas.SelectedIndex = 0;
            //        e.Handled = true;

            //    }
            //    if (e.Key == Key.F6)
            //    {
            //        ucConsultas.tabConsultas.SelectedIndex = 1;
            //        e.Handled = true;
            //    }
            //    if (e.Key == Key.F7)
            //    {
            //        ucConsultas.tabConsultas.SelectedIndex = 2;
            //        e.Handled = true;
            //    }
            //}

            if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }
            if (e.Key == Key.D1 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucInicio.tabVentas.SelectedIndex = 0;
            }
            if (e.Key == Key.D2 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucInicio.tabVentas.SelectedIndex = 1;
            }
            if (e.Key == Key.D3 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucInicio.tabVentas.SelectedIndex = 2;
            }
            if (e.Key == Key.D4 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucInicio.tabVentas.SelectedIndex = 3;
            }
            if (e.Key == Key.D5 && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                ucInicio.tabVentas.SelectedIndex = 4;
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
                //gridXFrom = gridMain.RenderTransform.Value.OffsetX;
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

                    ///pestana INICIO
                    if (selected == 0)
                    {
                        ///FOCO en tbDescripcion
                        ucInicio.tbDescripcion.Focus();
                    }
                    ///pestana IMPRESIONES
                    if (selected == 1)
                    {
                        //FOCO en listCaja
                        //int tempSeleccion = ucImpresiones.tabCaja.SelectedIndex;
                        //ucImpresiones.tabCaja.SelectedIndex = -1;
                        //ucImpresiones.tabCaja.SelectedIndex = tempSeleccion;

                        /// FOCO en listCaja
                        if (ucImpresiones.mPestanaCaja == 0)
                        {
                            if (ucImpresiones.listCaja.SelectedIndex == -1)
                            {
                                ucImpresiones.listCaja.SelectedIndex = 0;
                            }

                            var item = ucImpresiones.listCaja.ItemContainerGenerator.ContainerFromIndex(ucImpresiones.listCaja.SelectedIndex) as ListBoxItem;
                            if (item != null)
                            {
                                item.Focus();
                            }
                        }
                        else
                        /// FOCO en PESTANA IMPRIMIR
                        {


                            /// si el boton tiene el style de selecionado, hace foco en el textbox tbNombre, sino en el boton mBotonSelected

                            if (ucImpresiones.mBotones[ucImpresiones.mBotonSelected].Style == ucImpresiones.styleBotonSelected)
                            {
                                ucImpresiones.tbNombre.Focus();
                            }
                            else
                            {
                                ucImpresiones.mBotones[ucImpresiones.mBotonSelected].Focus();
                            }
                        }
                    }
                    ///pestana CONSULTAS
                    if (selected == 2)
                    {
                        if (ucConsultas.mTipoDeConsulta == "pendientes")
                        {
                            ///si esta visible el myInputBox (para cargar un pago), darle el foco
                            ///sino, darle foco al filtro de nombres
                            if (ucConsultas.myInputBox.Visibility == Visibility.Visible)
                            {
                                ucConsultas.myInputBox_Texto.Focus();
                            }
                            else
                            {
                                ucConsultas.tbFiltrar.Focus();
                            }
                        }
                    }
                    ///pestana GASTOS
                    if (selected == 3)
                    {
                        ucGastos.tbDescripcion.Focus();
                        //ayuda("");
                    }


                };

                //dont need to use story board but if you want pause,stop etc use story board
                gridMain.BeginAnimation(Grid.MarginProperty, ta);


                ///color textblock de las pestañas en tabMain

                tbPestanaVentas.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 0)
                {
                    tbPestanaVentas.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }

                tbPestanaCaja.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 1)
                {
                    tbPestanaCaja.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }

                tbPestanaConsultas.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 2)
                {
                    tbPestanaConsultas.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }
                tbPestanaGastos.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 3)
                {
                    tbPestanaGastos.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }

                ///Index Pestana MAIN seleecionada
                mPestanaMain = selected;
            }
            e.Handled = true;
        }

        ///expander
        private void expanderOpciones_LostFocus(object sender, RoutedEventArgs e)
        {
            //consola("chau");
            expanderOpciones.IsExpanded = false;
        }
        private void expanderOpciones_Expanded(object sender, RoutedEventArgs e)
        {
            var control = sender as Expander;
            bool expanded = control.IsExpanded;
            //consola(expanded.ToString());
            if (expanded)
            {
                gridCortinaOpciones.Visibility = Visibility.Visible;
            }
            else
            {
                gridCortinaOpciones.Visibility = Visibility.Hidden;

            }
            //consola(gridCortinaNegra.IsVisible.ToString());
        }

        ///PROGRESS BAR
        public static void mostrarPBar(bool mostrar)
        {
            if (mostrar)
            {
                statPBar.Visibility = Visibility.Visible;
            }
            else
            {
                statPBar.Visibility = Visibility.Hidden;
            }

        }

        /// /// BOTONES MENU

        /// boton 'Ver Grilla'
        private void btnOpcionesVerGrillaDeImpresion_Click(object sender, RoutedEventArgs e)
        {
            ///ayuda
            ayuda(zAyuda.grillaImpresionFacturas);

            ///crear control 
            zImpresion.impresionFactura hojaFactura = new zImpresion.impresionFactura();

            ///setear datos para mejor visualizacion
            hojaFactura.tbCantidad.Text = "";
            hojaFactura.tbDescripcion.Text = "";
            hojaFactura.tbPrecio.Text = "";
            hojaFactura.tbSubtotal.Text = "";
            for (int i = 1; i < 11; i++)
            {
                hojaFactura.tbCantidad.Text += "  " + i.ToString() + "  \n";
                hojaFactura.tbDescripcion.Text += "Articulo " + i.ToString() + "\n";
                hojaFactura.tbPrecio.Text += "$ " + i.ToString() + ".00\n";
                hojaFactura.tbSubtotal.Text += "$ " + (i * i).ToString() + ".00\n";
            }

            ///ubicacion del control
            hojaFactura.HorizontalAlignment = HorizontalAlignment.Center;
            hojaFactura.HorizontalContentAlignment = HorizontalAlignment.Center;
            hojaFactura.VerticalAlignment = VerticalAlignment.Center;
            hojaFactura.VerticalContentAlignment = VerticalAlignment.Center;

            ///recortar la parte de abajo del control (hoja sobrante)
            hojaFactura.Height = hojaFactura.Height * 0.55;

            ///redimensionar control
            ScaleTransform scale = new ScaleTransform();
            scale.ScaleY = 0.80;
            scale.ScaleX = 0.80;
            hojaFactura.LayoutTransform = scale;

            ///crear StackPanel para la hoja y los botones, y setear
            StackPanel sp = new StackPanel();
            sp.HorizontalAlignment = HorizontalAlignment.Center;
            sp.VerticalAlignment = VerticalAlignment.Center;
            sp.Orientation = Orientation.Horizontal;

            ///crear stackpanel 'main', que contiene al otro stack y ademas agrega un texto arriba
            StackPanel spMain = new StackPanel();
            spMain.HorizontalAlignment = HorizontalAlignment.Center;
            spMain.VerticalAlignment = VerticalAlignment.Center;
            spMain.Orientation = Orientation.Vertical;

            ///crear botones (y su respectivo StackPanel), setear vistas 
            StackPanel spBotones = new StackPanel();
            //spBotones.Width = 100;
            Button bImprimirPrueba = new Button();
            Button bGuardar = new Button();
            Button bCancelar = new Button();
            bImprimirPrueba.Content = "Imprimir Prueba";
            bGuardar.Content = "Guardar";
            bCancelar.Content = "Cancelar";
            Style styleBoton = Application.Current.FindResource("botonGrillaImpresionFactura") as Style;
            bImprimirPrueba.Style = styleBoton;
            bGuardar.Style = styleBoton;
            bCancelar.Style = styleBoton;
            spBotones.Children.Add(bImprimirPrueba);
            spBotones.Children.Add(bGuardar);
            spBotones.Children.Add(bCancelar);

            ///agregar click handlers a los botones
            bImprimirPrueba.Click += (o, s) =>
            {
                plantillaImpresion p = new plantillaImpresion();
                p.proceso = "impresionDePrueba";
                hojaFactura.imprimir(p);
            };
            bGuardar.Click += (o, s) =>
            {
                guardarPosicionesImpresionFactura(hojaFactura.gridHoja);
                gridCortinaNegra.Visibility = Visibility.Hidden;
                //gridMain.Children.Remove(sp);
                windowGrid.Children.Remove(spMain);
                ucInicio.tbDescripcion.Focus();
            };
            bCancelar.Click += (o, s) =>
            {
                gridCortinaNegra.Visibility = Visibility.Hidden;
                //gridMain.Children.Remove(sp);
                windowGrid.Children.Remove(spMain);
                ucInicio.tbDescripcion.Focus();
            };

            ///agregar elementos al StackPanel 
            sp.Children.Add(hojaFactura);
            sp.Children.Add(spBotones);

            ///mostrar 'cortina' de fondo
            gridCortinaNegra.Visibility = Visibility.Visible;

            ///crear texto de ayuda
            TextBlock tbAyuda = new TextBlock();
            tbAyuda.Text = "Seleccione un texto con el mouse y utilice las flechas del teclado para acomodarlo en la posición deseada.";
            tbAyuda.FontSize = 12;
            tbAyuda.FontWeight = FontWeights.Bold;
            tbAyuda.Margin = new Thickness(0, 0, 0, 5);
            tbAyuda.Foreground = Application.Current.FindResource("textoclaro") as Brush;

            ///Agregar al stackpanel 'main' el otro stack y el texto de ayuda
            spMain.Children.Add(tbAyuda);
            spMain.Children.Add(sp);

            ///agregar StackPanel al mainGrid (mostrar)
            //windowGrid.Children.Add(sp);
            windowGrid.Children.Add(spMain);

            ///foco en la hoja (no se si sirve)
            hojaFactura.gridHoja.Focus();


            //gridMain.Children.Add(hojaFactura);
        }
        ///boton Cambiar Numero de FActura
        private void btnOpcionesCambiarNroDeFactura_Click(object sender, RoutedEventArgs e)
        {
            ///mostrar 'cortina' de fondo
            gridCortinaNegra.Visibility = Visibility.Visible;

            ///Crear control
            zOpciones.opcCambiarNroFactura opcFactura = new zOpciones.opcCambiarNroFactura();

            ///agregar StackPanel al mainGrid (mostrar)
            //gridMain.Children.Add(opcFactura);
            windowGrid.Children.Add(opcFactura);
            opcFactura.tbCambiarPor.Focus();

        }
        /// boton Imprimir codigos
        private void btnOpcionesImprimirCodigos_Click(object sender, RoutedEventArgs e)
        {
            ///mostrar 'cortina' de fondo
            gridCortinaNegra.Visibility = Visibility.Visible;

            ///Crear control
            zOpciones.opcImprimirCodigos imprimirCodigos = new zOpciones.opcImprimirCodigos();

            ///agregar StackPanel al mainGrid (mostrar)
            windowGrid.Children.Add(imprimirCodigos);
            imprimirCodigos.btnSalir.Focus();

        }

        ///FUNCIONES
        private void guardarPosicionesImpresionFactura(Grid grid)
        {
            ///recorrer todos textblocks dentro del grid
            foreach (var t in LogicalTreeHelper.GetChildren(grid))
            {
                if (t is TextBlock)
                {
                    TextBlock tb = t as TextBlock;
                    string tbName = tb.Name.Trim();

                    Thickness pos = new Thickness();
                    pos = tb.Margin;

                    string nombre = "impresionFactura-" + tbName;
                    string valor = pos.Left.ToString() + "-" + pos.Top.ToString();
                    zdb.grabarConfig(nombre, valor);
                    //zfun.consola(nombre + " = " + valor);
                }
            }
        }


        ///-------------------------------------------------------------------------------------------



    }
}


