using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
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
using Microsoft.Win32;
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

        private bool mArchivoExcelCargado = false;

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
        public int[] mColumnaIndex = { 0, 0, 0, 0, 0 };
        const int CODIGO = 0;
        const int DESCRIPCION = 1;
        const int PRECIO = 2;
        const int PROVEEDOR = 3;
        const int CODPROVEEDOR = 4;

        _Application excel;
        Workbook wb;
        Worksheet ws;
        int totalFilas = 0;

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


            /// mostrar/ ocular grids
            gridExcel.Visibility = Visibility.Hidden;
            gridAnalizarArchivo.Visibility = Visibility.Visible;

            gridActualizarBD.Visibility = Visibility.Hidden;
            gridActualizarBD.Margin = new Thickness(0, 0, 0, 0);

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

            string path = "";
            /// precargar=cuando se abre por primera vez el archivo y se cargan solo las 1ras 10 filas
            if (!mArchivoExcelCargado)
            {
                mensaje("Cargando archivo de Excel...");
                //path = "C:\\Users\\Eduardo\\Documents\\GitHub\\SIV-Servidor\\articulos.xlsx";
                string miDir = System.AppDomain.CurrentDomain.BaseDirectory;
                //path = miDir + "articulos.xlsx";
                //path = "C:\\Users\\Eduardo\\Documents\\GitHub\\SIV-Servidor\\lista2.xlsx";
                //lblArchivo.Content = "ARCHIVO: " + path;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Archivos excel (*.xls;*.xlsx)|*.xls;*.xlsx|Todos (*.*)|*.*";
                openFileDialog.InitialDirectory = miDir;

                if (openFileDialog.ShowDialog() == true)
                {
                    //path = File.ReadAllText(openFileDialog.FileName);
                    path = openFileDialog.FileName;
                    zfun.consola(path);

                    lblArchivo.Content = "ARCHIVO: " + path;

                }
                else
                {
                    zfun.consola("cancelar");
                    Mouse.OverrideCursor = null;
                    gridExcel.Visibility = Visibility.Hidden;
                    btnSeleccionarArchivo.Visibility = Visibility.Visible;
                    barraProcesando(false);
                    return;

                }

            }

            ///borrar todos los elementos de mTotalArticulos
            if (mPreviewExcel != null)
            {
                mPreviewExcel.Clear();
            }

            /// NUEVO THREAD 
            new Thread(new ThreadStart(delegate
            {
                /// setear archivo excel
                if (!mArchivoExcelCargado)
                {
                    excel = new _Excel.Application();
                    wb = excel.Workbooks.Open(path);
                    ws = wb.Worksheets[1];
                }

                /// establecer cantidad de filas del excel a procesar
                if (!mArchivoExcelCargado)
                {
                    totalFilas = 10;
                }
                else
                {
                    totalFilas = 20;
                    //totalFilas = ws.UsedRange.Rows.Count;
                }

                //zfun.consola("TOTAL FILAS:" + totalFilas.ToString());


                /// AL TERMINAR EL PROCESO (actualizar view desde el main thread)
                System.Windows.Application.Current.Dispatcher.BeginInvoke((System.Action)(() =>
                {

                    barraProcesando(false);


                    /// mostrar progreso actualizado
                    pBar.IsIndeterminate = false;
                    pBar.Visibility = Visibility.Visible;
                    pBar.Minimum = 0;
                    pBar.Maximum = totalFilas;
                    labelProcesando.Visibility = Visibility.Visible;

                    /// background process (procesar cada fila)
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += worker_DoWork;
                    worker.ProgressChanged += worker_ProgressChanged;
                    //worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

                    worker.RunWorkerAsync();
                    //zfun.consola("hola" + mPreviewExcel.Count.ToString());

                }), DispatcherPriority.Normal, null);
            })).Start();


        }

        /// WORKER progress bar
        //private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //for (int i = 0; i < 20; i++)
            //{
            //    (sender as BackgroundWorker).ReportProgress(i);
            //    Thread.Sleep(100);
            //}
            //int cantFilas = 10;

            /// campos
            int colCodigo = mColumnaIndex[CODIGO];
            int colDescripcion = mColumnaIndex[DESCRIPCION];
            int colPrecio = mColumnaIndex[PRECIO];
            int colProveedor = mColumnaIndex[PROVEEDOR];
            int colCodProveedor = mColumnaIndex[CODPROVEEDOR];
            zfun.consola("colCodigo: " + colCodigo.ToString());
            zfun.consola("colDescripcion: " + colDescripcion.ToString());
            zfun.consola("colPrecio: " + colPrecio.ToString());
            zfun.consola("colProveedor: " + colProveedor.ToString());
            zfun.consola("colCodProveedor: " + colCodProveedor.ToString());


            ///Loop por todos los registros de la tabla
            ObservableCollection<itemPreviewExcel> tempItemsExcel = new ObservableCollection<itemPreviewExcel>();
            for (int i = 1; i < totalFilas + 1; i++)
            {
                (sender as BackgroundWorker).ReportProgress(i);

                //labelProcesando.Content = i.ToString() + "/" + cantFilas.ToString();
                //pBar.Value = i;

                itemPreviewExcel fila = new itemPreviewExcel();
                //fila.filaIndex = i;
                //fila.a = readCell(ref ws, i, 1);
                //fila.b = readCell(ref ws, i, 2);
                //fila.c = readCell(ref ws, i, 3);
                //fila.d = readCell(ref ws, i, 4);
                //fila.e = readCell(ref ws, i, 5);
                //fila.f = readCell(ref ws, i, 6);
                //fila.g = readCell(ref ws, i, 7);
                //fila.h = readCell(ref ws, i, 8);
                //fila.i = readCell(ref ws, i, 9);
                //fila.j = readCell(ref ws, i, 10);
                //fila.k = readCell(ref ws, i, 11);
                //fila.l = readCell(ref ws, i, 12);

                fila.celda[0] = i.ToString();
                for (int j = 1; j < fila.celda.Length; j++)
                {
                    fila.celda[j] = readCell(ref ws, i, j);
                }
                //mPreviewExcel.Add(fila);

                fila.procesoEnBD = 0;

                /// procesar archivo una vez que esta cargado
                if (mArchivoExcelCargado)
                {
                    bool modificarBD = false;
                    bool agregarEnBD = true;


                    string codigo = readCell(ref ws, i, colCodigo);
                    string descripcion = readCell(ref ws, i, colDescripcion);

                    //string tempPrecio = readCell(ref ws, i, colPrecio);
                    //float precio = float.Parse(tempPrecio, CultureInfo.InvariantCulture.NumberFormat);
                    string precio = readCell(ref ws, i, colPrecio);


                    string proveedor = readCell(ref ws, i, colProveedor);
                    string codProveedor = readCell(ref ws, i, colCodProveedor);

                    /// validacion de la fila
                    if (codigo == "")
                    {
                        agregarEnBD = false;
                    }
                    if (descripcion == "")
                    {
                        agregarEnBD = false;
                    }
                    if (precio == "")
                    {
                        agregarEnBD = false;
                    }
                    else
                    {
                        //if (precio != "")
                        //precio = float.Parse(precio, CultureInfo.InvariantCulture.NumberFormat).ToString();
                        //if (float.Parse(precio, CultureInfo.InvariantCulture.NumberFormat) == 0)
                        //{
                        //    agregarEnBD = false;
                        //}
                        int ignorarEsto;
                        bool isNumber = int.TryParse(precio, out ignorarEsto);
                        if (!isNumber)
                        {
                            agregarEnBD = false;
                        }
                    }



                    if (agregarEnBD)
                    {
                        fila.procesoEnBD = 2;
                    }

                    if (i < 4)
                    {
                        //string msj = i.ToString() + "-codigo: " + codigo + " -descripcion:" + descripcion + " -precio:" + precio.ToString() + " -prov:" + proveedor + " -codProv:" + codProveedor;
                        string msj = fila.celda[0] + "-codigo: " + fila.celda[colCodigo] + " -descripcion:"
                            + fila.celda[colDescripcion] + " -precio:" + fila.celda[colPrecio];
                        zfun.consola(msj);
                    }


                }


                tempItemsExcel.Add(fila);
            }

            mPreviewExcel = tempItemsExcel;

            ///// procesar archivo una vez que esta cargado
            //if (mArchivoExcelCargado)
            //{
            //    mPreviewExcel[2].procesoEnBD= 1;
            //    mPreviewExcel[3].procesoEnBD = 2;
            //}

            (sender as BackgroundWorker).ReportProgress(totalFilas);



        }
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int i = e.ProgressPercentage;
            //pBar.Value = e.ProgressPercentage;
            //labelProcesando.Content = e.ProgressPercentage.ToString();

            string XdeY = i.ToString() + "/" + totalFilas.ToString();
            labelProcesando.Content = XdeY;
            pBar.Value = i;
            //zfun.consola(labelProcesando.Content.ToString());

            mensaje("Procesando filas: " + XdeY + "");
            /// si se completo
            if (i == totalFilas)
            {
                /// ACCESO AL VIEW

                //zfun.consola("FIN!");
                ///asignar datos al list
                listExcel.ItemsSource = mPreviewExcel;
                barraProcesando(false);

                mensaje();

                /// mostrar grid excel
                gridExcel.Visibility = Visibility.Visible;

                btnSeleccionarArchivo.Visibility = Visibility.Visible;
                Mouse.OverrideCursor = null;

                if (mArchivoExcelCargado)
                {
                    gridAnalizarArchivo.Visibility = Visibility.Hidden;
                    gridActualizarBD.Visibility = Visibility.Visible;
                }

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
                //if (ws.Cells[fil, col].Value2 is string)
                //{
                //    retorno = ws.Cells[fil, col].Value2;

                //}
                //else
                //{
                //}
                retorno = ws.Cells[fil, col].Value2.ToString();
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
                    mColumnaIndex[comboIndex] = index;

                    ///// le sumo 2 porque las 2 primeras columnas no cuentan (nro de fila y procesoEnDB)
                    //mColumnaIndex[comboIndex] = mColumnaIndex[comboIndex] + 2;

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
            mArchivoExcelCargado = false;
            gridAnalizarArchivo.Visibility = Visibility.Visible;
            gridActualizarBD.Visibility = Visibility.Hidden;
            recargarListExcel();

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

        private void btnAnalizar_Click(object sender, RoutedEventArgs e)
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
            }
            mensaje(msj);
            //zfun.consola(msj);
            if (todoOk)
            {
                zfun.consola("todo OK");
                analizarArchivo();
            }
            else
            {
                zfun.consola("hubo un problema");
            }
        }
        private void analizarArchivo()
        {
            string msj = "Procesando...";
            mensaje(msj);

            /// cargar todas las filas en el listExcel
            mArchivoExcelCargado = true;
            recargarListExcel();

            //mPreviewExcel[2].procesar = true;
            //mPreviewExcel[3].procesar = true;

            /// actualizar datos
            //listExcel.ItemsSource = null;
            //listExcel.ItemsSource = mPreviewExcel;
            //listExcel.UpdateLayout();

        }

        /// agregar items a la base de datos
        private void btnProcesarArchivo_Click(object sender, RoutedEventArgs e)
        {
            ///ANTES DE NUEVO THREAD
            Mouse.OverrideCursor = Cursors.Wait;
            //gridExcel.Visibility = Visibility.Hidden;
            //btnSeleccionarArchivo.Visibility = Visibility.Hidden;

            barraProcesando(true);

            /// NUEVO THREAD 
            new Thread(new ThreadStart(delegate
            {
                totalFilas = mPreviewExcel.Count;
                //zfun.consola("TOTAL FILAS:" + totalFilas.ToString());

                string msj = "se agregarán los siguientes items a la BD:";
                zfun.consola(msj);

                /// AL TERMINAR EL PROCESO (actualizar view desde el main thread)
                System.Windows.Application.Current.Dispatcher.BeginInvoke((System.Action)(() =>
                {

                    barraProcesando(false);

                    /// mostrar progreso actualizado
                    pBar.IsIndeterminate = false;
                    pBar.Visibility = Visibility.Visible;
                    pBar.Minimum = 0;
                    pBar.Maximum = totalFilas;
                    labelProcesando.Visibility = Visibility.Visible;

                    /// background process (procesar cada fila)
                    BackgroundWorker worker2 = new BackgroundWorker();
                    worker2.WorkerReportsProgress = true;
                    worker2.DoWork += worker2_DoWork;
                    worker2.ProgressChanged += worker2_ProgressChanged;
                    //worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

                    worker2.RunWorkerAsync();
                    //zfun.consola("hola" + mPreviewExcel.Count.ToString());

                }), DispatcherPriority.Normal, null);
            })).Start();





        }
        private void worker2_DoWork(object sender, DoWorkEventArgs e)
        {
            /// procesa todos los items del mPreviewExcel y los agrega a la BD de articulos


            //for (int i = 0; i < 20; i++)
            //{
            //    (sender as BackgroundWorker).ReportProgress(i);
            //    Thread.Sleep(100);
            //}
            //int cantFilas = 10;

            /// campos
            int colCodigo = mColumnaIndex[CODIGO];
            int colDescripcion = mColumnaIndex[DESCRIPCION];
            int colPrecio = mColumnaIndex[PRECIO];
            int colProveedor = mColumnaIndex[PROVEEDOR];
            int colCodProveedor = mColumnaIndex[CODPROVEEDOR];

            int i = 0;
            foreach (itemPreviewExcel item in mPreviewExcel)
            {
                (sender as BackgroundWorker).ReportProgress(i);
                i++;

                string linea = "";
                /// proceso==2 significa que estan para ser agregadas a la base de datos
                if (item.procesoEnBD == 2)
                {

                    linea += item.CELDA[colCodigo] + ", ";
                    linea += item.CELDA[colDescripcion] + ", ";
                    linea += item.CELDA[colPrecio] + ", ";
                    linea += item.CELDA[colProveedor] + ", ";
                    linea += item.CELDA[colCodProveedor] + ", ";

                    /// preparar registro
                    long codigo = long.Parse(item.CELDA[colCodigo]);
                    string codigoStr = codigo.ToString();
                    string descripcion = item.CELDA[colDescripcion];
                    float precio = float.Parse(item.CELDA[colPrecio]);
                    string precioStr = precio.ToString().Replace(",", ".");
                    string proveedor = item.CELDA[colProveedor];
                    string codigopro = item.CELDA[colCodProveedor];

                    ///conexion SQL y ejecucion de comando
                    string archivo = "articulos.db";
                    string tabla = "articulos";
                    string parametros;
                    parametros = "(codigo, descripcion, precio, proveedor, codigopro) VALUES" +
                           "(" + codigoStr + ",'" + descripcion + "'," + precioStr + ",'" + proveedor + "','" + codigopro + "')";
                    zdb.InsertDB(archivo, tabla, parametros);
                }



                /// agregar a la BD
                if (linea != "") zfun.consola(linea);
            }

            (sender as BackgroundWorker).ReportProgress(totalFilas);



        }
        private void worker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int i = e.ProgressPercentage;
            //pBar.Value = e.ProgressPercentage;
            //labelProcesando.Content = e.ProgressPercentage.ToString();

            string XdeY = i.ToString() + "/" + totalFilas.ToString();
            labelProcesando.Content = XdeY;
            pBar.Value = i;
            //zfun.consola(labelProcesando.Content.ToString());

            mensaje("Procesando filas: " + XdeY + "");
            /// si se completo
            if (i == totalFilas)
            {
                /// ACCESO AL VIEW

                //zfun.consola("FIN!");
                ///asignar datos al list

                //listExcel.ItemsSource = mPreviewExcel;
                barraProcesando(false);

                mensaje();

                /// mostrar grid excel
                //gridExcel.Visibility = Visibility.Visible;

                btnSeleccionarArchivo.Visibility = Visibility.Visible;
                Mouse.OverrideCursor = null;
                gridExcel.Visibility = Visibility.Hidden;

                //if (mArchivoExcelCargado)
                //{
                //gridAnalizarArchivo.Visibility = Visibility.Hidden;
                //gridActualizarBD.Visibility = Visibility.Visible;
                //}

            }

        }
    }
}
