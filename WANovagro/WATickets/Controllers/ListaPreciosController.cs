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
    public class ListaPreciosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll()
        {
            try
            {
                var ListaPrecios = db.ListaPrecios.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, ListaPrecios);
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
        [Route("api/ListaPrecios/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                ListaPrecios listaprecios = db.ListaPrecios.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, listaprecios);
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
        [Route("api/ListaPrecios/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ListaPrecios listaprecios)
        {
            try
            {
                ListaPrecios ListaPrecio = db.ListaPrecios.Where(a => a.id == listaprecios.id).FirstOrDefault();
                if (ListaPrecio == null)
                {
                    ListaPrecio = new ListaPrecios();
                    ListaPrecio.id = listaprecios.id;
                    ListaPrecio.CodSAP = listaprecios.CodSAP;
                    ListaPrecio.Nombre = listaprecios.Nombre;
                    db.ListaPrecios.Add(ListaPrecio);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una lista de precio con este ID");
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
        [Route("api/ListaPrecios/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] ListaPrecios listaprecios)
        {
            try
            {
                ListaPrecios ListaPrecio = db.ListaPrecios.Where(a => a.id == listaprecios.id).FirstOrDefault();
                if (ListaPrecio != null)
                {
                    db.Entry(ListaPrecio).State = System.Data.Entity.EntityState.Modified;
                    ListaPrecio.CodSAP = listaprecios.CodSAP;
                    ListaPrecio.Nombre = listaprecios.Nombre;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una lista de precio" +
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
        [Route("api/ListaPrecios/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                ListaPrecios ListaPrecios = db.ListaPrecios.Where(a => a.id == id).FirstOrDefault();
                if (ListaPrecios != null)
                {
                    db.ListaPrecios.Remove(ListaPrecios);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una lista de precio con este ID");
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