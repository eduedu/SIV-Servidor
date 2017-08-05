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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SIV_Servidor
{

    public partial class ucConsultas : UserControl
    {
        ///Globales 
        public static ObservableCollection<itemListConsultas> mItemsConsulta =
            new ObservableCollection<itemListConsultas>();   //Lista los items en listConsultas
        public static ObservableCollection<itemListDetalles> mItemsDetalle =
            new ObservableCollection<itemListDetalles>();   //Lista los items en listDetalles

        public static bool actualizarListConsultas = true;

        public string mTipoDeConsulta = "pendientes";
        Storyboard sbListConsultas;
        Storyboard sbListDetalles;

        ///animacion
        Storyboard sbMyInputBox_Show;


        ///MAIN
        public ucConsultas()
        {
            InitializeComponent();

            ///animaciones
            sbListConsultas = this.FindResource("sbListConsultas") as Storyboard;
            sbListDetalles = this.FindResource("sbListDetalles") as Storyboard;

            ///myInputBox
            sbMyInputBox_Show = this.FindResource("sbMyInputBox_Show") as Storyboard;
            sbMyInputBox_Show.Completed += (s, o) =>
            {
                myInputBox.Visibility = Visibility.Visible;
                myInputBox_Texto.Focus();
            };

            ///INICIO Y ASIGNACIONES
            resetTextos();
            listConsultas.ItemsSource = mItemsConsulta;
            listDetalles.ItemsSource = mItemsDetalle;

            //listPendientes.ItemsSource = mItemsConsulta;
            listNombres.ItemsSource = mItemsConsulta;


            gridPendientes.Margin = new Thickness(gridRemitosyfacturas.Margin.Left, gridRemitosyfacturas.Margin.Top, 0, 0);
            ActualizarListConsultasDesdeDB();
        }

        ///--------------------------------------------------------------------------------------------------------

        ///FUNCIONES VARIAS
        private void resetTextos()
        {
            labId.Content = "";
            labFecha.Content = "";
            labNombre.Content = "";
            labDireccion.Content = "";
            labTelefono.Content = "";
            labCuit.Content = "";

            labTotal.Content = "$ 0,00";

            mItemsDetalle.Clear();

            habilitarBotones(false);
            listPendientes.ItemsSource = null;
        }
        private void habilitarBotones(bool habilitar)
        {
            if (habilitar)
            {
                //btnImprimir.Opacity = 1.0;
                //btnImprimir.IsEnabled = true;
                btnPagar.Opacity = 1.0;
                btnPagar.IsEnabled = true;
            }
            else
            {
                btnImprimir.Opacity = 0.3;
                btnImprimir.IsEnabled = false;
                btnPagar.Opacity = 0.3;
                btnPagar.IsEnabled = false;

            }
        }

        ///FUNCIONES BD
        public void ActualizarListConsultasDesdeDB()
        {
            ///definicion de parametros 
            string archivo = "";
            string tabla = "";
            string campoNro = "";

            ///asignacion de valores segun el tipo de consulta (mTipoDeConsulta) seleccionado

            //mTipoDeConsulta = "remitos";
            string consulta = "";
            //consulta = "select * from " + tabla + " ORDER BY id DESC";

            if (mTipoDeConsulta == "pendientes")
            {
                archivo = "pendientes.db";
                tabla = "pendientes";
                campoNro = "pendientenro";
                consulta = "select id, pendientenro, datetime(fecha), codigo, descripcion, cantidad, precio, subtotal, direccion, telefono from "
                    + tabla + " ORDER BY id DESC";
            }
            if (mTipoDeConsulta == "remitos")
            {
                archivo = "remitos.db";
                tabla = "remitos";
                campoNro = "remitonro";
                consulta = "select id, remitonro, datetime(fecha), codigo, descripcion, cantidad, precio, subtotal, direccion, telefono from "
                    + tabla + " ORDER BY id DESC";

            }
            if (mTipoDeConsulta == "facturas")
            {
                archivo = "facturas.db";
                tabla = "facturas";
                campoNro = "facturanro";
                consulta = "select id, facturanro, datetime(fecha), codigo, descripcion, cantidad, precio, subtotal, direccion, telefono, cuit from "
                    + tabla + " ORDER BY id DESC";
            }


            ///establecer conexion SQL
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivo + ";Version=3;New=False;Compress=True;");
            conexion.Open();


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

            ///borrar todos los elementos de mItemsConsulta
            if (mItemsConsulta != null)
            {
                mItemsConsulta.Clear();
            }

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {
                long codigo = (long)registro["codigo"];

                if (codigo == -100)
                {
                    itemListConsultas tempItem = new itemListConsultas();

                    tempItem.nro = zfun.toLong(registro[campoNro].ToString());
                    tempItem.fecha = zfun.toFechaMostrar(registro[2].ToString());
                    tempItem.nombre = registro["descripcion"].ToString();
                    tempItem.direccion = registro["direccion"].ToString();
                    tempItem.telefono = registro["telefono"].ToString();

                    ///si es consulta 'facturas'
                    if (mTipoDeConsulta == "facturas")
                    {
                        tempItem.cuit = registro["cuit"].ToString();
                    }
                    else
                    {
                        tempItem.cuit = "";
                    }

                    ///si es consulta 'pendientes'
                    if (mTipoDeConsulta == "pendientes")
                    {
                        ///calcular el saldo consultando en la base de datos los pendientes con el mismo numero
                        object tmpSaldo;
                        tmpSaldo = dataTable.Compute("Sum(subtotal)", "pendientenro = " + tempItem.nro.ToString() + " AND codigo<>-100");
                        tempItem.saldo = zfun.toFloat(tmpSaldo.ToString());

                        //tempItem.saldo = zfun.toFloat(registro["subtotal"].ToString());
                    }
                    else
                    {
                        tempItem.saldo = 0;
                    }


                    tempItem.total = zfun.toFloat(registro["subtotal"].ToString());


                    mItemsConsulta.Add(tempItem);

                }
            }


            ///asigno la lista al control listCaja
            //listCaja.ItemsSource = mArticulosCaja;
            //mArticulosCaja.Reverse();

            ///cerrar conexion
            conexion.Close();

            ///si es un pendiente, dar el foco al filtro, sino a la lista consulta
            if (mTipoDeConsulta == "pendientes")
            {
                tbFiltrar.Focus();
            }
            else
            {
                ///seleccionar el primer item (o ninguno) de listConsultas
                if (mItemsConsulta.Count > 0)
                {
                    listConsultas.SelectedIndex = 0;
                }
                else
                {
                    listConsultas.SelectedIndex = -1;

                }
            }

            ActualizarListDetallesDesdeDB();
            //actualizarListConsultas = false;

        }
        private void ActualizarListDetallesDesdeDB()
        {
            ///definicion de parametros 
            string archivo = "";
            string tabla = "";
            string campoNro = "";
            string id = labId.Content.ToString().Trim();
            string consulta = "";

            ///Si la ID no está vacia, buscar los registros con esa ID
            if (id != "")
            {
                ///asignacion de valores segun el tipo de consulta (mTipoDeConsulta) seleccionado
                if (mTipoDeConsulta == "pendientes")
                {
                    archivo = "pendientes.db";
                    tabla = "pendientes";
                    campoNro = "pendientenro";
                    consulta = "select id, pendientenro, datetime(fecha), codigo, descripcion, cantidad, precio, subtotal, direccion, telefono from "
                        + tabla + " WHERE " + campoNro + " = " + id + " ORDER BY id DESC";
                }
                if (mTipoDeConsulta == "remitos")
                {
                    archivo = "remitos.db";
                    tabla = "remitos";
                    campoNro = "remitonro";
                    consulta = "select id, remitonro, datetime(fecha), codigo, descripcion, cantidad, precio, subtotal, direccion, telefono from "
                        + tabla + " WHERE " + campoNro + " = " + id + " ORDER BY id DESC";

                }
                if (mTipoDeConsulta == "facturas")
                {
                    archivo = "facturas.db";
                    tabla = "facturas";
                    campoNro = "facturanro";
                    consulta = "select id, facturanro, datetime(fecha), codigo, descripcion, cantidad, precio, subtotal, direccion, telefono, cuit from "
                        + tabla + " WHERE " + campoNro + " = " + id + " ORDER BY id DESC";

                }
                //string consulta = "select * from " + tabla + " WHERE " + campoNro + " = " + id + " ORDER BY id DESC";


                ///establecer conexion SQL
                SQLiteConnection conexion;
                conexion = new SQLiteConnection("Data Source=" + archivo + ";Version=3;New=False;Compress=True;");
                conexion.Open();


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

                ///borrar todos los elementos de mItemsConsulta
                if (mItemsDetalle != null)
                {
                    mItemsDetalle.Clear();
                }

                ///Loop por todos los registros de la tabla
                foreach (DataRow registro in dataTable.Rows)
                {
                    long codigo = (long)registro["codigo"];

                    if (codigo != -100)
                    {
                        itemListDetalles tempItem = new itemListDetalles();

                        tempItem.nro = zfun.toLong(registro[campoNro].ToString());
                        tempItem.fecha = zfun.toFechaMostrar(registro[2].ToString());
                        tempItem.codigo = zfun.toLong(registro["codigo"].ToString());
                        tempItem.descripcion = registro["descripcion"].ToString();
                        tempItem.cantidad = zfun.toLong(registro["cantidad"].ToString());
                        tempItem.precio = zfun.toFloat(registro["precio"].ToString());
                        tempItem.subtotal = zfun.toFloat(registro["subtotal"].ToString());


                        ///color
                        tempItem.color = 0;
                        if (codigo == -99)
                            tempItem.color = 1;

                        mItemsDetalle.Add(tempItem);

                    }
                }


                ///asigno la lista al control listCaja
                //listCaja.ItemsSource = mArticulosCaja;
                //mArticulosCaja.Reverse();

                ///cerrar conexion
                conexion.Close();

                ///seleccionar el primer item (o ninguno) de listConsultas
                if (mItemsConsulta.Count > 0)
                {
                    listDetalles.SelectedIndex = 0;
                }
                else
                {
                    listDetalles.SelectedIndex = -1;

                }
            }
            //else
            //{
            //    resetTextos();
            //}
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

        /// CONTROLES
        private void tabConsultas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tab = sender as TabControl;
                int selected = tab.SelectedIndex;

                if (selected == 0)
                {
                    mTipoDeConsulta = "pendientes";
                    labIdTexto.Content = "Nro de Pend.";
                    labTotalTexto.Content = "Saldo:";
                    gridPendientes.Visibility = Visibility.Visible;
                    gridRemitosyfacturas.Visibility = Visibility.Hidden;
                }
                if (selected == 1)
                {
                    mTipoDeConsulta = "remitos";
                    labIdTexto.Content = "Nro de Remito";
                    labTotalTexto.Content = "Total:";
                    gridPendientes.Visibility = Visibility.Hidden;
                    gridRemitosyfacturas.Visibility = Visibility.Visible;
                }
                if (selected == 2)
                {
                    mTipoDeConsulta = "facturas";
                    labIdTexto.Content = "Nro de Factura";
                    labTotalTexto.Content = "Total:";
                    gridPendientes.Visibility = Visibility.Hidden;
                    gridRemitosyfacturas.Visibility = Visibility.Visible;
                }

                ///animacion
                //sbListConsultas.Begin();

                ///color textblock de las pestañas en tabConsultas
                tbTabCajaItem0.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 0)
                {
                    tbTabCajaItem0.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }

                tbTabCajaItem1.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 1)
                {
                    tbTabCajaItem1.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }

                tbTabCajaItem2.Foreground = App.Current.Resources["textoclaro"] as SolidColorBrush;
                if (selected == 2)
                {
                    tbTabCajaItem2.Foreground = App.Current.Resources["confoco2"] as SolidColorBrush;
                }

                ///actualizar datos a mostrar
                //if (mTipoDeConsulta == "facturas" || mTipoDeConsulta == "remitos")
                //{
                ActualizarListConsultasDesdeDB();
                //}

                //ayuda(zAyuda.consultas_f3Alternar);

            }
            /// si es factura o remito, foco en listConsultas. Sino, en el tbFiltrar
            if (mTipoDeConsulta == "facturas" || mTipoDeConsulta == "remitos")
            {
                ///foco en el listConsultas
                if (listConsultas.SelectedIndex == -1)
                {
                    listConsultas.SelectedIndex = 0;
                }

                //listConsultas.SelectedIndex = 0;
                //var item = listConsultas.ItemContainerGenerator.ContainerFromIndex(listConsultas.SelectedIndex) as ListViewItem;

                listConsultas.UpdateLayout();
                var ind = listConsultas.SelectedIndex;
                if (ind >= 0)
                {
                    listConsultas.ScrollIntoView(listConsultas.Items[ind]);
                }
                var item = (ListViewItem)listConsultas.ItemContainerGenerator.ContainerFromIndex(ind);

                //consola("hola " + listConsultas.SelectedIndex);
                if (item != null)
                {
                    item.Focus();
                    //consola("chau: " + item.ToString());
                }
                //listConsultas.Focus();
            }

            if (mTipoDeConsulta == "pendientes")
            {
                resetTextos();
                tbFiltrar.Focus();
            }

        }

        private void listConsultasYlistPendientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (mTipoDeConsulta == "pendientes")
            //{
            //    resetTextos();
            //}
            //if (mTipoDeConsulta == "facturas" || mTipoDeConsulta == "remitos")
            //{
            //}

            ///animacion
            sbListDetalles.Begin();

            ///mostrar datos del item seleccionado en listConsultas
            var list = sender as ListView;
            int selected = list.SelectedIndex;

            if (selected >= 0)
            {
                var item = list.SelectedItem as itemListConsultas;
                labId.Content = item.nro;
                labFecha.Content = item.fecha.Substring(0, 8);
                labNombre.Content = item.nombre;
                labDireccion.Content = item.direccion;
                labTelefono.Content = item.telefono;
                labCuit.Content = item.cuit;

                labTotal.Content = "$ " + item.total.ToString("0.00");

                ActualizarListDetallesDesdeDB();

                if (mTipoDeConsulta == "pendientes")
                {
                    ///habilitar botones de imprimir y de pagar
                    habilitarBotones(true);
                }
            }
            else
            {
                resetTextos();

            }
        }
        private void listNombres_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mTipoDeConsulta == "pendientes")
            {
                var list = listNombres;

                if (list.SelectedIndex > -1)
                {
                    var selected = list.SelectedItem;
                    var fila = selected as itemListConsultas;

                    //var textBox = sender as TextBox;
                    //string filtro = textBox.Text.Trim();
                    string filtro = fila.nombre.ToString();

                    var pendientesFiltrados = from registro in mItemsConsulta
                                              where registro.nombre.ToLower().Equals(filtro.ToLower())
                                              orderby registro.nombre
                                              select registro;

                    listPendientes.ItemsSource = pendientesFiltrados;
                }
                else
                {
                    listPendientes.ItemsSource = null;
                }
            }
        }
        private void listNombres_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {

                if (listPendientes.Items.Count > 0)
                {
                    //listPendientes.Focus();
                    //listPendientes.SelectedIndex = 0;
                    var ind = 0;
                    var item = (ListViewItem)listPendientes.ItemContainerGenerator.ContainerFromIndex(ind);
                    if (item != null)
                    {
                        item.Focus();
                        listPendientes.SelectedIndex = ind;
                    }
                }
                e.Handled = true;
            }
            if (e.Key == Key.Escape)
            {
                listNombres.SelectedIndex = -1;
                tbFiltrar.Text = "";
                tbFiltrar.Focus();
            }
        }
        private void listPendientes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (listPendientes.SelectedIndex > -1)
                {
                    btnPagar.Focus();
                    e.Handled = true;
                }
            }
            if (e.Key == Key.Escape)
            {
                if (listNombres.Items.Count > 0)
                {
                    //listNombres.UpdateLayout();
                    var ind = listNombres.SelectedIndex;
                    //var ind = 0;
                    if (ind >= 0)
                    {
                        listNombres.ScrollIntoView(listConsultas.Items[ind]);
                    }
                    var item = (ListViewItem)listNombres.ItemContainerGenerator.ContainerFromIndex(ind);
                    //consola("hola " + listConsultas.SelectedIndex);
                    if (item != null)
                    {
                        item.Focus();
                        listNombres.SelectedIndex = ind;
                        //consola("chau: " + item.ToString());
                    }
                }
            }
        }

        private void tbFiltrar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mTipoDeConsulta == "pendientes")
            {
                var textBox = sender as TextBox;
                string filtro = textBox.Text.Trim();
                if (filtro == "(Filtrar)")
                    filtro = "";

                if (filtro != "")
                {
                    var nombresFiltrados = from registro in mItemsConsulta
                                           where registro.nombre.ToLower().Contains(filtro.ToLower())
                                           orderby registro.nombre
                                           select registro;

                    listNombres.ItemsSource = nombresFiltrados;
                }
                else
                {
                    listNombres.ItemsSource = mItemsConsulta;
                }
            }
        }
        private void tbFiltrar_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Down)
            {
                if (listNombres.Items.Count > 0)
                {
                    //listNombres.UpdateLayout();
                    //var ind = listNombres.SelectedIndex;
                    var ind = 0;
                    //if (ind >= 0)
                    //{
                    //    listNombres.ScrollIntoView(listConsultas.Items[ind]);
                    //}
                    var item = (ListViewItem)listNombres.ItemContainerGenerator.ContainerFromIndex(ind);
                    //consola("hola " + listConsultas.SelectedIndex);
                    if (item != null)
                    {
                        item.Focus();
                        listNombres.SelectedIndex = ind;
                        //consola("chau: " + item.ToString());
                    }
                }
                e.Handled = true;
            }
            if (e.Key == Key.Escape)
            {
                tbFiltrar.Text = "";
            }
        }

        private void btnPagar_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                pagarPendiente();
                e.Handled = true;
            }
            if (e.Key == Key.Escape)
            {
                listPendientes.Focus();
            }
        }
        private void btnPagar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pagarPendiente();
            e.Handled = true;
        }
        private void pagarPendiente()
        {
            myInputBoxShow(true, "Importe: ($)", "pagarPendiente");
        }

        private void cargarImporteAPendiente(string importe)
        {
            bool error = false;
            string nro = labId.Content.ToString().Trim();
            string nombre = labNombre.Content.ToString();

            importe = importe.ToString().Replace(".", ",");
            float fImporte = zfun.toFloat(importe);

            ///si el nroPendiente no esta vacio, asentar en BD
            if (nro != "")
            {
                /// 1.1) Datos registro: 'codigo'='-99', 'descripcion'='Pago', 'subtotal'= importe del pago.
                string archivoDB = "pendientes.db";
                string tabla = "pendientes";
                string nombreCampoNro = "pendientenro";
                string parametros = "";

                long nroProceso = zfun.toLong(nro);
                string fecha = zfun.getFechaNow();
                long codigo = -99;
                string descripcion = "Pago";
                long cantidad = 1;
                float precio = fImporte * -1;
                string precioStr = precio.ToString().Replace(",", ".");
                float subtotal = precio;
                string subtotalStr = precioStr;

                string direccion = "";
                string telefono = "";

                /// 1.2) ejecutar comando SQL
                parametros = "(" + nombreCampoNro + ", fecha, codigo, descripcion, cantidad, precio, subtotal, direccion, telefono) VALUES" +
                    "(" + nroProceso + "," + fecha + ",'" + codigo + "','" + descripcion + "'," + cantidad + "," + precioStr + "," + subtotalStr + ",'" + direccion + "','" + telefono + "')";
                zdb.InsertDB(archivoDB, tabla, parametros);

                ///actualizar modelo de datos
                ActualizarListConsultasDesdeDB();
            }
            else
            {
                error = true;
            }

            ///error
            if (error)
            {
                ///mensaje error
                MessageBox.Show("Posiblemente se deba a que la cuenta no fue correctamente seleccionada.", "Error al intentar asentar el pago", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ///mensaje todo OK
                string mensaje = "Se acreditó $ " + fImporte.ToString("0.00") +
                    " a la cuenta de \n" + nombre + ".\n(Nro de Pendiente: " + nro + ")";
                MessageBox.Show(mensaje, "Pendientes", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        /// Input Box (myInputBox)
        public void myInputBoxShow(bool mostrar, string titulo = "", string operacion = "")
        {
            if (mostrar)
            {
                myInputBox_Titulo.Text = titulo;
                myInputBox_Titulo.Tag = operacion.ToString();
                myInputBox.Visibility = Visibility.Visible;
                sbMyInputBox_Show.Begin();
                //myInputBox_Texto.Focus();

            }
            else
            {
                myInputBox_Texto.Text = "";
                myInputBox_Titulo.Text = "";
                operacion = myInputBox_Titulo.Tag.ToString();
                ///darle el foco a cierto objeto, dependiendo de la operacion
                if (operacion == "pagarPendiente")
                {
                    btnPagar.Focus();
                }
                myInputBox.Visibility = Visibility.Collapsed;
                myInputBox_Titulo.Tag = "";
            }
        }
        private void myInputBox_Ok_Click(object sender, RoutedEventArgs e)
        {
            string operacion = "";
            if (myInputBox_Titulo.Tag != null)
                operacion = myInputBox_Titulo.Tag.ToString();

            ///operacion 'cargarImporte'
            if (operacion == "pagarPendiente")
            {
                cargarImporteAPendiente(myInputBox_Texto.Text);
            }

            myInputBoxShow(false);
        }
        private void myInputBox_Cancelar_Click(object sender, RoutedEventArgs e)
        {
            myInputBoxShow(false);
            e.Handled = true;
        }
        private void myInputBox_Botones_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                var boton = sender as Button;
                string nombre = boton.Name;
                if (nombre == myInputBox_Ok.Name)
                {
                    myInputBox_Cancelar.Focus();
                }
                else
                {
                    myInputBox_Ok.Focus();
                }
                e.Handled = true;
            }
            if (e.Key == Key.Up)
            {
                myInputBox_Texto.Focus();
                e.Handled = true;
            }
            if (e.Key == Key.Down)
            {
                e.Handled = true;
            }


        }
        private void myInputBox_Texto_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                ///si no esta vacio, ir al boton OK
                if (myInputBox_Texto.Text != "")
                {
                    myInputBox_Ok.Focus();
                }
                e.Handled = true;
            }
            if (e.Key == Key.Escape)
            {
                myInputBoxShow(false);
                e.Handled = true;
            }
        }

        /// gotFocus (MOSTRAR AYUDA)
        private void tbFiltrar_GotFocus(object sender, RoutedEventArgs e)
        {
            MainWindow.ayuda2(zAyuda.consultas_tbFiltrar,zAyuda.consultas_f3Alternar);
        }
        private void listConsultas_GotFocus(object sender, RoutedEventArgs e)
        {
            //MainWindow.ayuda2();
            ayuda(zAyuda.consultas_f3Alternar);
        }
        private void listNombres_GotFocus(object sender, RoutedEventArgs e)
        {
            MainWindow.ayuda2(zAyuda.consultas_listNombres);
        }
        private void listPendientes_GotFocus(object sender, RoutedEventArgs e)
        {
            MainWindow.ayuda2(zAyuda.consultas_listPendientes);
        }
        private void btnPagar_GotFocus(object sender, RoutedEventArgs e)
        {
            MainWindow.ayuda2(zAyuda.consultas_btnPagar);
        }
    }
}
