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
using WATickets.Models.APIS;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    
    public class SucursalesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll()
        {
            try
            {
                var Sucursales = db.Sucursales.ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Sucursales);
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
        [Route("api/Sucursales/Consultar")]
        public HttpResponseMessage GetOne([FromUri] string id)
        {
            try
            {
                Sucursales sucursales = db.Sucursales.Where(a => a.CodSuc == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, sucursales);
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
        [Route("api/Sucursales/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] SucursalesViewModel sucursales)
        {
            try
            {
                Sucursales Sucursal = db.Sucursales.Where(a => a.CodSuc == sucursales.CodSuc).FirstOrDefault();
                if (Sucursal == null)
                {
                    Sucursal = new Sucursales();
                    Sucursal.CodSuc = sucursales.CodSuc;
                    Sucursal.Nombre = sucursales.Nombre;
                    byte[] hex = Convert.FromBase64String(sucursales.Imagen.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));
                     
                    Sucursal.Imagen = hex;
                    Sucursal.TipoCedula = sucursales.TipoCedula;
                    Sucursal.Cedula = sucursales.Cedula;
                    Sucursal.Provincia = sucursales.Provincia;
                    Sucursal.Canton = sucursales.Canton;
                    Sucursal.Distrito = sucursales.Distrito;
                    Sucursal.Barrio = sucursales.Barrio;
                    Sucursal.Sennas = sucursales.Sennas;
                    Sucursal.Telefono = sucursales.Telefono;
                    Sucursal.Correo = sucursales.Correo;
                    db.Sucursales.Add(Sucursal);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe una sucursal con este ID");
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
        [Route("api/Sucursales/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] SucursalesViewModel sucursales)
        {
            try
            {
                Sucursales Sucursal = db.Sucursales.Where(a => a.CodSuc == sucursales.CodSuc).FirstOrDefault();
                if (Sucursal != null)
                {
                    db.Entry(Sucursal).State = System.Data.Entity.EntityState.Modified;
                    Sucursal.Nombre = sucursales.Nombre;
                    byte[] hex = Convert.FromBase64String(sucursales.Imagen.Replace("data:image/jpeg;base64,", "").Replace("data:image/png;base64,", ""));

                    Sucursal.Imagen = hex;
                    Sucursal.TipoCedula = sucursales.TipoCedula;
                    Sucursal.Cedula = sucursales.Cedula;
                    Sucursal.Provincia = sucursales.Provincia;
                    Sucursal.Canton = sucursales.Canton;
                    Sucursal.Distrito = sucursales.Distrito;
                    Sucursal.Barrio = sucursales.Barrio;
                    Sucursal.Sennas = sucursales.Sennas;
                    Sucursal.Telefono = sucursales.Telefono;
                    Sucursal.Correo = sucursales.Correo;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una sucursal" +
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
        [Route("api/Sucursales/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] string id)
        {
            try
            {
                Sucursales Sucursales = db.Sucursales.Where(a => a.CodSuc == id).FirstOrDefault();
                if (Sucursales != null)
                {
                    db.Sucursales.Remove(Sucursales);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe una sucursal con este ID");
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