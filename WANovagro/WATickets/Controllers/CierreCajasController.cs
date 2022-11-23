using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    public class CierreCajasController : ApiController
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
                var Cierre = db.CierreCajas.ToList().Where(a => (filtro.FechaInicial != time ? a.FechaCaja >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.FechaCaja <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos;


                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Cierre = Cierre.Where(a => a.idUsuario == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Cierre = Cierre.Where(a => a.idCaja == filtro.Codigo2).ToList();
                }
                if (filtro.Externo)
                {
                    Cierre = Cierre.Where(a => a.Activo == filtro.Activo).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Cierre);
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
        [Route("api/CierreCajas/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id, DateTime Fecha, int idUsuario)
        {
            
            try
            {
                var time = new DateTime(); //01-01-0001
                if (Fecha == time)
                {
                    Fecha = DateTime.Now.Date;

                }

                CierreCajas cierreCajas = db.CierreCajas.Where(a => a.idCaja == id && a.FechaCaja == Fecha && a.idUsuario == idUsuario).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, cierreCajas);
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
        [Route("api/CierreCajas/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] CierreCajas cierreCajas)
        {
            try
            {
                CierreCajas CierreCajas = db.CierreCajas.Where(a => a.idCaja == cierreCajas.idCaja && a.FechaCaja == cierreCajas.FechaCaja && a.idUsuario == cierreCajas.idUsuario).FirstOrDefault();
                if (CierreCajas != null)
                {
                    var TipoCambio = db.TipoCambios.Where(a => a.Fecha == cierreCajas.FechaCaja && a.Moneda == "USD").FirstOrDefault();
                   
                    db.Entry(CierreCajas).State = System.Data.Entity.EntityState.Modified;
                    
                    CierreCajas.FecUltAct = DateTime.Now;
                  
                    //CierreCajas.EfectivoColones = cierreCajas.EfectivoColones;
                    //CierreCajas.ChequesColones = cierreCajas.ChequesColones;
                    //CierreCajas.TarjetasColones = cierreCajas.TarjetasColones;
                    //CierreCajas.OtrosMediosColones = cierreCajas.OtrosMediosColones;
                    //CierreCajas.TotalVendidoColones = cierreCajas.TotalVendidoColones;
                    CierreCajas.TotalRegistradoColones = cierreCajas.TotalRegistradoColones;
                    CierreCajas.TotalAperturaColones = db.Cajas.Where(a => a.id == cierreCajas.idCaja).FirstOrDefault() == null ? 0 : db.Cajas.Where(a => a.id == cierreCajas.idCaja).FirstOrDefault().MontoAperturaColones;
                    //CierreCajas.TransferenciasColones = cierreCajas.TransferenciasColones;

                    //CierreCajas.EfectivoFC = cierreCajas.EfectivoFC;
                    //CierreCajas.ChequesFC = cierreCajas.ChequesFC;
                    //CierreCajas.TarjetasFC = cierreCajas.TarjetasFC;
                    //CierreCajas.OtrosMediosFC = cierreCajas.OtrosMediosFC;
                   // CierreCajas.TotalVendidoFC = cierreCajas.TotalVendidoFC;
                    CierreCajas.TotalRegistradoFC = cierreCajas.TotalRegistradoFC;
                    CierreCajas.TotalAperturaFC = db.Cajas.Where(a => a.id == cierreCajas.idCaja).FirstOrDefault() == null ? 0 : db.Cajas.Where(a => a.id == cierreCajas.idCaja).FirstOrDefault().MontoAperturaDolares;
                   // CierreCajas.TransferenciasDolares = cierreCajas.TransferenciasDolares;


                    CierreCajas.EfectivoColonesC = cierreCajas.EfectivoColonesC;
                    CierreCajas.ChequesColonesC = cierreCajas.ChequesColonesC;
                    CierreCajas.TarjetasColonesC = cierreCajas.TarjetasColonesC;
                    CierreCajas.OtrosMediosColonesC = cierreCajas.OtrosMediosColonesC;
            
                   
                    CierreCajas.TransferenciasColonesC = cierreCajas.TransferenciasColonesC;

                    CierreCajas.EfectivoFCC = cierreCajas.EfectivoFCC;
                    CierreCajas.ChequesFCC = cierreCajas.ChequesFCC;
                    CierreCajas.TarjetasFCC = cierreCajas.TarjetasFCC;
                    CierreCajas.OtrosMediosFCC = cierreCajas.OtrosMediosFCC;
                 
                    
                    CierreCajas.TransferenciasDolaresC = cierreCajas.TransferenciasDolaresC;

                    CierreCajas.Activo = false;
                    CierreCajas.HoraCierre = DateTime.Now;
                    CierreCajas.TotalizadoMonedas = cierreCajas.TotalRegistradoColones + (cierreCajas.TotalRegistradoFC * TipoCambio.TipoCambio);
                    db.SaveChanges();

                   
                    


                }
                else
                {
                    throw new Exception("No existe un cierre" +
                        " con estas caracteristicas");
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
          [Route("api/CierreCajas/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int idCaja, DateTime Fecha, int idUsuario)
        {
            try
            {
                CierreCajas CierreCajas = db.CierreCajas.Where(a => a.idCaja == idCaja && a.FechaCaja == Fecha && a.idUsuario == idUsuario).FirstOrDefault();

               
                if (CierreCajas != null)
                {
                    db.Entry(CierreCajas).State = EntityState.Modified;


                    if (CierreCajas.Activo)
                    {

                        CierreCajas.Activo = false;

                    }
                    else
                    {

                        CierreCajas.Activo = true;
                    }




                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("No existe un Cierre Cajas con este ID");
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