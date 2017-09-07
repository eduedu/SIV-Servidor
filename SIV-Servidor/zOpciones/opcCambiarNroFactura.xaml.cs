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
    /// Interaction logic for opcCambiarNroFactura.xaml
    /// </summary>
    public partial class opcCambiarNroFactura : UserControl
    {
        public opcCambiarNroFactura()
        {
            InitializeComponent();
            string ultimoNumero = zdb.leerConfig("nroFactura");
            tbUltimoNumero.Text = ultimoNumero;
            //tbCambiarPor.Focus();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            ///eliminar control
            ((Panel)this.Parent).Children.Remove(this);
            MainWindow.statCortinaNegra.Visibility = Visibility.Hidden;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            ///grabar nuevo Nro de factura en la BD
            int nuevoNumero = zfun.toInt(tbCambiarPor.Text);
            if (nuevoNumero > 0)
            {
                ///grabar si el nuevonumero es mayor a cero, o sea, si no hubo algun tipo de error
                zdb.grabarConfig("nroFactura", nuevoNumero.ToString());
            }

            ///eliminar control
            ((Panel)this.Parent).Children.Remove(this);
            MainWindow.statCortinaNegra.Visibility = Visibility.Hidden;

        }

        private void tbCambiarPor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (zfun.esDecimal(e.Key) == false)
            {
                e.Handled = true;
            }
        }
    }
}
