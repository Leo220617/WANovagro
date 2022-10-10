using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class MetodosPagos
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public decimal Monto { get; set; }
        public string BIN { get; set; }
        public string NumReferencia { get; set; }
        public string NumCheque { get; set; }
        public string Metodo { get; set; }

    }
}