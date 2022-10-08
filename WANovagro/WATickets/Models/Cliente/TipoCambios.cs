using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class TipoCambios
    {
        public int id { get; set; }
        public decimal TipoCambio { get; set; }
        public DateTime Fecha { get; set; }
        public string Moneda { get; set; }

    }
}