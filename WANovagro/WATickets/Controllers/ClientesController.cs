using Newtonsoft.Json;
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
                            Cliente.Cedula = item["Cedula"].ToString().Replace("-", "").Replace("-", "");
                            

                            switch (Cliente.Cedula.Replace("-", "").Replace("-", "").Length)
                            {
                                case 9:
                                    {
                                        Cliente.TipoCedula = "01";
                                        break;
                                    }
                                case 10:
                                    {
                                        Cliente.TipoCedula = "02";
                                        break;
                                    }
                                default:
                                    {
                                        Cliente.TipoCedula = "03";
                                        break;
                                    }
                            }

                            Cliente.Email = item["Correo"].ToString();
                            Cliente.CodPais = "506";
                            Cliente.Telefono = item["Telefono"].ToString();
                            if (!string.IsNullOrEmpty(item["Provincia"].ToString()))
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

                            Cliente.CorreoPublicitario = "";



                                Cliente.Saldo = Convert.ToDecimal(item["Saldo"]);
                            Cliente.Activo = true;
                            Cliente.ProcesadoSAP = true;

                            var idCond = item["idCondPago"].ToString();
                            Cliente.idCondicionPago = db.CondicionesPagos.Where(a => a.CodSAP == idCond).FirstOrDefault() == null ? 1 : db.CondicionesPagos.Where(a => a.CodSAP == idCond).FirstOrDefault().id;

                            var idGrupo = item["idGrupo"].ToString();
                            Cliente.idGrupo = db.GruposClientes.Where(a => a.CodSAP == idGrupo).FirstOrDefault() == null ? 1 : db.GruposClientes.Where(a => a.CodSAP == idGrupo).FirstOrDefault().id;

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
                            Cliente.Cedula = item["Cedula"].ToString().Replace("-", "").Replace("-", "");

                            switch (Cliente.Cedula.Replace("-", "").Replace("-", "").Length)
                            {
                                case 9:
                                    {
                                        Cliente.TipoCedula = "01";
                                        break;
                                    }
                                case 10:
                                    {
                                        Cliente.TipoCedula = "02";
                                        break;
                                    }
                                default:
                                    {
                                        Cliente.TipoCedula = "03";
                                        break;
                                    }
                            }

                            Cliente.Email = item["Correo"].ToString();
                            Cliente.CodPais = "506";
                            Cliente.Telefono = item["Telefono"].ToString();
                           
                            if (!string.IsNullOrEmpty(item["Provincia"].ToString()))
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
                            var idCond = item["idCondPago"].ToString();
                            Cliente.idCondicionPago = db.CondicionesPagos.Where(a => a.CodSAP == idCond).FirstOrDefault() == null ? 1 : db.CondicionesPagos.Where(a => a.CodSAP == idCond).FirstOrDefault().id;
                            var idGrupo = item["idGrupo"].ToString();
                            Cliente.idGrupo = db.GruposClientes.Where(a => a.CodSAP == idGrupo).FirstOrDefault() == null ? 1 : db.GruposClientes.Where(a => a.CodSAP == idGrupo).FirstOrDefault().id;
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

                if (filtro.Procesado != null) 
                {
                    Clientes = Clientes.Where(a => a.ProcesadoSAP == filtro.Procesado).ToList();
                }

                if (filtro.Codigo2 > 0) // esto por ser integer
                {
                    Clientes = Clientes.Where(a => a.idGrupo == filtro.Codigo2).ToList();
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
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                Clientes clientes = db.Clientes.Where(a => a.id == id).FirstOrDefault();


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
                Clientes Cliente = db.Clientes.Where(a => a.id == clientes.id).FirstOrDefault();
                if (Cliente == null)
                {
                    if (db.Clientes.Where(a => a.Cedula == clientes.Cedula && a.TipoCedula == clientes.TipoCedula).FirstOrDefault() == null)
                    {
                        Cliente = new Clientes();
                        Cliente.Codigo = DevuelveCodigoCliente();
                        var bandera = db.Clientes.Where(a => a.id == Cliente.id).FirstOrDefault() != null; //preguntamos si existe el cliente
                        while (bandera)
                        {

                            Cliente.Codigo = DevuelveCodigoCliente();
                            bandera = db.Clientes.Where(a => a.id == Cliente.id).FirstOrDefault() != null;

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
                        Cliente.idCondicionPago = db.CondicionesPagos.Where(a => a.Nombre.ToLower().Contains("Contado".ToLower())).FirstOrDefault() == null ? 1 : db.CondicionesPagos.Where(a => a.Nombre.ToLower().Contains("Contado".ToLower())).FirstOrDefault().id;
                        Cliente.idGrupo = clientes.idGrupo;
                        Cliente.Saldo = 0;
                        Cliente.Activo = true;
                        Cliente.ProcesadoSAP = false;

                        Cliente.CorreoPublicitario = clientes.CorreoPublicitario;
                        db.Clientes.Add(Cliente);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Ya existe un cliente con esta cedula");
                    }



                }
                else
                {
                    throw new Exception("Ya existe un cliente con este ID");
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Cliente);
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
            var t = db.Database.BeginTransaction();
            try
            {

                Clientes Clientes = db.Clientes.Where(a => a.id == clientes.id).FirstOrDefault();
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
                   
                    Clientes.idGrupo = clientes.idGrupo;
                    //Clientes.ProcesadoSAP = clientes.ProcesadoSAP;
                    Clientes.CorreoPublicitario = clientes.CorreoPublicitario;
                    db.SaveChanges();


                    try
                    {
                        if(Clientes.ProcesadoSAP)
                        {
                            var client = (SAPbobsCOM.BusinessPartners)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                            if (client.GetByKey(Clientes.Codigo))
                            {
                                client.EmailAddress = clientes.Email;
                                
                                client.CardForeignName = clientes.Cedula;
                                client.FederalTaxID = clientes.Cedula;
                                client.AdditionalID = clientes.Cedula;

                                client.GroupCode = db.GruposClientes.Where(a => a.id == clientes.idGrupo).FirstOrDefault() == null ? Convert.ToInt32(db.GruposClientes.FirstOrDefault()) : Convert.ToInt32(db.GruposClientes.Where(a => a.id == clientes.idGrupo).FirstOrDefault().CodSAP);
                                
                                client.Phone1 = clientes.Telefono;
                            

                                //Campos de usuario
                                client.UserFields.Fields.Item("U_LDT_TelLoc").Value = Convert.ToInt32(clientes.CodPais);
                                client.UserFields.Fields.Item("U_LDT_IDType").Value = Convert.ToInt32(clientes.TipoCedula);
                                client.UserFields.Fields.Item("U_LDT_Country").Value = "CR";
                                client.UserFields.Fields.Item("U_LDT_State").Value = clientes.Provincia.ToString();
                                switch (clientes.Provincia)
                                {
                                    case 1:
                                        {
                                            client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "San Jose";
                                            break;
                                        }
                                    case 2:
                                        {
                                            client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Alajuela";
                                            break;
                                        }
                                    case 3:
                                        {
                                            client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Cartago";
                                            break;
                                        }
                                    case 4:
                                        {
                                            client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Heredia";
                                            break;
                                        }
                                    case 5:
                                        {
                                            client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Guanacaste";
                                            break;
                                        }
                                    case 6:
                                        {
                                            client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Puntarenas";
                                            break;
                                        }
                                    case 7:
                                        {
                                            client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Limon";
                                            break;
                                        }
                                }
                                client.UserFields.Fields.Item("U_LDT_County").Value = clientes.Provincia.ToString() + "-" + clientes.Canton;
                                var canton = Convert.ToInt32(clientes.Canton);
                                client.UserFields.Fields.Item("U_LDT_Nom_County").Value = db.Cantones.Where(a => a.CodProvincia == clientes.Provincia && a.CodCanton == canton).FirstOrDefault().NomCanton;
                                //client.UserFields.Fields.Item("U_LDT_County").Value = cliente.Provincia + "-" + cliente.Canton;
                                client.UserFields.Fields.Item("U_LDT_District").Value = clientes.Provincia.ToString() + "-" + clientes.Canton + "-" + clientes.Distrito;
                                var distrito = Convert.ToInt32(clientes.Distrito);
                                client.UserFields.Fields.Item("U_LDT_Nom_District").Value = db.Distritos.Where(a => a.CodProvincia == clientes.Provincia && a.CodCanton == canton && a.CodDistrito == distrito).FirstOrDefault().NomDistrito;
                                client.UserFields.Fields.Item("U_LDT_NeighB").Value = clientes.Provincia.ToString() + "-" + clientes.Canton + "-" + clientes.Distrito + "-" + clientes.Barrio;
                                var barrio = Convert.ToInt32(clientes.Barrio);
                                client.UserFields.Fields.Item("U_LDT_Nom_NeighB").Value = db.Barrios.Where(a => a.CodProvincia == clientes.Provincia && a.CodCanton == canton && a.CodDistrito == distrito && a.CodBarrio == barrio).FirstOrDefault().NomBarrio;
                                client.UserFields.Fields.Item("U_LDT_Direccion").Value = clientes.Sennas;
                                var respuesta = client.Update();

                                if (respuesta == 0)
                                {
                                    
                                    Conexion.Desconectar();
                                }
                                else
                                {
                                    BitacoraErrores be = new BitacoraErrores();

                                    be.Descripcion = Conexion.Company.GetLastErrorDescription();
                                    be.StrackTrace = "Editar Cliente";
                                    be.Fecha = DateTime.Now;
                                    be.JSON = JsonConvert.SerializeObject(clientes);
                                    db.BitacoraErrores.Add(be);
                                    db.SaveChanges();
                                    Conexion.Desconectar();
                                }

                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                    t.Commit();
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
                t.Rollback();

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
        public HttpResponseMessage Delete([FromUri] int id)
        {
            try
            {
                Clientes Clientes = db.Clientes.Where(a => a.id == id).FirstOrDefault();
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

        [HttpGet]
        [Route("api/Clientes/SincronizarSAP")]
        public HttpResponseMessage GetSincronizar([FromUri] int id)
        {
            try
            {
                Clientes cliente = db.Clientes.Where(a => a.id == id).FirstOrDefault();

                if (cliente != null)
                {
                    var client = (SAPbobsCOM.BusinessPartners)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                    client.CardName = cliente.Nombre;
                    client.EmailAddress = cliente.Email;
                    client.Series = 70; //Serie para clientes
                    client.CardForeignName = cliente.Cedula; 
                    client.FederalTaxID = cliente.Cedula;
                    client.AdditionalID = cliente.Cedula;
                    client.GroupCode = db.GruposClientes.Where(a => a.id == cliente.idGrupo).FirstOrDefault() == null ? Convert.ToInt32(db.GruposClientes.FirstOrDefault()) : Convert.ToInt32(db.GruposClientes.Where(a => a.id == cliente.idGrupo).FirstOrDefault().CodSAP);
                    client.Currency = "##";
                    client.Phone1 = cliente.Telefono;
                    client.CardType = BoCardTypes.cCustomer;

                    //Campos de usuario
                    client.UserFields.Fields.Item("U_LDT_TelLoc").Value = Convert.ToInt32(cliente.CodPais);
                    client.UserFields.Fields.Item("U_LDT_IDType").Value = Convert.ToInt32(cliente.TipoCedula);
                    client.UserFields.Fields.Item("U_LDT_Country").Value = "CR";
                    client.UserFields.Fields.Item("U_LDT_State").Value = cliente.Provincia.ToString();
                    switch(cliente.Provincia)
                    {
                        case 1:
                            {
                                client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "San Jose";
                                break;
                            }
                        case 2:
                            {
                                client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Alajuela";
                                break;
                            }
                        case 3:
                            {
                                client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Cartago";
                                break;
                            }
                        case 4:
                            {
                                client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Heredia";
                                break;
                            }
                        case 5:
                            {
                                client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Guanacaste";
                                break;
                            }
                        case 6:
                            {
                                client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Puntarenas";
                                break;
                            }
                        case 7:
                            {
                                client.UserFields.Fields.Item("U_LDT_Nom_State").Value = "Limon";
                                break;
                            }
                    }
                    client.UserFields.Fields.Item("U_LDT_County").Value = cliente.Provincia.ToString() + "-" + cliente.Canton;
                    var canton = Convert.ToInt32(cliente.Canton);
                    client.UserFields.Fields.Item("U_LDT_Nom_County").Value = db.Cantones.Where(a => a.CodProvincia == cliente.Provincia && a.CodCanton == canton).FirstOrDefault().NomCanton;
                    //client.UserFields.Fields.Item("U_LDT_County").Value = cliente.Provincia + "-" + cliente.Canton;
                    client.UserFields.Fields.Item("U_LDT_District").Value = cliente.Provincia.ToString() + "-" + cliente.Canton + "-" + cliente.Distrito;
                    var distrito = Convert.ToInt32(cliente.Distrito);
                    client.UserFields.Fields.Item("U_LDT_Nom_District").Value = db.Distritos.Where(a => a.CodProvincia == cliente.Provincia && a.CodCanton == canton && a.CodDistrito == distrito).FirstOrDefault().NomDistrito;
                    client.UserFields.Fields.Item("U_LDT_NeighB").Value = cliente.Provincia.ToString() + "-" + cliente.Canton + "-" + cliente.Distrito + "-" + cliente.Barrio;
                    var barrio = Convert.ToInt32(cliente.Barrio);
                    client.UserFields.Fields.Item("U_LDT_Nom_NeighB").Value = db.Barrios.Where(a => a.CodProvincia == cliente.Provincia && a.CodCanton == canton && a.CodDistrito == distrito && a.CodBarrio == barrio).FirstOrDefault().NomBarrio;
                    client.UserFields.Fields.Item("U_LDT_Direccion").Value = cliente.Sennas;

                    var respuesta = client.Add();

                    if (respuesta == 0)
                    {
                        db.Entry(cliente).State = EntityState.Modified;
                        cliente.Codigo = Conexion.Company.GetNewObjectKey();
                        cliente.ProcesadoSAP = true;
                        db.SaveChanges();
                        Conexion.Desconectar();
                    }
                    else
                    {
                        BitacoraErrores be = new BitacoraErrores();

                        be.Descripcion = Conexion.Company.GetLastErrorDescription();
                        be.StrackTrace = "Crear Cliente";
                        be.Fecha = DateTime.Now;
                        be.JSON = JsonConvert.SerializeObject(cliente);
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();
                        Conexion.Desconectar();
                    }

                }
                else
                {
                    throw new Exception("El cliente no existe");
                }
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, cliente);
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