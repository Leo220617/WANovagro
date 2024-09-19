using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models.APIS
{
    public class Arqueos
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

        public decimal TotalCosto { get; set; }

        public decimal TotalCostoDiferencia { get; set; }

        public List<DetArqueos> Detalle { get; set; }
    }
}