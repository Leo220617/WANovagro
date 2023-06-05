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
    public class DepositosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

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
                var Depositos = db.Depositos.Select(a => new
                {
                    a.id,
                    a.CodSuc,
                    a.idUsuarioCreador,
                    a.idCaja,
                    a.Fecha,
                    a.Series,
                    a.Banco,
                    a.Referencia,
                    a.CuentaInicial,
                    a.CuentaFinal,
                    a.Saldo,
                    a.Comentarios,
                    a.ProcesadoSAP,
                    a.DocEntry,
                    a.Moneda,
                    a.SaldoDisponibleAnterior,




                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Depositos = Depositos.Where(a => a.CodSuc == filtro.Texto).ToList();
                }

                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Depositos = Depositos.Where(a => a.idUsuarioCreador == filtro.Codigo2).ToList();
                }


                if (filtro.Codigo3 > 0)
                {
                    Depositos = Depositos.Where(a => a.idCaja == filtro.Codigo3).ToList();

                }

                if (filtro.Procesado != null && filtro.Activo) //recordar poner el filtro.activo en novapp
                {
                    Depositos = Depositos.Where(a => a.ProcesadoSAP == filtro.Procesado).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Depositos);
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
        [Route("api/Depositos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Depositos depositos = db.Depositos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, depositos);
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
        [Route("api/Depositos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Depositos depositos)
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                Depositos Deposito = db.Depositos.Where(a => a.id == depositos.id).FirstOrDefault();
                if (Deposito == null)
                {
                    Deposito = new Depositos();
                    Deposito.id = depositos.id;
                    Deposito.CodSuc = depositos.CodSuc;
                    Deposito.Moneda = depositos.Moneda;
                    Deposito.Series = param.SerieDeposito; //Crear campo en Parametros
                    Deposito.Fecha = depositos.Fecha;
                    Deposito.Banco = depositos.Banco;
                    Deposito.Referencia = depositos.Referencia;
                    if (Deposito.Referencia == null)
                    {
                        Deposito.Referencia = "";
                    }
                    Deposito.CuentaInicial = depositos.CuentaInicial;
                    Deposito.CuentaFinal = depositos.CuentaFinal;
                    Deposito.Saldo = depositos.Saldo;
                    Deposito.Comentarios = depositos.Comentarios;
                    if (Deposito.Comentarios == null)
                    {
                        Deposito.Comentarios = "";
                    }
                    Deposito.ProcesadoSAP = false;
                    Deposito.idUsuarioCreador = depositos.idUsuarioCreador;
                    Deposito.idCaja = depositos.idCaja;
                    Deposito.SaldoDisponibleAnterior = depositos.SaldoDisponibleAnterior;
                    //Deposito.DocEntry = depositos.DocEntry;
                    db.Depositos.Add(Deposito);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una cuenta con este ID");
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
        [Route("api/Depositos/SincronizarSAP")]
        public HttpResponseMessage GetExtraeDatos([FromUri] int id)
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                var Deposito = db.Depositos.Where(a => a.id == id).FirstOrDefault();
                if (Deposito != null)
                {
                    if (!Deposito.ProcesadoSAP)
                    {
                        SAPbobsCOM.CompanyService oService = Conexion.Company.GetCompanyService();
                        SAPbobsCOM.DepositsService dpService = (SAPbobsCOM.DepositsService)oService.GetBusinessService(SAPbobsCOM.ServiceTypes.DepositsService);
                        SAPbobsCOM.Deposit depositoSAP = (SAPbobsCOM.Deposit)dpService.GetDataInterface(SAPbobsCOM.DepositsServiceDataInterfaces.dsDeposit);


                        depositoSAP.DepositType = SAPbobsCOM.BoDepositTypeEnum.dtCash;
                        depositoSAP.DepositAccountType = SAPbobsCOM.BoDepositAccountTypeEnum.datBankAccount;
                        depositoSAP.DepositAccount = Deposito.CuentaFinal;
                        depositoSAP.DepositDate = Deposito.Fecha;
                        depositoSAP.DepositCurrency = Deposito.Moneda;
                        depositoSAP.Series = param.SerieDeposito;
                        depositoSAP.BankAccountNum = Deposito.CuentaFinal;
                        depositoSAP.AllocationAccount = Deposito.CuentaInicial;
                        depositoSAP.Bank = Deposito.Banco;
                        depositoSAP.BankReference = Deposito.Referencia;
                        depositoSAP.TotalLC = Convert.ToDouble(Deposito.Saldo);
                        depositoSAP.JournalRemarks = Deposito.Comentarios;
                        depositoSAP.BankBranch = Deposito.CodSuc;

                        var respuesta = dpService.AddDeposit(depositoSAP);
                        if (respuesta.DepositNumber != 0)
                        {
                            db.Entry(Deposito).State = EntityState.Modified;
                            Deposito.DocEntry = respuesta.DepositNumber.ToString();
                            Deposito.ProcesadoSAP = true;
                            db.SaveChanges();

                        }
                        else
                        {
                            BitacoraErrores be = new BitacoraErrores();
                            be.Fecha = DateTime.Now;
                            be.Descripcion = respuesta.DepositNumber.ToString();
                            be.JSON = JsonConvert.SerializeObject(respuesta);
                            be.StrackTrace = Conexion.Company.GetLastErrorDescription();
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }

                        Conexion.Desconectar();
                    }
                    else
                    {
                        throw new Exception("Este deposito ya fue procesado");
                    }

                    
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Deposito);
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
                return Request.CreateResponse(System.Net.HttpStatusCode.BadRequest, ex);

            }
        }

     
        }
}