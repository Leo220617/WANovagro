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
    public class CabysController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll()
        {
            try
            {
                var Cabys = db.Cabys.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Cabys);
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
        [Route("api/Cabys/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Cabys cabys = db.Cabys.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, cabys);
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
        [Route("api/Cabys/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Cabys cabys)
        {
            try
            {
                Cabys Cabys = db.Cabys.Where(a => a.id == cabys.id).FirstOrDefault();
                if (Cabys == null)
                {
                    Cabys = new Cabys();
                    Cabys.id = cabys.id;
                    Cabys.Descripcion = cabys.Descripcion;
                    Cabys.CodCabys = cabys.CodCabys;
                    db.Cabys.Add(Cabys);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un cabys con este ID");
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
        [Route("api/Cabys/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Cabys cabys)
        {
            try
            {
                Cabys Cabys = db.Cabys.Where(a => a.id == cabys.id).FirstOrDefault();
                if (Cabys != null)
                {
                    db.Entry(Cabys).State = System.Data.Entity.EntityState.Modified;
                    Cabys.Descripcion = cabys.Descripcion;
                    Cabys.CodCabys = cabys.CodCabys;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un cabys" +
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
        [Route("api/Cabys/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Cabys Cabys = db.Cabys.Where(a => a.id == id).FirstOrDefault();
                if (Cabys != null)
                {
                    db.Cabys.Remove(Cabys);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un cabys con este ID");
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