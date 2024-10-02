using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    public class DetPromocionesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll([FromUri] Filtros filtros)
        {
            try
            {
                var time = DateTime.Now.Date;
                var Detalles = db.Promociones.Select(a => new {
                    a.id,
                    a.idEncabezado,
                    a.ItemCode,
                    a.idCategoria,
                    a.idListaPrecio,
                    a.PrecioFinal,
                    a.Moneda,
                    a.FechaVen,
                    a.Fecha,
                    a.PrecioAnterior,
                    a.Cliente,
                    ClientesPromociones = db.ClientesPromociones.Where(b => b.idPromocion == a.idEncabezado).ToList()

                }).Where(a => (filtros.Activo ? a.Fecha <= time && a.FechaVen >= time : true)).ToList();
           
               


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Detalles);
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