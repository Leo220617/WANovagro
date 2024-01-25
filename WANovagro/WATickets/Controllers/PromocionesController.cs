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
    public class PromocionesController : ApiController
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
                var Promociones = db.EncPromociones.Select(a => new
                {
                    a.id,
                    a.idListaPrecio,
                    a.Nombre,
                    a.Fecha,
                    a.FechaVencimiento,
                    
                    Detalle = db.Promociones.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

              

                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Promociones = Promociones.Where(a => a.idListaPrecio == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
             

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Promociones);
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
        [Route("api/Promociones/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Promocion = db.EncPromociones.Select(a => new
                {
                    a.id,
                    a.idListaPrecio,
                    a.Nombre,
                    a.Fecha,
                    a.FechaVencimiento,

                    Detalle = db.Promociones.Where(b => b.idEncabezado == a.id).ToList()
            

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Promocion);
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




        [Route("api/Promociones/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Promocion promocion)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                var time = DateTime.Now.Date;
                EncPromociones Promo = db.EncPromociones.Where(a => a.id == promocion.id).FirstOrDefault();
                if (Promo == null)
                {
                    Promo = new EncPromociones();
                    Promo.idListaPrecio = promocion.idListaPrecio;
                    Promo.Nombre = promocion.Nombre;
                    Promo.Fecha = promocion.Fecha.Date;
                    Promo.FechaVencimiento = promocion.FechaVencimiento.Date;
                    Promo.FechaCreacion = DateTime.Now;
                    Promo.idUsuarioCreador = promocion.idUsuarioCreador;
                    db.EncPromociones.Add(Promo);
                    db.SaveChanges();





                    var i = 0;
                    foreach (var item in promocion.Detalle)
                    {
                      
                        var PromoVieja = db.Promociones.Where(a => a.ItemCode == item.ItemCode && a.idListaPrecio == item.idListaPrecio && a.idCategoria == item.idCategoria && item.FechaVen > time).FirstOrDefault();
                        if(PromoVieja == null) {
                            Promociones det = new Promociones();
                            det.idEncabezado = Promo.id;
                            det.ItemCode = item.ItemCode;
                            det.idCategoria = item.idCategoria;
                            det.idListaPrecio = item.idListaPrecio;
                            det.FechaVen = item.FechaVen.Date;
                            det.Fecha = item.Fecha.Date;
                            det.Moneda = item.Moneda;
                            det.PrecioFinal = item.PrecioFinal;
                            det.PrecioAnterior = item.PrecioAnterior;
                            db.Promociones.Add(det);
                            db.SaveChanges();

                            if(det.Fecha == time && det.FechaVen >= time) {
                                var ProductoX = db.Productos.Where(a => a.Codigo == det.ItemCode && a.idCategoria == det.idCategoria && a.idListaPrecios == det.idListaPrecio).ToList();
                                foreach(var item2 in ProductoX)
                                {
                                    db.Entry(item2).State = EntityState.Modified;
                                    item2.PrecioUnitario = det.PrecioFinal;
                                    item2.FechaActualizacion = DateTime.Now;
                                    db.SaveChanges();

                                }

                            }
                           
                            
                            
                            i++;
                        }
                        else
                        {
                            var Producto = db.Productos.Where(a => a.Codigo == PromoVieja.ItemCode && a.idCategoria == PromoVieja.idCategoria && a.idListaPrecios == PromoVieja.idListaPrecio).FirstOrDefault();
                            var EncVieja = db.EncPromociones.Where(a => a.id == PromoVieja.idEncabezado && a.idListaPrecio == PromoVieja.idListaPrecio).FirstOrDefault();

                            throw new Exception("Ya hay una Promoción activa para el Producto " + PromoVieja.ItemCode + "-" + Producto.Nombre + " la cual vence " + PromoVieja.FechaVen.ToString("yyyy-MM-dd")
                            + " si desea realizar cambios favor editar la Promoción " + EncVieja.id + "-" + EncVieja.Nombre);


                        }

                        
                        

                    

                    }





                    t.Commit();

                }
                else
                {
                    throw new Exception("Ya existe una promocion con este ID");
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


        [Route("api/Promociones/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Promocion promocion)
        {
            var t = db.Database.BeginTransaction();
            try
            {
                EncPromociones Promo = db.EncPromociones.Where(a => a.id == promocion.id).FirstOrDefault();
                var time = DateTime.Now.Date;
                if (Promo != null)
                {
                    db.Entry(Promo).State = EntityState.Modified;
                    Promo.idListaPrecio = promocion.idListaPrecio;
                    Promo.Nombre = promocion.Nombre;
                    Promo.Fecha = promocion.Fecha.Date;
                    Promo.FechaVencimiento = promocion.FechaVencimiento.Date;
                    Promo.FechaCreacion = DateTime.Now;
                    Promo.idUsuarioCreador = promocion.idUsuarioCreador;
                 
                    db.SaveChanges();

                   

                    var Detalles = db.Promociones.Where(a => a.idEncabezado == Promo.id).ToList();

                    foreach (var item in Detalles)
                    {
                        db.Promociones.Remove(item);
                        db.SaveChanges();
                    }


                    var i = 0;
                    foreach (var item in promocion.Detalle)
                    {
                        var PromoVieja = db.Promociones.Where(a => a.ItemCode == item.ItemCode && a.idListaPrecio == item.idListaPrecio && a.idCategoria == item.idCategoria && item.FechaVen > time).FirstOrDefault();
                        if (PromoVieja == null)
                        {
                            Promociones det = new Promociones();
                            det.idEncabezado = Promo.id;
                            det.ItemCode = item.ItemCode;
                            det.idCategoria = item.idCategoria;
                            det.idListaPrecio = item.idListaPrecio;
                            det.FechaVen = Promo.FechaVencimiento.Date;
                            det.Fecha = Promo.Fecha.Date;
                            det.Moneda = item.Moneda;
                            det.PrecioFinal = item.PrecioFinal;
                            det.PrecioAnterior = item.PrecioAnterior;

                            db.Promociones.Add(det);
                            db.SaveChanges();


                            if (det.Fecha == time && det.FechaVen >= time)
                            {
                                var ProductoX = db.Productos.Where(a => a.Codigo == det.ItemCode && a.idCategoria == det.idCategoria && a.idListaPrecios == det.idListaPrecio).ToList();
                                foreach (var item2 in ProductoX)
                                {
                                    db.Entry(item2).State = EntityState.Modified;
                                    item2.PrecioUnitario = det.PrecioFinal;
                                    item2.FechaActualizacion = DateTime.Now;
                                    db.SaveChanges();

                                }

                            }
                            i++;
                        }
                        else
                        {
                            var Producto = db.Productos.Where(a => a.Codigo == PromoVieja.ItemCode && a.idCategoria == PromoVieja.idCategoria && a.idListaPrecios == PromoVieja.idListaPrecio).FirstOrDefault();
                            var EncVieja = db.EncPromociones.Where(a => a.id == PromoVieja.idEncabezado && a.idListaPrecio == PromoVieja.idListaPrecio).FirstOrDefault();

                            throw new Exception("Ya hay una Promoción activa para el Producto " + PromoVieja.ItemCode + "-" + Producto.Nombre + " la cual vence " + PromoVieja.FechaVen.ToString("yyyy-MM-dd")
                            + " si desea realizar cambios favor editar la Promoción " + EncVieja.id + "-" + EncVieja.Nombre);


                        }

                    }


                    t.Commit();
                }
                else
                {
                    throw new Exception("NO existe una promocion con este ID");
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