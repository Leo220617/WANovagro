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

    public class OfertasController: ApiController
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
                var Ofertas = db.EncOferta.Select(a => new {
                    a.id,
                    a.idCliente,
                    a.idUsuarioCreador,
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
                    a.Tipo,
                    a.BaseEntry,
                    a.DocEntry,
                    a.ProcesadaSAP,
                    Detalle = db.DetOferta.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if(!string.IsNullOrEmpty(filtro.Texto))
                {
                    Ofertas = Ofertas.Where(a => a.CodSuc == filtro.Texto).ToList();
                }

                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Ofertas = Ofertas.Where(a => a.idCliente == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Ofertas = Ofertas.Where(a => a.idUsuarioCreador == filtro.Codigo2).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.ItemCode)) // esto por ser string
                {
                    Ofertas = Ofertas.Where(a => a.Status == filtro.ItemCode).ToList();
                }

                if(!string.IsNullOrEmpty(filtro.Categoria))
                {
                    Ofertas = Ofertas.Where(a => a.Tipo == filtro.Categoria).ToList();
                }



                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Ofertas);
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

        [Route("api/Ofertas/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Oferta = db.EncOferta.Select(a => new {
                    a.id,
                    a.idCliente,
                    a.idUsuarioCreador,
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
                    a.Tipo,
                    a.BaseEntry,
                    a.DocEntry,
                    a.ProcesadaSAP,
                    Detalle = db.DetOferta.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Oferta);
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

        [Route("api/Ofertas/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Ofertas oferta)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                EncOferta Oferta = db.EncOferta.Where(a => a.id == oferta.id).FirstOrDefault();
                if (Oferta == null)
                {
                    Oferta = new EncOferta();
                    Oferta.idCliente = oferta.idCliente;
                    Oferta.idUsuarioCreador = oferta.idUsuarioCreador;
                    Oferta.Fecha = DateTime.Now;
                    Oferta.FechaVencimiento = oferta.FechaVencimiento;
                    Oferta.Comentarios = oferta.Comentarios;
                    Oferta.Subtotal = oferta.Subtotal;
                    Oferta.TotalImpuestos = oferta.TotalImpuestos;
                    Oferta.TotalDescuento = oferta.TotalDescuento;
                    Oferta.TotalCompra = oferta.TotalCompra;
                    Oferta.PorDescto = oferta.PorDescto;
                    Oferta.Status = "0";
                    Oferta.CodSuc = oferta.CodSuc;
                    Oferta.Moneda = oferta.Moneda;
                    Oferta.Tipo = oferta.Tipo;
                    Oferta.BaseEntry = oferta.BaseEntry;
                    Oferta.DocEntry = "";
                    Oferta.ProcesadaSAP = false;
                    // 0 is open, 1 is closed

                    db.EncOferta.Add(Oferta);
                    db.SaveChanges();

                    var i = 0;
                    foreach(var item in oferta.Detalle)
                    {
                        DetOferta det = new DetOferta();
                        det.idEncabezado = Oferta.id;
                        det.idProducto = item.idProducto;
                        det.NumLinea = i;
                        det.PorDescto = item.PorDescto;
                        det.PrecioUnitario = item.PrecioUnitario;
                        det.TotalImpuesto = item.TotalImpuesto;
                        det.Cantidad = item.Cantidad;
                        det.Descuento = item.Descuento;
                        det.TotalLinea = item.TotalLinea;//((det.PrecioUnitario * det.Cantidad) - det.Descuento) + det.TotalImpuesto;
                        det.Cabys = item.Cabys;
                        det.idExoneracion = item.idExoneracion;
                        db.DetOferta.Add(det);
                        db.SaveChanges();
                        i++;
                    }


                    BitacoraMovimientos btm = new BitacoraMovimientos();
                    btm.idUsuario = oferta.idUsuarioCreador;
                    btm.Descripcion = "Se crea una oferta para el cliente con el id: " + oferta.idCliente;
                    btm.Fecha = DateTime.Now;
                    btm.Metodo = "Insercion de Oferta";
                    db.BitacoraMovimientos.Add(btm);
                    db.SaveChanges();
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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }


        [Route("api/Ofertas/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Ofertas oferta)
        {
            var t = db.Database.BeginTransaction();
            try
            {
                EncOferta Oferta = db.EncOferta.Where(a => a.id == oferta.id).FirstOrDefault();
                if (Oferta != null)
                {
                    db.Entry(Oferta).State = EntityState.Modified;
                    Oferta.idCliente = oferta.idCliente;
                    Oferta.idUsuarioCreador = oferta.idUsuarioCreador;
                    Oferta.Fecha = DateTime.Now;
                    Oferta.FechaVencimiento = oferta.FechaVencimiento;
                    Oferta.Comentarios = oferta.Comentarios;
                    Oferta.Subtotal = oferta.Subtotal;
                    Oferta.TotalImpuestos = oferta.TotalImpuestos;
                    Oferta.TotalDescuento = oferta.TotalDescuento;
                    Oferta.TotalCompra = oferta.TotalCompra;
                    Oferta.PorDescto = oferta.PorDescto;
                   
                    //Oferta.CodSuc = oferta.CodSuc;
                    Oferta.Moneda = oferta.Moneda;
                    // Oferta.Status = oferta.Status;


                    db.SaveChanges();

                    var Detalles = db.DetOferta.Where(a => a.idEncabezado == Oferta.id).ToList();

                    foreach(var item in Detalles)
                    {
                        db.DetOferta.Remove(item);
                        db.SaveChanges();
                    }


                    var i = 0;
                    foreach (var item in oferta.Detalle)
                    {
                        DetOferta det = new DetOferta();
                        det.idEncabezado = Oferta.id;
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
                        db.DetOferta.Add(det);
                        db.SaveChanges();
                        i++;
                    }


                    BitacoraMovimientos btm = new BitacoraMovimientos();
                    btm.idUsuario = oferta.idUsuarioCreador;
                    btm.Descripcion = "Se edito la oferta: "+Oferta.id+" del cliente con el id: " + oferta.idCliente;
                    btm.Fecha = DateTime.Now;
                    btm.Metodo = "Edicion de Oferta";
                    db.BitacoraMovimientos.Add(btm);
                    db.SaveChanges();
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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/Ofertas/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                var Oferta = db.EncOferta.Where(a => a.id == id).FirstOrDefault();
                if (Oferta != null)
                {
                    db.Entry(Oferta).State = EntityState.Modified;


                    if (Oferta.Status == "0")
                    {

                        Oferta.Status = "1";

                    }
                    else
                    {

                        Oferta.Status = "0";

                    }




                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("No existe una oferta con este ID");
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