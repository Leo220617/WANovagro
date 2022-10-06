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
using WATickets.Models.APIS;
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
                var Exoneraciones = db.Exoneraciones.Select(a => new {
                    a.id,
                    a.TipoDoc,
                    a.NumDoc,
                    a.NomInst,
                    a.FechaEmision,
                    a.PorExon,
                    a.idCliente,
                    a.FechaVencimiento,
                    a.Imagen,
                    a.Activo,
                    Detalle = db.DetExoneraciones.Where(b => b.idEncabezado == a.id).ToList()

                }).ToList();


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


                var Exoneraciones = db.Exoneraciones.Select(a => new {
                    a.id,
                    a.TipoDoc,
                    a.NumDoc,
                    a.NomInst,
                    a.FechaEmision,
                    a.PorExon,
                    a.idCliente,
                    a.FechaVencimiento,
                    a.Imagen,
                    a.Activo,
                    Detalle = db.DetExoneraciones.Where(b => b.idEncabezado == a.id).ToList()


                }).Where(a => a.id == id).FirstOrDefault();
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

        [Route("api/Exoneraciones/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ExoneracionesX exoneraciones)
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
                    byte[] hex = Convert.FromBase64String(exoneraciones.Imagen.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                    Exoneracion.Imagen = hex;
                    Exoneracion.NomInst = exoneraciones.NomInst;
                    Exoneracion.FechaEmision = exoneraciones.FechaEmision;
                    Exoneracion.PorExon = exoneraciones.PorExon;
                    Exoneracion.idCliente = exoneraciones.idCliente;
                    Exoneracion.FechaVencimiento = exoneraciones.FechaVencimiento;
                    Exoneracion.Activo = true;
                    db.Exoneraciones.Add(Exoneracion);
                    db.SaveChanges();

                    var i = 0;
                    foreach (var item in exoneraciones.Detalle)
                    {
                        DetExoneraciones det = new DetExoneraciones();
                        det.idEncabezado = Exoneracion.id;
                        det.CodCabys = item.CodCabys;
                        db.DetExoneraciones.Add(det);
                        db.SaveChanges();               
                    }


                   
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
        public HttpResponseMessage Put([FromBody] ExoneracionesX exoneraciones)
        {
            try
            {
                Exoneraciones Exoneraciones = db.Exoneraciones.Where(a => a.id == exoneraciones.id).FirstOrDefault();
                if (Exoneraciones != null)
                {

                    db.Entry(Exoneraciones).State = System.Data.Entity.EntityState.Modified;
                    Exoneraciones.TipoDoc = exoneraciones.TipoDoc;
                    Exoneraciones.NumDoc = exoneraciones.NumDoc;
                    if(!string.IsNullOrEmpty(exoneraciones.Imagen))
                    {
                        byte[] hex = Convert.FromBase64String(exoneraciones.Imagen.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));

                        Exoneraciones.Imagen = hex;
                    }
                    
                    Exoneraciones.NomInst = exoneraciones.NomInst;
                    Exoneraciones.FechaEmision = exoneraciones.FechaEmision;
                    Exoneraciones.idCliente = exoneraciones.idCliente;
                    Exoneraciones.FechaVencimiento = exoneraciones.FechaVencimiento;
                    Exoneraciones.PorExon = exoneraciones.PorExon;
                    db.SaveChanges();

                    var Detalles = db.DetExoneraciones.Where(a => a.idEncabezado == Exoneraciones.id).ToList();

                    foreach (var item in Detalles)
                    {
                        db.DetExoneraciones.Remove(item);
                        db.SaveChanges();
                    }


                    var i = 0;
                    foreach (var item in exoneraciones.Detalle)
                    {
                        DetExoneraciones det = new DetExoneraciones();
                        det.idEncabezado = Exoneraciones.id;
                        det.CodCabys = item.CodCabys;
                        
                        db.DetExoneraciones.Add(det);
                        db.SaveChanges();
                        i++;
                    }

                }
                else
                {
                    throw new Exception("No existe una exoneración" +
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
                    db.Entry(Exoneraciones).State = EntityState.Modified;


                    if (Exoneraciones.Activo)
                    {

                        Exoneraciones.Activo = false;

                    }
                    else
                    {

                        Exoneraciones.Activo = true;
                    }




                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("No existe un usuario con este ID");
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