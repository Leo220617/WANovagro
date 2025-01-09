using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    [Table("DetAprovisionamiento")]
    public class DetAprovisionamiento
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Bodega { get; set; }
        public decimal Stock { get; set; }
        public decimal Pedido { get; set; }
        public string CodProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public decimal UltPrecioCompra { get; set; }
        public decimal CostoPromedio { get; set; }
        public decimal PromedioVenta { get; set; }
        public decimal InventarioIdeal { get; set; }
        public decimal IndicadorST { get; set; }
        public decimal PedidoSugerido { get; set; }
        public decimal Compra { get; set; }
        public bool Chequeado { get; set; }
        public decimal StockTodas { get; set; }
        public decimal PromedioVentaTodas { get; set; }
        public decimal IndicadorSTTodas { get; set; }
    }
}