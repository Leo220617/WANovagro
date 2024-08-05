using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.APIS;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class ArqueosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();



        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {

                var time = new DateTime(); // 01-01-0001
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaInicial = filtro.FechaInicial.Date;
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }
                var Arqueos = db.EncArqueos.Select(a => new
                {
                    a.id,
                    a.idCategoria,
                    a.PalabraClave,
                    a.CodSuc,
                    a.idUsuarioCreador,
                    a.FechaCreacion,
                    a.Validado,
                    a.Status,
                    a.FechaActualizacion,
                    Detalle = db.DetArqueos.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.pendientes == true ? a.Status == "P" : false) || (filtro.espera == true ? a.Status == "E" : false) || (filtro.contabilizado == true ? a.Status == "C" : false)
                || (filtro.rechazados == true ? a.Status == "R" : false)
                && (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true)
                && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)
                && (filtro.Codigo1 > 0 ? a.idUsuarioCreador == filtro.Codigo1 : true)

                && (filtro.Codigo2 > 0 ? a.idCategoria == filtro.Codigo2 : true)


                && (!string.IsNullOrEmpty(filtro.Texto) ? a.CodSuc == filtro.Texto : true)

                ).ToList();



                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Arqueos);
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
        [Route("api/Arqueos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Arqueo = db.EncArqueos.Select(a => new
                {
                    a.id,
                    a.idCategoria,
                    a.PalabraClave,
                    a.CodSuc,
                    a.idUsuarioCreador,
                    a.FechaCreacion,
                    a.Validado,
                    a.Status,
                    a.FechaActualizacion,
                    Detalle = db.DetArqueos.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Arqueo);
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
        [Route("api/Arqueos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Arqueos arqueo)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                EncArqueos Arqueo = db.EncArqueos.Where(a => a.id == arqueo.id).FirstOrDefault();
                if (Arqueo == null)
                {
                    Arqueo = new EncArqueos();
                    Arqueo.idCategoria = arqueo.idCategoria;
                    Arqueo.PalabraClave = arqueo.PalabraClave;
                    Arqueo.CodSuc = arqueo.CodSuc;
                    Arqueo.idUsuarioCreador = arqueo.idUsuarioCreador;
                    Arqueo.FechaCreacion = DateTime.Now;
                    Arqueo.Validado = arqueo.Validado;
                    Arqueo.Status = arqueo.Status;
                    Arqueo.FechaActualizacion = DateTime.Now;
                    db.EncArqueos.Add(Arqueo);
                    db.SaveChanges();





                    foreach (var item in arqueo.Detalle)
                    {
                        DetArqueos det = new DetArqueos();
                        det.idEncabezado = Arqueo.id;
                        det.idProducto = item.idProducto;

                        det.Stock = item.Stock;
                        det.Total = item.Total;
                        det.Diferencia = item.Diferencia;
                        det.Contado = item.Contado;
                        db.DetArqueos.Add(det);
                        db.SaveChanges();

                        var Producto = db.Productos.Where(a => a.id == det.idProducto).FirstOrDefault();
                        db.Entry(Producto).State = EntityState.Modified;
                        Producto.FechaConteo = DateTime.Now;
                        db.SaveChanges();

                    }

                    t.Commit();




                }
                else
                {
                    throw new Exception("Ya existe una oferta con este ID");
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                t.Rollback();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex);
                db.BitacoraErrores.Add(be);
                db.SaveChanges();

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, be);
            }
        }
        [Route("api/Arqueos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Arqueos arqueo)
        {
            var t = db.Database.BeginTransaction();
            try
            {
                EncArqueos Arqueo = db.EncArqueos.Where(a => a.id == arqueo.id).FirstOrDefault();
                if (Arqueo != null)
                {
                    db.Entry(Arqueo).State = EntityState.Modified;
                    if (arqueo.Status == "C" || arqueo.Status == "R")
                    {
                        Arqueo.Status = arqueo.Status;

                        db.SaveChanges();
                    }
                    else
                    {
                        Arqueo.idCategoria = arqueo.idCategoria;
                        Arqueo.PalabraClave = arqueo.PalabraClave;
                        Arqueo.CodSuc = arqueo.CodSuc;
                        Arqueo.idUsuarioCreador = arqueo.idUsuarioCreador;
                        //Arqueo.FechaCreacion = DateTime.Now;
                        Arqueo.Validado = arqueo.Validado;
                        Arqueo.Status = arqueo.Status;
                        Arqueo.FechaActualizacion = DateTime.Now;

                        db.SaveChanges();

                        var Detalles = db.DetArqueos.Where(a => a.idEncabezado == Arqueo.id).ToList();

                        foreach (var item in Detalles)
                        {
                            db.DetArqueos.Remove(item);
                            db.SaveChanges();
                        }



                        foreach (var item in arqueo.Detalle)
                        {
                            DetArqueos det = new DetArqueos();
                            det.idEncabezado = Arqueo.id;
                            det.idProducto = item.idProducto;
                            det.Stock = item.Stock;
                            det.Total = item.Total;
                            det.Diferencia = item.Diferencia;
                            det.Contado = item.Contado;
                            db.DetArqueos.Add(det);
                            db.SaveChanges();

                            var Producto = db.Productos.Where(a => a.id == det.idProducto).FirstOrDefault();
                            db.Entry(Producto).State = EntityState.Modified;
                            Producto.FechaConteo = DateTime.Now;
                            db.SaveChanges();
                        }
                    }







                    t.Commit();
                }
                else
                {
                    throw new Exception("NO existe una oferta con este ID");
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                t.Rollback();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex);
                db.BitacoraErrores.Add(be);
                db.SaveChanges();

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, be);
            }
        }

        [Route("api/Arqueos/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                EncArqueos Arqueos = db.EncArqueos.Where(a => a.id == id).FirstOrDefault();
                if (Arqueos != null)
                {
                    db.Entry(Arqueos).State = EntityState.Modified;


                    if (Arqueos.Status == "R")
                    {

                        Arqueos.Status = "P";

                    }
                  




                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("No existe un arqueo con este ID");
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