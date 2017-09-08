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

namespace SIV_Servidor.zOpciones
{
    /// <summary>
    /// Interaction logic for opcImprimirCodigos.xaml
    /// </summary>
    public partial class opcImprimirCodigos : UserControl
    {
        public opcImprimirCodigos()
        {
            InitializeComponent();
        }


        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
            MainWindow.statCortinaNegra.Visibility = Visibility.Hidden;
        }
    }
}
