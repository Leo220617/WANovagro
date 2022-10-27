using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class CondicionesPagos
    {
        public int id { get; set; }

       
        public string CodSAP { get; set; }

       
        public string Nombre { get; set; }

        public int Dias { get; set; }

    }
}