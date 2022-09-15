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
    
    public class UsuariosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var Usuarios = db.Usuarios.ToList();

                if(filtro.Codigo1 > 0)
                {
                    Usuarios = Usuarios.Where(a => a.idRol == filtro.Codigo1).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Usuarios);
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
        [Route("api/Usuarios/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Usuarios usuarios = db.Usuarios.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, usuarios);
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
        [Route("api/Usuarios/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Usuarios usuarios)
        {
            try
            {
                Usuarios Usuario = db.Usuarios.Where(a => a.id == usuarios.id).FirstOrDefault();
                if (Usuario == null)
                {
                    Usuario = new Usuarios();
                    Usuario.id = usuarios.id;
                    Usuario.idRol = usuarios.idRol;
                    Usuario.Nombre = usuarios.Nombre;
                    Usuario.NombreUsuario = usuarios.NombreUsuario;
                    Usuario.Clave = BCrypt.Net.BCrypt.HashPassword(usuarios.Clave);
                    Usuario.Activo = true;
                    Usuario.FecUltSup = DateTime.Now;
                    db.Usuarios.Add(Usuario);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un usuario con este ID");
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
        [Route("api/Usuarios/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Usuarios usuarios)
        {
            try
            {
                Usuarios Usuarios = db.Usuarios.Where(a => a.id == usuarios.id).FirstOrDefault();
                if (Usuarios != null)
                {
                    db.Entry(Usuarios).State = System.Data.Entity.EntityState.Modified;

                    if(usuarios.idRol > 0)
                    {
                        Usuarios.idRol = usuarios.idRol;

                    }
                    if(!string.IsNullOrEmpty(usuarios.Nombre))
                    {
                        Usuarios.Nombre = usuarios.Nombre;

                    }
                    if (!string.IsNullOrEmpty(usuarios.NombreUsuario))
                    {
                        Usuarios.NombreUsuario = usuarios.NombreUsuario;
                    }

                    if (!string.IsNullOrEmpty(usuarios.Clave))
                    {
                        Usuarios.Clave = BCrypt.Net.BCrypt.HashPassword(usuarios.Clave);
                    }

                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un usuario" +
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
        [Route("api/Usuarios/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Usuarios Usuarios = db.Usuarios.Where(a => a.id == id).FirstOrDefault();
                if (Usuarios != null)
                {
                    db.Entry(Usuarios).State = EntityState.Modified;


                    if (Usuarios.Activo)
                    {

                        Usuarios.Activo = false;

                    }
                    else
                    {

                        Usuarios.Activo = true;
                    }




                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("No existe un usuario con este ID");
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