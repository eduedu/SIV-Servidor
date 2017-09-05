using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIV_Servidor
{
    public class itemArticulo
    {
        ///se usa en la BD Articulos y en listFiltro
		public long id { get; set; }
        public string proveedor { get; set; }
        public string codigopro { get; set; }
        public long codigo { get; set; }
        public string descripcion { get; set; }
        //public float  precio { get; set; }
        public float precio { get; set; }
        public float costo { get; set; }
        public float fechacreacion { get; set; }
        public float fechamodif { get; set; }
        public string tags { get; set; }
        public string stock { get; set; }
    }
    public class itemVenta
    {
        ///se usa en listVentaX y luego pasa a la caja
        public long id { get; set; }
        public long codigo { get; set; }
        public string descripcion { get; set; }
        //public string cantidad { get; set; }
        public long cantidad { get; set; }
        public float precio { get; set; }
        public float subtotal { get; set; }
        public float costo { get; set; }
    }
    public class itemCaja
    ///se usa en la BD caja y en el listCaja
    {
        public long id { get; set; }
        public int idventa { get; set; }
        public string idventamostrar { get; set; }
        public int color { get; set; }
        public string fecha { get; set; }
        public string fechaMostrar { get; set; }
        public long codigo { get; set; }
        public string descripcion { get; set; }

        //public string cantidad { get; set; }
        //public string precio { get; set; }
        //public string costo { get; set; }
        //public string subtotal { get; set; }

        public long cantidad { get; set; }
        public float precio { get; set; }
        public float costo { get; set; }
        public float subtotal { get; set; }


        //public string total { get; set; }
        public string totalmostrar { get; set; }
    }

    public class itemCliente
    ///se usa en la BD clientes y en el listClientes
    {
        public long id { get; set; }
        public string nombre { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string cuit { get; set; }


    }
    //public class itemListNombres
    /////se usa con la BD pendientes, en el listNombres de ucConsultas
    //{
    //    public long id { get; set; }
    //    //public long nro { get; set; }
    //    //public string fecha { get; set; }
    //    public string nombre { get; set; }
    //    //public string direccion { get; set; }
    //    //public string telefono { get; set; }
    //    //public string cuit { get; set; }

    //    //public float total { get; set; }
    //    public float saldo { get; set; }
    //}

    public class itemListConsultas
    ///se usa en la BD remitos, facturas y pendientes, para el listConsultas
    {
        /// carga los datos con codigo=-100, total=campo "subtotal" 
        public long id { get; set; }
        public long nro { get; set; }
        public string fecha { get; set; }
        public string nombre { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string cuit { get; set; }

        public float total { get; set; }

        /// 'saldo' se utiliza solo en la consulta de 'pendientes'
        public float saldo { get; set; }
    }
    public class itemListDetalles
    ///se usa en la BD remitos, facturas y pendientes, para el listDetalle
    {
        /// carga los datos con codigo=-100, total=campo "subtotal" 
        public long id { get; set; }
        public long nro { get; set; }
        public string fecha { get; set; }
        //public string nombre { get; set; }
        //public string direccion { get; set; }
        //public string telefono { get; set; }
        //public string cuit { get; set; }

        //public float total { get; set; }
        public long codigo { get; set; }
        public string descripcion { get; set; }
        public long cantidad { get; set; }
        public float precio { get; set; }
        public float subtotal { get; set; }

        ///utilizo para diferenciar los articulos de los pagos
        public int color { get; set; }
    }

    public class itemGastos
    ///se usa en la BD gastos y en el listGastos
    {
        public long id { get; set; }
        public string fecha { get; set; }
        public string fechaMostrar { get; set; }
        public string descripcion { get; set; }
        public float monto { get; set; }

    }
    public class plantillaImpresion
    ///se usa para pasar los datos de impresion
    {
        public string proceso { get; set; }
        public string fecha { get; set; }
        public string nro { get; set; }
        public string nombre { get; set; }
        public string telefono { get; set; }
        public string direccion { get; set; }
        public string cuit { get; set; }
        public string cantidad { get; set; }
        public string descripcion { get; set; }
        public string precio { get; set; }
        public string subtotal { get; set; }
        public string total { get; set; }

    }

}