using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class DetMargenes
    {
        public int id { get; set; }

        public string ItemCode { get; set; }

        public int idListaPrecio { get; set; }

        public int idCategoria { get; set; } 

        public string Moneda { get; set; }

        public decimal PrecioSAP { get; set; }

        public decimal Cobertura { get; set; }

        public decimal Margen { get; set; }

        public decimal MargenMin { get; set; }

        public decimal PrecioFinal { get; set; }

        public decimal PrecioMin { get; set; }

        public decimal PrecioCob { get; set; }

        public bool Seteable { get; set; }
    }
}