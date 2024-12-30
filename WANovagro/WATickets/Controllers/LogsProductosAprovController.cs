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

                    if (SubCategoria == null && logs.PalabraClave != "")
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
                        var Producto = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                        LogsProductosAprov det = new LogsProductosAprov();
                        det.idProducto = item.idProducto;
                        det.idCategoria = item.idCategoria;
                        det.Solo = item.Solo;
                        if(det.Solo == false)
                        {
                            det.idSubCategoria = SubCategoria.id;
                        }
                        else
                        {
                            det.idSubCategoria = Producto.idSubCategoria;
                        }
                    
                        det.idUsuarioModificador = logs.idUsuarioModificador;
                        det.Minimo = item.Minimo;
                     
                        det.Fecha = DateTime.Now;
                        det.Clasificacion = item.Clasificacion;
                        det.ItemCode = item.ItemCode;
                        db.LogsProductosAprov.Add(det);
                        db.SaveChanges();

                      
                        if (Producto != null)
                        {
                            db.Entry(Producto).State = EntityState.Modified;
                            Producto.Minimo = det.Minimo;
                            Producto.Clasificacion = det.Clasificacion;
                            if(det.Solo == false)
                            {
                                Producto.idSubCategoria = det.idSubCategoria;
                            }
                      
                            db.SaveChanges();

                            try
                            {
                                var client = (SAPbobsCOM.Items)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                                var Bodega = db.Bodegas.Where(a => a.id == Producto.idBodega).FirstOrDefault();
                                if (client.GetByKey(Producto.Codigo))
                                {
                                    client.UserFields.Fields.Item("U_CategoriaABC").Value = Producto.Clasificacion;
                                    if (det.Solo == false)
                                    {
                                        client.UserFields.Fields.Item("U_SubCategoria").Value = item.idSubCategoria.ToString();
                                    }
                                    bool warehouseFound = false;
                                    for (int i = 0; i < client.WhsInfo.Count; i++)
                                    {
                                        client.WhsInfo.SetCurrentLine(i);

                                        if (client.WhsInfo.WarehouseCode == Bodega.CodSAP)
                                        {
                                            // Actualizar el MinStock para el almacén especificado
                                            client.WhsInfo.MinimalStock = Convert.ToDouble(Producto.Minimo); // Nuevo valor de MinStock
                                            warehouseFound = true;
                                            break;
                                        }
                                    }

                                    if (warehouseFound)
                                    {
                                        var resp = client.Update();
                                        if(resp == 0)
                                        {
                                            db.Entry(Producto).State = EntityState.Modified;
                                            Producto.FechaActualizacion = DateTime.Now;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            throw new Exception("Error al actualizar minimo en SAP " + Conexion.Company.GetLastErrorDescription());
                                        }
                                    }

                                }

                            }
                            catch (Exception ex )
                            {
                                ModelCliente db2 = new ModelCliente();
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = ex.Message;
                                be.StrackTrace = ex.StackTrace;
                                be.Fecha = DateTime.Now;
                                be.JSON = JsonConvert.SerializeObject(ex);
                                db2.BitacoraErrores.Add(be);
                                db2.SaveChanges();
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

        [HttpGet]
        [Route("api/LogsProductosAprov/SincronizarSAP")]
        public HttpResponseMessage GetSincronizar()
        {
            try
            {
                var Productos = db.LogsProductosAprov.Where(a => a.ProcesadoSAP == false).ToList();
                Parametros param = db.Parametros.FirstOrDefault();

                foreach(var item in Productos)
                {
                    try
                    {
                        var SubCategoria = db.SubCategorias.Where(a => a.id == item.idSubCategoria && a.ProcesadoSAP == false).FirstOrDefault();

                        if (SubCategoria != null)
                        {
                             

                            var Datos = db.ConexionSAP.FirstOrDefault();
                             

                            var SQL = "INSERT INTO [" + Datos.SQLBD + "].dbo.[@NPOS_SUBCA] (Code, Name, U_idCategoria) VALUES (" + SubCategoria.id + ",'" + SubCategoria.Nombre + "'," + SubCategoria.idCategoria + ")";

                            db.Database.ExecuteSqlCommand(SQL);



                            db.Entry(SubCategoria).State = EntityState.Modified;
                            SubCategoria.ProcesadoSAP = true;
                            db.SaveChanges();
                        }


                        var client = (SAPbobsCOM.Items)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);
                        var Producto = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                        var Bodega = db.Bodegas.Where(a => a.id == Producto.idBodega).FirstOrDefault();
                        if (client.GetByKey(item.ItemCode))
                        {
                            client.UserFields.Fields.Item("U_CategoriaABC").Value = item.Clasificacion;
                            if (item.Solo == false)
                            {
                                client.UserFields.Fields.Item("U_SubCategoria").Value = item.idSubCategoria.ToString();
                            }
                            bool warehouseFound = false;
                            for (int i = 0; i < client.WhsInfo.Count; i++)
                            {
                                client.WhsInfo.SetCurrentLine(i);

                                if (client.WhsInfo.WarehouseCode == Bodega.CodSAP)
                                {
                                    // Actualizar el MinStock para el almacén especificado
                                    client.WhsInfo.MinimalStock = Convert.ToDouble(item.Minimo); // Nuevo valor de MinStock
                                    warehouseFound = true;
                                    break;
                                }
                            }

                            if (warehouseFound)
                            {
                                var resp = client.Update();
                                if (resp == 0)
                                {
                                    db.Entry(Producto).State = EntityState.Modified;
                                    Producto.FechaActualizacion = DateTime.Now;
                                    db.SaveChanges();

                                    db.Entry(item).State = EntityState.Modified;
                                    item.ProcesadoSAP = true;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    throw new Exception("Error al actualizar minimo en SAP " + Conexion.Company.GetLastErrorDescription());
                                }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        ModelCliente db2 = new ModelCliente();
                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = ex.Message;
                        be.StrackTrace = ex.StackTrace;
                        be.Fecha = DateTime.Now;
                        be.JSON = JsonConvert.SerializeObject(ex);
                        db2.BitacoraErrores.Add(be);
                        db2.SaveChanges();
                    }
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