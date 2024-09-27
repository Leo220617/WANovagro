using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class SeriesProductosController : ApiController
    {

        ModelCliente db = new ModelCliente();
        G G = new G();
        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {

                var Bodegas = db.Bodegas.Where(a => a.CodSuc == filtro.Texto).Select(a => a.CodSAP).ToList();
                var BodegasIN = new StringBuilder();

                for (int i = 0; i < Bodegas.Count; i++)
                {
                    BodegasIN.Append($"'{Bodegas[i]}'"); // Añade cada elemento con comillas simples.

                    if (i < Bodegas.Count - 1)
                    {
                        BodegasIN.Append(","); // Añade una coma solo si no es el último elemento.
                    }
                } 

                var Parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db);
                var SQL = Parametros.SQLSeriesProductos + " and t2.WhsCode in (" + BodegasIN.ToString() + ") ";
                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open();
                Da.Fill(Ds, "SeriesProductos");


                Cn.Close();
                Cn.Dispose();

                
                return Request.CreateResponse(HttpStatusCode.OK, Ds);
            }
            catch (Exception ex1)
            {
                ModelCliente db2 = new ModelCliente();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex1.Message;
                be.StrackTrace = ex1.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex1);
                db2.BitacoraErrores.Add(be);
                db2.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex1);
            }
        }
    }
}

