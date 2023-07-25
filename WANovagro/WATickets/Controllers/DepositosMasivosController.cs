using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    public class DepositosMasivosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [HttpGet]
        public HttpResponseMessage GetMasivo()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                var DepositoSP = db.Depositos.Where(a => a.ProcesadoSAP == false).ToList();
               

                foreach (var item2 in DepositoSP)
                {
                    Depositos Deposito = db.Depositos.Where(a => a.id == item2.id).FirstOrDefault();
                    var Sucursal = db.Sucursales.Where(a => a.CodSuc == Deposito.CodSuc).FirstOrDefault();
                    if (!Deposito.ProcesadoSAP)
                    {
                        SAPbobsCOM.CompanyService oService = Conexion.Company.GetCompanyService();
                        SAPbobsCOM.DepositsService dpService = (SAPbobsCOM.DepositsService)oService.GetBusinessService(SAPbobsCOM.ServiceTypes.DepositsService);
                        SAPbobsCOM.Deposit depositoSAP = (SAPbobsCOM.Deposit)dpService.GetDataInterface(SAPbobsCOM.DepositsServiceDataInterfaces.dsDeposit);


                        depositoSAP.DepositType = SAPbobsCOM.BoDepositTypeEnum.dtCash;
                        depositoSAP.DepositAccountType = SAPbobsCOM.BoDepositAccountTypeEnum.datBankAccount;
                        depositoSAP.DepositAccount = Deposito.CuentaFinal;
                        depositoSAP.DepositDate = Deposito.Fecha;
                        depositoSAP.DepositCurrency = Deposito.Moneda == "CRC" ? param.MonedaLocal : Deposito.Moneda;
                        depositoSAP.Series = Sucursal.SerieDeposito;
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
                return Request.CreateResponse(System.Net.HttpStatusCode.BadRequest, ex);
            }
        }
    }
}