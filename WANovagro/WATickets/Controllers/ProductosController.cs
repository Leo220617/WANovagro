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
    public class ProductosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

    




        [Route("api/Productos/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos( )
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLProductos; //Preparo el query 

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion 
                Da.Fill(Ds, "Productos");

                var Productos = db.Productos.ToList();
                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    var itemCode = item["Codigo"].ToString();
                    var Whscode = item["idBodega"].ToString();
                    var bod = db.Bodegas.Where(a => a.CodSAP == Whscode).FirstOrDefault() == null ? 0 : db.Bodegas.Where(a => a.CodSAP == Whscode).FirstOrDefault().id;
                    if(bod > 0) // si existe la bodega
                    {
                        var PriceList = item["ListaPrecio"].ToString();
                        var list = db.ListaPrecios.Where(a => a.CodSAP == PriceList).FirstOrDefault() == null ? 0 : db.ListaPrecios.Where(a => a.CodSAP == PriceList).FirstOrDefault().id;
                        if(list > 0) //si existe la lista
                        {
                            var Producto = Productos.Where(a => a.Codigo == itemCode && a.idBodega == bod && a.idListaPrecios == list).FirstOrDefault();

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
                var Productos = db.Productos.ToList(); //Traemos el listado de productos

                if(!string.IsNullOrEmpty(filtro.Texto))
                {
                    // and = &&, or = ||
                    Productos = Productos.Where(a => a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper()) || a.CodBarras.ToUpper().Contains(filtro.Texto.ToUpper()) ).ToList();// filtramos por lo que trae texto
                }
                if(!string.IsNullOrEmpty(filtro.CardCode))
                {
                    var Bodegas = db.Bodegas.Where(a => a.CodSuc != filtro.CardCode).ToList();
                    foreach(var item in Bodegas)
                    {
                        Productos = Productos.Where(a => a.idBodega != item.id).ToList();

                    }
                }
                if(filtro.Codigo1 > 0) // esto por ser integer
                {
                    Productos = Productos.Where(a => a.idBodega == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Productos = Productos.Where(a => a.idListaPrecios == filtro.Codigo2).ToList(); 
                }



                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Productos);
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
        [Route("api/Productos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Productos productos = db.Productos.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, productos);
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
        [Route("api/Productos/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Productos productos)
        {
            try
            {
                Productos Producto = db.Productos.Where(a => a.id == productos.id).FirstOrDefault();
                if (Producto == null)
                {
                    Producto = new Productos();
                    Producto.id = productos.id;
                    Producto.Codigo = productos.Codigo;
                    Producto.idBodega = productos.idBodega;
                    Producto.idImpuesto = productos.idImpuesto;
                    Producto.idListaPrecios = productos.idListaPrecios;
                    Producto.Nombre = productos.Nombre;
                    Producto.PrecioUnitario = productos.PrecioUnitario;
                    Producto.UnidadMedida = productos.UnidadMedida;
                    Producto.Cabys = productos.Cabys;
                    Producto.TipoCod = productos.TipoCod;
                    Producto.CodBarras = productos.CodBarras;
                    Producto.Costo = productos.Costo;
                    Producto.Stock = productos.Stock;
                    Producto.Moneda = Producto.Moneda;

                    Producto.Activo = true;
                    Producto.ProcesadoSAP = false;
                    db.Productos.Add(Producto);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un producto con este ID");
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
        [Route("api/Productos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Productos productos)
        {
            try
            {
                Productos Productos = db.Productos.Where(a => a.id == productos.id).FirstOrDefault();
                if (Productos != null)
                {
                    db.Entry(Productos).State = System.Data.Entity.EntityState.Modified;
                    Productos.idBodega = productos.idBodega;
                    Productos.idImpuesto = productos.idImpuesto;
                    Productos.idListaPrecios = productos.idListaPrecios;
                    Productos.Nombre = productos.Nombre;
                    Productos.PrecioUnitario = productos.PrecioUnitario;
                    Productos.UnidadMedida = productos.UnidadMedida;
                    Productos.Cabys = productos.Cabys;
                    Productos.TipoCod = productos.TipoCod;
                    Productos.CodBarras = productos.CodBarras;
                    Productos.Costo = productos.Costo;
                    Productos.Stock = productos.Stock;
                    Productos.Activo = productos.Activo;
                    Productos.Moneda = productos.Moneda;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un producto" +
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
        [Route("api/Productos/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Productos Productos = db.Productos.Where(a => a.id == id).FirstOrDefault();
                if (Productos != null)
                {
                    db.Productos.Remove(Productos);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un producto con este ID");
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
        [Route("api/Productos/InsertarSAPByProduct")]
        public HttpResponseMessage GetExtraeByProduct([FromUri] int idBod)
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db);

                var code = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? db.Bodegas.FirstOrDefault() : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault();
               
                var SQL = parametros.SQLProductos + " and t2.WhsCode = '" + db.Bodegas.Where(a => a.id == code.id).FirstOrDefault().CodSAP + "' " ;


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
                            Producto.ProcesadoSAP = true;

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
                            Producto.ProcesadoSAP = true;

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