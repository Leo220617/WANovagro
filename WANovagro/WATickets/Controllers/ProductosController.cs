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
        public HttpResponseMessage GetExtraeDatos()
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

                //var Productos = db.Productos.ToList();
                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    var itemCode = item["Codigo"].ToString();
                    var Whscode = item["idBodega"].ToString();
                    var bod = db.Bodegas.Where(a => a.CodSAP == Whscode).FirstOrDefault() == null ? 0 : db.Bodegas.Where(a => a.CodSAP == Whscode).FirstOrDefault().id;
                    if (bod > 0) // si existe la bodega
                    {
                        var PriceList = item["ListaPrecio"].ToString();
                        var list = db.ListaPrecios.Where(a => a.CodSAP == PriceList).FirstOrDefault() == null ? 0 : db.ListaPrecios.Where(a => a.CodSAP == PriceList).FirstOrDefault().id;
                        if (list > 0) //si existe la lista
                        {
                            var Producto = db.Productos.Where(a => a.Codigo == itemCode && a.idBodega == bod && a.idListaPrecios == list).FirstOrDefault();

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

                                    var idCategoria = item["Categoria"].ToString();
                                    Producto.idCategoria = db.Categorias.Where(a => a.CodSAP == idCategoria).FirstOrDefault() == null ? 0 : db.Categorias.Where(a => a.CodSAP == idCategoria).FirstOrDefault().id;

                                    Producto.Nombre = item["Nombre"].ToString();
                                    Producto.PrecioUnitario = Convert.ToDecimal(item["PrecioUnitario"]);
                                    Producto.UnidadMedida = item["UnidadMedida"].ToString();
                                    Producto.Cabys = item["Cabys"].ToString();
                                    Producto.TipoCod = item["TipoCodigo"].ToString();
                                    Producto.CodBarras = item["CodigoBarras"].ToString();
                                    Producto.Costo = Convert.ToDecimal(item["Costo"]);
                                    Producto.Stock = Convert.ToDecimal(item["StockReal"]);
                                    Producto.Moneda = item["Moneda"].ToString();
                                    Producto.Dimension = Convert.ToInt32(item["Dimension"]);
                                    Producto.NormaReparto = item["Norma"].ToString();
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
                                    Producto.Editable = Convert.ToBoolean(Convert.ToInt32(item["Editable"]));
                                    var Serie = Convert.ToInt32(item["Serie"]);
                                    if (Serie == 1)
                                    {
                                        Producto.Serie = true;
                                    }
                                    else if (Serie == 0)
                                    {
                                        Producto.Serie = false;
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

                                    var idCategoria = item["Categoria"].ToString();
                                    Producto.idCategoria = db.Categorias.Where(a => a.CodSAP == idCategoria).FirstOrDefault() == null ? 0 : db.Categorias.Where(a => a.CodSAP == idCategoria).FirstOrDefault().id;

                                    Producto.Nombre = item["Nombre"].ToString();
                                    Producto.Dimension = Convert.ToInt32(item["Dimension"]);
                                    Producto.NormaReparto = item["Norma"].ToString();
                                    decimal Porcentaje = 0;
                                    Producto.PrecioUnitario = Convert.ToDecimal(item["PrecioUnitario"]);
                                    Producto.Moneda = item["Moneda"].ToString();
                                    Producto.Costo = Convert.ToDecimal(item["Costo"]);

                                    var time = DateTime.Now.Date;
                                    var Promocion = db.Promociones.Where(a => a.ItemCode == Producto.Codigo && a.idListaPrecio == Producto.idListaPrecios && a.idCategoria == Producto.idCategoria && a.Fecha <= time && a.FechaVen >= time).FirstOrDefault();
                                    var Margenes = db.EncMargenes.Where(a => a.idListaPrecio == Producto.idListaPrecios && a.Moneda == Producto.Moneda && a.idCategoria == Producto.idCategoria).FirstOrDefault();
                                    var DetMargenes = db.DetMargenes.Where(a => a.ItemCode == Producto.Codigo && a.idListaPrecio == Producto.idListaPrecios && a.Moneda == Producto.Moneda && a.idCategoria == Producto.idCategoria).FirstOrDefault();
                                    if (Promocion != null)
                                    {
                                        Producto.PrecioUnitario = Promocion.PrecioFinal;

                                    }
                                    else if (DetMargenes != null)
                                    {
                                        var PrecioCob = Producto.Costo / (1 - (DetMargenes.Cobertura / 100));
                                        var PrecioMin = PrecioCob / (1 - (DetMargenes.MargenMin / 100));
                                        var PrecioFinal = PrecioCob / (1 - (DetMargenes.Margen / 100));

                                        db.Entry(DetMargenes).State = EntityState.Modified;
                                        DetMargenes.PrecioCob = PrecioCob;
                                        DetMargenes.PrecioMin = PrecioMin;
                                        DetMargenes.PrecioFinal = PrecioFinal;
                                       // db.SaveChanges();
                                        Producto.PrecioUnitario = DetMargenes.PrecioFinal;
                                    }
                                    else if (Margenes != null)
                                    {
                                        var PrecioCob = Producto.Costo / (1 - (Margenes.Cobertura / 100));
                                        var PrecioFinal = PrecioCob / (1 - (Margenes.Margen / 100));
                                        Producto.PrecioUnitario = PrecioFinal;
                                    }



                                    Producto.UnidadMedida = item["UnidadMedida"].ToString(); ;
                                    Producto.Cabys = item["Cabys"].ToString();
                                    Producto.TipoCod = item["TipoCodigo"].ToString();
                                    Producto.CodBarras = item["CodigoBarras"].ToString();

                                    Producto.Stock = Convert.ToDecimal(item["StockReal"]);


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
                                    Producto.Editable = Convert.ToBoolean(Convert.ToInt32(item["Editable"]));

                                    var Serie = Convert.ToInt32(item["Serie"]);
                                    if (Serie == 1)
                                    {
                                        Producto.Serie = true;
                                    }
                                    else if (Serie == 0)
                                    {
                                        Producto.Serie = false;
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

        [Route("api/Productos/InsertarSAPByClient")]
        public HttpResponseMessage GetExtraeByClient([FromUri] int id)
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var code = db.Productos.Where(a => a.id == id).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == id).FirstOrDefault().Codigo;
                if (code == "0")
                {
                    throw new Exception("El codigo del producto no es valido");
                }
                var SQL = parametros.SQLProductos + " and t0.ItemCode = '" + code + "'"; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Productos");

                var Productos = db.Productos.ToList();
                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    var ItemCode = item["Codigo"].ToString();

                    var Producto = Productos.Where(a => a.Codigo == ItemCode).FirstOrDefault();

                    if (Producto != null) //Existe ?
                    {

                        try
                        {
                            db.Entry(Producto).State = EntityState.Modified;


                            Producto.FechaActualizacion = DateTime.Now;

                            var MAG = Convert.ToInt32(item["MAG"]);
                            if (MAG == 1)
                            {
                                Producto.MAG = true;
                            }
                            else if (MAG == 0)
                            {
                                Producto.MAG = false;
                            }
                            Producto.Editable = Convert.ToBoolean(Convert.ToInt32(item["Editable"]));

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
                if (!string.IsNullOrEmpty(filtro.CardCode))
                {
                    var Bodegas = db.Bodegas.Where(a => a.CodSuc != filtro.CardCode).Select(a => a.id).ToList();
                    var Productos = db.Productos.AsQueryable();
                     Productos = Productos.Where(a => (filtro.Codigo2 > 0 ? a.idListaPrecios == filtro.Codigo2 : true)
                 && (filtro.Codigo1 > 0 ? a.idBodega == filtro.Codigo1 : true)
                 && (!string.IsNullOrEmpty(filtro.Texto) ? a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper()) || a.CodBarras.ToUpper().Contains(filtro.Texto.ToUpper()) : true)
                 && (filtro.Codigo3 > 0 ? a.idCategoria == filtro.Codigo3 : true)
                 && (!string.IsNullOrEmpty(filtro.CardCode) ? !Bodegas.Contains(a.idBodega) : true)
                 &&  (filtro.Activo ? a.Activo == filtro.Activo : true)

                 ); //Traemos el listado de productos

                    //if (!string.IsNullOrEmpty(filtro.CardCode)) // este no
                    //{
                    //    var Bodegas = db.Bodegas.Where(a => a.CodSuc != filtro.CardCode).ToList();
                    //    foreach (var item in Bodegas)
                    //    {
                    //        Productos = Productos.Where(a => a.idBodega != item.id).ToList();

                    //    }
                    //}


                    return Request.CreateResponse(System.Net.HttpStatusCode.OK, Productos.ToList());
                }
                else
                {
                    var Productos = db.Productos.AsQueryable();
                    Productos = Productos.Where(a => (filtro.Codigo2 > 0 ? a.idListaPrecios == filtro.Codigo2 : true)
                && (filtro.Codigo1 > 0 ? a.idBodega == filtro.Codigo1 : true)
                && (!string.IsNullOrEmpty(filtro.Texto) ? a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper()) || a.CodBarras.ToUpper().Contains(filtro.Texto.ToUpper()) : true)
                && (filtro.Codigo3 > 0 ? a.idCategoria == filtro.Codigo3 : true)


                ); //Traemos el listado de productos

                    return Request.CreateResponse(System.Net.HttpStatusCode.OK, Productos.ToList());
                }

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
                    Producto.MAG = false;
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
                    //Productos.MAG = productos.MAG;
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
        [Route("api/Productos/DesactivarProductos")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] string code)
        {
            try
            {
                var Productos = db.Productos.Where(a => a.Codigo == code).ToList();

                foreach (var item in Productos)
                {


                    if (Productos != null)
                    {

                        var Producto = db.Productos.Where(a => a.id == item.id).FirstOrDefault();
                        db.Entry(Producto).State = EntityState.Modified;


                        if (Producto.Activo)
                        {

                            Producto.Activo = false;

                        }
                        else
                        {

                            Producto.Activo = true;
                        }




                        db.SaveChanges();

                    }
                    else
                    {
                        throw new Exception("No existe un producto con este ID");
                    }
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
                    var PriceList = item["ListaPrecio"].ToString();
                    var list = db.ListaPrecios.Where(a => a.CodSAP == PriceList).FirstOrDefault() == null ? 0 : db.ListaPrecios.Where(a => a.CodSAP == PriceList).FirstOrDefault().id;
                    var cardCode = item["Codigo"].ToString();

                    var Whscode = item["idBodega"].ToString();
                    var bod = db.Bodegas.Where(a => a.CodSAP == Whscode).FirstOrDefault() == null ? 0 : db.Bodegas.Where(a => a.CodSAP == Whscode).FirstOrDefault().id;

                    var Producto = db.Productos.Where(a => a.Codigo == cardCode && a.idListaPrecios == list && a.idBodega == idBod).FirstOrDefault();
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

                            var MAG = Convert.ToInt32(item["MAG"]);
                            if (MAG == 1)
                            {
                                Producto.MAG = true;
                            }
                            else if (MAG == 0)
                            {
                                Producto.MAG = false;
                            }
                            Producto.Editable = Convert.ToBoolean(Convert.ToInt32(item["Editable"]));

                            var Serie = Convert.ToInt32(item["Serie"]);
                            if (Serie == 1)
                            {
                                Producto.Serie = true;
                            }
                            else if (Serie == 0)
                            {
                                Producto.Serie = false;
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
                            var idCategoria = item["Categoria"].ToString();
                            Producto.idCategoria = db.Categorias.Where(a => a.CodSAP == idCategoria).FirstOrDefault() == null ? 0 : db.Categorias.Where(a => a.CodSAP == idCategoria).FirstOrDefault().id;
                            Producto.Costo = Convert.ToDecimal(item["Costo"]);
                            Producto.Moneda = item["Moneda"].ToString();

                            var time = DateTime.Now.Date;
                            var Promocion = db.Promociones.Where(a => a.ItemCode == Producto.Codigo && a.idListaPrecio == Producto.idListaPrecios && a.idCategoria == Producto.idCategoria && a.Fecha <= time && a.FechaVen >= time).FirstOrDefault();
                            var Margenes = db.EncMargenes.Where(a => a.idListaPrecio == Producto.idListaPrecios && a.Moneda == Producto.Moneda && a.idCategoria == Producto.idCategoria).FirstOrDefault();
                            var DetMargenes = db.DetMargenes.Where(a => a.ItemCode == Producto.Codigo && a.idListaPrecio == Producto.idListaPrecios && a.Moneda == Producto.Moneda && a.idCategoria == Producto.idCategoria).FirstOrDefault();
                            if (Promocion != null)
                            {
                                Producto.PrecioUnitario = Promocion.PrecioFinal;

                            }
                            else if (DetMargenes != null)
                            {
                                Producto.PrecioUnitario = DetMargenes.PrecioFinal;
                            }
                            else if (Margenes != null)
                            {
                                var PrecioCob = Producto.Costo / (1 - (Margenes.Cobertura / 100));
                                var PrecioFinal = PrecioCob / (1 - (Margenes.Margen / 100));
                                Producto.PrecioUnitario = PrecioFinal;
                            }
                            Producto.UnidadMedida = item["UnidadMedida"].ToString(); ;
                            Producto.Cabys = item["Cabys"].ToString();
                            Producto.TipoCod = item["TipoCodigo"].ToString();
                            Producto.CodBarras = item["CodigoBarras"].ToString();

                            Producto.Stock = Convert.ToDecimal(item["StockReal"]);


                            Producto.Activo = true;
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
                            Producto.Editable = Convert.ToBoolean(Convert.ToInt32(item["Editable"]));
                            var Serie = Convert.ToInt32(item["Serie"]);
                            if (Serie == 1)
                            {
                                Producto.Serie = true;
                            }
                            else if (Serie == 0)
                            {
                                Producto.Serie = false;
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

        [Route("api/Productos/InsertarSAPByItemCode")]
        public HttpResponseMessage GetExtraeByItemCode([FromUri] string Codigo)
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                if (Codigo == "0")
                {
                    throw new Exception("El codigo del producto no es valido");
                }

                var SQL = parametros.SQLProductos + " and t0.ItemCode = '" + Codigo + "'"; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Productos");

                var Productos = db.Productos.ToList();
                foreach (DataRow item in Ds.Tables["Productos"].Rows)
                {
                    var ItemCode = item["Codigo"].ToString();
                    var Whscode = item["idBodega"].ToString();
                    var bod = db.Bodegas.Where(a => a.CodSAP == Whscode).FirstOrDefault() == null ? 0 : db.Bodegas.Where(a => a.CodSAP == Whscode).FirstOrDefault().id;
                    if (bod > 0) // si existe la bodega
                    {
                        var Producto = Productos.Where(a => a.Codigo == ItemCode && a.idBodega == bod).FirstOrDefault();

                        if (Producto != null) //Existe ?
                        {

                            try
                            {
                                db.Entry(Producto).State = EntityState.Modified;


                                Producto.FechaActualizacion = DateTime.Now;

                                var MAG = Convert.ToInt32(item["MAG"]);
                                if (MAG == 1)
                                {
                                    Producto.MAG = true;
                                }
                                else if (MAG == 0)
                                {
                                    Producto.MAG = false;
                                }
                                Producto.Editable = Convert.ToBoolean(Convert.ToInt32(item["Editable"]));

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
                                Producto = new Productos();
                                Producto.Codigo = item["Codigo"].ToString();
                                var idBodega = item["idBodega"].ToString();
                                Producto.idBodega = db.Bodegas.Where(a => a.CodSAP == idBodega).FirstOrDefault() == null ? 0 : db.Bodegas.Where(a => a.CodSAP == idBodega).FirstOrDefault().id;
                                var idImpuesto = item["Impuesto"].ToString();
                                Producto.idImpuesto = db.Impuestos.Where(a => a.Codigo == idImpuesto).FirstOrDefault() == null ? 0 : db.Impuestos.Where(a => a.Codigo == idImpuesto).FirstOrDefault().id;
                                var idLista = item["ListaPrecio"].ToString();
                                Producto.idListaPrecios = db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault() == null ? 0 : db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault().id;

                                var idCategoria = item["Categoria"].ToString();
                                Producto.idCategoria = db.Categorias.Where(a => a.CodSAP == idCategoria).FirstOrDefault() == null ? 0 : db.Categorias.Where(a => a.CodSAP == idCategoria).FirstOrDefault().id;

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
                                Producto.Editable = Convert.ToBoolean(Convert.ToInt32(item["Editable"]));
                                var Serie = Convert.ToInt32(item["Serie"]);
                                if (Serie == 1)
                                {
                                    Producto.Serie = true;
                                }
                                else if (Serie == 0)
                                {
                                    Producto.Serie = false;
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

    }
}