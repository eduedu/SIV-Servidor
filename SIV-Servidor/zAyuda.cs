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
            "<TEXTO> para LISTAR artículos o crear uno NUEVO \n" +
            "<NRO. DE CÓDIGO> + <ENTER> para cargar el artículo con ese número código \n" +
            "";
        public static string descripcion2 = "" +
            "<TEXTO> para filtrar artículos \n" +
            "<FLECHA ABAJO> para acceder a la lista \n" +
            "<ENTER> para crear un NUEVO artículo con esa descripción y agregarlo a la Base de Datos \n" +
            "<ESCAPE> Cancelar";
        public static string descripcion3 = "" +
            "<ENTER> para crear un NUEVO artículo con esa descripción y agregarlo a la Base de Datos\n" +
            "" +
            "";
        public static string descripcion4 = "" +
            "<ENTER> para cargar el artículo con ese número de código \n" +
            "" +
            "";

        public static string precio1 = "" +
            "<ENTER> Cargar artículo en la Lista de Ventas \n" +
            "<OTRO PRECIO> Modificar el precio del artículo y grabarlo en la Base de Datos\n" +
            "<ESCAPE> Cancelar";
        public static string precio2 = "" +
            "<ENTER> Actualziar el precio en la BASE DE DATOS y agregar el artículo a la Lista de Ventas \n" +
            "" +
            "<ESCAPE> Cancelar";


        public static string listFiltro1 = "" +
            "<ENTER> Seleccionar artículo \n" +
            "<DELETE> Borrar artículo\n" +
            "<ESPACIO> Copiar descripción y crear un nuevo artículo \n" +
            "<ESCAPE> Cancelar";

        public static string listCaja1 = "" +
            "<ENTER> Seleccionar Venta \n" +
            "" +
            "" +
            "";

        public static string articuloNoEncontrado = "Artículo no encontrado";
        public static string nuevoArticulo = "<ENTER> para crear un NUEVO artículo";
    }
}
