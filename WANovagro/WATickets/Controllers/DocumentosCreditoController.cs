using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using SAPbobsCOM;
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
    [Authorize]
    public class DocumentosCreditoController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var time = new DateTime(); // 01-01-0001
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaInicial = filtro.FechaInicial.Date;
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }
                var Creditos = db.EncDocumentoCredito.Select(a => new
                {

                    a.id,
                    a.idCliente,
                    a.idCondPago,
                    a.idVendedor,
                    a.CodSuc,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.TipoDocumento,
                    a.Comentarios,
                    a.Moneda,
                    a.Subtotal,
                    a.TotalImpuestos,
                    a.TotalDescuento,
                    a.TotalCompra,
                    a.PorDescto,
                    a.Status,
                    a.DocEntry,
                    a.DocNum,
                    a.ClaveHacienda,
                    a.ConsecutivoHacienda,
                    a.Saldo,



                    Detalle = db.DetDocumentoCredito.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)).ToList(); //Traemos el listado de productos

                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    Creditos = Creditos.Where(a => a.CodSuc == filtro.Texto).ToList();
                }

                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Creditos = Creditos.Where(a => a.idCliente == filtro.Codigo1).ToList(); // filtramos por lo que traiga el codigo1 
                }

                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    var Fecha = DateTime.Now;
                    Creditos = Creditos.Where(a => a.idCliente == filtro.Codigo2 && a.FechaVencimiento < Fecha && a.Saldo > 0).ToList(); // filtramos por lo que traiga el codigo1 
                }
                if (!string.IsNullOrEmpty(filtro.ItemCode)) // esto por ser string
                {
                    Creditos = Creditos.Where(a => a.Status == filtro.ItemCode).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.Categoria))
                {
                    Creditos = Creditos.Where(a => a.TipoDocumento == filtro.Categoria).ToList();
                }
                if (filtro.Codigo3 > 0) // esto por ser integer
                {
                    Creditos = Creditos.Where(a => a.idCondPago == filtro.Codigo3).ToList();
                }
                if (filtro.Codigo4 > 0) // esto por ser integer
                {
                    Creditos = Creditos.Where(a => a.idVendedor == filtro.Codigo4).ToList();
                }







                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Creditos);
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

        [Route("api/DocumentosCredito/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Credito = db.EncDocumentoCredito.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.idCondPago,
                    a.idVendedor,
                    a.CodSuc,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.TipoDocumento,
                    a.Comentarios,
                    a.Moneda,
                    a.Subtotal,
                    a.TotalImpuestos,
                    a.TotalDescuento,
                    a.TotalCompra,
                    a.PorDescto,
                    a.Status,
                    a.DocEntry,
                    a.DocNum,
                    a.ClaveHacienda,
                    a.ConsecutivoHacienda,
                    a.Saldo,

                    Detalle = db.DetDocumentoCredito.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Credito);
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

        [Route("api/DocumentosCredito/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault();

                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLDocumentoCredito; //Preparo el query


                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Encabezado");


//                var Creditos = db.EncDocumentoCredito.ToList();


                foreach (DataRow item in Ds.Tables["Encabezado"].Rows)
                {
                    var DocEntry = item["DocEntry"].ToString();

                    var EncCredito = db.EncDocumentoCredito.Where(a => a.DocEntry == DocEntry).FirstOrDefault();
                    if (EncCredito == null)
                    {
                        var t = db.Database.BeginTransaction();
                        try
                        {

                            EncCredito = new EncDocumentoCredito();
                            var idCliente = item["Cliente"].ToString();
                            EncCredito.idCliente = db.Clientes.Where(a => a.Codigo == idCliente).FirstOrDefault() == null ? 0 : db.Clientes.Where(a => a.Codigo == idCliente).FirstOrDefault().id;

                            var idVendedor = item["Vendedor"].ToString();
                            EncCredito.idVendedor = db.Vendedores.Where(a => a.CodSAP == idVendedor).FirstOrDefault() == null ? 0 : db.Vendedores.Where(a => a.CodSAP == idVendedor).FirstOrDefault().id;


                            var idCondPago = item["CondPago"].ToString();
                            EncCredito.idCondPago = db.CondicionesPagos.Where(a => a.CodSAP == idCondPago).FirstOrDefault() == null ? 0 : db.CondicionesPagos.Where(a => a.CodSAP == idCondPago).FirstOrDefault().id;
                            var Condicion = db.CondicionesPagos.Where(a => a.CodSAP == idCondPago).FirstOrDefault() == null ? new CondicionesPagos() : db.CondicionesPagos.Where(a => a.CodSAP == idCondPago).FirstOrDefault();
                            var Dias = Condicion.Dias;
                            EncCredito.Fecha = Convert.ToDateTime(item["Fecha"]);
                            EncCredito.FechaVencimiento = EncCredito.Fecha.AddDays(Dias); //Convert.ToDateTime(item["FechaVencimiento"]);

                            EncCredito.Comentarios = item["Comentarios"].ToString();
                            EncCredito.TotalImpuestos = Convert.ToDecimal(item["Impuestos"]);
                            EncCredito.TotalDescuento = Convert.ToDecimal(item["Descuentos"]);
                            EncCredito.TotalCompra = Convert.ToDecimal(item["Total"]);
                            EncCredito.PorDescto = Convert.ToDecimal(item["PorDescto"]);
                            EncCredito.Saldo = Convert.ToDecimal(item["Saldo"]);
                            EncCredito.Subtotal = Convert.ToDecimal(item["Subtotal"]);
                            EncCredito.DocEntry = item["DocEntry"].ToString();
                            EncCredito.DocNum = item["DocNum"].ToString();
                            EncCredito.Status = item["Status"].ToString();
                            EncCredito.Moneda = item["Moneda"].ToString();
                            EncCredito.ClaveHacienda = item["ClaveHacienda"].ToString();
                            EncCredito.ConsecutivoHacienda = item["ConsecutivoHacienda"].ToString();
                            EncCredito.TipoDocumento = "01";
                            db.EncDocumentoCredito.Add(EncCredito);
                            db.SaveChanges();
                            Cn.Close();
                            Cn.Dispose();

                            


                            SQL = parametros.SQLDetDocumentoCredito + "'" + DocEntry + "'";
                            Cn = new SqlConnection(conexion);
                            Cmd = new SqlCommand(SQL, Cn);
                            Da = new SqlDataAdapter(Cmd);
                            Ds = new DataSet();
                            Cn.Open();
                            Da.Fill(Ds, "Detalle");

                            var i = 0;
                            foreach (DataRow item2 in Ds.Tables["Detalle"].Rows)
                            {
                                i++;
                                DetDocumentoCredito det = new DetDocumentoCredito();
                                det.idEncabezado = EncCredito.id;
                                det.NumLinea = Convert.ToInt32(item2["Linea"].ToString());
                                var idProducto = item2["Producto"].ToString();
                                det.idProducto = db.Productos.Where(a => a.Codigo == idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.Codigo == idProducto).FirstOrDefault().id;


                                det.Cantidad = Convert.ToInt32(item2["Cantidad"]);
                                det.PrecioUnitario = Convert.ToDecimal(item2["PrecioUnitario"]);
                                det.PorDescto = Convert.ToDecimal(item2["PorDescto"]);
                                det.Cantidad = Convert.ToInt32(item2["Cantidad"]);
                                det.PrecioUnitario = Convert.ToDecimal(item2["PrecioUnitario"]);
                                det.PorDescto = Convert.ToDecimal(item2["PorDescto"]);
                                det.TotalImpuesto = Convert.ToDecimal(item2["Impuesto"]);
                                det.Descuento = Convert.ToDecimal(item2["Descuento"]);
                                det.TotalLinea = Convert.ToDecimal(item2["TotalLinea"]);

                                db.DetDocumentoCredito.Add(det);
                                db.SaveChanges();

                            }
                            t.Commit();

                            Cn.Close();
                            Cn.Dispose();
                        }
                        catch (Exception ex1)
                        {
                            t.Rollback();
                            //ModelCliente db2 = new ModelCliente();
                            //BitacoraErrores be = new BitacoraErrores();
                            //be.Descripcion = ex1.Message;
                            //be.StrackTrace = ex1.StackTrace;
                            //be.Fecha = DateTime.Now;
                            //be.JSON = JsonConvert.SerializeObject(ex1);
                            //db2.BitacoraErrores.Add(be);
                            //db2.SaveChanges();
                        }


                    }




                    else
                    {
                        var t = db.Database.BeginTransaction();

                        try
                        {
                            db.Entry(EncCredito).State = EntityState.Modified;
                            var idCliente = item["Cliente"].ToString();
                            EncCredito.idCliente = db.Clientes.Where(a => a.Codigo == idCliente).FirstOrDefault() == null ? 0 : db.Clientes.Where(a => a.Codigo == idCliente).FirstOrDefault().id;

                            var idVendedor = item["Vendedor"].ToString();
                            EncCredito.idVendedor = db.Vendedores.Where(a => a.CodSAP == idVendedor).FirstOrDefault() == null ? 0 : db.Vendedores.Where(a => a.CodSAP == idVendedor).FirstOrDefault().id;


                            var idCondPago = item["CondPago"].ToString();
                            EncCredito.idCondPago = db.CondicionesPagos.Where(a => a.CodSAP == idCondPago).FirstOrDefault() == null ? 0 : db.CondicionesPagos.Where(a => a.CodSAP == idCondPago).FirstOrDefault().id;
                            var Condicion = db.CondicionesPagos.Where(a => a.CodSAP == idCondPago).FirstOrDefault() == null ? new CondicionesPagos() : db.CondicionesPagos.Where(a => a.CodSAP == idCondPago).FirstOrDefault();
                            var Dias = Condicion.Dias;
                            EncCredito.Fecha = Convert.ToDateTime(item["Fecha"]);
                            EncCredito.FechaVencimiento = EncCredito.Fecha.AddDays(Dias); //Convert.ToDateTime(item["FechaVencimiento"]);
                           

                            EncCredito.Comentarios = item["Comentarios"].ToString();
                            EncCredito.TotalImpuestos = Convert.ToDecimal(item["Impuestos"]);
                            EncCredito.TotalDescuento = Convert.ToDecimal(item["Descuentos"]);
                            EncCredito.TotalCompra = Convert.ToDecimal(item["Total"]);
                            EncCredito.PorDescto = Convert.ToDecimal(item["PorDescto"]);
                            EncCredito.Saldo = Convert.ToDecimal(item["Saldo"]);
                            EncCredito.Subtotal = Convert.ToDecimal(item["Subtotal"]);
                            EncCredito.DocEntry = item["DocEntry"].ToString();
                            EncCredito.DocNum = item["DocNum"].ToString();
                            EncCredito.Status = item["Status"].ToString();


                            EncCredito.Moneda = item["Moneda"].ToString();
                            EncCredito.ClaveHacienda = item["ClaveHacienda"].ToString();
                            EncCredito.ConsecutivoHacienda = item["ConsecutivoHacienda"].ToString();
                            EncCredito.TipoDocumento = "01";

                            db.SaveChanges();

                            Cn.Close();
                            Cn.Dispose();

                            var detalles = db.DetDocumentoCredito.Where(a => a.idEncabezado == EncCredito.id).ToList();

                            foreach (var item2 in detalles)
                            {
                                db.DetDocumentoCredito.Remove(item2);
                                db.SaveChanges();
                            }


                            SQL = parametros.SQLDetDocumentoCredito + "'" + DocEntry + "'";
                            Cn = new SqlConnection(conexion);
                            Cmd = new SqlCommand(SQL, Cn);
                            Da = new SqlDataAdapter(Cmd);
                            Ds = new DataSet();
                            Cn.Open();
                            Da.Fill(Ds, "Detalle");

                            var i = 0;
                            foreach (DataRow item2 in Ds.Tables["Detalle"].Rows)
                            {
                                i++;
                                DetDocumentoCredito det = new DetDocumentoCredito();
                                det.idEncabezado = EncCredito.id;
                                det.NumLinea = Convert.ToInt32(item2["Linea"].ToString());
                                var idProducto = item2["Producto"].ToString();
                                det.idProducto = db.Productos.Where(a => a.Codigo == idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.Codigo == idProducto).FirstOrDefault().id;


                                det.Cantidad = Convert.ToInt32(item2["Cantidad"]);
                                det.PrecioUnitario = Convert.ToDecimal(item2["PrecioUnitario"]);
                                det.PorDescto = Convert.ToDecimal(item2["PorDescto"]);
                                det.TotalImpuesto = Convert.ToDecimal(item2["Impuesto"]);
                                det.Descuento = Convert.ToDecimal(item2["Descuento"]);
                                det.TotalLinea = Convert.ToDecimal(item2["TotalLinea"]);


                                db.DetDocumentoCredito.Add(det);
                                db.SaveChanges();
                            }

                            t.Commit();
                            

                        }
                        catch (Exception ex1)
                        {
                            t.Rollback();
                            
                        }

                    }




                }
                Cn.Close(); //se cierra la conexion
                Cn.Dispose();

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, "Procesado con exito");
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

        [Route("api/DocumentosCredito/ConsultarByClient")]
        public HttpResponseMessage GetClient([FromUri] int idCliente)
        {
            try
            {
                var Fecha = DateTime.Now;
                var Credito = db.EncDocumentoCredito.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.idCondPago,
                    a.idVendedor,
                    a.CodSuc,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.TipoDocumento,
                    a.Comentarios,
                    a.Moneda,
                    a.Subtotal,
                    a.TotalImpuestos,
                    a.TotalDescuento,
                    a.TotalCompra,
                    a.PorDescto,
                    a.Status,
                    a.DocEntry,
                    a.DocNum,
                    a.ClaveHacienda,
                    a.ConsecutivoHacienda,
                    a.Saldo,

                    Detalle = db.DetDocumentoCredito.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.idCliente == idCliente && a.FechaVencimiento < Fecha && a.Saldo > 0).ToList();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Credito);
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