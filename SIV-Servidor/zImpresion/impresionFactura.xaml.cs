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

namespace SIV_Servidor.zImpresion
{
    public partial class impresionFactura : UserControl
    {
        ///MAIN
        public impresionFactura()
        {
            InitializeComponent();

            ///recorrer todos textblocks dentro del grid y cargar su posicion desde BD
            foreach (var t in LogicalTreeHelper.GetChildren(gridHoja))
            {
                if (t is TextBlock)
                {
                    TextBlock tb = t as TextBlock;
                    string tbName = tb.Name.Trim();

                    Thickness pos = new Thickness();
                    pos = tb.Margin;

                    /// buscar registro en BD
                    string nombreConfig = "impresionFactura-" + tbName;
                    string valorConfig = zdb.leerConfig(nombreConfig);

                    /// si existe el registro
                    if (valorConfig != "registroInexistente")
                    {
                        var val = valorConfig.Split('-');
                        pos.Left = zfun.toDouble(val[0]);
                        pos.Top = zfun.toDouble(val[1]);
                        tb.Margin = new Thickness(pos.Left, pos.Top, 0, 0);
                    }
                }
            }
        }

        ///IMPRIMIR PLANTILLA
        public void imprimir(plantillaImpresion p)
        {

            ///cargar los datos del parámetro 'p' en los textbox del control 'plantilla'
            if (p.proceso == "factura")
            {
                tbFecha.Text = p.fecha;
                //tbNro.Text = p.nro;
                tbNro.Text = "";
                tbNombre.Text = p.nombre;
                tbTelefono.Text = p.telefono;
                tbDireccion.Text = p.direccion;
                tbCuit.Text = p.cuit;
                tbCantidad.Text = p.cantidad;
                tbDescripcion.Text = p.descripcion;
                tbPrecio.Text = p.precio;
                tbSubtotal.Text = p.subtotal;
                tbTotal.Text = p.total;

                ///casos particulares
                ///si es un 'pendiente'' 
                //if (p.proceso == "pendiente")
                //{
                //    ///los 3 renglones en la zona del TOTAL son UN MISMO TEXTBLOCK
                //    remito.tbTotalTexto.Text = "TOTAL COMPRA:\nPAGADO:\nSALDO:";
                //}

                ///mandar impresion
                PrintDialog pd = new PrintDialog();
                pd.PrintVisual(this, "Impresión de " + p.proceso);


            }

            ///impresion de prueba (imprime los controles así como están)
            if (p.proceso == "impresionDePrueba")
            {
                PrintDialog pd = new PrintDialog();
                pd.PrintVisual(this, "Impresión de " + p.proceso);

            }

        }

        /// CONTROLES
        private void tb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ///poner fondo blanco todos los textblocks
            foreach (var t in LogicalTreeHelper.GetChildren(gridHoja))
            {
                if (t is TextBlock)
                {
                    (t as TextBlock).Background = Brushes.White;
                }
            }

            ///poner fondo amarillo en el q se hizo click
            TextBlock tb = sender as TextBlock;
            tb.Background = Application.Current.FindResource("confoco") as Brush;
            tb.Focus();

            string pos = tb.Margin.Left.ToString() + " - " + tb.Margin.Top.ToString();
            MainWindow.ayuda2(pos, zAyuda.grillaImpresionFacturas);
        }
        private void tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            Double inc = 5;
            if (e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.Right || e.Key == Key.Down)
            {
                Thickness pos = new Thickness();
                pos = tb.Margin;

                if (e.Key == Key.Up || e.Key == Key.Down)
                {
                    Double top = pos.Top;
                    if (e.Key == Key.Up)
                    {
                        top -= inc;
                    }
                    if (e.Key == Key.Down)
                    {
                        top += inc;
                    }
                    tb.Margin = new Thickness(pos.Left, top, 0, 0);
                    if (tb.Name == "tbCantidad" || tb.Name == "tbDescripcion" || tb.Name == "tbPrecio" || tb.Name == "tbSubtotal")
                    {
                        tbCantidad.Margin = new Thickness(tbCantidad.Margin.Left, top, 0, 0);
                        tbDescripcion.Margin = new Thickness(tbDescripcion.Margin.Left, top, 0, 0);
                        tbPrecio.Margin = new Thickness(tbPrecio.Margin.Left, top, 0, 0);
                        tbSubtotal.Margin = new Thickness(tbSubtotal.Margin.Left, top, 0, 0);
                    }
                }

                if (e.Key == Key.Left || e.Key == Key.Right)
                {
                    Double left = pos.Left;

                    if (e.Key == Key.Left)
                    {
                        left -= inc;
                    }
                    if (e.Key == Key.Right)
                    {
                        left += inc;

                    }
                    tb.Margin = new Thickness(left, pos.Top, 0, 0);
                    if (tb.Name == "tbTotal" || tb.Name == "tbSubtotal")
                    {
                        tbTotal.Margin = new Thickness(left, tbTotal.Margin.Top, 0, 0);
                        tbSubtotal.Margin = new Thickness(left, tbSubtotal.Margin.Top, 0, 0);
                    }

                }

            }
            e.Handled = true;
        }


    }
}
