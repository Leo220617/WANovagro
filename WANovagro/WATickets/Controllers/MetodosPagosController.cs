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
    public class MetodosPagosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll([FromUri] Filtros filtros)
        {
            try
            {
                var time = new DateTime();
                var Pagos = db.MetodosPagos.ToList();
              
                if(filtros.Codigo1 > 0 && filtros.FechaInicial != time && filtros.Codigo2 > 0) // para buscar los pagos realizados en una caja
                {

                    //var Documentos = db.EncDocumento.Where(a => a.idCaja != filtros.Codigo1 ).ToList(); //Todos los documentos hechos en otras cajas
                    //foreach(var item in Documentos)
                    //{
                    //    Pagos = Pagos.Where(a => a.idEncabezado != item.id).ToList(); //tenemos todos los pagos pertenecientes a una caja
                    //}
                    //filtros.FechaFinal = filtros.FechaInicial.AddDays(1);
                    //Documentos = db.EncDocumento.Where(a => a.idCaja == filtros.Codigo1 && (a.Fecha < filtros.FechaInicial || a.Fecha >= filtros.FechaFinal)).ToList();
                    //foreach (var item in Documentos)
                    //{
                    //    Pagos = Pagos.Where(a => a.idEncabezado != item.id).ToList();
                    //}

                    Pagos = Pagos.Where(a => a.idCaja == filtros.Codigo1 && a.idCajero == filtros.Codigo2 && a.Fecha == filtros.FechaInicial).ToList();
                    //Pagos = Pagos.Where(a => a.idEncabezado != 0 ).ToList();
                }


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
        [Route("api/MetodosPagos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                MetodosPagos metodosPagos = db.MetodosPagos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, metodosPagos);
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