using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class Vendedores
    {
        public int id { get; set; }

        [StringLength(50)]
        public string CodSAP { get; set; }

        [StringLength(500)]
        public string Nombre { get; set; }

        [StringLength(3)]
        public string CodSuc { get; set; }
    }
}