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
    public class PreCierresController : ApiController
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
                var Cierre = db.PreCierres.ToList().Where(a => (filtro.FechaInicial != time ? a.FechaCaja >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.FechaCaja <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos;


                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Cierre = Cierre.Where(a => a.idUsuario == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Cierre = Cierre.Where(a => a.idCaja == filtro.Codigo2).ToList();
                }
                if (filtro.Codigo3 > 0) // esto por ser integer
                {
                    Cierre = Cierre.Where(a => a.idUsuario == filtro.Codigo3).ToList();
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

        [Route("api/PreCierres/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {

            try
            {
                PreCierres preCierres = db.PreCierres.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, preCierres);

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

        [Route("api/PreCierres/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] PreCierres cierres)
        {
            try
            {

                PreCierres PreCierre = db.PreCierres.Where(a => a.id == cierres.id).FirstOrDefault();
                var time = DateTime.Now.Date;
                var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == cierres.FechaCaja && a.idCaja == cierres.idCaja && a.idUsuario == cierres.idUsuario).FirstOrDefault();
                var FechaActual = DateTime.Now.Date;
                var TipoCambio = db.TipoCambios.Where(a => a.Fecha == CierreCaja.FechaCaja && a.Moneda == "USD").FirstOrDefault();

                if (PreCierre == null)
                {


                    PreCierre = new PreCierres();
                    PreCierre.id = cierres.id;
                    PreCierre.idCaja = CierreCaja.idCaja;
                    PreCierre.idUsuario = CierreCaja.idUsuario;
                    PreCierre.FechaCaja = FechaActual;
                    CierreCaja.FecUltAct = DateTime.Now;
                    PreCierre.IP = CierreCaja.IP;
                    PreCierre.EfectivoColones = CierreCaja.EfectivoColones;
                    PreCierre.EfectivoFC = CierreCaja.EfectivoFC;
                    PreCierre.ChequesColones = CierreCaja.ChequesColones;
                    PreCierre.ChequesFC = CierreCaja.ChequesFC;
                    PreCierre.TarjetasColones = CierreCaja.TarjetasColones;
                    PreCierre.TarjetasFC = CierreCaja.TarjetasFC;
                    PreCierre.OtrosMediosColones = CierreCaja.OtrosMediosColones;
                    PreCierre.OtrosMediosFC = CierreCaja.OtrosMediosFC;
                    PreCierre.TotalVendidoColones = CierreCaja.TotalVendidoColones;
                    PreCierre.TotalVendidoFC = CierreCaja.TotalVendidoFC;
                    PreCierre.TotalRegistradoColones = CierreCaja.TotalRegistradoColones;
                    PreCierre.TotalRegistradoFC = CierreCaja.TotalRegistradoFC;
                    PreCierre.TotalAperturaColones = CierreCaja.TotalAperturaColones;
                    PreCierre.TransferenciasColones = CierreCaja.TransferenciasColones;
                    PreCierre.TransferenciasDolares = CierreCaja.TransferenciasDolares;
                    PreCierre.TotalAperturaFC = CierreCaja.TotalAperturaFC;


                    PreCierre.EfectivoColonesC = cierres.EfectivoColonesC;
                    PreCierre.EfectivoFCC = cierres.EfectivoFCC;
                    PreCierre.ChequesColonesC = cierres.ChequesColonesC;
                    PreCierre.ChequesFCC = cierres.ChequesFCC;
                    PreCierre.TarjetasColonesC = cierres.TarjetasColonesC;
                    PreCierre.TarjetasFCC = cierres.TarjetasFCC;
                    PreCierre.OtrosMediosColonesC = cierres.OtrosMediosColonesC;
                    PreCierre.OtrosMediosFCC = cierres.OtrosMediosFCC;


                    PreCierre.TransferenciasColonesC = cierres.TransferenciasColonesC;
                    PreCierre.TransferenciasDolaresC = cierres.TransferenciasDolaresC;

                    PreCierre.Activo = false;
                    PreCierre.HoraCierre = DateTime.Now;
                    PreCierre.TotalizadoMonedas = cierres.TotalRegistradoColones + (cierres.TotalRegistradoFC * TipoCambio.TipoCambio);
                    db.PreCierres.Add(PreCierre);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una caja con este ID");
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
