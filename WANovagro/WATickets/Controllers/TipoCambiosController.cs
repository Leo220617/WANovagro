using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using WATickets.Models;
using WATickets.Models.APIS;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    public class TipoCambiosController : ApiController
    {

        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/TipoCambios/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLTipoCambio; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "TipoCambios");

                var TipoCambios = db.TipoCambios.ToList();
                foreach (DataRow item in Ds.Tables["TipoCambios"].Rows)
                {
                    var FechaActual = DateTime.Now.Date;

                    var Moneda = item["Moneda"].ToString();

                    var TiposCambio = TipoCambios.Where(a => a.Fecha == FechaActual && a.Moneda == Moneda ).FirstOrDefault();

                    if (TiposCambio == null) //Existe ?
                    {
                        try
                        {
                            
                            TiposCambio = new TipoCambios();
                            TiposCambio.TipoCambio = Convert.ToDecimal(item["Precio"]);
                            TiposCambio.Moneda = item["Moneda"].ToString();
                            TiposCambio.Fecha = Convert.ToDateTime(item["Fecha"]);

                           


                            db.TipoCambios.Add(TiposCambio);
                            db.SaveChanges();

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
                        }
                    }
                    else
                    {
                        try
                        {
                            db.Entry(TiposCambio).State = EntityState.Modified;


                            TiposCambio.TipoCambio = Convert.ToDecimal(item["Precio"]);
                            TiposCambio.Moneda = item["Moneda"].ToString();
                            TiposCambio.Fecha = Convert.ToDateTime(item["Fecha"]);
                            db.SaveChanges();
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
                        }

                    }


                }


                Cn.Close(); //se cierra la conexion
                Cn.Dispose();

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, "Procesado con exito");

            }
            catch (Exception ex)
            {

                ModelCliente db2 = new ModelCliente();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex);
                db2.BitacoraErrores.Add(be);
                db2.SaveChanges();

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAsync()
        {

            try
            {
                HttpClient clienteProd = new HttpClient();
                clienteProd.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string url = "https://tipodecambio.paginasweb.cr/api//" + DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
                try
                {
                    HttpResponseMessage response3 = await clienteProd.GetAsync(url);
                    if (response3.IsSuccessStatusCode)
                    {
                        response3.Content.Headers.ContentType.MediaType = "application/json";
                        var res = await response3.Content.ReadAsStringAsync();


                        try
                        {
                            var respZoho = await response3.Content.ReadAsAsync<RespuestaTipoCambio>();

                            var tp = (SAPbobsCOM.SBObob)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);

                            tp.SetCurrencyRate("USD", DateTime.Now, Convert.ToDouble(respZoho.venta));

                        }
                        catch (Exception)
                        {


                        }

                    }
                }
                catch (Exception)
                {


                }




                return Request.CreateResponse(HttpStatusCode.OK, "procesado con exito");
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }


        }

        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var TipoCambios = db.TipoCambios.ToList();

                var time = new DateTime();

                if(filtro.FechaInicial != time)
                {
                    var Fecha = filtro.FechaInicial.Date;
                    TipoCambios = TipoCambios.Where(a => a.Fecha == Fecha).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, TipoCambios);
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
        [Route("api/TipoCambios/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                TipoCambios tipoCambios = db.TipoCambios.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, tipoCambios);
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