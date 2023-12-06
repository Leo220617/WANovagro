using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class AprobacionesCreditos
    {
        public int id { get; set; }

        public int idCliente { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string Status { get; set; }

        public bool Activo { get; set; }

        public decimal Total { get; set; }
    }
}