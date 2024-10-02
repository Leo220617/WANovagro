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
    public class ClientesPromocionesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var time = DateTime.Now.Date;
                var Detalles = db.Promociones.ToList();

                var Promocion = db.EncPromociones.Where(a => a.Fecha <= time && a.FechaVencimiento >= time).ToList();
                var ClientesPromociones = db.ClientesPromociones.ToList();

   

                if (filtro.Activo)
                {
                    var promocionesIds = Promocion.Select(p => p.id).ToList();
                    ClientesPromociones = ClientesPromociones
                        .Where(a => promocionesIds.Contains(a.idPromocion))
                        .ToList();
                }


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, ClientesPromociones);
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
       
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ClientesPromociones[] objeto)
        {
            var t = db.Database.BeginTransaction();
            try
            {
                var primero = objeto[0].idPromocion;
                var clientesPromociones = db.ClientesPromociones.Where(a => a.idPromocion == primero).ToList();
                foreach (var item in clientesPromociones)
                {
                    var Objeto = db.ClientesPromociones.Where(a => a.idPromocion == item.idPromocion && a.idCliente == item.idCliente).FirstOrDefault();

                    if (Objeto != null)
                    {
                        db.ClientesPromociones.Remove(Objeto);
                        db.SaveChanges();
                    }
                }

                foreach (var item in objeto)
                {



                    var Objeto = db.ClientesPromociones.Where(a => a.idPromocion == item.idPromocion && a.idCliente == item.idCliente).FirstOrDefault();

                    if (Objeto == null)
                    {
                        var Objetos = new ClientesPromociones();
                        Objetos.idPromocion = item.idPromocion;
                        Objetos.idCliente = item.idCliente;


                        db.ClientesPromociones.Add(Objetos);
                        db.SaveChanges();

                    }


                }
                t.Commit();
                return Request.CreateResponse(HttpStatusCode.OK, objeto);
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

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                t.Rollback();

            }
        }

        [Route("api/ClientesPromociones/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] int idCliente, int idPromocion)
        {
            try
            {
                ClientesPromociones ClientesPromociones = db.ClientesPromociones.Where(a => a.idCliente == idCliente & a.idPromocion == idPromocion).FirstOrDefault();
                if (ClientesPromociones != null)
                {
                    db.ClientesPromociones.Remove(ClientesPromociones);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un ClientePromocion con este ID");
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