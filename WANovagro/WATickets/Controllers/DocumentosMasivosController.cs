using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class DocumentosMasivosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();
    }
}