using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class VendedoresController : ApiController
    {
       
        ModelCliente db = new ModelCliente();
        G G = new G();


        [Route("api/Vendedores/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLVendedores; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Vendedores");

                var Vendedores = db.Vendedores.ToList();
                foreach (DataRow item in Ds.Tables["Vendedores"].Rows)
                {
                    var cardCode = item["id"].ToString();

                    var Vendedor = Vendedores.Where(a => a.CodSAP == cardCode).FirstOrDefault();

                    if (Vendedor == null) //Existe ?
                    {
                        try
                        {
                            string Status = item["Estado"].ToString();
                            Vendedor = new Vendedores();
                            Vendedor.CodSAP = item["id"].ToString();
                            Vendedor.Nombre = item["Nombre"].ToString();
                          
                            Vendedor.CodSuc = "";

                            if (Status == "Y")
                            {
                                Vendedor.Activo = true;
                            }
                            else if (Status == "N")
                            {
                                Vendedor.Activo = false;
                            }


                            db.Vendedores.Add(Vendedor);
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
                            db.Entry(Vendedor).State = EntityState.Modified;
                            string Status = item["Estado"].ToString();
                            Vendedor.Nombre = item["Nombre"].ToString();

                            if (Status == "Y")
                            {
                                Vendedor.Activo = true;
                            }
                            else if (Status == "N")
                            {
                                Vendedor.Activo = false;
                            }

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
        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var Vendedores = db.Vendedores.ToList();

                if (!string.IsNullOrEmpty(filtro.CardName))
                {
                    Vendedores = Vendedores.Where(a => a.CodSuc == filtro.CardName).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Vendedores);
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

        [Route("api/Vendedores/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Vendedores vendedores = db.Vendedores.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, vendedores);
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
        [Route("api/Vendedores/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Vendedores vendedores)
        {
            try
            {
                Vendedores Vendedor = db.Vendedores.Where(a => a.id == vendedores.id).FirstOrDefault();
                if (Vendedor == null)
                {
                    Vendedor = new Vendedores();
                    Vendedor.id = vendedores.id;
                    Vendedor.CodSuc = vendedores.CodSuc;
                    Vendedor.Nombre = vendedores.Nombre;
                    Vendedor.CodSAP = vendedores.CodSAP;
                    Vendedor.Activo = true;
                    db.Vendedores.Add(Vendedor);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un vendedor con este ID");
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
        [Route("api/Vendedores/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Vendedores vendedores)
        {
            try
            {
                Vendedores Vendedor = db.Vendedores.Where(a => a.id == vendedores.id).FirstOrDefault();
                if (Vendedor != null)
                {
                    db.Entry(Vendedor).State = System.Data.Entity.EntityState.Modified;
                    Vendedor.Nombre = vendedores.Nombre;
                    Vendedor.CodSAP = vendedores.CodSAP;
                    Vendedor.CodSuc = vendedores.CodSuc;
                    Vendedor.Activo = Vendedor.Activo;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un vendedor" +
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
    }
}