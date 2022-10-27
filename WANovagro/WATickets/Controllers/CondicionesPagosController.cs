using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
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

    [Authorize]
    public class CondicionesPagosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


        [Route("api/CondicionesPagos/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLCondiciones; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "CondicionesPagos");

                var CondicionesPagos = db.CondicionesPagos.ToList();
                foreach (DataRow item in Ds.Tables["CondicionesPagos"].Rows)
                {
                    var cardCode = item["id"].ToString();

                    var CondicionPago = CondicionesPagos.Where(a => a.CodSAP == cardCode).FirstOrDefault();

                    if (CondicionPago == null) //Existe ?
                    {
                        try
                        {
                            CondicionPago = new CondicionesPagos();
                            CondicionPago.CodSAP = item["id"].ToString();
                            CondicionPago.Nombre = item["Condicion"].ToString();
                            CondicionPago.Dias = Convert.ToInt32(item["Dias"]);
                            db.CondicionesPagos.Add(CondicionPago);
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
                            db.Entry(CondicionPago).State = EntityState.Modified;


                            CondicionPago.Nombre = item["Condicion"].ToString();
                            CondicionPago.Dias = Convert.ToInt32(item["Dias"]);
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




        public HttpResponseMessage GetAll()
        {
            try
            {
                var CondicionesPagos = db.CondicionesPagos.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, CondicionesPagos);
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

        [Route("api/CondicionesPagos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                CondicionesPagos condicionesPagos = db.CondicionesPagos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, condicionesPagos);
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