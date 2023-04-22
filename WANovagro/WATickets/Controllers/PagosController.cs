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
    public class PagosController : ApiController
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
                var Pagos = db.EncPagos.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.CodSuc,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.FechaContabilizacion,
                    a.Comentarios,
                    a.Referencia,
                    a.TotalPagado,
                    a.Moneda,
                 
                    Detalle = db.DetPagos.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Pagos = Pagos.Where(a => a.CodSuc == filtro.Texto).ToList();
                }

                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Pagos = Pagos.Where(a => a.idCliente == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
          

               
            
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Pagos);
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


        [Route("api/Pagos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Pago = db.EncPagos.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.CodSuc,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.FechaContabilizacion,
                    a.Comentarios,
                    a.Referencia,
                    a.TotalPagado,
                    a.Moneda,

                    Detalle = db.DetPagos.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Pago);
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

        [Route("api/Pagos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Pagos pago)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                EncPagos Pago = db.EncPagos.Where(a => a.id == pago.id).FirstOrDefault();
                if (Pago == null)
                {
                    Pago = new EncPagos();
                    Pago.idCliente = pago.idCliente;
                    Pago.CodSuc = pago.CodSuc;
                    Pago.Fecha = DateTime.Now;
                    Pago.FechaVencimiento = pago.FechaVencimiento;
                    Pago.FechaContabilizacion = pago.FechaContabilizacion;
                    Pago.Comentarios = pago.Comentarios;
                    Pago.Referencia = pago.Referencia;
                    Pago.TotalPagado = pago.TotalPagado;
                    Pago.Moneda = pago.Moneda;

                    db.EncPagos.Add(Pago);
                    db.SaveChanges();

                    var i = 0;
                    foreach (var item in pago.Detalle)
                    {
                        DetPagos det = new DetPagos();
                        det.idEncabezado = Pago.id;
                        det.idEncDocumentoCredito = item.idEncDocumentoCredito;
                        det.NumLinea = i;
                       
                        det.Total = item.Total;
                   
                     
                        db.DetPagos.Add(det);
                        db.SaveChanges();
                        i++;
                    }


                
                    t.Commit();

                  


                }
                else
                {
                    throw new Exception("Ya existe un Pago con este ID");
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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/Pagos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Pagos pagos)
        {
            var t = db.Database.BeginTransaction();
            try
            {
                EncPagos Pago = db.EncPagos.Where(a => a.id == pagos.id).FirstOrDefault();
                if (Pago != null)
                {
                    db.Entry(Pago).State = EntityState.Modified;
                    Pago.idCliente = pagos.idCliente;
                    Pago.CodSuc = pagos.CodSuc;
                    Pago.Fecha = DateTime.Now;
                    Pago.FechaVencimiento = pagos.FechaVencimiento;
                    Pago.FechaContabilizacion = pagos.FechaContabilizacion;
                    Pago.Comentarios = pagos.Comentarios;
                    Pago.Referencia = pagos.Referencia;
                    Pago.TotalPagado = pagos.TotalPagado;
                    Pago.Moneda = pagos.Moneda;

                 


                    db.SaveChanges();

                    var Detalles = db.DetPagos.Where(a => a.idEncabezado == Pago.id).ToList();

                    foreach (var item in Detalles)
                    {
                        db.DetPagos.Remove(item);
                        db.SaveChanges();
                    }


                    var i = 0;
                    foreach (var item in pagos.Detalle)
                    {
                        DetPagos det = new DetPagos();
                        det.idEncabezado = Pago.id;
                        det.idEncDocumentoCredito = item.idEncDocumentoCredito;
                        det.NumLinea = i;
                        det.Total = item.Total;
                        db.DetPagos.Add(det);
                        db.SaveChanges();
                        i++;
                    }


                   
                    t.Commit();
                }
                else
                {
                    throw new Exception("NO existe un Pago con este ID");
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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}