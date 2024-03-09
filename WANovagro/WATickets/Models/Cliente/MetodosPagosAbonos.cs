using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class MetodosPagosAbonos
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idCuentaBancaria { get; set; }
        public decimal Monto { get; set; }
        public string BIN { get; set; }
        public string NumReferencia { get; set; }
        public string NumCheque { get; set; }
        public string Metodo { get; set; }
        public string Moneda { get; set; }
        public int idCaja { get; set; }
        public int idCajero { get; set; }
        public DateTime Fecha { get; set; }
        public string MonedaVuelto { get; set; }
        public decimal PagadoCon { get; set; }
    }
}