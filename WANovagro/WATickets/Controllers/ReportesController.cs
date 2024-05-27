using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
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
    public class ReportesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();
        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                if (string.IsNullOrEmpty(filtro.CardCode))
                {
                    throw new Exception("Producto no existe");
                }
                 if (filtro.Codigo1 == null || filtro.Codigo1 == 0)
                {
                    throw new Exception("Bodega no existe");
                }
                DateTime time = new DateTime();
                if(filtro.FechaInicial == time)
                {
                    throw new Exception("Fecha no valida");
                }
                    

                var Parametros = db.Parametros.FirstOrDefault();
              
                var SQL = Parametros.SQLReporteVentas;
                string codigoProductoConComillas = $"'{filtro.CardCode}'";

                var FechaInicio = filtro.FechaInicial.Year.ToString() + (filtro.FechaInicial.Month.ToString().Length == 1 ? "0" + filtro.FechaInicial.Month.ToString() : filtro.FechaInicial.Month.ToString()) + (filtro.FechaInicial.Day.ToString().Length == 1 ? "0" + filtro.FechaInicial.Day.ToString() : filtro.FechaInicial.Day.ToString());
                var FechaFinal = filtro.FechaFinal.Year.ToString() + (filtro.FechaFinal.Month.ToString().Length == 1 ? "0" + filtro.FechaFinal.Month.ToString() : filtro.FechaFinal.Month.ToString()) + (filtro.FechaFinal.Day.ToString().Length == 1 ? "0" + filtro.FechaFinal.Day.ToString() : filtro.FechaFinal.Day.ToString());

                FechaInicio = $"'{FechaInicio}'";
                FechaFinal = $"'{FechaFinal}'";


                SQL = SQL.Replace("@CodigoProducto", codigoProductoConComillas).Replace("@Bodega", filtro.Codigo1.ToString()).Replace("@FechaInicial", FechaInicio ).Replace("@FechaFinal", FechaFinal);





                return Request.CreateResponse(HttpStatusCode.OK, db.Database.SqlQuery<ReporteVentas>(SQL).ToList());
            }
            catch (Exception ex1)
            {
                ModelCliente db2 = new ModelCliente();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex1.Message;
                be.StrackTrace = ex1.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex1);
                db2.BitacoraErrores.Add(be);
                db2.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex1);
            }
        }
    }
}