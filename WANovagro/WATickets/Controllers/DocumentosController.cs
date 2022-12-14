
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

    public class DocumentosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/Documentos/SincronizarSAP")]
        public HttpResponseMessage GetExtraeDatos([FromUri] int id)
        {
            try
            {
                var Documento = db.EncDocumento.Where(a => a.id == id).FirstOrDefault();
                var param = db.Parametros.FirstOrDefault();
                if (Documento.id != null)
                {
                    //Inserccion NC SAP
                    if (Documento.TipoDocumento == "03") // Si es una nota de credito
                    {
                        try
                        {
                            var DocumentoG = db.EncDocumento.Where(a => a.id == Documento.BaseEntry).FirstOrDefault();
                            var DetalleG = db.DetDocumento.Where(a => a.idEncabezado == DocumentoG.id).ToList();
                            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Documento.CodSuc).FirstOrDefault();
                            var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                            //Encabezado

                            documentoSAP.DocObjectCode = BoObjectTypes.oCreditNotes;
                            documentoSAP.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                            documentoSAP.DocCurrency = Documento.Moneda == "CRC" ? "CRC" : Documento.Moneda;
                            documentoSAP.DocDate = Documento.Fecha;
                            //documentoSAP.DocDueDate = Documento.FechaVencimiento;

                            //documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
                            documentoSAP.NumAtCard = "APP NC" + " " + Documento.id;
                            documentoSAP.Comments = Documento.Comentarios;

                            // documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().CodSAP);

                            documentoSAP.Series = Sucursal.SerieNC; //Quemada

                            //documentoSAP.GroupNumber = -1;
                            //documentoSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault().CodSAP);
                            documentoSAP.UserFields.Fields.Item("U_LDT_NumeroGTI").Value = Documento.ConsecutivoHacienda;
                            documentoSAP.UserFields.Fields.Item("U_LDT_FiscalDoc").Value = Documento.ClaveHacienda;
                            documentoSAP.UserFields.Fields.Item("U_DYD_Estado").Value = "A";



                            //Detalle
                            int z = 0;
                            var Detalle = db.DetDocumento.Where(a => a.idEncabezado == id).ToList();
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
                                //documentoSAP.Lines.BaseType = Convert.ToInt32(SAPbobsCOM.BoObjectTypes.oInvoices);
                                //documentoSAP.Lines.BaseEntry = Convert.ToInt32(DocumentoG.DocEntry);

                                //documentoSAP.Lines.BaseLine = DetalleG.Where(a => a.idProducto == item.idProducto).FirstOrDefault() == null ? 0 : DetalleG.Where(a => a.idProducto == item.idProducto).FirstOrDefault().NumLinea;
                                documentoSAP.Lines.CostingCode = "Frrt";
                                documentoSAP.Lines.CostingCode2 = "NOS";
                                documentoSAP.Lines.CostingCode3 = "Ventas";
                                //documentoSAP.Lines.CostingCode4 = "";
                                //documentoSAP.Lines.CostingCode5 = "";
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





                                Conexion.Desconectar();

                            }
                            else
                            {
                                var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = error;
                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                be.Fecha = DateTime.Now;
                                be.JSON = JsonConvert.SerializeObject(DocumentoG);
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
                    else
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
                            documentoSAP.NumAtCard = "APP FAC" + " " + Documento.id;
                            documentoSAP.Comments = Documento.Comentarios;
                            documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().CodSAP);
                            var CondPago = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().Nombre;
                            documentoSAP.Series = CondPago.ToLower().Contains("contado") ? Sucursal.SerieFECO : Sucursal.SerieFECR;  //4;  //param.SerieProforma; //Quemada


                            documentoSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault().CodSAP);
                            documentoSAP.UserFields.Fields.Item("U_LDT_NumeroGTI").Value = Documento.ConsecutivoHacienda;
                            documentoSAP.UserFields.Fields.Item("U_LDT_FiscalDoc").Value = Documento.ClaveHacienda;
                            documentoSAP.UserFields.Fields.Item("U_DYD_Estado").Value = "A";

                            //Detalle
                            int z = 0;
                            var Detalle = db.DetDocumento.Where(a => a.idEncabezado == id).ToList();
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
                                documentoSAP.Lines.CostingCode = "Frrt";
                                documentoSAP.Lines.CostingCode2 = "NOS";
                                documentoSAP.Lines.CostingCode3 = "Ventas";
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
                                var CondicionPago = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? db.CondicionesPagos.FirstOrDefault() : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault();
                                //Procesamos el pago
                                if (CondicionPago.Dias == 0)
                                {
                                    try
                                    {
                                        var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                        pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                        pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                                        pagoProcesado.DocDate = Documento.Fecha;
                                        pagoProcesado.DueDate = DateTime.Now;
                                        pagoProcesado.TaxDate = DateTime.Now;
                                        pagoProcesado.VatDate = DateTime.Now;
                                        pagoProcesado.Remarks = "pago procesado por novapos";
                                        pagoProcesado.CounterReference = "APP FAC" + Documento.id;
                                        pagoProcesado.DocCurrency = Documento.Moneda;
                                        pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                        //ligar la factura con el pago 

                                        pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                        pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);
                                        pagoProcesado.Invoices.SumApplied = Convert.ToDouble(Documento.TotalCompra);

                                        //meter los metodos de pago

                                        var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id).ToList();


                                        var MontoOtros = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Count() == null || db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Count() == 0 ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Sum(a => a.Monto);

                                        pagoProcesado.CashAccount = db.CuentasBancarias.Where(a => a.Nombre.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Nombre.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc).FirstOrDefault().CuentaSAP;
                                        pagoProcesado.CashSum = Convert.ToDouble(MetodosPagos.Sum(a => a.Monto) + MontoOtros);
                                        pagoProcesado.Series = 161;

                                        //foreach (var item in MetodosPagos)
                                        //{
                                        //    item.Metodo = "Efectivo";
                                        //    item.idCuentaBancaria = db.CuentasBancarias.Where(a => a.Nombre.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc).FirstOrDefault() == null ? item.idCuentaBancaria : db.CuentasBancarias.Where(a => a.Nombre.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc).FirstOrDefault().id;

                                        //    switch (item.Metodo)
                                        //    {
                                        //        case "Efectivo":
                                        //            {
                                        //                pagoProcesado.CashAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                        //                pagoProcesado.CashSum = Convert.ToDouble(item.Monto + MontoOtros);

                                        //                break;
                                        //            }
                                        //        case "Tarjeta":
                                        //            {

                                        //                pagoProcesado.CreditCards.SetCurrentLine(0);
                                        //                pagoProcesado.CreditCards.CardValidUntil = new DateTime(Documento.Fecha.Year, Documento.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                        //                pagoProcesado.CreditCards.CreditCard = 1;
                                        //                pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                        //                pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                        //                pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                        //                pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                        //                pagoProcesado.CreditCards.CreditAcct = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                        //                pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);



                                        //                break;
                                        //            }
                                        //        case "Transferencia":
                                        //            {
                                        //                pagoProcesado.TransferAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                        //                pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                        //                pagoProcesado.TransferReference = item.NumReferencia;
                                        //                pagoProcesado.TransferSum = Convert.ToDouble(item.Monto);

                                        //                break;
                                        //            }
                                        //        case "Cheque":
                                        //            {
                                        //                pagoProcesado.Checks.SetCurrentLine(0);
                                        //                pagoProcesado.Checks.CheckAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                                        //                pagoProcesado.Checks.DueDate = DateTime.Now; //Fecha en la que se mete el pago 
                                        //                pagoProcesado.Checks.CheckNumber = Convert.ToInt32(item.NumReferencia);
                                        //                pagoProcesado.Checks.CheckSum = Convert.ToDouble(item.Monto);
                                        //                //pagoProcesado.Checks.CountryCode = "CR";
                                        //                //pagoProcesado.Checks.Trnsfrable = BoYesNoEnum.tYES;
                                        //                pagoProcesado.Checks.ManualCheck = BoYesNoEnum.tNO;


                                        //                break;
                                        //            }
                                        //    }
                                        //}

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

                var time = new DateTime(); // 01-01-0001
                if (filtro.FechaFinal != time)
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
                    a.DocEntry,

                    a.idCondPago,
                    a.idVendedor,
                    a.ProcesadaSAP,
                    a.PagoProcesadaSAP,
                    a.ClaveHacienda,
                    a.ConsecutivoHacienda,
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


                if (filtro.Codigo3 > 0)
                {
                    Documentos = Documentos.Where(a => a.idCaja == filtro.Codigo3).ToList();

                }

                if (filtro.Codigo4 > 0) // esto por ser integer
                {
                    Documentos = Documentos.Where(a => a.idCondPago == filtro.Codigo4).ToList();
                }

                if (filtro.Codigo5 > 0) // esto por ser integer
                {
                    Documentos = Documentos.Where(a => a.idVendedor == filtro.Codigo5).ToList();
                }

                if (filtro.Codigo6 > 0) // esto por ser integer
                {
                    Documentos = Documentos.Where(a => a.idCaja == filtro.Codigo6).ToList();
                }


                if (filtro.Procesado != null && filtro.Activo) //recordar poner el filtro.activo en novapp
                {
                    Documentos = Documentos.Where(a => a.ProcesadaSAP == filtro.Procesado).ToList();
                }

                //if (filtro.PagoProcesado != null)
                //{
                //    Documentos = Documentos.Where(a => a.PagoProcesadaSAP == filtro.PagoProcesado).ToList();
                //}






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
                    a.DocEntry,
                    a.ProcesadaSAP,
                    a.idCondPago,
                    a.idVendedor,
                    a.ClaveHacienda,
                    a.ConsecutivoHacienda,
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
        public async Task<HttpResponseMessage> PostAsync([FromBody] Documentos documento)
        {
            var t = db.Database.BeginTransaction();
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
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
                    Documento.DocEntry = "";
                    Documento.ProcesadaSAP = false;
                    Documento.idCondPago = documento.idCondPago;
                    Documento.idVendedor = documento.idVendedor;
                    Documento.ClaveHacienda = "";
                    Documento.ConsecutivoHacienda = "";
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

                    if (Documento.TipoDocumento == "03")
                    {

                        var DocumentoG = db.EncDocumento.Where(a => a.id == documento.BaseEntry).FirstOrDefault();

                        db.Entry(Documento).State = EntityState.Modified;
                        Documento.idVendedor = DocumentoG.idVendedor;
                        db.SaveChanges();
                      
                        // Si es NC debe rebajar de los cierres el monto
                        var time2 = DocumentoG.Fecha.Date;
                        var MontoDevuelto = db.EncDocumento.Where(a => a.BaseEntry == documento.BaseEntry && a.TipoDocumento == "03").Sum(a => a.TotalCompra) - documento.TotalCompra; //Este es el monto que ya se ha devuelto de dineros
                        var MontosxMetodo = db.MetodosPagos.Where(a => a.idEncabezado == documento.BaseEntry).GroupBy(a => a.Metodo).ToList(); // Cantidad de Dinero pagados por metodos de pago
                        var CierreCajaM = db.CierreCajas.Where(a => a.FechaCaja == time2 && a.idCaja == DocumentoG.idCaja && a.idUsuario == DocumentoG.idUsuarioCreador).FirstOrDefault();

                        decimal banderaDevuelto = 0;
                        decimal montoadevolver = documento.TotalCompra;
                        foreach(var item in MontosxMetodo)
                        {
                            if(CierreCajaM != null)
                            {
                                if (banderaDevuelto < documento.TotalCompra) // Si ya le llegue al total que tengo que devolver
                                {
                                    
                                    if(item.Sum(a => a.Monto) >= montoadevolver) // si lo que pagaron con un metodo es mayor o igual a lo que estoy rebajando
                                    {
                                        banderaDevuelto += montoadevolver;
                                        db.Entry(CierreCajaM).State = EntityState.Modified;
                                        switch (item.Key)
                                        {
                                            case "Efectivo":
                                                {
                                                    if(DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.EfectivoColones -= montoadevolver;
                                                        CierreCajaM.TotalVendidoColones -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = - montoadevolver;
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();

                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.EfectivoFC -= montoadevolver;
                                                        CierreCajaM.TotalVendidoFC -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                   
                                                    break;
                                                }
                                            case "Tarjeta":
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.TarjetasColones -= montoadevolver;
                                                        CierreCajaM.TotalVendidoColones -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = item.FirstOrDefault().BIN;
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = item.FirstOrDefault().NumReferencia;
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();

                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.TarjetasFC -= montoadevolver;
                                                        CierreCajaM.TotalVendidoFC -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = item.FirstOrDefault().BIN;
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = item.FirstOrDefault().NumReferencia;
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                       

                                                    break;
                                                }
                                            case "Transferencia":
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.TransferenciasColones -= montoadevolver;
                                                        CierreCajaM.TotalVendidoColones -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.TransferenciasDolares -= montoadevolver;
                                                        CierreCajaM.TotalVendidoFC -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                  

                                                    break;
                                                }
                                            case "Cheque":
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.ChequesColones -= montoadevolver;
                                                        CierreCajaM.TotalVendidoColones -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = item.FirstOrDefault().NumCheque;
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.ChequesFC -= montoadevolver;
                                                        CierreCajaM.TotalVendidoFC -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = item.FirstOrDefault().NumCheque;
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                   

                                                    break;
                                                }

                                            default:
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.OtrosMediosColones -= montoadevolver;
                                                        CierreCajaM.TotalVendidoColones -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.OtrosMediosFC -= montoadevolver;
                                                        CierreCajaM.TotalVendidoFC -= montoadevolver;

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -montoadevolver;
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    

                                                    break;
                                                }
                                        }

                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        montoadevolver -= item.Sum(a => a.Monto);
                                        banderaDevuelto += item.Sum(a => a.Monto);
                                        db.Entry(CierreCajaM).State = EntityState.Modified;

                                        switch (item.Key)
                                        {
                                            case "Efectivo":
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.EfectivoColones -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoColones -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.EfectivoFC -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoFC -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }

                                                    break;
                                                }
                                            case "Tarjeta":
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.TarjetasColones -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoColones -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = item.FirstOrDefault().BIN;
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = item.FirstOrDefault().NumReferencia;
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.TarjetasFC -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoFC -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = item.FirstOrDefault().BIN;
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = item.FirstOrDefault().NumReferencia;
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }


                                                    break;
                                                }
                                            case "Transferencia":
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.TransferenciasColones -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoColones -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.TransferenciasDolares -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoFC -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }


                                                    break;
                                                }
                                            case "Cheque":
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.ChequesColones -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoColones -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = item.FirstOrDefault().NumCheque;
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.ChequesFC -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoFC -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = item.FirstOrDefault().NumCheque;
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }


                                                    break;
                                                }

                                            default:
                                                {
                                                    if (DocumentoG.Moneda == "CRC")
                                                    {
                                                        CierreCajaM.OtrosMediosColones -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoColones -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        CierreCajaM.OtrosMediosFC -= item.Sum(a => a.Monto);
                                                        CierreCajaM.TotalVendidoFC -= item.Sum(a => a.Monto);

                                                        MetodosPagos MetodosPagos = new MetodosPagos();
                                                        MetodosPagos.idEncabezado = DocumentoG.id;
                                                        MetodosPagos.Monto = -item.Sum(a => a.Monto);
                                                        MetodosPagos.BIN = "";
                                                        MetodosPagos.NumCheque = "";
                                                        MetodosPagos.NumReferencia = "";
                                                        MetodosPagos.Metodo = item.Key;
                                                        MetodosPagos.idCuentaBancaria = item.FirstOrDefault().idCuentaBancaria;
                                                        db.MetodosPagos.Add(MetodosPagos);
                                                        db.SaveChanges();
                                                    }


                                                    break;
                                                }
                                        }
                                        db.SaveChanges();

                                    }
                                }
                            }
                        }

                        if((MontoDevuelto + documento.TotalCompra) >= DocumentoG.TotalCompra)
                        {
                            db.Entry(DocumentoG).State = EntityState.Modified;
                            DocumentoG.Status = "1";
                            db.SaveChanges();
                        }

                        //

                    }
                    var time = DateTime.Now.Date;
                    var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == time && a.idCaja == documento.idCaja && a.idUsuario == Documento.idUsuarioCreador).FirstOrDefault();
                    if (documento.MetodosPagos != null)
                    {
                        foreach (var item in documento.MetodosPagos)
                        {
                            MetodosPagos MetodosPagos = new MetodosPagos();
                            MetodosPagos.idEncabezado = Documento.id;
                            MetodosPagos.Monto = item.Monto;
                            MetodosPagos.BIN = item.BIN;
                            MetodosPagos.NumCheque = item.NumCheque;
                            MetodosPagos.NumReferencia = item.NumReferencia;
                            MetodosPagos.Metodo = item.Metodo;
                            MetodosPagos.idCuentaBancaria = item.idCuentaBancaria;
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
                                        case "Transferencia":
                                            {
                                                CierreCaja.TransferenciasColones += item.Monto;
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
                                        case "Transferencia":
                                            {
                                                CierreCaja.TransferenciasDolares += item.Monto;
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
                    }




                    documento.id = Documento.id;
                    BitacoraMovimientos btm = new BitacoraMovimientos();
                    btm.idUsuario = documento.idUsuarioCreador;
                    btm.Descripcion = "Se crea un documento para el cliente con el id: " + documento.idCliente;
                    btm.Fecha = DateTime.Now;
                    btm.Metodo = "Insercion de Documento";
                    db.BitacoraMovimientos.Add(btm);
                    db.SaveChanges();
                    t.Commit();


                    ////Mandar al api de facturacion
                    ///

                    var Parametros = db.Parametros.FirstOrDefault();
                    HttpClient cliente = new HttpClient();

                    try
                    {

                        var Url = Parametros.UrlFacturaElectronica.Replace("@DocNumR", Documento.id.ToString()).Replace("@ObjTypeR", (Documento.TipoDocumento != "03" ? "13" : "14")).Replace("@SucursalR", "099");

                        HttpResponseMessage response = await cliente.GetAsync(Url);
                        if (response.IsSuccessStatusCode)
                        {
                            response.Content.Headers.ContentType.MediaType = "application/json";
                            var res = await response.Content.ReadAsAsync<RecibidoFacturacion>();

                            db.Entry(Documento).State = EntityState.Modified;
                            Documento.ClaveHacienda = res.ClaveHacienda;
                            Documento.ConsecutivoHacienda = res.ConsecutivoHacienda;

                            documento.ClaveHacienda = res.ClaveHacienda;
                            documento.ConsecutivoHacienda = res.ConsecutivoHacienda;
                            db.SaveChanges();


                            try
                            {
                                HttpClient cliente2 = new HttpClient();

                                var Url2 = Parametros.UrlConsultaFacturas.Replace("@ClaveR", Documento.ClaveHacienda.ToString()).Replace("@SucursalR", "099");

                                HttpResponseMessage response2 = await cliente2.GetAsync(Url2);
                                if (response2.IsSuccessStatusCode)
                                {
                                    response2.Content.Headers.ContentType.MediaType = "application/json";
                                    var res2 = await response2.Content.ReadAsStringAsync();
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


                    //Se termina el api de facturacion 


                    //Insercion e itento a SAP
                    //if (Documento.id != null)
                    //{
                    //    //Inserccion NC SAP
                    //    if (Documento.TipoDocumento == "03" ) // Si es una nota de credito
                    //    {
                    //        try
                    //        {
                    //            var DocumentoG = db.EncDocumento.Where(a => a.id == Documento.BaseEntry).FirstOrDefault();
                    //            var DetalleG = db.DetDocumento.Where(a => a.idEncabezado == DocumentoG.id).ToList();
                    //            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Documento.CodSuc).FirstOrDefault();
                    //            var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                    //            //Encabezado

                    //            documentoSAP.DocObjectCode = BoObjectTypes.oCreditNotes;
                    //            documentoSAP.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                    //            documentoSAP.DocCurrency = Documento.Moneda == "CRC" ? "CRC" : Documento.Moneda;
                    //            documentoSAP.DocDate = Documento.Fecha;
                    //            //documentoSAP.DocDueDate = Documento.FechaVencimiento;

                    //            //documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
                    //            documentoSAP.NumAtCard = "APP NC" + " " + Documento.id;
                    //            documentoSAP.Comments = Documento.Comentarios;

                    //           // documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().CodSAP);

                    //            documentoSAP.Series =  Sucursal.SerieNC; //Quemada

                    //            //documentoSAP.GroupNumber = -1;
                    //            //documentoSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault().CodSAP);


                    //            //Detalle
                    //            int z = 0;

                    //            foreach (var item in documento.Detalle)
                    //            {






                    //                documentoSAP.Lines.SetCurrentLine(z);

                    //                documentoSAP.Lines.Currency = Documento.Moneda == "CRC" ? "CRC" : Documento.Moneda;
                    //                documentoSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                    //                documentoSAP.Lines.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                    //                documentoSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                    //                var idImp = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idImpuesto;
                    //                documentoSAP.Lines.TaxCode = item.idExoneracion > 0 ? "EX" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().Codigo;
                    //                documentoSAP.Lines.TaxOnly = BoYesNoEnum.tNO;


                    //                documentoSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                    //                var idBod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                    //                documentoSAP.Lines.WarehouseCode = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? "01" : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault().CodSAP;
                    //                //documentoSAP.Lines.BaseType = Convert.ToInt32(SAPbobsCOM.BoObjectTypes.oInvoices);
                    //                //documentoSAP.Lines.BaseEntry = Convert.ToInt32(DocumentoG.DocEntry);

                    //                //documentoSAP.Lines.BaseLine = DetalleG.Where(a => a.idProducto == item.idProducto).FirstOrDefault() == null ? 0 : DetalleG.Where(a => a.idProducto == item.idProducto).FirstOrDefault().NumLinea ;

                    //                documentoSAP.Lines.Add();
                    //                z++;
                    //            }


                    //            var respuesta = documentoSAP.Add();
                    //            if (respuesta == 0) //se creo exitorsamente 
                    //            {
                    //                db.Entry(Documento).State = EntityState.Modified;
                    //                Documento.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                    //                Documento.ProcesadaSAP = true;
                    //                db.SaveChanges();





                    //                Conexion.Desconectar();

                    //            }
                    //            else
                    //            {
                    //                var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                    //                BitacoraErrores be = new BitacoraErrores();
                    //                be.Descripcion = error;
                    //                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                    //                be.Fecha = DateTime.Now;
                    //                be.JSON = JsonConvert.SerializeObject(DocumentoG);
                    //                db.BitacoraErrores.Add(be);
                    //                db.SaveChanges();
                    //                Conexion.Desconectar();


                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            BitacoraErrores be = new BitacoraErrores();
                    //            be.Descripcion = ex.Message;
                    //            be.StrackTrace = ex.StackTrace;
                    //            be.Fecha = DateTime.Now;
                    //            be.JSON = JsonConvert.SerializeObject(ex);
                    //            db.BitacoraErrores.Add(be);
                    //            db.SaveChanges();

                    //        }
                    //    }
                    //    else
                    //    {
                    //        try
                    //        {
                    //            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Documento.CodSuc).FirstOrDefault();
                    //            var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                    //            //Encabezado

                    //            documentoSAP.DocObjectCode = BoObjectTypes.oInvoices;
                    //            documentoSAP.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                    //            documentoSAP.DocCurrency = Documento.Moneda == "CRC" ? "CRC" : Documento.Moneda;
                    //            documentoSAP.DocDate = Documento.Fecha;
                    //            documentoSAP.DocDueDate = Documento.FechaVencimiento;
                    //            documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
                    //            documentoSAP.NumAtCard = "APP FAC" + " " + Documento.id;
                    //            documentoSAP.Comments = Documento.Comentarios;
                    //            documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().CodSAP);
                    //            var CondPago = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().Nombre;
                    //            documentoSAP.Series = CondPago.ToLower().Contains("contado") ? Sucursal.SerieFECO : Sucursal.SerieFECR;  //4;  //param.SerieProforma; //Quemada


                    //            documentoSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault().CodSAP);


                    //            //Detalle
                    //            int z = 0;

                    //            foreach (var item in documento.Detalle)
                    //            {
                    //                documentoSAP.Lines.SetCurrentLine(z);

                    //                documentoSAP.Lines.Currency = Documento.Moneda == "CRC" ? "CRC" : Documento.Moneda;
                    //                documentoSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                    //                documentoSAP.Lines.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                    //                documentoSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                    //                var idImp = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idImpuesto;
                    //                documentoSAP.Lines.TaxCode = item.idExoneracion > 0 ? "EX" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().Codigo;
                    //                documentoSAP.Lines.TaxOnly = BoYesNoEnum.tNO;


                    //                documentoSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                    //                var idBod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                    //                documentoSAP.Lines.WarehouseCode = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? "01" : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault().CodSAP;

                    //                documentoSAP.Lines.Add();
                    //                z++;
                    //            }


                    //            var respuesta = documentoSAP.Add();
                    //            if (respuesta == 0) //se creo exitorsamente 
                    //            {
                    //                db.Entry(Documento).State = EntityState.Modified;
                    //                Documento.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                    //                Documento.ProcesadaSAP = true;
                    //                db.SaveChanges();

                    //                //Procesamos el pago
                    //                try
                    //                {
                    //                    var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                    //                    pagoProcesado.DocType = BoRcptTypes.rCustomer;
                    //                    pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                    //                    pagoProcesado.DocDate = DateTime.Now;
                    //                    pagoProcesado.DueDate = DateTime.Now;
                    //                    pagoProcesado.TaxDate = DateTime.Now;
                    //                    pagoProcesado.VatDate = DateTime.Now;
                    //                    pagoProcesado.Remarks = "pago procesado por novapos";
                    //                    pagoProcesado.CounterReference = "APP FAC" + Documento.id;
                    //                    pagoProcesado.DocCurrency = Documento.Moneda;
                    //                    pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                    //                    //ligar la factura con el pago 

                    //                    pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                    //                    pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);
                    //                    pagoProcesado.Invoices.SumApplied = Convert.ToDouble(Documento.TotalCompra);

                    //                    //meter los metodos de pago

                    //                    var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id).ToList();


                    //                    var MontoOtros = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Count() == null || db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Count() == 0 ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Metodo.Contains("Otros")).Sum(a => a.Monto);

                    //                    foreach (var item in MetodosPagos)
                    //                    {
                    //                        switch (item.Metodo)
                    //                        {
                    //                            case "Efectivo":
                    //                                {
                    //                                    pagoProcesado.CashAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                    //                                    pagoProcesado.CashSum = Convert.ToDouble(item.Monto + MontoOtros);

                    //                                    break;
                    //                                }
                    //                            case "Tarjeta":
                    //                                {

                    //                                    pagoProcesado.CreditCards.SetCurrentLine(0);
                    //                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Documento.Fecha.Year, Documento.Fecha.Month, 28); //Fecha en la que se mete el pago 
                    //                                    pagoProcesado.CreditCards.CreditCard = 1;
                    //                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                    //                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                    //                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                    //                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                    //                                    pagoProcesado.CreditCards.CreditAcct = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                    //                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);



                    //                                    break;
                    //                                }
                    //                            case "Transferencia":
                    //                                {
                    //                                    pagoProcesado.TransferAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                    //                                    pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                    //                                    pagoProcesado.TransferReference = item.NumReferencia;
                    //                                    pagoProcesado.TransferSum = Convert.ToDouble(item.Monto);

                    //                                    break;
                    //                                }
                    //                            case "Cheque":
                    //                                {
                    //                                    pagoProcesado.Checks.SetCurrentLine(0);
                    //                                    pagoProcesado.Checks.CheckAccount = db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == item.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                    //                                    pagoProcesado.Checks.DueDate = DateTime.Now; //Fecha en la que se mete el pago 
                    //                                    pagoProcesado.Checks.CheckNumber = Convert.ToInt32(item.NumReferencia);
                    //                                    pagoProcesado.Checks.CheckSum = Convert.ToDouble(item.Monto);
                    //                                    //pagoProcesado.Checks.CountryCode = "CR";
                    //                                    //pagoProcesado.Checks.Trnsfrable = BoYesNoEnum.tYES;
                    //                                    pagoProcesado.Checks.ManualCheck = BoYesNoEnum.tNO;


                    //                                    break;
                    //                                }
                    //                        }
                    //                    }

                    //                    var respuestaPago = pagoProcesado.Add();
                    //                    if (respuestaPago == 0)
                    //                    {

                    //                        db.Entry(Documento).State = EntityState.Modified;
                    //                        Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                    //                        Documento.PagoProcesadaSAP = true;
                    //                        db.SaveChanges();
                    //                    }
                    //                    else
                    //                    {
                    //                        var error = "hubo un error en el pago " + Conexion.Company.GetLastErrorDescription();
                    //                        BitacoraErrores be = new BitacoraErrores();
                    //                        be.Descripcion = error;
                    //                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                    //                        be.Fecha = DateTime.Now;
                    //                        be.JSON = JsonConvert.SerializeObject(documentoSAP);
                    //                        db.BitacoraErrores.Add(be);
                    //                        db.SaveChanges();
                    //                    }


                    //                }
                    //                catch (Exception ex)
                    //                {

                    //                    BitacoraErrores be = new BitacoraErrores();
                    //                    be.Descripcion = ex.Message;
                    //                    be.StrackTrace = ex.StackTrace;
                    //                    be.Fecha = DateTime.Now;
                    //                    be.JSON = JsonConvert.SerializeObject(ex);
                    //                    db.BitacoraErrores.Add(be);
                    //                    db.SaveChanges();
                    //                }


                    //                Conexion.Desconectar();

                    //            }
                    //            else
                    //            {
                    //                var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                    //                BitacoraErrores be = new BitacoraErrores();
                    //                be.Descripcion = error;
                    //                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                    //                be.Fecha = DateTime.Now;
                    //                be.JSON = JsonConvert.SerializeObject(documentoSAP);
                    //                db.BitacoraErrores.Add(be);
                    //                db.SaveChanges();
                    //                Conexion.Desconectar();


                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            BitacoraErrores be = new BitacoraErrores();
                    //            be.Descripcion = ex.Message;
                    //            be.StrackTrace = ex.StackTrace;
                    //            be.Fecha = DateTime.Now;
                    //            be.JSON = JsonConvert.SerializeObject(ex);
                    //            db.BitacoraErrores.Add(be);
                    //            db.SaveChanges();

                    //        }
                    //    }


                    //}


                    ///

                }




                else
                {
                    throw new Exception("Ya existe un documento con este ID");
                }
                documento.Fecha = Documento.Fecha;
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, documento);
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


        [Route("api/Documentos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Documentos documento)
        {
            var t = db.Database.BeginTransaction();
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
                    Documento.idVendedor = documento.idVendedor;

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
                    if (documento.MetodosPagos != null)
                    {
                        foreach (var item in documento.MetodosPagos)
                        {
                            MetodosPagos MetodosPagos = new MetodosPagos();
                            MetodosPagos.idEncabezado = Documento.id;
                            MetodosPagos.Monto = item.Monto;
                            MetodosPagos.BIN = item.BIN;
                            MetodosPagos.NumCheque = item.NumCheque;
                            MetodosPagos.NumReferencia = item.NumReferencia;
                            MetodosPagos.idCuentaBancaria = item.idCuentaBancaria;
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
                    }



                    BitacoraMovimientos btm = new BitacoraMovimientos();
                    btm.idUsuario = documento.idUsuarioCreador;
                    btm.Descripcion = "Se edito el documento: " + Documento.id + " del cliente con el id: " + documento.idCliente;
                    btm.Fecha = DateTime.Now;
                    btm.Metodo = "Edicion de Documento";
                    db.BitacoraMovimientos.Add(btm);
                    db.SaveChanges();
                    t.Commit();
                }
                else
                {
                    throw new Exception("NO existe un documento con este ID");
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



    }
}