using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class EncMargenes
    {
        [Key]
        [Column(Order = 0)]
        public int idListaPrecio { get; set; }
        [Key]
        [Column(Order = 1)]
        public int idCategoria { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Moneda { get; set; }

        public decimal Cobertura { get; set; }

        public decimal Margen { get; set; }

        public decimal MargenMin { get; set; }

        public int idUsuarioCreador { get; set; }

        public DateTime FechaCreacion { get; set; }


    }
}