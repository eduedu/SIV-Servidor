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

using Bespoke.Common;
using Bespoke.Common.Osc;
using System.Windows.Threading;
using System.Net;

//using System;
using System.IO;
using System.Data.SQLite; //Utilizamos la DLL
using System.Data;


namespace SIV_Servidor {
	public partial class MainWindow : Window {
		///Variables Globales
		List<articuloClass> mArticulos;
		//static SQLiteConnection conexion;

		///MAIN
		public MainWindow() {
			InitializeComponent();
			mArticulos = new List<articuloClass>();

			///SETEAR CONTROLES
			//gridFiltro.Visibility = Visibility.Hidden;
			listFiltro.Visibility = Visibility.Hidden;
			listFiltro.Margin = new Thickness(tbBuscar.Margin.Left, tbBuscar.Margin.Top + tbBuscar.Height + 2, 0, 0);
			tbBuscar.Focus();


			///FUNCIONES DE INICIO
			//iniciarOSC();
			//gridFiltroSQL();
			cargarListaDeArticulos();
		}
		///---------------------------------------------------------------------------

		///OSC 
		public void iniciarOSC() {
			//Console.WriteLine("hola");

			OscServer oscServer;

			//DemoType demoType = GetDemoType();
			DemoType demoType = DemoType.Udp;
			IPAddress miIp;
			//miIp=IPAddress.Loopback;
			//miIp= IP DEL SERVER:
			miIp = IPAddress.Parse("192.168.0.9");

			oscServer = new OscServer(TransportType.Udp, miIp, MiOSC.Port);
			//oscServer = new OscServer(miIp, Port);
			//oscServer = new OscServer(IPAddress.Parse("224.25.26.27"), Port);

			/*
			switch (demoType)
			{
					case DemoType.Udp:
							oscServer = new OscServer(TransportType.Udp, miIp, Port);
							break;

					case DemoType.Tcp:
							oscServer = new OscServer(TransportType.Tcp, IPAddress.Loopback, Port);
							break;

					case DemoType.Multicast:
							oscServer = new OscServer(IPAddress.Parse("224.25.26.27"), Port);
							break;

					default:
							throw new Exception("Unsupported receiver type.");
			}
			*/
			//Console.WriteLine("IP ADDRESSSSSSSSSSSSSS:" + IPAddress.Loopback);

			oscServer.FilterRegisteredMethods = false;
			oscServer.RegisterMethod(MiOSC.AliveMethod);
			oscServer.RegisterMethod(MiOSC.TestMethod);
			oscServer.BundleReceived += new EventHandler<OscBundleReceivedEventArgs>(MiOSC.oscServer_BundleReceived);
			//oscServer.MessageReceived += new EventHandler<OscMessageReceivedEventArgs>(MiOSC.oscServer_MessageReceived);
			oscServer.MessageReceived += new EventHandler<OscMessageReceivedEventArgs>(NuevoMensajeOSC);
			oscServer.ReceiveErrored += new EventHandler<Bespoke.Common.ExceptionEventArgs>(MiOSC.oscServer_ReceiveErrored);
			oscServer.ConsumeParsingExceptions = false;

			oscServer.Start();


			Console.WriteLine("Osc Receiver: " + demoType.ToString());
			Console.WriteLine("Press any key to exit.");
			//Console.ReadKey();
			//oscServer.Stop();
		}
		public void NuevoMensajeOSC(object sender, OscMessageReceivedEventArgs e) {
			MiOSC.sMessagesReceivedCount++;

			OscMessage message = e.Message;

			Console.WriteLine(string.Format("\nMessage Received [{0}]: {1}", message.SourceEndPoint.Address, message.Address));
			Console.WriteLine(string.Format("Message contains {0} objects.", message.Data.Count));

			//ACCION LAYOUT
			Application.Current.Dispatcher.BeginInvoke((Action)(() => {
				Label1.Content = "";
				//txtUrlCompartirTitulo.Text = mCambioUrlCompartirTitulo;
			}), DispatcherPriority.Normal, null);


			for (int i = 0; i < message.Data.Count; i++) {
				string dataString;

				if (message.Data[i] == null) {
					dataString = "Nil";
				}
				else {
					dataString = (message.Data[i] is byte[] ? BitConverter.ToString((byte[])message.Data[i]) : message.Data[i].ToString());
				}
				Console.WriteLine(string.Format("[{0}]: {1}", i, dataString));

				//ACCION LAYOUT
				Application.Current.Dispatcher.BeginInvoke((Action)(() => {
					Label1.Content = Label1.Content + "Mensaje:" + dataString;
				}), DispatcherPriority.Normal, null);

			}

			Console.WriteLine("Total Messages Received: {0}", MiOSC.sMessagesReceivedCount);
		}

