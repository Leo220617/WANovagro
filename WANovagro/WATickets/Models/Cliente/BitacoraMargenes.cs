using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class BitacoraMargenes
    {
        public int id { get; set; }

        public string ItemCode { get; set; }

        public int idCategoria { get; set; }

        public int idListaPrecio { get; set; }

        public decimal PrecioAnterior { get; set; }

        public decimal PrecioNuevo { get; set; }

        public DateTime Fecha { get; set; }
    }
}