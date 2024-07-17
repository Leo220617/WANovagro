using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
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
                var Margenes = db.EncMargenes.Where(a => (filtro.Codigo1 > 0 ? a.idListaPrecio == filtro.Codigo1 : true)
                && (filtro.Codigo2 > 0 ? a.idCategoria == filtro.Codigo2 : true)
                && (!string.IsNullOrEmpty(filtro.Texto) ? a.Moneda.ToUpper().Contains(filtro.Texto.ToUpper()) : true)
                ).ToList();

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
                            det.Seteable = item.Seteable;
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
                            det.Seteable = item.Seteable;
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

        [Route("api/Margenes/MargenesProductos")]
        public HttpResponseMessage GetExtraeDatos([FromUri]int idListaPrecio, int idCategoria, string Moneda)
        {
            try
            {


                var ProductosSP = db.Productos.Where(a => a.idListaPrecios == idListaPrecio && a.idCategoria == idCategoria && a.Moneda == Moneda).ToList();

                foreach (var item in ProductosSP)
                {


                    var Producto = item;

                    if (Producto != null)
                    {
                        try
                        {
                            db.Entry(Producto).State = EntityState.Modified;


                            var time = DateTime.Now.Date;
                            var Promocion = db.Promociones.Where(a => a.ItemCode == Producto.Codigo && a.idListaPrecio == Producto.idListaPrecios && a.idCategoria == Producto.idCategoria && a.Fecha <= time && a.FechaVen >= time).FirstOrDefault();
                            var Margenes = db.EncMargenes.Where(a => a.idListaPrecio == Producto.idListaPrecios && a.Moneda == Producto.Moneda && a.idCategoria == Producto.idCategoria).FirstOrDefault();
                            var DetMargenes = db.DetMargenes.Where(a => a.ItemCode == Producto.Codigo && a.idListaPrecio == Producto.idListaPrecios && a.Moneda == Producto.Moneda && a.idCategoria == Producto.idCategoria).FirstOrDefault();

                            if (Promocion != null)
                            {
                                if (Producto.PrecioUnitario != Promocion.PrecioFinal)
                                {
                                    var BitacoraMargenes = new BitacoraMargenes();
                                    BitacoraMargenes.ItemCode = Promocion.ItemCode;
                                    BitacoraMargenes.idCategoria = Promocion.idCategoria;
                                    BitacoraMargenes.idListaPrecio = Promocion.idListaPrecio;
                                    BitacoraMargenes.PrecioAnterior = Producto.PrecioUnitario;
                                    BitacoraMargenes.PrecioNuevo = Promocion.PrecioFinal;
                                    BitacoraMargenes.Fecha = DateTime.Now;
                                    db.BitacoraMargenes.Add(BitacoraMargenes);
                                    db.SaveChanges();
                                }
                                Producto.PrecioUnitario = Promocion.PrecioFinal;


                            }
                            else if (DetMargenes != null)
                            {
                                if (Producto.PrecioUnitario != DetMargenes.PrecioFinal)
                                {
                                    var BitacoraMargenes = new BitacoraMargenes();
                                    BitacoraMargenes.ItemCode = Producto.Codigo;
                                    BitacoraMargenes.idCategoria = DetMargenes.idCategoria;
                                    BitacoraMargenes.idListaPrecio = DetMargenes.idListaPrecio;
                                    BitacoraMargenes.PrecioAnterior = Producto.PrecioUnitario;
                                    BitacoraMargenes.PrecioNuevo = DetMargenes.PrecioFinal;
                                    BitacoraMargenes.Fecha = DateTime.Now;
                                    db.BitacoraMargenes.Add(BitacoraMargenes);
                                    db.SaveChanges();
                                }
                                Producto.PrecioUnitario = DetMargenes.PrecioFinal;
                            }
                            else if (Margenes != null)
                            {
                                var PrecioCob = Producto.Costo / (1 - (Margenes.Cobertura / 100));
                                var PrecioFinal = PrecioCob / (1 - (Margenes.Margen / 100));
                                if (Producto.PrecioUnitario != PrecioFinal)
                                {
                                    var BitacoraMargenes = new BitacoraMargenes();
                                    BitacoraMargenes.ItemCode = Producto.Codigo;
                                    BitacoraMargenes.idCategoria = Margenes.idCategoria;
                                    BitacoraMargenes.idListaPrecio = Margenes.idListaPrecio;
                                    BitacoraMargenes.PrecioAnterior = Producto.PrecioUnitario;
                                    BitacoraMargenes.PrecioNuevo = PrecioFinal;
                                    BitacoraMargenes.Fecha = DateTime.Now;
                                    db.BitacoraMargenes.Add(BitacoraMargenes);
                                    db.SaveChanges();
                                }

                                Producto.PrecioUnitario = PrecioFinal;
                            }



                            db.SaveChanges();
                        }
                        catch (Exception ex1)
                        {

                            ModelCliente db2 = new ModelCliente();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = ex1.Message;
                            be.StrackTrace = ex1.StackTrace;
                            be.Fecha = DateTime.Now;
                            be.JSON = JsonConvert.SerializeObject(ex1);
                            db2.BitacoraErrores.Add(be);
                            db2.SaveChanges();
                        }

                    }
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