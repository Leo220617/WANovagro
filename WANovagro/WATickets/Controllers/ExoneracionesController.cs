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
    public class ExoneracionesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll()
        {
            try
            {
               

                var Exoneraciones = db.Exoneraciones.ToList();
               


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Exoneraciones);
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
        [Route("api/Exoneraciones/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
              

                Exoneraciones exoneraciones = db.Exoneraciones.Where(a => a.id == id).FirstOrDefault();

               

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, exoneraciones);
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

        [Route("api/Exoneraciones/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Exoneraciones exoneraciones)
        {
            try
            {
               

                Exoneraciones Exoneracion = db.Exoneraciones.Where(a => a.id == exoneraciones.id).FirstOrDefault();
                if (Exoneracion == null)
                {
                    Exoneracion = new Exoneraciones();
                    Exoneracion.id = exoneraciones.id;
                    Exoneracion.TipoDoc = exoneraciones.TipoDoc;
                    Exoneracion.NumDoc = exoneraciones.NumDoc;
                    Exoneracion.NomInst = exoneraciones.NomInst;
                    Exoneracion.FechaEmision = exoneraciones.FechaEmision;
                    Exoneracion.PorExon = exoneraciones.PorExon;
                    db.Exoneraciones.Add(Exoneracion);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una exoneración con este ID");
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
        [Route("api/Exoneraciones/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Exoneraciones exoneraciones)
        {
            try
            {
                Exoneraciones Exoneraciones = db.Exoneraciones.Where(a => a.id == exoneraciones.id).FirstOrDefault();
                if (Exoneraciones != null)
                {
                    db.Entry(Exoneraciones).State = System.Data.Entity.EntityState.Modified;
                    Exoneraciones.TipoDoc = exoneraciones.TipoDoc;
                    Exoneraciones.NumDoc = exoneraciones.NumDoc;
                    Exoneraciones.NomInst = exoneraciones.NomInst;
                    Exoneraciones.FechaEmision = exoneraciones.FechaEmision;
                    Exoneraciones.PorExon = exoneraciones.PorExon;
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
        [Route("api/Exoneraciones/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
              

                Exoneraciones Exoneraciones = db.Exoneraciones.Where(a => a.id == id).FirstOrDefault();
                if (Exoneraciones != null)
                {
                    db.Exoneraciones.Remove(Exoneraciones);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una exoneración con este ID");
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