		///CONTROLES
		private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var grid = sender as DataGrid;
			if (grid.SelectedItem != null) {
				var selected = grid.SelectedItem;
				// ... Set Title to selected names.
				//this.Title = string.Join(", ", names);

				/*var item = selected as usuarioClass;
				if (item != null) {
					//labelid.Content = item.id.ToString();
					lNombre.Content = item.nya.ToString();
					string tmpN = item.num.ToString();
					if (tmpN.Length > 4) {
						tmpN = tmpN.Substring(0, 3) + "-" + tmpN.Substring(3, tmpN.Length - 3);
					}
					lNumero.Content = tmpN;
				}
				*/
				string tmpTexto = "";
				Console.WriteLine(selected);
				if (selected.ToString() == "System.Data.DataRowView") {
					DataRowView drv = (DataRowView)grid.SelectedItem;

					String valor = drv[3].ToString();

					tmpTexto = valor;
					//int columna = grid.CurrentColumn.DisplayIndex;
					//valor = columna.ToString();
					tmpTexto = valor;
					labelPreview.Content = tmpTexto;
				}
			}
		}
		private void tbBuscar_TextChanged(object sender, TextChangedEventArgs e) {
			var textBox = sender as TextBox;
			string filtro = textBox.Text.Trim();
			if (filtro != "") {
				///aplicar filtro
				//gridFiltroSQL(filtro);
				filtroArticulos(filtro);
				listFiltro.Visibility = Visibility.Visible;
			}
			else {
				///ocultar control si el text esta vacio
				listFiltro.Visibility = Visibility.Hidden;
			}

		}
		private void tbBuscar_PreviewKeyDown(object sender, KeyEventArgs e) {
			//Console.WriteLine(e.Key.ToString());
			if (e.Key == Key.Down) {
				//Console.WriteLine("DOWN");
				//gridFiltro.SelectedItem = 0;
				//gridFiltro.Focus();
				//tbBuscar.MoveFocus(gridFiltro);
				listFiltro.Focus();
			}
		}
		private void tbBuscar_PreviewKeyUp(object sender, KeyEventArgs e) {

		}
		private void listFiltro_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				//Console.WriteLine("ESCAPE");
				//gridFiltro.SelectedItem = 0;
				//gridFiltro.Focus();
				//tbBuscar.MoveFocus(gridFiltro);
				tbBuscar.Focus();
			}
			if (e.Key == Key.Enter) {
				//Console.WriteLine("ENTER");
				var list = sender as ListView;
				/*
				DataRowView fila = (DataRowView)list.SelectedItem;
				String codigo = fila["codigo"].ToString();
				String descripcion = fila["descripcion"].ToString();
				String precio = fila["precio"].ToString();
				*/

				var selected= list.SelectedItem;
				var fila = selected as articuloClass;
				
				string codigopro= ((fila.codigo =="" || fila.codigo == null) ? "-": fila.codigo.ToString());
				string descripcion = fila.descripcion.ToString();
				string precio = fila.precio.ToString();
				
				tbCodigo.Text = codigopro;
				tbDescripcion.Text = descripcion;
				tbPrecio.Text = precio;


				tbBuscar.Text = "";
				tbBuscar.Focus();
				//listFiltro.Visibility = Visibility.Hidden;
			}
		}

		///FUNCIONES BD
		private void gridFiltroSQL(string filtro = "") {
			SQLiteConnection conexion;
			conexion = new SQLiteConnection("Data Source=lista.db;Version=3;New=False;Compress=True;");
			conexion.Open();

			// Lanzamos la consulta y preparamos la estructura para leer datos
			string consulta = "select * from lista where descripcion like '%" + filtro + "%' ";

			// Adaptador de datos, DataSet y tabla
			SQLiteDataAdapter db = new SQLiteDataAdapter(consulta, conexion);

			DataSet ds = new DataSet();
			ds.Reset();

			DataTable dt = new DataTable();
			db.Fill(ds);

			//Asigna al DataTable la primer tabla (ciudades) 
			// y la mostramos en el DataGridView
			dt = ds.Tables[0];

			//dataGrid.DataSource = dt;
			//dataGrid.DataContext = dt.DefaultView;  //esto anda
			gridFiltro.ItemsSource = dt.DefaultView;
			listFiltro.ItemsSource = dt.DefaultView;

			// Y ya podemos cerrar la conexion
			conexion.Close();


		}
		private void cargarListaDeArticulos() {
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
			//dataGrid.DataSource = dt;
			//dataGrid.DataContext = dt.DefaultView;  //esto anda

			//gridFiltro.ItemsSource = dataTable.DefaultView;
			//listFiltro.ItemsSource = dataTable.DefaultView;

			///borrar todos los elementos de mArticulos
			if (mArticulos != null) {
				mArticulos.Clear();
			}

			///Loop por todos los registros de la tabla
			foreach (DataRow registro in dataTable.Rows) {
				//Console.WriteLine(registro.ItemArray.GetValue(0));
				//Console.WriteLine(registro["descripcion"]);
				//Console.WriteLine(registro["id"]+"-"+registro["id"].GetType());

				string tmpStringPrecio = registro["precio"].ToString();
				float tmpPrecio = 0;
				float.TryParse(tmpStringPrecio, out tmpPrecio);

				articuloClass tempArticulo = new articuloClass();
				tempArticulo.id = (long)registro["id"];
				tempArticulo.codigo = registro["codigo"].ToString();
				tempArticulo.codigopro = registro["codigopro"].ToString();
				tempArticulo.descripcion = registro["descripcion"].ToString();
				tempArticulo.precio = tmpPrecio;

				mArticulos.Add(tempArticulo);
				/*
				mArticulos.Add(new articuloClass() {
					id = (long)registro["id"],
					codigopro = registro["codigopro"].ToString(),
					descripcion = registro["descripcion"].ToString(),
					precio = tmpPrecio,
				});
				*/
			}

			//popular lista mArticulos con los datos de todos los registros (se puede??)
			//List<articuloClass> tempArticulos = new List<articuloClass>();
			//tempArticulos.AddRange(dataTable.Rows);


			//cerrar conexion
			gridFiltro.ItemsSource = mArticulos;
			listFiltro.ItemsSource = mArticulos;
			conexion.Close();

		}
		private void filtroArticulos(string filtro = "") {
			var listaTotal = from registro in mArticulos
											 where registro.descripcion.ToLower().Contains(filtro.ToLower())
											 select registro;
			listFiltro.ItemsSource = listaTotal;
			/*
			foreach (var reg in listaTotal) {
				Console.WriteLine("Id = {0} , Descripcion = {1}",
													reg.id, reg.descripcion);
			}
			*/
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
			var textBox = sender as TextBox;
			textBox.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 120));
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
			var textBox = sender as TextBox;
			textBox.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
		}
	}
}

