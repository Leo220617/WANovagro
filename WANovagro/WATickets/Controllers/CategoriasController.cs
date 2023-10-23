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
    public class CategoriasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var Categorias = db.Categorias.ToList();

                

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Categorias);
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
        [Route("api/Categorias/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Categorias categorias = db.Categorias.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, categorias);
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
        [Route("api/Categorias/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Categorias categorias)
        {
            try
            {
                Categorias Categoria = db.Categorias.Where(a => a.id == categorias.id).FirstOrDefault();
                if (Categoria == null)
                {
                    Categoria = new Categorias();
                    Categoria.id = categorias.id;
                    Categoria.CodSAP = categorias.CodSAP;
                    Categoria.Nombre = categorias.Nombre;
                   
                    db.Categorias.Add(Categoria);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una categoria con este ID");
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
        [Route("api/Categorias/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Categorias categorias)
        {
            try
            {
                Categorias Categorias = db.Categorias.Where(a => a.id == categorias.id).FirstOrDefault();
                if (Categorias != null)
                {
                    db.Entry(Categorias).State = System.Data.Entity.EntityState.Modified;
                    Categorias.CodSAP = categorias.CodSAP;
                    Categorias.Nombre = categorias.Nombre;
                   
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una categoria" +
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
        [Route("api/Categorias/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Categorias Categorias = db.Categorias.Where(a => a.id == id).FirstOrDefault();
                if (Categorias != null)
                {
                    db.Categorias.Remove(Categorias);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una categoria con este ID");
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