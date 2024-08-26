using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class BitacoraMargenes
    {
        public int id { get; set; }

        public int idProducto { get; set; }

        public decimal PrecioAnterior { get; set; }

        public decimal PrecioNuevo { get; set; }

        public DateTime Fecha { get; set; }
    }
}