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
using SIV_Servidor;
using System.Data;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;

namespace SIV_Servidor
{
    public partial class ucCaja : UserControl
    {
        ///Globales 
        public static ObservableCollection<itemCaja> mArticulosCaja =
            new ObservableCollection<itemCaja>();   //Lista los articulos de la caja

        public double gridXTo { get; set; }
        public double gridXFrom { get; set; }
        double mAnchoPantalla;
        double mMargen = 15;
        public static int mPestanaCaja { get; set; }

        public static List<Button> mBotones = new List<Button>();
        public static int mBotonSelected = 0;

        public ObservableCollection<itemCliente>
            mTotalClientes = new ObservableCollection<itemCliente>();     //Listado de articulos a la venta filtrados



        bool seEditoDescripcionDesdeElPrograma = false;

        /// animaciones
        Storyboard sbDatosDelClienteMostrar;
        Storyboard sbDatosDelClienteOcultar;
        Storyboard sbListFiltroClientesMostrar;
        Storyboard sbListFiltroClientesOcultar;


        ///ANIMACION BOTONES
        double botonXTo;
        CircleEase easing;
        ThicknessAnimation aniAnchoBtn;
        Style styleBoton = Application.Current.FindResource("BotonMenu") as Style;
        public static Style styleBotonSelected = Application.Current.FindResource("BotonMenuSelected") as Style;

        ///MAIN
        public ucCaja()
        {
            InitializeComponent();

            ///Globales
            mAnchoPantalla = 980; // gridCajaMain.Width;
            //gridCajaSlide.Width = 970; //mAnchoPantalla - (mMargen * 2);
            gridCajaSlide.Margin = new Thickness(gridCajaSlide.Margin.Left, gridCajaSlide.Margin.Top, gridCajaSlide.Margin.Bottom, 0);
            gridXTo = -500;
            gridXFrom = 0;

            mBotones.Add(btnRemito);
            mBotones.Add(btnPendiente);
            mBotones.Add(btnFactura);
            mBotones.Add(btnTarjeta);
            mBotones.Add(btnListaDeControl);


            ///ANIMACIONES
            sbDatosDelClienteMostrar = this.FindResource("sbDatosDelClienteMostrar") as Storyboard;
            sbDatosDelClienteOcultar = this.FindResource("sbDatosDelClienteOcultar") as Storyboard;
            sbDatosDelClienteOcultar.Completed += (s, o) =>
            {
                gridDatosDelCliente.Visibility = Visibility.Hidden;
            };
            sbListFiltroClientesMostrar = this.FindResource("sbListFiltroClientesMostrar") as Storyboard;
            sbListFiltroClientesOcultar = this.FindResource("sbListFiltroClientesOcultar") as Storyboard;

            sbListFiltroClientesOcultar.Completed += (s, o) =>
            {
                listFiltroClientes.Visibility = Visibility.Hidden;
            };


            ///ANIMACION BOTONES
            botonXTo = (double)(gridBotones.Width - 3);
            //EASING
            easing = new CircleEase();  // or whatever easing class you want
            easing.EasingMode = EasingMode.EaseInOut;
            aniAnchoBtn = new ThicknessAnimation();
            aniAnchoBtn.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            aniAnchoBtn.EasingFunction = easing;

            ///INICIO Y ASIGNACIONES
            listFiltroClientes.Visibility = Visibility.Hidden;
            listFiltroClientes.Margin = new Thickness(tbNombre.Margin.Left + 0, tbNombre.Margin.Top + tbNombre.Height + 2, 0, 0);

            //gridDatosDelClienteOcultar();
            listCaja.ItemsSource = mArticulosCaja;
            gridDatosDelCliente.Visibility = Visibility.Hidden;
            ActualiarCajaDesdeDB();
            cargarListaDeClientes();
        }

        //------------------------------------------------------------------------------------------
        ///-----------------------------------------------------------------------------------------

