using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    public class Conexion
    {
        public readonly static Conexion _instance = new Conexion();
        public static Company _company = null;
        ModelCliente db = new ModelCliente();
        G G = new G();


        private static Conexion Instance
        {
            get
            {
                return _instance;
            }
        }

        public static Company Company
        {
            get
            {

                if (_company == null)
                    new Conexion().DoSapConnection();



                var ins = Instance;
                return _company;
            }
        }
        public int DoSapConnection()
        {
            try
            {
               
                var Datos = db.ConexionSAP.FirstOrDefault();

                G.GuardarTxt("Conexion.txt", Datos.SAPUser+ "- " + Datos.ServerLicense + "-" + Datos.SAPPass + "-"+ Datos.ServerSQL + "-" + Datos.SQLBD + "-" + Datos.SQLType);

                _company = new Company
                {
                    Server = Datos.ServerSQL,
                    LicenseServer = Datos.ServerLicense,
                    DbServerType = getBDType(Datos.SQLType),
                    language = BoSuppLangs.ln_English,
                    CompanyDB = Datos.SQLBD,
                    UserName = Datos.SAPUser,
                    Password = Datos.SAPPass
                };

                var resp = _company.Connect();


                if (resp != 0)
                {
                    var msg = _company.GetLastErrorDescription();
                    G.GuardarTxt("ErrorConexionSAP.txt", msg.ToString());
                    return -1;
                }

                return resp;
            }
            catch (Exception ex)
            {


                G.GuardarTxt("ErrorConexionSAP.txt", ex.ToString());

                return -1;
            }

        }

        private BoDataServerTypes getBDType(string sql)
        {
            switch (sql)
            {
                case "2005":
                    return BoDataServerTypes.dst_MSSQL2005;
                case "2008":
                    return BoDataServerTypes.dst_MSSQL2008;
                case "2012":
                    return BoDataServerTypes.dst_MSSQL2012;
                case "2014":
                    return BoDataServerTypes.dst_MSSQL2014;
                case "2016":
                    return BoDataServerTypes.dst_MSSQL2016;
                //case "2017":
                //    return BoDataServerTypes.dst_MSSQL2017;
                case "HANA":
                    return BoDataServerTypes.dst_HANADB;
                default:
                    return BoDataServerTypes.dst_MSSQL;
            }
        }

        public static bool Desconectar()
        {
            try
            {
                if (_company != null)
                {
                    _company = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;

            }
        }
    }
}