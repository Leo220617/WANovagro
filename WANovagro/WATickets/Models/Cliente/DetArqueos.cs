using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class DetArqueos
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idProducto { get; set; }
        public decimal Stock { get; set; }
        public decimal Total { get; set; }
        public decimal Diferencia { get; set; }
        public bool Contado { get; set; }
        public decimal Costo { get; set; }
        public decimal CostoDiferencia { get; set; }
        public decimal Cantidad1 { get; set; }
        public decimal Cantidad2 { get; set; }
        public decimal Cantidad3 { get; set; }

    }
}