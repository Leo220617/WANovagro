using Newtonsoft.Json;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.APIS;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]

    public class PagoCuentasController : ApiController
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
                var Cuentas = db.PagoCuentas.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.CodSuc,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.FechaContabilizacion,
                    a.Comentarios,
                    a.Total,
                    a.Moneda,
                    a.DocEntry,
                    a.ProcesadaSAP,
                    a.idUsuarioCreador,
                    a.idCaja,
                    a.idCuentaBancaria,
                    MetodosPagos = db.MetodosPagos.Where(b => b.idEncabezado == a.id).ToList(),

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (!string.IsNullOrEmpty(filtro.Texto) && filtro.Texto != "0")
                {
                    Cuentas = Cuentas.Where(a => a.CodSuc == filtro.Texto).ToList();
                }

                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Cuentas = Cuentas.Where(a => a.idCliente == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }

                if (filtro.Procesado != null && filtro.Activo) //recordar poner el filtro.activo en novapp
                {
                    Cuentas = Cuentas.Where(a => a.ProcesadaSAP == filtro.Procesado).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.CardCode)) // esto por ser string
                {
                    Cuentas = Cuentas.Where(a => a.Moneda == filtro.CardCode).ToList();
                }
                if (filtro.Codigo3 > 0)
                {
                    Cuentas = Cuentas.Where(a => a.idCaja == filtro.Codigo3).ToList();

                }
                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Cuentas = Cuentas.Where(a => a.idUsuarioCreador == filtro.Codigo2).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Cuentas);
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

        [Route("api/PagoCuentas/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Cuenta = db.PagoCuentas.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.CodSuc,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.FechaContabilizacion,
                    a.Comentarios,
                    a.Total,
                    a.Moneda,
                    a.DocEntry,
                    a.ProcesadaSAP,
                    a.idUsuarioCreador,
                    a.idCaja,
                    a.idCuentaBancaria,
                    MetodosPagos = db.MetodosPagos.Where(b => b.idEncabezado == a.id).ToList(),



                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Cuenta);
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

        [Route("api/PagoCuentas/Insertar")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostAsync([FromBody] PagoCuentaViewModel cuenta)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                PagoCuentas PagoCuenta = db.PagoCuentas.Where(a => a.id == cuenta.id).FirstOrDefault();
                if (PagoCuenta == null)
                {
                    PagoCuenta = new PagoCuentas();
                    PagoCuenta.idCliente = cuenta.idCliente;
                    PagoCuenta.CodSuc = cuenta.CodSuc;
                    PagoCuenta.Fecha = DateTime.Now;
                    PagoCuenta.FechaVencimiento = cuenta.FechaVencimiento;
                    PagoCuenta.FechaContabilizacion = cuenta.FechaContabilizacion;
                    PagoCuenta.Comentarios = cuenta.Comentarios;
                    PagoCuenta.Total = cuenta.Total;
                    PagoCuenta.Moneda = cuenta.Moneda;
                    PagoCuenta.idUsuarioCreador = cuenta.idUsuarioCreador;
                    PagoCuenta.idCaja = cuenta.idCaja;
                    var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault() == null ? 0 : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault().id;
                    if (param.MontosPagosSeparados)
                    {
                        PagoCuenta.idCuentaBancaria = cuenta.idCuentaBancaria;
                    }
                    else
                    {
                        PagoCuenta.idCuentaBancaria = Cuenta;
                    }

                    db.PagoCuentas.Add(PagoCuenta);
                    db.SaveChanges();

                    // Se guarda metodo de pago para despues contabilizarlo en el cierre de cajas
                    var time = DateTime.Now.Date;
                    var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == time && a.idCaja == cuenta.idCaja && a.idUsuario == cuenta.idUsuarioCreador).FirstOrDefault();
                    var Asiento = db.Asientos.Where(a => a.Fecha == time && a.idCaja == PagoCuenta.idCaja && a.idUsuario == PagoCuenta.idUsuarioCreador && a.CodSuc == PagoCuenta.CodSuc && a.ProcesadoSAP == false).FirstOrDefault();

                    if (cuenta.MetodosPagos != null)
                    {

                        foreach (var item in cuenta.MetodosPagos.Where(a => a.Metodo != "Pago a Cuenta"))
                        {
                            var Usuario = db.Usuarios.Where(a => a.id == cuenta.idUsuarioCreador).FirstOrDefault() == null ? "0" : db.Usuarios.Where(a => a.id == cuenta.idUsuarioCreador).FirstOrDefault().Nombre;
                            BitacoraMovimientos bm = new BitacoraMovimientos();
                            bm.Descripcion = "El usuario " + Usuario + " ha pagado a cuenta la factura #" + cuenta.id + " con el saldo " + item.Monto + ", favor conciliar en SAP";
                            bm.idUsuario = cuenta.idUsuarioCreador;
                            bm.Fecha = DateTime.Now;
                            bm.Metodo = "Insercion de Pago a Cuenta de PagoCuenta";
                            db.BitacoraMovimientos.Add(bm);
                            db.SaveChanges();

                        }

                        foreach (var item in cuenta.MetodosPagos.Where(a => a.Metodo != "Pago a Cuenta"))
                        {
                            MetodosPagos MetodosPagos = new MetodosPagos();
                            MetodosPagos.idEncabezado = PagoCuenta.id;
                            MetodosPagos.Monto = item.Monto;
                            MetodosPagos.BIN = item.BIN;
                            MetodosPagos.NumCheque = item.NumCheque;
                            MetodosPagos.NumReferencia = item.NumReferencia;
                            MetodosPagos.Metodo = item.Metodo;
                            MetodosPagos.idCuentaBancaria = item.idCuentaBancaria;
                            MetodosPagos.Moneda = item.Moneda;
                            MetodosPagos.idCaja = PagoCuenta.idCaja;
                            MetodosPagos.idCajero = PagoCuenta.idUsuarioCreador;
                            MetodosPagos.Fecha = DateTime.Now.Date;
                            MetodosPagos.MonedaVuelto = item.MonedaVuelto;
                            MetodosPagos.PagadoCon = item.PagadoCon;
                            db.MetodosPagos.Add(MetodosPagos);
                            db.SaveChanges();
                            if (CierreCaja != null)
                            {
                                db.Entry(CierreCaja).State = EntityState.Modified;
                                if (MetodosPagos.Moneda == "CRC")
                                {
                                    switch (item.Metodo)
                                    {
                                        case "Efectivo":
                                            {
                                                if (MetodosPagos.Moneda != MetodosPagos.MonedaVuelto)
                                                {
                                                    var FechaX = DateTime.Now.Date;
                                                    var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == FechaX).FirstOrDefault();
                                                    var MontoDevuelto = (MetodosPagos.PagadoCon - MetodosPagos.Monto) / TipoCambio.TipoCambio;
                                                    CierreCaja.EfectivoFC -= MontoDevuelto;
                                                    CierreCaja.TotalVendidoFC -= MontoDevuelto;

                                                    CierreCaja.EfectivoColones += MetodosPagos.PagadoCon;
                                                    CierreCaja.TotalVendidoColones += MetodosPagos.PagadoCon;
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
                                                if (MetodosPagos.Moneda != MetodosPagos.MonedaVuelto)
                                                {
                                                    var FechaX2 = DateTime.Now.Date;
                                                    var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == FechaX2).FirstOrDefault();
                                                    var MontoDevuelto = (MetodosPagos.PagadoCon - MetodosPagos.Monto) * TipoCambio.TipoCambio;
                                                    CierreCaja.EfectivoColones -= MontoDevuelto;
                                                    CierreCaja.TotalVendidoColones -= MontoDevuelto;

                                                    CierreCaja.EfectivoFC += MetodosPagos.PagadoCon;
                                                    CierreCaja.TotalVendidoFC += MetodosPagos.PagadoCon;

                                                    var Debito = MetodosPagos.PagadoCon - MetodosPagos.Monto;
                                                    var Credito = Debito * TipoCambio.TipoCambio;

                                                    if (Debito > 0)
                                                    {
                                                        if (Asiento == null)
                                                        {
                                                            Asientos Asientos = new Asientos();
                                                            Asientos.idUsuario = PagoCuenta.idUsuarioCreador;
                                                            Asientos.idCaja = PagoCuenta.idCaja;
                                                            Asientos.Fecha = DateTime.Now.Date;
                                                            Asientos.CodSuc = PagoCuenta.CodSuc;
                                                            Asientos.idCuentaCredito = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "CRC").FirstOrDefault() == null ? 0 : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "CRC").FirstOrDefault().id;
                                                            Asientos.idCuentaDebito = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? 0 : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "USD").FirstOrDefault().id;
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



                        foreach (var item in cuenta.MetodosPagos.Where(a => a.Metodo == "Pago a Cuenta"))
                        {
                            var ClienteX = db.Clientes.Where(a => a.id == cuenta.idCliente).FirstOrDefault();
                            if (ClienteX != null)
                            {
                                db.Entry(ClienteX).State = EntityState.Modified;
                                ClienteX.Saldo += item.Monto;
                                db.SaveChanges();

                            }
                        }

                    }
                    ////
                }

                var Cliente = db.Clientes.Where(a => a.id == cuenta.idCliente).FirstOrDefault();
                if (Cliente == null)
                {
                    throw new Exception("Cliente " + cuenta.id + " no se encuentra en las bases de datos");

                }
                var FechaS = DateTime.Now.Date;
                var TP = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == FechaS).FirstOrDefault();
                db.Entry(Cliente).State = EntityState.Modified;
                Cliente.Saldo -= cuenta.Moneda == "USD" ? (cuenta.Total * TP.TipoCambio) : cuenta.Total;
                db.SaveChanges();

                t.Commit();

                //Generar parte de SAP


                try
                {
                    if (PagoCuenta != null)
                    {

                        var Sucursal = db.Sucursales.Where(a => a.CodSuc == PagoCuenta.CodSuc).FirstOrDefault();

                        var ClienteI = db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault();
                      
                        //Procesamos el pago


                        if (param.MontosPagosSeparados == true)
                        {
                            try
                            {


                                var Fecha = PagoCuenta.Fecha.Date;
                                var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                                var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == PagoCuenta.id && a.Monto > 0).FirstOrDefault() == null ? new List<MetodosPagos>() : db.MetodosPagos.Where(a => a.idEncabezado == PagoCuenta.id && a.Monto > 0).ToList();

                                var MetodosPagosColones = MetodosPagos.Where(a => a.Moneda == "CRC").ToList();
                                var MetodosPagosDolares = MetodosPagos.Where(a => a.Moneda == "USD").ToList();

                                bool pagoColonesProcesado = false;
                                bool pagoDolaresProcesado = false;







                                if (MetodosPagosColones.Count() > 0)
                                {
                                    try
                                    {


                                        var pagocuentaSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                        pagocuentaSAP.DocType = BoRcptTypes.rCustomer;
                                        pagocuentaSAP.CardCode = db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault().Codigo;
                                        pagocuentaSAP.DocDate = PagoCuenta.Fecha;
                                        pagocuentaSAP.DueDate = DateTime.Now;
                                        pagocuentaSAP.TaxDate = DateTime.Now;
                                        pagocuentaSAP.VatDate = DateTime.Now;
                                        pagocuentaSAP.Remarks = "Pago a cuenta procesado por NOVAPOS";
                                        pagocuentaSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "P";
                                    
                                        pagocuentaSAP.CounterReference = "APP PAGO C" + PagoCuenta.id;
                                        pagocuentaSAP.DocCurrency = param.MonedaLocal;
                                        pagocuentaSAP.HandWritten = BoYesNoEnum.tNO;
                                        pagocuentaSAP.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                        pagocuentaSAP.Invoices.DocEntry = Convert.ToInt32(PagoCuenta.DocEntry);

                                        if (PagoCuenta.Moneda != "CRC")
                                        {
                                            var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto) / TipoCambio.TipoCambio;
                                            pagocuentaSAP.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones);
                                        }
                                        else
                                        {
                                            var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto);

                                            pagocuentaSAP.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

                                        }
                                        pagocuentaSAP.Series = Sucursal.SeriePago;//154; 161;


                                        var SumatoriaEfectivo = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                        var SumatoriaTarjeta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).Sum(a => a.Monto);
                                        var SumatoriaTransferencia = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                        if (SumatoriaEfectivo > 0)
                                        {
                                            var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                            pagocuentaSAP.CashAccount = Cuenta;
                                            pagocuentaSAP.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                        }

                                        if (SumatoriaTarjeta > 0)
                                        {
                                            var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

                                            pagocuentaSAP.CreditCards.SetCurrentLine(0);
                                            pagocuentaSAP.CreditCards.CardValidUntil = new DateTime(PagoCuenta.Fecha.Year, PagoCuenta.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                            pagocuentaSAP.CreditCards.CreditCard = 1;
                                            pagocuentaSAP.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                            pagocuentaSAP.CreditCards.PaymentMethodCode = 1; //Quemado
                                            pagocuentaSAP.CreditCards.CreditCardNumber = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().BIN; // Ultimos 4 digitos
                                            pagocuentaSAP.CreditCards.VoucherNum = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().NumReferencia;// 
                                            pagocuentaSAP.CreditCards.CreditAcct = Cuenta;
                                            pagocuentaSAP.CreditCards.CreditSum = Convert.ToDouble(SumatoriaTarjeta);


                                        }

                                        if (SumatoriaTransferencia > 0)
                                        {
                                            var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

                                            pagocuentaSAP.TransferAccount = Cuenta;
                                            pagocuentaSAP.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                            pagocuentaSAP.TransferReference = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                            pagocuentaSAP.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                        }

                                        var respuestaPago = pagocuentaSAP.Add();
                                        if (respuestaPago == 0)
                                        {
                                            pagoColonesProcesado = true;

                                        }
                                        else
                                        {
                                            var error = "Hubo un error en el pago de la factura #" + PagoCuenta.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = error;
                                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                            be.Fecha = DateTime.Now;
                                            be.JSON = JsonConvert.SerializeObject(pagocuentaSAP);
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


                                if (MetodosPagosDolares.Count() > 0)
                                {
                                    try
                                    {


                                        var pagocuentaSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                        pagocuentaSAP.DocType = BoRcptTypes.rCustomer;
                                        pagocuentaSAP.CardCode = db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault().Codigo;
                                        pagocuentaSAP.DocDate = PagoCuenta.Fecha;
                                        pagocuentaSAP.DueDate = DateTime.Now;
                                        pagocuentaSAP.TaxDate = DateTime.Now;
                                        pagocuentaSAP.VatDate = DateTime.Now;
                                        pagocuentaSAP.Remarks = "Pago procesado por NOVAPOS";
                                         pagocuentaSAP.CounterReference = "APP PAGO C" + PagoCuenta.id;
                                        pagocuentaSAP.DocCurrency = "USD";
                                        pagocuentaSAP.HandWritten = BoYesNoEnum.tNO;
                                        pagocuentaSAP.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                        pagocuentaSAP.Invoices.DocEntry = Convert.ToInt32(PagoCuenta.DocEntry);


                                        var SumatoriaPagod = MetodosPagosDolares.Sum(a => a.Monto);
                                        pagocuentaSAP.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagod);
                                        pagocuentaSAP.Series = Sucursal.SeriePago;//154; 161;


                                        var SumatoriaEfectivo = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                        var SumatoriaTarjeta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).Sum(a => a.Monto);
                                        var SumatoriaTransferencia = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                        if (SumatoriaEfectivo > 0)
                                        {
                                            var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                            pagocuentaSAP.CashAccount = Cuenta;
                                            pagocuentaSAP.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                        }

                                        if (SumatoriaTarjeta > 0)
                                        {
                                            var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "tarjeta".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                            pagocuentaSAP.CreditCards.SetCurrentLine(0);
                                            pagocuentaSAP.CreditCards.CardValidUntil = new DateTime(PagoCuenta.Fecha.Year, PagoCuenta.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                            pagocuentaSAP.CreditCards.CreditCard = 1;
                                            pagocuentaSAP.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                            pagocuentaSAP.CreditCards.PaymentMethodCode = 1; //Quemado
                                            pagocuentaSAP.CreditCards.CreditCardNumber = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().BIN; // Ultimos 4 digitos
                                            pagocuentaSAP.CreditCards.VoucherNum = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).FirstOrDefault().NumReferencia;// 
                                            pagocuentaSAP.CreditCards.CreditAcct = Cuenta;
                                            pagocuentaSAP.CreditCards.CreditSum = Convert.ToDouble(SumatoriaTarjeta);


                                        }

                                        if (SumatoriaTransferencia > 0)
                                        {
                                            var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                            pagocuentaSAP.TransferAccount = Cuenta;
                                            pagocuentaSAP.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                            pagocuentaSAP.TransferReference = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                            pagocuentaSAP.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                        }
                                        var respuestaPago = pagocuentaSAP.Add();
                                        if (respuestaPago == 0)
                                        {
                                            pagoDolaresProcesado = true;

                                        }
                                        else
                                        {
                                            var error = "Hubo un error en el pago de la factura # " + PagoCuenta.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = error;
                                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                            be.Fecha = DateTime.Now;
                                            be.JSON = JsonConvert.SerializeObject(pagocuentaSAP);
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
                                    db.Entry(PagoCuenta).State = EntityState.Modified;
                                    PagoCuenta.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                                    PagoCuenta.ProcesadaSAP = true;
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


                                //meter los metodos de pago
                                var Fecha = PagoCuenta.Fecha.Date;
                                var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                                var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == PagoCuenta.id).ToList();


                                var SumatoriaPagoColones = db.MetodosPagos.Where(a => a.idEncabezado == PagoCuenta.id && a.Moneda == "CRC" && a.Monto > 0).FirstOrDefault() == null ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == PagoCuenta.id && a.Moneda == "CRC" && a.Monto > 0).Sum(a => a.Monto);
                                var SumatoriaPagoDolares = db.MetodosPagos.Where(a => a.idEncabezado == PagoCuenta.id && a.Moneda != "CRC" && a.Monto > 0).FirstOrDefault() == null ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == PagoCuenta.id && a.Moneda != "CRC" && a.Monto > 0).Sum(a => a.Monto);
                                bool pagoColonesProcesado = false;
                                bool pagoDolaresProcesado = false;


                                if (SumatoriaPagoColones > 0)
                                {
                                    try
                                    {
                                        var pagocuentaSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                        pagocuentaSAP.DocType = BoRcptTypes.rCustomer;
                                        pagocuentaSAP.CardCode = db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault().Codigo;
                                        pagocuentaSAP.DocDate = PagoCuenta.Fecha;
                                        pagocuentaSAP.DueDate = DateTime.Now;
                                        pagocuentaSAP.TaxDate = DateTime.Now;
                                        pagocuentaSAP.VatDate = DateTime.Now;

                                         pagocuentaSAP.CounterReference = "APP PAGO C" + PagoCuenta.id;
                                        pagocuentaSAP.DocCurrency = param.MonedaLocal;
                                        pagocuentaSAP.HandWritten = BoYesNoEnum.tNO;
                                        pagocuentaSAP.Remarks = "Pago a cuenta procesado por NOVAPOS";
                                      
                                       
                                    
                                        pagocuentaSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "P";
                                        //ligar la factura con el pago 

                                        pagocuentaSAP.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                        pagocuentaSAP.Invoices.DocEntry = Convert.ToInt32(PagoCuenta.DocEntry);
                                        if (PagoCuenta.Moneda != "CRC")
                                        {
                                            var SumatoriaPagoColones2 = SumatoriaPagoColones / TipoCambio.TipoCambio;
                                            pagocuentaSAP.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones2);
                                        }
                                        else
                                        {
                                            pagocuentaSAP.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

                                        }


                                        var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "CRC").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "CRC").FirstOrDefault().CuentaSAP;




                                        pagocuentaSAP.CashSum = Convert.ToDouble(SumatoriaPagoColones);
                                        pagocuentaSAP.Series = Sucursal.SeriePago;//154; 161;
                                        pagocuentaSAP.CashAccount = Cuenta;

                                        var respuestaPago = pagocuentaSAP.Add();
                                        if (respuestaPago == 0)
                                        {
                                            pagoColonesProcesado = true;
                                            //db.Entry(PagoCuenta).State = EntityState.Modified;
                                            //PagoCuenta.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                            //PagoCuenta.PagoProcesadaSAP = true;
                                            //db.SaveChanges();
                                        }
                                        else
                                        {
                                            var error = "Hubo un error en el pago de la factura #" + PagoCuenta.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = error;
                                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                            be.Fecha = DateTime.Now;
                                            be.JSON = JsonConvert.SerializeObject(pagocuentaSAP);
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

                                if (SumatoriaPagoDolares > 0)
                                {
                                    try
                                    {
                                        var pagocuentaSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                        pagocuentaSAP.DocType = BoRcptTypes.rCustomer;
                                        pagocuentaSAP.CardCode = db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault().Codigo;
                                        pagocuentaSAP.DocDate = PagoCuenta.Fecha;
                                        pagocuentaSAP.DueDate = DateTime.Now;
                                        pagocuentaSAP.TaxDate = DateTime.Now;
                                        pagocuentaSAP.VatDate = DateTime.Now;
                                        pagocuentaSAP.Remarks = "Pago a cuenta procesado por NOVAPOS";



                                        pagocuentaSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "P";
                                        //ligar la factura con el pago 
                                        pagocuentaSAP.CounterReference = "APP PAGO C" + PagoCuenta.id;
                                        pagocuentaSAP.DocCurrency = "USD";
                                        pagocuentaSAP.HandWritten = BoYesNoEnum.tNO;
                                        //ligar la factura con el pago 

                                        pagocuentaSAP.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                        pagocuentaSAP.Invoices.DocEntry = Convert.ToInt32(PagoCuenta.DocEntry);
                                        //  pagocuentaSAP.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoDolares);
                                        //if (PagoCuenta.Moneda != "CRC")
                                        //{
                                        //    SumatoriaPagoColones = SumatoriaPagoColones / TipoCambio.TipoCambio;
                                        //}
                                        pagocuentaSAP.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoDolares);



                                        //var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "USD").FirstOrDefault().CuentaSAP;

                                        var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == "USD").FirstOrDefault().CuentaSAP;
                                        pagocuentaSAP.Series = Sucursal.SeriePago;//154; 161;
                                        pagocuentaSAP.CashSum = Convert.ToDouble(SumatoriaPagoDolares);
                                        pagocuentaSAP.CashAccount = Cuenta;
                                        var respuestaPago = pagocuentaSAP.Add();
                                        if (respuestaPago == 0)
                                        {
                                            pagoDolaresProcesado = true;
                                            //db.Entry(PagoCuenta).State = EntityState.Modified;
                                            //PagoCuenta.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                            //PagoCuenta.PagoProcesadaSAP = true;
                                            //db.SaveChanges();
                                        }
                                        else
                                        {
                                            var error = "Hubo un error en el pago de la factura # " + PagoCuenta.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                            BitacoraErrores be = new BitacoraErrores();
                                            be.Descripcion = error;
                                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                            be.Fecha = DateTime.Now;
                                            be.JSON = JsonConvert.SerializeObject(pagocuentaSAP);
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
                                    db.Entry(PagoCuenta).State = EntityState.Modified;
                                    PagoCuenta.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                                    PagoCuenta.ProcesadaSAP = true;
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


                try
                {
                    cuenta.id = cuenta.id;

                }
                catch (Exception)
                {


                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, cuenta);
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
        [Route("api/PagoCuentas/SincronizarSAP")]
        public HttpResponseMessage GetSincronizar([FromUri] int id)
        {
            try
            {
                var PagoCuenta = db.PagoCuentas.Where(a => a.id == id).FirstOrDefault();
                var param = db.Parametros.FirstOrDefault();

                if (PagoCuenta.ProcesadaSAP != true)
                {


                    var Sucursal = db.Sucursales.Where(a => a.CodSuc == PagoCuenta.CodSuc).FirstOrDefault();

                    var ClienteI = db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault();
                    var pagocuentaSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                    pagocuentaSAP.CounterReference = "APP PAGO C" + PagoCuenta.id;
                    pagocuentaSAP.DocDate = PagoCuenta.Fecha;
                    pagocuentaSAP.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                    pagocuentaSAP.CardCode = ClienteI.Codigo;
                    pagocuentaSAP.CashSum = Convert.ToDouble(PagoCuenta.Total);
                    if (param.MontosPagosSeparados)
                    {
                        pagocuentaSAP.CashAccount = db.CuentasBancarias.Where(a => a.id == PagoCuenta.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == PagoCuenta.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                    }
                    else
                    {
                        var CuentaI = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault().CuentaSAP;
                        pagocuentaSAP.CashAccount = CuentaI;
                    }
                    pagocuentaSAP.Remarks = "Pago a cuenta procesado por NOVAPOS";
                    pagocuentaSAP.DocCurrency = PagoCuenta.Moneda == "CRC" ? param.MonedaLocal : PagoCuenta.Moneda;
                    pagocuentaSAP.Series = Sucursal.SeriePago; //Crear en parametros
                    pagocuentaSAP.JournalRemarks = PagoCuenta.Comentarios;
                    pagocuentaSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "P";





                    var respuestaPago = pagocuentaSAP.Add();
                    if (respuestaPago == 0)
                    {
                        db.Entry(PagoCuenta).State = EntityState.Modified;
                        PagoCuenta.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                        PagoCuenta.ProcesadaSAP = true;
                        db.SaveChanges();
                        Conexion.Desconectar();

                    }
                    else
                    {
                        var error = "hubo un error en el pago a cuenta" + Conexion.Company.GetLastErrorDescription();
                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = error;
                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                        be.Fecha = DateTime.Now;
                        be.JSON = JsonConvert.SerializeObject(pagocuentaSAP);
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();

                    }



                }



                else
                {
                    throw new Exception("Ya existe un Pago a cuenta con este ID");
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
        [Route("api/PagoCuentas/SincronizarSAPMasivo")]
        public HttpResponseMessage GetMasivo()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();

                var PagoCuentasSP = db.PagoCuentas.Where(a => a.ProcesadaSAP == false).ToList();

                foreach (var item2 in PagoCuentasSP)
                {
                    var PagoCuenta = db.PagoCuentas.Where(a => a.id == item2.id).FirstOrDefault();


                    if (PagoCuenta.ProcesadaSAP != true)
                    {


                        var Sucursal = db.Sucursales.Where(a => a.CodSuc == PagoCuenta.CodSuc).FirstOrDefault();

                        var ClienteI = db.Clientes.Where(a => a.id == PagoCuenta.idCliente).FirstOrDefault();
                        var pagocuentaSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                        pagocuentaSAP.CounterReference = "APP PAGO C" + PagoCuenta.id;
                        pagocuentaSAP.DocDate = PagoCuenta.Fecha;
                        pagocuentaSAP.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                        pagocuentaSAP.CardCode = ClienteI.Codigo;
                        pagocuentaSAP.CashSum = Convert.ToDouble(PagoCuenta.Total);
                        if (param.MontosPagosSeparados)
                        {
                            pagocuentaSAP.CashAccount = db.CuentasBancarias.Where(a => a.id == PagoCuenta.idCuentaBancaria).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == PagoCuenta.idCuentaBancaria).FirstOrDefault().CuentaSAP;
                        }
                        else
                        {
                            var CuentaI = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault().CuentaSAP;
                            pagocuentaSAP.CashAccount = CuentaI;
                        }
                        pagocuentaSAP.Remarks = "Pago a cuenta procesado por NOVAPOS";
                        pagocuentaSAP.DocCurrency = PagoCuenta.Moneda == "CRC" ? param.MonedaLocal : PagoCuenta.Moneda;
                        pagocuentaSAP.Series = Sucursal.SeriePago; //Crear en parametros
                        pagocuentaSAP.JournalRemarks = PagoCuenta.Comentarios;
                        pagocuentaSAP.UserFields.Fields.Item("U_DYD_Tipo").Value = "P";





                        var respuestaPago = pagocuentaSAP.Add();
                        if (respuestaPago == 0)
                        {
                            db.Entry(PagoCuenta).State = EntityState.Modified;
                            PagoCuenta.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                            PagoCuenta.ProcesadaSAP = true;
                            db.SaveChanges();

                        }
                        else
                        {
                            var error = "hubo un error en el pago a cuenta " + Conexion.Company.GetLastErrorDescription();
                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = error;
                            be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                            be.Fecha = DateTime.Now;
                            be.JSON = JsonConvert.SerializeObject(pagocuentaSAP);
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }



                    }



                    else
                    {
                        throw new Exception("Ya existe un Pago a Cuenta con este ID");
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