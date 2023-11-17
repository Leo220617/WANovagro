using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    public class DetMargenesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll([FromUri] Filtros filtros)
        {
            try
            {
                
                var Detalles = db.DetMargenes.ToList();

                if (filtros.Codigo1 > 0)
                {
                    Detalles = Detalles.Where(a => a.idListaPrecio == filtros.Codigo1).ToList();
                }


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Detalles);
            }
            catch (Exception ex)
            {
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex);
                db.BitacoraErrores.Add(be);
                db.SaveChanges();

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);

            }

        }
    }
}