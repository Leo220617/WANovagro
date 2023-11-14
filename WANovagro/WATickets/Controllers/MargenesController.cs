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
    public class MargenesController : ApiController
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
                var Margenes = db.EncMargenes.Select(a => new
                {
                    a.idListaPrecio,
                    a.idCategoria,
                    a.Moneda,
                    a.Cobertura,
                    a.Margen,
                    a.MargenMin,
                    a.idUsuarioCreador,
                    a.FechaCreacion,

                    Detalle = db.DetMargenes.Where(b => b.idListaPrecio == a.idListaPrecio && b.idCategoria == a.idCategoria && b.Moneda == a.Moneda).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.FechaCreacion >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.FechaCreacion <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos



                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Margenes = Margenes.Where(a => a.idListaPrecio == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }

                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Margenes = Margenes.Where(a => a.idCategoria == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (!string.IsNullOrEmpty(filtro.Texto))
                {

                    Margenes = Margenes.Where(a => a.Moneda.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();// filtramos por lo que trae texto
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Margenes);
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

        [Route("api/Margenes/Consultar")]
        public HttpResponseMessage GetOne([FromUri]int idListaPrecio, int idCategoria, string Moneda)
        {
            try
            {
                var Margen = db.EncMargenes.Select(a => new
                {
                    a.idListaPrecio,
                    a.idCategoria,
                    a.Moneda,
                    a.Cobertura,
                    a.Margen,
                    a.MargenMin,
                    a.idUsuarioCreador,
                    a.FechaCreacion,

                    Detalle = db.DetMargenes.Where(b => b.idListaPrecio == a.idListaPrecio && b.idCategoria == a.idCategoria && b.Moneda == a.Moneda).ToList()


                }).Where(a => a.idListaPrecio == idListaPrecio && a.idCategoria == idCategoria && a.Moneda == Moneda).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Margen);
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

        [Route("api/Margenes/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Margenes margenes)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                Parametros param = db.Parametros.FirstOrDefault();

                EncMargenes Margen = db.EncMargenes.Where(a => a.idListaPrecio == margenes.idListaPrecio && a.idCategoria == margenes.idCategoria && a.Moneda == margenes.Moneda).FirstOrDefault();
                if (Margen == null)
                {
                    Margen = new EncMargenes();
                    Margen.idListaPrecio = margenes.idListaPrecio;
                    Margen.idCategoria = margenes.idCategoria;
                    Margen.Moneda = margenes.Moneda;
                    Margen.Cobertura = margenes.Cobertura;
                    Margen.Margen = margenes.Margen;
                    Margen.MargenMin = margenes.MargenMin;
                    Margen.idUsuarioCreador = margenes.idUsuarioCreador;
                    Margen.FechaCreacion = DateTime.Now;
                    db.EncMargenes.Add(Margen);
                    db.SaveChanges();





                    if (margenes.Detalle != null)
                    {
                        var i = 0;
                        foreach (var item in margenes.Detalle)
                        {


                            DetMargenes det = new DetMargenes();
                            det.ItemCode = item.ItemCode;
                            det.idListaPrecio = item.idListaPrecio;
                            det.idCategoria = item.idCategoria;
                            det.Moneda = item.Moneda;
                            det.PrecioSAP = item.PrecioSAP;
                            det.Cobertura = item.Cobertura;
                            det.Margen = item.Margen;
                            det.MargenMin = item.MargenMin;
                            det.PrecioFinal = item.PrecioFinal;
                            det.PrecioMin = item.PrecioMin;
                            det.PrecioCob = item.PrecioCob;
                            db.DetMargenes.Add(det);
                            db.SaveChanges();

                            i++;


                        }
                    }




                    t.Commit();

                }
                else
                {
                    throw new Exception("Ya existe un Margen con este ID");
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

        [Route("api/Margenes/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody]  Margenes margenes)
        {
            var t = db.Database.BeginTransaction();
            try
            {
                EncMargenes Margen = db.EncMargenes.Where(a => a.idListaPrecio == margenes.idListaPrecio && a.idCategoria == margenes.idCategoria && a.Moneda == margenes.Moneda).FirstOrDefault();

                if (Margen != null)
                {
                    db.Entry(Margen).State = EntityState.Modified;
                    Margen.idListaPrecio = margenes.idListaPrecio;
                    Margen.idCategoria = margenes.idCategoria;
                    Margen.Moneda = margenes.Moneda;
                    Margen.Cobertura = margenes.Cobertura;
                    Margen.Margen = margenes.Margen;
                    Margen.MargenMin = margenes.MargenMin;
                    Margen.idUsuarioCreador = margenes.idUsuarioCreador;
                    Margen.FechaCreacion = DateTime.Now;

                    db.SaveChanges();



                    var Detalles = db.DetMargenes.Where(a => a.idListaPrecio == Margen.idListaPrecio && a.idCategoria == Margen.idCategoria && a.Moneda == Margen.Moneda).ToList();

                    foreach (var item in Detalles)
                    {
                        db.DetMargenes.Remove(item);
                        db.SaveChanges();
                    }

                    if (margenes.Detalle != null)
                    {
                        var i = 0;
                        foreach (var item in margenes.Detalle)
                        {
                            DetMargenes det = new DetMargenes();
                            det.ItemCode = item.ItemCode;
                            det.idListaPrecio = item.idListaPrecio;
                            det.idCategoria = item.idCategoria;
                            det.Moneda = item.Moneda;
                            det.PrecioSAP = item.PrecioSAP;
                            det.Cobertura = item.Cobertura;
                            det.Margen = item.Margen;
                            det.MargenMin = item.MargenMin;
                            det.PrecioFinal = item.PrecioFinal;
                            det.PrecioMin = item.PrecioMin;
                            det.PrecioCob = item.PrecioCob;
                            db.DetMargenes.Add(det);
                            db.SaveChanges();
                            i++;
                        }

                    }



                    t.Commit();
                }
                else
                {
                    throw new Exception("NO existe un Margen con este ID");
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
    }
}