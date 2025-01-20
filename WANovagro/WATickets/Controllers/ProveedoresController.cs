using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class ProveedoresController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var Parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db);
                var SQL = Parametros.SQLProveedores;
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "Proveedores");


                Cn.Close();
                Cn.Dispose();


                return Request.CreateResponse(HttpStatusCode.OK, Ds);

            }
            catch (Exception ex)
            {
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace.ToString();
                be.Fecha = DateTime.Now;
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


    }
}