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
	public enum DemoType {
		Udp,
		Tcp,
		Multicast
	}

	public partial class MainWindow : Window {
		static SQLiteConnection conexion;
		public MainWindow() {
			InitializeComponent();
			gridFiltro.Visibility = Visibility.Hidden;
			gridFiltro.Margin = new Thickness(textBox.Margin.Left, gridFiltro.Margin.Top, 0, 0);

			parteOSC();
			conexionSQLite();
		}
		/*----------------------------------------------------------------------------------*/
		/*----------------------------------------------------------------------------------*/
		private void conexionSQLite(string filtro = "") {
			conexion = new SQLiteConnection
				("Data Source=lista.db;Version=3;New=False;Compress=True;");
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

			// Y ya podemos cerrar la conexion
			conexion.Close();


		}

		private void parteOSC() {
			Console.WriteLine("hola");

			OscServer oscServer;

			//DemoType demoType = GetDemoType();
			DemoType demoType = DemoType.Udp;
			IPAddress miIp;
			//miIp=IPAddress.Loopback;
			miIp = IPAddress.Parse("192.168.0.9");

			oscServer = new OscServer(TransportType.Udp, miIp, Port);
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
			oscServer.RegisterMethod(AliveMethod);
			oscServer.RegisterMethod(TestMethod);
			oscServer.BundleReceived += new EventHandler<OscBundleReceivedEventArgs>(oscServer_BundleReceived);
			oscServer.MessageReceived += new EventHandler<OscMessageReceivedEventArgs>(oscServer_MessageReceived);
			oscServer.ReceiveErrored += new EventHandler<Bespoke.Common.ExceptionEventArgs>(oscServer_ReceiveErrored);
			oscServer.ConsumeParsingExceptions = false;

			oscServer.Start();


			Console.WriteLine("Osc Receiver: " + demoType.ToString());
			Console.WriteLine("Press any key to exit.");
			//Console.ReadKey();
			//oscServer.Stop();
		}
		private static DemoType GetDemoType() {
			Dictionary<ConsoleKey, DemoType> keyMappings = new Dictionary<ConsoleKey, DemoType>();
			keyMappings.Add(ConsoleKey.D1, DemoType.Udp);
			keyMappings.Add(ConsoleKey.D2, DemoType.Tcp);
			keyMappings.Add(ConsoleKey.D3, DemoType.Multicast);

			Console.WriteLine("\nWelcome to the Bespoke Osc Receiver Demo.\nPlease select the type of receiver you would like to use:");
			Console.WriteLine("  1. Udp\n  2. Tcp\n  3. Udp Multicast");

			ConsoleKeyInfo key = Console.ReadKey();
			while (keyMappings.ContainsKey(key.Key) == false) {
				Console.WriteLine("\nInvalid selection\n");
				Console.WriteLine("  1. Udp\n  2. Tcp\n  3. Udp Multicast");
				key = Console.ReadKey();
			}

			Console.Clear();

			return keyMappings[key.Key];
		}
		private static void oscServer_BundleReceived(object sender, OscBundleReceivedEventArgs e) {
			sBundlesReceivedCount++;

			OscBundle bundle = e.Bundle;
			Console.WriteLine(string.Format("\nBundle Received [{0}:{1}]: Nested Bundles: {2} Nested Messages: {3}", bundle.SourceEndPoint.Address, bundle.TimeStamp, bundle.Bundles.Count, bundle.Messages.Count));
			Console.WriteLine("Total Bundles Received: {0}", sBundlesReceivedCount);
		}
		private void oscServer_MessageReceived(object sender, OscMessageReceivedEventArgs e) {
			sMessagesReceivedCount++;

			OscMessage message = e.Message;

			Console.WriteLine(string.Format("\nMessage Received [{0}]: {1}", message.SourceEndPoint.Address, message.Address));
			Console.WriteLine(string.Format("Message contains {0} objects.", message.Data.Count));

			//label1.
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


				Application.Current.Dispatcher.BeginInvoke((Action)(() => {
					Label1.Content = Label1.Content + dataString;
				}), DispatcherPriority.Normal, null);

			}

			Console.WriteLine("Total Messages Received: {0}", sMessagesReceivedCount);
		}

		private static void oscServer_ReceiveErrored(object sender, Bespoke.Common.ExceptionEventArgs e) {
			Console.WriteLine("Error during reception of packet: {0}", e.Exception.Message);
		}

		private static readonly int Port = 9999;
		//private static readonly string AliveMethod = "/edu/alive";
		//private static readonly string TestMethod = "/edu/test";
		private static readonly string AliveMethod = "/edu";
		private static readonly string TestMethod = "/edu";
		private static int sBundlesReceivedCount;
		private static int sMessagesReceivedCount;

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

		private void textBox_TextChanged(object sender, TextChangedEventArgs e) {
			var textBox = sender as TextBox;
			string filtro = textBox.Text.ToLower().Trim();
			if (filtro != "") {
				conexionSQLite(filtro);
				gridFiltro.Visibility = Visibility.Visible;
			}
			else {
				gridFiltro.Visibility = Visibility.Hidden;
			}

		}
	}
}

