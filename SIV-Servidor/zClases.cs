using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIV_Servidor {
	class articuloClass{
		public long id { get; set; }
		public string proveedor { get; set; }
		public string codigopro { get; set; }
		public string codigo { get; set; }
		public string descripcion { get; set; }
		public float  precio { get; set; }
		public float costo { get; set; }
		public float fechacreacion { get; set; }
		public float fechamodif { get; set; }
		public string tags { get; set; }
	}
    class itemVenta
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public string cantidad { get; set; }
        public string precio { get; set; }
        public string subtotal { get; set; }
        public string costo { get; set; }
    }
    class itemCaja
    {
        public int idventa { get; set; }
        public float fecha { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public string cantidad { get; set; }
        public string precio { get; set; }
        public string costo { get; set; }
    }
}
