﻿using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SIV_Servidor.zOpciones
{
    public partial class opcImprimirCodigos : UserControl
    {
        ///GLOBALES
        public ObservableCollection<itemArticuloVendido>
            mArticulosVendidos = new ObservableCollection<itemArticuloVendido>();     //Listado de articulos vendidos

        ///MAIN
        public opcImprimirCodigos()
        {
            InitializeComponent();

            ///INICIO Y ASIGNACIONES
            recargarListArticulos();
        }

        ///FUNCIONES
        private void recargarListArticulos()
        {

            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=caja.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            //string consulta = "select * from caja ORDER BY id DESC";
            string consulta = "select id, idventa, datetime(fecha), codigo, descripcion, cantidad, precio, costo from caja ORDER BY id DESC";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///borrar todos los elementos de mArticulos
            if (mArticulosVendidos != null)
            {
                mArticulosVendidos.Clear();
            }

            ///Loop por todos los registros de la tabla
            int cantidadItems = 0;
            foreach (DataRow registro in dataTable.Rows)
            {
                long codigo = zfun.toLong(registro["codigo"].ToString());

                ///evaluar si ya existe un articulo con ese codigo en la lista
                bool hayEseArticulo = false;
                itemArticuloVendido tmpItem = mArticulosVendidos.Where(x => x.codigo == codigo).FirstOrDefault();
                if (tmpItem != null)
                {
                    hayEseArticulo = true;
                }

                ///si no esta este codigo en la lista, agregar
                if (!hayEseArticulo)
                {
                    ///cargar datos a tempArticulo
                    itemArticuloVendido tempArticulo = new itemArticuloVendido();
                    tempArticulo.fechaVenta = registro[2].ToString();
                    tempArticulo.fechaVentaMostrar = zfun.toFechaMostrar(registro[2].ToString());

                    tempArticulo.codigo = codigo;
                    tempArticulo.descripcion = registro["descripcion"].ToString().Trim();
                    tempArticulo.precio = zfun.toFloat(registro["precio"].ToString());

                    


                    ///CONSULTAR BD 'ARTICULOS'
                    ///si el campo 'fechaImpresion' es null, imprimir
                    string fechaImpresion = "";
                    fechaImpresion = dbVerificarImpresionEnBaseAlCodigo(codigo.ToString());
                    //if (fechaImpresion == "registroInexistente")
                    //{
                    //    zfun.consola("codigo:" + codigo.ToString());
                    //}
                    if (fechaImpresion == "")
                    {
                        tempArticulo.imprimir = true;
                        tempArticulo.color = 1;
                    }
                    else
                    {
                        tempArticulo.imprimir = false;
                        tempArticulo.color = 0;
                    }

                    tempArticulo.fechaImpresion = fechaImpresion;
                    //tempArticulo.fechaImpresionMostrar = fechaImpresion;
                    tempArticulo.fechaImpresionMostrar = zfun.toFechaMostrar(fechaImpresion);

                    ///agregar tempArticulo a la lista
                    mArticulosVendidos.Add(tempArticulo);

                    ///aumentar la cuenta de items que hay en la lista
                    cantidadItems++;
                }

                ///si se llega a la cantidad de items seleccionada en el comboBox, salir
                int max = zfun.toInt((comboCantidad.SelectedItem as ComboBoxItem).Content.ToString());
                if (cantidadItems > max - 1)
                {
                    break;
                }
            }


            ///asigno la lista al control listCaja
            //listCaja.ItemsSource = mArticulosCaja;
            //mArticulosCaja.Reverse();
            zfun.consola(mArticulosVendidos.Count.ToString());
            listArticulos.ItemsSource = mArticulosVendidos;

            ///cerrar conexion
            conexion.Close();

        }
        public static string dbVerificarImpresionEnBaseAlCodigo(string codigo)
        {
            /// Trabaja con DATOS.DB: Devuelve el valor del campo 'fechaImpresion' que coincide con campo 'codigo'=codigo. Si no existe, devuelve la cadena "registroInexistente".

            ///definir parametros y variables
            string valor = "registroInexistente";
            string archivoDB = "articulos.db";
            string tabla = "articulos";

            ///abrir conexion DB
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivoDB + ";Version=3;New=False;Compress=True;");
            conexion.Open();

            ///realizar CONSULTA
            //string consulta = "select *, MAX(id) from articulos";
            //string consulta = "select MAX(" + campo + ") from " + tabla;
            //string consulta = "SELECT * FROM " + tabla + " WHERE codigo='" + codigo + "'";
            string consulta = "select codigo, datetime(fechaImpresion) FROM " + tabla + " WHERE codigo='" + codigo + "'";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///cierro base de datos
            conexion.Close();

            ///// retornar valor maximo de idventa (error=-1)
            //int resultado = -1;
            //Int32.TryParse(dataTable.Rows[0].ItemArray.GetValue(0).ToString(), out resultado);

            if (dataTable.Rows.Count > 0)
            {
                valor = dataTable.Rows[0].ItemArray.GetValue(1).ToString();
            }

            return valor;
        }
        ///CONTROLES
        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
            MainWindow.statCortinaNegra.Visibility = Visibility.Hidden;
        }
        private void comboCantidad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listArticulos != null)
            {
                int max = zfun.toInt((comboCantidad.SelectedItem as ComboBoxItem).Content.ToString());
                zfun.consola("co:" + max.ToString());
                recargarListArticulos();

            }
        }
        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            ///recorrer listArticulos
            foreach (var item in listArticulos.Items)
            {
                itemArticuloVendido it = item as itemArticuloVendido;

                ///si checkBox imprimir=true, buscar en BD 'articulos' el item con ese numero de codigo y cargar la fecha(now)
                if (it.imprimir)
                {
                    ///buscar y modificar articulo en BD 'articulos'
                    //DATETIME('NOW')
                    string archivo = "articulos.db";
                    string tabla = "articulos";
                    string campoBusqueda = "codigo";
                    string valorBusqueda = it.codigo.ToString();
                    string campoModificacion = "fechaimpresion";
                    string valorModificacion = zfun.getFechaNow();
                    zdb.modificarRegistroDBcualquierCampo(archivo, tabla, campoBusqueda, valorBusqueda, campoModificacion, valorModificacion, true);
                }
            }

        }

        private void cbOcultarImpresos_Click(object sender, RoutedEventArgs e)
        {
            //recargarListArticulos();
            ///si cbOcultarImpresos=true, ocultar los que no se van a imprimir
            if (cbOcultarImpresos.IsChecked == true)
            {

                var articulosFiltrados = from registro in mArticulosVendidos
                                         where registro.imprimir.Equals(true)
                                         select registro;
                //orderby registro.descripcion

                //mTotalArticulosConFiltro = null;
                //mTotalArticulosConFiltro = articulosFiltrados.ToList();
                //listFiltro.ItemsSource = mTotalArticulosConFiltro;

                listArticulos.ItemsSource = articulosFiltrados;
            }
            else
            {
                listArticulos.ItemsSource = mArticulosVendidos;
            }
        }

        private void listArticulos_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListView;
            var selected = list.SelectedItem as itemArticuloVendido;

            zfun.consola(selected.imprimir.ToString());
            if (selected.imprimir==true)
            {
                //selected.imprimir = false;
            } else
            {
                //selected.imprimir = true;
                
            }
        }
    }
}