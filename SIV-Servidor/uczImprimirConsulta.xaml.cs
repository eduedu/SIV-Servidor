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

namespace SIV_Servidor
{
    /// <summary>
    /// Interaction logic for uczImprimirConsulta.xaml
    /// </summary>
    public partial class uczImprimirConsulta : UserControl
    {
        bool moviendo = false;
        Point ubicacion;
        double left;
        double top;
        /// MAIN
        public uczImprimirConsulta()
        {
            InitializeComponent();

            ///ubicacion controles
            left = zfun.toDouble(zdb.leerConfig("borrarX"));
            top = zfun.toDouble(zdb.leerConfig("borrarY"));

            label1.Margin = new Thickness(left, top, 0, 0);
        }

        private void label1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            moviendo = true;
            ubicacion = Mouse.GetPosition(label1);
            zfun.consola("down");
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (moviendo)
            {
                var control = sender as Label;

                //double top = Canvas.GetTop(control) + Mouse.GetPosition(control).Y - ubicacion.Y;
                //Canvas.SetTop(control, top);

                //double left = Canvas.GetLeft(control) + Mouse.GetPosition(control).X - ubicacion.X;

                //Canvas.SetLeft(control, left);


                ubicacion = Mouse.GetPosition(control);

                left = label1.Margin.Left + ubicacion.X - (label1.Width / 2);
                top = label1.Margin.Top + ubicacion.Y - (label1.Height / 2);

                label1.Margin = new Thickness(left, top , 0, 0);
                zfun.consola("x:" + ubicacion.X.ToString() + ",y:" + ubicacion.Y.ToString() + ".");
            }
        }

        private void label1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            moviendo = false;
            Mouse.Capture(null);

            ///grabar en bd
            zdb.grabarConfig("borrarX", left.ToString());
            zdb.grabarConfig("borrarY", top.ToString());



            zfun.consola("up");
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            moviendo = false;
            Mouse.Capture(null);
            zfun.consola("up");
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
        }
    }
}
