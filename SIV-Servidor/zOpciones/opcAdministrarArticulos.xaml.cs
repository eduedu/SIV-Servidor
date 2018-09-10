using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
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

        public static List<ComboBox> mCombos = new List<ComboBox>();
        public static List<GridViewColumn> mColumnasPreviewExcel = new List<GridViewColumn>();
        public string[] header1 ={
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G",
                "H",
                "I",
                "J",
                "K",
                "L"
            };
        string[] header2 ={
                    "CODIGO",
                    "DESCRIPCION",
                    "PRECIO",
                    "PROVEEDOR",
                    "COD. PROV.",
                };

        /// MAIN
        public opcAdministrarArticulos()
        {
            InitializeComponent();

            /// setear inicio


            /// funciones de inicio
            setearControles();
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
            ///ANTES DE NUEVO THREAD
            Mouse.OverrideCursor = Cursors.Wait;
            gridExcel.Visibility = Visibility.Hidden;
            btnSeleccionarArchivo.Visibility = Visibility.Hidden;

            barraProcesando(true);
            //string path = "C:\\Users\\Eduardo\\Documents\\GitHub\\SIV-Servidor\\articulos.xlsx";
            string path = "C:\\Users\\Eduardo\\Documents\\GitHub\\SIV-Servidor\\lista.xlsx";

            lblArchivo.Content = "ARCHIVO: " + path;

            ///borrar todos los elementos de mTotalArticulos
            if (mPreviewExcel != null)
            {
                mPreviewExcel.Clear();
            }

            /// NUEVO THREAD 
            new Thread(new ThreadStart(delegate
            {
                _Application excel = new _Excel.Application();
                Workbook wb;
                Worksheet ws;
                wb = excel.Workbooks.Open(path);
                ws = wb.Worksheets[1];

                int totalFilas = 0;
                totalFilas = ws.UsedRange.Rows.Count;
                zfun.consola("TOTAL FILAS:" + totalFilas.ToString());


                /// AL TERMINAR EL PROCESO (actualizar view desde el main thread)
                System.Windows.Application.Current.Dispatcher.BeginInvoke((System.Action)(() =>
                {

                    barraProcesando(false);

                    /// mostrar proceso
                    pBar.IsIndeterminate = false;
                    pBar.Minimum = 0;
                    pBar.Maximum = 10;
                    int cantFilas = 10;
                    labelProcesando

                    ///Loop por todos los registros de la tabla
                    ObservableCollection<itemPreviewExcel> tempItemsExcel = new ObservableCollection<itemPreviewExcel>();
                    for (int i = 1; i < cantFilas+1; i++)
                    {

                        itemPreviewExcel fila = new itemPreviewExcel();
                        fila.filaIndex = i;
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
                        //mPreviewExcel.Add(fila);
                        tempItemsExcel.Add(fila);
                    }

                    mPreviewExcel = tempItemsExcel;

                    ///asignar datos al list
                    listExcel.ItemsSource = mPreviewExcel;

                    zfun.consola("hola" + mPreviewExcel.Count.ToString());

                    /// mostrar grid excel
                    gridExcel.Visibility = Visibility.Visible;

                    btnSeleccionarArchivo.Visibility = Visibility.Visible;

                    Mouse.OverrideCursor = null;

                }), DispatcherPriority.Normal, null);
            })).Start();


        }
        private void setearControles()
        {

            /// set combos
            mCombos.Clear();
            mCombos.Add(cmbCodigoInterno);
            mCombos.Add(cmbDescripcion);
            mCombos.Add(cmbPrecio);
            mCombos.Add(cmbProveedor);
            mCombos.Add(cmbCodProveedor);
            string[] strArray =
            {
                "-",
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G",
                "H",
                "I",
                "J",
                "K",
                "L"
            };
            //cmbCodigoInterno.ItemsSource = strArray;
            //cmbDescripcion.ItemsSource = strArray;
            //cmbPrecio.ItemsSource = strArray;
            //cmbProveedor.ItemsSource = strArray;
            //cmbCodProveedor.ItemsSource = strArray;
            foreach (ComboBox combo in mCombos)
            {
                combo.ItemsSource = strArray;
            }

            /// set listExcel
            mColumnasPreviewExcel.Clear();
            mColumnasPreviewExcel.Add(col1);
            mColumnasPreviewExcel.Add(col2);
            mColumnasPreviewExcel.Add(col3);
            mColumnasPreviewExcel.Add(col4);
            mColumnasPreviewExcel.Add(col5);
            mColumnasPreviewExcel.Add(col6);
            mColumnasPreviewExcel.Add(col7);
            mColumnasPreviewExcel.Add(col8);
            mColumnasPreviewExcel.Add(col9);
            mColumnasPreviewExcel.Add(col10);
            mColumnasPreviewExcel.Add(col11);
            mColumnasPreviewExcel.Add(col12);

            int colIndex = 0;
            foreach (GridViewColumn col in mColumnasPreviewExcel)
            {
                col.Header = header1[colIndex];
                colIndex++;
            }

            /// labels =""
            lblArchivo.Content = "";
            mensaje();

            /// mostrar/ ubicar grid lista
            gridListar.Margin = new Thickness(0, 40, 0, 0);
            gridActualizar.Visibility = Visibility.Hidden;


            /// ocultar grid excel
            gridExcel.Visibility = Visibility.Hidden;

            /// progressbar
            barraProcesando(false);
        }
        private void mensaje(string mensaje = "")
        {
            if (mensaje == "")
            {
                labelMensaje.Visibility = Visibility.Hidden;
            }
            else
            {
                labelMensaje.Content = mensaje;
                labelMensaje.Visibility = Visibility.Visible;


            }
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
        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /// borrar mensaje
            mensaje();

            /// recorro columna por columna para setear los headers con header1
            int colIndex = 0;
            foreach (GridViewColumn col in mColumnasPreviewExcel)
            {

                col.Header = header1[colIndex];

                /// asignar binding (
                /// porque no se puede usar "DisplayMemberBinding" y "CellTemplate" a la vez)
                //Binding binding = new Binding();
                ////binding.Path = new PropertyPath("{Binding " + header1[colIndex].ToLower() + "}");
                //binding.Path = new PropertyPath(header1[colIndex].ToLower());
                //col.DisplayMemberBinding = binding;

                mColumnasPreviewExcel[colIndex].HeaderContainerStyle = null;


                colIndex++;
            }

            /// recorro combo por combo buscando setear con header2
            int comboIndex = 0;
            foreach (ComboBox combo in mCombos)
            {
                int index = combo.SelectedIndex;
                //string valor = combo.SelectedItem as string;

                /// si el combo tiene una letra, poner ese nombre(header2) a la columna
                if (index > 0)
                {
                    mColumnasPreviewExcel[index - 1].Header = header2[comboIndex];


                    /// formatear celdas de la columna
                    /// porque no se puede usar "DisplayMemberBinding" y "CellTemplate" a la vez)
                    //            string bindingPath = header1[index-1].ToLower();
                    //            string xaml = @"
                    //<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""> 
                    //    <TextBlock Text=""{Binding " + bindingPath + @"}""> 
                    //        <TextBlock.Style>
                    //            <Style TargetType=""{x:Type TextBlock}"">
                    //                <Setter Property=""Background"" Value=""Yellow"" />
                    //            </Style>
                    //        </TextBlock.Style>
                    //    </TextBlock>
                    //    </DataTemplate>";
                    //            StringReader stringReader = new StringReader(xaml);
                    //            XmlReader xmlReader = XmlReader.Create(stringReader);
                    //            mColumnasPreviewExcel[index - 1].DisplayMemberBinding = null;
                    //            mColumnasPreviewExcel[index - 1].CellTemplate = XamlReader.Load(xmlReader) as DataTemplate;

                    /// formato del header
                    System.Windows.Style style = this.FindResource("columnaSeleccionada") as System.Windows.Style;
                    mColumnasPreviewExcel[index - 1].HeaderContainerStyle = style;

                }
                comboIndex++;
            }


        }
        private void btnSeleccionarArchivo_Click(object sender, RoutedEventArgs e)
        {
            recargarListExcel();

        }

        private void btnProcesar_Click(object sender, RoutedEventArgs e)
        {
            int comboIndex = 0;
            string msj = "";
            bool faltaSeleccionarColumna = false;
            bool mismasColumnasSeleccionadas = false;
            foreach (ComboBox combo in mCombos)
            {
                int index = combo.SelectedIndex;
                string valor = combo.SelectedItem as string;


                if (valor == "-")
                {
                    msj = "Falta seleccionar la columna: " + header2[comboIndex];
                    faltaSeleccionarColumna = true;
                    break;
                }
                else
                {
                    /// evaluar si se repiten las columnas
                    if (comboIndex < mCombos.Count - 1)
                    {
                        for (int i = comboIndex + 1; i < mCombos.Count; i++)
                        {
                            string valor2 = mCombos[i].SelectedItem as string;
                            if (valor == valor2)
                            {
                                mismasColumnasSeleccionadas = true;
                                msj = header2[comboIndex] + "=" + header2[i];
                            }
                        }
                        //int comboIndex2 = 0;
                        //foreach (ComboBox combo2 in mCombos)
                        //{
                        //    comboIndex2++;
                        //}
                    }
                }
                comboIndex++;
            }
            if (mismasColumnasSeleccionadas)
            {
                msj = "Hay 2 campos que direccionan a la misma columna: " + msj;
            }
            bool todoOk = false;
            if (!faltaSeleccionarColumna && !mismasColumnasSeleccionadas)
            {
                todoOk = true;
                msj = "Procesando...";
            }
            mensaje(msj);
            zfun.consola(msj);
        }

        private void barraProcesando(bool estado)
        {
            if (estado)
            {
                pBar.IsIndeterminate = true;
                labelProcesando.Content = "Procesando...";
                pBar.Visibility = Visibility.Visible;
                labelProcesando.Visibility = Visibility.Visible;

            }
            else
            {
                pBar.Visibility = Visibility.Hidden;
                labelProcesando.Visibility = Visibility.Hidden;

            }
        }


    }
}
