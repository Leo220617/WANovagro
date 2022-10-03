using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models.APIS
{
    public class ExoneracionesX
    {
        public int id { get; set; }
        public string TipoDoc { get; set; }
        public string NumDoc { get; set; }
        public string NomInst { get; set; }
        public DateTime FechaEmision { get; set; }
        public int PorExon { get; set; }
        public int idCliente { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Imagen { get; set; }
        public List<DetExoneraciones> Detalle { get; set; }
    }
}