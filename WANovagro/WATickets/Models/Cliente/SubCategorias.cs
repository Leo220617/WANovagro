using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class SubCategorias
    {
        public int id { get; set; }
        public int idCategoria { get; set; }
        public string Nombre { get; set; }
        public bool ProcesadoSAP { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}