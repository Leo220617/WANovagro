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
    public class AprovisionamientosController : ApiController
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
                var Aprovisionamiento = db.EncAprovisionamiento.Select(a => new
                {
                    a.id,
                    a.idCategoria,
                    a.idSubCategoria,
                    a.idUsuarioCreador,
                    a.Fecha,
                    a.Status,
                    a.Clasificacion,
                    a.IndicadorMenor,
                    a.IndicadorMayor,
                    a.FiltroSeleccionado,
                    a.FechaActualizacion,
                    Detalle = db.DetAprovisionamiento.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => ((filtro.pendientes == true ? a.Status == "P" : false) || (filtro.espera == true ? a.Status == "E" : false) || (filtro.contabilizado == true ? a.Status == "C" : false))
                && (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true)
                && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)
                && (filtro.Codigo1 > 0 ? a.idUsuarioCreador == filtro.Codigo1 : true)

                && (filtro.Codigo2 > 0 ? a.idCategoria == filtro.Codigo2 : true)

                && (filtro.Codigo3 > 0 ? a.idSubCategoria == filtro.Codigo3 : true)
                   && (!string.IsNullOrEmpty(filtro.Texto) ? a.Clasificacion.Contains(filtro.Texto) : true)
                ).ToList();



                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Aprovisionamiento);
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
        [Route("api/Aprovisionamientos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Aprovisionamiento = db.EncAprovisionamiento.Select(a => new
                {
                    a.id,
                    a.idCategoria,
                    a.idSubCategoria,
                    a.idUsuarioCreador,
                    a.Fecha,
                    a.Status,
                    a.Clasificacion,
                    a.IndicadorMenor,
                    a.IndicadorMayor,
                    a.FiltroSeleccionado,
                    a.FechaActualizacion,
                    Detalle = db.DetAprovisionamiento.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Aprovisionamiento);
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
        [Route("api/Aprovisionamientos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Aprovisionamientos aprovisionamiento)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                EncAprovisionamiento Aprovisionamiento = db.EncAprovisionamiento.Where(a => a.id == aprovisionamiento.id).FirstOrDefault();
                if (Aprovisionamiento == null)
                {
                    Aprovisionamiento = new EncAprovisionamiento();
                    Aprovisionamiento.idCategoria = aprovisionamiento.idCategoria;
                    Aprovisionamiento.idSubCategoria = aprovisionamiento.idSubCategoria;
                    Aprovisionamiento.idUsuarioCreador = aprovisionamiento.idUsuarioCreador;
                    Aprovisionamiento.Fecha = DateTime.Now;
                    Aprovisionamiento.FechaActualizacion = DateTime.Now;
                    Aprovisionamiento.Status = "P";
                    Aprovisionamiento.Clasificacion = aprovisionamiento.Clasificacion;
                    Aprovisionamiento.IndicadorMenor = aprovisionamiento.IndicadorMenor;
                    Aprovisionamiento.IndicadorMayor = aprovisionamiento.IndicadorMayor;
                    Aprovisionamiento.FiltroSeleccionado = aprovisionamiento.FiltroSeleccionado;
                    db.EncAprovisionamiento.Add(Aprovisionamiento);
                    db.SaveChanges();


                    foreach (var item in aprovisionamiento.Detalle)
                    {
                        DetAprovisionamiento det = new DetAprovisionamiento();
                        det.idEncabezado = Aprovisionamiento.id;
                        det.CodigoProducto = item.CodigoProducto;
                        det.NombreProducto = item.NombreProducto;
                        det.Bodega = item.Bodega;
                        det.Stock = item.Stock;
                        det.Pedido = item.Pedido;
                        det.CodProveedor = item.CodProveedor;
                        det.NombreProveedor = item.NombreProveedor;
                        det.UltPrecioCompra = item.UltPrecioCompra;
                        det.CostoPromedio = item.CostoPromedio;
                        det.PromedioVenta = item.PromedioVenta;
                        det.InventarioIdeal = item.InventarioIdeal;
                        det.IndicadorST = item.IndicadorST;
                        det.PedidoSugerido = item.PedidoSugerido;
                        det.Compra = item.Compra;
                        det.Chequeado = item.Chequeado;
                        det.StockTodas = item.StockTodas;
                        det.PromedioVentaTodas = item.PromedioVentaTodas;
                        det.IndicadorSTTodas = item.IndicadorSTTodas;
                        det.PrecioCompra = item.PrecioCompra;
                        det.Impuesto = item.Impuesto;
                        det.TotalImpuesto = item.TotalImpuesto;
                        det.TotalCompra = item.TotalCompra;

                        db.DetAprovisionamiento.Add(det);
                        db.SaveChanges();


                    }

                    t.Commit();

                    if (Aprovisionamiento.Status == "E")
                    {
                        var detallesAgrupados = aprovisionamiento.Detalle.Where(a => a.Compra > 0)
                        .GroupBy(d => d.CodProveedor)
                        .Select(grupo => new
                        {
                            CodProveedor = grupo.Key,
                            NombreProveedor = grupo.First().NombreProveedor,
                            Subtotal = grupo.Sum(d => d.TotalCompra - d.TotalImpuesto),
                            TotalImpuesto = grupo.Sum(d => d.TotalImpuesto),
                            TotalCompra = grupo.Sum(d => d.TotalCompra),
                            Detalles = grupo.ToList()
                        })
                        .ToList();
                        foreach (var grupo in detallesAgrupados)
                        {
                            EncCompras Compras = db.EncCompras.Where(a => a.idAprovisionamiento == Aprovisionamiento.id && a.CodProveedor == grupo.CodProveedor && a.ProcesadaSAP == false).FirstOrDefault();

                            if (Compras == null)
                            {
                                Compras = new EncCompras();
                                Compras.idAprovisionamiento = Aprovisionamiento.id;
                                Compras.idUsuarioCreador = Aprovisionamiento.idUsuarioCreador;
                                Compras.CodProveedor = grupo.CodProveedor;
                                Compras.NombreProveedor = grupo.NombreProveedor;
                                Compras.Fecha = DateTime.Now;
                                Compras.FechaVencimiento = DateTime.Now;
                                Compras.Subtotal = grupo.Subtotal;
                                Compras.TotalImpuesto = grupo.TotalImpuesto;
                                Compras.TotalCompra = grupo.TotalCompra;
                                Compras.Moneda = "CRC";
                                Compras.ProcesadaSAP = false;




                                db.SaveChanges();

                                foreach (var item in grupo.Detalles)
                                {
                                    DetCompras det = new DetCompras();
                                    det.idEncabezado = Compras.id;
                                    det.idDetAprovisionamiento = item.id;
                                    det.CodigoProducto = item.CodigoProducto;
                                    det.NombreProducto = item.NombreProducto;
                                    det.Bodega = item.Bodega;
                                    det.Cantidad = item.Compra;
                                    det.Impuesto = item.Impuesto;
                                    det.TotalImpuesto = item.TotalImpuesto;
                                    det.TotalLinea = item.TotalCompra;


                                    db.DetCompras.Add(det);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                db.Entry(Compras).State = EntityState.Modified;
                                Compras.idAprovisionamiento = Aprovisionamiento.id;
                                Compras.idUsuarioCreador = Aprovisionamiento.idUsuarioCreador;
                                Compras.CodProveedor = grupo.CodProveedor;
                                Compras.NombreProveedor = grupo.NombreProveedor;
                                Compras.Fecha = DateTime.Now;
                                Compras.FechaVencimiento = DateTime.Now;
                                Compras.Subtotal = grupo.Subtotal;
                                Compras.TotalImpuesto = grupo.TotalImpuesto;
                                Compras.TotalCompra = grupo.TotalCompra;
                                Compras.Moneda = "CRC";
                                Compras.ProcesadaSAP = false;



                                db.SaveChanges();

                                var DetallesCompras = db.DetCompras.Where(a => a.idEncabezado == Compras.id).ToList();

                                foreach (var item in DetallesCompras)
                                {
                                    db.DetCompras.Remove(item);
                                    db.SaveChanges();
                                }

                                foreach (var item in grupo.Detalles)
                                {
                                    DetCompras det = new DetCompras();
                                    det.idEncabezado = Compras.id;
                                    det.idDetAprovisionamiento = item.id;
                                    det.CodigoProducto = item.CodigoProducto;
                                    det.NombreProducto = item.NombreProducto;
                                    det.Bodega = item.Bodega;
                                    det.Cantidad = item.Compra;
                                    det.Impuesto = item.Impuesto;
                                    det.TotalImpuesto = item.TotalImpuesto;
                                    det.TotalLinea = item.TotalCompra;

                                    db.DetCompras.Add(det);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }


                }
                else
                {
                    throw new Exception("Ya existe un aprovisionamiento con este ID");
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

        [Route("api/Aprovisionamientos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Aprovisionamientos aprovisionamiento)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                EncAprovisionamiento Aprovisionamiento = db.EncAprovisionamiento.Where(a => a.id == aprovisionamiento.id).FirstOrDefault();
                if (Aprovisionamiento != null)
                {
                    db.Entry(Aprovisionamiento).State = EntityState.Modified;
                    Aprovisionamiento.idCategoria = aprovisionamiento.idCategoria;
                    Aprovisionamiento.idSubCategoria = aprovisionamiento.idSubCategoria;
                    Aprovisionamiento.idUsuarioCreador = aprovisionamiento.idUsuarioCreador;
                    Aprovisionamiento.FechaActualizacion = DateTime.Now;
                    Aprovisionamiento.Status = aprovisionamiento.Status;
                    Aprovisionamiento.Clasificacion = aprovisionamiento.Clasificacion;
                    Aprovisionamiento.IndicadorMenor = aprovisionamiento.IndicadorMenor;
                    Aprovisionamiento.IndicadorMayor = aprovisionamiento.IndicadorMayor;

                    db.SaveChanges();

                    var Detalles = db.DetAprovisionamiento.Where(a => a.idEncabezado == Aprovisionamiento.id).ToList();

                    foreach (var item in Detalles)
                    {
                        db.DetAprovisionamiento.Remove(item);
                        db.SaveChanges();
                    }
                    if (aprovisionamiento.Detalle != null)
                    {
                        var i = 0;
                        foreach (var item in aprovisionamiento.Detalle)
                        {
                            DetAprovisionamiento det = new DetAprovisionamiento();
                            det.idEncabezado = Aprovisionamiento.id;
                            det.CodigoProducto = item.CodigoProducto;
                            det.NombreProducto = item.NombreProducto;
                            det.Bodega = item.Bodega;
                            det.Stock = item.Stock;
                            det.Pedido = item.Pedido;
                            det.CodProveedor = item.CodProveedor;
                            det.NombreProveedor = item.NombreProveedor;
                            det.UltPrecioCompra = item.UltPrecioCompra;
                            det.CostoPromedio = item.CostoPromedio;
                            det.PromedioVenta = item.PromedioVenta;
                            det.InventarioIdeal = item.InventarioIdeal;
                            det.IndicadorST = item.IndicadorST;
                            det.PedidoSugerido = item.PedidoSugerido;
                            det.Compra = item.Compra;
                            det.Chequeado = item.Chequeado;
                            det.StockTodas = item.StockTodas;
                            det.PromedioVentaTodas = item.PromedioVentaTodas;
                            det.IndicadorSTTodas = item.IndicadorSTTodas;
                            det.PrecioCompra = item.PrecioCompra;
                            det.Impuesto = item.Impuesto;
                            det.TotalImpuesto = item.TotalImpuesto;
                            det.TotalCompra = item.TotalCompra;

                            db.DetAprovisionamiento.Add(det);
                            db.SaveChanges();
                            i++;


                        }
                    }

                    t.Commit();

                    if (Aprovisionamiento.Status == "E")
                    {
                        var detallesAgrupados = aprovisionamiento.Detalle.Where(a => a.Compra > 0)
                        .GroupBy(d => d.CodProveedor)
                        .Select(grupo => new
                        {
                            CodProveedor = grupo.Key,
                            NombreProveedor = grupo.First().NombreProveedor,
                            Subtotal = grupo.Sum(d => d.TotalCompra - d.TotalImpuesto),
                            TotalImpuesto = grupo.Sum(d => d.TotalImpuesto),
                            TotalCompra = grupo.Sum(d => d.TotalCompra),
                            Detalles = grupo.ToList()
                        })
                        .ToList();
                        foreach (var grupo in detallesAgrupados)
                        {
                            EncCompras Compras = db.EncCompras.Where(a => a.idAprovisionamiento == Aprovisionamiento.id && a.CodProveedor == grupo.CodProveedor && a.ProcesadaSAP == false).FirstOrDefault();

                            if(Compras == null)
                            {
                                Compras = new EncCompras();
                                Compras.idAprovisionamiento = Aprovisionamiento.id;
                                Compras.idUsuarioCreador = Aprovisionamiento.idUsuarioCreador;
                                Compras.CodProveedor = grupo.CodProveedor;
                                Compras.NombreProveedor = grupo.NombreProveedor;
                                Compras.Fecha = DateTime.Now;
                                Compras.FechaVencimiento = DateTime.Now;
                                Compras.Subtotal = grupo.Subtotal;
                                Compras.TotalImpuesto = grupo.TotalImpuesto;
                                Compras.TotalCompra = grupo.TotalCompra;
                                Compras.Moneda = "CRC";
                                Compras.ProcesadaSAP = false;
                  



                                db.SaveChanges();

                                foreach (var item in grupo.Detalles)
                                {
                                    DetCompras det = new DetCompras();
                                    det.idEncabezado = Compras.id;
                                    det.idDetAprovisionamiento = item.id;
                                    det.CodigoProducto = item.CodigoProducto;
                                    det.NombreProducto = item.NombreProducto;
                                    det.Bodega = item.Bodega;
                                    det.Cantidad = item.Compra;
                                    det.Impuesto = item.Impuesto;
                                    det.TotalImpuesto = item.TotalImpuesto;
                                    det.TotalLinea = item.TotalCompra;


                                    db.DetCompras.Add(det);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                db.Entry(Compras).State = EntityState.Modified;
                                Compras.idAprovisionamiento = Aprovisionamiento.id;
                                Compras.idUsuarioCreador = Aprovisionamiento.idUsuarioCreador;
                                Compras.CodProveedor = grupo.CodProveedor;
                                Compras.NombreProveedor = grupo.NombreProveedor;
                                Compras.Fecha = DateTime.Now;
                                Compras.FechaVencimiento = DateTime.Now;
                                Compras.Subtotal = grupo.Subtotal;
                                Compras.TotalImpuesto = grupo.TotalImpuesto;
                                Compras.TotalCompra = grupo.TotalCompra;
                                Compras.Moneda = "CRC";
                                Compras.ProcesadaSAP = false;



                                db.SaveChanges();

                                var DetallesCompras = db.DetCompras.Where(a => a.idEncabezado == Compras.id).ToList();

                                foreach (var item in DetallesCompras)
                                {
                                    db.DetCompras.Remove(item);
                                    db.SaveChanges();
                                }

                                foreach (var item in grupo.Detalles)
                                {
                                    DetCompras det = new DetCompras();
                                    det.idEncabezado = Compras.id;
                                    det.idDetAprovisionamiento = item.id;
                                    det.CodigoProducto = item.CodigoProducto;
                                    det.NombreProducto = item.NombreProducto;
                                    det.Bodega = item.Bodega;
                                    det.Cantidad = item.Compra;
                                    det.Impuesto = item.Impuesto;
                                    det.TotalImpuesto = item.TotalImpuesto;
                                    det.TotalLinea = item.TotalCompra;

                                    db.DetCompras.Add(det);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                }
                else
                {
                    throw new Exception("Ya existe un aprovisionamiento con este ID");
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