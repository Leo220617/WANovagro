﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                    a.idUsuarioCreador,
                    a.idUsuarioAceptador,
                    a.FechaCreacion,
                    a.Status,
                    a.Activo,
                    a.Total,
                    a.TotalAprobado
                  

                }).Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true) 
                && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)
                && (filtro.Activo ? a.Activo == filtro.Activo : true)
                && (filtro.Codigo1 > 0 ? a.idCliente == filtro.Codigo1 : true)
                && (!string.IsNullOrEmpty(filtro.Texto) ? a.Status.ToUpper().Contains(filtro.Texto.ToUpper()) : true)
                ).ToList(); //Traemos el listado de productos

                

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
                    Aprobacion.idUsuarioCreador = aprobaciones.idUsuarioCreador;
                    Aprobacion.idUsuarioAceptador = 0;
                    Aprobacion.FechaCreacion = DateTime.Now.Date;
                    Aprobacion.Status = "P";
                    Aprobacion.Activo = true;
                    Aprobacion.Total = 0;
                    Aprobacion.TotalAprobado = 0;
                    
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
                    Aprobacion.idUsuarioAceptador = aprobaciones.idUsuarioAceptador;
                    if (Aprobacion.Status == "A")
                    {
                        Aprobacion.Activo = true;
                    }
                    else
                    {
                        Aprobacion.Activo = false;
                    }
                    Aprobacion.TotalAprobado = aprobaciones.TotalAprobado;
                    Aprobacion.Total = Aprobacion.TotalAprobado;
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
        [Route("api/AprobacionesCreditos/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                AprobacionesCreditos Aprobaciones = db.AprobacionesCreditos.Where(a => a.id == id).FirstOrDefault();
                if (Aprobaciones != null)
                {
                    db.Entry(Aprobaciones).State = System.Data.Entity.EntityState.Modified;


                    if (Aprobaciones.Activo)
                    {

                        Aprobaciones.Activo = false;

                    }
                    else
                    {

                        Aprobaciones.Activo = true;
                    }




                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("No existe una aprobacion con este ID");
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