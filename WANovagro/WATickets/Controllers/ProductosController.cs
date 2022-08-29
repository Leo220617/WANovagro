using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        public HttpResponseMessage GetAll()
        {
            try
            {
                var Productos = db.Productos.ToList();


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
        public HttpResponseMessage GetOne([FromUri] string id)
        {
            try
            {
                Productos productos = db.Productos.Where(a => a.Codigo == id).FirstOrDefault();


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
                Productos Producto = db.Productos.Where(a => a.Codigo == productos.Codigo).FirstOrDefault();
                if (Producto == null)
                {
                    Producto = new Productos();
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
                    Producto.Activo = productos.Activo;
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
                Productos Productos = db.Productos.Where(a => a.Codigo == productos.Codigo).FirstOrDefault();
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
        public HttpResponseMessage Delete([FromUri] string id)
        {
            try
            {
                Productos Productos = db.Productos.Where(a => a.Codigo == id).FirstOrDefault();
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
    }
}