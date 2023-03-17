using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class CuentasBancarias
    {
        public int id { get; set; }

        public string CodSuc { get; set; }

        public string Nombre { get; set; }

        public string CuentaSAP { get; set; }

        public bool Estado { get; set; }

        public string Banco { get; set; }

        public string Moneda { get; set; }

        public string Tipo { get; set; }
    }
}