using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class OrdenesCompraController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
            
                var Compras = db.EncCompras.Select(a => new
                {
                    a.id,
                    a.idAprovisionamiento,
                    a.idUsuarioCreador,
                    a.CodProveedor,
                    a.NombreProveedor,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.Subtotal,
                    a.TotalImpuesto,
                    a.TotalCompra,
                    a.Moneda,
                    a.DocEntry,
                    a.ProcesadaSAP,
                    Detalle = db.DetCompras.Where(b => b.idEncabezado == a.id).ToList()
                }).Where(a =>  (filtro.Codigo1 > 0 ? a.idAprovisionamiento == filtro.Codigo1 : true)
                && (filtro.Codigo2 > 0 ? a.idUsuarioCreador == filtro.Codigo2 : true)
                && (filtro.Procesado != null ? a.ProcesadaSAP == filtro.Procesado : true)
                ).ToList(); //Traemos el listado de productos


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Compras);
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

        [Route("api/OrdenesCompra/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Compra = db.EncCompras.Select(a => new
                {
                    a.id,
                    a.idAprovisionamiento,
                    a.idUsuarioCreador,
                    a.CodProveedor,
                    a.NombreProveedor,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.Subtotal,
                    a.TotalImpuesto,
                    a.TotalCompra,
                    a.Moneda,
                    a.DocEntry,
                    a.ProcesadaSAP,
                    Detalle = db.DetCompras.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Compra);
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

        //[Route("api/OrdenesCompra/SincronizarSAP")]
        //public HttpResponseMessage GetExtraeDatos([FromUri] int id)
        //{
        //    try
        //    {
        //        var Compra = db.EncCompras.Where(a => a.id == id).FirstOrDefault();
        //        var param = db.Parametros.FirstOrDefault();
        //        if (Compra.id != null)
        //        {
        //            //Inserccion NC SAP
              
               
        //                try
        //                {
        //                    var Sucursal = db.Sucursales.Where(a => a.CodSuc == Documento.CodSuc).FirstOrDefault();
        //                    var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

        //                    //Encabezado

        //                    documentoSAP.DocObjectCode = BoObjectTypes.oInvoices;
        //                    documentoSAP.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
        //                    documentoSAP.DocCurrency = Documento.Moneda == "CRC" ? param.MonedaLocal : param.MonedaDolar;
        //                    var Dias = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? 0 : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().Dias;

        //                    documentoSAP.DocDate = Documento.Fecha;
        //                    documentoSAP.DocDueDate = Documento.Fecha.AddDays(Dias);
        //                    documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
        //                    documentoSAP.NumAtCard = "APP FAC:" + " " + Documento.id;
        //                    documentoSAP.Comments = Documento.Comentarios.Length > 200 ? Documento.Comentarios.Substring(0, 200) : Documento.Comentarios;
        //                    documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().CodSAP);
        //                    var CondPago = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().Nombre;
        //                    documentoSAP.Series = CondPago.ToLower().Contains("contado") ? Sucursal.SerieFECO : Sucursal.SerieFECR;  //4;  //param.SerieProforma; //Quemada




        //                    if (param.Pais == "C")
        //                    {


        //                        if (Documento.Moneda == "USD")
        //                        {
        //                            documentoSAP.DocTotalFc = Convert.ToDouble(Documento.TotalCompra + Documento.Redondeo);
        //                        }
        //                        else
        //                        {
        //                            documentoSAP.DocTotal = Convert.ToDouble(Documento.TotalCompra + Documento.Redondeo);
        //                        }
        //                    }

        //                    if (param.Pais == "P")
        //                    {


        //                        if (Documento.Moneda == "USD")
        //                        {
        //                            documentoSAP.DocTotal = Convert.ToDouble(Documento.TotalCompra + Documento.Redondeo);
        //                        }
        //                        else
        //                        {
        //                            documentoSAP.DocTotalFc = Convert.ToDouble(Documento.TotalCompra + Documento.Redondeo);
        //                        }
        //                    }

        //                    documentoSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault().CodSAP);
        //                    documentoSAP.UserFields.Fields.Item(param.CampoConsecutivo).Value = Documento.ConsecutivoHacienda; //"U_LDT_NumeroGTI"
        //                    documentoSAP.UserFields.Fields.Item(param.CampoClave).Value = Documento.ClaveHacienda;       //"U_LDT_FiscalDoc"
        //                    //documentoSAP.UserFields.Fields.Item("U_DYD_Estado").Value = "A";
        //                    var Lotes1 = db.Lotes.Where(a => a.idEncabezado == Documento.id && a.Tipo == "F").ToList();
        //                    //Detalle
        //                    int z = 0;
        //                    var Detalle = db.DetDocumento.Where(a => a.idEncabezado == id).ToList();
        //                    foreach (var item in Detalle)
        //                    {
        //                        var BodProducto = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
        //                        var Bodega = db.Bodegas.Where(a => a.id == BodProducto).FirstOrDefault();
        //                        documentoSAP.Lines.SetCurrentLine(z);

        //                        documentoSAP.Lines.Currency = Documento.Moneda == "CRC" ? param.MonedaLocal : param.MonedaDolar;
        //                        documentoSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
        //                        documentoSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
        //                        documentoSAP.Lines.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;



        //                        var idImp = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idImpuesto;
        //                        var ClienteMAG = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? false : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().MAG;
        //                        var ProductoMAG = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? false : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().MAG;
        //                        var Producto = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();

        //                        var ProductoCabys = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Cabys;
        //                        var DetExoneracion = db.DetExoneraciones.Where(a => a.CodCabys == ProductoCabys && a.idEncabezado == item.idExoneracion).FirstOrDefault() == null ? 0 : db.DetExoneraciones.Where(a => a.CodCabys == ProductoCabys && a.idEncabezado == item.idExoneracion).FirstOrDefault().id;
        //                        if (ClienteMAG == true && ProductoMAG == true && DetExoneracion == 0)
        //                        {

        //                            documentoSAP.Lines.TaxCode = db.Impuestos.Where(a => a.Tarifa == 1).FirstOrDefault() == null ? "IVA-1" : db.Impuestos.Where(a => a.Tarifa == 1).FirstOrDefault().Codigo;

        //                        }
        //                        else
        //                        {
        //                            documentoSAP.Lines.TaxCode = item.idExoneracion > 0 ? "EXONERA" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().Codigo;
        //                            if (item.idExoneracion > 0)
        //                            {
        //                                var Exoneracion = db.Exoneraciones.Where(a => a.id == item.idExoneracion).FirstOrDefault();

        //                                documentoSAP.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Exoneracion.TipoDoc;
        //                                documentoSAP.Lines.UserFields.Fields.Item("U_NumDoc").Value = Exoneracion.NumDoc;
        //                                documentoSAP.Lines.UserFields.Fields.Item("U_NomInst").Value = Exoneracion.NomInst;
        //                                documentoSAP.Lines.UserFields.Fields.Item("U_FecEmis").Value = Exoneracion.FechaEmision;
        //                            }
        //                        }
        //                        documentoSAP.Lines.TaxOnly = BoYesNoEnum.tNO;



        //                        documentoSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
        //                        var idBod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
        //                        documentoSAP.Lines.WarehouseCode = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? "01" : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault().CodSAP;
        //                        if (param.CostingCode != "N" && param.CostingCode2 != "N" && param.CostingCode3 != "N")
        //                        {
        //                            documentoSAP.Lines.CostingCode = param.CostingCode;
        //                            //documentoSAP.Lines.CostingCode2 = param.CostingCode2;
        //                            documentoSAP.Lines.CostingCode2 = Sucursal.NormaReparto; //Cambiar progra para Novagro
        //                            documentoSAP.Lines.CostingCode3 = param.CostingCode3;

        //                        }
        //                        else
        //                        {
        //                            switch (Sucursal.Dimension)
        //                            {
        //                                case 1:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode = Sucursal.NormaReparto;

        //                                        break;
        //                                    }
        //                                case 2:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode2 = Sucursal.NormaReparto;
        //                                        break;
        //                                    }
        //                                case 3:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode3 = Sucursal.NormaReparto;
        //                                        break;
        //                                    }
        //                                case 4:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode4 = Sucursal.NormaReparto;
        //                                        break;
        //                                    }
        //                                case 5:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode5 = Sucursal.NormaReparto;
        //                                        break;
        //                                    }

        //                            }
        //                            switch (Producto.Dimension)
        //                            {
        //                                case 1:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode = Producto.NormaReparto;

        //                                        break;
        //                                    }
        //                                case 2:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode2 = Producto.NormaReparto;
        //                                        break;
        //                                    }
        //                                case 3:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode3 = Producto.NormaReparto;
        //                                        break;
        //                                    }
        //                                case 4:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode4 = Producto.NormaReparto;
        //                                        break;
        //                                    }
        //                                case 5:
        //                                    {
        //                                        documentoSAP.Lines.CostingCode5 = Producto.NormaReparto;
        //                                        break;
        //                                    }

        //                            }
        //                        }

        //                        //if(item.NumSerie != "0" || item.NumSerie != null)
        //                        //{
        //                        //    documentoSAP.Lines.SerialNumbers.ManufacturerSerialNumber = item.NumSerie;
        //                        //    documentoSAP.Lines.SerialNumbers.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
        //                        //    documentoSAP.Lines.SerialNumbers.Quantity = Convert.ToDouble(item.Cantidad);
        //                        //    documentoSAP.Lines.SerialNumbers.Add();
        //                        //    //documentoSAP.Lines.SerialNum = item.NumSerie;
        //                        //}
        //                        var ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
        //                        var Lotes2 = Lotes1.Where(a => a.ItemCode == ItemCode && a.idDetalle == item.id).ToList();

        //                        var x = 0;
        //                        foreach (var lot in Lotes2)
        //                        {


        //                            documentoSAP.Lines.SerialNumbers.ManufacturerSerialNumber = lot.Serie;
        //                            documentoSAP.Lines.SerialNumbers.ItemCode = lot.ItemCode;
        //                            documentoSAP.Lines.SerialNumbers.Quantity = Convert.ToDouble(lot.Cantidad);

        //                            documentoSAP.Lines.SerialNumbers.Add();


        //                            x++;
        //                        }

        //                        documentoSAP.Lines.Add();
        //                        z++;
        //                    }


        //                    if (Documento.Redondeo != 0)
        //                    {
        //                        documentoSAP.Rounding = BoYesNoEnum.tYES;
        //                        documentoSAP.RoundingDiffAmount = Convert.ToDouble(Documento.Redondeo);
        //                    }





        //                    var respuesta = documentoSAP.Add();
        //                    if (respuesta == 0) //se creo exitorsamente 
        //                    {
        //                        db.Entry(Documento).State = EntityState.Modified;
        //                        Documento.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
        //                        Documento.ProcesadaSAP = true;
        //                        db.SaveChanges();


        //                        //Procesamos el pago
        //                        var CondicionPago = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? db.CondicionesPagos.FirstOrDefault() : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault();
        //                        //Procesamos el pago
        //                        if (CondicionPago.Dias == 0)
        //                        {
        //                            if (param.MontosPagosSeparados == true)
        //                            {
        //                                try
        //                                {


        //                                    var Fecha = Documento.Fecha.Date;
        //                                    var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
        //                                    var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Monto > 0).FirstOrDefault() == null ? new List<MetodosPagos>() : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Monto > 0).ToList();

        //                                    var MetodosPagosColones = MetodosPagos.Where(a => a.Moneda == "CRC").ToList();
        //                                    var MetodosPagosDolares = MetodosPagos.Where(a => a.Moneda == "USD").ToList();

        //                                    bool pagoColonesProcesado = false;
        //                                    bool pagoDolaresProcesado = false;


        //                                    var contador = 0;




        //                                    if (MetodosPagosColones.Count() > 0)
        //                                    {
        //                                        try
        //                                        {


        //                                            var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
        //                                            pagoProcesado.DocType = BoRcptTypes.rCustomer;
        //                                            pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
        //                                            pagoProcesado.DocDate = Documento.Fecha;
        //                                            pagoProcesado.DueDate = DateTime.Now;
        //                                            pagoProcesado.TaxDate = DateTime.Now;
        //                                            pagoProcesado.VatDate = DateTime.Now;
        //                                            pagoProcesado.Remarks = "Pago procesado por NOVAPOS";
        //                                            pagoProcesado.CounterReference = "APP FAC" + Documento.id;
        //                                            pagoProcesado.DocCurrency = param.MonedaLocal;
        //                                            pagoProcesado.HandWritten = BoYesNoEnum.tNO;
        //                                            pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
        //                                            pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);



        //                                            if (Documento.Moneda != "CRC")
        //                                            {
        //                                                var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto) / TipoCambio.TipoCambio;
        //                                                pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones);
        //                                            }
        //                                            else
        //                                            {
        //                                                var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto);

        //                                                pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

        //                                            }



        //                                            pagoProcesado.Series = Sucursal.SeriePago;//154; 161;


        //                                            var SumatoriaEfectivo = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
        //                                            var PagosTarjetas = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList(); //.Sum(a => a.Monto);
        //                                            var SumatoriaTransferencia = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

        //                                            if (SumatoriaEfectivo > 0)
        //                                            {
        //                                                var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
        //                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


        //                                                pagoProcesado.CashAccount = Cuenta;
        //                                                pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

        //                                            }


        //                                            foreach (var item in PagosTarjetas)
        //                                            {

        //                                                if (item.Monto > 0)
        //                                                {
        //                                                    var idcuenta = item.idCuentaBancaria;
        //                                                    var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
        //                                                    if (contador > 0)
        //                                                    {
        //                                                        pagoProcesado.CreditCards.Add();
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        pagoProcesado.CreditCards.SetCurrentLine(contador);

        //                                                    }
        //                                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Documento.Fecha.Year, Documento.Fecha.Month, 28); //Fecha en la que se mete el pago 
        //                                                    pagoProcesado.CreditCards.CreditCard = 1;
        //                                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
        //                                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
        //                                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
        //                                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
        //                                                    pagoProcesado.CreditCards.CreditAcct = Cuenta;
        //                                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);
        //                                                    contador++;

        //                                                }
        //                                            }


        //                                            if (SumatoriaTransferencia > 0)
        //                                            {
        //                                                var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
        //                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

        //                                                pagoProcesado.TransferAccount = Cuenta;
        //                                                pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
        //                                                pagoProcesado.TransferReference = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
        //                                                pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
        //                                            }

        //                                            var respuestaPago = pagoProcesado.Add();
        //                                            if (respuestaPago == 0)
        //                                            {
        //                                                pagoColonesProcesado = true;

        //                                            }
        //                                            else
        //                                            {
        //                                                var error = "Hubo un error en el pago de la factura #" + Documento.id + " -> " + Conexion.Company.GetLastErrorDescription();
        //                                                BitacoraErrores be = new BitacoraErrores();
        //                                                be.Descripcion = error;
        //                                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
        //                                                be.Fecha = DateTime.Now;
        //                                                be.JSON = JsonConvert.SerializeObject(documentoSAP);
        //                                                db.BitacoraErrores.Add(be);
        //                                                db.SaveChanges();
        //                                                Conexion.Desconectar();

        //                                            }
        //                                        }
        //                                        catch (Exception ex)
        //                                        {

        //                                            BitacoraErrores be = new BitacoraErrores();
        //                                            be.Descripcion = ex.Message;
        //                                            be.StrackTrace = ex.StackTrace;
        //                                            be.Fecha = DateTime.Now;
        //                                            be.JSON = JsonConvert.SerializeObject(ex);
        //                                            db.BitacoraErrores.Add(be);
        //                                            db.SaveChanges();
        //                                            Conexion.Desconectar();
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        pagoColonesProcesado = true;

        //                                    }


        //                                    if (MetodosPagosDolares.Count() > 0)
        //                                    {
        //                                        try
        //                                        {


        //                                            var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
        //                                            pagoProcesado.DocType = BoRcptTypes.rCustomer;
        //                                            pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
        //                                            pagoProcesado.DocDate = Documento.Fecha;
        //                                            pagoProcesado.DueDate = DateTime.Now;
        //                                            pagoProcesado.TaxDate = DateTime.Now;
        //                                            pagoProcesado.VatDate = DateTime.Now;
        //                                            pagoProcesado.Remarks = "Pago procesado por NOVAPOS";
        //                                            pagoProcesado.CounterReference = "APP FAC" + Documento.id;
        //                                            pagoProcesado.DocCurrency = param.MonedaDolar;
        //                                            pagoProcesado.HandWritten = BoYesNoEnum.tNO;
        //                                            pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
        //                                            pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);


        //                                            var SumatoriaPagod = MetodosPagosDolares.Sum(a => a.Monto);
        //                                            if (param.Pais == "C")
        //                                            {
        //                                                pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagod);
        //                                            }
        //                                            if (param.Pais == "P")
        //                                            {
        //                                                pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagod);
        //                                            }
        //                                            pagoProcesado.Series = Sucursal.SeriePago;//154; 161;


        //                                            var SumatoriaEfectivo = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
        //                                            var PagosTarjetas = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList();//.Sum(a => a.Monto);
        //                                            var SumatoriaTransferencia = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

        //                                            if (SumatoriaEfectivo > 0)
        //                                            {
        //                                                var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
        //                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


        //                                                pagoProcesado.CashAccount = Cuenta;
        //                                                pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

        //                                            }


        //                                            foreach (var item in PagosTarjetas)
        //                                            {

        //                                                if (item.Monto > 0)
        //                                                {
        //                                                    var idcuenta = item.idCuentaBancaria;
        //                                                    var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
        //                                                    if (contador > 0)
        //                                                    {
        //                                                        pagoProcesado.CreditCards.Add();
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        pagoProcesado.CreditCards.SetCurrentLine(contador);

        //                                                    }
        //                                                    pagoProcesado.CreditCards.CardValidUntil = new DateTime(Documento.Fecha.Year, Documento.Fecha.Month, 28); //Fecha en la que se mete el pago 
        //                                                    pagoProcesado.CreditCards.CreditCard = 1;
        //                                                    pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
        //                                                    pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
        //                                                    pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
        //                                                    pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
        //                                                    pagoProcesado.CreditCards.CreditAcct = Cuenta;
        //                                                    pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);

        //                                                    contador++;
        //                                                }
        //                                            }



        //                                            if (SumatoriaTransferencia > 0)
        //                                            {
        //                                                var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
        //                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


        //                                                pagoProcesado.TransferAccount = Cuenta;
        //                                                pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
        //                                                pagoProcesado.TransferReference = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
        //                                                pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
        //                                            }
        //                                            var respuestaPago = pagoProcesado.Add();
        //                                            if (respuestaPago == 0)
        //                                            {
        //                                                pagoDolaresProcesado = true;

        //                                            }
        //                                            else
        //                                            {
        //                                                var error = "Hubo un error en el pago de la factura # " + Documento.id + " -> " + Conexion.Company.GetLastErrorDescription();
        //                                                BitacoraErrores be = new BitacoraErrores();
        //                                                be.Descripcion = error;
        //                                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
        //                                                be.Fecha = DateTime.Now;
        //                                                be.JSON = JsonConvert.SerializeObject(documentoSAP);
        //                                                db.BitacoraErrores.Add(be);
        //                                                db.SaveChanges();
        //                                                Conexion.Desconectar();

        //                                            }
        //                                        }
        //                                        catch (Exception ex)
        //                                        {

        //                                            BitacoraErrores be = new BitacoraErrores();
        //                                            be.Descripcion = ex.Message;
        //                                            be.StrackTrace = ex.StackTrace;
        //                                            be.Fecha = DateTime.Now;
        //                                            be.JSON = JsonConvert.SerializeObject(ex);
        //                                            db.BitacoraErrores.Add(be);
        //                                            db.SaveChanges();
        //                                            Conexion.Desconectar();
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        pagoDolaresProcesado = true;

        //                                    }

        //                                    if (pagoColonesProcesado && pagoDolaresProcesado)
        //                                    {
        //                                        db.Entry(Documento).State = EntityState.Modified;
        //                                        Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
        //                                        Documento.PagoProcesadaSAP = true;
        //                                        db.SaveChanges();
        //                                    }


        //                                }
        //                                catch (Exception ex)
        //                                {

        //                                    BitacoraErrores be = new BitacoraErrores();
        //                                    be.Descripcion = ex.Message;
        //                                    be.StrackTrace = ex.StackTrace;
        //                                    be.Fecha = DateTime.Now;
        //                                    be.JSON = JsonConvert.SerializeObject(ex);
        //                                    db.BitacoraErrores.Add(be);
        //                                    db.SaveChanges();
        //                                }
        //                            }
        //                            else
        //                            {
        //                                try
        //                                {


        //                                    //meter los metodos de pago
        //                                    var Fecha = Documento.Fecha.Date;
        //                                    var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
        //                                    var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id).ToList();


        //                                    var SumatoriaPagoColones = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Moneda == "CRC" && a.Monto > 0).FirstOrDefault() == null ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Moneda == "CRC" && a.Monto > 0).Sum(a => a.Monto);
        //                                    var SumatoriaPagoDolares = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Moneda != "CRC" && a.Monto > 0).FirstOrDefault() == null ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Moneda != "CRC" && a.Monto > 0).Sum(a => a.Monto);
        //                                    bool pagoColonesProcesado = false;
        //                                    bool pagoDolaresProcesado = false;



        //                                    if (SumatoriaPagoColones > 0)
        //                                    {
        //                                        try
        //                                        {
        //                                            var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
        //                                            pagoProcesado.DocType = BoRcptTypes.rCustomer;
        //                                            pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
        //                                            pagoProcesado.DocDate = Documento.Fecha;
        //                                            pagoProcesado.DueDate = DateTime.Now;
        //                                            pagoProcesado.TaxDate = DateTime.Now;
        //                                            pagoProcesado.VatDate = DateTime.Now;
        //                                            pagoProcesado.Remarks = "Pago procesado por NOVAPOS";
        //                                            pagoProcesado.CounterReference = "APP FAC" + Documento.id;
        //                                            pagoProcesado.DocCurrency = param.MonedaLocal;
        //                                            pagoProcesado.HandWritten = BoYesNoEnum.tNO;
        //                                            //ligar la factura con el pago 

        //                                            pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
        //                                            pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);
        //                                            if (Documento.Moneda != "CRC")
        //                                            {
        //                                                var SumatoriaPagoColones2 = SumatoriaPagoColones / TipoCambio.TipoCambio;
        //                                                pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones2);
        //                                            }
        //                                            else
        //                                            {
        //                                                pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

        //                                            }


        //                                            var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "CRC").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "CRC").FirstOrDefault().CuentaSAP;




        //                                            pagoProcesado.CashSum = Convert.ToDouble(SumatoriaPagoColones);
        //                                            pagoProcesado.Series = Sucursal.SeriePago;//154; 161;
        //                                            pagoProcesado.CashAccount = Cuenta;

        //                                            var respuestaPago = pagoProcesado.Add();
        //                                            if (respuestaPago == 0)
        //                                            {
        //                                                pagoColonesProcesado = true;
        //                                                //db.Entry(Documento).State = EntityState.Modified;
        //                                                //Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
        //                                                //Documento.PagoProcesadaSAP = true;
        //                                                //db.SaveChanges();
        //                                            }
        //                                            else
        //                                            {
        //                                                var error = "Hubo un error en el pago de la factura #" + Documento.id + " -> " + Conexion.Company.GetLastErrorDescription();
        //                                                BitacoraErrores be = new BitacoraErrores();
        //                                                be.Descripcion = error;
        //                                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
        //                                                be.Fecha = DateTime.Now;
        //                                                be.JSON = JsonConvert.SerializeObject(documentoSAP);
        //                                                db.BitacoraErrores.Add(be);
        //                                                db.SaveChanges();
        //                                                Conexion.Desconectar();

        //                                            }
        //                                        }
        //                                        catch (Exception ex)
        //                                        {
        //                                            BitacoraErrores be = new BitacoraErrores();
        //                                            be.Descripcion = ex.Message;
        //                                            be.StrackTrace = ex.StackTrace;
        //                                            be.Fecha = DateTime.Now;
        //                                            be.JSON = JsonConvert.SerializeObject(ex);
        //                                            db.BitacoraErrores.Add(be);
        //                                            db.SaveChanges();
        //                                            Conexion.Desconectar();

        //                                        }

        //                                    }
        //                                    else
        //                                    {
        //                                        pagoColonesProcesado = true;

        //                                    }

        //                                    if (SumatoriaPagoDolares > 0)
        //                                    {
        //                                        try
        //                                        {
        //                                            var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
        //                                            pagoProcesado.DocType = BoRcptTypes.rCustomer;
        //                                            // pagoProcesado.PayToBankAccountNo = "N";
        //                                            pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
        //                                            pagoProcesado.DocDate = Documento.Fecha;
        //                                            pagoProcesado.DueDate = DateTime.Now;
        //                                            pagoProcesado.TaxDate = DateTime.Now;
        //                                            pagoProcesado.VatDate = DateTime.Now;
        //                                            pagoProcesado.Remarks = "Pago procesado por NOVAPOS";
        //                                            pagoProcesado.CounterReference = "APP FAC" + Documento.id;
        //                                            pagoProcesado.DocCurrency = param.MonedaDolar;
        //                                            pagoProcesado.HandWritten = BoYesNoEnum.tNO;
        //                                            pagoProcesado.LocalCurrency = BoYesNoEnum.tNO;
        //                                            //ligar la factura con el pago 

        //                                            pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
        //                                            pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);
        //                                            //  pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoDolares);
        //                                            //if (Documento.Moneda != "CRC")
        //                                            //{
        //                                            //    SumatoriaPagoColones = SumatoriaPagoColones / TipoCambio.TipoCambio;
        //                                            //}
        //                                            if (param.Pais == "C")
        //                                            {
        //                                                pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoDolares);
        //                                                pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoDolares * TipoCambio.TipoCambio);

        //                                            }
        //                                            if (param.Pais == "P")
        //                                            {
        //                                                pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoDolares);
        //                                            }


        //                                            pagoProcesado.Invoices.Add();


        //                                            //var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault().CuentaSAP;

        //                                            var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault().CuentaSAP;
        //                                            pagoProcesado.Series = Sucursal.SeriePago;//154; 161;
        //                                            pagoProcesado.CashSum = Convert.ToDouble(SumatoriaPagoDolares);
        //                                            pagoProcesado.CashAccount = Cuenta;




        //                                            var respuestaPago = pagoProcesado.Add();
        //                                            if (respuestaPago == 0)
        //                                            {
        //                                                pagoDolaresProcesado = true;
        //                                                //db.Entry(Documento).State = EntityState.Modified;
        //                                                //Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
        //                                                //Documento.PagoProcesadaSAP = true;
        //                                                //db.SaveChanges();
        //                                            }
        //                                            else
        //                                            {
        //                                                var error = "Hubo un error en el pago de la factura # " + Documento.id + " -> " + Conexion.Company.GetLastErrorDescription();
        //                                                BitacoraErrores be = new BitacoraErrores();
        //                                                be.Descripcion = error;
        //                                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
        //                                                be.Fecha = DateTime.Now;
        //                                                be.JSON = JsonConvert.SerializeObject(documentoSAP);
        //                                                db.BitacoraErrores.Add(be);
        //                                                db.SaveChanges();
        //                                                Conexion.Desconectar();

        //                                            }
        //                                        }
        //                                        catch (Exception ex)
        //                                        {

        //                                            BitacoraErrores be = new BitacoraErrores();
        //                                            be.Descripcion = ex.Message;
        //                                            be.StrackTrace = ex.StackTrace;
        //                                            be.Fecha = DateTime.Now;
        //                                            be.JSON = JsonConvert.SerializeObject(ex);
        //                                            db.BitacoraErrores.Add(be);
        //                                            db.SaveChanges();
        //                                            Conexion.Desconectar();

        //                                        }

        //                                    }
        //                                    else
        //                                    {
        //                                        pagoDolaresProcesado = true;

        //                                    }

        //                                    if (pagoColonesProcesado && pagoDolaresProcesado)
        //                                    {
        //                                        db.Entry(Documento).State = EntityState.Modified;
        //                                        Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
        //                                        Documento.PagoProcesadaSAP = true;
        //                                        db.SaveChanges();
        //                                    }




        //                                }

        //                                catch (Exception ex)
        //                                {

        //                                    BitacoraErrores be = new BitacoraErrores();
        //                                    be.Descripcion = ex.Message;
        //                                    be.StrackTrace = ex.StackTrace;
        //                                    be.Fecha = DateTime.Now;
        //                                    be.JSON = JsonConvert.SerializeObject(ex);
        //                                    db.BitacoraErrores.Add(be);
        //                                    db.SaveChanges();

        //                                }
        //                            }

        //                        }



        //                        Conexion.Desconectar();

        //                    }
        //                    else
        //                    {
        //                        var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
        //                        BitacoraErrores be = new BitacoraErrores();
        //                        be.Descripcion = error;
        //                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
        //                        be.Fecha = DateTime.Now;
        //                        be.JSON = JsonConvert.SerializeObject(documentoSAP);
        //                        db.BitacoraErrores.Add(be);
        //                        db.SaveChanges();
        //                        Conexion.Desconectar();
        //                        throw new Exception(be.Descripcion);


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
        //                    Conexion.Desconectar();
        //                    throw new Exception(be.Descripcion);

        //                }
        //            }


                





        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK);

        //    }
        //    catch (Exception ex)
        //    {

        //        ModelCliente db2 = new ModelCliente();
        //        BitacoraErrores be = new BitacoraErrores();
        //        be.Descripcion = ex.Message;
        //        be.StrackTrace = ex.StackTrace;
        //        be.Fecha = DateTime.Now;
        //        be.JSON = JsonConvert.SerializeObject(ex);
        //        db2.BitacoraErrores.Add(be);
        //        db2.SaveChanges();
        //        Conexion.Desconectar();

        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
        //    }
        //}
    }
}