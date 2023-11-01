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
    public class CategoriasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/Categorias/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLCategorias; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Categorias");

                var Categorias = db.Categorias.ToList();
                foreach (DataRow item in Ds.Tables["Categorias"].Rows)
                {
                    var cardCode = item["id"].ToString();

                    var Categoria = Categorias.Where(a => a.CodSAP == cardCode).FirstOrDefault();

                    if (Categoria == null) //Existe ?
                    {
                        try
                        {
                            
                            Categoria = new Categorias();
                            Categoria.CodSAP = item["id"].ToString();
                            Categoria.Nombre = item["Nombre"].ToString();






                            db.Categorias.Add(Categoria);
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
                            db.Entry(Categoria).State = EntityState.Modified;




                            Categoria.Nombre = item["Nombre"].ToString();
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
                var Categorias = db.Categorias.ToList();

                

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Categorias);
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
        [Route("api/Categorias/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Categorias categorias = db.Categorias.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, categorias);
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
        [Route("api/Categorias/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Categorias categorias)
        {
            try
            {
                Categorias Categoria = db.Categorias.Where(a => a.id == categorias.id).FirstOrDefault();
                if (Categoria == null)
                {
                    Categoria = new Categorias();
                    Categoria.id = categorias.id;
                    Categoria.CodSAP = categorias.CodSAP;
                    Categoria.Nombre = categorias.Nombre;
                   
                    db.Categorias.Add(Categoria);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una categoria con este ID");
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
        [Route("api/Categorias/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Categorias categorias)
        {
            try
            {
                Categorias Categorias = db.Categorias.Where(a => a.id == categorias.id).FirstOrDefault();
                if (Categorias != null)
                {
                    db.Entry(Categorias).State = System.Data.Entity.EntityState.Modified;
                    Categorias.CodSAP = categorias.CodSAP;
                    Categorias.Nombre = categorias.Nombre;
                   
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una categoria" +
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
        [Route("api/Categorias/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Categorias Categorias = db.Categorias.Where(a => a.id == id).FirstOrDefault();
                if (Categorias != null)
                {
                    db.Categorias.Remove(Categorias);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una categoria con este ID");
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