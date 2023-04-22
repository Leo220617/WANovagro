using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    [Table("DetPagos")]
    public partial class DetPagos
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idEncDocumentoCredito { get; set; }
        public int NumLinea { get; set; }
        public decimal Total { get; set; }
    
    }
}