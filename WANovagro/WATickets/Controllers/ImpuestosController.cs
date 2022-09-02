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
    public class ImpuestosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/Impuestos/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLImpuestos; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Impuestos");

                var Impuestos = db.Impuestos.ToList();
                foreach (DataRow item in Ds.Tables["Impuestos"].Rows)
                {
                    var cardCode = item["id"].ToString();

                    var Impuesto = Impuestos.Where(a => a.Codigo == cardCode).FirstOrDefault();

                    if (Impuesto == null) //Existe ?
                    {
                        try
                        {
                            Impuesto = new Impuestos();
                            Impuesto.Codigo = item["id"].ToString();
                            Impuesto.Tarifa = Convert.ToDecimal(item["Impuesto"].ToString());

                            db.Impuestos.Add(Impuesto);
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
                            db.Entry(Impuesto).State = EntityState.Modified;


                            Impuesto.Tarifa = Convert.ToDecimal(item["Impuesto"].ToString());
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
                var Impuestos = db.Impuestos.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Impuestos);
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
        [Route("api/Impuestos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Impuestos impuestos = db.Impuestos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, impuestos);
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
        [Route("api/Impuestos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Impuestos impuestos)
        {
            try
            {
                Impuestos Impuesto = db.Impuestos.Where(a => a.id == impuestos.id).FirstOrDefault();
                if (Impuesto == null)
                {
                    Impuesto = new Impuestos();
                    Impuesto.id = impuestos.id;
                    Impuesto.Codigo = impuestos.Codigo;
                    Impuesto.Tarifa = impuestos.Tarifa;
                    db.Impuestos.Add(Impuesto);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un impuesto con este ID");
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
        [Route("api/Impuestos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Impuestos impuestos)
        {
            try
            {
                Impuestos Impuestos = db.Impuestos.Where(a => a.id == impuestos.id).FirstOrDefault();
                if (Impuestos != null)
                {
                    db.Entry(Impuestos).State = System.Data.Entity.EntityState.Modified;
                    Impuestos.Codigo = impuestos.Codigo;
                    Impuestos.Tarifa = impuestos.Tarifa;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un impuesto" +
                        " con este ID");
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
        [Route("api/Impuestos/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Impuestos Impuestos = db.Impuestos.Where(a => a.id == id).FirstOrDefault();
                if (Impuestos != null)
                {
                    db.Impuestos.Remove(Impuestos);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un impuesto con este ID");
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