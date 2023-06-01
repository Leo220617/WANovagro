using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class Depositos
    {
        public int id { get; set; }
        public int idUsuarioCreador { get; set; }
        public int idCaja { get; set; }
        public string CodSuc { get; set; }
        public int Series { get; set; }
        public DateTime Fecha { get; set; }
        public string Banco { get; set; }
        public string Referencia { get; set; }
        public string CuentaInicial { get; set; }
        public string CuentaFinal { get; set; }
        public decimal Saldo { get; set; }
        public string Comentarios { get; set; }
        public bool ProcesadoSAP { get; set; }
        public string DocEntry { get; set; }
        public string Moneda { get; set; }
     
    }
}