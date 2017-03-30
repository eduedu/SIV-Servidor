using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bespoke.Common;
using Bespoke.Common.Osc;
using System.Windows.Threading;
using System.Net;

namespace SIV_Servidor {
	public enum DemoType {
		Udp,
		Tcp,
		Multicast
	}
	public class MiOSC {
		//OSC
		public static DemoType GetDemoType() {
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
		public static void oscServer_BundleReceived(object sender, OscBundleReceivedEventArgs e) {
			sBundlesReceivedCount++;

			OscBundle bundle = e.Bundle;
			//Console.WriteLine(string.Format("\nBundle Received [{0}:{1}]: Nested Bundles: {2} Nested Messages: {3}", bundle.SourceEndPoint.Address, bundle.TimeStamp, bundle.Bundles.Count, bundle.Messages.Count));
			//Console.WriteLine("Total Bundles Received: {0}", sBundlesReceivedCount);
		}
		
		public  void oscServer_MessageReceived(object sender, OscMessageReceivedEventArgs e) {
			//ESTA FUNCION NO OCUPO, TRABAJO DESDE MAINWINDOW PARA ACCEDER A LOS CONTROLES
			sMessagesReceivedCount++;

			OscMessage message = e.Message;

			Console.WriteLine(string.Format("\nMessage Received [{0}]: {1}", message.SourceEndPoint.Address, message.Address));
			Console.WriteLine(string.Format("Message contains {0} objects.", message.Data.Count));

			//ACCION LAYOUT
			/*
			Application.Current.Dispatcher.BeginInvoke((Action)(() => {
				Label1.Content = "";
				//txtUrlCompartirTitulo.Text = mCambioUrlCompartirTitulo;
			}), DispatcherPriority.Normal, null);
			*/

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
				/*
				Application.Current.Dispatcher.BeginInvoke((Action)(() => {
					Label1.Content = Label1.Content + dataString;
				}), DispatcherPriority.Normal, null);
				*/
			}

			Console.WriteLine("Total Messages Received: {0}", sMessagesReceivedCount);
		}

		public static void oscServer_ReceiveErrored(object sender, Bespoke.Common.ExceptionEventArgs e) {
			Console.WriteLine("Error during reception of packet: {0}", e.Exception.Message);
		}

		public static readonly int Port = 9999;
		//private static readonly string AliveMethod = "/edu/alive";
		//private static readonly string TestMethod = "/edu/test";
		public static readonly string AliveMethod = "/edu";
		public static readonly string TestMethod = "/edu";
		public static int sBundlesReceivedCount;
		public static int sMessagesReceivedCount;
	}
}
