using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
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
    public class CorreoEnvioController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll()
        {
            try
            {
                var CorreoEnvios = db.CorreoEnvio.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, CorreoEnvios);
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
        [Route("api/CorreoEnvio/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                CorreoEnvio correoEnvio = db.CorreoEnvio.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, correoEnvio);
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
        [Route("api/CorreoEnvio/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] CorreoEnvio correoEnvio)
        {
            try
            {
                CorreoEnvio Correo = db.CorreoEnvio.Where(a => a.id == correoEnvio.id).FirstOrDefault();
                if (Correo == null)
                {
                    Correo = new CorreoEnvio();
                    Correo.id = correoEnvio.id;
                    Correo.RecepcionHostName = correoEnvio.RecepcionHostName;
                    Correo.EnvioPort = correoEnvio.EnvioPort;
                    Correo.RecepcionUseSSL = correoEnvio.RecepcionUseSSL;
                    Correo.RecepcionEmail = correoEnvio.RecepcionEmail;
                    Correo.RecepcionPassword = correoEnvio.RecepcionPassword;
                    db.CorreoEnvio.Add(Correo);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un Correo Envio con este ID");
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
        [Route("api/CorreoEnvio/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] CorreoEnvio correoEnvio)
        {
            try
            {
                CorreoEnvio Correo = db.CorreoEnvio.Where(a => a.id == correoEnvio.id).FirstOrDefault();
                if (Correo != null)
                {
                    db.Entry(Correo).State = System.Data.Entity.EntityState.Modified;
                    Correo.RecepcionHostName = correoEnvio.RecepcionHostName;
                    Correo.EnvioPort = correoEnvio.EnvioPort;
                    Correo.RecepcionUseSSL = correoEnvio.RecepcionUseSSL;
                    Correo.RecepcionEmail = correoEnvio.RecepcionEmail;
                    Correo.RecepcionPassword = correoEnvio.RecepcionPassword;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un Correo Envio" +
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
        [Route("api/CorreoEnvio/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                CorreoEnvio Correo = db.CorreoEnvio.Where(a => a.id == id).FirstOrDefault();
                if (Correo != null)
                {
                    db.CorreoEnvio.Remove(Correo);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un Correo Envio con este ID");
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


