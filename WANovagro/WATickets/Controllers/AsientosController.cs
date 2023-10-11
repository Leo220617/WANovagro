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
    public class AsientosController : ApiController
    {

        ModelCliente db = new ModelCliente();
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
                var Asientos = db.Asientos.Select(a => new
                {
                    a.id,
                    a.idUsuario,
                    a.idCaja,
                    a.Fecha,
                    a.CodSuc,
                    a.idCuentaDebito,
                    a.idCuentaCredito,
                    a.Referencia,
                    a.ProcesadoSAP,
                    a.Credito,
                    a.Debito,
                    a.DocEntry,
                 




                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Asientos = Asientos.Where(a => a.CodSuc == filtro.Texto).ToList();
                }

                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Asientos = Asientos.Where(a => a.idUsuario == filtro.Codigo2).ToList();
                }


                if (filtro.Codigo3 > 0)
                {
                    Asientos = Asientos.Where(a => a.idCaja == filtro.Codigo3).ToList();

                }

                if (filtro.Procesado != null && filtro.Activo) //recordar poner el filtro.activo en novapp
                {
                    Asientos = Asientos.Where(a => a.ProcesadoSAP == filtro.Procesado).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Asientos);
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
        [Route("api/Asientos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Asientos asientos = db.Asientos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, asientos);
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
        [Route("api/Asientos/SincronizarSAPMasivo")]
        public HttpResponseMessage GetMasivo()
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                var AsientoSP = db.Asientos.Where(a => a.ProcesadoSAP == false).ToList();
               

                foreach (var item2 in AsientoSP)
                {
                    Asientos Asiento = db.Asientos.Where(a => a.id == item2.id).FirstOrDefault();
                    var Sucursal = db.Sucursales.Where(a => a.CodSuc == Asiento.CodSuc).FirstOrDefault();

                    if (!Asiento.ProcesadoSAP)
                    {

                        var asientoSAP = (SAPbobsCOM.JournalEntries)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);

                        //Encabezado

                        //asientoSAP.DocObjectCode = BoObjectTypes.oJournalEntries;
                        asientoSAP.ReferenceDate = Asiento.Fecha;
                        asientoSAP.DueDate = Asiento.Fecha;
                        asientoSAP.TaxDate = Asiento.Fecha;

                        asientoSAP.Series = 19;
                        


                        asientoSAP.Reference = Asiento.Referencia;
                       

                        int z = 0;
                     if(z == 0) { 
                        asientoSAP.Lines.SetCurrentLine(z);

                        asientoSAP.Lines.AccountCode = db.CuentasBancarias.Where(a => a.id == Asiento.idCuentaCredito).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == Asiento.idCuentaCredito).FirstOrDefault().CuentaSAP;
                            
                       
                            asientoSAP.Lines.Credit = Convert.ToDouble(Asiento.Credito);
                           //asientoSAP.Lines.CreditSys = Convert.ToDouble(Asiento.Debito);
                            asientoSAP.Lines.FCCurrency = "USD";
                            asientoSAP.Lines.FCCredit = Convert.ToDouble(Asiento.Debito);



                            //Normas de reparto
                            if (param.CostingCode != "N" && param.CostingCode2 != "N" && param.CostingCode3 != "N")
                        {
                            asientoSAP.Lines.CostingCode = param.CostingCode;
                            asientoSAP.Lines.CostingCode2 = param.CostingCode2;
                            asientoSAP.Lines.CostingCode3 = param.CostingCode3;
                        }
                        else
                        {
                            switch (Sucursal.Dimension)
                            {
                                case 1:
                                    {
                                        asientoSAP.Lines.CostingCode = Sucursal.NormaReparto;

                                        break;
                                    }
                                case 2:
                                    {
                                        asientoSAP.Lines.CostingCode2 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 3:
                                    {
                                        asientoSAP.Lines.CostingCode3 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 4:
                                    {
                                        asientoSAP.Lines.CostingCode4 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 5:
                                    {
                                        asientoSAP.Lines.CostingCode5 = Sucursal.NormaReparto;
                                        break;
                                    }

                            }
                        }



                        asientoSAP.Lines.Add();

                        z++;
                        }
                        asientoSAP.Lines.SetCurrentLine(z);

                        asientoSAP.Lines.AccountCode = db.CuentasBancarias.Where(a => a.id == Asiento.idCuentaDebito).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == Asiento.idCuentaDebito).FirstOrDefault().CuentaSAP;
                        asientoSAP.Lines.Debit = Convert.ToDouble(Asiento.Credito);
                        //asientoSAP.Lines.DebitSys = Convert.ToDouble(Asiento.Debito);
                        asientoSAP.Lines.FCDebit = Convert.ToDouble(Asiento.Debito);
                        asientoSAP.Lines.FCCurrency = "USD";



                        //Normas de reparto
                        if (param.CostingCode != "N" && param.CostingCode2 != "N" && param.CostingCode3 != "N")
                        {
                            asientoSAP.Lines.CostingCode = param.CostingCode;
                            asientoSAP.Lines.CostingCode2 = param.CostingCode2;
                            asientoSAP.Lines.CostingCode3 = param.CostingCode3;
                        }
                        else
                        {
                            switch (Sucursal.Dimension)
                            {
                                case 1:
                                    {
                                        asientoSAP.Lines.CostingCode = Sucursal.NormaReparto;

                                        break;
                                    }
                                case 2:
                                    {
                                        asientoSAP.Lines.CostingCode2 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 3:
                                    {
                                        asientoSAP.Lines.CostingCode3 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 4:
                                    {
                                        asientoSAP.Lines.CostingCode4 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 5:
                                    {
                                        asientoSAP.Lines.CostingCode5 = Sucursal.NormaReparto;
                                        break;
                                    }

                            }
                        }



                        asientoSAP.Lines.Add();


                        var respuesta = asientoSAP.Add();
                        if (respuesta == 0) //se creo exitorsamente 
                        {
                            db.Entry(Asiento).State = EntityState.Modified;
                            Asiento.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                            Asiento.ProcesadoSAP = true;
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
                            be.JSON = JsonConvert.SerializeObject(Asiento);
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();


                        }


                    }
                    else
                    {
                        throw new Exception("Este asiento ya fue procesado");
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

        [Route("api/Asientos/SincronizarSAP")]
        public HttpResponseMessage GetExtraeDatos([FromUri] int id)
        {
          try  
            {
                Parametros param = db.Parametros.FirstOrDefault();




                var Asiento = db.Asientos.Where(a => a.id == id).FirstOrDefault();
                var Sucursal = db.Sucursales.Where(a => a.CodSuc == Asiento.CodSuc).FirstOrDefault();

                    if (!Asiento.ProcesadoSAP)
                    {

                        var asientoSAP = (SAPbobsCOM.JournalEntries)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);

                        //Encabezado

                        //asientoSAP.DocObjectCode = BoObjectTypes.oJournalEntries;
                        asientoSAP.ReferenceDate = Asiento.Fecha;
                        asientoSAP.DueDate = Asiento.Fecha;
                        asientoSAP.TaxDate = Asiento.Fecha;

                        asientoSAP.Series = 19;



                        asientoSAP.Reference = Asiento.Referencia;


                        int z = 0;
                        if (z == 0)
                        {
                            asientoSAP.Lines.SetCurrentLine(z);

                            asientoSAP.Lines.AccountCode = db.CuentasBancarias.Where(a => a.id == Asiento.idCuentaCredito).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == Asiento.idCuentaCredito).FirstOrDefault().CuentaSAP;


                            asientoSAP.Lines.Credit = Convert.ToDouble(Asiento.Credito);
                            //asientoSAP.Lines.CreditSys = Convert.ToDouble(Asiento.Debito);
                            asientoSAP.Lines.FCCurrency = "USD";
                            asientoSAP.Lines.FCCredit = Convert.ToDouble(Asiento.Debito);



                            //Normas de reparto
                            if (param.CostingCode != "N" && param.CostingCode2 != "N" && param.CostingCode3 != "N")
                            {
                                asientoSAP.Lines.CostingCode = param.CostingCode;
                                asientoSAP.Lines.CostingCode2 = param.CostingCode2;
                                asientoSAP.Lines.CostingCode3 = param.CostingCode3;
                            }
                            else
                            {
                                switch (Sucursal.Dimension)
                                {
                                    case 1:
                                        {
                                            asientoSAP.Lines.CostingCode = Sucursal.NormaReparto;

                                            break;
                                        }
                                    case 2:
                                        {
                                            asientoSAP.Lines.CostingCode2 = Sucursal.NormaReparto;
                                            break;
                                        }
                                    case 3:
                                        {
                                            asientoSAP.Lines.CostingCode3 = Sucursal.NormaReparto;
                                            break;
                                        }
                                    case 4:
                                        {
                                            asientoSAP.Lines.CostingCode4 = Sucursal.NormaReparto;
                                            break;
                                        }
                                    case 5:
                                        {
                                            asientoSAP.Lines.CostingCode5 = Sucursal.NormaReparto;
                                            break;
                                        }

                                }
                            }



                            asientoSAP.Lines.Add();

                            z++;
                        }
                        asientoSAP.Lines.SetCurrentLine(z);

                        asientoSAP.Lines.AccountCode = db.CuentasBancarias.Where(a => a.id == Asiento.idCuentaDebito).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == Asiento.idCuentaDebito).FirstOrDefault().CuentaSAP;
                        asientoSAP.Lines.Debit = Convert.ToDouble(Asiento.Credito);
                        //asientoSAP.Lines.DebitSys = Convert.ToDouble(Asiento.Debito);
                        asientoSAP.Lines.FCDebit = Convert.ToDouble(Asiento.Debito);
                        asientoSAP.Lines.FCCurrency = "USD";



                        //Normas de reparto
                        if (param.CostingCode != "N" && param.CostingCode2 != "N" && param.CostingCode3 != "N")
                        {
                            asientoSAP.Lines.CostingCode = param.CostingCode;
                            asientoSAP.Lines.CostingCode2 = param.CostingCode2;
                            asientoSAP.Lines.CostingCode3 = param.CostingCode3;
                        }
                        else
                        {
                            switch (Sucursal.Dimension)
                            {
                                case 1:
                                    {
                                        asientoSAP.Lines.CostingCode = Sucursal.NormaReparto;

                                        break;
                                    }
                                case 2:
                                    {
                                        asientoSAP.Lines.CostingCode2 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 3:
                                    {
                                        asientoSAP.Lines.CostingCode3 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 4:
                                    {
                                        asientoSAP.Lines.CostingCode4 = Sucursal.NormaReparto;
                                        break;
                                    }
                                case 5:
                                    {
                                        asientoSAP.Lines.CostingCode5 = Sucursal.NormaReparto;
                                        break;
                                    }

                            }
                        }



                        asientoSAP.Lines.Add();


                        var respuesta = asientoSAP.Add();
                        if (respuesta == 0) //se creo exitorsamente 
                        {
                            db.Entry(Asiento).State = EntityState.Modified;
                            Asiento.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                            Asiento.ProcesadoSAP = true;
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
                            be.JSON = JsonConvert.SerializeObject(Asiento);
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                            Conexion.Desconectar();


                        }


                    }
                    else
                    {
                        throw new Exception("Este asiento ya fue procesado");
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