        ///Funciones DB
        public static void ActualiarCajaDesdeDB()
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
            //listFiltroClientes.ItemsSource = dataTable.DefaultView;

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
                float cantidad = zfun.toFloat(registro["cantidad"].ToString());
                float precio = zfun.toFloat(registro["precio"].ToString());
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
                tempArticulo.codigo = zfun.toLong(registro["codigo"].ToString());
                tempArticulo.descripcion = registro["descripcion"].ToString();
                tempArticulo.cantidad = cantidad.ToString();
                tempArticulo.precio = precio.ToString("0.00");
                tempArticulo.costo = registro["costo"].ToString();
                tempArticulo.subtotal = tmpSubtotal.ToString("0.00");
                tempArticulo.totalmostrar = "";
                if (dataTable.Rows.Count != index + 1)
                {
                    String tmpSiguienteId = dataTable.Rows[index + 1]["idventa"].ToString();
                    //consola(registro["idventa"].ToString() + "-" + tmpSiguienteId);
                    if (registro["idventa"].ToString() != tmpSiguienteId)
                    {
                        tempArticulo.totalmostrar = "$" + tmpTotal.ToString("0.00");
                    }
                }
                else
                {
                    tempArticulo.totalmostrar = tmpTotal.ToString();
                }
                mArticulosCaja.Add(tempArticulo);
                index++;
            }


            ///asigno la lista al control listCaja
            //listCaja.ItemsSource = mArticulosCaja;
            //mArticulosCaja.Reverse();

