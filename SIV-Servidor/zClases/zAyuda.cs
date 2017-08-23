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

        /// AYUDA CONTROL INICIO

        public static string descripcion1 = "" +
            "<TEXTO> listar artículos o crear uno NUEVO \n" +
            "<NRO. DE CÓDIGO> cargar el artículo con ese número código";

        public static string descripcion2a = "" +
            "<TEXTO> para filtrar artículos \n" +
            "<FLECHA ABAJO> para acceder a la lista \n";
        public static string descripcion2b = "" +
            "<ENTER> crear artículo NUEVO y añadir a la Base de Datos \n" +
            "<ESC> Cancelar";

        public static string descripcion3 = "" +
            "<ENTER> crear artículo NUEVO con esa descripción" +
            "y añadir a la Base de Datos\n";

        public static string descripcion4 = "" +
            "<ENTER> para cargar el artículo con ese número de código \n";

        public static string precio1a = "" +
            "<ENTER> Cargar artículo en la Lista de Ventas \n" +
            "<OTRO PRECIO> Modificar precio del artículo y grabar en Base de Datos\n";
        public static string precio1b = "" +
            "<ESC> Cancelar";

        public static string precio2a = "" +
            "<ENTER> Actualiziar el precio en la BASE DE DATOS \n" +
            "y agregar el artículo a la Lista de Ventas \n";
        public static string precio2b = "" +
            "<ESC> Cancelar";

        public static string precio3AgregarArticulo = "<ENTER> Agregar artículo a Base de Datos\n<ESC> Cancelar";
        public static string listFiltro1a = "" +
            "<ENTER> Seleccionar artículo \n" +
            "<DELETE> Borrar artículo\n";

        public static string listFiltro1b = "" +
            "<ESPACIO> Copiar descripción y crear un nuevo artículo \n" +
            "<ESC> Cancelar";


        public static string listVenta1 = "" +
            "<DEL> Eliminar Item \n";

        public static string articuloNoEncontrado = "Artículo no encontrado";
        public static string nuevoArticulo = "<ENTER> para crear un NUEVO artículo";

        public static string tipCambiarPrecio = "<ENTER> Actualizar precio en Base de Datos\n<ESC> Cancelar";
        public static string tipArticuloNuevo = "Artículo Nuevo";
        public static string tipAgregarArticulo = "<ENTER> Agregar artículo a Base de Datos";

        public static string inicio_tbCaja = "" +
            "<F5>  Ocultar Caja \n" +
            "<ESC> Ir a 'Artículo'";
        public static string inicio_tbCajab = "" +
            "<Flecha Derecha> / <Flecha Izquierda> Modificar valor  \n" +
            "<Flecha Arriba> / <Flecha Abajo> Moverse entre los billetes ";

        /// AYUDA CONTROL IMPRESIONES
        public static string listCaja1 = "" +
            "<ENTER> Para imprimir una Factura, Remito o cargar como Pendiente \n";

        public static string bontonMenu1a = "" +
            "<FLECHA ARRIBA/ABAJO> Moverse entre las opciones \n" +
            "<ENTER> Seleccionar \n";

        public static string bontonMenu1b = "" +
            "<ESC> Volver al Listado de Movimientos \n";

        public static string tbEnGridClientes = "" +
            "<ENTER> Continuar \n" +
            "<ESC> Resetear datos del cliente \n";

        public static string tbNombreVacioEnGridClientes = "" +
            "Escriba un nombre para filtrar de la lista \n" +
            "<ESC> Para cancelar \n";

        public static string tbNombreEnGridClientes1 = "" +
            "<ENTER> Para crear un Cliente Nuevo \n" +
            "<FLECHA ABAJO> para acceder a la lista \n";

        public static string tbNombreEnGridClientes2 = "" +
            "<ESC> Resetear datos del cliente \n" +
            "";

        public static string tbNombreEnGridClientesCrearNuevo1 = "" +
            "<ENTER> Para crear un Cliente Nuevo \n" +
            "";
        public static string tbNombreEnGridClientesCrearNuevo2 = "" +
            "<ESC> Resetear datos del cliente  \n" +
            "";

        public static string ListFiltroClientes = "" +
            "<ENTER> Seleccionar Cliente \n" +
            "<ESC> Cancelar \n";

        public static string tipAgregarNuevoCliente = "<ENTER> Agregar cliente";
        /// 

        /// AYUDA CONTROL CONSULTAS
        public static string consultas_tbFiltrar = "" +
            "<F3> Alternar entre las pestañas Pendientes/Remitos/Facturas \n" +
            "<ENTER> Ir a la Lista de Clientes \n";
            
        public static string consultas_tbFiltrar2 = "" +
            "<ESC> Borrar Filtro";

        public static string consultas_listConsultas = "" +
            "< F3 > Alternar entre las pestañas Pendientes/Remitos/Facturas";

        public static string consultas_listNombres = "" +
            "<ENTER> Ir a la lista de 'Cuentas Pendientes'\n" +
            "<ESC> Volver al cuadro 'Filtrar'";
        public static string consultas_listPendientes = "" +
            "<ENTER> Ir al boton 'Asentar Pago' \n" +
            "<ESC> Volver";
        public static string consultas_btnPagar = "" +
            "<ENTER> Asentar un pago para la cuenta seleccionada \n" +
            "<ESC> Volver";

        /// AYUDA CONTROL MONTOS
        public static string gastos_tbDescripcion = "" +
            "<ENTER> Ir al control 'Filtrar'" +
            "";
        public static string gastos_tbMonto = "" +
            "<ENTER> Asentar gasto \n" +
            "<ESC> Cancelar";

        public static string gastos_tbFiltrar = "" +
            "Escriba un texto para filtrar lista 'Gastos'\n" +
            "<ENTER> Ir a la lista 'Gastos'";
        public static string gastos_tbFiltrar2 = "" +
            "<ESC> Cancelar";

        public static string gastos_listGastos = "" +
            "<DELETE> Eliminar item seleccionado \n" +
            "<ESC> Volver a 'Descripción'";


    }
}
