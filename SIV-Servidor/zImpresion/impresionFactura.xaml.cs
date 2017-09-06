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
    /// Interaction logic for impresionFactura.xaml
    /// </summary>
    public partial class impresionFactura : UserControl
    {
        public impresionFactura()
        {
            InitializeComponent();
        }
        ///IMPRIMIR PLANTILLA
        public void imprimir(plantillaImpresion p)
        {

            ///cargar los datos del parámetro 'p' en los textbox del control 'plantilla'
            if (p.proceso == "factura" )
            {
                tbFecha.Text = p.fecha;
                tbNro.Text = p.nro;
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
        }
    }
}
