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
using System.Globalization;
using System.Threading;
using System.Windows.Threading;

namespace SIV_Servidor
{
    public partial class ucImpresiones : UserControl
    {
        ///Globales 
        public static ObservableCollection<itemCaja> mArticulosCaja =
            new ObservableCollection<itemCaja>();   //Lista los articulos de la caja

        public double gridXTo { get; set; }
        public double gridXFrom { get; set; }
        double mAnchoPantalla;
        double mMargen = 15;
        public static int mPestanaCaja { get; set; }

        public static List<Button> mBotones = new List<Button>();
        public static int mBotonSelected = 0;

        public ObservableCollection<itemCliente>
            mTotalClientes = new ObservableCollection<itemCliente>();     //Listado de articulos a la venta filtrados

        Style StyleTbNoEditable = Application.Current.FindResource("StyleTBNoEditableFondo2") as Style;
        Style StyleTbNoEditableNuevo = Application.Current.FindResource("StyleTbNoEditableNuevo2") as Style;
        Style StyleTextbox = Application.Current.FindResource("StyleTextbox") as Style;
        Style StyleTextboxUpper = Application.Current.FindResource("StyleTextboxUpper") as Style;
        Style StyleTBNoEditableFondo2Left = Application.Current.FindResource("StyleTBNoEditableFondo2Left") as Style;
        Style StyleTextboxModificar = Application.Current.FindResource("StyleTextboxModificar") as Style;



        bool seEditoDescripcionDesdeElPrograma = false;

        /// animaciones
        Storyboard sbDatosDelClienteMostrar;
        Storyboard sbDatosDelClienteOcultar;
        Storyboard sbListFiltroClientesMostrar;
        Storyboard sbListFiltroClientesOcultar;


        ///ANIMACION BOTONES
        double botonXTo;
        CircleEase easing;
        ThicknessAnimation aniAnchoBtn;
        Style styleBoton = Application.Current.FindResource("BotonMenu") as Style;
        public static Style styleBotonSelected = Application.Current.FindResource("BotonMenuSelected") as Style;




        ///MAIN
        public ucImpresiones()
        {
            InitializeComponent();

            ///Globales
            mAnchoPantalla = 980; // gridCajaMain.Width;
            //gridCajaSlide.Width = 970; //mAnchoPantalla - (mMargen * 2);
            gridCajaSlide.Margin = new Thickness(gridCajaSlide.Margin.Left, gridCajaSlide.Margin.Top, gridCajaSlide.Margin.Bottom, 0);
            gridXTo = -500;
            gridXFrom = 0;

            mBotones.Add(btnRemito);
            mBotones.Add(btnPendiente);
            mBotones.Add(btnFactura);
            mBotones.Add(btnTarjeta);
            mBotones.Add(btnListaDeControl);


            ///ANIMACIONES
            sbDatosDelClienteMostrar = this.FindResource("sbDatosDelClienteMostrar") as Storyboard;
            sbDatosDelClienteOcultar = this.FindResource("sbDatosDelClienteOcultar") as Storyboard;
            sbDatosDelClienteOcultar.Completed += (s, o) =>
            {
                gridDatosDelCliente.Visibility = Visibility.Hidden;
                btnImprimir.Tag = "";
            };
            sbListFiltroClientesMostrar = this.FindResource("sbListFiltroClientesMostrar") as Storyboard;
            sbListFiltroClientesOcultar = this.FindResource("sbListFiltroClientesOcultar") as Storyboard;

            sbListFiltroClientesOcultar.Completed += (s, o) =>
            {
                listFiltroClientes.Visibility = Visibility.Hidden;
            };


            ///ANIMACION BOTONES
            botonXTo = (double)(gridBotones.Width - 3);
            //EASING
            easing = new CircleEase();  // or whatever easing class you want
            easing.EasingMode = EasingMode.EaseInOut;
            aniAnchoBtn = new ThicknessAnimation();
            aniAnchoBtn.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            aniAnchoBtn.EasingFunction = easing;

            ///INICIO Y ASIGNACIONES
            listFiltroClientes.Visibility = Visibility.Hidden;
            listFiltroClientes.Margin = new Thickness(tbNombre.Margin.Left + 0, tbNombre.Margin.Top + tbNombre.Height + 2, 0, 0);
            btnImprimir.Tag = "";

            //gridDatosDelClienteOcultar();
            listCaja.ItemsSource = mArticulosCaja;
            gridDatosDelCliente.Visibility = Visibility.Hidden;
            ActualiarCajaDesdeDB();
            cargarListaDeClientes();
            resetTB();
        }

        //------------------------------------------------------------------------------------------
        ///-----------------------------------------------------------------------------------------

        ///Funciones DB
        public static void ActualiarCajaDesdeDB()
        {
            new Thread(new ThreadStart(delegate
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
                //dataGrid.DataSource = dt;
                //dataGrid.DataContext = dt.DefaultView;  //esto anda

                //gridFiltro.ItemsSource = dataTable.DefaultView;
                //listFiltroClientes.ItemsSource = dataTable.DefaultView;

                ///borrar todos los elementos de mArticulos
                if (mArticulosCaja != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        mArticulosCaja.Clear();
                    }), DispatcherPriority.Normal, null);
                }

