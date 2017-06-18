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
            labNombre.Content = "";
            labTotal.Content = "$ ";
            labId.Content = "";

            listConsultas.ItemsSource = mItemsConsulta;
            listDetalles.ItemsSource = mItemsDetalle;
            ActualizarListConsultasDesdeDB();
        }

        ///--------------------------------------------------------------------------------------------------------

        ///FUNCIONES BD
        private void ActualizarListConsultasDesdeDB()
        {
            ///definicion de parametros 
            string archivo = "";
            string tabla = "";
            string campoNro = "";

            ///asignacion de valores segun el tipo de consulta (mTipoDeConsulta) seleccionado

            //mTipoDeConsulta = "remitos";

            if (mTipoDeConsulta == "pendientes")
            {
                archivo = "pendientes.db";
                tabla = "pendientes";
                campoNro = "pendientenro";
            }
            if (mTipoDeConsulta == "remitos")
            {
                archivo = "remitos.db";
                tabla = "remitos";
                campoNro = "remitonro";
            }
            if (mTipoDeConsulta == "facturas")
            {
                archivo = "facturas.db";
                tabla = "facturas";
                campoNro = "facturanro";
            }


            ///establecer conexion SQL
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivo + ";Version=3;New=False;Compress=True;");
            conexion.Open();

            string consulta = "select * from " + tabla + " ORDER BY id DESC";

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
                    tempItem.fecha = zfun.toLong(registro["fecha"].ToString());
                    tempItem.nombre = registro["descripcion"].ToString();
                    tempItem.direccion = registro["direccion"].ToString();
                    tempItem.telefono = registro["telefono"].ToString();
                    if (mTipoDeConsulta == "factura")
                    {
                        tempItem.cuit = registro["descripcion"].ToString();
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


            ///Si la ID no está vacia, buscar los registros con esa ID
            if (id != "")
            {
                ///asignacion de valores segun el tipo de consulta (mTipoDeConsulta) seleccionado
                if (mTipoDeConsulta == "pendientes")
                {
                    archivo = "pendientes.db";
                    tabla = "pendientes";
                    campoNro = "pendientenro";
                }
                if (mTipoDeConsulta == "remitos")
                {
                    archivo = "remitos.db";
                    tabla = "remitos";
                    campoNro = "remitonro";
                }
                if (mTipoDeConsulta == "facturas")
                {
                    archivo = "facturas.db";
                    tabla = "facturas";
                    campoNro = "facturanro";
                }


                ///establecer conexion SQL
                SQLiteConnection conexion;
                conexion = new SQLiteConnection("Data Source=" + archivo + ";Version=3;New=False;Compress=True;");
                conexion.Open();

                string consulta = "select * from " + tabla + " WHERE " + campoNro + " = " + id + " ORDER BY id DESC";

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
                        tempItem.codigo = zfun.toLong(registro["codigo"].ToString());
                        tempItem.descripcion= registro["descripcion"].ToString();
                        tempItem.cantidad = zfun.toLong( registro["cantidad"].ToString());
                        tempItem.precio = zfun.toFloat( registro["precio"].ToString());
                        tempItem.subtotal= zfun.toFloat(registro["subtotal"].ToString());

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
                }
                if (selected == 1)
                {
                    mTipoDeConsulta = "remitos";
                }
                if (selected == 2)
                {
                    mTipoDeConsulta = "facturas";
                }

                ///animacion
                sbListConsultas.Begin();

                ///actualizar datos a mostrar
                ActualizarListConsultasDesdeDB();
            }
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
                labNombre.Content = item.nombre;
                labTotal.Content = "$ " + item.total.ToString("#.##");
                labId.Content = item.nro;

                ActualizarListDetallesDesdeDB();
            }
            else
            {
                labNombre.Content = "";
                labTotal.Content = "$ ";

            }
        }
    }
}
