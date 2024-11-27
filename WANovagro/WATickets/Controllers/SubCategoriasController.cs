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
    [Authorize]
    public class SubCategoriasController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var SubCategorias = db.SubCategorias.ToList();

                if (filtro.Codigo1 > 0)
                {
                    SubCategorias = SubCategorias.Where(a => a.idCategoria == filtro.Codigo1).ToList();
                }
                   if (filtro.Procesado != null)
                {
                    SubCategorias = SubCategorias.Where(a => a.ProcesadoSAP == filtro.Procesado).ToList();
                }
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, SubCategorias);
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