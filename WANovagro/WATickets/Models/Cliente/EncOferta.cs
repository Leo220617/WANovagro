 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("EncOferta")]
    public partial class EncOferta
    {
        public int id { get; set; }
        public int idCliente { get; set; }
        public int idUsuarioCreador { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Comentarios { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalImpuestos { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal TotalCompra { get; set; }
        public decimal PorDescto { get; set; }
        public string Status { get; set; }
        public string CodSuc { get; set; }
        public string Moneda { get; set; }
        public int BaseEntry { get; set; }
        public string Tipo { get; set; }
        public string DocEntry { get; set; }
        public bool ProcesadaSAP { get; set; }
        public int idCondPago { get; set; }
        public string TipoDocumento { get; set; }
        public int idVendedor { get; set; }
    }
}