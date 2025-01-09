using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    [Table("EncAprovisionamiento")]
    public class EncAprovisionamiento
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
    }
}