                ///Loop por todos los registros de la tabla
                int idventaMostrar = -1;
                float tmpTotal = 0;
                int index = 0;
                foreach (DataRow registro in dataTable.Rows)
                {

                    long cantidad = zfun.toLong(registro["cantidad"].ToString());
                    float precio = zfun.toFloat(registro["precio"].ToString());

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
                    tempArticulo.codigo = zfun.toLong(registro["codigo"].ToString());
                    tempArticulo.descripcion = registro["descripcion"].ToString();

                    //tempArticulo.cantidad = cantidad.ToString();
                    tempArticulo.cantidad = cantidad;
                    //tempArticulo.precio = precio.ToString("0.00");
                    tempArticulo.costo = zfun.toFloat(registro["costo"].ToString());

                    //tempArticulo.subtotal = tmpSubtotal.ToString("0.00");
                    tempArticulo.subtotal = zfun.toFloat(tmpSubtotal.ToString("0.00"));

                    //tempArticulo.cantidad = (long)cantidad;
                    tempArticulo.precio = precio;
                    //tempArticulo.costo = zfun.toFloat( registro["costo"].ToString());
                    //tempArticulo.subtotal =zfun.toFloat(tmpSubtotal.ToString("0.00"));

                    ///formato fecha
                    tempArticulo.fecha = registro[2].ToString();
                    tempArticulo.fechaMostrar = zfun.toFechaMostrar(registro[2].ToString());


                    tempArticulo.totalmostrar = "";
                    if (dataTable.Rows.Count != index + 1)
                    {
                        String tmpSiguienteId = dataTable.Rows[index + 1]["idventa"].ToString();
                        //consola(registro["idventa"].ToString() + "-" + tmpSiguienteId);
                        if (registro["idventa"].ToString() != tmpSiguienteId)
                        {
                            //tempArticulo.totalmostrar = "$" + tmpTotal.ToString("0.00");
                            tempArticulo.totalmostrar = "$ " + tmpTotal.ToString("0.00");
                        }
                    }
                    else
                    {
                        tempArticulo.totalmostrar = tmpTotal.ToString();
                    }
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        mArticulosCaja.Add(tempArticulo);
                    }), DispatcherPriority.Normal, null);

                    index++;
                }


                ///asigno la lista al control listCaja
                //listCaja.ItemsSource = mArticulosCaja;
                //mArticulosCaja.Reverse();

                ///cerrar conexion
                conexion.Close();
            })).Start();
        }
        private void cargarListaDeClientes()
        {
            SQLiteConnection conexion;
            //conexion = new SQLiteConnection("Data Source=clientes.db;Version=3;New=False;Compress=True;");
            conexion = new SQLiteConnection("Data Source=clientes.db;Version=3;New=False;Compress=True;");
            conexion.Open();

            string consulta = "select * from clientes";

            /// Adaptador de datos, DataSet y tabla
            SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            dataSet.Reset();
            db.Fill(dataSet);
            dataTable = dataSet.Tables[0];

            ///borrar todos los elementos de mTotalArticulos
            if (mTotalClientes != null)
            {
                mTotalClientes.Clear();
            }

            ///Loop por todos los registros de la tabla
            foreach (DataRow registro in dataTable.Rows)
            {

                itemCliente tempArticulo = new itemCliente();
                tempArticulo.id = (long)registro["id"];
                tempArticulo.nombre = registro["nombre"].ToString();
                tempArticulo.direccion = registro["direccion"].ToString();
                tempArticulo.telefono = registro["telefono"].ToString();
                tempArticulo.cuit = registro["cuit"].ToString();

                mTotalClientes.Add(tempArticulo);
            }


            ///cerrar conexion
            conexion.Close();

            ///ordenar lista (coleccion)
            //mTotalClientes = new ObservableCollection<itemCliente>(mTotalClientes.OrderBy(i => i));

            ///asignar datos al list
            listFiltroClientes.ItemsSource = mTotalClientes;

            //consola("clientes: " + listFiltroClientes.Items.Count.ToString());

        }
        private void filtroClientes(string filtro = "")
        {
            var articulosFiltrados = from registro in mTotalClientes
                                     where registro.nombre.ToLower().Contains(filtro.ToLower())
                                     orderby registro.nombre
                                     select registro;

            //mTotalArticulosConFiltro = null;
            //mTotalArticulosConFiltro = articulosFiltrados.ToList();
            //listFiltro.ItemsSource = mTotalArticulosConFiltro;
            listFiltroClientes.ItemsSource = articulosFiltrados;


        }
        private void insertarItemClienteEnDB(itemCliente item)
        {
            ///parametros
            string archivo = "clientes.db";
            //string archivo = "pendientes.db";
            string tabla = "clientes";
            //int idMax = -1;

            ///abrir conexion DB
            SQLiteConnection conexion;
            conexion = new SQLiteConnection("Data Source=" + archivo + ";Version=3;New=False;Compress=True;");
            conexion.Open();

            ///comando SQL a ejecutar
            SQLiteCommand insertSQL;
            //insertSQL = new SQLiteCommand("INSERT INTO " + tabla + " (proveedor, codigopro, codigo, descripcion, precio, costo, fechacreacion) VALUES (?,?,?,?,?,?,DATETIME('NOW'))", conexion);
            insertSQL = new SQLiteCommand("INSERT INTO " + tabla + " (nombre, direccion, telefono, cuit) VALUES (?,?,?,?)", conexion);

            insertSQL.Parameters.AddWithValue("nombre", item.nombre.ToString());
            insertSQL.Parameters.AddWithValue("direccion", item.direccion.ToString());
            insertSQL.Parameters.AddWithValue("telefono", item.telefono.ToString());
            insertSQL.Parameters.AddWithValue("cuit", item.cuit);

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

        ///funciones varias
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
                    ///si es tbNombre, acomodo arriba del control, sino abajo
                    if (control.Name == "tbNombre")
                    {
                        labelTip.Margin = new Thickness(control.Margin.Left + 240, control.Margin.Top - 18, control.Margin.Right - 278, 0);
                    }
                    else
                    {
                        labelTip.Margin = new Thickness(control.Margin.Left + 2, control.Margin.Top + control.Height + 2, 0, 0);
                    }
                    //labelTip.Margin = new Thickness(control.Margin.Left + 2, control.Margin.Top + control.Height + 2, 0, 0);
                }
                else if (sender is Border)
                {
                    var control = sender as Border;
                    labelTip.Margin = new Thickness(control.Margin.Left + 2 - 1, control.Margin.Top + control.Height + 2, 0, 0);

                }
                labelTip.Visibility = Visibility.Visible;
            }
            //Console.WriteLine(texto);

        }
        private void tipClienteNuevo(bool mostrar = false)
        {
            if (!mostrar)
            {
                //labelTip2.Visibility = Visibility.Hidden;
                tbIdCliente.Style = StyleTbNoEditable;
                //tbIdCliente.Text = "";
            }
            else
            {
                /////mostrar tip2=articulo nuevo
                TextBox control = tbIdCliente;
                //labelTip2.Content = zAyuda.tipArticuloNuevo;
                //labelTip2.Margin = new Thickness(control.Margin.Left + 2, control.Margin.Top + control.Height + 2, 0, 0);
                //labelTip2.Visibility = Visibility.Visible;

                ///tbIdCliente con style diferente
                tbIdCliente.Style = StyleTbNoEditableNuevo;
                tbIdCliente.Text = "Nuevo";
                //tbCodigo.Text = obtenerCodigoArticuloMax().ToString();
            }
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
        private void MostrarBotones()
        {
            mBotones[mBotonSelected].Focus();
            //gridDatosDelCliente.Visibility = Visibility.Hidden;
            gridDatosDelClienteOcultar();

            int selected = 0;
            foreach (Button item in mBotones)
            {
                //mBotones[selected].Background = App.Current.Resources["sinfoco"] as SolidColorBrush;
                //mBotones[selected].Style = null;
                mBotones[selected].Style = styleBoton;
                //mBotones[selected].BorderThickness = new Thickness(0);

                ///animacion en sí
                aniAnchoBtn.From = mBotones[selected].Margin;
                aniAnchoBtn.To = new Thickness(aniAnchoBtn.From.Value.Left, aniAnchoBtn.From.Value.Top, 0, aniAnchoBtn.From.Value.Bottom);
                mBotones[selected].BeginAnimation(Button.MarginProperty, aniAnchoBtn);

                mBotones[selected].IsEnabled = true;
                selected++;
            }
        }
        private void resetTB()
        {
            tbIdCliente.Tag = "";
            tbNombre.Tag = "";
            tbDireccion.Tag = "";
            tbTelefono.Tag = "";
            tbCuit.Tag = "";
            tbEntrega.Tag = "";

            tbIdCliente.Text = "";
            tbNombre.Text = "";
            tbDireccion.Text = "";
            tbTelefono.Text = "";
            tbCuit.Text = "";
            tbEntrega.Text = "";

            tbNombre.Style = StyleTextboxUpper;
            tbTelefono.Style = StyleTbNoEditable;
            tbDireccion.Style = StyleTbNoEditable;
            tbCuit.Style = StyleTbNoEditable;
            tbEntrega.Style = StyleTbNoEditable;

            tipClienteNuevo();
        }
        private bool tbEstanVacios()
        {
            ///devuelve TRUE si todos los textos estan vacios
            bool respuesta = false;
            if (tbIdCliente.Text == "" &&
                tbNombre.Text == "" &&
                tbDireccion.Text == "" &&
                tbTelefono.Text == "" &&
                tbCuit.Text == "")
            {
                respuesta = true;
            }
            return respuesta;
        }


        ///ASENTAR PROCESOS
        private void AsentarProceso()
        {

            ///si el tbNombre esta vacio, ignorar
            if (tbNombre.Text.Trim() == "")
            {
                tbNombre.Focus();
                return;
            }


            ///si es un cliente nuevo, agregar a la BD
            if (tbIdCliente.Text == "Nuevo")
            {
                ///definir variables y obtener valores de los textbox
                string nombre = "";
                string direccion = "";
                string telefono = "";
                string cuit = "";
                nombre = tbNombre.Text.Trim();
                direccion = tbDireccion.Text.Trim();
                telefono = tbTelefono.Text.Trim();
                cuit = tbCuit.Text.Trim();

                ///crear itemVenta
                itemCliente nuevoItem = new itemCliente
                {
                    nombre = nombre,
                    direccion = direccion,
                    telefono = telefono,
                    cuit = cuit
                };

                ///agregar item a la tabla temporal
                insertarItemClienteEnDB(nuevoItem);

                ///agregar item a mTotalClientes
                //mTotalClientes.Add(nuevoItem);
                cargarListaDeClientes();


            }

            ///si no es un cliente nuevo, pero se modificaron los datos del cliente, actualizar BD

            ///variables:
            //string archivoDB = "|123456";
            string archivoDB = "clientes.db";
            string tabla = "clientes";

            string index = tbIdCliente.Text.ToString();
            string campo = "";
            string valor = "";


            //en textChanged, si el valor de los TB es diferente al tag, poner el fondo rosado
            //acá evaluar si el fondo es rosado
            //ir TB por TB, si es rosado, hacer un UPDATE de ese campo

            ///si encuentra el item, ver si se hicieron modificaciones
            if (mTotalClientes.Any((i => i.id.ToString() == index)))
            {

                var item = mTotalClientes.First(i => i.id.ToString() == index);
                if (item != null)
                {

                    ///modificacion en telefono
                    if (tbTelefono.Style == StyleTextboxModificar)
                    {
                        //consola("grabar modificación en teléfono");
                        campo = "telefono";
                        valor = tbTelefono.Text.ToString();
                        zdb.modificarRegistroDB(archivoDB, tabla, index, campo, valor);
                        item.telefono = valor;
                    }

                    ///modificacion en direccion
                    if (tbDireccion.Style == StyleTextboxModificar)
                    {
                        //consola("grabar modificación en Dirección");
                        campo = "direccion";
                        valor = tbDireccion.Text.ToString();
                        zdb.modificarRegistroDB(archivoDB, tabla, index, campo, valor);
                        item.direccion = valor;
                    }

                    ///modificacion en cuit
                    if (tbCuit.Style == StyleTextboxModificar && tbCuit.IsVisible == true)
                    {
                        //consola("grabar modificación en cuit");
                        campo = "cuit";
                        valor = tbCuit.Text.ToString();
                        zdb.modificarRegistroDB(archivoDB, tabla, index, campo, valor);
                        item.cuit = valor;
                    }

                }
            }

            ///comprobar proceso a realizar para guardar en BD del proceso
            string proceso = btnImprimir.Tag.ToString();
            //consola("PROCESO:" + proceso);

            ///REMITO
            if (proceso == "remito")
            {
                //AsentarProcesoRemito();
                asentarRegistrosEnDB(proceso, "remitos.db", "remitos", "remitonro");
            }

            ///FACTURA
            if (proceso == "factura")
            {
                //AsentarProcesoFactura();
                asentarRegistrosEnDB(proceso, "facturas.db", "facturas", "facturanro");
            }
            ///PENDIENTE
            if (proceso == "pendiente")
            {
                asentarRegistrosEnDB(proceso, "pendientes.db", "pendientes", "pendientenro");
            }



            ///Reset estado
            MainWindow.statUcConsultas.ActualizarListConsultasDesdeDB();
            resetTB();
            tbNombre.Focus();
            MostrarBotones();
        }
        private void asentarRegistrosEnDB(string proceso, string archivoDB, string tabla, string nombreCampoNro)
        {

            /// 0) definir variables

            /// 0.1) variables archivo
            //string archivoDB = "remitos.db";
            //string tabla = "remitos";
            string parametros = "";

            long nroProceso = -2;
            if (proceso == "remito")
            {
                nroProceso = zdb.valorMaxDB(archivoDB, tabla, "remitonro");
                if (nroProceso > -1)
                {
                    nroProceso++;
                }
            }
            if (proceso == "factura")
            {
                nroProceso = zfun.toLong(tbFacturaNro.Text);
            }
            if (proceso == "pendiente")
            {
                nroProceso = zdb.valorMaxDB(archivoDB, tabla, "pendientenro");
                if (nroProceso > -1)
                {
                    nroProceso++;
                }
            }

            if ((nroProceso == -2) || (nroProceso == -1))
            {
                MessageBox.Show("No se asignó correctamente la variable 'nroProceso'. El proceso se canceló.", "error");
                return;
            }

            /// 0.2) variables venta
            string fecha = "";

            long codigo = -1;
            string descripcion = "";
            long cantidad = 0;
            float precio = 0;
            string precioStr = "0";
            float subtotal = 0;
            string subtotalStr;
            string direccion = "";
            string telefono = "";
            string cuit = "";

            /// 1) cargar el primer registro con los datos del cliente y del proceso

            /// 1.1) Datos del primer registro: 'codigo'='-100', 'descripcion'=nombre del cliente, 'subtotal'= total de la factura.
            //fecha = "DATETIME('NOW')";
            fecha = zfun.getFechaNow();
            codigo = -100;
            descripcion = tbNombre.Text.Trim();
            subtotal = toFloat(tbTotal.Text.Replace("$", ""));
            subtotalStr = subtotal.ToString().Replace(",", ".");
            direccion = tbDireccion.Text.Trim();
            telefono = tbTelefono.Text.Trim();
            cuit = tbCuit.Text.Trim();

            /// 1.2) ejecutar comando SQL
            if (proceso == "remito")
            {
                parametros = "(" + nombreCampoNro + ", fecha, codigo, descripcion, cantidad, precio, subtotal, direccion, telefono) VALUES" +
                    "(" + nroProceso + "," + fecha + ",'" + codigo + "','" + descripcion + "'," + cantidad + "," + precioStr + "," + subtotalStr + ",'" + direccion + "','" + telefono + "')";
            }
            if (proceso == "factura")
            {
                parametros = "(" + nombreCampoNro + ", fecha, codigo, descripcion, cantidad, precio, subtotal, direccion, telefono, cuit) VALUES" +
                    "(" + nroProceso + "," + fecha + ",'" + codigo + "','" + descripcion + "'," + cantidad + "," + precioStr + "," + subtotalStr + ",'" + direccion + "','" + telefono + "','" + cuit + "')";
            }
            if (proceso == "pendiente")
            {
                parametros = "(" + nombreCampoNro + ", fecha, codigo, descripcion, cantidad, precio, subtotal, direccion, telefono) VALUES" +
                    "(" + nroProceso + "," + fecha + ",'" + codigo + "','" + descripcion + "'," + cantidad + "," + precioStr + "," + subtotalStr + ",'" + direccion + "','" + telefono + "')";
            }

            zdb.InsertDB(archivoDB, tabla, parametros);


            /// 2) el resto de los registros son los mismos de la listImpresion, pero compartiendo el nro de remito

            /// 2.1) procesar cada articulo de la listImpresion
            direccion = "";
            telefono = "";
            cuit = "";

            foreach (var itemventa in listImpresion.Items)
            {
                var item = itemventa as itemCaja;

                ///setear variables:
                codigo = 0;
                descripcion = "";
                cantidad = 0;
                precio = 0;
                subtotal = 0;

                codigo = item.codigo;
                descripcion = item.descripcion;
                cantidad = item.cantidad;
                precio = toFloat(item.precio.ToString());
                precioStr = precio.ToString().Replace(",", ".");
                subtotal = item.subtotal;
                subtotalStr = subtotal.ToString().Replace(",", ".");
                fecha = "DATETIME('" + item.fecha + "')";


                ///comando sql para agregar el registro del articulo
                if (proceso == "remito")
                {
                    parametros = "(" + nombreCampoNro + ", fecha, codigo, descripcion, cantidad, precio, subtotal, direccion, telefono) VALUES" +
                        "(" + nroProceso + "," + fecha + ",'" + codigo + "','" + descripcion + "'," + cantidad + "," + precioStr + "," + subtotalStr + ",'" + direccion + "','" + telefono + "')";
                }
                if (proceso == "factura")
                {
                    parametros = "(" + nombreCampoNro + ", fecha, codigo, descripcion, cantidad, precio, subtotal, direccion, telefono, cuit) VALUES" +
                        "(" + nroProceso + "," + fecha + ",'" + codigo + "','" + descripcion + "'," + cantidad + "," + precioStr + "," + subtotalStr + ",'" + direccion + "','" + telefono + "','" + cuit + "')";
                }
                if (proceso == "pendiente")
                {
                    parametros = "(" + nombreCampoNro + ", fecha, codigo, descripcion, cantidad, precio, subtotal, direccion, telefono) VALUES" +
                        "(" + nroProceso + "," + fecha + ",'" + codigo + "','" + descripcion + "'," + cantidad + "," + precioStr + "," + subtotalStr + ",'" + direccion + "','" + telefono + "')";
                }

                zdb.InsertDB(archivoDB, tabla, parametros);
            }

            ///PROCESOS POSTERIORES

            ///actualizar datos a mostrar
            if (proceso == "remito")
            {
                //ucConsultas.actualizarListConsultas = true;
            }
            if (proceso == "factura")
            {
                ///grabar el nuevo nro de factura
                zdb.grabarConfig("nroFactura", nroProceso.ToString());
            }
            if (proceso == "pendiente")
            {
                ///agregar si hizo una 'entrega'
                string importe = tbEntrega.Text.Trim();
                if (tbEntrega.Text != "")
                {
                    string nroPendiente = nroProceso.ToString().Trim();
                    string nombre = "EsEntregaNoPago";
                    MainWindow.statUcConsultas.cargarImporteAPendiente(nroPendiente, nombre, importe);
                }


                ///agregar al registro de 'balanceCaja' en datos.db
                string monto = tbTotal.Text.ToString().Replace("$", "").Trim();
                zdb.balanceCajaDB(monto, true);
                ///actualizar balance
                MainWindow.statucInicio.calcularTotalBalance();
            }

            ///volver a la pestaña anterior y poner el foco en listCaja
            tabCaja.SelectedIndex = 0;
            if (listCaja.SelectedIndex == -1)
            {
                listCaja.SelectedIndex = 0;
            }
            var itemList = listCaja.ItemContainerGenerator.ContainerFromIndex(listCaja.SelectedIndex) as ListBoxItem;
            if (itemList != null)
            {
                itemList.Focus();
            }

        }
        private void imprimir(string proceso)
        {
            ///variables
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

        ///CONTROLES
        private void tabCaja_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tab = sender as TabControl;
                int selected = tab.SelectedIndex;
                //consola(e.Source.ToString());

                /// Hacer la animacion sólo si esta seleccionada la pestana CAJA
                if (MainWindow.mPestanaMain == 1)
                {
                    if (selected != -1)
                    {


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
                        ta.Duration = new Duration(TimeSpan.FromSeconds((double)App.Current.Resources["TiempoAnimacion"]));
                        ta.EasingFunction = easing;
                        ta.Completed += (s, e2) =>
                        {
                            ///pestana VENTAS
                            if (selected == 0)
                            {
                                ///FOCO en listCaja
                                if (listCaja.SelectedIndex == -1)
                                {
                                    listCaja.SelectedIndex = 0;
                                }
                                var item = listCaja.ItemContainerGenerator.ContainerFromIndex(listCaja.SelectedIndex) as ListBoxItem;
                                if (item != null)
                                {
                                    item.Focus();
                                }
                            }
                            ///pestana Impresiones
                            if (selected == 1)
                            {
                                ///FOCO en botones
                                //if (ucImpresiones.listCaja.SelectedIndex == -1)
                                //{
                                //    ucImpresiones.listCaja.SelectedIndex = 0;
                                //}
                                //var item = ucImpresiones.listCaja.ItemContainerGenerator.ContainerFromIndex(ucImpresiones.listCaja.SelectedIndex) as ListBoxItem;
                                //if (item != null)
                                //{
                                //    item.Focus();
                                //}
                                //btnRemito.Focus();

                                seleccionarVenta();

                                /// si el boton tiene el style de selecionado, hace foco en el textbox tbNombre, sino en el boton mBotonSelected
                                if (mBotones[mBotonSelected].Style == styleBotonSelected)
                                {
                                    tbNombre.Focus();
                                }
                                else
                                {
                                    mBotones[mBotonSelected].Focus();
                                }
                            }

                        };

                        gridCajaSlide.BeginAnimation(Grid.MarginProperty, ta);

                    }

                    ///Index Pestana CAJA seleecionada
                    mPestanaCaja = selected;

                }
                ///color textblock de las pestañas en tabCaja
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
            }
            e.Handled = true;

        }

        private void gridDatosDelCliente_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ///tecla ESC
            if (e.Key == Key.Escape)
            {
                //mBotones[mBotonSelected].Focus();
                ////gridDatosDelCliente.Visibility = Visibility.Hidden;
                //gridDatosDelClienteOcultar();
                ///si estoy filtrando, que se cierre el listFiltroClientes
                if (listFiltroClientes.Visibility == Visibility.Visible)
                {
                    listFiltroOcultar();

                }
                else
                {
                    ///si los TB estan vacios, ocultar el gridCliente y mostrar botones de opciones
                    if (tbEstanVacios())
                    {
                        MostrarBotones();
                    }
                    else
                    ///si alguno de los TB de datos del cliente tiene texto, resetear TB
                    {
                        resetTB();
                        tbNombre.Focus();
                    }
                }

            }
        }

        private void gridDatosDelCliente_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var objeto = sender as Grid;
            Boolean mostrar = objeto.IsVisible;
            //consola(list.IsVisible.ToString());

            if (mostrar)
            {
                sbDatosDelClienteMostrar.Begin();

                //} else
                //{
                //    sbDatosDelClienteOcultar.Begin();
            }
        }
        private void gridDatosDelClienteOcultar()
        {
            sbDatosDelClienteOcultar.Begin();
        }

        private void Button_GotFocus(object sender, RoutedEventArgs e)
        {
            ayuda(zAyuda.bontonMenu1a, zAyuda.bontonMenu1b);
        }
        private void Button_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var boton = sender as Button;
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                //gridDatosDelCliente.Visibility = Visibility.Visible;
                //tbNombre.Focus();
                ActivarBotonMenu(boton);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                /// volver a la pestana "movimientos"
                tabCaja.SelectedIndex = 0;
            }
            else if (e.Key == Key.Up)
            {
                if (boton.Name == "btnRemito")
                {
                    btnListaDeControl.Focus();
                    e.Handled = true;
                    return;
                }
                else if (boton.Name == "btnPendiente")
                {
                    btnRemito.Focus();
                    e.Handled = true;
                    return;
                }
                else if (boton.Name == "btnFactura")
                {
                    btnPendiente.Focus();
                    e.Handled = true;
                    return;
                }
                else if (boton.Name == "btnTarjeta")
                {
                    btnFactura.Focus();
                    e.Handled = true;
                    return;
                }
                else if (boton.Name == "btnListaDeControl")
                {
                    btnTarjeta.Focus();
                    e.Handled = true;
                    return;
                }

            }
            else if (e.Key == Key.Down)
            {
                if (boton.Name == "btnRemito")
                {
                    btnPendiente.Focus();
                    e.Handled = true;
                    return;
                }
                else if (boton.Name == "btnPendiente")
                {
                    btnFactura.Focus();
                    e.Handled = true;
                    return;
                }
                else if (boton.Name == "btnFactura")
                {
                    btnTarjeta.Focus();
                    e.Handled = true;
                    return;
                }
                else if (boton.Name == "btnTarjeta")
                {
                    btnListaDeControl.Focus();
                    e.Handled = true;
                    return;
                }
                else if (boton.Name == "btnListaDeControl")
                {
                    btnRemito.Focus();
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                e.Handled = true;
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;

            if (boton.Style == styleBoton)
            {
                ActivarBotonMenu(boton);
            }
            else
            {
                MostrarBotones();
            }

            e.Handled = true;
        }
        private void ActivarBotonMenu(Button boton = null)
        {
            ///CANCELAR si es uno de los botones que todavia no tienen funcion
            if (boton.Name == "btnTarjeta" || boton.Name == "btnListaDeControl")
            {
                return;
            }

            int selected = 0;
            foreach (Button item in mBotones)
            {
                //mBotones[selected].BorderThickness = new Thickness(10,10,10,10); 

                //string tmpTexto = item.Name;
                if (item.Name == boton.Name)
                {
                    //tmpTexto = item + tmpTexto + " -SELECTED";
                    mBotonSelected = selected;
                }
                else
                {
                    ///animacion en sí
                    aniAnchoBtn.From = mBotones[selected].Margin;
                    aniAnchoBtn.To = new Thickness(aniAnchoBtn.From.Value.Left, aniAnchoBtn.From.Value.Top, botonXTo, aniAnchoBtn.From.Value.Bottom);

                    mBotones[selected].BeginAnimation(Button.MarginProperty, aniAnchoBtn);
                    mBotones[selected].IsEnabled = false;

                }
                //consola(tmpTexto);
                selected++;
            }

            //mBotones[mBotonSelected].Background = App.Current.Resources["confoco2"] as SolidColorBrush;
            mBotones[mBotonSelected].Style = styleBotonSelected;

            labEntrega.Visibility = Visibility.Hidden;
            tbEntrega.Visibility = Visibility.Hidden;
            labFacturaNro.Visibility = Visibility.Hidden;
            tbFacturaNro.Visibility = Visibility.Hidden;
            labCuit.Visibility = Visibility.Hidden;
            tbCuit.Visibility = Visibility.Hidden;
            btnImprimir.Content = "IMPRIMIR";

            bool mostrarGridCliente = true;

            ///REMITO
            if (mBotones[mBotonSelected].Name == "btnRemito")
            {
                btnImprimir.Tag = "remito";
            }

            ///PENDIENTE
            if (mBotones[mBotonSelected].Name == "btnPendiente")
            {
                labEntrega.Visibility = Visibility.Visible;
                tbEntrega.Visibility = Visibility.Visible;
                btnImprimir.Content = "GUARDAR";

                btnImprimir.Tag = "pendiente";
            }

            ///FACTURA
            if (mBotones[mBotonSelected].Name == "btnFactura")
            {
                ///consultar y aumentar el nro de factura (leer desde BD configuracion.db)
                string ftemp = zdb.leerConfig("nroFactura");
                long facturaNro = zfun.toLong(ftemp);
                facturaNro++;
                tbFacturaNro.Text = facturaNro.ToString();

                labFacturaNro.Visibility = Visibility.Visible;
                tbFacturaNro.Visibility = Visibility.Visible;
                labCuit.Visibility = Visibility.Visible;
                tbCuit.Visibility = Visibility.Visible;

                btnImprimir.Tag = "factura";
            }

            ///TARJETA
            if (mBotones[mBotonSelected].Name == "btnTarjeta")
            {
                //btnImprimir.Content = "GUARDAR";

                btnImprimir.Tag = "tarjeta";

                mostrarGridCliente = false;
            }

            ///LISTA DE CONTROL
            if (mBotones[mBotonSelected].Name == "btnListaDeControl")
            {
                btnImprimir.Tag = "lista_de_control";
                mostrarGridCliente = false;
            }


            ///mostrar grid datos del cliente
            if (mostrarGridCliente)
            {
                gridDatosDelCliente.Visibility = Visibility.Visible;
                tbNombre.Focus();

            }

        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            string nombre = tb.Name;
            ///ENTER
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (nombre == "tbCuit")
                {
                    if (tbEntrega.IsVisible)
                    {

                        tbEntrega.Focus();
                    }
                    else
                    {
                        btnImprimir.Focus();
                    }

                }
                else if (nombre == "tbDireccion")
                {
                    if (tbCuit.IsVisible)
                    {
                        tbCuit.Focus();
                    }
                    else
                    {

                        if (tbEntrega.IsVisible)
                        {

                            tbEntrega.Focus();
                        }
                        else
                        {
                            btnImprimir.Focus();
                        }
                    }

                }
                else if (nombre == "tbTelefono")
                {
                    tbDireccion.Focus();
                }
                else if (nombre == "tbEntrega")
                {
                    //consola("hola");
                    btnImprimir.Focus();
                }
            }

        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Name == "tbNombre")
            {
                if (tb.Text == "")
                {
                    ayuda(zAyuda.tbNombreVacioEnGridClientes);
                }
                else
                {
                    ayuda(zAyuda.tbNombreEnGridClientes1, zAyuda.tbNombreEnGridClientes2);
                }
            }
            else
            {
                ayuda(zAyuda.tbEnGridClientes);
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            string text = tb.Text;
            string tag = "";
            if (tb.Tag != null)
            {
                tag = tb.Tag.ToString();
            }

            ///Ver si se modificaron los datos de los TB (comparando con el campo Tag)

            ///Si son iguales, textbox con fondo blanco
            if (text.Equals(tag))
            {
                if (tb.Style != StyleTextbox)
                {
                    tb.Style = StyleTextbox;
                }
            }
            ///si son diferentes, tb con fondo rosado y tip diciendo q se grabaran los cambios
            else
            {
                ///mostrar que se modificara solo si no es un usuario nuevo
                if (tbIdCliente.Text != "Nuevo")
                {

                    //consola("no son iguales:" + text + "-" + tag + ".");
                    if (tb.Style != StyleTextboxModificar)
                    {
                        tb.Style = StyleTextboxModificar;
                        tip("Se grabarán las modificaciones en la Base de Datos", bordeBlancoGridCliente);
                    }
                }
            }
        }
        private void tbNombre_TextChanged(object sender, TextChangedEventArgs e)
        {

            ///si anteriormente se busco un articulo y no se encontro, borrar mensaje al editer tbDescripcion
            //if (labelTip.Content.ToString() == zAyuda.articuloNoEncontrado)
            //{
            //    tip();
            //}


            ///borrar el tbCodigo ya que si se edita la descripcion, deja de ser ESE articulo
            //if (seEditoDescripcionDesdeElPrograma)
            //{
            //    seEditoDescripcionDesdeElPrograma = false;
            //    e.Handled = true;
            //    return;
            //}
            //else
            //{
            //    tbCodigo.Text = "";

            //}

            var textBox = sender as TextBox;
            string filtro = textBox.Text.Trim();
            //consola(listFiltroClientes.Items.Count.ToString());
            if (filtro != "" && textBox.IsFocused)
            {
                ///texto filtro NO esta vacio

                ///si es una letra

                ///aplicar filtro
                //gridFiltroSQL(filtro);
                filtroClientes(filtro);

                ///mostrar tip paa agregar nuevo articulo
                if (tbNombre.IsFocused)
                {
                    tip(zAyuda.tipAgregarNuevoCliente, tbNombre);
                }

                ///mostrar list si hay resultados
                if (listFiltroClientes.Items.Count > 0)
                {
                    listFiltroClientes.Visibility = Visibility.Visible;
                    //ayuda(zAyuda.descripcion2a, zAyuda.descripcion2b);
                    ayuda(zAyuda.tbNombreEnGridClientes1, zAyuda.tbNombreEnGridClientes2);
                }
                else
                {
                    //ayuda(zAyuda.descripcion3);
                    ayuda(zAyuda.tbNombreEnGridClientesCrearNuevo1, zAyuda.tbNombreEnGridClientesCrearNuevo2);
                    ///ocultar control si no hay resultados
                    listFiltroOcultar();
                }

                tbTelefono.Style = StyleTextbox;
                tbDireccion.Style = StyleTextboxUpper;
                tbCuit.Style = StyleTextbox;
                tbEntrega.Style = StyleTextbox;


            }
            else
            {
                ///texto filtro vacio
                ayuda(zAyuda.tbNombreVacioEnGridClientes);
                tip();
                /////ocultar control si el text esta vacio
                listFiltroOcultar();

                //tbTelefono.Style = StyleTbNoEditable;
                //tbDireccion.Style = StyleTbNoEditable;
                //tbCuit.Style = StyleTbNoEditable;
                //tbEntrega.Style = StyleTbNoEditable;

            }
        }
        private void tbNombre_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            ///ENTER
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {

                ///si esta vacio tbNombre, no hacer nada
                if (tbNombre.Text.Trim() == "")
                {
                    //if (listVenta.Items.Count > 0)
                    //{
                    //    tbPagaCon.Focus();
                    //}
                    string proceso = btnImprimir.Tag.ToString();
                    if (proceso == "remito" || proceso == "factura")
                    {
                        tbTelefono.Focus();
                        seEditoDescripcionDesdeElPrograma = true;
                        tbNombre.Text = "Consumidor Final";
                        //listFiltroOcultar();
                    }
                    e.Handled = true;
                    return;
                }
                else
                ///si no esta vacio tbNombre
                {
                    //        ///comprobar si es un numero (para buscar articulo por codigo)
                    //        if (mBuscarArticuloPorCodigo)
                    //        {
                    //            buscarArticuloPorCodigo();
                    //            e.Handled = true;
                    //        }
                    //        else
                    //        ///si no es un numero pero tampoco se selecciono de la lista, entonces es un ARTICULO NUEVO
                    //        {
                    if (estadoTip(zAyuda.tipAgregarNuevoCliente))
                    {
                        tipClienteNuevo(true);
                    }
                    //            ///tbCodigo = codigoMax 
                    //            tbCodigo.Text = obtenerCodigoArticuloMax().ToString();
                    //            tbPrecio.Tag = "";
                    //            tbCantidad.Tag = "";

                    //            tbCantidad.Text = "1";
                    //            tbCantidad.SelectAll();
                    //            tbCantidad.Focus();
                    tip();
                    listFiltroOcultar();

                    //        }
                }
                tbTelefono.Focus();
            }
            ///FLECHA ABAJO
            if (e.Key == Key.Down)
            {
                //listFiltro.Focus();
                if (listFiltroClientes.Visibility == Visibility.Visible)
                {
                    listFiltroClientes.SelectedIndex = 0;
                    var item = listFiltroClientes.ItemContainerGenerator.ContainerFromIndex(listFiltroClientes.SelectedIndex) as ListBoxItem;
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
                //resetTb();
            }
        }
        private void tbNombre_LostFocus(object sender, RoutedEventArgs e)
        {
            if (estadoTip(zAyuda.tipAgregarNuevoCliente))
            {
                tip();
            }
        }

        private void imgCerrarDatosCliente_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            MostrarBotones();
        }

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
                seleccionarVenta();
            }

        }
        private void seleccionarVenta()
        {
            //var list = sender as ListView;
            var list = listCaja;

            var selected = list.SelectedItem;
            var fila = selected as itemCaja;


            //string codigopro = ((fila.codigo == "" || fila.codigo == null) ? "" : fila.codigo.ToString());
            //string descripcion = fila.descripcion.ToString();
            //string precio = fila.precio.ToString("0.00");

            string fecha = fila.fechaMostrar;
            string idVenta = fila.idventa.ToString();

            ///calcular total
            string idven = fila.idventa.ToString();

            var filasFiltradas = from registro in mArticulosCaja
                                 where registro.idventa.ToString().Equals(idven)
                                 select registro;
            float sumador = 0;
            foreach (itemCaja item in filasFiltradas)
            {
                sumador += item.subtotal;
            }
            //consola(fila.idventa.ToString() + ":" + sumador.ToString());
            //string total = fila.totalmostrar.ToString();
            string total = sumador.ToString("0.00");


            tbFecha.Text = fecha;
            tbIdVenta.Text = idVenta;
            tbTotal.Text = "$ " + total;

            //tbCodigo.Text = codigopro;
            //seEditoDescripcionDesdeElPrograma = true;
            //tbDescripcion.Text = descripcion;
            //tbPrecio.Text = precio;
            //tbPrecio.Tag = precio;

            //tbCantidad.Tag = fila.costo;

            //tbCantidad.Focus();
            //listFiltroClientes.Visibility = Visibility.Hidden;

            //consola("Venta Nro:" + fila.idventa.ToString());
            //MessageBox.Show("Crear campos (nombre, direccion, cuit, etc. Qu");

            ///consultar mArticulosCaja para filtrar solo los que coinciden con ese nro idVenta
            var listaVentaId = from registro in mArticulosCaja
                               where registro.idventa.Equals(Int32.Parse(idVenta))
                               select registro;
            listImpresion.ItemsSource = listaVentaId;


            tabCaja.SelectedIndex = 1;
        }

        private void listFiltroOcultar()
        {
            sbListFiltroClientesOcultar.Begin();
        }
        private void listFiltroClientes_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var list = sender as ListView;
            Boolean mostrar = list.IsVisible;

            if (mostrar)
            {
                sbListFiltroClientesMostrar.Begin();

            }

        }
        private void listFiltroClientes_KeyDown(object sender, KeyEventArgs e)
        {
            var list = sender as ListView;
            var item = list.SelectedItem as itemCliente;
            if (e.Key == Key.Escape)
            {
                tbNombre.Focus();
            }
            else if (e.Key == Key.Enter)
            {


                //string codigo = ((item.codigo == "" || item.codigo == null) ? "" : item.codigo.ToString());
                string id = item.id.ToString();
                string nombre = item.nombre.ToString();
                string direccion = item.direccion.ToString();
                string telefono = item.telefono.ToString();
                string cuit = item.cuit.ToString();

                seEditoDescripcionDesdeElPrograma = true;

                tbIdCliente.Tag = id;
                tbNombre.Tag = nombre;
                tbDireccion.Tag = direccion;
                tbTelefono.Tag = telefono;
                tbCuit.Tag = cuit;

                tbIdCliente.Text = id;
                tbNombre.Text = nombre;
                tbDireccion.Text = direccion;
                tbTelefono.Text = telefono;
                tbCuit.Text = cuit;

                tbTelefono.Focus();
                tbNombre.Style = StyleTBNoEditableFondo2Left;

                listFiltroOcultar();

                //e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
            }
        }
        private void listFiltroClientes_GotFocus(object sender, RoutedEventArgs e)
        {
            ayuda(zAyuda.ListFiltroClientes);
        }


        private void btnImprimir_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                AsentarProceso();
                e.Handled = true;
            }
        }
        private void btnImprimir_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AsentarProceso();
            e.Handled = true;

        }

    }
}
