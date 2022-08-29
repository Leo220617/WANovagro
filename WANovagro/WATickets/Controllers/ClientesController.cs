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
    public class ClientesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var Clientes = db.Clientes.ToList();
                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    
                    Clientes = Clientes.Where(a => a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper()) || a.Cedula.ToUpper().Contains(filtro.Texto.ToUpper())
                    || a.Email.ToUpper().Contains(filtro.Texto.ToUpper()) || a.Telefono.ToUpper().Contains(filtro.Texto.ToUpper()) ).ToList();// filtramos por lo que trae texto
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Clientes);
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
        [Route("api/Clientes/Consultar")]
        public HttpResponseMessage GetOne([FromUri] string id)
        {
            try
            {
                Clientes clientes = db.Clientes.Where(a => a.Codigo == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, clientes);
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
        [Route("api/Clientes/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Clientes clientes)
        {
            try
            {
                Clientes Cliente = db.Clientes.Where(a => a.Codigo == clientes.Codigo).FirstOrDefault();
                if (Cliente == null)
                {
                    Cliente = new Clientes();
                    Cliente.Codigo = clientes.Codigo;
                    Cliente.idListaPrecios = clientes.idListaPrecios;
                    Cliente.Nombre = clientes.Nombre;
                    Cliente.TipoCedula = clientes.TipoCedula;
                    Cliente.Cedula = clientes.Cedula;
                    Cliente.Email = clientes.Email;
                    Cliente.CodPais = clientes.CodPais;
                    Cliente.Telefono = clientes.Telefono;
                    Cliente.Provincia = clientes.Provincia;
                    Cliente.Canton = clientes.Canton;
                    Cliente.Distrito = clientes.Distrito;
                    Cliente.Barrio = clientes.Barrio;
                    Cliente.Sennas = clientes.Sennas;
                    Cliente.Saldo = 0;
                    Cliente.Activo = true;
                    Cliente.ProcesadoSAP = false;
                    db.Clientes.Add(Cliente);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un cliente con este ID");
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
        [Route("api/Clientes/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Clientes clientes)
        {
            try
            {
                Clientes Clientes = db.Clientes.Where(a => a.Codigo == clientes.Codigo).FirstOrDefault();
                if (Clientes != null)
                {
                    db.Entry(Clientes).State = System.Data.Entity.EntityState.Modified;
                    Clientes.idListaPrecios = clientes.idListaPrecios;
                    Clientes.Nombre = clientes.Nombre;
                    Clientes.TipoCedula = clientes.TipoCedula;
                    Clientes.Cedula = clientes.Cedula;
                    Clientes.Email = clientes.Email;
                    Clientes.CodPais = clientes.CodPais;
                    Clientes.Telefono = clientes.Telefono;
                    Clientes.Provincia = clientes.Provincia;
                    Clientes.Canton = clientes.Canton;
                    Clientes.Distrito = clientes.Distrito;
                    Clientes.Barrio = clientes.Barrio;
                    Clientes.Sennas = clientes.Sennas;
                    Clientes.Saldo = clientes.Saldo;
                    Clientes.Activo = clientes.Activo;
                    //Clientes.ProcesadoSAP = clientes.ProcesadoSAP;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un cliente" +
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
        [Route("api/Clientes/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] string id)
        {
            try
            {
                Clientes Clientes = db.Clientes.Where(a => a.Codigo == id).FirstOrDefault();
                if (Clientes != null)
                {
                    db.Clientes.Remove(Clientes);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un cliente con este ID");
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