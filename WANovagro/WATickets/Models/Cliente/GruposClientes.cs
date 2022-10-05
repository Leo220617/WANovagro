using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class GruposClientes
    {
        public int id { get; set; }


        public string CodSAP { get; set; }


        public string Nombre { get; set; }

        public bool Estado { get; set; }
    }
}