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
    [Authorize]
    public class ClientesController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/Clientes/InsertarSAP")]
        public HttpResponseMessage GetExtraeDatos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault(); //de aqui nos traemos los querys
                var conexion = G.DevuelveCadena(db); //aqui extraemos la informacion de la tabla de sap para hacerle un query a sap

                var SQL = parametros.SQLClientes; //Preparo el query

                SqlConnection Cn = new SqlConnection(conexion);
                SqlCommand Cmd = new SqlCommand(SQL, Cn);
                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                DataSet Ds = new DataSet();
                Cn.Open(); //se abre la conexion
                Da.Fill(Ds, "Clientes");

                var Clientes = db.Clientes.ToList();
                foreach (DataRow item in Ds.Tables["Clientes"].Rows)
                {
                    var cardCode = item["id"].ToString();

                    var Cliente = Clientes.Where(a => a.Codigo == cardCode).FirstOrDefault();

                    if(Cliente == null) //Existe ?
                    {
                        try
                        {
                            Cliente = new Clientes();
                            Cliente.Codigo = item["id"].ToString();
                            var idLista = item["ListaPrecio"].ToString();
                            Cliente.idListaPrecios = db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault() == null ? 0 : db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault().id;
                            Cliente.Nombre = item["Nombre"].ToString();
                            Cliente.Cedula = item["Cedula"].ToString();

                            switch (Cliente.Cedula.Replace("-", "").Replace("-", "").Length)
                            {
                                case 9:
                                    {
                                        Cliente.TipoCedula = "1";
                                        break;
                                    }
                                case 10:
                                    {
                                        Cliente.TipoCedula = "2";
                                        break;
                                    }
                                default:
                                    {
                                        Cliente.TipoCedula = "3";
                                        break;
                                    }
                            }

                            Cliente.Email = item["Correo"].ToString();
                            Cliente.CodPais = "506";
                            Cliente.Telefono = item["Telefono"].ToString();
                            if(!string.IsNullOrEmpty( item["Provincia"].ToString()))
                            {
                                Cliente.Provincia = Convert.ToInt32(item["Provincia"]);
                                var canton = item["Canton"].ToString();
                                Cliente.Canton = db.Cantones.Where(a => a.CodProvincia == Cliente.Provincia && a.NomCanton.ToUpper().Contains(canton.ToUpper())).FirstOrDefault() == null ? db.Cantones.Where(a => a.CodProvincia == Cliente.Provincia).FirstOrDefault().CodCanton.ToString() : db.Cantones.Where(a => a.CodProvincia == Cliente.Provincia && a.NomCanton.ToUpper().Contains(canton.ToUpper())).FirstOrDefault().CodCanton.ToString();
                                var canton2 = Convert.ToInt32(Cliente.Canton);
                                var distrito = item["Distrito"].ToString();
                                Cliente.Distrito = db.Distritos.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.NomDistrito.ToUpper().Contains(distrito.ToUpper())).FirstOrDefault() == null ? db.Distritos.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2).FirstOrDefault().CodDistrito.ToString() : db.Distritos.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.NomDistrito.ToUpper().Contains(distrito.ToUpper())).FirstOrDefault().CodDistrito.ToString();
                                var distrito2 = Convert.ToInt32(Cliente.Distrito);

                                var barrio = item["Barrio"].ToString();
                                Cliente.Barrio = db.Barrios.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.CodDistrito == distrito2 && a.NomBarrio.ToUpper().Contains(barrio.ToUpper())).FirstOrDefault() == null ? db.Barrios.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.CodDistrito == distrito2).FirstOrDefault().CodBarrio.ToString() : db.Barrios.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.CodDistrito == distrito2 && a.NomBarrio.ToUpper().Contains(barrio.ToUpper())).FirstOrDefault().CodBarrio.ToString();

                                Cliente.Sennas = item["Sennas"].ToString();
                            }
                            else
                            {
                                Cliente.Provincia = 0;
                            }


                            Cliente.Saldo = Convert.ToDecimal(item["Saldo"]);
                            Cliente.Activo = true;
                            Cliente.ProcesadoSAP = true;

                            db.Clientes.Add(Cliente);
                            db.SaveChanges();

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
                        }
                    }
                    else
                    {
                        try
                        {
                            db.Entry(Cliente).State = EntityState.Modified;
                            var idLista = item["ListaPrecio"].ToString();
                            Cliente.idListaPrecios = db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault() == null ? 0 : db.ListaPrecios.Where(a => a.CodSAP == idLista).FirstOrDefault().id;
                            Cliente.Nombre = item["Nombre"].ToString();
                            Cliente.Cedula = item["Cedula"].ToString();

                            switch (Cliente.Cedula.Replace("-", "").Replace("-", "").Length)
                            {
                                case 9:
                                    {
                                        Cliente.TipoCedula = "1";
                                        break;
                                    }
                                case 10:
                                    {
                                        Cliente.TipoCedula = "2";
                                        break;
                                    }
                                default:
                                    {
                                        Cliente.TipoCedula = "3";
                                        break;
                                    }
                            }

                            Cliente.Email = item["Correo"].ToString();
                            Cliente.CodPais = "506";
                            Cliente.Telefono = item["Telefono"].ToString();
                            if(!string.IsNullOrEmpty(item["Provincia"].ToString()))
                            {
                                Cliente.Provincia = Convert.ToInt32(item["Provincia"]);
                                var canton = item["Canton"].ToString();
                                Cliente.Canton = db.Cantones.Where(a => a.CodProvincia == Cliente.Provincia && a.NomCanton.ToUpper().Contains(canton.ToUpper())).FirstOrDefault() == null ? db.Cantones.Where(a => a.CodProvincia == Cliente.Provincia).FirstOrDefault().CodCanton.ToString() : db.Cantones.Where(a => a.CodProvincia == Cliente.Provincia && a.NomCanton.ToUpper().Contains(canton.ToUpper())).FirstOrDefault().CodCanton.ToString();
                                var canton2 = Convert.ToInt32(Cliente.Canton);
                                var distrito = item["Distrito"].ToString();
                                Cliente.Distrito = db.Distritos.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.NomDistrito.ToUpper().Contains(distrito.ToUpper())).FirstOrDefault() == null ? db.Distritos.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2).FirstOrDefault().CodDistrito.ToString() : db.Distritos.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.NomDistrito.ToUpper().Contains(distrito.ToUpper())).FirstOrDefault().CodDistrito.ToString();
                                var distrito2 = Convert.ToInt32(Cliente.Distrito);

                                var barrio = item["Barrio"].ToString();
                                Cliente.Barrio = db.Barrios.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.CodDistrito == distrito2 && a.NomBarrio.ToUpper().Contains(barrio.ToUpper())).FirstOrDefault() == null ? db.Barrios.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.CodDistrito == distrito2).FirstOrDefault().CodBarrio.ToString() : db.Barrios.Where(a => a.CodProvincia == Cliente.Provincia && a.CodCanton == canton2 && a.CodDistrito == distrito2 && a.NomBarrio.ToUpper().Contains(barrio.ToUpper())).FirstOrDefault().CodBarrio.ToString();

                                Cliente.Sennas = item["Sennas"].ToString();
                            }
                            else
                            {
                                Cliente.Provincia = 0;
                            }
                            

                            Cliente.Saldo = Convert.ToDecimal(item["Saldo"]);
                            Cliente.Activo = true;
                            Cliente.ProcesadoSAP = true;
                            db.SaveChanges();
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
                        }
                       
                    }


                }


                Cn.Close(); //se cierra la conexion
                Cn.Dispose();

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, "Procesado con exito");

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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }


        public HttpResponseMessage GetAll([FromUri] Filtros filtro)
        {
            try
            {
                var Clientes = db.Clientes.ToList();
                if (!string.IsNullOrEmpty(filtro.Texto))
                {
                    
                    Clientes = Clientes.Where(a => a.Nombre.ToUpper().Contains(filtro.Texto.ToUpper()) || a.Cedula.ToUpper().Contains(filtro.Texto.ToUpper())
                    || a.Email.ToUpper().Contains(filtro.Texto.ToUpper()) || a.Telefono.ToUpper().Contains(filtro.Texto.ToUpper()) ).ToList();// filtramos por lo que trae texto
                }

                if (filtro.Codigo1 > 0) // esto por ser integer
                {
                    Clientes = Clientes.Where(a => a.idListaPrecios == filtro.Codigo1).ToList();
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Clientes);
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
        [Route("api/Clientes/Consultar")]
        public HttpResponseMessage GetOne([FromUri] string id)
        {
            try
            {
                Clientes clientes = db.Clientes.Where(a => a.Codigo == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, clientes);
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
        [Route("api/Clientes/Insertar")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody] Clientes clientes)
        {
            try
            {
                Clientes Cliente = db.Clientes.Where(a => a.Codigo == clientes.Codigo).FirstOrDefault();
                if (Cliente == null)
                {
                    Cliente = new Clientes();
                    Cliente.Codigo = DevuelveCodigoCliente();
                    var bandera = db.Clientes.Where(a => a.Codigo == Cliente.Codigo).FirstOrDefault() != null; //preguntamos si existe el cliente
                    while(bandera)
                    {
                        
                            Cliente.Codigo = DevuelveCodigoCliente();
                            bandera = db.Clientes.Where(a => a.Codigo == Cliente.Codigo).FirstOrDefault() != null;

                    }
                    

                    Cliente.idListaPrecios = clientes.idListaPrecios;
                    Cliente.Nombre = clientes.Nombre;
                    Cliente.TipoCedula = clientes.TipoCedula;
                    Cliente.Cedula = clientes.Cedula;
                    Cliente.Email = clientes.Email;
                    Cliente.CodPais = "506";
                    Cliente.Telefono = clientes.Telefono;
                    Cliente.Provincia = clientes.Provincia;
                    Cliente.Canton = clientes.Canton;
                    Cliente.Distrito = clientes.Distrito;
                    Cliente.Barrio = clientes.Barrio;
                    Cliente.Sennas = clientes.Sennas;
                    Cliente.Saldo = 0;
                    Cliente.Activo = true;
                    Cliente.ProcesadoSAP = false;
                    db.Clientes.Add(Cliente);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("Ya existe un cliente con este ID");
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
        [Route("api/Clientes/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Clientes clientes)
        {
            try
            {
                Clientes Clientes = db.Clientes.Where(a => a.Codigo == clientes.Codigo).FirstOrDefault();
                if (Clientes != null)
                {
                    db.Entry(Clientes).State = System.Data.Entity.EntityState.Modified;
                    Clientes.idListaPrecios = clientes.idListaPrecios;
                    Clientes.Nombre = clientes.Nombre;
                    Clientes.TipoCedula = clientes.TipoCedula;
                    Clientes.Cedula = clientes.Cedula;
                    Clientes.Email = clientes.Email;
                    Clientes.CodPais = clientes.CodPais;
                    Clientes.Telefono = clientes.Telefono;
                    Clientes.Provincia = clientes.Provincia;
                    Clientes.Canton = clientes.Canton;
                    Clientes.Distrito = clientes.Distrito;
                    Clientes.Barrio = clientes.Barrio;
                    Clientes.Sennas = clientes.Sennas;
                    Clientes.Saldo = clientes.Saldo;
                    Clientes.Activo = clientes.Activo;
                    //Clientes.ProcesadoSAP = clientes.ProcesadoSAP;
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un cliente" +
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
        [Route("api/Clientes/Eliminar")]
        [HttpDelete]
        public HttpResponseMessage Delete([FromUri] string id)
        {
            try
            {
                Clientes Clientes = db.Clientes.Where(a => a.Codigo == id).FirstOrDefault();
                if (Clientes != null)
                {
                    db.Clientes.Remove(Clientes);
                    db.SaveChanges();

                }
                else
                {
                    throw new Exception("No existe un cliente con este ID");
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

        public string DevuelveCodigoCliente()
        {
            try
            {
                var characters = "0123456789";
                var Charsarr = new char[4];
                var random = new Random();

                for (int i = 0; i < Charsarr.Length; i++)
                {
                    Charsarr[i] = characters[random.Next(characters.Length)];
                }

                var resultString = new String(Charsarr);
                return "C" + resultString;
            }
            catch (Exception ex )
            {
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex);
                db.BitacoraErrores.Add(be);
                db.SaveChanges();
                return "";
            }
        }
    }
}