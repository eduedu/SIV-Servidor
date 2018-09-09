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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;


namespace SIV_Servidor.zOpciones
{
    public partial class opcAdministrarArticulos : UserControl
    {

        ///GLOBALES
        public ObservableCollection<itemArticulo>
            mTotalArticulos = new ObservableCollection<itemArticulo>();       //Listado de articulos vendidos
        private ObservableCollection<itemArticuloVendido>
            mArticulosFiltrados = new ObservableCollection<itemArticuloVendido>();

        private ObservableCollection<itemPreviewExcel>
            mPreviewExcel = new ObservableCollection<itemPreviewExcel>();

        /// MAIN
        public opcAdministrarArticulos()
        {
            InitializeComponent();

            /// setear inicio
            gridActualizar.Visibility = Visibility.Hidden;
            gridListar.Margin = new Thickness(0, 40, 0, 0);

            /// funciones de inicio
            recargarListArticulos();
        }

        /// FUNCIONES
        private void recargarListArticulos()
        {
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=articulos.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            string consulta = "select * from articulos";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            System.Data.DataTable dataTable = new System.Data.DataTable();
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
                tempArticulo.proveedor = registro["proveedor"].ToString();
                tempArticulo.descripcion = registro["descripcion"].ToString();

                float precio = zfun.toFloat(registro["precio"].ToString());
                tempArticulo.precio = precio;

                //tempArticulo.precio= string.Format("0.00", registro["precio"].ToString());
                //tempArticulo.precio = registro["precio"].ToString();

                float costo = zfun.toFloat(registro["costo"].ToString());
                tempArticulo.costo = costo;
                tempArticulo.tags = registro["tags"].ToString();
                tempArticulo.stock = registro["stock"].ToString();
                //tempArticulo.stock = "1";

                mTotalArticulos.Add(tempArticulo);
            }


            ///cerrar conexion
            conexion.Close();

            ///asignar datos al list
            zfun.consola("hola" + mTotalArticulos.Count.ToString());

            listArticulos.ItemsSource = mTotalArticulos;

        }
        private void recargarListExcel()
        {
            string path = "C:\\Users\\Eduardo\\Documents\\GitHub\\SIV-Servidor\\articulos.xlsx";
            _Application excel = new _Excel.Application();
            Workbook wb;
            Worksheet ws;
            wb = excel.Workbooks.Open(path);
            ws = wb.Worksheets[1];

            //SQLiteConnection conexion;
            //conexion = new SQLiteConnection("Data Source=articulos.db;Version=3;New=False;Compress=True;");
            //conexion.Open();

            //string consulta = "select * from articulos";

            ///// Adaptador de datos, DataSet y tabla
            //SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            //DataSet dataSet = new DataSet();
            //System.Data.DataTable dataTable = new System.Data.DataTable();
            //dataSet.Reset();
            //db.Fill(dataSet);
            //dataTable = dataSet.Tables[0];


            ///borrar todos los elementos de mTotalArticulos
            if (mPreviewExcel != null)
            {
                mPreviewExcel.Clear();
            }

            ///Loop por todos los registros de la tabla
            //foreach (DataRow registro in dataTable.Rows)
            //{

            //    itemArticulo tempArticulo = new itemArticulo();
            //    tempArticulo.id = (long)registro["id"];
            //    tempArticulo.codigo = (long)registro["codigo"];
            //    tempArticulo.codigopro = registro["codigopro"].ToString();
            //    tempArticulo.proveedor = registro["proveedor"].ToString();
            //    tempArticulo.descripcion = registro["descripcion"].ToString();

            //    float precio = zfun.toFloat(registro["precio"].ToString());
            //    tempArticulo.precio = precio;

            //    //tempArticulo.precio= string.Format("0.00", registro["precio"].ToString());
            //    //tempArticulo.precio = registro["precio"].ToString();

            //    float costo = zfun.toFloat(registro["costo"].ToString());
            //    tempArticulo.costo = costo;
            //    tempArticulo.tags = registro["tags"].ToString();
            //    tempArticulo.stock = registro["stock"].ToString();
            //    //tempArticulo.stock = "1";

            //    mPreviewExcel.Add(tempArticulo);
            //}
            for (int i = 1; i < 10; i++)
            {
                itemPreviewExcel fila = new itemPreviewExcel();
                fila.a = readCell(ref ws, i, 1);
                fila.b = readCell(ref ws, i, 2);
                fila.c = readCell(ref ws, i, 3);
                fila.d = readCell(ref ws, i, 4);
                fila.e = readCell(ref ws, i, 5);
                fila.f = readCell(ref ws, i, 6);
                fila.h = readCell(ref ws, i, 7);
                fila.i = readCell(ref ws, i, 8);
                fila.j = readCell(ref ws, i, 9);
                fila.k = readCell(ref ws, i, 10);
                fila.l = readCell(ref ws, i, 11);
                mPreviewExcel.Add(fila);
                //for (int j = 1; j < 10; j++)
                //{
                //retorno = ws.Cells[fil, col].Value2;
                //}
            }


            ///cerrar conexion
            //conexion.Close();

            ///asignar datos al list
            zfun.consola("hola" + mPreviewExcel.Count.ToString());

            listExcel.ItemsSource = mPreviewExcel;

        }

        ///CONTROLES
        private void imgCerrar_PreviewMouseDown(object sender, MouseButtonEventArgs e)

        {
            ((Panel)this.Parent).Children.Remove(this);
            MainWindow.statCortinaNegra.Visibility = Visibility.Hidden;
            if (MainWindow.mPestanaMain == 0)
            {
                MainWindow.statucInicio.tbDescripcion.Focus();
            }
        }
        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            //tbDescripcion.Focus();
        }

        private void tabAccion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = sender as TabControl;
            int selected = tab.SelectedIndex;
            /// mostrar lista de articulos
            if (selected == 0)
            {
                //listArticulos.Visibility = Visibility.Visible;
                gridListar.Visibility = Visibility.Visible;
                gridActualizar.Visibility = Visibility.Hidden;
            }

            /// actualizar lista
            if (selected == 1)
            {
                //listArticulos.Visibility = Visibility.Hidden;
                gridListar.Visibility = Visibility.Hidden;
                gridActualizar.Visibility = Visibility.Visible;

            }


        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {

            recargarListExcel();
            //string msj = "";
            //msj = "hola";
            //msj = readCell(ref ws, 2, 2);

            //zfun.consola(msj);
        }

        public string readCell(ref Worksheet ws, int fil, int col)
        {
            string retorno = "";
            if (ws.Cells[fil, col].Value2 != null)
            {
                if (ws.Cells[fil, col].Value2 is string)
                {
                    retorno = ws.Cells[fil, col].Value2;

                }
                else
                {
                    retorno = ws.Cells[fil, col].Value2.ToString();
                }
            }
            return retorno;
        }
    }
}
