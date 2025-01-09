using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models.APIS
{
    public class Aprovisionamientos
    {
        public int id { get; set; }
        public int idCategoria { get; set; }
        public int idSubCategoria { get; set; }
        public int idUsuarioCreador { get; set; }
        public DateTime Fecha { get; set; }
        public string Status { get; set; }
        public string Clasificacion { get; set; }
        public decimal IndicadorMenor { get; set; }
        public decimal IndicadorMayor { get; set; }
        public List<DetAprovisionamiento> Detalle { get; set; }
    }
}