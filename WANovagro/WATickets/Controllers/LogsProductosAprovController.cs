using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.APIS;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class LogsProductosAprovController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();


        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var LogsProductos = db.LogsProductosAprov.Select(a => new
                {
                    a.id,
                    a.idCategoria,
                    a.idProducto,
                    NombreProducto = db.Productos.Where(c => c.id == a.idProducto).FirstOrDefault() == null ? "" : db.Productos.Where(c => c.id == a.idProducto).FirstOrDefault().Nombre,
                    a.idSubCategoria,
                    a.idUsuarioModificador,
                    a.ItemCode,
                    a.Minimo,
                    a.Clasificacion,
                    a.Fecha

                }).AsQueryable();

                if (!string.IsNullOrEmpty(filtro.Buscar))
                {
                    filtro.Buscar = filtro.Buscar.TrimEnd();
                    LogsProductos = LogsProductos.Where(a => a.ItemCode.ToUpper().Contains(filtro.Buscar.ToUpper()));
                    return Request.CreateResponse(HttpStatusCode.OK, LogsProductos.ToList());
                }


                LogsProductos = LogsProductos.Where(a => (filtro.Codigo3 > 0 ? a.idCategoria == filtro.Codigo3 : true)
          && (filtro.Codigo2 > 0 ? a.idSubCategoria == filtro.Codigo2 : true)
  && (filtro.Codigo1 > 0 ? a.idUsuarioModificador == filtro.Codigo1 : true)
          );

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, LogsProductos);
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

        [Route("api/LogsProductosAprov/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] LogsProductosAprovisionamiento logs)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                if (logs.Detalle.Count > 0)
                {
                    var SubCategoria = db.SubCategorias.Where(a => a.Nombre.ToUpper().Contains(logs.PalabraClave.ToUpper())).FirstOrDefault();

                    if (SubCategoria == null)
                    {
                        SubCategoria = new SubCategorias();
                        SubCategoria.Nombre = logs.PalabraClave;
                        SubCategoria.idCategoria = logs.idCategoria;
                        SubCategoria.FechaActualizacion = DateTime.Now;
                        SubCategoria.ProcesadoSAP = false;
                        db.SubCategorias.Add(SubCategoria);
                        db.SaveChanges();

                        var Datos = db.ConexionSAP.FirstOrDefault();





                        var SQL = "INSERT INTO " + Datos.SQLBD + ".dbo.[@NPOS_SUBCA] (Code, Name, U_idCategoria) VALUES (" + SubCategoria.id + "," + SubCategoria.Nombre + "," + SubCategoria.idCategoria + ")";

                        db.Database.ExecuteSqlCommand(SQL);



                        db.Entry(SubCategoria).State = EntityState.Modified;
                        SubCategoria.ProcesadoSAP = true;
                        db.SaveChanges();
                    }





                    foreach (var item in logs.Detalle)
                    {

                        LogsProductosAprov det = new LogsProductosAprov();
                        det.idProducto = item.idProducto;
                        det.idCategoria = item.idCategoria;
                        det.idSubCategoria = SubCategoria.id;
                        det.idUsuarioModificador = logs.idUsuarioModificador;
                        det.Minimo = item.Minimo;

                        det.Fecha = DateTime.Now;
                        det.Clasificacion = item.Clasificacion;
                        det.ItemCode = item.ItemCode;
                        db.LogsProductosAprov.Add(det);
                        db.SaveChanges();

                        var Producto = db.Productos.Where(a => a.id == det.idProducto).FirstOrDefault();
                        if (Producto != null)
                        {
                            db.Entry(Producto).State = EntityState.Modified;
                            Producto.Minimo = det.Minimo;
                            Producto.Clasificacion = det.Clasificacion;
                            Producto.idSubCategoria = det.idSubCategoria;
                            db.SaveChanges();

                            try
                            {
                                //var client = (SAPbobsCOM.IItemWarehouseInfo)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.ItemW);

                                var Bodega = db.Bodegas.Where(a => a.id == Producto.idBodega).FirstOrDefault();
                                //if (client.GetByKey(Producto.Codigo))
                                //{
                                
                                //    oitemWarehouseInfo = oItems.WhsInfo;

                                //    oitemWarehouseInfo.SetCurrentLine("your warehouse line number");

                                //    oitemWarehouseInfo.MinimalOrder = 501;

                                //    if (oItems.Update() != 0)

                                //    {

                                //        MessageBox.Show(oComp.GetLastErrorDescription());

                                //    }
                                //}

                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }

                    }

                    t.Commit();




                }
                else
                {
                    throw new Exception("Se debe ingresar productos para poder guardar");
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                t.Rollback();
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex);
                db.BitacoraErrores.Add(be);
                db.SaveChanges();

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, be);
            }
        }
    }
}