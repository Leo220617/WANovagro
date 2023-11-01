using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models.APIS
{
    public class Promocion
    {
        public int id { get; set; }
        public int idListaPrecio { get; set; }
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public List<Promociones> Detalle { get; set; }
    }
}