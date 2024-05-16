using Newtonsoft.Json;
using SAPbobsCOM;
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
                    a.DocEntryInt,
                    a.ProcesadaSAP,
                    a.IntProcesadaSAP,
                    a.idUsuarioCreador,
                    a.idCaja,

                    Detalle = db.DetPagos.Where(b => b.idEncabezado == a.id).ToList(),
                    MetodosPagosAbonos = db.MetodosPagosAbonos.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (!string.IsNullOrEmpty(filtro.Texto) && filtro.Texto != "0")
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
                    a.DocEntryInt,
                    a.IntProcesadaSAP,
                    a.TotalInteres,
                    a.TotalCapital,
                    a.idUsuarioCreador,
                    a.idCaja,

                    Detalle = db.DetPagos.Where(b => b.idEncabezado == a.id).ToList(),
                    MetodosPagosAbonos = db.MetodosPagosAbonos.Where(b => b.idEncabezado == a.id).ToList()

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
                    Pago.TotalCapital = pago.TotalCapital;
                    Pago.Moneda = pago.Moneda;
                    Pago.idUsuarioCreador = pago.idUsuarioCreador;
                    Pago.idCaja = pago.idCaja;


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
                        det.Capital = item.Capital;
                        db.DetPagos.Add(det);
                        db.SaveChanges();
                        i++;
                        var Factura = db.EncDocumentoCredito.Where(a => a.id == det.idEncDocumentoCredito).FirstOrDefault();
                        if (Factura == null)
                        {
                            throw new Exception("Factura " + det.idEncDocumentoCredito + " no se encuentra en las bases de datos");
                        }
                        db.Entry(Factura).State = EntityState.Modified;
                        Factura.Saldo -= det.Capital;
                        if (Factura.Saldo <= 0)
                        {
                            Factura.Status = "C";
                        }
                        db.SaveChanges();


                    }

                    // Se guarda metodo de pago para despues contabilizarlo en el cierre de cajas
                    var time = DateTime.Now.Date;
                    var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == time && a.idCaja == pago.idCaja && a.idUsuario == pago.idUsuarioCreador).FirstOrDefault();
                    var Asiento = db.Asientos.Where(a => a.Fecha == time && a.idCaja == Pago.idCaja && a.idUsuario == Pago.idUsuarioCreador && a.CodSuc == Pago.CodSuc && a.ProcesadoSAP == false).FirstOrDefault();

                    if (pago.MetodosPagosAbonos != null)
                    {

                        foreach (var item in pago.MetodosPagosAbonos.Where(a => a.Metodo != "Pago a Cuenta"))
                        {
                            var Usuario = db.Usuarios.Where(a => a.id == pago.idUsuarioCreador).FirstOrDefault() == null ? "0" : db.Usuarios.Where(a => a.id == pago.idUsuarioCreador).FirstOrDefault().Nombre;
                            BitacoraMovimientos bm = new BitacoraMovimientos();
                            bm.Descripcion = "El usuario " + Usuario + " ha pagado a cuenta del abono #" + pago.id + " con el saldo " + item.Monto + ", favor conciliar en SAP";
                            bm.idUsuario = pago.idUsuarioCreador;
                            bm.Fecha = DateTime.Now;
                            bm.Metodo = "Insercion de Pago a Cuenta de Abono";
                            db.BitacoraMovimientos.Add(bm);
                            db.SaveChanges();

                        }

                        foreach (var item in pago.MetodosPagosAbonos.Where(a => a.Metodo != "Pago a Cuenta"))
                        {
                            MetodosPagosAbonos MetodosPagosAbonos = new MetodosPagosAbonos();
                            MetodosPagosAbonos.idEncabezado = Pago.id;
                            MetodosPagosAbonos.Monto = item.Monto;
                            MetodosPagosAbonos.BIN = item.BIN;
                            MetodosPagosAbonos.NumCheque = item.NumCheque;
                            MetodosPagosAbonos.NumReferencia = item.NumReferencia;
                            MetodosPagosAbonos.Metodo = item.Metodo;
                            MetodosPagosAbonos.idCuentaBancaria = item.idCuentaBancaria;
                            MetodosPagosAbonos.Moneda = item.Moneda;
                            MetodosPagosAbonos.idCaja = Pago.idCaja;
                            MetodosPagosAbonos.idCajero = Pago.idUsuarioCreador;
                            MetodosPagosAbonos.Fecha = DateTime.Now.Date;
                            MetodosPagosAbonos.MonedaVuelto = item.MonedaVuelto;
                            MetodosPagosAbonos.PagadoCon = item.PagadoCon;
                            db.MetodosPagosAbonos.Add(MetodosPagosAbonos);
                            db.SaveChanges();
                            if (CierreCaja != null)
                            {
                                db.Entry(CierreCaja).State = EntityState.Modified;
                                if (MetodosPagosAbonos.Moneda == "CRC")
                                {
                                    switch (item.Metodo)
                                    {
                                        case "Efectivo":
                                            {
                                                if (MetodosPagosAbonos.Moneda != MetodosPagosAbonos.MonedaVuelto)
                                                {
                                                    var FechaX = DateTime.Now.Date;
                                                    var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == FechaX).FirstOrDefault();
                                                    var MontoDevuelto = (MetodosPagosAbonos.PagadoCon - MetodosPagosAbonos.Monto) / TipoCambio.TipoCambio;
                                                    CierreCaja.EfectivoFC -= MontoDevuelto;
                                                    CierreCaja.TotalVendidoFC -= MontoDevuelto;

                                                    CierreCaja.EfectivoColones += MetodosPagosAbonos.PagadoCon;
                                                    CierreCaja.TotalVendidoColones += MetodosPagosAbonos.PagadoCon;
                                                }
                                                else
                                                {
                                                    CierreCaja.EfectivoColones += item.Monto;
                                                    CierreCaja.TotalVendidoColones += item.Monto;
                                                }


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
                                                if (MetodosPagosAbonos.Moneda != MetodosPagosAbonos.MonedaVuelto)
                                                {
                                                    var FechaX2 = DateTime.Now.Date;
                                                    var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == FechaX2).FirstOrDefault();
                                                    var MontoDevuelto = (MetodosPagosAbonos.PagadoCon - MetodosPagosAbonos.Monto) * TipoCambio.TipoCambio;
                                                    CierreCaja.EfectivoColones -= MontoDevuelto;
                                                    CierreCaja.TotalVendidoColones -= MontoDevuelto;

                                                    CierreCaja.EfectivoFC += MetodosPagosAbonos.PagadoCon;
                                                    CierreCaja.TotalVendidoFC += MetodosPagosAbonos.PagadoCon;

                                                    var Debito = MetodosPagosAbonos.PagadoCon - MetodosPagosAbonos.Monto;
                                                    var Credito = Debito * TipoCambio.TipoCambio;

                                                    if (Debito > 0)
                                                    {
                                                        if (Asiento == null)
                                                        {
                                                            Asientos Asientos = new Asientos();
                                                            Asientos.idUsuario = Pago.idUsuarioCreador;
                                                            Asientos.idCaja = Pago.idCaja;
                                                            Asientos.Fecha = DateTime.Now.Date;
                                                            Asientos.CodSuc = Pago.CodSuc;
                                                            Asientos.idCuentaCredito = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == "CRC").FirstOrDefault() == null ? 0 : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == "CRC").FirstOrDefault().id;
                                                            Asientos.idCuentaDebito = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? 0 : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == "USD").FirstOrDefault().id;
                                                            Asientos.Referencia = "Asiento compra de Dolares";
                                                            Asientos.ProcesadoSAP = false;
                                                            Asientos.Credito = Credito;
                                                            Asientos.Debito = Debito;
                                                            db.Asientos.Add(Asientos);
                                                            db.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            db.Entry(Asiento).State = EntityState.Modified;
                                                            Asiento.Credito += Credito;
                                                            Asiento.Debito += Debito;
                                                            db.SaveChanges();
                                                        }

                                                    }



                                                }
                                                else
                                                {
                                                    CierreCaja.EfectivoFC += item.Monto;
                                                    CierreCaja.TotalVendidoFC += item.Monto;
                                                }

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



                        foreach (var item in pago.MetodosPagosAbonos.Where(a => a.Metodo == "Pago a Cuenta"))
                        {
                            var ClienteX = db.Clientes.Where(a => a.id == pago.idCliente).FirstOrDefault();
                            if (ClienteX != null)
                            {
                                db.Entry(ClienteX).State = EntityState.Modified;
                                ClienteX.Saldo += item.Monto;
                                db.SaveChanges();

                            }
                        }

                    }
                    ////

                    t.Commit();

                    //Generar parte de SAP
                    try
                    {
                        if (Pago != null)
                        {

                            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Pago.CodSuc).FirstOrDefault();

                            var ClienteI = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault();
                            if (param.MontosPagosSeparados == true)
                            {
                                try
                                {


                                    var Fecha = Pago.Fecha.Date;
                                    var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                                    var MetodosPagosAbonos = db.MetodosPagosAbonos.Where(a => a.idEncabezado == Pago.id && a.Monto > 0).FirstOrDefault() == null ? new List<MetodosPagosAbonos>() : db.MetodosPagosAbonos.Where(a => a.idEncabezado == Pago.id && a.Monto > 0).ToList();

                                    var MetodosPagosAbonosColones = MetodosPagosAbonos.Where(a => a.Moneda == "CRC").ToList();
                                    var MetodosPagosAbonosDolares = MetodosPagosAbonos.Where(a => a.Moneda == "USD").ToList();

                                    bool pagoColonesProcesado = false;
                                    bool pagoDolaresProcesado = false;
                                    var detalle = db.DetPagos.Where(a => a.idEncabezado == Pago.id).ToList();






                                    if (MetodosPagosAbonosColones.Count() > 0)
                                    {
                                        try
                                        {


                                            var pagoSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                            pagoSAP.DocDate = DateTime.Now;
                                            pagoSAP.DueDate = Pago.FechaVencimiento;
                                            pagoSAP.TaxDate = Pago.FechaContabilizacion;
                                            pagoSAP.VatDate = DateTime.Now;
                                            pagoSAP.CardCode = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault().Codigo;
                                            pagoSAP.Remarks = "Abono procesado por NOVAPOS";
                                            pagoSAP.DocCurrency = Pago.Moneda == "CRC" ? param.MonedaLocal : Pago.Moneda;
                                            pagoSAP.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                                            pagoSAP.CounterReference = "APP ABONO" + Pago.id;
                                            pagoSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "A";

                                            if (Pago.Moneda != "CRC")
                                            {
                                                var SumatoriaPagoColones = MetodosPagosAbonosColones.Sum(a => a.Monto) / TipoCambio.TipoCambio;
                                                pagoSAP.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones);
                                            }
                                            else
                                            {
                                                var SumatoriaPagoColones = MetodosPagosAbonosColones.Sum(a => a.Monto);

                                                pagoSAP.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

                                            }
                                            pagoSAP.Series = Sucursal.SeriePago;//154; 161;


                                            var SumatoriaEfectivo = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                            var SumatoriaTarjeta = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).Sum(a => a.Monto);
                                            var SumatoriaTransferencia = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                            if (SumatoriaEfectivo > 0)
                                            {
                                                var idcuenta = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoSAP.CashAccount = Cuenta;
                                                pagoSAP.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                            }

                                            if (SumatoriaTarjeta > 0)
                                            {
                                                var idcuenta = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

                                                pagoSAP.CreditCards.SetCurrentLine(0);
                                                pagoSAP.CreditCards.CardValidUntil = new DateTime(Pago.Fecha.Year, Pago.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                pagoSAP.CreditCards.CreditCard = 1;
                                                pagoSAP.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                pagoSAP.CreditCards.PaymentMethodCode = 1; //Quemado
                                                pagoSAP.CreditCards.CreditCardNumber = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().BIN; // Ultimos 4 digitos
                                                pagoSAP.CreditCards.VoucherNum = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().NumReferencia;// 
                                                pagoSAP.CreditCards.CreditAcct = Cuenta;
                                                pagoSAP.CreditCards.CreditSum = Convert.ToDouble(SumatoriaTarjeta);


                                            }

                                            if (SumatoriaTransferencia > 0)
                                            {
                                                var idcuenta = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

                                                pagoSAP.TransferAccount = Cuenta;
                                                pagoSAP.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                pagoSAP.TransferReference = MetodosPagosAbonosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                                pagoSAP.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                            }
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

                                                        pagoSAP.Invoices.AppliedFC = Convert.ToDouble(item.Capital);
                                                    }
                                                    else
                                                    {
                                                        pagoSAP.Invoices.SumApplied = Convert.ToDouble(item.Capital);

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
                                                pagoColonesProcesado = true;

                                            }
                                            else
                                            {
                                                var error = "Hubo un error en el pago de la factura #" + Pago.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                BitacoraErrores be = new BitacoraErrores();
                                                be.Descripcion = error;
                                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                                be.Fecha = DateTime.Now;
                                                be.JSON = JsonConvert.SerializeObject(pagoSAP);
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
                                            Conexion.Desconectar();
                                        }
                                    }
                                    else
                                    {
                                        pagoColonesProcesado = true;

                                    }


                                    if (MetodosPagosAbonosDolares.Count() > 0)
                                    {
                                        try
                                        {


                                            var pagoSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                            pagoSAP.DocDate = DateTime.Now;
                                            pagoSAP.DueDate = Pago.FechaVencimiento;
                                            pagoSAP.TaxDate = Pago.FechaContabilizacion;
                                            pagoSAP.VatDate = DateTime.Now;
                                            pagoSAP.CardCode = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault().Codigo;
                                            pagoSAP.Remarks = "Abono procesado por NOVAPOS";
                                            pagoSAP.DocCurrency = "USD";
                                            pagoSAP.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                                            pagoSAP.CounterReference = "APP ABONO" + Pago.id;
                                            pagoSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "A";


                                            var SumatoriaPagod = MetodosPagosAbonosDolares.Sum(a => a.Monto);
                                            pagoSAP.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagod);
                                            pagoSAP.Series = Sucursal.SeriePago;//154; 161;


                                            var SumatoriaEfectivo = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                            var SumatoriaTarjeta = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).Sum(a => a.Monto);
                                            var SumatoriaTransferencia = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                            if (SumatoriaEfectivo > 0)
                                            {
                                                var idcuenta = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoSAP.CashAccount = Cuenta;
                                                pagoSAP.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                            }

                                            if (SumatoriaTarjeta > 0)
                                            {
                                                var idcuenta = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "tarjeta".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoSAP.CreditCards.SetCurrentLine(0);
                                                pagoSAP.CreditCards.CardValidUntil = new DateTime(Pago.Fecha.Year, Pago.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                pagoSAP.CreditCards.CreditCard = 1;
                                                pagoSAP.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                pagoSAP.CreditCards.PaymentMethodCode = 1; //Quemado
                                                pagoSAP.CreditCards.CreditCardNumber = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().BIN; // Ultimos 4 digitos
                                                pagoSAP.CreditCards.VoucherNum = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().NumReferencia;// 
                                                pagoSAP.CreditCards.CreditAcct = Cuenta;
                                                pagoSAP.CreditCards.CreditSum = Convert.ToDouble(SumatoriaTarjeta);


                                            }

                                            if (SumatoriaTransferencia > 0)
                                            {
                                                var idcuenta = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                pagoSAP.TransferAccount = Cuenta;
                                                pagoSAP.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                pagoSAP.TransferReference = MetodosPagosAbonosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                                pagoSAP.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                            }
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

                                                        pagoSAP.Invoices.AppliedFC = Convert.ToDouble(item.Capital);
                                                    }
                                                    else
                                                    {
                                                        pagoSAP.Invoices.SumApplied = Convert.ToDouble(item.Capital);

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
                                                pagoDolaresProcesado = true;

                                            }
                                            else
                                            {
                                                var error = "Hubo un error en el pago de la factura # " + Pago.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                BitacoraErrores be = new BitacoraErrores();
                                                be.Descripcion = error;
                                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                                be.Fecha = DateTime.Now;
                                                be.JSON = JsonConvert.SerializeObject(pagoSAP);
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
                                            Conexion.Desconectar();
                                        }
                                    }
                                    else
                                    {
                                        pagoDolaresProcesado = true;

                                    }

                                    if (pagoColonesProcesado && pagoDolaresProcesado)
                                    {
                                        db.Entry(Pago).State = EntityState.Modified;
                                        Pago.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                        Pago.ProcesadaSAP = true;
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


                                var detalle = db.DetPagos.Where(a => a.idEncabezado == Pago.id).ToList();


                                var pagoSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);

                                //Encabezado 
                                pagoSAP.DocDate = DateTime.Now;
                                pagoSAP.DueDate = Pago.FechaVencimiento;
                                pagoSAP.TaxDate = Pago.FechaContabilizacion;
                                pagoSAP.VatDate = DateTime.Now;
                                pagoSAP.CardCode = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault().Codigo;
                                pagoSAP.Remarks = "Abono procesado por NOVAPOS";
                                pagoSAP.DocCurrency = Pago.Moneda == "CRC" ? param.MonedaLocal : Pago.Moneda;
                                pagoSAP.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                                pagoSAP.CounterReference = "APP ABONO" + Pago.id;
                                var Cuenta2 = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;

                                pagoSAP.CashSum = Convert.ToDouble(Pago.TotalCapital);
                                pagoSAP.Series = Sucursal.SeriePago; //154; 161;
                                pagoSAP.CashAccount = Cuenta2;
                                pagoSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "A";

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

                                            pagoSAP.Invoices.AppliedFC = Convert.ToDouble(item.Capital);
                                        }
                                        else
                                        {
                                            pagoSAP.Invoices.SumApplied = Convert.ToDouble(item.Capital);

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
                                    Conexion.Desconectar();

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
                                    Conexion.Desconectar();

                                }

                                if (Pago.IntProcesadaSAP != true)
                                {
                                    try
                                    {
                                     
                                        var interesSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                        interesSAP.CounterReference = "APP INTÉRES" + Pago.id;
                                        interesSAP.DocDate = DateTime.Now;
                                        interesSAP.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                                        interesSAP.CardCode = ClienteI.Codigo;
                                        interesSAP.CashSum = Convert.ToDouble(Pago.TotalInteres);
                                        var CuentaI = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;
                                        interesSAP.CashAccount = CuentaI;
                                        interesSAP.Remarks = "Interés procesado por NOVAPOS";
                                        interesSAP.DocCurrency = Pago.Moneda == "CRC" ? param.MonedaLocal : Pago.Moneda;
                                        interesSAP.Series = Sucursal.SeriePago; //Crear en parametros
                                        interesSAP.JournalRemarks = Pago.Comentarios;
                                        interesSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "I";

                                        var respuestaInteres = interesSAP.Add();
                                        if (respuestaInteres == 0)
                                        {
                                            db.Entry(Pago).State = EntityState.Modified;
                                            Pago.DocEntryInt = Conexion.Company.GetNewObjectKey().ToString();
                                            Pago.IntProcesadaSAP = true;
                                            db.SaveChanges();
                                            Conexion.Desconectar();

                                        }
                                        else
                                        {
                                            var error = "hubo un error en el interes " + Conexion.Company.GetLastErrorDescription();
                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = error;
                                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                            be.Fecha = DateTime.Now;
                                            be.JSON = JsonConvert.SerializeObject(interesSAP);
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

                                        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
                                    }


                                }

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

                try
                {
                    pago.id = Pago.id;

                }
                catch (Exception)
                {


                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, pago);
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
                    Pago.TotalCapital = pagos.TotalCapital;
                    Pago.Moneda = pagos.Moneda;
                    Pago.idUsuarioCreador = pagos.idUsuarioCreador;
                    Pago.idCaja = pagos.idCaja;
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
                        det.Capital = item.Capital;
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
                    pagoSAP.DocDate = Pago.Fecha;
                    pagoSAP.DueDate = Pago.FechaVencimiento;
                    pagoSAP.TaxDate = Pago.FechaContabilizacion;
                    pagoSAP.VatDate = DateTime.Now;
                    pagoSAP.CardCode = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault().Codigo;
                    pagoSAP.Remarks = "Abono procesado por NOVAPOS";
                    pagoSAP.DocCurrency = Pago.Moneda == "CRC" ? param.MonedaLocal : Pago.Moneda;
                    pagoSAP.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                    pagoSAP.CounterReference = "APP ABONO" + Pago.id;
                    var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;

                    pagoSAP.CashSum = Convert.ToDouble(Pago.TotalCapital);
                    pagoSAP.Series = Sucursal.SeriePago; //154; 161;
                    pagoSAP.CashAccount = Cuenta;
                    pagoSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "A";

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

                                pagoSAP.Invoices.AppliedFC = Convert.ToDouble(item.Capital);
                            }
                            else
                            {
                                pagoSAP.Invoices.SumApplied = Convert.ToDouble(item.Capital);

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
                        Conexion.Desconectar();

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
                        Conexion.Desconectar();

                    }



                }
                if (Pago.IntProcesadaSAP != true)
                {
                    try
                    {

                        var Sucursal = db.Sucursales.Where(a => a.CodSuc == Pago.CodSuc).FirstOrDefault();
                        var Cliente = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault();
                        var interesSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                        interesSAP.CounterReference = "APP INTÉRES" + Pago.id;
                        interesSAP.DocDate = DateTime.Now;
                        interesSAP.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                        interesSAP.CardCode = Cliente.Codigo;
                        interesSAP.CashSum = Convert.ToDouble(Pago.TotalInteres);
                        var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;
                        interesSAP.CashAccount = Cuenta;
                        interesSAP.Remarks = "Interés procesado por NOVAPOS";
                        interesSAP.DocCurrency = Pago.Moneda == "CRC" ? param.MonedaLocal : Pago.Moneda;
                        interesSAP.Series = Sucursal.SeriePago; //Crear en parametros
                        interesSAP.JournalRemarks = Pago.Comentarios;
                        interesSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "I";


                        var respuestaInteres = interesSAP.Add();
                        if (respuestaInteres == 0)
                        {
                            db.Entry(Pago).State = EntityState.Modified;
                            Pago.DocEntryInt = Conexion.Company.GetNewObjectKey().ToString();
                            Pago.IntProcesadaSAP = true;
                            db.SaveChanges();
                            Conexion.Desconectar();

                        }
                        else
                        {
                            var error = "hubo un error en el interes " + Conexion.Company.GetLastErrorDescription();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = error;
                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                            be.Fecha = DateTime.Now;
                            be.JSON = JsonConvert.SerializeObject(interesSAP);
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

                        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
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
                        pagoSAP.DocCurrency = Pago.Moneda == "CRC" ? param.MonedaLocal : Pago.Moneda;
                        pagoSAP.HandWritten = SAPbobsCOM.BoYesNoEnum.tNO;
                        pagoSAP.CounterReference = "APP ABONO" + Pago.id;
                        var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;

                        pagoSAP.CashSum = Convert.ToDouble(Pago.TotalCapital);
                        pagoSAP.Series = Sucursal.SeriePago; //154; 161;
                        pagoSAP.CashAccount = Cuenta;
                        pagoSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "A";

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

                                    pagoSAP.Invoices.AppliedFC = Convert.ToDouble(item.Capital);
                                }
                                else
                                {
                                    pagoSAP.Invoices.SumApplied = Convert.ToDouble(item.Capital);

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

                    if (Pago.IntProcesadaSAP != true)
                    {
                        try
                        {

                            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Pago.CodSuc).FirstOrDefault();
                            var Cliente = db.Clientes.Where(a => a.id == Pago.idCliente).FirstOrDefault();
                            var interesSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                            interesSAP.CounterReference = "APP INTÉRES" + Pago.id;
                            interesSAP.DocDate = DateTime.Now;
                            interesSAP.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                            interesSAP.CardCode = Cliente.Codigo;
                            interesSAP.CashSum = Convert.ToDouble(Pago.TotalInteres);
                            var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Pago.CodSuc && a.Moneda == Pago.Moneda).FirstOrDefault().CuentaSAP;
                            interesSAP.CashAccount = Cuenta;
                            interesSAP.Remarks = "Interés procesado por NOVAPOS";
                            interesSAP.DocCurrency = Pago.Moneda == "CRC" ? param.MonedaLocal : Pago.Moneda;
                            interesSAP.Series = Sucursal.SeriePago; //Crear en parametros
                            interesSAP.JournalRemarks = Pago.Comentarios;
                            interesSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "I";


                            var respuestaInteres = interesSAP.Add();
                            if (respuestaInteres == 0)
                            {
                                db.Entry(Pago).State = EntityState.Modified;
                                Pago.DocEntryInt = Conexion.Company.GetNewObjectKey().ToString();
                                Pago.IntProcesadaSAP = true;
                                db.SaveChanges();
                            }
                            else
                            {
                                var error = "hubo un error en el interes " + Conexion.Company.GetLastErrorDescription();
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = error;
                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                be.Fecha = DateTime.Now;
                                be.JSON = JsonConvert.SerializeObject(interesSAP);
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

                            return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
                        }


                    }

                    else
                    {
                        throw new Exception("Ya existe un Pago con este ID");
                    }


                }
                Conexion.Desconectar();

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
