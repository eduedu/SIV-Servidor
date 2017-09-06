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
    /// <summary>
    /// Interaction logic for impresionConPlantilla.xaml
    /// </summary>
    public partial class impresionConPlantilla : UserControl
    {
        ///controls static
        //public static plantillaRemitos remito;

        ///MAIN
        public impresionConPlantilla()
        {
            InitializeComponent();
        }

        ///LOADED
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            /// referencia a los controles static
            //remito = plantillaRemitos;
        }

        ///IMPRIMIR PLANTILLA
        public void imprimir(plantillaImpresion p)
        {

            ///cargar los datos del parámetro 'p' en los textbox del control 'plantilla'
            if (p.proceso == "remito" || p.proceso == "pendiente")
            {
                remito.tbFecha.Text = p.fecha;
                remito.tbNro.Text = p.nro;
                remito.tbNombre.Text = p.nombre;
                remito.tbTelefono.Text = p.telefono;
                remito.tbDireccion.Text = p.direccion;
                remito.tbCuit.Text = p.cuit;
                remito.tbCantidad.Text = p.cantidad;
                remito.tbDescripcion.Text = p.descripcion;
                remito.tbPrecio.Text = p.precio;
                remito.tbSubtotal.Text = p.subtotal;
                remito.tbTotal.Text = p.total;

                ///casos particulares
                ///si es un 'pendiente'' 
                if (p.proceso == "pendiente")
                {
                    ///los 3 renglones en la zona del TOTAL son UN MISMO TEXTBLOCK
                    remito.tbTotalTexto.Text = "TOTAL COMPRA:\nPAGADO:\nSALDO:";
                }

                ///mandar impresion
                PrintDialog pd = new PrintDialog();
                pd.PrintVisual(this, "Impresión de " + p.proceso);


            }

            /////crear control hoja y referencia a su plantilla
            //zImpresion.impresionConPlantilla hoja = new zImpresion.impresionConPlantilla();
            //zImpresion.plantillaRemitos p = hoja.plantilla;

            /////cargar datos de la venta
            ////p.tbFecha.Text = tbFecha.Text.ToString();
            //p.tbFecha.Text = tbFecha.Text.ToString().Substring(0, 8);
            ////p.tbNro.Text = tbIdVenta.Text.ToString();
            //p.tbNro.Text = nroProceso.ToString();
            //p.tbNombre.Text = tbNombre.Text.ToString();
            //p.tbTelefono.Text = tbTelefono.Text.ToString();
            //p.tbDireccion.Text = tbDireccion.Text.ToString();
            //p.tbCuit.Text = tbCuit.Text.ToString();
            //p.tbCantidad.Text = "";
            //p.tbDescripcion.Text = "";
            //p.tbPrecio.Text = "";
            //p.tbSubtotal.Text = "";
            //p.tbTotal.Text = tbTotal.Text.ToString();

            /////cargar detalles recorriendo todos los elementos del listDetalles
            ////foreach (var itemList in listDetalles.Items)
            //for (int i = listImpresion.Items.Count - 1; i > -1; i--)
            //{
            //    //itemListDetalles item = itemList as itemListDetalles;
            //    itemCaja item = listImpresion.Items[i] as itemCaja;

            //    p.tbCantidad.Text += item.cantidad.ToString() + "\n";
            //    p.tbDescripcion.Text += item.descripcion.ToString().Trim() + "\n";
            //    p.tbPrecio.Text += "$ " + item.precio.ToString("0.00") + "\n";
            //    p.tbSubtotal.Text += "$ " + item.subtotal.ToString("0.00") + "\n";
            //}

            /////mandar impresion
            //PrintDialog pd = new PrintDialog();
            //pd.PrintVisual(hoja, "test Imprimir");
            //pd.PrintVisual(hoja, "Impresión de " + p.proceso);

            ///eliminar este control de memoria
            //((Panel)this.Parent).Children.Remove(this);
            
        }

    }
}
