﻿using System;
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
        public ObservableCollection<itemArticulo>
            mTotalArticulos = new ObservableCollection<itemArticulo>();     //Listado de articulos a la venta filtrados
        //public ObservableCollection<articuloClass>
        //    mTotalArticulosConFiltro = new ObservableCollection<articuloClass>();    //lista de mTotalArticulos filtrados por el campo descripcion
        //public List<articuloClass> mTotalArticulosConFiltro = new List<articuloClass>();
        public static ObservableCollection<itemVenta>
            mItemsListVenta = new ObservableCollection<itemVenta>();   //articulos cargados en la pestana venta actual

        bool mBuscarArticuloPorCodigo = false;  //al apretar enter end escripcion, si empieza por un numero, busca el articulo
        bool seEditoDescripcionDesdeElPrograma = false;
        int mPestana;   //pestana venta activa

        Style tbNoEditable = Application.Current.FindResource("StyleTBNoEditableFondo2") as Style;
        Style tbNoEditableNuevo = Application.Current.FindResource("StyleTbNoEditableNuevo2") as Style;

        /// animaciones
        Storyboard sbListVentas;
        Storyboard sbListFiltroMostrar;
        Storyboard sbListFiltroOcultar;


        /// MAIN
        public ucVentas()
        {
            InitializeComponent();

            ///variables globales, inicializacion
            //mTotalArticulos = new List<articuloClass>();
            //mTotalArticulosConFiltro = new List<articuloClass>();
            listVenta.ItemsSource = mItemsListVenta;

            ///SETEAR CONTROLES
            ayuda();
            tip();
            listFiltro.Visibility = Visibility.Hidden;
            listFiltro.Margin = new Thickness(tbCodigo.Margin.Left + 2, tbDescripcion.Margin.Top + tbDescripcion.Height + 0 + 2, 0, 0);

            ///animaciones
            sbListVentas = this.FindResource("sbListVentas") as Storyboard;
            sbListFiltroMostrar = this.FindResource("sbListFiltroMostrar") as Storyboard;
            sbListFiltroOcultar = this.FindResource("sbListFiltroOcultar") as Storyboard;

            sbListFiltroOcultar.Completed += (s, o) =>
            {
                listFiltro.Visibility = Visibility.Hidden;
            };

            ///FUNCIONES DE INICIO
            seleccionarPestana(0);
            cargarListaDeArticulos();
            calcularTotal();
            resetTb();
            //tbDescripcion.Focus();

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
            tbCodigo.Tag = "";
            tbCodigo.Text = "";
            tbDescripcion.Text = "";
            tbCantidad.Text = "";
            tbCantidad.Tag = "";
            tbPrecio.Text = "";
            tbPrecio.Tag = "";
            tbSubtotal.Text = "";
            tip();
            tipArticuloNuevo();
            tbDescripcion.Focus();
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
                    //labelTip.Margin = new Thickness(control.Margin.Left + 2, control.Margin.Top + control.Height + 2, 0, 0);
                    ///si es tbDescripcion, acomodo arriba del control, sino abajo
                    if (control.Name == "tbDescripcion")
                    {
                        labelTip.Margin = new Thickness(control.Margin.Left + 135, control.Margin.Top - 16, 0, 0);
                    }
                    else
                    {
                        labelTip.Margin = new Thickness(control.Margin.Left + 2, control.Margin.Top + control.Height + 2, 0, 0);
                    }
                }
                labelTip.Visibility = Visibility.Visible;
            }
            //Console.WriteLine(texto);

        }
        private void tipArticuloNuevo(bool mostrar = false)
        {
            if (!mostrar)
            {
                labelTip2.Visibility = Visibility.Hidden;
                tbCodigo.Style = tbNoEditable;
            }
            else
            {
                ///mostrar tip2=articulo nuevo
                TextBox control = tbCodigo;
                labelTip2.Content = zAyuda.tipArticuloNuevo;
                labelTip2.Margin = new Thickness(control.Margin.Left + 2, control.Margin.Top + control.Height + 2, 0, 0);
                labelTip2.Visibility = Visibility.Visible;

                ///tbCodigo con style diferente
                tbCodigo.Style = tbNoEditableNuevo;
                //tbCodigo.Text = obtenerCodigoArticuloMax().ToString();
            }
        }
        private bool EsArticuloNuevo()
        {
            bool respuesta = false;
            if (labelTip2.Visibility == Visibility.Visible)
            {
                respuesta = true;
            }
            return respuesta;
        }
        private bool estadoTip(string texto)
        {
            if (labelTip.Content.ToString() == texto && labelTip.Visibility == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void calcularSubtotal()
        {

            tbCantidad.Text = tbCantidad.Text.Replace('.', ',');
            //tbPrecio.Text = tbPrecio.Text.Replace('.', ',');
            //tbPrecio.CaretIndex = tbPrecio.Text.Length;   //poner el cursor al final

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

            ///borrar todos los elementos de mTotalArticulos
            if (mTotalArticulos != null)
            {
                mTotalArticulos.Clear();
            }

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {

                itemArticulo tempArticulo = new itemArticulo();
                tempArticulo.id = (long)registro["id"];
                tempArticulo.codigo = (long)registro["codigo"];
                tempArticulo.codigopro = registro["codigopro"].ToString();
                tempArticulo.descripcion = registro["descripcion"].ToString();

                float precio = toFloat(registro["precio"].ToString());
                tempArticulo.precio = precio;

                //tempArticulo.precio= string.Format("0.00", registro["precio"].ToString());
                //tempArticulo.precio = registro["precio"].ToString();

                float costo = toFloat(registro["costo"].ToString());
                tempArticulo.costo = costo;
                tempArticulo.tags = registro["tags"].ToString();
                tempArticulo.stock = registro["stock"].ToString();
                //tempArticulo.stock = "1";

                mTotalArticulos.Add(tempArticulo);
            }


            ///cerrar conexion
            conexion.Close();

            ///asignar datos al list
            listFiltro.ItemsSource = mTotalArticulos;

        }
        private void filtroArticulos(string filtro = "")
        {
            var articulosFiltrados = from registro in mTotalArticulos
                                     where registro.descripcion.ToLower().Contains(filtro.ToLower())
                                     orderby registro.descripcion
                                     select registro;

            //mTotalArticulosConFiltro = null;
            //mTotalArticulosConFiltro = articulosFiltrados.ToList();
            //listFiltro.ItemsSource = mTotalArticulosConFiltro;
            listFiltro.ItemsSource = articulosFiltrados;


        }

        private long obtenerIdParaListVenta()
        {
            string tabla = "listventa" + mPestana.ToString().Trim();

            int resultado = zdb.valorMaxDB("caja.db", tabla, "id");
            if (resultado != -1)
            {
                resultado++;
            }
            return resultado;
        }
        private int obtenerIdVentaMax()
        {
            int resultado = zdb.valorMaxDB("caja.db", "caja", "idventa");
            if (resultado != -1)
            {
                resultado++;
            }
            return resultado;
        }
        private int obtenerCodigoArticuloMax()
        {
            int resultado = zdb.valorMaxDB("articulos.db", "articulos", "codigo");
            if (resultado != -1)
            {
                resultado++;
            }
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

            precio = toFloat(precio).ToString();

            if (tbCantidad.Tag != null)
                costo = tbCantidad.Tag.ToString();


            long id = obtenerIdParaListVenta();
            if (id == -1)
            {
                consola("Error al intentar obtener IdParaListVenta");
                return;
            }

            ///crear itemVenta
            itemVenta itemTemp = new itemVenta
            {
                id = id,

                codigo = zfun.toLong(codigo),
                descripcion = descripcion,
                cantidad = cantidad,
                precio = precio,
                subtotal = subtotal,
                costo = costo
            };

            ///agregar item a la tabla temporal
            string tabla = "listventa" + mPestana.ToString().Trim();
            insertarItemVentaEnDB(itemTemp, tabla);

            ///agregar item al listbox
            mItemsListVenta.Add(itemTemp);
            //listVenta.Items.Add(itemTemp);


            ///actualizar listVenta
            //mostrarListVentaX();

            ///Calcular total
            calcularTotal();

            tbDescripcion.Focus();
        }
        private void insertarItemVentaEnDB(itemVenta item, string tabla, int idMax = -1)
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
        private void insertarItemArticuloEnDB(itemArticulo item)
        {
            string archivo = "articulos.db";
            string tabla = "articulos";

            ///abrir conexion DB
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivo + ";Version=3;New=False;Compress=True;");
            conexion.Open();

            ///comando SQL a ejecutar
            SQLiteCommand insertSQL;
            insertSQL = new SQLiteCommand("INSERT INTO " + tabla + " (proveedor, codigopro, codigo, descripcion, precio, costo, fechacreacion) VALUES (?,?,?,?,?,?,DATETIME('NOW'))", conexion);

            insertSQL.Parameters.AddWithValue("proveedor", item.proveedor.ToString());
            insertSQL.Parameters.AddWithValue("codigopro", item.codigopro.ToString());
            insertSQL.Parameters.AddWithValue("codigo", item.codigo);
            insertSQL.Parameters.AddWithValue("descripcion", item.descripcion);
            insertSQL.Parameters.AddWithValue("precio", item.precio);
            insertSQL.Parameters.AddWithValue("costo", item.costo);

            ///ejecutar comando SQL
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
                int idVentaMax = obtenerIdVentaMax();

                ///recorrer la lista (listVenta) e ir asentando fila por fila
                foreach (itemVenta item in listVenta.Items)
                {
                    //insertarItemVentaEnDB(item, "caja", conexion, idVentaMax);
                    insertarItemVentaEnDB(item, "caja", idVentaMax);
                }

                ucCaja.ActualiarCajaDesdeDB();

                ///resetear list y recalcular valores
                resetListVenta();
                calcularTotal();
                resetTb();

            }
            else
            {
                consola("No hay articulos en la lista.");
                tbDescripcion.Focus();
            }

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

            ///borrar todos los elementos de mItemsListVenta
            if (mItemsListVenta != null)
            {
                mItemsListVenta.Clear();

            }

            //consola("Registros: " + dataTable.Rows.Count.ToString());

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {
                int id = -1;
                Int32.TryParse(registro["id"].ToString(), out id);
                if (id == -1)
                {
                    consola("Error en campo ID al cargar registro (descripcion=" + registro["descripcion"].ToString() + ")");
                    return;
                }
                itemVenta tempItem = new itemVenta();
                tempItem.id = id;
                tempItem.codigo = zfun.toLong(registro["codigo"].ToString());
                tempItem.descripcion = registro["descripcion"].ToString();
                tempItem.cantidad = registro["cantidad"].ToString();
                tempItem.precio = registro["precio"].ToString();
                tempItem.subtotal = registro["subtotal"].ToString();
                tempItem.costo = registro["costo"].ToString();
                //tempArticulo.stock = "1";

                mItemsListVenta.Add(tempItem);
            }

            ///cerrar conexion
            conexion.Close();

            ///asignar datos al list
            //listVenta.ItemsSource = null;
            //listVenta.ItemsSource = mItemsListVenta;

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
            if (mItemsListVenta != null)
            {
                mItemsListVenta.Clear();

            }

            ///des-asignar listVenta a datos
            //listVenta.ItemsSource = null;
        }
        private void buscarArticuloPorCodigo()
        {
            string codigoTemp = tbDescripcion.Text.Trim();
            var listaTotal = from registro in mTotalArticulos
                             where registro.codigo.ToString().Trim().Equals(codigoTemp)
                             select registro;

            int articulosEncontrados = listaTotal.Count();

            ///si encuentra 1 articulo con codigo=codigoTemp, lo muestra, sino sale mensaje de art no encontrado
            if (articulosEncontrados < 1)
            {
                tip("Artículo no encontrado", tbDescripcion);
            }
            else
            {
                itemArticulo articulo;
                articulo = listaTotal.ElementAt(0);

                //consola("Res: " + tempArticulo.descripcion.ToString());
                //string codigo = ((articulo.codigo == "" || articulo.codigo == null) ? "" : articulo.codigo.ToString());
                string codigo = articulo.codigo.ToString();
                string descripcion = articulo.descripcion.ToString();

                tbCodigo.Text = codigo;
                tbCodigo.Tag = articulo.id;
                seEditoDescripcionDesdeElPrograma = true;
                tbDescripcion.Text = descripcion;

                string precio = articulo.precio.ToString("0.00");
                //string precio = string.Format("0.00", articulo.precio);
                tbPrecio.Text = precio;
                tbPrecio.Tag = precio;

                tbCantidad.Tag = articulo.costo;

                tbCantidad.Text = "1";
                tbCantidad.SelectAll();
                tbCantidad.Focus();
                listFiltroOcultar();

            }


        }




        ///CONTROLES
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //ayuda();
            var textBox = sender as TextBox;
            textBox.Background = App.Current.Resources["confoco"] as SolidColorBrush;

            ///tbPRECIO
            if (textBox.Name == "tbPrecio")
            {
                ///si es un articulo nuevo (se hizo enter desde tbDescripcion y no desde listFiltro ni se cargo un nro de codigo)
                if (EsArticuloNuevo())
                {
                    ayuda(zAyuda.precio3AgregarArticulo);
                    tip(zAyuda.tipAgregarArticulo, tbPrecio);
                }
                else
                ///si no es un articulo nuevo
                {
                    ayuda(zAyuda.precio1a, zAyuda.precio1b);

                }
            }
            ///tbDESCRIPCION
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
                            //listFiltroOcultar();
                        }
                    }
                }
                else
                {
                    ///si esta vacio el texto del filtro
                    ayuda(zAyuda.descripcion1);
                    ///ocultar control si el text esta vacio
                    //listFiltroOcultar();
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
            var list = sender as ListView;
            var item = list.SelectedItem as itemArticulo;
            if (e.Key == Key.Escape)
            {
                tbDescripcion.Focus();
            }
            else if (e.Key == Key.Enter)
            {

                //string codigo = ((item.codigo == "" || item.codigo == null) ? "" : item.codigo.ToString());
                string codigo = item.codigo.ToString();
                string descripcion = item.descripcion.ToString();
                string precio = item.precio.ToString("0.00");
                //string precio = string.Format("0.00", item.precio);

                tbCodigo.Text = codigo;
                tbCodigo.Tag = item.id;
                seEditoDescripcionDesdeElPrograma = true;
                tbDescripcion.Text = descripcion;
                tbPrecio.Text = precio;
                tbPrecio.Tag = precio;
                //consola("costo:" + fila.costo.ToString());
                tbCantidad.Tag = item.costo;
                //consola("tbPrecio.Tag:"+tbPrecio.Tag.ToString());

                tbCantidad.Text = "1";
                tbCantidad.SelectAll();
                tbCantidad.Focus();
                listFiltroOcultar();
            }
            else if (e.Key == Key.Delete)
            {
                if (item != null)
                {
                    string mensaje = "¿Está seguro de eliminar:\n\n";
                    mensaje = mensaje + item.descripcion.ToUpper() + " (cód:" + item.codigo + ")\n\n";
                    mensaje = mensaje + "de la Base de Datos?";
                    MessageBoxResult result = MessageBox.Show(mensaje, "Eliminar Artículo", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            string archivoDB = "articulos.db";
                            string tabla = "articulos";
                            long index = item.id;
                            ///eliminar de la BD, de la lista principal y de la lista filtrada
                            zdb.EliminarRegistroDB(archivoDB, tabla, index);
                            mTotalArticulos.Remove(item);
                            //mTotalArticulosConFiltro.Remove(item);

                            ///volver a filtrar y dar el foco a descripcion
                            string filtro = tbDescripcion.Text.Trim();
                            filtroArticulos(filtro);
                            tbDescripcion.Focus();

                            consola("Se elimino el item:" + index.ToString());

                            break;
                        case MessageBoxResult.No:
                            break;
                    }


                }
                else
                {
                    consola("no hay ningun item seleccionado para borrar");
                }
            }
        }
        private void listFiltro_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var list = sender as ListView;
            Boolean mostrar = list.IsVisible;
            //consola(list.IsVisible.ToString());

            if (mostrar)
            {
                sbListFiltroMostrar.Begin();

            }
            //else
            //{
            //    sbListFiltroOcultar.Begin();
            //}

        }
        private void listFiltro_GotFocus(object sender, RoutedEventArgs e)
        {
            ayuda(zAyuda.listFiltro1a, zAyuda.listFiltro1b);

        }
        private void listFiltro_LostFocus(object sender, RoutedEventArgs e)
        {
        }
        private void listFiltroOcultar()
        {
            sbListFiltroOcultar.Begin();
        }

        private void listVenta_GotFocus(object sender, RoutedEventArgs e)
        {
            ayuda(zAyuda.listVenta1);
        }
        private void listVenta_KeyDown(object sender, KeyEventArgs e)
        {
            var list = sender as ListView;
            var item = list.SelectedItem as itemVenta;
            if (e.Key == Key.Delete)
            {
                if (item != null)
                {
                    string archivoDB = "caja.db";
                    string tabla = "listventa" + mPestana.ToString().Trim();
                    long index = item.id;
                    zdb.EliminarRegistroDB(archivoDB, tabla, index);
                    mItemsListVenta.Remove(item);
                    consola("Se elimino el item:" + index.ToString());
                }
                else
                {
                    consola("no hay ningun item seleccionado para borrar");
                }
            }
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
                return;
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
                        listFiltroOcultar();
                    }

                }
            }
            else
            {
                ///texto filtro vacio
                ayuda(zAyuda.descripcion1);
                tip();
                ///ocultar control si el text esta vacio
                listFiltroOcultar();
            }




        }
        private void tbDescripcion_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ///ENTER
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
                ///si no esta vacio tbDescipcion
                {
                    ///comprobar si es un numero (para buscar articulo por codigo)
                    if (mBuscarArticuloPorCodigo)
                    {
                        buscarArticuloPorCodigo();
                        e.Handled = true;
                    }
                    else
                    ///si no es un numero pero tampoco se selecciono de la lista, entonces es un ARTICULO NUEVO
                    {
                        if (estadoTip(zAyuda.nuevoArticulo))
                        {
                            tipArticuloNuevo(true);
                        }
                        ///tbCodigo = codigoMax 
                        tbCodigo.Text = obtenerCodigoArticuloMax().ToString();
                        tbPrecio.Tag = "";
                        tbCantidad.Tag = "";

                        tbCantidad.Text = "1";
                        tbCantidad.SelectAll();
                        tbCantidad.Focus();
                        listFiltroOcultar();

                    }
                }
            }
            ///FLECHA ABAJO
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
            ///FLECHA DERECHA
            if (e.Key == Key.Right)
            {
                //btnNuevo.Focus();
                //e.Handled = true;

            }
            ///ESC
            if (e.Key == Key.Escape)
            {
                resetTb();
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
                //tbDescripcion.Focus();
            }

        }

        private void tbPrecio_TextChanged(object sender, TextChangedEventArgs e)
        {
            //consola("textchanged");
            tbPrecio.Text = tbPrecio.Text.Replace('.', ',');


            tbPrecio.CaretIndex = tbPrecio.Text.Length;   //poner el cursor al final

            var textBox = sender as TextBox;
            if (textBox.IsFocused)
            {
                ///si no es un articulo nuevo
                if (!EsArticuloNuevo())
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
                        tip(zAyuda.tipCambiarPrecio, sender);
                    }
                }
            }
            calcularSubtotal();

        }
        private void tbPrecio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //consola("previewtextinput");

            ///solo acepta digitos y coma
            //fsoreach (char ch in e.Text)
            //{
            //    if (!((Char.IsDigit(ch) || ch.Equals(','))))
            //    {
            //        consola("caracter no valido:" + ch.ToString());
            //        e.Handled = true;
            //        break;
            //    }

            //}

            //var textbox = sender as TextBox;
            //int comas = 0;
            //foreach (char ch in textbox.Text)
            //{

            //        consola("caracter:" + ch.ToString());
            //    if (ch.Equals(',') || ch.Equals('.'))
            //    {
            //        comas++;
            //        //consola(comas.ToString());
            //        //if (comas > 0)
            //        //{
            //        //    consola("Ya existe un separador decimal (1 coma).");
            //        //    e.Handled = true;
            //        //    break;
            //        //}
            //    }

            //}
        }
        private void tbPrecio_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ///Si no es una tecla 'decimal' que ignore
            if (esDecimal(e.Key) == false)
            {
                e.Handled = true;
            }


            ///evitar que haya mas de 1 coma
            if ((e.Key == Key.OemPeriod) || (e.Key == Key.Decimal) || (e.Key == Key.OemComma))
            {
                int comas = 0;
                consola("--");
                foreach (char ch in tbPrecio.Text)
                {
                    consola("caracter:" + ch.ToString());
                    if (ch.Equals(',') || ch.Equals('.'))
                    {
                        comas++;
                    }

                }
                consola(comas.ToString());
                if (comas > 0)
                {
                    //consola("Ya existe un separador decimal (1 coma).");
                    e.Handled = true;
                    //break;
                }
            }
            ///ENTER
            if (e.Key == Key.Enter)
            {
                ///si los campos descripcion, codigo o precio estan vacios, q ignore
                if (tbDescripcion.Text=="" || tbCodigo.Text=="" || tbPrecio.Text=="")
                {
                    e.Handled = true;
                    return;
                }
                ///si se modifico el precio, ACTUALIZAR en la BD y en las mListas
                if (estadoTip(zAyuda.tipCambiarPrecio))
                {
                    //consola("cambiar precio");
                    string archivo = "articulos.db";
                    string tabla = "articulos";
                    string index = tbCodigo.Tag.ToString();
                    string campo = "precio";
                    //string valor = toFloat(tbPrecio.Text).ToString();
                    string valor = zfun.toMoneda(tbPrecio.Text);

                    ///ACTUALIZAR EN BD
                    zdb.modificarRegistroDB(archivo, tabla, index, campo, valor, true);

                    ///ACTUALIZAR EN mTotalArticulos
                    long itemId = -1;
                    long.TryParse(index, out itemId);
                    if (itemId != -1)
                    {

                        var item = mTotalArticulos.First(i => i.id == itemId);
                        if (item != null)
                        {
                            item.precio = zfun.toFloat(tbPrecio.Text);
                        }
                        else
                        {
                            consola("Error al querer leer el ID del articulo. No se encuentra el item con id=" + itemId.ToString());
                        }

                    }
                    else
                    {
                        consola("Error al querer leer el ID del articulo");
                    }
                }
                ///Si es un articulo NUEVO, agregar a BD de articulos
                if (EsArticuloNuevo())
                {
                    ///item temporal
                    itemArticulo item = new itemArticulo();
                    item.proveedor = "interno";
                    item.codigopro = "";
                    item.codigo = zfun.toLong(tbCodigo.Text);
                    item.descripcion = tbDescripcion.Text.Trim();
                    item.precio = toFloat(tbPrecio.Text);
                    item.costo = 0;

                    insertarItemArticuloEnDB(item);
                    mTotalArticulos.Add(item);
                }

                agregarItemALista();
                resetTb();
                //tbDescripcion.Focus();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                tbCantidad.SelectAll();
                tbCantidad.Focus();
            }
            else if (e.Key == Key.Up)
            {
                resetTb();
                //tbDescripcion.Focus();

            }

            if (e.Key == Key.Escape)
            {
                ///si se modifico el precio, volver a mostrar el original
                if (estadoTip(zAyuda.tipCambiarPrecio))
                {
                    tbPrecio.Text = tbPrecio.Tag.ToString();
                    tip();
                    e.Handled = true;
                }
                else
                {
                    resetTb();
                    //tbDescripcion.Focus();
                }
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

    }
}
