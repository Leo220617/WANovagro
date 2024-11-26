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
                if (!string.IsNullOrEmpty(filtro.CardName))
                {
                    var EncPromociones = db.EncPromociones.Select(a => new
                    {
                        a.id,
                        a.idListaPrecio,
                        a.Nombre,
                        a.Fecha,
                        a.FechaVencimiento,
                        a.Moneda,
                        Detalle = db.Promociones.Where(b => b.idEncabezado == a.id).ToList(),
                        Clientes = db.ClientesPromociones.Where(c => c.idPromocion == a.id).ToList()
                    }).ToList();

           
                    EncPromociones = EncPromociones.Where(b => b.Detalle.Any(c => !string.IsNullOrEmpty(filtro.CardName) ? c.ItemCode.ToUpper().Contains(filtro.CardName.ToUpper()) : true)).ToList();

                    return Request.CreateResponse(System.Net.HttpStatusCode.OK, EncPromociones);
                }

                else
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
                        a.Moneda,

                        Detalle = db.Promociones.Where(b => b.idEncabezado == a.id).ToList(),
                        Clientes = db.ClientesPromociones.Where(c => c.idPromocion == a.id).ToList()

                    }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos



                    if (filtro.Codigo1 > 0) // esto por ser integer
                    {
                        Promociones = Promociones.Where(a => a.idListaPrecio == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                    }
                    return Request.CreateResponse(System.Net.HttpStatusCode.OK, Promociones);
                }


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
                    a.Moneda,

                    Detalle = db.Promociones.Where(b => b.idEncabezado == a.id).ToList(),
                    Clientes = db.ClientesPromociones.Where(c => c.idPromocion == a.id).ToList()


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
                var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == time).FirstOrDefault();
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
                    Promo.Moneda = promocion.Moneda;
                    db.EncPromociones.Add(Promo);
                    db.SaveChanges();




                    var x = 0;
                    if (promocion.Clientes != null)
                    {
                        foreach (var item in promocion.Clientes)
                        {

                            var ClientePromocion = db.ClientesPromociones.Where(a => a.idPromocion == item.idPromocion && a.idCliente == item.idCliente).FirstOrDefault();
                            if (ClientePromocion == null)
                            {
                                var Objetos = new ClientesPromociones();
                                Objetos.idPromocion = Promo.id;
                                Objetos.idCliente = item.idCliente;


                                db.ClientesPromociones.Add(Objetos);
                                db.SaveChanges();





                                x++;
                            }
                            else
                            {
                                var Client = db.Clientes.Where(a => a.id == item.idCliente).FirstOrDefault();
                                throw new Exception("Ya el Cliente" + Client.Codigo + "-" + Client.Nombre + " esta asignado a la promocion #" + Promo.id);
                            }







                        }
                    }
                    var i = 0;
                    foreach (var item in promocion.Detalle)
                    {
                        var ClientePromo = db.ClientesPromociones.Where(a => a.idPromocion == Promo.id).FirstOrDefault();
                        var PromoVieja = db.Promociones.Where(a => a.ItemCode == item.ItemCode && a.idListaPrecio == item.idListaPrecio && a.idCategoria == item.idCategoria && a.FechaVen > time && a.Cliente == false).FirstOrDefault();
                        if (PromoVieja == null || promocion.Clientes.Count > 0)
                        {
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
                            if (ClientePromo != null)
                            {
                                det.Cliente = true;
                            }
                            else
                            {
                                det.Cliente = false;
                            }

                            db.Promociones.Add(det);
                            db.SaveChanges();

                            if (det.Fecha == time && det.FechaVen >= time && det.Cliente == false)
                            {
                                var ProductoX = db.Productos.Where(a => a.Codigo == det.ItemCode && a.idCategoria == det.idCategoria && a.idListaPrecios == det.idListaPrecio).ToList();
                                foreach (var item2 in ProductoX)
                                {
                                    db.Entry(item2).State = EntityState.Modified;
                                    if (Promo.Moneda == "CRC")
                                    {
                                        item2.PrecioUnitario = det.PrecioFinal;
                                    }
                                    else
                                    {
                                        if (param.Pais == "C")
                                        {
                                            item2.PrecioUnitario = det.PrecioFinal * TipoCambio.TipoCambio;
                                        }
                                        else
                                        {
                                            item2.PrecioUnitario = det.PrecioFinal;
                                        }

                                    }

                                    item2.FechaActualizacion = DateTime.Now;
                                    db.SaveChanges();

                                }

                            }



                            i++;
                        }
                        else if (promocion.Clientes == null)
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
                var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == time).FirstOrDefault();
                Parametros param = db.Parametros.FirstOrDefault();
                if (Promo != null)
                {
                    db.Entry(Promo).State = EntityState.Modified;
                    Promo.idListaPrecio = promocion.idListaPrecio;
                    Promo.Nombre = promocion.Nombre;
                    Promo.Fecha = promocion.Fecha.Date;
                    Promo.FechaVencimiento = promocion.FechaVencimiento.Date;
                    Promo.FechaCreacion = DateTime.Now;
                    Promo.idUsuarioCreador = promocion.idUsuarioCreador;
                    Promo.Moneda = promocion.Moneda;

                    db.SaveChanges();



                    var Detalles = db.Promociones.Where(a => a.idEncabezado == Promo.id).ToList();


                    foreach (var item in Detalles)
                    {
                        var Producto = db.Productos.Where(a => a.Codigo == item.ItemCode && a.idCategoria == item.idCategoria && a.idListaPrecios == item.idListaPrecio).FirstOrDefault();
                        if(Producto != null)
                        {
                            db.Entry(Producto).State = EntityState.Modified;
                            if (Promo.Moneda == "CRC")
                            {
                                Producto.PrecioUnitario = item.PrecioAnterior;
                            }
                            else
                            {
                                if (param.Pais == "C")
                                {
                                    Producto.PrecioUnitario = item.PrecioAnterior * TipoCambio.TipoCambio;
                                }
                                else
                                {
                                    Producto.PrecioUnitario = item.PrecioAnterior;
                                }

                            }
                            Producto.FechaActualizacion = DateTime.Now;
                            db.SaveChanges();
                        }
                        db.Promociones.Remove(item);
                        db.SaveChanges();
                    }

                    var x = 0;

                    var ClientesP = db.ClientesPromociones.Where(a => a.idPromocion == Promo.id).ToList();

                    foreach (var item in ClientesP)
                    {
                        db.ClientesPromociones.Remove(item);
                        db.SaveChanges();
                    }
                    if (promocion.Clientes != null)
                    {
                        foreach (var item in promocion.Clientes)
                        {

                            var ClientePromocion = db.ClientesPromociones.Where(a => a.idPromocion == item.idPromocion && a.idCliente == item.idCliente).FirstOrDefault();
                            if (ClientePromocion == null)
                            {
                                var Objetos = new ClientesPromociones();
                                Objetos.idPromocion = Promo.id;
                                Objetos.idCliente = item.idCliente;


                                db.ClientesPromociones.Add(Objetos);
                                db.SaveChanges();





                                x++;
                            }
                            else
                            {
                                var Client = db.Clientes.Where(a => a.id == item.idCliente).FirstOrDefault();
                                throw new Exception("Ya el Cliente" + Client.Codigo + "-" + Client.Nombre + " esta asignado a la promocion #" + Promo.id);
                            }







                        }
                    }
                    var i = 0;
                    foreach (var item in promocion.Detalle)
                    {
                        var ClientePromo = db.ClientesPromociones.Where(a => a.idPromocion == Promo.id).FirstOrDefault();
                        var PromoVieja = db.Promociones.Where(a => a.ItemCode == item.ItemCode && a.idListaPrecio == item.idListaPrecio && a.idCategoria == item.idCategoria && a.FechaVen > time && a.Cliente == false).FirstOrDefault();
                        if (PromoVieja == null)
                        {
                            Promociones det = new Promociones();
                            det.idEncabezado = Promo.id;
                            det.ItemCode = item.ItemCode;
                            det.idCategoria = item.idCategoria;
                            det.idListaPrecio = item.idListaPrecio;
                            det.FechaVen = Promo.FechaVencimiento.Date;
                            det.Fecha = Promo.Fecha.Date;


                            if (ClientePromo != null)
                            {
                                det.Cliente = true;
                            }
                            else
                            {
                                det.Cliente = false;
                            }
                            det.Moneda = item.Moneda;
                            det.PrecioFinal = item.PrecioFinal;
                            det.PrecioAnterior = item.PrecioAnterior;

                            db.Promociones.Add(det);
                            db.SaveChanges();


                            if (det.Fecha <= time && det.FechaVen >= time && det.Cliente == false)
                            {
                                var ProductoX = db.Productos.Where(a => a.Codigo == det.ItemCode && a.idCategoria == det.idCategoria && a.idListaPrecios == det.idListaPrecio).ToList();
                                foreach (var item2 in ProductoX)
                                {
                                    db.Entry(item2).State = EntityState.Modified;
                                    if (Promo.Moneda == "CRC")
                                    {
                                        item2.PrecioUnitario = det.PrecioFinal;
                                    }
                                    else
                                    {
                                        if (param.Pais == "C")
                                        {
                                            item2.PrecioUnitario = det.PrecioFinal * TipoCambio.TipoCambio;
                                        }
                                        else
                                        {
                                            item2.PrecioUnitario = det.PrecioFinal;
                                        }

                                    }
                                    item2.FechaActualizacion = DateTime.Now;
                                    db.SaveChanges();

                                }

                            }
                            i++;
                        }
                        else if(promocion.Clientes == null)
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