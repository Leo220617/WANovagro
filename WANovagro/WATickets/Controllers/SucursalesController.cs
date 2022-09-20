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
    }
}