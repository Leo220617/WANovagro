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
    public class CajasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var Cajas = db.Cajas.ToList();

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    // and = &&, or = ||
                    Cajas = Cajas.Where(a => a.CodSuc.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();// filtramos por lo que trae texto
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Cajas);
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
        [Route("api/Cajas/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Cajas cajas = db.Cajas.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, cajas);
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
        [Route("api/Cajas/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Cajas cajas)
        {
            try
            {
                Cajas Caja = db.Cajas.Where(a => a.id == cajas.id).FirstOrDefault();
                if (Caja == null)
                {
                    Caja = new Cajas();
                    Caja.id = cajas.id;
                    Caja.CodSuc = cajas.CodSuc;
                    Caja.Nombre = cajas.Nombre;
                    db.Cajas.Add(Caja);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una caja con este ID");
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
        [Route("api/Cajas/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Cajas cajas)
        {
            try
            {
                Cajas Cajas = db.Cajas.Where(a => a.id == cajas.id).FirstOrDefault();
                if (Cajas != null)
                {
                    db.Entry(Cajas).State = System.Data.Entity.EntityState.Modified;
                    Cajas.CodSuc = cajas.CodSuc;
                    Cajas.Nombre = cajas.Nombre;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una caja" +
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
        [Route("api/Cajas/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Cajas Cajas = db.Cajas.Where(a => a.id == id).FirstOrDefault();
                if (Cajas != null)
                {
                    db.Cajas.Remove(Cajas);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una caja con este ID");
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