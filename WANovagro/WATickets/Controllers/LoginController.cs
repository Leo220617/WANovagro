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
                    if (!Usuario.Activo)
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


        [Route("api/Login/ConectarPOS")] //Este metodo lo hacemos entre los dos 
        public async Task<HttpResponseMessage> GetLoginPOSAsync([FromUri]string CodSuc, int idCaja, string nombreUsuario, string clave, string ip)
        {
            try
            {
                if (!string.IsNullOrEmpty(nombreUsuario) && !string.IsNullOrEmpty(clave) && !string.IsNullOrEmpty(CodSuc) && idCaja > 0)
                {


                    var Sucursal = db.Sucursales.Where(a => a.CodSuc == CodSuc).FirstOrDefault();
                    if(Sucursal == null)
                    {
                        throw new Exception("Sucursal no existe");

                    }

                    var Caja = db.Cajas.Where(a => a.CodSuc == CodSuc && a.id == idCaja).FirstOrDefault();

                    if (Caja == null)
                    {
                        throw new Exception("Caja no existe");

                    }



                    var Usuario = db.Usuarios.Where(a => a.NombreUsuario.ToUpper().Contains(nombreUsuario.ToUpper())).FirstOrDefault();
                    var UsuariosSucursales = db.UsuariosSucursales.Where(a => a.CodSuc == CodSuc && a.idUsuario == Usuario.id).FirstOrDefault();

                    if(UsuariosSucursales == null)
                    {
                        throw new Exception("Usuario no pertenece a esta sucursal");
                    }

                    if (Usuario == null)
                    {
                        throw new Exception("Usuario o clave incorrecta");
                    }
                    if(Usuario.id != Caja.idUsuario)
                    {
                        throw new Exception("Usuario no esta asignado a esta caja");
                    }
                    if (!BCrypt.Net.BCrypt.Verify(clave, Usuario.Clave))
                    {
                        throw new Exception("Clave o Usuario incorrectos");
                    }
                    if (!Usuario.Activo)
                    {
                        throw new Exception("Usuario desactivado");
                    }
                    var token = TokenGenerator.GenerateTokenJwt(Usuario.Nombre, Usuario.id.ToString());
                    var SeguridadModulos = db.SeguridadRolesModulos.Where(a => a.CodRol == Usuario.idRol).ToList();

                    var FechaActual = DateTime.Now.Date;
                    var CierreCaja = db.CierreCajas.Where(a => a.idCaja == Caja.id && a.FechaCaja == FechaActual && a.Activo == true).FirstOrDefault();

                    if(CierreCaja == null)
                    {
                        CierreCaja = db.CierreCajas.Where(a => a.idCaja == Caja.id && a.FechaCaja == FechaActual && a.idUsuario == Usuario.id && a.Activo == false).FirstOrDefault();
                        if(CierreCaja != null)
                        {
                            throw new Exception("No se puede abrir una caja ya cerrada por un cajero");
                        }

                        CierreCaja = new CierreCajas();
                        CierreCaja.idCaja = Caja.id;
                        CierreCaja.idUsuario = Usuario.id;
                        CierreCaja.FechaCaja = FechaActual;
                        CierreCaja.FecUltAct = DateTime.Now;
                        CierreCaja.IP = ip;
                        CierreCaja.EfectivoColones = 0;
                        CierreCaja.EfectivoFC = 0;
                        CierreCaja.ChequesColones = 0;
                        CierreCaja.ChequesFC = 0;
                        CierreCaja.TarjetasColones = 0;
                        CierreCaja.TarjetasFC = 0;
                        CierreCaja.OtrosMediosColones = 0;
                        CierreCaja.OtrosMediosFC = 0;
                        CierreCaja.TotalVendidoColones = 0;
                        CierreCaja.TotalVendidoFC = 0;
                        CierreCaja.TotalRegistradoColones = 0;
                        CierreCaja.TotalRegistradoFC = 0;
                        CierreCaja.TotalAperturaColones = db.Cajas.Where(a => a.id == CierreCaja.idCaja).FirstOrDefault() == null ? 0 : db.Cajas.Where(a => a.id == CierreCaja.idCaja).FirstOrDefault().MontoAperturaColones;
                        CierreCaja.TransferenciasColones = 0;
                        CierreCaja.TransferenciasDolares = 0;
                        CierreCaja.TotalAperturaFC = db.Cajas.Where(a => a.id == CierreCaja.idCaja).FirstOrDefault() == null ? 0 : db.Cajas.Where(a => a.id == CierreCaja.idCaja).FirstOrDefault().MontoAperturaDolares; //Pone el monto de apertura de acuerdo a la caja que abre
                        CierreCaja.NotasCreditoColones = 0;
                        CierreCaja.NotasCreditoFC = 0;

                        CierreCaja.EfectivoColonesC = 0;
                        CierreCaja.EfectivoFCC = 0;
                        CierreCaja.ChequesColonesC = 0;
                        CierreCaja.ChequesFCC = 0;
                        CierreCaja.TarjetasColonesC = 0;
                        CierreCaja.TarjetasFCC = 0;
                        CierreCaja.OtrosMediosColonesC = 0;
                        CierreCaja.OtrosMediosFCC = 0;
                       
                        
                        CierreCaja.TransferenciasColonesC = 0;
                        CierreCaja.TransferenciasDolaresC = 0;
                       
                        CierreCaja.Activo = true;
                        CierreCaja.HoraCierre = DateTime.Now;
                        CierreCaja.TotalizadoMonedas = 0;
                        db.CierreCajas.Add(CierreCaja);
                        db.SaveChanges();
                    }
                    else
                    {
                        if(CierreCaja.idUsuario != Usuario.id)
                        {
                            throw new Exception("Caja abierta por otro usuario, realice cierre de caja para utilizarla");
                        }

                           if(CierreCaja.IP != ip)
                            {
                                BitacoraMovimientos bm = new BitacoraMovimientos();
                                bm.idUsuario = Usuario.id;
                                bm.Metodo = "Apertura Caja";
                                bm.Fecha = DateTime.Now;
                                bm.Descripcion = "El usuario con la ip: '" + ip + "' ha iniciado la caja " + Caja.id + " - " + Caja.Nombre;
                                db.BitacoraMovimientos.Add(bm);
                                db.SaveChanges();
                            }
                            db.Entry(CierreCaja).State = EntityState.Modified;
                            CierreCaja.FecUltAct = DateTime.Now;
                            db.SaveChanges();
                        

                    }


                    DevolucionPOS de = new DevolucionPOS();
                    de.id = Usuario.id;
                    de.Nombre = Usuario.Nombre;
                    de.idRol = Usuario.idRol;
                    de.Clave = "";
                    de.Activo = Usuario.Activo;
                    de.Email = Usuario.NombreUsuario;
                    de.CodigoVendedor = "";
                    de.idCaja = Caja.id;
                    de.Caja = Caja.Nombre;
                    de.CodSuc = Sucursal.CodSuc;
                    de.token = token;
                    de.Seguridad = SeguridadModulos;
                    de.PIN = Usuario.PIN;
                    de.Imagen = Sucursal.Imagen;
                    return Request.CreateResponse(HttpStatusCode.OK, de);

                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Debe incluir todos los datos respectivos");
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

    public class DevolucionPOS
    {
        public int id { get; set; }

        public int? idRol { get; set; }
        public int idCierre { get; set; }
        public int idCaja { get; set; }
        public string Caja { get; set; }
        public string CodSuc { get; set; }

        public string Email { get; set; }


        public string Nombre { get; set; }

        public bool? Activo { get; set; }


        public string Clave { get; set; }
        public string CodigoVendedor { get; set; }
        public string token { get; set; }
        public byte[] Imagen { get; set; }
        public string PIN { get; set; }
        public List<SeguridadRolesModulos> Seguridad { get; set; }
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