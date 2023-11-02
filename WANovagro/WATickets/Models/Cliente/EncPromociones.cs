using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class EncPromociones
    {
        public int id { get; set; }
        public int idListaPrecio { get; set; }
        public int idUsuarioCreador { get; set; }
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaCreacion { get; set; }

    }
}