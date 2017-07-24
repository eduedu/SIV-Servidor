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


        public string mTipoDeConsulta = "pendientes";
        Storyboard sbListConsultas;
        Storyboard sbListDetalles;


        ///MAIN
        public ucConsultas()
        {
            InitializeComponent();

            ///animaciones
            sbListConsultas = this.FindResource("sbListConsultas") as Storyboard;
            sbListDetalles = this.FindResource("sbListDetalles") as Storyboard;

            ///INICIO Y ASIGNACIONES
            resetTextos();
            listConsultas.ItemsSource = mItemsConsulta;
            listDetalles.ItemsSource = mItemsDetalle;
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
                    if (mTipoDeConsulta == "facturas")
                    {
                        tempItem.cuit = registro["cuit"].ToString();
                    }
                    else
                    {
                        tempItem.cuit = "";
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

            ///seleccionar el primer item (o ninguno) de listConsultas
            if (mItemsConsulta.Count > 0)
            {
                listConsultas.SelectedIndex = 0;
            }
            else
            {
                listConsultas.SelectedIndex = -1;

            }

            ActualizarListDetallesDesdeDB();

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
                    labIdTexto.Content = "Nro de Pendiente";
                }
                if (selected == 1)
                {
                    mTipoDeConsulta = "remitos";
                    labIdTexto.Content = "Nro de Remito";
                }
                if (selected == 2)
                {
                    mTipoDeConsulta = "facturas";
                    labIdTexto.Content = "Nro de Factura";
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
                ActualizarListConsultasDesdeDB();

            }
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

            consola("hola " + listConsultas.SelectedIndex);
            if (item != null)
            {
                item.Focus();
                consola("chau: " + item.ToString());
            }
            //listConsultas.Focus();



        }

        private void listConsultas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            }
            else
            {
                resetTextos();

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
