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
    public class UsuariosSucursalesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var UsuariosS = db.UsuariosSucursales.ToList();


                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    // and = &&, or = ||
                    UsuariosS = UsuariosS.Where(a => a.CodSuc.ToUpper().Contains(filtro.Texto.ToUpper())).ToList();// filtramos por lo que trae texto
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, UsuariosS);
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
        [Route("api/UsuariosSucursales/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] UsuariosSucursales usuariossucursales)
        {
            try
            {
                UsuariosSucursales UsuarioSucursal = db.UsuariosSucursales.Where(a => a.idUsuario == usuariossucursales.idUsuario & a.CodSuc == usuariossucursales.CodSuc).FirstOrDefault();
                if (UsuarioSucursal == null)
                {
                    UsuarioSucursal = new UsuariosSucursales();
                    UsuarioSucursal.idUsuario = usuariossucursales.idUsuario;
                    UsuarioSucursal.CodSuc = usuariossucursales.CodSuc;
                    db.UsuariosSucursales.Add(UsuarioSucursal);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un UsuarioSucursal con este ID");
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

        [Route("api/UsuariosSucursales/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id, string cod)
        {
            try
            {
                UsuariosSucursales UsuariosSucursales = db.UsuariosSucursales.Where(a => a.idUsuario == id & a.CodSuc == cod).FirstOrDefault();
                if (UsuariosSucursales != null)
                {
                    db.UsuariosSucursales.Remove(UsuariosSucursales);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un UsuarioSucursal con este ID");
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