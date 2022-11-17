﻿using Newtonsoft.Json;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models.APIS;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class DocumentosMasivosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


        [HttpGet]
        public HttpResponseMessage GetMasivo()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();

                var DocumentosSP = db.EncDocumento.Where(a => a.ProcesadaSAP == false).ToList();

                foreach (var item2 in DocumentosSP)
                {
                    EncDocumento Documento = db.EncDocumento.Where(a => a.id == item2.id).FirstOrDefault();


                    //Insercion e itento a SAP
                    if (Documento.ProcesadaSAP == false)
                    {
                        try
                        {
                            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Documento.CodSuc).FirstOrDefault();
                            var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                            //Encabezado

                            documentoSAP.DocObjectCode = BoObjectTypes.oInvoices;
                            documentoSAP.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                            documentoSAP.DocCurrency = Documento.Moneda == "CRC" ? "CRC" : Documento.Moneda;
                            documentoSAP.DocDate = Documento.Fecha;
                            documentoSAP.DocDueDate = Documento.FechaVencimiento;
                            documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
                            documentoSAP.NumAtCard = "Creado en NOVAPOS" + " " + Documento.id;
                            documentoSAP.Comments = Documento.Comentarios;
                            documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().CodSAP);
                            var CondPago = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().Nombre;
                            documentoSAP.Series = CondPago.ToLower().Contains("contado") ? Sucursal.SerieFECO : Sucursal.SerieFECR;  //4;  //param.SerieProforma; //Quemada


                            documentoSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault().CodSAP);


                            //Detalle
                            var Detalle = db.DetDocumento.Where(a => a.idEncabezado == item2.id).ToList();
                            int z = 0;

                            foreach (var item in Detalle)
                            {
                                documentoSAP.Lines.SetCurrentLine(z);

                                documentoSAP.Lines.Currency = Documento.Moneda == "CRC" ? "CRC" : Documento.Moneda;
                                documentoSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                                documentoSAP.Lines.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                                documentoSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                var idImp = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idImpuesto;
                                documentoSAP.Lines.TaxCode = item.idExoneracion > 0 ? "EX" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().Codigo;
                                documentoSAP.Lines.TaxOnly = BoYesNoEnum.tNO;


                                documentoSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                                var idBod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                                documentoSAP.Lines.WarehouseCode = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? "01" : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault().CodSAP;

                                documentoSAP.Lines.Add();
                                z++;
                            }


                            var respuesta = documentoSAP.Add();
                            if (respuesta == 0) //se creo exitorsamente 
                            {
                                db.Entry(Documento).State = EntityState.Modified;
                                Documento.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                                Documento.ProcesadaSAP = true;
                                db.SaveChanges();

                                //Procesamos el pago
                                try
                                {
                                    var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                    pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                    pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                                    pagoProcesado.DocDate = DateTime.Now;
                                    pagoProcesado.DueDate = DateTime.Now;
                                    pagoProcesado.TaxDate = DateTime.Now;
                                    pagoProcesado.VatDate = DateTime.Now;
                                    pagoProcesado.Remarks = "pago procesado por novapos";
                                    pagoProcesado.DocCurrency = Documento.Moneda;
                                    pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                    //ligar la factura con el pago 

                                    pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                    pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);
                                    pagoProcesado.Invoices.SumApplied = Convert.ToDouble(Documento.TotalCompra);

                                    //meter los metodos de pago

                                    var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id).ToList();


                                    var MontoOtros = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Count() == null || db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Count() == 0 ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Sum(a => a.Monto);

                                    foreach (var item in MetodosPagos)
                                    {
                                        switch (item.Metodo)
                                        {
                                            case "Efectivo":
                                                {
                                                    pagoProcesado.CashAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.CashSum = Convert.ToDouble(item.Monto + MontoOtros);

                                                    break;
                                                }
                                            case "Tarjeta":
                                                {

                                                    pagoProcesado.CreditCards.SetCurrentLine(0);
                                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Documento.Fecha.Year, Documento.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                    pagoProcesado.CreditCards.CreditCard = 1;
                                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                    pagoProcesado.CreditCards.CreditAcct = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);



                                                    break;
                                                }
                                            case "Transferencia":
                                                {
                                                    pagoProcesado.TransferAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                    pagoProcesado.TransferReference = item.NumReferencia;
                                                    pagoProcesado.TransferSum = Convert.ToDouble(item.Monto);

                                                    break;
                                                }
                                            case "Cheque":
                                                {
                                                    pagoProcesado.Checks.SetCurrentLine(0);
                                                    pagoProcesado.Checks.CheckAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.Checks.DueDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                    pagoProcesado.Checks.CheckNumber = Convert.ToInt32(item.NumReferencia);
                                                    pagoProcesado.Checks.CheckSum = Convert.ToDouble(item.Monto);
                                                    //pagoProcesado.Checks.CountryCode = "CR";
                                                    //pagoProcesado.Checks.Trnsfrable = BoYesNoEnum.tYES;
                                                    pagoProcesado.Checks.ManualCheck = BoYesNoEnum.tNO;


                                                    break;
                                                }
                                        }
                                    }

                                    var respuestaPago = pagoProcesado.Add();
                                    if (respuestaPago == 0)
                                    {

                                        db.Entry(Documento).State = EntityState.Modified;
                                        Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                        Documento.PagoProcesadaSAP = true;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var error = "hubo un error en el pago " + Conexion.Company.GetLastErrorDescription();
                                        BitacoraErrores be = new BitacoraErrores();
                                        be.Descripcion = error;
                                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                        be.Fecha = DateTime.Now;
                                        be.JSON = JsonConvert.SerializeObject(documentoSAP);
                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();
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


                                Conexion.Desconectar();

                            }
                            else
                            {
                                var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = error;
                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                be.Fecha = DateTime.Now;
                                be.JSON = JsonConvert.SerializeObject(documentoSAP);
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


                    ///







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


        [Route("api/DocumentosMasivos/InsertarSAPPago")]
        [HttpGet]
        public HttpResponseMessage GetMasivoPago()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();

                var DocumentosSPP = db.EncDocumento.Where(a => a.ProcesadaSAP == true).ToList();

                foreach (var item2 in DocumentosSPP)
                {
                    EncDocumento Documento = db.EncDocumento.Where(a => a.id == item2.id).FirstOrDefault();


                    //Insercion e itento a SAP
                    if (Documento.ProcesadaSAP == true && Documento.PagoProcesadaSAP == false)
                    {
                        try
                        {
                            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Documento.CodSuc).FirstOrDefault();
                            var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);


                          

                          


                            
                            if (Documento.ProcesadaSAP == true && Documento.PagoProcesadaSAP == false) //se creo exitorsamente 
                            {
                           

                                //Procesamos el pago
                                try
                                {
                                    var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                    pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                    pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                                    pagoProcesado.DocDate = DateTime.Now;
                                    pagoProcesado.DueDate = DateTime.Now;
                                    pagoProcesado.TaxDate = DateTime.Now;
                                    pagoProcesado.VatDate = DateTime.Now;
                                    pagoProcesado.Remarks = "pago procesado por novapos";
                                    pagoProcesado.DocCurrency = Documento.Moneda;
                                    pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                    //ligar la factura con el pago 

                                    pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                    pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);
                                    pagoProcesado.Invoices.SumApplied = Convert.ToDouble(Documento.TotalCompra);

                                    //meter los metodos de pago

                                    var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id).ToList();


                                    var MontoOtros = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Count() == null || db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Count() == 0 ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Sum(a => a.Monto);

                                    foreach (var item in MetodosPagos)
                                    {
                                        switch (item.Metodo)
                                        {
                                            case "Efectivo":
                                                {
                                                    pagoProcesado.CashAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.CashSum = Convert.ToDouble(item.Monto + MontoOtros);

                                                    break;
                                                }
                                            case "Tarjeta":
                                                {

                                                    pagoProcesado.CreditCards.SetCurrentLine(0);
                                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Documento.Fecha.Year, Documento.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                    pagoProcesado.CreditCards.CreditCard = 1;
                                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                    pagoProcesado.CreditCards.CreditAcct = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);



                                                    break;
                                                }
                                            case "Transferencia":
                                                {
                                                    pagoProcesado.TransferAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                    pagoProcesado.TransferReference = item.NumReferencia;
                                                    pagoProcesado.TransferSum = Convert.ToDouble(item.Monto);

                                                    break;
                                                }
                                            case "Cheque":
                                                {
                                                    pagoProcesado.Checks.SetCurrentLine(0);
                                                    pagoProcesado.Checks.CheckAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.Checks.DueDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                    pagoProcesado.Checks.CheckNumber = Convert.ToInt32(item.NumReferencia);
                                                    pagoProcesado.Checks.CheckSum = Convert.ToDouble(item.Monto);
                                                    //pagoProcesado.Checks.CountryCode = "CR";
                                                    //pagoProcesado.Checks.Trnsfrable = BoYesNoEnum.tYES;
                                                    pagoProcesado.Checks.ManualCheck = BoYesNoEnum.tNO;


                                                    break;
                                                }
                                        }
                                    }

                                    var respuestaPago = pagoProcesado.Add();
                                    if (respuestaPago == 0)
                                    {

                                        db.Entry(Documento).State = EntityState.Modified;
                                        Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                        Documento.PagoProcesadaSAP = true;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var error = "hubo un error en el pago " + Conexion.Company.GetLastErrorDescription();
                                        BitacoraErrores be = new BitacoraErrores();
                                        be.Descripcion = error;
                                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                        be.Fecha = DateTime.Now;
                                        be.JSON = JsonConvert.SerializeObject(documentoSAP);
                                        db.BitacoraErrores.Add(be);
                                        db.SaveChanges();
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


                                Conexion.Desconectar();

                            }
                            else
                            {
                                var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = error;
                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                be.Fecha = DateTime.Now;
                                be.JSON = JsonConvert.SerializeObject(documentoSAP);
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


                    ///







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
