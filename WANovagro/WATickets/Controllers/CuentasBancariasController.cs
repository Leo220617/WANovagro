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
    public class CuentasBancariasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


       

        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var CuentasBancarias = db.CuentasBancarias.ToList();

                if(!string.IsNullOrEmpty(filtro.Texto))
                {
                    CuentasBancarias = CuentasBancarias.Where(a => a.CodSuc == filtro.Texto).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, CuentasBancarias);
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

        [Route("api/CuentasBancarias/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                CuentasBancarias cuentasBancarias = db.CuentasBancarias.Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, cuentasBancarias);
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

        [Route("api/CuentasBancarias/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] CuentasBancarias cuentas)
        {
            try
            {
                CuentasBancarias Cuentas = db.CuentasBancarias.Where(a => a.id == cuentas.id).FirstOrDefault();
                if (Cuentas == null)
                {
                    Cuentas = new CuentasBancarias();
                    Cuentas.id = cuentas.id;
                    Cuentas.CodSuc = cuentas.CodSuc;
                    Cuentas.Nombre = cuentas.Nombre;
                    Cuentas.CuentaSAP = cuentas.CuentaSAP;
                    Cuentas.Estado = true;
                    db.CuentasBancarias.Add(Cuentas);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una cuenta con este ID");
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

        [Route("api/CuentasBancarias/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] CuentasBancarias cuentas)
        {
            try
            {
                CuentasBancarias Cuentas = db.CuentasBancarias.Where(a => a.id == cuentas.id).FirstOrDefault();
                if (Cuentas != null)
                {
                    db.Entry(Cuentas).State = System.Data.Entity.EntityState.Modified;
                    Cuentas.CodSuc = cuentas.CodSuc;
                    Cuentas.Nombre = cuentas.Nombre;
                    Cuentas.CuentaSAP = cuentas.CuentaSAP;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una Cuenta Bancaria" +
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
        [Route("api/CuentasBancarias/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                CuentasBancarias Cuentas = db.CuentasBancarias.Where(a => a.id == id).FirstOrDefault();
                if (Cuentas != null)
                {
                    db.Entry(Cuentas).State = EntityState.Modified;


                    if (Cuentas.Estado)
                    {

                        Cuentas.Estado = false;

                    }
                    else
                    {

                        Cuentas.Estado = true;
                    }




                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("No existe una cuenta bancaria con este ID");
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