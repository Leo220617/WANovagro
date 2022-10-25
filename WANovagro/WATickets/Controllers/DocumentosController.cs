
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

    public class DocumentosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var time = DateTime.Now; // 01-01-0001
                if(filtro.FechaFinal != time)
                {
                    filtro.FechaInicial = filtro.FechaInicial.Date;
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }
                var Documentos = db.EncDocumento.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.idUsuarioCreador,
                    a.idCaja,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.Comentarios,
                    a.Subtotal,
                    a.TotalImpuestos,
                    a.TotalDescuento,
                    a.TotalCompra,
                    a.PorDescto,
                    a.Status,
                    a.CodSuc,
                    a.Moneda,
                    a.TipoDocumento,
                    a.BaseEntry,
                    MetodosPagos = db.MetodosPagos.Where(b => b.idEncabezado == a.id).ToList(),
                    Detalle = db.DetDocumento.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Documentos = Documentos.Where(a => a.CodSuc == filtro.Texto).ToList();
                }

                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Documentos = Documentos.Where(a => a.idCliente == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Documentos = Documentos.Where(a => a.idUsuarioCreador == filtro.Codigo2).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.ItemCode)) // esto por ser string
                {
                    Documentos = Documentos.Where(a => a.Status == filtro.ItemCode).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.CardCode)) // esto por ser string
                {
                    Documentos = Documentos.Where(a => a.TipoDocumento == filtro.CardCode).ToList();
                }
                
                
                if(filtro.Codigo3 > 0)
                {
                    Documentos = Documentos.Where(a => a.idCaja == filtro.Codigo3).ToList();

                }



                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Documentos);
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

        [Route("api/Documentos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Documento = db.EncDocumento.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.idUsuarioCreador,
                    a.idCaja,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.Comentarios,
                    a.Subtotal,
                    a.TotalImpuestos,
                    a.TotalDescuento,
                    a.TotalCompra,
                    a.PorDescto,
                    a.Status,
                    a.CodSuc,
                    a.Moneda,
                    a.TipoDocumento,
                    a.BaseEntry,
                    MetodosPagos = db.MetodosPagos.Where(b => b.idEncabezado == a.id).ToList(),
                    Detalle = db.DetDocumento.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Documento);
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

        [Route("api/Documentos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Documentos documento)
        {
            try
            {
                EncDocumento Documento = db.EncDocumento.Where(a => a.id == documento.id).FirstOrDefault();
                if (Documento == null)
                {
                    Documento = new EncDocumento();
                    Documento.idCliente = documento.idCliente;
                    Documento.idUsuarioCreador = documento.idUsuarioCreador;
                    Documento.Fecha = DateTime.Now;
                    Documento.FechaVencimiento = DateTime.Now;//documento.FechaVencimiento;
                    Documento.Comentarios = documento.Comentarios;
                    Documento.Subtotal = documento.Subtotal;
                    Documento.TotalImpuestos = documento.TotalImpuestos;
                    Documento.TotalDescuento = documento.TotalDescuento;
                    Documento.TotalCompra = documento.TotalCompra;
                    Documento.PorDescto = documento.PorDescto;
                    Documento.CodSuc = documento.CodSuc;
                    Documento.Moneda = documento.Moneda;
                    Documento.TipoDocumento = documento.TipoDocumento;
                    Documento.Status = "0";
                    Documento.idCaja = documento.idCaja;
                    Documento.BaseEntry = documento.BaseEntry;

                    // 0 is open, 1 is closed

                    db.EncDocumento.Add(Documento);
                    db.SaveChanges();

                    var i = 0;
                    foreach (var item in documento.Detalle)
                    {
                        DetDocumento det = new DetDocumento();
                        det.idEncabezado = Documento.id;
                        det.idProducto = item.idProducto;
                        det.NumLinea = i;
                        det.PorDescto = item.PorDescto;
                        det.PrecioUnitario = item.PrecioUnitario;
                        det.TotalImpuesto = item.TotalImpuesto;
                        det.Cantidad = item.Cantidad;
                        det.Descuento = item.Descuento;
                        det.TotalLinea = item.TotalLinea; //((det.PrecioUnitario * det.Cantidad) - det.Descuento) + det.TotalImpuesto;
                        det.Cabys = item.Cabys;
                        det.idExoneracion = item.idExoneracion;
                        db.DetDocumento.Add(det);
                        db.SaveChanges();
                        i++;

                        var prod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                        if(prod != null)
                        {
                           
                            db.Entry(prod).State = EntityState.Modified;
                            if(Documento.TipoDocumento == "01" || Documento.TipoDocumento == "04")
                            {
                                prod.Stock -= item.Cantidad;

                            }
                            else 
                            {
                                prod.Stock += item.Cantidad;
                            }
                            db.SaveChanges();
                        }

                    }
                    var time = DateTime.Now.Date;
                    var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == time && a.idCaja == documento.idCaja && a.idUsuario == Documento.idUsuarioCreador).FirstOrDefault();
                    
                    foreach (var item in documento.MetodosPagos)
                    {
                        MetodosPagos MetodosPagos = new MetodosPagos();
                        MetodosPagos.idEncabezado = Documento.id;
                        MetodosPagos.Monto = item.Monto;
                        MetodosPagos.BIN = item.BIN;
                        MetodosPagos.NumCheque = item.NumCheque;
                        MetodosPagos.NumReferencia = item.NumReferencia;
                        MetodosPagos.Metodo = item.Metodo;
                        db.MetodosPagos.Add(MetodosPagos);
                        db.SaveChanges();
                        if (CierreCaja != null)
                        {
                            db.Entry(CierreCaja).State = EntityState.Modified;
                            if (Documento.Moneda == "CRC")
                            {
                                switch (item.Metodo)
                                {
                                    case "Efectivo":
                                        {
                                            CierreCaja.EfectivoColones += item.Monto;
                                            CierreCaja.TotalVendidoColones += item.Monto;
                                            break;
                                        }
                                    case "Tarjeta":
                                        {
                                            CierreCaja.TarjetasColones += item.Monto;
                                            CierreCaja.TotalVendidoColones += item.Monto;

                                            break;
                                        }
                                    case "Cheque":
                                        {
                                            CierreCaja.ChequesColones += item.Monto;
                                            CierreCaja.TotalVendidoColones += item.Monto;

                                            break;
                                        }
                                    
                                    default:
                                        {
                                            CierreCaja.OtrosMediosColones += item.Monto;
                                            CierreCaja.TotalVendidoColones += item.Monto;

                                            break;
                                        }

                                }

                            }
                            else
                            {
                                switch (item.Metodo)
                                {
                                    case "Efectivo":
                                        {
                                            CierreCaja.EfectivoFC += item.Monto;
                                            CierreCaja.TotalVendidoFC += item.Monto;
                                            break;
                                        }
                                    case "Tarjeta":
                                        {
                                            CierreCaja.TarjetasFC += item.Monto;
                                            CierreCaja.TotalVendidoFC += item.Monto;

                                            break;
                                        }
                                    case "Cheque":
                                        {
                                            CierreCaja.ChequesFC += item.Monto;
                                            CierreCaja.TotalVendidoFC += item.Monto;

                                            break;
                                        }

                                    default:
                                        {
                                            CierreCaja.OtrosMediosFC += item.Monto;
                                            CierreCaja.TotalVendidoFC += item.Monto;

                                            break;
                                        }

                                }
                            }
                            db.SaveChanges();
                        }


                    }

                    BitacoraMovimientos btm = new BitacoraMovimientos();
                    btm.idUsuario = documento.idUsuarioCreador;
                    btm.Descripcion = "Se crea un documento para el cliente con el id: " + documento.idCliente;
                    btm.Fecha = DateTime.Now;
                    btm.Metodo = "Insercion de Documento";
                    db.BitacoraMovimientos.Add(btm);
                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("Ya existe un documento con este ID");
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


        [Route("api/Documentos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Documentos documento)
        {
            try
            {
                EncDocumento Documento = db.EncDocumento.Where(a => a.id == documento.id).FirstOrDefault();
                if (Documento != null)
                {
                    db.Entry(Documento).State = EntityState.Modified;
                    Documento.idCliente = documento.idCliente;
                    Documento.idUsuarioCreador = documento.idUsuarioCreador;
                    Documento.Fecha = DateTime.Now;
                    Documento.FechaVencimiento = DateTime.Now;//documento.FechaVencimiento;

                    Documento.Comentarios = documento.Comentarios;
                    Documento.Subtotal = documento.Subtotal;
                    Documento.TotalImpuestos = documento.TotalImpuestos;
                    Documento.TotalDescuento = documento.TotalDescuento;
                    Documento.TotalCompra = documento.TotalCompra;
                    Documento.PorDescto = documento.PorDescto;
                    Documento.Moneda = documento.Moneda;
                    Documento.TipoDocumento = documento.TipoDocumento;
              
                    // Documento.Status = documetno.Status;


                    db.SaveChanges();

                    var Detalles = db.DetDocumento.Where(a => a.idEncabezado == Documento.id).ToList();

                    foreach (var item in Detalles)
                    {
                        var prod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                        if (prod != null)
                        {

                            db.Entry(prod).State = EntityState.Modified;
                            if (Documento.TipoDocumento == "01" || Documento.TipoDocumento == "04")
                            {
                                prod.Stock += item.Cantidad;

                            }
                            else
                            {
                                prod.Stock -= item.Cantidad;
                            }
                            db.SaveChanges();
                        }

                        db.DetDocumento.Remove(item);
                        db.SaveChanges();
                    }


                    var i = 0;
                    foreach (var item in documento.Detalle)
                    {
                        DetDocumento det = new DetDocumento();
                        det.idEncabezado = Documento.id;
                        det.idProducto = item.idProducto;
                        det.NumLinea = i;
                        det.PorDescto = item.PorDescto;
                        det.PrecioUnitario = item.PrecioUnitario;
                        det.TotalImpuesto = item.TotalImpuesto;
                        det.Cantidad = item.Cantidad;
                        det.Descuento = item.Descuento;
                        det.Cabys = item.Cabys;
                        det.TotalLinea = item.TotalLinea;//((det.PrecioUnitario * det.Cantidad) - det.Descuento) + det.TotalImpuesto;
                        det.idExoneracion = item.idExoneracion;
                        db.DetDocumento.Add(det);
                        db.SaveChanges();
                        i++;

                        var prod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                        if (prod != null)
                        {

                            db.Entry(prod).State = EntityState.Modified;
                            if (Documento.TipoDocumento == "01" || Documento.TipoDocumento == "04")
                            {
                                prod.Stock -= item.Cantidad;

                            }
                            else
                            {
                                prod.Stock += item.Cantidad;
                            }
                            db.SaveChanges();
                        }
                    }

                    var MetodosPagos2 = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id).ToList();
                    var time = DateTime.Now.Date;
                    var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == time && a.idCaja == documento.idCaja).FirstOrDefault();

                    foreach (var item in MetodosPagos2)
                    {
                        if (CierreCaja != null)
                        {
                            db.Entry(CierreCaja).State = EntityState.Modified;
                            if (Documento.Moneda == "CRC")
                            {
                                switch (item.Metodo)
                                {
                                    case "Efectivo":
                                        {
                                            CierreCaja.EfectivoColones -= item.Monto;
                                            CierreCaja.TotalVendidoColones -= item.Monto;
                                            break;
                                        }
                                    case "Tarjeta":
                                        {
                                            CierreCaja.TarjetasColones -= item.Monto;
                                            CierreCaja.TotalVendidoColones -= item.Monto;

                                            break;
                                        }
                                    case "Cheque":
                                        {
                                            CierreCaja.ChequesColones -= item.Monto;
                                            CierreCaja.TotalVendidoColones -= item.Monto;

                                            break;
                                        }

                                    default:
                                        {
                                            CierreCaja.OtrosMediosColones -= item.Monto;
                                            CierreCaja.TotalVendidoColones -= item.Monto;

                                            break;
                                        }

                                }

                            }
                            else
                            {
                                switch (item.Metodo)
                                {
                                    case "Efectivo":
                                        {
                                            CierreCaja.EfectivoFC -= item.Monto;
                                            CierreCaja.TotalVendidoFC -= item.Monto;
                                            break;
                                        }
                                    case "Tarjeta":
                                        {
                                            CierreCaja.TarjetasFC -= item.Monto;
                                            CierreCaja.TotalVendidoFC -= item.Monto;

                                            break;
                                        }
                                    case "Cheque":
                                        {
                                            CierreCaja.ChequesFC -= item.Monto;
                                            CierreCaja.TotalVendidoFC -= item.Monto;

                                            break;
                                        }

                                    default:
                                        {
                                            CierreCaja.OtrosMediosFC -= item.Monto;
                                            CierreCaja.TotalVendidoFC -= item.Monto;

                                            break;
                                        }

                                }
                            }
                            db.SaveChanges();
                        }
                        db.MetodosPagos.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (var item in documento.MetodosPagos)
                    {
                        MetodosPagos MetodosPagos = new MetodosPagos();
                        MetodosPagos.idEncabezado = Documento.id;
                        MetodosPagos.Monto = item.Monto;
                        MetodosPagos.BIN = item.BIN;
                        MetodosPagos.NumCheque = item.NumCheque;
                        MetodosPagos.NumReferencia = item.NumReferencia;
                        MetodosPagos.Metodo = item.Metodo;

                        db.MetodosPagos.Add(MetodosPagos);
                        db.SaveChanges();

                        if (CierreCaja != null)
                        {
                            db.Entry(CierreCaja).State = EntityState.Modified;
                            if (Documento.Moneda == "CRC")
                            {
                                switch (item.Metodo)
                                {
                                    case "Efectivo":
                                        {
                                            CierreCaja.EfectivoColones += item.Monto;
                                            CierreCaja.TotalVendidoColones += item.Monto;
                                            break;
                                        }
                                    case "Tarjeta":
                                        {
                                            CierreCaja.TarjetasColones += item.Monto;
                                            CierreCaja.TotalVendidoColones += item.Monto;

                                            break;
                                        }
                                    case "Cheque":
                                        {
                                            CierreCaja.ChequesColones += item.Monto;
                                            CierreCaja.TotalVendidoColones += item.Monto;

                                            break;
                                        }

                                    default:
                                        {
                                            CierreCaja.OtrosMediosColones += item.Monto;
                                            CierreCaja.TotalVendidoColones += item.Monto;

                                            break;
                                        }

                                }

                            }
                            else
                            {
                                switch (item.Metodo)
                                {
                                    case "Efectivo":
                                        {
                                            CierreCaja.EfectivoFC += item.Monto;
                                            CierreCaja.TotalVendidoFC += item.Monto;
                                            break;
                                        }
                                    case "Tarjeta":
                                        {
                                            CierreCaja.TarjetasFC += item.Monto;
                                            CierreCaja.TotalVendidoFC += item.Monto;

                                            break;
                                        }
                                    case "Cheque":
                                        {
                                            CierreCaja.ChequesFC += item.Monto;
                                            CierreCaja.TotalVendidoFC += item.Monto;

                                            break;
                                        }

                                    default:
                                        {
                                            CierreCaja.OtrosMediosFC += item.Monto;
                                            CierreCaja.TotalVendidoFC += item.Monto;

                                            break;
                                        }

                                }
                            }
                            db.SaveChanges();
                        }

                    }


                    BitacoraMovimientos btm = new BitacoraMovimientos();
                    btm.idUsuario = documento.idUsuarioCreador;
                    btm.Descripcion = "Se edito el documento: " + Documento.id + " del cliente con el id: " + documento.idCliente;
                    btm.Fecha = DateTime.Now;
                    btm.Metodo = "Edicion de Documento";
                    db.BitacoraMovimientos.Add(btm);
                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("NO existe un documento con este ID");
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