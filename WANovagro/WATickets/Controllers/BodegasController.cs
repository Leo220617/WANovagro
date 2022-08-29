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
{   [Authorize]
    public class BodegasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll()
        {
            try
            {
                var Bodegas = db.Bodegas.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Bodegas);
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
        [Route("api/Bodegas/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Bodegas bodegas = db.Bodegas.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, bodegas);
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
        [Route("api/Bodegas/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Bodegas bodegas)
        {
            try
            {
                Bodegas Bodega = db.Bodegas.Where(a => a.id == bodegas.id).FirstOrDefault();
                if (Bodega == null)
                {
                    Bodega= new Bodegas();
                    Bodega.id = bodegas.id;
                    Bodega.CodSuc = bodegas.CodSuc;
                    Bodega.Nombre = bodegas.Nombre;
                    db.Bodegas.Add(Bodega);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una bodega con este ID");
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
        [Route("api/Bodegas/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Bodegas bodegas)
        {
            try
            {
                Bodegas Bodegas = db.Bodegas.Where(a => a.id == bodegas.id).FirstOrDefault();
                if (Bodegas != null)
                {
                    db.Entry(Bodegas).State = System.Data.Entity.EntityState.Modified;
                    Bodegas.CodSuc = bodegas.CodSuc;
                    Bodegas.Nombre = bodegas.Nombre;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una bodega" +
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
        [Route("api/Bodegas/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Bodegas Bodegas = db.Bodegas.Where(a => a.id == id).FirstOrDefault();
                if (Bodegas != null)
                {
                    db.Bodegas.Remove(Bodegas);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una bodega con este ID");
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