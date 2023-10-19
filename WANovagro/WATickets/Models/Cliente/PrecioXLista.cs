using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class PrecioXLista
    {
        [Key]
        [Column(Order = 0)]
        public int idProducto { get; set; }
        [Key]
        [Column(Order = 1)]
        public int idListaPrecio { get; set; }

        [Key]
        [Column(Order = 2)]
        public int idBodega { get; set; }

        public decimal Porcentaje { get; set; }
    }
}