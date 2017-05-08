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
using System.Collections.ObjectModel;

namespace SIV_Servidor
{

    public partial class ucVentas : UserControl
    {
        ///Variables Globales
        List<articuloClass> mArticulos;     //Listado de articulos a la venta filtrados
        public static ObservableCollection<itemVenta> mItemsVenta =
            new ObservableCollection<itemVenta>();   //articulos cargados en la pestana venta actual

        int mPestana;   //pestana venta activa
        bool mBuscarArticuloPorCodigo = false;  //al apretar enter end escripcion, si empieza por un numero, busca el articulo
        bool seEditoDescripcionDesdeElPrograma = false;


        /// animaciones
        Storyboard sbListVentas;
        Storyboard sbListFiltroMostrar;

        /// MAIN
        public ucVentas()
        {
            InitializeComponent();

            ///variables globales, inicializacion
            mArticulos = new List<articuloClass>();
            listVenta.ItemsSource = mItemsVenta;

            ///SETEAR CONTROLES
            ayuda();
            tip();
            listFiltro.Visibility = Visibility.Hidden;
            listFiltro.Margin = new Thickness(tbCodigo.Margin.Left + 2, tbDescripcion.Margin.Top + tbDescripcion.Height + 0 + 2, 0, 0);

            ///animaciones
            sbListVentas = this.FindResource("sbListVentas") as Storyboard;
            sbListFiltroMostrar = this.FindResource("sbListFiltroMostrar") as Storyboard;

            ///FUNCIONES DE INICIO
            seleccionarPestana(0);
            cargarListaDeArticulos();
            calcularTotal();
            tbDescripcion.Focus();


        }

        //------------------------------------------------------------------------------------------
        ///-----------------------------------------------------------------------------------------

        ///FUNCIONES GENERALES
        private void seleccionarPestana(int pestana)
        {
            mPestana = pestana;

            //LTemp.Content = mPestana;
        }
        public void resetTb()
        {
            tbCodigo.Text = "";
            tbDescripcion.Text = "";
            tbCantidad.Text = "";
            tbPrecio.Text = "";
            tbSubtotal.Text = "";
            tip();
            //ayuda();
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
        private void calcularSubtotal()
        {

            tbCantidad.Text = tbCantidad.Text.Replace('.', ',');
            tbPrecio.Text = tbPrecio.Text.Replace('.', ',');

            string resultado = "";
            float precio = 0;
            float cantidad = 0;
            float subtotal = 0;
            float.TryParse(tbCantidad.Text, out cantidad);
            float.TryParse(tbPrecio.Text, out precio);

            subtotal = cantidad * precio;
            resultado = subtotal.ToString();
            //consola(resultado);

            tbSubtotal.Text = subtotal.ToString("0.00");
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

        ///FUNCIONES BD
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
            listFiltro.ItemsSource = mArticulos;

        }
        private void filtroArticulos(string filtro = "")
        {
            var listaTotal = from registro in mArticulos
                             where registro.descripcion.ToLower().Contains(filtro.ToLower())
                             select registro;
            listFiltro.ItemsSource = listaTotal;
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
            mItemsVenta.Add(itemTemp);
            //listVenta.Items.Add(itemTemp);


            ///actualizar listVenta
            //mostrarListVentaX();

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

                //CargarDBCaja();
                ucCaja.ActualiarCajaDesdeDB();

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
            //listVenta.ItemsSource = null;
            //listVenta.ItemsSource = mItemsVenta;

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
            //listVenta.ItemsSource = null;
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

        ///FUNCIONES CAJA


        ///CONTROLES
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //ayuda();
            var textBox = sender as TextBox;
            textBox.Background = App.Current.Resources["confoco"] as SolidColorBrush;

            if (textBox.Name == "tbPrecio")
            {
                ayuda(zAyuda.precio1a, zAyuda.precio1b);
            }
            else if (textBox.Name == "tbDescripcion")
            {
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
                tbDescripcion.Focus();
            }
            if (e.Key == Key.Enter)
            {
                var list = sender as ListView;

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
                    listFiltro.SelectedIndex = 0;
                    var item = listFiltro.ItemContainerGenerator.ContainerFromIndex(listFiltro.SelectedIndex) as ListBoxItem;
                    if (item != null)
                    {
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
            sbListFiltroMostrar.Begin();
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
    }
}
