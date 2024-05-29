using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class Asientos
    {
        public int id { get; set; }
        public int idUsuario { get; set; }
        public int idCaja { get; set; }
        public DateTime Fecha { get; set; }
        public string CodSuc { get; set; }
        public int idCuentaCredito { get; set; }
        public int idCuentaDebito { get; set; }
        public string Referencia { get; set; }
        public bool ProcesadoSAP { get; set; }
        public string DocEntry { get; set; }
        public decimal Credito { get; set; }
        public decimal Debito { get; set; }

    }
}