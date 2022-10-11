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

        public HttpResponseMessage GetAll()
        {
            try
            {
                var Cierre = db.CierreCajas.ToList();


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
        public HttpResponseMessage GetOne([FromUri] int id, DateTime Fecha)
        {
            
            try
            {
                var time = new DateTime(); //01-01-0001
                if (Fecha == time)
                {
                    Fecha = DateTime.Now.Date;

                }

                CierreCajas cierreCajas = db.CierreCajas.Where(a => a.idCaja == id && a.FechaCaja == Fecha).FirstOrDefault();


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
                CierreCajas CierreCajas = db.CierreCajas.Where(a => a.idCaja == cierreCajas.idCaja && a.FechaCaja == cierreCajas.FechaCaja).FirstOrDefault();
                if (CierreCajas != null)
                {
                    var TipoCambio = db.TipoCambios.Where(a => a.Fecha == cierreCajas.FechaCaja && a.Moneda == "USD").FirstOrDefault();
                    db.Entry(CierreCajas).State = System.Data.Entity.EntityState.Modified;
                    
                    CierreCajas.FecUltAct = DateTime.Now;
                  
                    CierreCajas.EfectivoColones = cierreCajas.EfectivoColones;
                    CierreCajas.ChequesColones = cierreCajas.ChequesColones;
                    CierreCajas.TarjetasColones = cierreCajas.TarjetasColones;
                    CierreCajas.OtrosMediosColones = cierreCajas.OtrosMediosColones;
                    CierreCajas.TotalVendidoColones = cierreCajas.TotalVendidoColones;
                    CierreCajas.TotalRegistradoColones = cierreCajas.TotalRegistradoColones;
                    CierreCajas.TotalAperturaColones = cierreCajas.TotalAperturaColones;

                    CierreCajas.EfectivoFC = cierreCajas.EfectivoFC;
                    CierreCajas.ChequesFC = cierreCajas.ChequesFC;
                    CierreCajas.TarjetasFC = cierreCajas.TarjetasFC;
                    CierreCajas.OtrosMediosFC = cierreCajas.OtrosMediosFC;
                    CierreCajas.TotalVendidoFC = cierreCajas.TotalVendidoFC;
                    CierreCajas.TotalRegistradoFC = cierreCajas.TotalRegistradoFC;
                    CierreCajas.TotalAperturaFC = cierreCajas.TotalAperturaFC;
                    CierreCajas.Activo = false;
                    CierreCajas.HoraCierre = DateTime.Now;
                    CierreCajas.TotalizadoMonedas = cierreCajas.TotalVendidoColones + (cierreCajas.TotalVendidoFC * TipoCambio.TipoCambio);
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
    }
}