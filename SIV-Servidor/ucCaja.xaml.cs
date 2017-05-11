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

        ///MAIN
        public ucCaja()
        {
            InitializeComponent();

            mAnchoPantalla = 980; // gridCajaMain.Width;
            //gridCajaSlide.Width = 970; //mAnchoPantalla - (mMargen * 2);
            gridCajaSlide.Margin = new Thickness(gridCajaSlide.Margin.Left, gridCajaSlide.Margin.Top , gridCajaSlide.Margin.Bottom, 0);
            gridXTo = -500;
            gridXFrom = 0;

            listCaja.ItemsSource = mArticulosCaja;

            ActualiarCajaDesdeDB();
        }

        //------------------------------------------------------------------------------------------
        ///-----------------------------------------------------------------------------------------

        ///CONTROLES
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

                string fecha= fila.fecha.ToString();
                string idVenta=fila.idventa.ToString();
                string total= fila.totalmostrar.ToString();

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
                //listFiltro.Visibility = Visibility.Hidden;

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




        ///Funciones caja
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
                float cantidad = funciones.toFloat(registro["cantidad"].ToString());
                float precio = funciones.toFloat(registro["precio"].ToString());
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


        ///extensiones
        private void ayuda(string texto = "", string texto2 = "")
        {
            MainWindow.ayuda2(texto, texto2);
        }
        private float toFloat(string cadena)
        {
            return funciones.toFloat(cadena);
        }
        private bool esDecimal(Key key)
        {
            return funciones.esDecimal(key);
        }
        private void consola(string texto)
        {
            funciones.consola(texto);
        }

        private void tabCaja_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tab = sender as TabControl;
                int selected = tab.SelectedIndex;
                //consola(e.Source.ToString());


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
                ta.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                ta.EasingFunction = easing;
                ta.Completed += (s, e2) =>
                {
                    ///pestana VENTAS
                    if (selected == 0)
                    {
                        ///FOCO en tbDescripcion
                        //ucVentas.tbDescripcion.Focus();
                    }
                    ///pestana Impresiones
                    if (selected == 1)
                    {
                        ///FOCO en listCaja
                        //if (ucCaja.listCaja.SelectedIndex == -1)
                        //{
                        //    ucCaja.listCaja.SelectedIndex = 0;
                        //}
                        //var item = ucCaja.listCaja.ItemContainerGenerator.ContainerFromIndex(ucCaja.listCaja.SelectedIndex) as ListBoxItem;
                        //if (item != null)
                        //{
                        //    item.Focus();
                        //}
                    }

                };

                //dont need to use story board but if you want pause,stop etc use story board
                gridCajaSlide.BeginAnimation(Grid.MarginProperty, ta);

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
            e.Handled = true;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
