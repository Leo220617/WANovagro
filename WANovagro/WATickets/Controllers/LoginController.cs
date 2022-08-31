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
    [EnableCors("*", "*", "*")]
    public class LoginController : ApiController
    {
        ModelCliente db = new ModelCliente();
        [Route("api/Login/Conectar")] //Este metodo lo hacemos entre los dos 
        public async Task<HttpResponseMessage> GetLoginAsync([FromUri] string nombreUsuario, string clave)
        {
                try
                {
                    if (!string.IsNullOrEmpty(nombreUsuario) && !string.IsNullOrEmpty(clave))
                    {
                        var Usuario = db.Usuarios.Where(a => a.NombreUsuario.ToUpper().Contains(nombreUsuario.ToUpper())).FirstOrDefault();

                        if (Usuario == null)
                        {
                            throw new Exception("Usuario o clave incorrecta");
                        }

                        if (!BCrypt.Net.BCrypt.Verify(clave, Usuario.Clave))
                        {
                            throw new Exception("Clave o Usuario incorrectos");
                        }
                        if(!Usuario.Activo)
                        {
                            throw new Exception("Usuario desactivado");
                        }
                        var token = TokenGenerator.GenerateTokenJwt(Usuario.Nombre, Usuario.id.ToString());
                        var SeguridadModulos = db.SeguridadRolesModulos.Where(a => a.CodRol == Usuario.idRol).ToList();


                    DevolcionLogin de = new DevolcionLogin();
                                de.id = Usuario.id;
                                de.Nombre = Usuario.Nombre;
                                de.idRol = Usuario.idRol;
                                de.Clave = "";
                                de.Activo = Usuario.Activo;
                                de.Email = Usuario.NombreUsuario;
                                de.CodigoVendedor = "";
                                de.token = token;
                                de.Seguridad = SeguridadModulos;
                    

                                return Request.CreateResponse(HttpStatusCode.OK, de);

                    }
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Debe incluir usuario y clave");
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




        public class DevolcionLogin
    {
        public int id { get; set; }

        public int? idRol { get; set; }

        
        public string Email { get; set; }

        
        public string Nombre { get; set; }

        public bool? Activo { get; set; }

        
        public string Clave { get; set; }
        public string CodigoVendedor { get; set; }
        public string token { get; set; }
        public List<SeguridadRolesModulos> Seguridad { get; set; }
    }
}