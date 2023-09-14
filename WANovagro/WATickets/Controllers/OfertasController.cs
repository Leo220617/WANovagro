using Newtonsoft.Json;
using SAPbobsCOM;
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

    public class OfertasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


        [Route("api/Ofertas/SincronizarSAP")]
        public HttpResponseMessage GetExtraeDatos([FromUri] int id) 
        {
            try
            {
                var Oferta = db.EncOferta.Where(a => a.id == id).FirstOrDefault();
                var param = db.Parametros.FirstOrDefault();
                if (Oferta.Tipo == "01")
                {
                    try
                    {
                        var ofertaSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations);

                        //Encabezado

                        ofertaSAP.DocObjectCode = BoObjectTypes.oQuotations;
                        ofertaSAP.CardCode = db.Clientes.Where(a => a.id == Oferta.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Oferta.idCliente).FirstOrDefault().Codigo;
                        ofertaSAP.DocCurrency = Oferta.Moneda == "CRC" ? param.MonedaLocal : Oferta.Moneda;
                        ofertaSAP.DocDate = Oferta.Fecha;
                        ofertaSAP.DocDueDate = Oferta.FechaVencimiento;
                        ofertaSAP.DocType = BoDocumentTypes.dDocument_Items;
                        ofertaSAP.NumAtCard = "APP PROF" + "" + Oferta.id;
                        ofertaSAP.Series = param.SerieProforma;
                        ofertaSAP.Comments = Oferta.Comentarios;
                        ofertaSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Oferta.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Oferta.idCondPago).FirstOrDefault().CodSAP);
                        ofertaSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Oferta.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Oferta.idVendedor).FirstOrDefault().CodSAP);


                        //Detalle
                        int z = 0;
                        var Detalle = db.DetOferta.Where(a => a.idEncabezado == id).ToList();
                        foreach (var item in Detalle)
                        {
                            ofertaSAP.Lines.SetCurrentLine(z);

                            ofertaSAP.Lines.Currency = Oferta.Moneda == "CRC" ? param.MonedaLocal : Oferta.Moneda;
                            ofertaSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                            ofertaSAP.Lines.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                            ofertaSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            var idImp = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idImpuesto;
                            ofertaSAP.Lines.TaxCode = item.idExoneracion > 0 ? "EX" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().Codigo;
                            ofertaSAP.Lines.TaxOnly = BoYesNoEnum.tNO;


                            ofertaSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            var idBod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                            ofertaSAP.Lines.WarehouseCode = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? "01" : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault().CodSAP;
                            ofertaSAP.Lines.Add();
                            z++;
                        }


                        var respuesta = ofertaSAP.Add();
                        if (respuesta == 0)
                        {
                            db.Entry(Oferta).State = EntityState.Modified;
                            Oferta.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                            Oferta.ProcesadaSAP = true;
                            db.SaveChanges();
                            Conexion.Desconectar();

                        }
                        else
                        {
                            var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = error;
                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                            be.Fecha = DateTime.Now;
                            be.JSON = JsonConvert.SerializeObject(ofertaSAP);
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();


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

                    }
                }
                else if (Oferta.Tipo == "02")
                {
                    try
                    {
                        var ofertaSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                        //Encabezado

                        ofertaSAP.DocObjectCode = BoObjectTypes.oOrders;
                        ofertaSAP.CardCode = db.Clientes.Where(a => a.id == Oferta.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Oferta.idCliente).FirstOrDefault().Codigo;
                        ofertaSAP.DocCurrency = Oferta.Moneda == "CRC" ? param.MonedaLocal : Oferta.Moneda;
                        ofertaSAP.DocDate = Oferta.Fecha;
                        ofertaSAP.DocDueDate = Oferta.FechaVencimiento;
                        ofertaSAP.DocType = BoDocumentTypes.dDocument_Items;
                        ofertaSAP.NumAtCard = "APP ORV" + "" + Oferta.id;
                        ofertaSAP.Series = param.SerieOrden;
                        ofertaSAP.Comments = Oferta.Comentarios;
                        ofertaSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Oferta.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Oferta.idCondPago).FirstOrDefault().CodSAP);
                       ofertaSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Oferta.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Oferta.idVendedor).FirstOrDefault().CodSAP);


                        //Detalle
                        int z = 0;
                        var Detalle = db.DetOferta.Where(a => a.idEncabezado == id).ToList();
                        foreach (var item in Detalle)
                        {
                            ofertaSAP.Lines.SetCurrentLine(z);

                            ofertaSAP.Lines.Currency = Oferta.Moneda == "CRC" ? param.MonedaLocal : Oferta.Moneda;
                            ofertaSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                            ofertaSAP.Lines.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                            ofertaSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                            var idImp = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idImpuesto;
                            ofertaSAP.Lines.TaxCode = item.idExoneracion > 0 ? "EX" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().Codigo;
                            ofertaSAP.Lines.TaxOnly = BoYesNoEnum.tNO;


                            ofertaSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                            var idBod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                            ofertaSAP.Lines.WarehouseCode = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? "01" : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault().CodSAP;
                            ofertaSAP.Lines.Add();
                            z++;
                        }


                        var respuesta = ofertaSAP.Add();
                        if (respuesta == 0)
                        {
                            db.Entry(Oferta).State = EntityState.Modified;
                            Oferta.DocEntry = Conexion.Company.GetNewObjectKey().ToString();

                            Oferta.ProcesadaSAP = true;
                            db.SaveChanges();
                            Conexion.Desconectar();

                        }
                        else
                        {
                            var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = error;
                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                            be.Fecha = DateTime.Now;
                            be.JSON = JsonConvert.SerializeObject(ofertaSAP);
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();


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

                    }
                }




                return Request.CreateResponse(System.Net.HttpStatusCode.OK);

            }
            catch (Exception ex)
            {

                ModelCliente db2 = new ModelCliente();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex);
                db2.BitacoraErrores.Add(be);
                db2.SaveChanges();

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

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
                var Ofertas = db.EncOferta.Select(a => new
                {
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
                    a.idCondPago,
                    a.TipoDocumento,
                    a.idVendedor,
                    a.Redondeo,
                    Detalle = db.DetOferta.Where(b => b.idEncabezado == a.id).ToList(),
                    Lotes = db.Lotes.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (!string.IsNullOrEmpty(filtro.Texto))
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

                if (!string.IsNullOrEmpty(filtro.Categoria))
                {
                    Ofertas = Ofertas.Where(a => a.Tipo == filtro.Categoria).ToList();
                }
                if (filtro.Codigo3 > 0) // esto por ser integer
                {
                    Ofertas = Ofertas.Where(a => a.idCondPago == filtro.Codigo3).ToList();
                }
                if (filtro.Codigo4 > 0) // esto por ser integer
                {
                    Ofertas = Ofertas.Where(a => a.idVendedor == filtro.Codigo4).ToList();
                }

                if (filtro.Procesado != null )
                {
                    Ofertas = Ofertas.Where(a => a.ProcesadaSAP == filtro.Procesado).ToList();
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
                var Oferta = db.EncOferta.Select(a => new
                {
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
                    a.idCondPago,
                    a.TipoDocumento,
                    a.idVendedor,
                    a.Redondeo,
                    Detalle = db.DetOferta.Where(b => b.idEncabezado == a.id).ToList(),
                    Lotes = db.Lotes.Where(b => b.idEncabezado == a.id).ToList()

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
                Parametros param = db.Parametros.FirstOrDefault();
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
                    Oferta.idCondPago = oferta.idCondPago;
                    Oferta.idVendedor = oferta.idVendedor;
                    Oferta.TipoDocumento = oferta.TipoDocumento;
                    Oferta.Redondeo = oferta.Redondeo;
                    db.EncOferta.Add(Oferta);
                    db.SaveChanges();





                    var i = 0;
                    foreach (var item in oferta.Detalle)
                    {
                        var itemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() != null ? db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo : "";
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
                        det.NomPro = item.NomPro;
                        db.DetOferta.Add(det);
                        db.SaveChanges();
                        i++;

                        if (Oferta.Tipo == "01")
                        {
                            
                            if (oferta.Lotes == null)
                            {
                                oferta.Lotes = new List<Lotes>();
                            }

                            foreach (var lote in oferta.Lotes.Where(a => a.ItemCode == itemCode))
                            {
                                Lotes Lote = new Lotes();
                                Lote.idEncabezado = Oferta.id;
                                Lote.Tipo = "P";
                                Lote.Serie = lote.Serie;
                                Lote.ItemCode = lote.ItemCode;
                                Lote.Cantidad = lote.Cantidad;
                                Lote.idDetalle = det.id;
                                Lote.Manufactura = lote.Manufactura;
                                db.Lotes.Add(Lote);
                                db.SaveChanges();
                            }
                        }
                        else
                        {

                             
                            if (oferta.Lotes == null)
                            {
                                oferta.Lotes = new List<Lotes>();
                            }
                            foreach (var lote in oferta.Lotes.Where(a => a.ItemCode == itemCode))
                            {
                                Lotes Lote = new Lotes();
                                Lote.idEncabezado = Oferta.id;
                                Lote.Tipo = "O";
                                Lote.Serie = lote.Serie;
                                Lote.ItemCode = lote.ItemCode;
                                Lote.Cantidad = lote.Cantidad;
                                Lote.Manufactura = lote.Manufactura;
                                Lote.idDetalle = det.id;
                                db.Lotes.Add(Lote);
                                db.SaveChanges();
                            }
                        }

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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, be);
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
                    Oferta.idCondPago = oferta.idCondPago;
                    Oferta.idVendedor = oferta.idVendedor;
                    Oferta.TipoDocumento = oferta.TipoDocumento;

                    //Oferta.CodSuc = oferta.CodSuc;
                    Oferta.Moneda = oferta.Moneda;
                    // Oferta.Status = oferta.Status;
                    Oferta.Redondeo = oferta.Redondeo;

                    db.SaveChanges();

                    if (Oferta.Tipo == "01")
                    {
                        var Lotes = db.Lotes.Where(a => a.idEncabezado == Oferta.id && a.Tipo == "P").ToList();
                        foreach (var lote in Lotes)
                        {
                            db.Lotes.Remove(lote);
                            db.SaveChanges();
                        }


                    }
                    else
                    {

                        var Lotes = db.Lotes.Where(a => a.idEncabezado == Oferta.id && a.Tipo == "O").ToList();
                        foreach (var lote in Lotes)
                        {
                            db.Lotes.Remove(lote);
                            db.SaveChanges();
                        }

                    }


                    var Detalles = db.DetOferta.Where(a => a.idEncabezado == Oferta.id).ToList();

                    foreach (var item in Detalles)
                    {
                        db.DetOferta.Remove(item);
                        db.SaveChanges();
                    }


                    var i = 0;
                    foreach (var item in oferta.Detalle)
                    {
                        var itemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() != null ? db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo : "";

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
                        det.NomPro = item.NomPro;
                        db.DetOferta.Add(det);
                        db.SaveChanges();
                        i++;
                        if (Oferta.Tipo == "01")
                        {

                            if (oferta.Lotes == null)
                            {
                                oferta.Lotes = new List<Lotes>();
                            }

                            foreach (var lote in oferta.Lotes.Where(a => a.ItemCode == itemCode))
                            {
                                Lotes Lote = new Lotes();
                                Lote.idEncabezado = Oferta.id;
                                Lote.Tipo = "P";
                                Lote.Serie = lote.Serie;
                                Lote.ItemCode = lote.ItemCode;
                                Lote.Cantidad = lote.Cantidad;
                                Lote.idDetalle = det.id;
                                db.Lotes.Add(Lote);
                                db.SaveChanges();
                            }
                        }
                        else
                        {


                            if (oferta.Lotes == null)
                            {
                                oferta.Lotes = new List<Lotes>();
                            }
                            foreach (var lote in oferta.Lotes.Where(a => a.ItemCode == itemCode))
                            {
                                Lotes Lote = new Lotes();
                                Lote.idEncabezado = Oferta.id;
                                Lote.Tipo = "O";
                                Lote.Serie = lote.Serie;
                                Lote.ItemCode = lote.ItemCode;
                                Lote.Cantidad = lote.Cantidad;
                                Lote.idDetalle = det.id;
                                db.Lotes.Add(Lote);
                                db.SaveChanges();
                            }
                        }
                    }


                    BitacoraMovimientos btm = new BitacoraMovimientos();
                    btm.idUsuario = oferta.idUsuarioCreador;
                    btm.Descripcion = "Se edito la oferta: " + Oferta.id + " del cliente con el id: " + oferta.idCliente;
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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, be);
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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, be);
            }
        }


    }
}