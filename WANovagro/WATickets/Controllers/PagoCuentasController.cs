using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models;
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
        public HttpResponseMessage Post([FromBody] PagoCuentas cuenta)
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

                    db.PagoCuentas.Add(PagoCuenta);
                    db.SaveChanges();

                    // Se guarda metodo de pago para despues contabilizarlo en el cierre de cajas
                    MetodosPagos MetodosPagos = new MetodosPagos();
                    MetodosPagos.idEncabezado = 0;
                    MetodosPagos.Monto = PagoCuenta.Total;
                    MetodosPagos.BIN = PagoCuenta.id.ToString();
                    MetodosPagos.NumCheque = "";
                    MetodosPagos.NumReferencia = "";
                    MetodosPagos.Metodo = "Pago a Cuenta";
                    var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault() == null ? 0 : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault().id;

                    MetodosPagos.idCuentaBancaria = Cuenta;
                    MetodosPagos.Moneda = PagoCuenta.Moneda;
                    MetodosPagos.idCaja = PagoCuenta.idCaja;
                    MetodosPagos.idCajero = PagoCuenta.idUsuarioCreador;
                    MetodosPagos.Fecha = DateTime.Now.Date;
                    db.MetodosPagos.Add(MetodosPagos);
                    db.SaveChanges();
                    var time = DateTime.Now.Date;

                    var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == time && a.idCaja == PagoCuenta.idCaja && a.idUsuario == PagoCuenta.idUsuarioCreador).FirstOrDefault();
                    if (CierreCaja != null)
                    {
                        db.Entry(CierreCaja).State = EntityState.Modified;
                        if (MetodosPagos.Moneda == "CRC")
                        {
                            switch (MetodosPagos.Metodo)
                            {

                                default:
                                    {
                                        CierreCaja.OtrosMediosColones += MetodosPagos.Monto;
                                        CierreCaja.TotalVendidoColones += MetodosPagos.Monto;

                                        break;
                                    }

                            }

                        }
                        else
                        {
                            switch (MetodosPagos.Metodo)
                            {

                                default:
                                    {
                                        CierreCaja.OtrosMediosFC += MetodosPagos.Monto;
                                        CierreCaja.TotalVendidoFC += MetodosPagos.Monto;

                                        break;
                                    }

                            }
                        }

                        db.SaveChanges();
                        ////
                    }

                    var Cliente = db.Clientes.Where(a => a.id == cuenta.idCliente).FirstOrDefault();
                    if (Cliente == null)
                    {
                        throw new Exception("Cliente " + cuenta.id + " no se encuentra en las bases de datos");

                    }
                    var Fecha = DateTime.Now.Date;
                    var TP = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
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
                            var pagocuentaSAP = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                            pagocuentaSAP.CounterReference = "APP PAGO C" + PagoCuenta.id;
                            pagocuentaSAP.DocDate = DateTime.Now;
                            pagocuentaSAP.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                            pagocuentaSAP.CardCode = ClienteI.Codigo;
                            pagocuentaSAP.CashSum = Convert.ToDouble(PagoCuenta.Total);
                            var CuentaI = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault().CuentaSAP;
                            pagocuentaSAP.CashAccount = CuentaI;
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
                                var error = "hubo un error en el pago a cuenta " + Conexion.Company.GetLastErrorDescription();
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
                    throw new Exception("Ya existe un Pago a cuenta con este ID");
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
                    pagocuentaSAP.DocDate = DateTime.Now;
                    pagocuentaSAP.DocType = SAPbobsCOM.BoRcptTypes.rCustomer;
                    pagocuentaSAP.CardCode = ClienteI.Codigo;
                    pagocuentaSAP.CashSum = Convert.ToDouble(PagoCuenta.Total);
                    var CuentaI = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault().CuentaSAP;
                    pagocuentaSAP.CashAccount = CuentaI;
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
                        var CuentaI = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == PagoCuenta.CodSuc && a.Moneda == PagoCuenta.Moneda).FirstOrDefault().CuentaSAP;
                        pagocuentaSAP.CashAccount = CuentaI;
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