            ///cerrar conexion
            conexion.Close();
        }
        private void cargarListaDeClientes()
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=clientes.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            string consulta = "select * from clientes";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///borrar todos los elementos de mTotalArticulos
            if (mTotalClientes != null)
            {
                mTotalClientes.Clear();
            }

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {

                itemCliente tempArticulo = new itemCliente();
                tempArticulo.id = (long)registro["id"];
                tempArticulo.nombre = registro["nombre"].ToString();
                tempArticulo.direccion = registro["direccion"].ToString();
                tempArticulo.telefono = registro["telefono"].ToString();
                tempArticulo.cuit = registro["cuit"].ToString();

                mTotalClientes.Add(tempArticulo);
            }


            ///cerrar conexion
            conexion.Close();

            ///asignar datos al list
            listFiltroClientes.ItemsSource = mTotalClientes;

            consola("clientes: " + listFiltroClientes.Items.Count.ToString());

        }
        private void filtroClientes(string filtro = "")
        {
            var articulosFiltrados = from registro in mTotalClientes
                                     where registro.nombre.ToLower().Contains(filtro.ToLower())
                                     select registro;

            //mTotalArticulosConFiltro = null;
            //mTotalArticulosConFiltro = articulosFiltrados.ToList();
            //listFiltro.ItemsSource = mTotalArticulosConFiltro;
            listFiltroClientes.ItemsSource = articulosFiltrados;


        }

        ///funciones varias
        private void ActivarBotonMenu(Button boton = null)
        {
            int selected = 0;
            foreach (Button item in mBotones)
            {
                //string tmpTexto = item.Name;
                if (item.Name == boton.Name)
                {
                    //tmpTexto = item + tmpTexto + " -SELECTED";
                    mBotonSelected = selected;
                }
                else
                {
                    ///animacion en sí
                    aniAnchoBtn.From = mBotones[selected].Margin;
                    aniAnchoBtn.To = new Thickness(aniAnchoBtn.From.Value.Left, aniAnchoBtn.From.Value.Top, botonXTo, aniAnchoBtn.From.Value.Bottom);

                    mBotones[selected].BeginAnimation(Button.MarginProperty, aniAnchoBtn);
                    mBotones[selected].IsEnabled = false;
                }
                //consola(tmpTexto);
                selected++;
            }

            //mBotones[mBotonSelected].Background = App.Current.Resources["confoco2"] as SolidColorBrush;
            mBotones[mBotonSelected].Style = styleBotonSelected;

            labEntrega.Visibility = Visibility.Visible;
            tbEntrega.Visibility = Visibility.Visible;
            labFacturaNro.Visibility = Visibility.Hidden;
            tbFacturaNro.Visibility = Visibility.Hidden;
            if (mBotones[mBotonSelected].Name == "btnFactura")
            {
                labFacturaNro.Visibility = Visibility.Visible;
                tbFacturaNro.Visibility = Visibility.Visible;
                labEntrega.Visibility = Visibility.Hidden;
                tbEntrega.Visibility = Visibility.Hidden;
            }


            gridDatosDelCliente.Visibility = Visibility.Visible;
            tbNombre.Focus();

        }
        private void MostrarBotones()
        {
            mBotones[mBotonSelected].Focus();
            //gridDatosDelCliente.Visibility = Visibility.Hidden;
            gridDatosDelClienteOcultar();

            int selected = 0;
            foreach (Button item in mBotones)
            {
                //mBotones[selected].Background = App.Current.Resources["sinfoco"] as SolidColorBrush;
                //mBotones[selected].Style = null;
                mBotones[selected].Style = styleBoton;

                ///animacion en sí
                aniAnchoBtn.From = mBotones[selected].Margin;
                aniAnchoBtn.To = new Thickness(aniAnchoBtn.From.Value.Left, aniAnchoBtn.From.Value.Top, 0, aniAnchoBtn.From.Value.Bottom);
                mBotones[selected].BeginAnimation(Button.MarginProperty, aniAnchoBtn);

                mBotones[selected].IsEnabled = true;
                selected++;
            }
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
        private bool esDecimal(Key key)
        {
            return zfun.esDecimal(key);
        }
        private void consola(string texto)
        {
            zfun.consola(texto);
        }

        ///CONTROLES
        private void tabCaja_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tab = sender as TabControl;
                int selected = tab.SelectedIndex;
                //consola(e.Source.ToString());

                /// Hacer la animacion sólo si esta seleccionada la pestana CAJA
                if (MainWindow.mPestanaMain == 1)
                {
                    if (selected != -1)
                    {


                        /// animacion
                        gridXFrom = gridCajaSlide.RenderTransform.Value.OffsetX;
                        gridXTo = (double)(mAnchoPantalla * selected * -1);
                        gridXTo += mMargen;
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
                        ta.From = gridCajaSlide.Margin;
                        ta.To = new Thickness(gridXTo, ta.From.Value.Top, ta.From.Value.Right, ta.From.Value.Bottom);
                        ta.Duration = new Duration(TimeSpan.FromSeconds((double)App.Current.Resources["TiempoAnimacion"]));
                        ta.EasingFunction = easing;
                        ta.Completed += (s, e2) =>
                        {
                            ///pestana VENTAS
                            if (selected == 0)
                            {
                                ///FOCO en listCaja
                                if (listCaja.SelectedIndex == -1)
                                {
                                    listCaja.SelectedIndex = 0;
                                }
                                var item = listCaja.ItemContainerGenerator.ContainerFromIndex(listCaja.SelectedIndex) as ListBoxItem;
                                if (item != null)
                                {
                                    item.Focus();
                                }
                            }
                            ///pestana Impresiones
                            if (selected == 1)
                            {
                                ///FOCO en botones
                                //if (ucCaja.listCaja.SelectedIndex == -1)
                                //{
                                //    ucCaja.listCaja.SelectedIndex = 0;
                                //}
                                //var item = ucCaja.listCaja.ItemContainerGenerator.ContainerFromIndex(ucCaja.listCaja.SelectedIndex) as ListBoxItem;
                                //if (item != null)
                                //{
                                //    item.Focus();
                                //}
                                //btnRemito.Focus();

                                /// si el boton tiene el style de selecionado, hace foco en el textbox tbNombre, sino en el boton mBotonSelected
                                if (mBotones[mBotonSelected].Style == styleBotonSelected)
                                {
                                    tbNombre.Focus();
                                }
                                else
                                {
                                    mBotones[mBotonSelected].Focus();
                                }
                            }

                        };

                        gridCajaSlide.BeginAnimation(Grid.MarginProperty, ta);

                    }

                    ///Index Pestana CAJA seleecionada
                    mPestanaCaja = selected;

                    ///color textblock de las pestañas en tabMain
                    //tbTabCajaItem0.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                    //tbTabCajaItem1.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                    //if (selected == 0)
                    //{
                    //    tbTabCajaItem0.Foreground = App.Current.Resources["infocable3"] as SolidColorBrush;
                    //}
                    //if (selected == 1)
                    //{
                    //    tbTabCajaItem1.Foreground = App.Current.Resources["infocable3"] as SolidColorBrush;
                    //}
                }
            }
            e.Handled = true;

        }

        private void Button_GotFocus(object sender, RoutedEventArgs e)
        {
            ayuda(zAyuda.bontonMenu1a, zAyuda.bontonMenu1b);
        }
        private void Button_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var boton = sender as Button;
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                //gridDatosDelCliente.Visibility = Visibility.Visible;
                //tbNombre.Focus();
                ActivarBotonMenu(boton);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                /// volver a la pestana "movimientos"
                tabCaja.SelectedIndex = 0;
            }
            else if (e.Key == Key.Up)
            {
                if (boton.Name == "btnRemito")
                {
                    e.Handled = true;
                    return;
                }
            }
            else if (e.Key == Key.Down)
            {
                if (boton.Name == "btnListaDeControl")
                {
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                e.Handled = true;
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;

            if (boton.Style == styleBoton)
            {
                ActivarBotonMenu(boton);
            }
            else
            {
                MostrarBotones();
            }

            e.Handled = true;
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            string nombre = tb.Name;
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                //if (nombre == "tbNombre")
                //{
                //if (tbCuit.IsVisible)
                //{
                //    tbCuit.Focus();
                //}
                //else
                //{
                //    tbDireccion.Focus();

                //}
                //}
                if (nombre == "tbCuit")
                {
                    if (tbEntrega.IsVisible)
                    {

                        tbEntrega.Focus();
                    }
                    else
                    {
                        btnImprimir.Focus();
                    }

                }
                else if (nombre == "tbDireccion")
                {
                    if (tbCuit.IsVisible)
                    {
                        tbCuit.Focus();
                    }
                    else
                    {

                        if (tbEntrega.IsVisible)
                        {

                            tbEntrega.Focus();
                        }
                        else
                        {
                            btnImprimir.Focus();
                        }
                    }

                }
                else if (nombre == "tbTelefono")
                {
                    tbDireccion.Focus();
                }
                else if (nombre == "tbEntrega")
                {
                    //consola("hola");
                    btnImprimir.Focus();
                }
            }
        }

        private void imgCerrarDatosCliente_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            MostrarBotones();
        }

        private void listCaja_GotFocus(object sender, RoutedEventArgs e)
        {
            ///ayuda(zAyuda.listCaja1);
            MainWindow.ayuda2(zAyuda.listCaja1);
            //MainWindow.consola(zAyuda.listCaja1);

        }
        private void listCaja_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var list = sender as ListView;

                var selected = list.SelectedItem;
                var fila = selected as itemCaja;


                //string codigopro = ((fila.codigo == "" || fila.codigo == null) ? "" : fila.codigo.ToString());
                //string descripcion = fila.descripcion.ToString();
                //string precio = fila.precio.ToString("0.00");

                string fecha = fila.fecha.ToString();
                string idVenta = fila.idventa.ToString();
                string total = fila.totalmostrar.ToString();

                tbFecha.Text = fecha;
                tbIdVenta.Text = idVenta;
                tbTotal.Text = total;

                //tbCodigo.Text = codigopro;
                //seEditoDescripcionDesdeElPrograma = true;
                //tbDescripcion.Text = descripcion;
                //tbPrecio.Text = precio;
                //tbPrecio.Tag = precio;

                //tbCantidad.Tag = fila.costo;

                //tbCantidad.Focus();
                //listFiltroClientes.Visibility = Visibility.Hidden;

                //consola("Venta Nro:" + fila.idventa.ToString());
                //MessageBox.Show("Crear campos (nombre, direccion, cuit, etc. Qu");

                ///consultar mArticulosCaja para filtrar solo los que coinciden con ese nro idVenta
                var listaVentaId = from registro in mArticulosCaja
                                   where registro.idventa.Equals(Int32.Parse(idVenta))
                                   select registro;
                listImpresion.ItemsSource = listaVentaId;


                tabCaja.SelectedIndex = 1;
            }
        }

        private void listFiltroOcultar()
        {
            sbListFiltroClientesOcultar.Begin();
        }
        private void listFiltroClientes_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var list = sender as ListView;
            Boolean mostrar = list.IsVisible;

            if (mostrar)
            {
                sbListFiltroClientesMostrar.Begin();

            }

        }
        private void listFiltroClientes_KeyDown(object sender, KeyEventArgs e)
        {
            var list = sender as ListView;
            var item = list.SelectedItem as itemCliente;
            if (e.Key == Key.Escape)
            {
                tbNombre.Focus();
            }
            else if (e.Key == Key.Enter)
            {

                //string codigo = ((item.codigo == "" || item.codigo == null) ? "" : item.codigo.ToString());
                string id = item.id.ToString();
                string nombre = item.nombre.ToString();
                string direccion = item.direccion.ToString();
                string telefono = item.telefono.ToString();
                string cuit = item.cuit.ToString();

                seEditoDescripcionDesdeElPrograma = true;

                tbIdCliente.Text = id;
                tbNombre.Text = nombre;
                tbDireccion.Text = direccion;
                tbTelefono.Text = telefono;
                tbCuit.Tag = cuit;

                tbTelefono.Focus();
                listFiltroOcultar();
            }
        }

        private void gridDatosDelCliente_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //mBotones[mBotonSelected].Focus();
                ////gridDatosDelCliente.Visibility = Visibility.Hidden;
                //gridDatosDelClienteOcultar();
                MostrarBotones();

            }
        }
        private void gridDatosDelCliente_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var objeto = sender as Grid;
            Boolean mostrar = objeto.IsVisible;
            //consola(list.IsVisible.ToString());

            if (mostrar)
            {
                sbDatosDelClienteMostrar.Begin();

                //} else
                //{
                //    sbDatosDelClienteOcultar.Begin();
            }
        }
        private void gridDatosDelClienteOcultar()
        {
            sbDatosDelClienteOcultar.Begin();
        }

        private void tbNombre_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            ///ENTER
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                tbTelefono.Focus();
                //    ///si esta vacio tbDescipcion, ir a tbPagaCon
                //    if (tbDescripcion.Text.Trim() == "")
                //    {
                //        if (listVenta.Items.Count > 0)
                //        {
                //            tbPagaCon.Focus();
                //        }
                //    }
                //    else
                //    ///si no esta vacio tbDescipcion
                //    {
                //        ///comprobar si es un numero (para buscar articulo por codigo)
                //        if (mBuscarArticuloPorCodigo)
                //        {
                //            buscarArticuloPorCodigo();
                //            e.Handled = true;
                //        }
                //        else
                //        ///si no es un numero pero tampoco se selecciono de la lista, entonces es un ARTICULO NUEVO
                //        {
                //            if (estadoTip(zAyuda.nuevoArticulo))
                //            {
                //                tipArticuloNuevo(true);
                //            }
                //            ///tbCodigo = codigoMax 
                //            tbCodigo.Text = obtenerCodigoArticuloMax().ToString();
                //            tbPrecio.Tag = "";
                //            tbCantidad.Tag = "";

                //            tbCantidad.Text = "1";
                //            tbCantidad.SelectAll();
                //            tbCantidad.Focus();
                //            listFiltroOcultar();

                //        }
                //    }
            }
            ///FLECHA ABAJO
            if (e.Key == Key.Down)
            {
                //listFiltro.Focus();
                if (listFiltroClientes.Visibility == Visibility.Visible)
                {
                    listFiltroClientes.SelectedIndex = 0;
                    var item = listFiltroClientes.ItemContainerGenerator.ContainerFromIndex(listFiltroClientes.SelectedIndex) as ListBoxItem;
                    if (item != null)
                    {
                        item.Focus();
                        //listFiltro.ScrollIntoView(listFiltro.SelectedItem);
                    }
                    e.Handled = true;

                }
            }
            ///FLECHA DERECHA
            if (e.Key == Key.Right)
            {
                //btnNuevo.Focus();
                //e.Handled = true;

            }
            ///ESC
            if (e.Key == Key.Escape)
            {
                //resetTb();
            }
        }
        private void tbNombre_TextChanged(object sender, TextChangedEventArgs e)
        {

            ///si anteriormente se busco un articulo y no se encontro, borrar mensaje al editer tbDescripcion
            //if (labelTip.Content.ToString() == zAyuda.articuloNoEncontrado)
            //{
            //    tip();
            //}


            ///borrar el tbCodigo ya que si se edita la descripcion, deja de ser ESE articulo
            //if (seEditoDescripcionDesdeElPrograma)
            //{
            //    seEditoDescripcionDesdeElPrograma = false;
            //    e.Handled = true;
            //    return;
            //}
            //else
            //{
            //    tbCodigo.Text = "";

            //}

            var textBox = sender as TextBox;
            string filtro = textBox.Text.Trim();
            //consola(listFiltroClientes.Items.Count.ToString());
            if (filtro != "" && textBox.IsFocused)
            {
                ///texto filtro NO esta vacio

                ///si es una letra

                ///aplicar filtro
                //gridFiltroSQL(filtro);
                filtroClientes(filtro);

                ///mostrar tip paa agregar nuevo articulo
                //if (tbDescripcion.IsFocused)
                //{
                //    tip(zAyuda.nuevoArticulo, tbDescripcion);
                //}

                ///mostrar list si hay resultados
                if (listFiltroClientes.Items.Count > 0)
                {
                    listFiltroClientes.Visibility = Visibility.Visible;
                    ayuda(zAyuda.descripcion2a, zAyuda.descripcion2b);
                }
                else
                {
                    ayuda(zAyuda.descripcion3);
                    ///ocultar control si no hay resultados
                    listFiltroOcultar();
                }


            }
            else
            {
                ///texto filtro vacio
                //ayuda(zAyuda.descripcion1);
                //tip();
                /////ocultar control si el text esta vacio
                listFiltroOcultar();
            }
        }



    }
}
