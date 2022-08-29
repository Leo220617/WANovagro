using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class ImpuestosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll()
        {
            try
            {
                var Impuestos = db.Impuestos.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Impuestos);
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
        [Route("api/Impuestos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Impuestos impuestos = db.Impuestos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, impuestos);
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
        [Route("api/Impuestos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Impuestos impuestos)
        {
            try
            {
                Impuestos Impuesto = db.Impuestos.Where(a => a.id == impuestos.id).FirstOrDefault();
                if (Impuesto == null)
                {
                    Impuesto = new Impuestos();
                    Impuesto.id = impuestos.id;
                    Impuesto.Codigo = impuestos.Codigo;
                    Impuesto.Tarifa = impuestos.Tarifa;
                    db.Impuestos.Add(Impuesto);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un impuesto con este ID");
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
        [Route("api/Impuestos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Impuestos impuestos)
        {
            try
            {
                Impuestos Impuestos = db.Impuestos.Where(a => a.id == impuestos.id).FirstOrDefault();
                if (Impuestos != null)
                {
                    db.Entry(Impuestos).State = System.Data.Entity.EntityState.Modified;
                    Impuestos.Codigo = impuestos.Codigo;
                    Impuestos.Tarifa = impuestos.Tarifa;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un impuesto" +
                        " con este ID");
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
        [Route("api/Impuestos/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Impuestos Impuestos = db.Impuestos.Where(a => a.id == id).FirstOrDefault();
                if (Impuestos != null)
                {
                    db.Impuestos.Remove(Impuestos);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un impuesto con este ID");
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