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
    public class AprobacionesCreditosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var time = DateTime.Now; // 01-01-0001
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaInicial = filtro.FechaInicial.Date;
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }
                var Aprobaciones = db.AprobacionesCreditos.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.FechaCreacion,
                    a.Status,
                    a.Activo,
                    a.Total
                  

                }).Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (filtro.Activo)
                {
                    Aprobaciones = Aprobaciones.Where(a => a.Activo == filtro.Activo).ToList();
                }
                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Aprobaciones = Aprobaciones.Where(a => a.idCliente == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Aprobaciones = Aprobaciones.Where(a => a.Status.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Aprobaciones);
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
        [Route("api/AprobacionesCreditos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                AprobacionesCreditos aprobaciones = db.AprobacionesCreditos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, aprobaciones);
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
        [Route("api/AprobacionesCreditos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] AprobacionesCreditos aprobaciones)
        {
            try
            {
                AprobacionesCreditos Aprobacion = db.AprobacionesCreditos.Where(a => a.id == aprobaciones.id).FirstOrDefault();
                if (Aprobacion == null)
                {
                    Aprobacion = new AprobacionesCreditos();
                    Aprobacion.id = aprobaciones.id;
                    Aprobacion.idCliente = aprobaciones.idCliente;
                    Aprobacion.FechaCreacion = DateTime.Now;
                    Aprobacion.Status = "P";
                    Aprobacion.Activo = true;
                    Aprobacion.Total = 0;
                    db.AprobacionesCreditos.Add(Aprobacion);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una aprobacion de credito con este ID");
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
        [Route("api/AprobacionesCreditos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] AprobacionesCreditos aprobaciones)
        {
            try
            {
                AprobacionesCreditos Aprobacion = db.AprobacionesCreditos.Where(a => a.id == aprobaciones.id).FirstOrDefault();
                if (Aprobacion != null && aprobaciones.Status != "0")
                {
                    db.Entry(Aprobacion).State = System.Data.Entity.EntityState.Modified;
                    Aprobacion.Status = aprobaciones.Status;
                    if (Aprobacion.Status == "A")
                    {
                        Aprobacion.Activo = true;
                    }
                    else
                    {
                        Aprobacion.Activo = false;
                    }

                    Aprobacion.Total = aprobaciones.Total;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una aprobacion" +
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
    }
}