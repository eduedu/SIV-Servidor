using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIV_Servidor
{
    public class zAyuda
    {
        ///Textos de ayuda
        public static string descripcion1 = "" +
            "<TEXTO> listar artículos o crear uno NUEVO \n" +
            "<NRO. DE CÓDIGO> cargar el artículo con ese número código";

        public static string descripcion2a = "" +
            "<TEXTO> para filtrar artículos \n" +
            "<FLECHA ABAJO> para acceder a la lista \n";
        public static string descripcion2b = "" +
            "<ENTER> crear artículo NUEVO y añadir a la Base de Datos \n" +
            "<ESCAPE> Cancelar";

        public static string descripcion3 = "" +
            "<ENTER> crear artículo NUEVO con esa descripción" +
            "y añadir a la Base de Datos\n";

        public static string descripcion4 = "" +
            "<ENTER> para cargar el artículo con ese número de código \n";

        public static string precio1a = "" +
            "<ENTER> Cargar artículo en la Lista de Ventas \n" +
            "<OTRO PRECIO> Modificar precio del artículo y grabar en Base de Datos\n";
        public static string precio1b = "" +
            "<ESCAPE> Cancelar";

        public static string precio2a = "" +
            "<ENTER> Actualiziar el precio en la BASE DE DATOS \n" +
            "y agregar el artículo a la Lista de Ventas \n";
        public static string precio2b = "" +
            "<ESCAPE> Cancelar";

        public static string listFiltro1a = "" +
            "<ENTER> Seleccionar artículo \n" +
            "<DELETE> Borrar artículo\n";

        public static string listFiltro1b = "" +
            "<ESPACIO> Copiar descripción y crear un nuevo artículo \n" +
            "<ESCAPE> Cancelar";

        public static string listCaja1 = "" +
            "<ENTER> Seleccionar Venta \n"; 

        public static string articuloNoEncontrado = "Artículo no encontrado";
        public static string nuevoArticulo = "<ENTER> para crear un NUEVO artículo";
    }
}
