using Newtonsoft.Json;
using System;
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

{   [Authorize]
    public class BodegasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/Bodegas/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLBodegas; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Bodegas");

                var Bodegas  = db.Bodegas.ToList();
                foreach (DataRow item in Ds.Tables["Bodegas"].Rows)
                {
                    var cardCode = item["id"].ToString();

                   // var Sucursal = db.Sucursales.Where(a => a.CodSuc == cardCode).FirstOrDefault();

                     
                        var Bodega = Bodegas.Where(a => a.CodSAP == cardCode).FirstOrDefault();

                        if (Bodega == null) //Existe ?
                        {
                            try
                            {
                                Bodega = new Bodegas();
                                Bodega.CodSAP = item["id"].ToString();
                                Bodega.CodSuc = "";
                                Bodega.Nombre = item["Bodega"].ToString();

                                db.Bodegas.Add(Bodega);
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
                                db.Entry(Bodega).State = EntityState.Modified;

                                //Bodega.CodSuc = item["id"].ToString();
                                Bodega.Nombre = item["Bodega"].ToString();


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
                var Bodegas = db.Bodegas.ToList();
                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    // and = &&, or = ||
                    Bodegas = Bodegas.Where(a => a.CodSuc.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();// filtramos por lo que trae texto
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Bodegas);
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
        [Route("api/Bodegas/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Bodegas bodegas = db.Bodegas.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, bodegas);
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
        [Route("api/Bodegas/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Bodegas bodegas)
        {
            try
            {
                Bodegas Bodega = db.Bodegas.Where(a => a.id == bodegas.id).FirstOrDefault();
                if (Bodega == null)
                {
                    Bodega= new Bodegas();
                    Bodega.id = bodegas.id;
                    Bodega.CodSuc = bodegas.CodSuc;
                    Bodega.Nombre = bodegas.Nombre;
                    db.Bodegas.Add(Bodega);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una bodega con este ID");
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
        [Route("api/Bodegas/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Bodegas bodegas)
        {
            try
            {
                Bodegas Bodegas = db.Bodegas.Where(a => a.id == bodegas.id).FirstOrDefault();
                if (Bodegas != null)
                {
                    db.Entry(Bodegas).State = System.Data.Entity.EntityState.Modified;
                    Bodegas.CodSuc = bodegas.CodSuc;
                   //Bodegas.Nombre = bodegas.Nombre;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una bodega" +
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
        [Route("api/Bodegas/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Bodegas Bodegas = db.Bodegas.Where(a => a.id == id).FirstOrDefault();
                if (Bodegas != null)
                {
                    db.Bodegas.Remove(Bodegas);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una bodega con este ID");
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
        [Route("api/Bodegas/InsertarSAPByProduct")]
        public HttpResponseMessage GetExtraeByProduct([FromUri] int id)
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db);

                var code = db.Bodegas.Where(a => a.id == id).FirstOrDefault() == null ? db.Bodegas.FirstOrDefault() : db.Bodegas.Where(a => a.id == id).FirstOrDefault();

                var SQL = parametros.SQLProductos + " and t2.WhsCode = '" + db.Bodegas.Where(a => a.id == code.id).FirstOrDefault().CodSAP + "' ";


                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Productos");

                var Productos = db.Productos.ToList();

                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    var cardCode = item["Codigo"].ToString();

                    var Producto = Productos.Where(a => a.Codigo == cardCode).FirstOrDefault();
                    if (Producto == null) //Existe ?
                    {

                        try
                        {
                            Producto = new Productos();
                            Producto.Codigo = item["Codigo"].ToString();
                            var idBodega = item["idBodega"].ToString();
                            Producto.idBodega = db.Bodegas.Where(a => a.CodSAP == idBodega).FirstOrDefault() == null ? 0 : db.Bodegas.Where(a => a.CodSAP == idBodega).FirstOrDefault().id;
                            var idImpuesto = item["Impuesto"].ToString();
                            Producto.idImpuesto = db.Impuestos.Where(a => a.Codigo == idImpuesto).FirstOrDefault() == null ? 0 : db.Impuestos.Where(a => a.Codigo == idImpuesto).FirstOrDefault().id;
                            var idLista = item["ListaPrecio"].ToString();
                            Producto.idListaPrecios = db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault() == null ? 0 : db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault().id;
                            Producto.Nombre = item["Nombre"].ToString();
                            Producto.PrecioUnitario = Convert.ToDecimal(item["PrecioUnitario"]);
                            Producto.UnidadMedida = item["UnidadMedida"].ToString();
                            Producto.Cabys = item["Cabys"].ToString();
                            Producto.TipoCod = item["TipoCodigo"].ToString();
                            Producto.CodBarras = item["CodigoBarras"].ToString();
                            Producto.Costo = Convert.ToDecimal(item["Costo"]);
                            Producto.Stock = Convert.ToDecimal(item["StockReal"]);
                            Producto.Moneda = item["Moneda"].ToString();
                            Producto.Activo = true;
                            Producto.FechaActualizacion = DateTime.Now;
                            Producto.ProcesadoSAP = true;
                            var MAG = Convert.ToInt32(item["MAG"]);
                            if (MAG == 1)
                            {
                                Producto.MAG = true;
                            }
                            else if (MAG == 0)
                            {
                                Producto.MAG = false;
                            }
                            db.Productos.Add(Producto);
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
                            db.Entry(Producto).State = EntityState.Modified;
                            var idBodega = item["idBodega"].ToString();
                            Producto.idBodega = db.Bodegas.Where(a => a.CodSAP == idBodega).FirstOrDefault() == null ? 0 : db.Bodegas.Where(a => a.CodSAP == idBodega).FirstOrDefault().id;
                            var idImpuesto = item["Impuesto"].ToString();
                            Producto.idImpuesto = db.Impuestos.Where(a => a.Codigo == idImpuesto).FirstOrDefault() == null ? 0 : db.Impuestos.Where(a => a.Codigo == idImpuesto).FirstOrDefault().id;
                            var idLista = item["ListaPrecio"].ToString();
                            Producto.idListaPrecios = db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault() == null ? 0 : db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault().id;
                            Producto.Nombre = item["Nombre"].ToString();
                            Producto.PrecioUnitario = Convert.ToDecimal(item["PrecioUnitario"]);
                            Producto.UnidadMedida = item["UnidadMedida"].ToString(); ;
                            Producto.Cabys = item["Cabys"].ToString();
                            Producto.TipoCod = item["TipoCodigo"].ToString();
                            Producto.CodBarras = item["CodigoBarras"].ToString();
                            Producto.Costo = Convert.ToDecimal(item["Costo"]);
                            Producto.Stock = Convert.ToDecimal(item["StockReal"]);
                            Producto.Moneda = item["Moneda"].ToString();

                            Producto.Activo = true;
                            Producto.FechaActualizacion = DateTime.Now;
                            Producto.ProcesadoSAP = true;
                            var MAG = Convert.ToInt32(item["MAG"]);
                            if (MAG == 1)
                            {
                                Producto.MAG = true;
                            }
                            else if (MAG == 0)
                            {
                                Producto.MAG = false;
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
                return Request.CreateResponse(System.Net.HttpStatusCode.OK);

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
    }
}