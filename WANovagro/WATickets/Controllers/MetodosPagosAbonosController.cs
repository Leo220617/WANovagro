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
    public class MetodosPagosAbonosController : ApiController
    {

        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll([FromUri] Filtros filtros)
        {
            try
            {
                var time = new DateTime();
                var Pagos = db.MetodosPagosAbonos.Where(a => (filtros.Codigo1 > 0 && filtros.FechaInicial != time && filtros.Codigo2 > 0 ? a.idCaja == filtros.Codigo1 && a.idCajero == filtros.Codigo2 && a.Fecha == filtros.FechaInicial : true)
                ).ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Pagos);
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
        [Route("api/MetodosPagosAbonos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                MetodosPagosAbonos MetodosPagosAbonos = db.MetodosPagosAbonos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, MetodosPagosAbonos);
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