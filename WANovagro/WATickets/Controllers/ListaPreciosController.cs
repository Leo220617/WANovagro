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
    public class ListaPreciosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/ListaPrecios/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLListaPrecios; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "ListaPrecios");

                var ListaPrecios = db.ListaPrecios.ToList();
                foreach (DataRow item in Ds.Tables["ListaPrecios"].Rows)
                {
                    var cardCode = item["Codigo"].ToString();

                    var ListaPrecio = ListaPrecios.Where(a => a.CodSAP == cardCode).FirstOrDefault();

                    if (ListaPrecio == null) //Existe ?
                    {
                        try
                        {
                            ListaPrecio = new ListaPrecios();
                            ListaPrecio.CodSAP = item["id"].ToString();
                            ListaPrecio.Nombre = item["ListaPrecio"].ToString();

                            db.ListaPrecios.Add(ListaPrecio);
                            db.SaveChanges();

                        }
                        catch (Exception ex1)
                        {

                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = ex1.Message;
                            be.StrackTrace = ex1.StackTrace;
                            be.Fecha = DateTime.Now;
                            be.JSON = JsonConvert.SerializeObject(ex1);
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        try
                        {
                            db.Entry(ListaPrecios).State = EntityState.Modified;

                            ListaPrecio.Nombre = item["ListaPrecio"].ToString();

                            db.SaveChanges();
                        }
                        catch (Exception ex1)
                        {

                            BitacoraErrores be = new BitacoraErrores();
                            be.Descripcion = ex1.Message;
                            be.StrackTrace = ex1.StackTrace;
                            be.Fecha = DateTime.Now;
                            be.JSON = JsonConvert.SerializeObject(ex1);
                            db.BitacoraErrores.Add(be);
                            db.SaveChanges();
                        }

                    }


                }


                Cn.Close(); //se cierra la conexion
                Cn.Dispose();

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, "Procesado con exito");

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

        public HttpResponseMessage GetAll()
        {
            try
            {
                var ListaPrecios = db.ListaPrecios.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, ListaPrecios);
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
        [Route("api/ListaPrecios/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                ListaPrecios listaprecios = db.ListaPrecios.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, listaprecios);
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
        [Route("api/ListaPrecios/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ListaPrecios listaprecios)
        {
            try
            {
                ListaPrecios ListaPrecio = db.ListaPrecios.Where(a => a.id == listaprecios.id).FirstOrDefault();
                if (ListaPrecio == null)
                {
                    ListaPrecio = new ListaPrecios();
                    ListaPrecio.id = listaprecios.id;
                    ListaPrecio.CodSAP = listaprecios.CodSAP;
                    ListaPrecio.Nombre = listaprecios.Nombre;
                    db.ListaPrecios.Add(ListaPrecio);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una lista de precio con este ID");
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
        [Route("api/ListaPrecios/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] ListaPrecios listaprecios)
        {
            try
            {
                ListaPrecios ListaPrecio = db.ListaPrecios.Where(a => a.id == listaprecios.id).FirstOrDefault();
                if (ListaPrecio != null)
                {
                    db.Entry(ListaPrecio).State = System.Data.Entity.EntityState.Modified;
                    ListaPrecio.CodSAP = listaprecios.CodSAP;
                    ListaPrecio.Nombre = listaprecios.Nombre;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una lista de precio" +
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
        [Route("api/ListaPrecios/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                ListaPrecios ListaPrecios = db.ListaPrecios.Where(a => a.id == id).FirstOrDefault();
                if (ListaPrecios != null)
                {
                    db.ListaPrecios.Remove(ListaPrecios);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una lista de precio con este ID");
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