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
                    a.DocEntryPago,
                    a.ProcesadaSAP,

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

                if (filtro.Procesado != null && filtro.Activo) //recordar poner el filtro.activo en novapp
                {
                    Pagos = Pagos.Where(a => a.ProcesadaSAP == filtro.Procesado).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.CardCode)) // esto por ser string
                {
                    Pagos = Pagos.Where(a => a.Moneda == filtro.CardCode).ToList();
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
                    a.DocEntryPago,
                    a.ProcesadaSAP,
                    a.TotalInteres,

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
                    Pago.TotalInteres = pago.TotalInteres;
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
                        det.Interes = item.Interes;
                        db.DetPagos.Add(det);
                        db.SaveChanges();
                        i++;
                        var Factura = db.EncDocumentoCredito.Where(a => a.id == det.idEncDocumentoCredito).FirstOrDefault();
                        if (Factura == null)
                        {
                            throw new Exception("Factura " + det.idEncDocumentoCredito + " no se encuentra en las bases de datos");
                        }
                        db.Entry(Factura).State = EntityState.Modified;
                        Factura.Saldo -= det.Total;
                        if (Factura.Saldo <= 0)
                        {
                            Factura.Status = "C";
                        }
                        db.SaveChanges();


                    }

                    var Cliente = db.Clientes.Where(a => a.id == pago.idCliente).FirstOrDefault();
                    if (Cliente == null)
                    {
                        throw new Exception("Cliente " + pago.id + " no se encuentra en las bases de datos");

                    }
                    var Fecha = DateTime.Now.Date;
                    var TP = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                    db.Entry(Cliente).State = EntityState.Modified;
                    Cliente.Saldo -= pago.Moneda == "USD" ? (pago.TotalPagado * TP.TipoCambio) : pago.TotalPagado;
                    db.SaveChanges();

                    t.Commit();

                    //Generar parte de SAP
                    try
                    {
                        if (Pago != null)
                        {
                            var detalle = db.DetPagos.Where(a => a.idEncabezado == Pago.id).ToList();
                            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Pago.CodSuc).FirstOrDefault();

                            var pagoSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);

                            //Encabezado 
                            pagoSAP.DocDate = DateTime.Now;
                            pagoSAP.DueDate = Pago.FechaVencimiento;
                            pagoSAP.TaxDate = Pago.FechaContabilizacion;
                            pagoSAP.VatDate = DateTime.Now;
                            pagoSAP.CardCode = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault().Codigo;
                            pagoSAP.Remarks = "Abono procesado por NOVAPOS";
                            pagoSAP.DocCurrency = Pago.Moneda;
                            pagoSAP.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                            pagoSAP.CounterReference = "APP ABONO" + Pago.id;
                            var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;

                            pagoSAP.CashSum = Convert.ToDouble(Pago.TotalPagado);
                            pagoSAP.Series = Sucursal.SeriePago; //154; 161;
                            pagoSAP.CashAccount = Cuenta;

                            int z = 0;
                            foreach (var item in detalle)
                            {
                                pagoSAP.Invoices.SetCurrentLine(z);

                                pagoSAP.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_Invoice;
                                var Factura = db.EncDocumentoCredito.Where(a => a.id == item.idEncDocumentoCredito).FirstOrDefault();
                                if (Factura != null)
                                {
                                    pagoSAP.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);
                                    if (Pago.Moneda != "CRC")
                                    {

                                        pagoSAP.Invoices.AppliedFC = Convert.ToDouble(item.Total);
                                    }
                                    else
                                    {
                                        pagoSAP.Invoices.SumApplied = Convert.ToDouble(item.Total);

                                    }
                                }
                                else
                                {
                                    throw new Exception("Esta factura no existe");
                                }

                                pagoSAP.Invoices.Add();
                                z++;
                            }

                            var respuestaPago = pagoSAP.Add();
                            if (respuestaPago == 0)
                            {
                                db.Entry(Pago).State = EntityState.Modified;
                                Pago.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                Pago.ProcesadaSAP = true;
                                db.SaveChanges();
                            }
                            else
                            {
                                var error = "hubo un error en el pago " + Conexion.Company.GetLastErrorDescription();
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = error;
                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                be.Fecha = DateTime.Now;
                                be.JSON = JsonConvert.SerializeObject(pagoSAP);
                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                            }



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
                    Pago.TotalInteres = pagos.TotalInteres;
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
                        det.Interes = item.Interes;
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

        [HttpGet]
        [Route("api/Pagos/SincronizarSAP")]
        public HttpResponseMessage GetSincronizar([FromUri] int id)
        {
            try
            {
                var Pago = db.EncPagos.Where(a => a.id == id).FirstOrDefault();
                var param = db.Parametros.FirstOrDefault();

                if (Pago.ProcesadaSAP != true)
                {

                    var detalle = db.DetPagos.Where(a => a.idEncabezado == Pago.id).ToList();
                    var Sucursal = db.Sucursales.Where(a => a.CodSuc == Pago.CodSuc).FirstOrDefault();

                    var pagoSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);

                    //Encabezado 
                    pagoSAP.DocDate = DateTime.Now;
                    pagoSAP.DueDate = Pago.FechaVencimiento;
                    pagoSAP.TaxDate = Pago.FechaContabilizacion;
                    pagoSAP.VatDate = DateTime.Now;
                    pagoSAP.CardCode = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault().Codigo;
                    pagoSAP.Remarks = "Abono procesado por NOVAPOS";
                    pagoSAP.DocCurrency = Pago.Moneda;
                    pagoSAP.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                    pagoSAP.CounterReference = "APP ABONO" + Pago.id;
                    var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;

                    pagoSAP.CashSum = Convert.ToDouble(Pago.TotalPagado);
                    pagoSAP.Series = Sucursal.SeriePago; //154; 161;
                    pagoSAP.CashAccount = Cuenta;

                    int z = 0;
                    foreach (var item in detalle)
                    {
                        pagoSAP.Invoices.SetCurrentLine(z);

                        pagoSAP.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_Invoice;
                        var Factura = db.EncDocumentoCredito.Where(a => a.id == item.idEncDocumentoCredito).FirstOrDefault();
                        if (Factura != null)
                        {
                            pagoSAP.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);
                            if (Pago.Moneda != "CRC")
                            {

                                pagoSAP.Invoices.AppliedFC = Convert.ToDouble(item.Total);
                            }
                            else
                            {
                                pagoSAP.Invoices.SumApplied = Convert.ToDouble(item.Total);

                            }
                        }
                        else
                        {
                            throw new Exception("Esta factura no existe");
                        }
                        pagoSAP.Invoices.Add();
                        z++;

                    }

                    var respuestaPago = pagoSAP.Add();
                    if (respuestaPago == 0)
                    {
                        db.Entry(Pago).State = EntityState.Modified;
                        Pago.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                        Pago.ProcesadaSAP = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        var error = "hubo un error en el pago " + Conexion.Company.GetLastErrorDescription();
                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = error;
                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                        be.Fecha = DateTime.Now;
                        be.JSON = JsonConvert.SerializeObject(pagoSAP);
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                    }



                }

                else
                {
                    throw new Exception("Ya existe un Pago con este ID");
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

        [HttpGet]
        [Route("api/Pagos/SincronizarSAPMasivo")]
        public HttpResponseMessage GetMasivo()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();

                var PagosSP = db.EncPagos.Where(a => a.ProcesadaSAP == false).ToList();

                foreach (var item2 in PagosSP)
                {
                    var Pago = db.EncPagos.Where(a => a.id == item2.id).FirstOrDefault();


                    if (Pago.ProcesadaSAP != true)
                    {

                        var detalle = db.DetPagos.Where(a => a.idEncabezado == Pago.id).ToList();
                        var Sucursal = db.Sucursales.Where(a => a.CodSuc == Pago.CodSuc).FirstOrDefault();

                        var pagoSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);

                        //Encabezado 
                        pagoSAP.DocDate = DateTime.Now;
                        pagoSAP.DueDate = Pago.FechaVencimiento;
                        pagoSAP.TaxDate = Pago.FechaContabilizacion;
                        pagoSAP.VatDate = DateTime.Now;
                        pagoSAP.CardCode = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault().Codigo;
                        pagoSAP.Remarks = "Abono procesado por NOVAPOS";
                        pagoSAP.DocCurrency = Pago.Moneda;
                        pagoSAP.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                        pagoSAP.CounterReference = "APP ABONO" + Pago.id;
                        var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;

                        pagoSAP.CashSum = Convert.ToDouble(Pago.TotalPagado);
                        pagoSAP.Series = Sucursal.SeriePago; //154; 161;
                        pagoSAP.CashAccount = Cuenta;

                        int z = 0;
                        foreach (var item in detalle)
                        {
                            pagoSAP.Invoices.SetCurrentLine(z);

                            pagoSAP.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_Invoice;
                            var Factura = db.EncDocumentoCredito.Where(a => a.id == item.idEncDocumentoCredito).FirstOrDefault();
                            if (Factura != null)
                            {
                                pagoSAP.Invoices.DocEntry = Convert.ToInt32(Factura.DocEntry);
                                if (Pago.Moneda != "CRC")
                                {

                                    pagoSAP.Invoices.AppliedFC = Convert.ToDouble(item.Total);
                                }
                                else
                                {
                                    pagoSAP.Invoices.SumApplied = Convert.ToDouble(item.Total);

                                }

                            }
                          
                            else
                            {
                                throw new Exception("Esta factura no existe");
                            }
                            pagoSAP.Invoices.Add();
                            z++;


                        }

                        var respuestaPago = pagoSAP.Add();
                        if (respuestaPago == 0)
                        {
                            db.Entry(Pago).State = EntityState.Modified;
                            Pago.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                            Pago.ProcesadaSAP = true;
                            db.SaveChanges();
                        }
                        else
                        {
                            var error = "hubo un error en el pago " + Conexion.Company.GetLastErrorDescription();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = error;
                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                            be.Fecha = DateTime.Now;
                            be.JSON = JsonConvert.SerializeObject(pagoSAP);
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }



                    }

                    else
                    {
                        throw new Exception("Ya existe un Pago con este ID");
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
