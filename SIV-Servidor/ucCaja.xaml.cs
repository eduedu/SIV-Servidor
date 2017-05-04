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


namespace SIV_Servidor
{
    /// <summary>
    /// Interaction logic for ucCaja.xaml
    /// </summary>
    public partial class ucCaja : UserControl
    {
        public ucCaja()
        {
            InitializeComponent();
        }
        private void listCaja_GotFocus(object sender, RoutedEventArgs e)
        {
            ///ayuda(zAyuda.listCaja1);
            MainWindow.ayuda2(zAyuda.listCaja1);
            MainWindow.consola(zAyuda.listCaja1);
            
        }
        //public void ayuda2(string texto = "", string texto2 = "")
        //{
        //     if (MainWindow.statAyuda1.Content != null)
        //    {
        //        if (MainWindow.statAyuda1.Content.ToString() != texto)
        //        {
        //            MainWindow.statAyuda1.Content = texto;
        //            MainWindow.statAyuda2.Content = texto2;

        //            MainWindow.statSbAyuda.Begin();
        //        }

        //    }
        //    else
        //    {
        //        MainWindow.consola("error, content nulo en statAyuda1");
        //    }
        //}
    }
}
