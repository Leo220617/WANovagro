using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models.APIS
{
    public class LogsProductosAprovisionamiento
    {
        public string PalabraClave { get; set; }
        public int idCategoria { get; set; }

        public int idUsuarioModificador { get; set; }

        public List<LogsProductosAprov> Detalle { get; set; }
    }
}