using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class EncArqueos
    {
        public int id { get; set; }

        public int idCategoria { get; set; }


        public string PalabraClave { get; set; }

        public string CodSuc { get; set; }

        public int idUsuarioCreador { get; set; }

        public DateTime FechaCreacion { get; set; }

        public bool Validado { get; set; }

        public string Status { get; set; }

        public DateTime FechaActualizacion { get; set; }
    }
}