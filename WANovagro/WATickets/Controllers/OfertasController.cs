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
                    Detalle = db.DetOferta.Where(b => b.idEncabezado == a.id).ToList()

                }).ToList(); //Traemos el listado de productos

                

                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Ofertas = Ofertas.Where(a => a.idCliente == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Ofertas = Ofertas.Where(a => a.idUsuarioCreador == filtro.Codigo2).ToList();
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
                        det.TotalLinea = ((det.PrecioUnitario * det.Cantidad) - det.Descuento) + det.TotalImpuesto;
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
                }
                else
                {
                    throw new Exception("Ya existe una oferta con este ID");
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


        [Route("api/Ofertas/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Ofertas oferta)
        {
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
                }
                else
                {
                    throw new Exception("NO existe una oferta con este ID");
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