using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    [Table("LogsProductosAprov")]
    public class LogsProductosAprov
    {
        public int id { get; set; }
        public int idProducto { get; set; }
        public int idCategoria { get; set; }
        public int idSubCategoria { get; set; }
        public int idUsuarioModificador { get; set; }
        public decimal Minimo { get; set; }
        public DateTime Fecha { get; set; }
        public string Clasificacion { get; set; }
        public string ItemCode { get; set; }
    }
}