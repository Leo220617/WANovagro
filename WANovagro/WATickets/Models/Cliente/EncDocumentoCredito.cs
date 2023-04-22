using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    [Table("EncDocumentoCredito")]
    public partial class EncDocumentoCredito
    {
        public int id { get; set; }
        public int idCliente { get; set; }
        public int idVendedor { get; set; }
        public int idCondPago { get; set; }
        public string CodSuc { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Comentarios { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalImpuestos { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal TotalCompra { get; set; }
        public decimal Saldo { get; set; }
        public decimal PorDescto { get; set; }
        public string Status { get; set; }
        public string Moneda { get; set; }
        public string TipoDocumento { get; set; }
        public string DocEntry { get; set; }
        public string DocNum { get; set; }
        public string ClaveHacienda { get; set; }
        public string ConsecutivoHacienda { get; set; }
    }
}