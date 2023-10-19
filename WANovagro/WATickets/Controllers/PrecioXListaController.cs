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
    [Authorize]
    public class PrecioXListaController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();
        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var PrecioXLista = db.PrecioXLista.ToList();
             
                if (filtro.Codigo1 > 0) 
                {
                    PrecioXLista = PrecioXLista.Where(a => a.idListaPrecio == filtro.Codigo2).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, PrecioXLista);
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

        [Route("api/PrecioXLista/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] PrecioXLista[] precios)
        {
            try
            {
                foreach(var prec in precios)
                {
                    var PrecioXLista = db.PrecioXLista.Where(a => a.idProducto == prec.idProducto && a.idListaPrecio == prec.idListaPrecio).FirstOrDefault();

                    if(PrecioXLista == null)
                    {
                        PrecioXLista Prec = new PrecioXLista();
                        Prec.idProducto = prec.idProducto;
                        Prec.idListaPrecio = prec.idListaPrecio;
                        Prec.idBodega = prec.idBodega;
                        Prec.Porcentaje = prec.Porcentaje;

                        db.PrecioXLista.Add(Prec);
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Entry(PrecioXLista).State = System.Data.Entity.EntityState.Modified;
                        PrecioXLista.Porcentaje = prec.Porcentaje;
                        db.SaveChanges();
                    }
                }

                   

                return Request.CreateResponse(System.Net.HttpStatusCode.OK);
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