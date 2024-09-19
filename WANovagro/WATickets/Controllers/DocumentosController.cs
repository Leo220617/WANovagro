
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
using WATickets.Models.APIS;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]

    public class DocumentosController : ApiController
    {
        ModelCliente db = new ModelCliente();
        G G = new G();

        [Route("api/Documentos/SincronizarSAP")]
        public HttpResponseMessage GetExtraeDatos([FromUri] int id)
        {
            try
            {
                var Documento = db.EncDocumento.Where(a => a.id == id).FirstOrDefault();
                var param = db.Parametros.FirstOrDefault();
                if (Documento.id != null)
                {
                    //Inserccion NC SAP
                    if (Documento.TipoDocumento == "03") // Si es una nota de credito
                    {
                        try
                        {
                            var DocumentoG = db.EncDocumento.Where(a => a.id == Documento.BaseEntry).FirstOrDefault();
                            var DetalleG = db.DetDocumento.Where(a => a.idEncabezado == DocumentoG.id).ToList();
                            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Documento.CodSuc).FirstOrDefault();
                            var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                            //Encabezado

                            documentoSAP.DocObjectCode = BoObjectTypes.oCreditNotes;
                            documentoSAP.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                            documentoSAP.DocCurrency = Documento.Moneda == "CRC" ? param.MonedaLocal : Documento.Moneda;
                            documentoSAP.DocDate = Documento.Fecha;
                            //documentoSAP.DocDueDate = Documento.FechaVencimiento;

                            //documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
                            documentoSAP.NumAtCard = "APP NC" + " " + Documento.id;
                            documentoSAP.Comments = Documento.Comentarios;

                            // documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().CodSAP);

                            documentoSAP.Series = Sucursal.SerieNC; //Quemada


                            if (Documento.Moneda == "USD")
                            {
                                documentoSAP.DocTotalFc = Convert.ToDouble(Documento.TotalCompra + Documento.Redondeo);
                            }
                            else
                            {
                                documentoSAP.DocTotal = Convert.ToDouble(Documento.TotalCompra + Documento.Redondeo);
                            }

                            //documentoSAP.GroupNumber = -1;
                            documentoSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault().CodSAP);
                            documentoSAP.UserFields.Fields.Item(param.CampoConsecutivo).Value = Documento.ConsecutivoHacienda;
                            documentoSAP.UserFields.Fields.Item(param.CampoClave).Value = Documento.ClaveHacienda;
                            var Lotes1 = db.Lotes.Where(a => a.idEncabezado == Documento.id && a.Tipo == "F").ToList();

                            documentoSAP.UserFields.Fields.Item("U_DYD_Estado").Value = "A";



                            //Detalle
                            int z = 0;
                            var Detalle = db.DetDocumento.Where(a => a.idEncabezado == id).ToList();
                            foreach (var item in Detalle)
                            {


                                var BodProducto = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                                var Bodega = db.Bodegas.Where(a => a.id == BodProducto).FirstOrDefault();


                                documentoSAP.Lines.SetCurrentLine(z);

                                documentoSAP.Lines.Currency = Documento.Moneda == "CRC" ? param.MonedaLocal : Documento.Moneda;
                                documentoSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                                documentoSAP.Lines.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                                documentoSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                var idImp = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idImpuesto;
                                var ClienteMAG = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? false : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().MAG;
                                var ProductoMAG = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? false : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().MAG;
                                var Producto = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                                var ProductoCabys = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Cabys;
                                var DetExoneracion = db.DetExoneraciones.Where(a => a.CodCabys == ProductoCabys && a.idEncabezado == item.idExoneracion).FirstOrDefault() == null ? 0 : db.DetExoneraciones.Where(a => a.CodCabys == ProductoCabys && a.idEncabezado == item.idExoneracion).FirstOrDefault().id;
                                if (ClienteMAG == true && ProductoMAG == true && DetExoneracion == 0)
                                {

                                    documentoSAP.Lines.TaxCode = db.Impuestos.Where(a => a.Tarifa == 1).FirstOrDefault() == null ? "IVA-1" : db.Impuestos.Where(a => a.Tarifa == 1).FirstOrDefault().Codigo;

                                }
                                else
                                {
                                    documentoSAP.Lines.TaxCode = item.idExoneracion > 0 ? "EXONERA" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().Codigo; //Revisar
                                    if (item.idExoneracion > 0)
                                    {
                                        var Exoneracion = db.Exoneraciones.Where(a => a.id == item.idExoneracion).FirstOrDefault();

                                        documentoSAP.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Exoneracion.TipoDoc;
                                        documentoSAP.Lines.UserFields.Fields.Item("U_NumDoc").Value = Exoneracion.NumDoc;
                                        documentoSAP.Lines.UserFields.Fields.Item("U_NomInst").Value = Exoneracion.NomInst;
                                        documentoSAP.Lines.UserFields.Fields.Item("U_FecEmis").Value = Exoneracion.FechaEmision;
                                    }
                                }
                                documentoSAP.Lines.TaxOnly = BoYesNoEnum.tNO;


                                documentoSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                                var idBod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                                documentoSAP.Lines.WarehouseCode = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? "01" : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault().CodSAP;
                                //documentoSAP.Lines.BaseType = Convert.ToInt32(SAPbobsCOM.BoObjectTypes.oInvoices);
                                //documentoSAP.Lines.BaseEntry = Convert.ToInt32(DocumentoG.DocEntry);


                                if (param.CostingCode != "N" && param.CostingCode2 != "N" && param.CostingCode3 != "N")
                                {
                                    documentoSAP.Lines.CostingCode = param.CostingCode;
                                    //documentoSAP.Lines.CostingCode2 = param.CostingCode2;
                                    documentoSAP.Lines.CostingCode2 = Sucursal.NormaReparto; //Cambiar progra para Novagro
                                    documentoSAP.Lines.CostingCode3 = param.CostingCode3;

                                }
                                else
                                {
                                    switch (Sucursal.Dimension)
                                    {
                                        case 1:
                                            {
                                                documentoSAP.Lines.CostingCode = Sucursal.NormaReparto;

                                                break;
                                            }
                                        case 2:
                                            {
                                                documentoSAP.Lines.CostingCode2 = Sucursal.NormaReparto;
                                                break;
                                            }
                                        case 3:
                                            {
                                                documentoSAP.Lines.CostingCode3 = Sucursal.NormaReparto;
                                                break;
                                            }
                                        case 4:
                                            {
                                                documentoSAP.Lines.CostingCode4 = Sucursal.NormaReparto;
                                                break;
                                            }
                                        case 5:
                                            {
                                                documentoSAP.Lines.CostingCode5 = Sucursal.NormaReparto;
                                                break;
                                            }

                                    }

                                    switch (Producto.Dimension)
                                    {
                                        case 1:
                                            {
                                                documentoSAP.Lines.CostingCode = Producto.NormaReparto;

                                                break;
                                            }
                                        case 2:
                                            {
                                                documentoSAP.Lines.CostingCode2 = Producto.NormaReparto;
                                                break;
                                            }
                                        case 3:
                                            {
                                                documentoSAP.Lines.CostingCode3 = Producto.NormaReparto;
                                                break;
                                            }
                                        case 4:
                                            {
                                                documentoSAP.Lines.CostingCode4 = Producto.NormaReparto;
                                                break;
                                            }
                                        case 5:
                                            {
                                                documentoSAP.Lines.CostingCode5 = Producto.NormaReparto;
                                                break;
                                            }

                                    }
                                }

                                //documentoSAP.Lines.BaseLine = DetalleG.Where(a => a.idProducto == item.idProducto).FirstOrDefault() == null ? 0 : DetalleG.Where(a => a.idProducto == item.idProducto).FirstOrDefault().NumLinea;

                                //documentoSAP.Lines.CostingCode4 = "";
                                //documentoSAP.Lines.CostingCode5 = "";

                                var ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                                var Lotes2 = Lotes1.Where(a => a.ItemCode == ItemCode && a.idDetalle == item.id).ToList();

                                var x = 0;
                                foreach (var lot in Lotes2)
                                {


                                    documentoSAP.Lines.SerialNumbers.ManufacturerSerialNumber = lot.Serie;
                                    documentoSAP.Lines.SerialNumbers.ItemCode = lot.ItemCode;
                                    documentoSAP.Lines.SerialNumbers.Quantity = Convert.ToDouble(lot.Cantidad);

                                    documentoSAP.Lines.SerialNumbers.Add();


                                    x++;
                                }

                                documentoSAP.Lines.Add();
                                z++;



                            }

                            if (Documento.Redondeo != 0)
                            {
                                documentoSAP.Rounding = BoYesNoEnum.tYES;
                                documentoSAP.RoundingDiffAmount = Convert.ToDouble(Documento.Redondeo);
                            }

                            var respuesta = documentoSAP.Add();
                            if (respuesta == 0) //se creo exitorsamente 
                            {
                                db.Entry(Documento).State = EntityState.Modified;
                                Documento.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                                Documento.ProcesadaSAP = true;
                                db.SaveChanges();





                                Conexion.Desconectar();

                            }
                            else
                            {
                                var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = error;
                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                be.Fecha = DateTime.Now;
                                be.JSON = JsonConvert.SerializeObject(DocumentoG);
                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                                Conexion.Desconectar();


                            }
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

                        }
                    }
                    else
                    {
                        try
                        {
                            var Sucursal = db.Sucursales.Where(a => a.CodSuc == Documento.CodSuc).FirstOrDefault();
                            var documentoSAP = (Documents)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                            //Encabezado

                            documentoSAP.DocObjectCode = BoObjectTypes.oInvoices;
                            documentoSAP.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                            documentoSAP.DocCurrency = Documento.Moneda == "CRC" ? param.MonedaLocal : Documento.Moneda;
                            var Dias = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? 0 : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().Dias;

                            documentoSAP.DocDate = Documento.Fecha;
                            documentoSAP.DocDueDate = Documento.Fecha.AddDays(Dias);
                            documentoSAP.DocType = BoDocumentTypes.dDocument_Items;
                            documentoSAP.NumAtCard = "APP FAC:" + " " + Documento.id;
                            documentoSAP.Comments = Documento.Comentarios;
                            documentoSAP.PaymentGroupCode = Convert.ToInt32(db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().CodSAP);
                            var CondPago = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? "0" : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault().Nombre;
                            documentoSAP.Series = CondPago.ToLower().Contains("contado") ? Sucursal.SerieFECO : Sucursal.SerieFECR;  //4;  //param.SerieProforma; //Quemada





                            if (Documento.Moneda == "USD")
                            {
                                documentoSAP.DocTotalFc = Convert.ToDouble(Documento.TotalCompra + Documento.Redondeo);
                            }
                            else
                            {
                                documentoSAP.DocTotal = Convert.ToDouble(Documento.TotalCompra + Documento.Redondeo);
                            }



                            documentoSAP.SalesPersonCode = Convert.ToInt32(db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault() == null ? "0" : db.Vendedores.Where(a => a.id == Documento.idVendedor).FirstOrDefault().CodSAP);
                            documentoSAP.UserFields.Fields.Item(param.CampoConsecutivo).Value = Documento.ConsecutivoHacienda; //"U_LDT_NumeroGTI"
                            documentoSAP.UserFields.Fields.Item(param.CampoClave).Value = Documento.ClaveHacienda;       //"U_LDT_FiscalDoc"
                            //documentoSAP.UserFields.Fields.Item("U_DYD_Estado").Value = "A";
                            var Lotes1 = db.Lotes.Where(a => a.idEncabezado == Documento.id && a.Tipo == "F").ToList();
                            //Detalle
                            int z = 0;
                            var Detalle = db.DetDocumento.Where(a => a.idEncabezado == id).ToList();
                            foreach (var item in Detalle)
                            {
                                var BodProducto = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                                var Bodega = db.Bodegas.Where(a => a.id == BodProducto).FirstOrDefault();
                                documentoSAP.Lines.SetCurrentLine(z);

                                documentoSAP.Lines.Currency = Documento.Moneda == "CRC" ? param.MonedaLocal : Documento.Moneda;
                                documentoSAP.Lines.Quantity = Convert.ToDouble(item.Cantidad);
                                documentoSAP.Lines.DiscountPercent = Convert.ToDouble(item.PorDescto);
                                documentoSAP.Lines.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;



                                var idImp = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idImpuesto;
                                var ClienteMAG = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? false : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().MAG;
                                var ProductoMAG = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? false : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().MAG;
                                var Producto = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();

                                var ProductoCabys = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Cabys;
                                var DetExoneracion = db.DetExoneraciones.Where(a => a.CodCabys == ProductoCabys && a.idEncabezado == item.idExoneracion).FirstOrDefault() == null ? 0 : db.DetExoneraciones.Where(a => a.CodCabys == ProductoCabys && a.idEncabezado == item.idExoneracion).FirstOrDefault().id;
                                if (ClienteMAG == true && ProductoMAG == true && DetExoneracion == 0)
                                {

                                    documentoSAP.Lines.TaxCode = db.Impuestos.Where(a => a.Tarifa == 1).FirstOrDefault() == null ? "IVA-1" : db.Impuestos.Where(a => a.Tarifa == 1).FirstOrDefault().Codigo;

                                }
                                else
                                {
                                    documentoSAP.Lines.TaxCode = item.idExoneracion > 0 ? "EXONERA" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault() == null ? "IV" : db.Impuestos.Where(a => a.id == idImp).FirstOrDefault().Codigo;
                                    if (item.idExoneracion > 0)
                                    {
                                        var Exoneracion = db.Exoneraciones.Where(a => a.id == item.idExoneracion).FirstOrDefault();

                                        documentoSAP.Lines.UserFields.Fields.Item("U_Tipo_Doc").Value = Exoneracion.TipoDoc;
                                        documentoSAP.Lines.UserFields.Fields.Item("U_NumDoc").Value = Exoneracion.NumDoc;
                                        documentoSAP.Lines.UserFields.Fields.Item("U_NomInst").Value = Exoneracion.NomInst;
                                        documentoSAP.Lines.UserFields.Fields.Item("U_FecEmis").Value = Exoneracion.FechaEmision;
                                    }
                                }
                                documentoSAP.Lines.TaxOnly = BoYesNoEnum.tNO;



                                documentoSAP.Lines.UnitPrice = Convert.ToDouble(item.PrecioUnitario);
                                var idBod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? 0 : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().idBodega;
                                documentoSAP.Lines.WarehouseCode = db.Bodegas.Where(a => a.id == idBod).FirstOrDefault() == null ? "01" : db.Bodegas.Where(a => a.id == idBod).FirstOrDefault().CodSAP;
                                if (param.CostingCode != "N" && param.CostingCode2 != "N" && param.CostingCode3 != "N")
                                {
                                    documentoSAP.Lines.CostingCode = param.CostingCode;
                                    //documentoSAP.Lines.CostingCode2 = param.CostingCode2;
                                    documentoSAP.Lines.CostingCode2 = Sucursal.NormaReparto; //Cambiar progra para Novagro
                                    documentoSAP.Lines.CostingCode3 = param.CostingCode3;

                                }
                                else
                                {
                                    switch (Sucursal.Dimension)
                                    {
                                        case 1:
                                            {
                                                documentoSAP.Lines.CostingCode = Sucursal.NormaReparto;

                                                break;
                                            }
                                        case 2:
                                            {
                                                documentoSAP.Lines.CostingCode2 = Sucursal.NormaReparto;
                                                break;
                                            }
                                        case 3:
                                            {
                                                documentoSAP.Lines.CostingCode3 = Sucursal.NormaReparto;
                                                break;
                                            }
                                        case 4:
                                            {
                                                documentoSAP.Lines.CostingCode4 = Sucursal.NormaReparto;
                                                break;
                                            }
                                        case 5:
                                            {
                                                documentoSAP.Lines.CostingCode5 = Sucursal.NormaReparto;
                                                break;
                                            }

                                    }
                                    switch (Producto.Dimension)
                                    {
                                        case 1:
                                            {
                                                documentoSAP.Lines.CostingCode = Producto.NormaReparto;

                                                break;
                                            }
                                        case 2:
                                            {
                                                documentoSAP.Lines.CostingCode2 = Producto.NormaReparto;
                                                break;
                                            }
                                        case 3:
                                            {
                                                documentoSAP.Lines.CostingCode3 = Producto.NormaReparto;
                                                break;
                                            }
                                        case 4:
                                            {
                                                documentoSAP.Lines.CostingCode4 = Producto.NormaReparto;
                                                break;
                                            }
                                        case 5:
                                            {
                                                documentoSAP.Lines.CostingCode5 = Producto.NormaReparto;
                                                break;
                                            }

                                    }
                                }

                                //if(item.NumSerie != "0" || item.NumSerie != null)
                                //{
                                //    documentoSAP.Lines.SerialNumbers.ManufacturerSerialNumber = item.NumSerie;
                                //    documentoSAP.Lines.SerialNumbers.ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                                //    documentoSAP.Lines.SerialNumbers.Quantity = Convert.ToDouble(item.Cantidad);
                                //    documentoSAP.Lines.SerialNumbers.Add();
                                //    //documentoSAP.Lines.SerialNum = item.NumSerie;
                                //}
                                var ItemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() == null ? "0" : db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo;
                                var Lotes2 = Lotes1.Where(a => a.ItemCode == ItemCode && a.idDetalle == item.id).ToList();

                                var x = 0;
                                foreach (var lot in Lotes2)
                                {


                                    documentoSAP.Lines.SerialNumbers.ManufacturerSerialNumber = lot.Serie;
                                    documentoSAP.Lines.SerialNumbers.ItemCode = lot.ItemCode;
                                    documentoSAP.Lines.SerialNumbers.Quantity = Convert.ToDouble(lot.Cantidad);

                                    documentoSAP.Lines.SerialNumbers.Add();


                                    x++;
                                }

                                documentoSAP.Lines.Add();
                                z++;
                            }


                            if (Documento.Redondeo != 0)
                            {
                                documentoSAP.Rounding = BoYesNoEnum.tYES;
                                documentoSAP.RoundingDiffAmount = Convert.ToDouble(Documento.Redondeo);
                            }





                            var respuesta = documentoSAP.Add();
                            if (respuesta == 0) //se creo exitorsamente 
                            {
                                db.Entry(Documento).State = EntityState.Modified;
                                Documento.DocEntry = Conexion.Company.GetNewObjectKey().ToString();
                                Documento.ProcesadaSAP = true;
                                db.SaveChanges();


                                //Procesamos el pago
                                var CondicionPago = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault() == null ? db.CondicionesPagos.FirstOrDefault() : db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault();
                                //Procesamos el pago
                                if (CondicionPago.Dias == 0)
                                {
                                    if (param.MontosPagosSeparados == true)
                                    {
                                        try
                                        {


                                            var Fecha = Documento.Fecha.Date;
                                            var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                                            var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Monto > 0).FirstOrDefault() == null ? new List<MetodosPagos>() : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Monto > 0).ToList();

                                            var MetodosPagosColones = MetodosPagos.Where(a => a.Moneda == "CRC").ToList();
                                            var MetodosPagosDolares = MetodosPagos.Where(a => a.Moneda == "USD").ToList();

                                            bool pagoColonesProcesado = false;
                                            bool pagoDolaresProcesado = false;


                                            var contador = 0;




                                            if (MetodosPagosColones.Count() > 0)
                                            {
                                                try
                                                {


                                                    var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                                    pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                                    pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                                                    pagoProcesado.DocDate = Documento.Fecha;
                                                    pagoProcesado.DueDate = DateTime.Now;
                                                    pagoProcesado.TaxDate = DateTime.Now;
                                                    pagoProcesado.VatDate = DateTime.Now;
                                                    pagoProcesado.Remarks = "Pago procesado por NOVAPOS";
                                                    pagoProcesado.CounterReference = "APP FAC" + Documento.id;
                                                    pagoProcesado.DocCurrency = param.MonedaLocal;
                                                    pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                                    pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                                    pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);

                                                    if (Documento.Moneda != "CRC")
                                                    {
                                                        var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto) / TipoCambio.TipoCambio;
                                                        pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones);
                                                    }
                                                    else
                                                    {
                                                        var SumatoriaPagoColones = MetodosPagosColones.Sum(a => a.Monto);

                                                        pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

                                                    }
                                                    pagoProcesado.Series = Sucursal.SeriePago;//154; 161;


                                                    var SumatoriaEfectivo = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                                    var PagosTarjetas = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList(); //.Sum(a => a.Monto);
                                                    var SumatoriaTransferencia = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                                    if (SumatoriaEfectivo > 0)
                                                    {
                                                        var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                        var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                        pagoProcesado.CashAccount = Cuenta;
                                                        pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                                    }


                                                    foreach (var item in PagosTarjetas)
                                                    {

                                                        if (item.Monto > 0)
                                                        {
                                                            var idcuenta = item.idCuentaBancaria;
                                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
                                                            if (contador > 0)
                                                            {
                                                                pagoProcesado.CreditCards.Add();
                                                            }
                                                            else
                                                            {
                                                                pagoProcesado.CreditCards.SetCurrentLine(contador);

                                                            }
                                                            pagoProcesado.CreditCards.CardValidUntil = new DateTime(Documento.Fecha.Year, Documento.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                            pagoProcesado.CreditCards.CreditCard = 1;
                                                            pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                            pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                            pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                            pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                            pagoProcesado.CreditCards.CreditAcct = Cuenta;
                                                            pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);
                                                            contador++;

                                                        }
                                                    }


                                                    if (SumatoriaTransferencia > 0)
                                                    {
                                                        var idcuenta = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                        var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;

                                                        pagoProcesado.TransferAccount = Cuenta;
                                                        pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                        pagoProcesado.TransferReference = MetodosPagosColones.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                                        pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                                    }

                                                    var respuestaPago = pagoProcesado.Add();
                                                    if (respuestaPago == 0)
                                                    {
                                                        pagoColonesProcesado = true;

                                                    }
                                                    else
                                                    {
                                                        var error = "Hubo un error en el pago de la factura #" + Documento.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                        BitacoraErrores be = new BitacoraErrores();
                                                        be.Descripcion = error;
                                                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                                        be.Fecha = DateTime.Now;
                                                        be.JSON = JsonConvert.SerializeObject(documentoSAP);
                                                        db.BitacoraErrores.Add(be);
                                                        db.SaveChanges();
                                                        Conexion.Desconectar();

                                                    }
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
                                                    Conexion.Desconectar();
                                                }
                                            }
                                            else
                                            {
                                                pagoColonesProcesado = true;

                                            }


                                            if (MetodosPagosDolares.Count() > 0)
                                            {
                                                try
                                                {


                                                    var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                                    pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                                    pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                                                    pagoProcesado.DocDate = Documento.Fecha;
                                                    pagoProcesado.DueDate = DateTime.Now;
                                                    pagoProcesado.TaxDate = DateTime.Now;
                                                    pagoProcesado.VatDate = DateTime.Now;
                                                    pagoProcesado.Remarks = "Pago procesado por NOVAPOS";
                                                    pagoProcesado.CounterReference = "APP FAC" + Documento.id;
                                                    pagoProcesado.DocCurrency = "USD";
                                                    pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                                    pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                                    pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);


                                                    var SumatoriaPagod = MetodosPagosDolares.Sum(a => a.Monto);
                                                    pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagod);
                                                    pagoProcesado.Series = Sucursal.SeriePago;//154; 161;


                                                    var SumatoriaEfectivo = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Efectivo".ToUpper()).Sum(a => a.Monto);
                                                    var PagosTarjetas = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Tarjeta".ToUpper()).ToList();//.Sum(a => a.Monto);
                                                    var SumatoriaTransferencia = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "Transferencia".ToUpper()).Sum(a => a.Monto);

                                                    if (SumatoriaEfectivo > 0)
                                                    {
                                                        var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "efectivo".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                        var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                        pagoProcesado.CashAccount = Cuenta;
                                                        pagoProcesado.CashSum = Convert.ToDouble(SumatoriaEfectivo);

                                                    }


                                                    foreach (var item in PagosTarjetas)
                                                    {

                                                        if (item.Monto > 0)
                                                        {
                                                            var idcuenta = item.idCuentaBancaria;
                                                            var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;
                                                            if (contador > 0)
                                                            {
                                                                pagoProcesado.CreditCards.Add();
                                                            }
                                                            else
                                                            {
                                                                pagoProcesado.CreditCards.SetCurrentLine(contador);

                                                            }
                                                            pagoProcesado.CreditCards.CardValidUntil = new DateTime(Documento.Fecha.Year, Documento.Fecha.Month, 28); //Fecha en la que se mete el pago 
                                                            pagoProcesado.CreditCards.CreditCard = 1;
                                                            pagoProcesado.CreditCards.CreditType = BoRcptCredTypes.cr_Regular;
                                                            pagoProcesado.CreditCards.PaymentMethodCode = 1; //Quemado
                                                            pagoProcesado.CreditCards.CreditCardNumber = item.BIN; // Ultimos 4 digitos
                                                            pagoProcesado.CreditCards.VoucherNum = item.NumReferencia;// 
                                                            pagoProcesado.CreditCards.CreditAcct = Cuenta;
                                                            pagoProcesado.CreditCards.CreditSum = Convert.ToDouble(item.Monto);

                                                            contador++;
                                                        }
                                                    }



                                                    if (SumatoriaTransferencia > 0)
                                                    {
                                                        var idcuenta = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().idCuentaBancaria;
                                                        var Cuenta = db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.id == idcuenta).FirstOrDefault().CuentaSAP;


                                                        pagoProcesado.TransferAccount = Cuenta;
                                                        pagoProcesado.TransferDate = DateTime.Now; //Fecha en la que se mete el pago 
                                                        pagoProcesado.TransferReference = MetodosPagosDolares.Where(a => a.Metodo.ToUpper() == "transferencia".ToUpper()).FirstOrDefault().NumReferencia;
                                                        pagoProcesado.TransferSum = Convert.ToDouble(SumatoriaTransferencia);
                                                    }
                                                    var respuestaPago = pagoProcesado.Add();
                                                    if (respuestaPago == 0)
                                                    {
                                                        pagoDolaresProcesado = true;

                                                    }
                                                    else
                                                    {
                                                        var error = "Hubo un error en el pago de la factura # " + Documento.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                        BitacoraErrores be = new BitacoraErrores();
                                                        be.Descripcion = error;
                                                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                                        be.Fecha = DateTime.Now;
                                                        be.JSON = JsonConvert.SerializeObject(documentoSAP);
                                                        db.BitacoraErrores.Add(be);
                                                        db.SaveChanges();
                                                        Conexion.Desconectar();

                                                    }
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
                                                    Conexion.Desconectar();
                                                }
                                            }
                                            else
                                            {
                                                pagoDolaresProcesado = true;

                                            }

                                            if (pagoColonesProcesado && pagoDolaresProcesado)
                                            {
                                                db.Entry(Documento).State = EntityState.Modified;
                                                Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                                Documento.PagoProcesadaSAP = true;
                                                db.SaveChanges();
                                            }


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
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {


                                            //meter los metodos de pago
                                            var Fecha = Documento.Fecha.Date;
                                            var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                                            var MetodosPagos = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id).ToList();


                                            var SumatoriaPagoColones = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Moneda == "CRC" && a.Monto > 0).FirstOrDefault() == null ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Moneda == "CRC" && a.Monto > 0).Sum(a => a.Monto);
                                            var SumatoriaPagoDolares = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Moneda != "CRC" && a.Monto > 0).FirstOrDefault() == null ? 0 : db.MetodosPagos.Where(a => a.idEncabezado == Documento.id && a.Moneda != "CRC" && a.Monto > 0).Sum(a => a.Monto);
                                            bool pagoColonesProcesado = false;
                                            bool pagoDolaresProcesado = false;



                                            if (SumatoriaPagoColones > 0)
                                            {
                                                try
                                                {
                                                    var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                                    pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                                    pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                                                    pagoProcesado.DocDate = Documento.Fecha;
                                                    pagoProcesado.DueDate = DateTime.Now;
                                                    pagoProcesado.TaxDate = DateTime.Now;
                                                    pagoProcesado.VatDate = DateTime.Now;
                                                    pagoProcesado.Remarks = "Pago procesado por NOVAPOS";
                                                    pagoProcesado.CounterReference = "APP FAC" + Documento.id;
                                                    pagoProcesado.DocCurrency = param.MonedaLocal;
                                                    pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                                    //ligar la factura con el pago 

                                                    pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                                    pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);
                                                    if (Documento.Moneda != "CRC")
                                                    {
                                                        var SumatoriaPagoColones2 = SumatoriaPagoColones / TipoCambio.TipoCambio;
                                                        pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoColones2);
                                                    }
                                                    else
                                                    {
                                                        pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoColones);

                                                    }


                                                    var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "CRC").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "CRC").FirstOrDefault().CuentaSAP;




                                                    pagoProcesado.CashSum = Convert.ToDouble(SumatoriaPagoColones);
                                                    pagoProcesado.Series = Sucursal.SeriePago;//154; 161;
                                                    pagoProcesado.CashAccount = Cuenta;

                                                    var respuestaPago = pagoProcesado.Add();
                                                    if (respuestaPago == 0)
                                                    {
                                                        pagoColonesProcesado = true;
                                                        //db.Entry(Documento).State = EntityState.Modified;
                                                        //Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                                        //Documento.PagoProcesadaSAP = true;
                                                        //db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        var error = "Hubo un error en el pago de la factura #" + Documento.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                        BitacoraErrores be = new BitacoraErrores();
                                                        be.Descripcion = error;
                                                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                                        be.Fecha = DateTime.Now;
                                                        be.JSON = JsonConvert.SerializeObject(documentoSAP);
                                                        db.BitacoraErrores.Add(be);
                                                        db.SaveChanges();
                                                        Conexion.Desconectar();

                                                    }
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
                                                    Conexion.Desconectar();

                                                }

                                            }
                                            else
                                            {
                                                pagoColonesProcesado = true;

                                            }

                                            if (SumatoriaPagoDolares > 0)
                                            {
                                                try
                                                {
                                                    var pagoProcesado = (SAPbobsCOM.Payments)Conexion.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                                                    pagoProcesado.DocType = BoRcptTypes.rCustomer;
                                                    // pagoProcesado.PayToBankAccountNo = "N";
                                                    pagoProcesado.CardCode = db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault() == null ? "0" : db.Clientes.Where(a => a.id == Documento.idCliente).FirstOrDefault().Codigo;
                                                    pagoProcesado.DocDate = Documento.Fecha;
                                                    pagoProcesado.DueDate = DateTime.Now;
                                                    pagoProcesado.TaxDate = DateTime.Now;
                                                    pagoProcesado.VatDate = DateTime.Now;
                                                    pagoProcesado.Remarks = "Pago procesado por NOVAPOS";
                                                    pagoProcesado.CounterReference = "APP FAC" + Documento.id;
                                                    pagoProcesado.DocCurrency = "USD";
                                                    pagoProcesado.HandWritten = BoYesNoEnum.tNO;
                                                    pagoProcesado.LocalCurrency = BoYesNoEnum.tNO;
                                                    //ligar la factura con el pago 

                                                    pagoProcesado.Invoices.InvoiceType = BoRcptInvTypes.it_Invoice;
                                                    pagoProcesado.Invoices.DocEntry = Convert.ToInt32(Documento.DocEntry);
                                                    //  pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoDolares);
                                                    //if (Documento.Moneda != "CRC")
                                                    //{
                                                    //    SumatoriaPagoColones = SumatoriaPagoColones / TipoCambio.TipoCambio;
                                                    //}
                                                    pagoProcesado.Invoices.AppliedFC = Convert.ToDouble(SumatoriaPagoDolares);


                                                    pagoProcesado.Invoices.SumApplied = Convert.ToDouble(SumatoriaPagoDolares * TipoCambio.TipoCambio);
                                                    pagoProcesado.Invoices.Add();


                                                    //var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault().CuentaSAP;

                                                    var Cuenta = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? "0" : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault().CuentaSAP;
                                                    pagoProcesado.Series = Sucursal.SeriePago;//154; 161;
                                                    pagoProcesado.CashSum = Convert.ToDouble(SumatoriaPagoDolares);
                                                    pagoProcesado.CashAccount = Cuenta;




                                                    var respuestaPago = pagoProcesado.Add();
                                                    if (respuestaPago == 0)
                                                    {
                                                        pagoDolaresProcesado = true;
                                                        //db.Entry(Documento).State = EntityState.Modified;
                                                        //Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                                        //Documento.PagoProcesadaSAP = true;
                                                        //db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        var error = "Hubo un error en el pago de la factura # " + Documento.id + " -> " + Conexion.Company.GetLastErrorDescription();
                                                        BitacoraErrores be = new BitacoraErrores();
                                                        be.Descripcion = error;
                                                        be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                                        be.Fecha = DateTime.Now;
                                                        be.JSON = JsonConvert.SerializeObject(documentoSAP);
                                                        db.BitacoraErrores.Add(be);
                                                        db.SaveChanges();
                                                        Conexion.Desconectar();

                                                    }
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
                                                    Conexion.Desconectar();

                                                }

                                            }
                                            else
                                            {
                                                pagoDolaresProcesado = true;

                                            }

                                            if (pagoColonesProcesado && pagoDolaresProcesado)
                                            {
                                                db.Entry(Documento).State = EntityState.Modified;
                                                Documento.DocEntryPago = Conexion.Company.GetNewObjectKey().ToString();
                                                Documento.PagoProcesadaSAP = true;
                                                db.SaveChanges();
                                            }




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
                                        }
                                    }

                                }



                                Conexion.Desconectar();

                            }
                            else
                            {
                                var error = "hubo un error " + Conexion.Company.GetLastErrorDescription();
                                BitacoraErrores be = new BitacoraErrores();
                                be.Descripcion = error;
                                be.StrackTrace = Conexion.Company.GetLastErrorCode().ToString();
                                be.Fecha = DateTime.Now;
                                be.JSON = JsonConvert.SerializeObject(documentoSAP);
                                db.BitacoraErrores.Add(be);
                                db.SaveChanges();
                                Conexion.Desconectar();


                            }
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
                            Conexion.Desconectar();


                        }
                    }


                }





                return Request.CreateResponse(System.Net.HttpStatusCode.OK);

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
                Conexion.Desconectar();

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }


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
                var Documentos = db.EncDocumento.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.idUsuarioCreador,
                    a.idCaja,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.Comentarios,
                    a.Subtotal,
                    a.TotalImpuestos,
                    a.TotalDescuento,
                    a.TotalCompra,
                    a.PorDescto,
                    a.Status,
                    a.CodSuc,
                    a.Moneda,
                    a.TipoDocumento,
                    a.BaseEntry,
                    a.DocEntry,

                    a.idCondPago,
                    a.idVendedor,
                    a.ProcesadaSAP,
                    a.PagoProcesadaSAP,
                    a.ClaveHacienda,
                    a.ConsecutivoHacienda,
                    a.Redondeo,
                    MetodosPagos = db.MetodosPagos.Where(b => b.idEncabezado == a.id).ToList(),
                    Detalle = db.DetDocumento.Where(b => b.idEncabezado == a.id).ToList(),
                    Lotes = db.Lotes.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true)
                && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)
                && (filtro.BaseEntry > 0 ? a.BaseEntry == filtro.BaseEntry : true)
                && (!string.IsNullOrEmpty(filtro.CardCode) ? a.TipoDocumento == filtro.CardCode : true)
                && (filtro.Codigo1 > 0 ? a.idCliente == filtro.Codigo1 : true)
                && (filtro.Codigo2 > 0 ? a.idUsuarioCreador == filtro.Codigo2 : true)
                && (!string.IsNullOrEmpty(filtro.ItemCode) ? a.Status == filtro.ItemCode : true)
                && (filtro.Codigo3 > 0 ? a.idCaja == filtro.Codigo3 : true)
                && (filtro.Codigo4 > 0 ? a.idCondPago == filtro.Codigo4 : true)
                && (filtro.Codigo5 > 0 ? a.idVendedor == filtro.Codigo5 : true)
                && (filtro.Codigo6 > 0 ? a.idCaja == filtro.Codigo6 : true)
                && (filtro.Procesado != null && filtro.Activo ? a.ProcesadaSAP == filtro.Procesado : true)
                && (!string.IsNullOrEmpty(filtro.Texto) ? a.CodSuc == filtro.Texto : true)
                //&& (filtro.PagoProcesado != null  ? a.PagoProcesadaSAP == filtro.PagoProcesado : true)
                ).ToList(); //Traemos el listado de productos


                //if (filtro.PagoProcesado != null)
                //{
                //    Documentos = Documentos.Where(a => a.PagoProcesadaSAP == filtro.PagoProcesado).ToList();
                //}

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Documentos);
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

        [Route("api/Documentos/Consultar")]
        public HttpResponseMessage GetOne([FromUri] int id)
        {
            try
            {
                var Documento = db.EncDocumento.Select(a => new
                {
                    a.id,
                    a.idCliente,
                    a.idUsuarioCreador,
                    a.idCaja,
                    a.Fecha,
                    a.FechaVencimiento,
                    a.Comentarios,
                    a.Subtotal,
                    a.TotalImpuestos,
                    a.TotalDescuento,
                    a.TotalCompra,
                    a.PorDescto,
                    a.Status,
                    a.CodSuc,
                    a.Moneda,
                    a.TipoDocumento,
                    a.BaseEntry,
                    a.DocEntry,
                    a.ProcesadaSAP,
                    a.idCondPago,
                    a.idVendedor,
                    a.ClaveHacienda,
                    a.ConsecutivoHacienda,
                    a.Redondeo,
                    MetodosPagos = db.MetodosPagos.Where(b => b.idEncabezado == a.id).ToList(),
                    Detalle = db.DetDocumento.Where(b => b.idEncabezado == a.id).ToList(),
                    Lotes = db.Lotes.Where(b => b.idEncabezado == a.id).ToList()

                }).Where(a => a.id == id).FirstOrDefault();


                return Request.CreateResponse(System.Net.HttpStatusCode.OK, Documento);
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

        [Route("api/Documentos/Insertar")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostAsync([FromBody] Documentos documento)
        {
            try
            {
                Parametros param = db.Parametros.FirstOrDefault();
                EncDocumento Documento = db.EncDocumento.Where(a => a.id == documento.id || (a.BaseEntry  == documento.BaseEntry && a.BaseEntry != 0 && a.TipoDocumento != "03" && documento.TipoDocumento != "03")).FirstOrDefault();

                if (Documento == null)
                {
                    if (documento.Detalle == null || documento.Detalle.Count == 0)
                    {
                        return Request.CreateResponse(System.Net.HttpStatusCode.BadRequest, "El detalle del documento está vacío.");
                    }
                    var t = db.Database.BeginTransaction();

                    try
                    {
                        Documento = new EncDocumento();
                        Documento.idCliente = documento.idCliente;
                        Documento.idUsuarioCreador = documento.idUsuarioCreador;
                        Documento.Fecha = DateTime.Now;
                        Documento.FechaVencimiento = documento.FechaVencimiento;
                        Documento.Comentarios = documento.Comentarios;
                        Documento.Subtotal = documento.Subtotal;
                        Documento.TotalImpuestos = documento.TotalImpuestos;
                        Documento.TotalDescuento = documento.TotalDescuento;
                        Documento.TotalCompra = documento.TotalCompra;
                        Documento.PorDescto = documento.PorDescto;
                        Documento.CodSuc = documento.CodSuc;
                        Documento.Moneda = documento.Moneda;
                        Documento.TipoDocumento = documento.TipoDocumento;
                        Documento.Status = "0";// 0 is open, 1 is closed
                        Documento.idCaja = documento.idCaja;
                        Documento.BaseEntry = documento.BaseEntry;
                        Documento.DocEntry = "";
                        Documento.ProcesadaSAP = false;
                        Documento.idCondPago = documento.idCondPago;
                        Documento.idVendedor = documento.idVendedor;
                        Documento.ClaveHacienda = "";
                        Documento.ConsecutivoHacienda = "";
                        Documento.Redondeo = documento.Redondeo;
                        db.EncDocumento.Add(Documento);
                        db.SaveChanges();



                        var i = 0;
                        foreach (var item in documento.Detalle)
                        {
                            var itemCode = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault() != null ? db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault().Codigo : "";
                            DetDocumento det = new DetDocumento();
                            det.idEncabezado = Documento.id;
                            det.idProducto = item.idProducto;
                            det.NumLinea = i;
                            det.PorDescto = item.PorDescto;
                            det.PrecioUnitario = item.PrecioUnitario;
                            det.TotalImpuesto = item.TotalImpuesto;
                            det.Cantidad = item.Cantidad;
                            det.Descuento = item.Descuento;
                            det.TotalLinea = item.TotalLinea; //((det.PrecioUnitario * det.Cantidad) - det.Descuento) + det.TotalImpuesto;
                            det.Cabys = item.Cabys;
                            det.idExoneracion = item.idExoneracion;
                            db.DetDocumento.Add(det);
                            db.SaveChanges();
                            i++;






                            if (documento.Lotes == null)
                            {
                                documento.Lotes = new List<Lotes>();
                            }


                            foreach (var lote in documento.Lotes.Where(a => a.ItemCode == itemCode))
                            {
                                Lotes Lote = new Lotes();
                                Lote.idEncabezado = Documento.id;
                                Lote.Tipo = "F";
                                Lote.Serie = lote.Serie;
                                Lote.ItemCode = lote.ItemCode;
                                Lote.Cantidad = lote.Cantidad;
                                Lote.Manufactura = lote.Manufactura;
                                Lote.idDetalle = det.id;
                                db.Lotes.Add(Lote);
                                db.SaveChanges();
                            }

                            var prod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                            if (prod != null)
                            {

                                db.Entry(prod).State = EntityState.Modified;
                                if (Documento.TipoDocumento == "01" || Documento.TipoDocumento == "04")
                                {
                                    prod.Stock -= item.Cantidad;

                                }
                                else
                                {
                                    prod.Stock += item.Cantidad;
                                }
                                db.SaveChanges();
                            }

                        }

                        if (Documento.TipoDocumento == "03")
                        {

                            var DocumentoG = db.EncDocumento.Where(a => a.id == documento.BaseEntry).FirstOrDefault();

                            db.Entry(Documento).State = EntityState.Modified;
                            Documento.idVendedor = DocumentoG.idVendedor;
                            db.SaveChanges();

                            // Si es NC debe rebajar de los cierres el monto
                            var time2 = DocumentoG.Fecha.Date;
                            var MontoDevuelto = db.EncDocumento.Where(a => a.BaseEntry == documento.BaseEntry && a.TipoDocumento == "03").Sum(a => a.TotalCompra) - documento.TotalCompra; //Este es el monto que ya se ha devuelto de dineros
                            var MontosxMetodo = db.MetodosPagos.Where(a => a.idEncabezado == documento.BaseEntry && a.Monto > 0).GroupBy(a => a.Metodo).ToList(); // Cantidad de Dinero pagados por metodos de pago
                            var CierreCajaM = db.CierreCajas.Where(a => a.FechaCaja == time2 && a.idCaja == DocumentoG.idCaja && a.idUsuario == DocumentoG.idUsuarioCreador).FirstOrDefault();


                            decimal banderaDevuelto = 0;
                            decimal montoadevolver = documento.TotalCompra;
                            foreach (var item in MontosxMetodo)
                            {
                                if (CierreCajaM != null)
                                {
                                    if (banderaDevuelto < documento.TotalCompra) // Si ya le llegue al total que tengo que devolver
                                    {
                                        var Fecha = DocumentoG.Fecha.Date;
                                        var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                                        var PagadoMismaMoneda = item.Where(a => a.Moneda == documento.Moneda).Sum(a => a.Monto);
                                        var PagadoOtraMoneda = item.Where(a => a.Moneda != documento.Moneda).Sum(a => a.Monto);
                                        var PagadoMismaMonedaConvertido = documento.Moneda != "CRC" ? PagadoOtraMoneda / TipoCambio.TipoCambio : PagadoOtraMoneda * TipoCambio.TipoCambio;


                                        if ((Math.Round(PagadoMismaMoneda + PagadoMismaMonedaConvertido)) >= montoadevolver) // si lo que pagaron con un metodo de la misma moneda es mayor o igual a lo que estoy rebajando
                                        {
                                            var montoadevolver2 = montoadevolver;

                                            foreach (var item2 in item)
                                            {
                                                db.Entry(CierreCajaM).State = EntityState.Modified;
                                                switch (item2.Metodo)
                                                {
                                                    case "Efectivo":
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));
                                                                CierreCajaM.EfectivoColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto; //Si la moneda del documento es dolares y estoy metiendo colones, lo convierto a colones el monto a devolver
                                                                CierreCajaM.NotasCreditoColones -= devuelto;
                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;

                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.EfectivoFC -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC += devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }

                                                            break;
                                                        }
                                                    case "Tarjeta":
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.TarjetasColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto;
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = item.FirstOrDefault().BIN;
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = item2.NumReferencia;
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;



                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.TarjetasFC -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = item.FirstOrDefault().BIN;
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = item2.NumReferencia;
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }


                                                            break;
                                                        }
                                                    case "Transferencia":
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.TransferenciasColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto;
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto - (Math.Round(banderaDevuelto * TipoCambio.TipoCambio))) : (item2.Monto - (banderaDevuelto))));
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;


                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.TransferenciasDolares -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto - (Math.Round(banderaDevuelto / TipoCambio.TipoCambio))) : (item2.Monto - (banderaDevuelto))));
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }


                                                            break;
                                                        }
                                                    case "Cheque":
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.ChequesColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto;
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = item2.NumCheque;
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;


                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.ChequesFC -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto - (Math.Round(banderaDevuelto / TipoCambio.TipoCambio))) : (item2.Monto - (banderaDevuelto))));
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = item2.NumCheque;
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }


                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.OtrosMediosColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto;
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;


                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.OtrosMediosFC -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }


                                                            break;
                                                        }
                                                }

                                                db.SaveChanges();
                                            }
                                            banderaDevuelto += montoadevolver;


                                        }
                                        else
                                        {


                                            var montoadevolver2 = montoadevolver;

                                            foreach (var item2 in item)
                                            {
                                                db.Entry(CierreCajaM).State = EntityState.Modified;
                                                switch (item2.Metodo)
                                                {
                                                    case "Efectivo":
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));
                                                                CierreCajaM.EfectivoColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto; //Si la moneda del documento es dolares y estoy metiendo colones, lo convierto a colones el monto a devolver
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;

                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.EfectivoFC -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }

                                                            break;
                                                        }
                                                    case "Tarjeta":
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.TarjetasColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto;
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = item.FirstOrDefault().BIN;
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = item2.NumReferencia;
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;



                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.TarjetasFC -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = item.FirstOrDefault().BIN;
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = item2.NumReferencia;
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }


                                                            break;
                                                        }
                                                    case "Transferencia":
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.TransferenciasColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto;
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto - (Math.Round(banderaDevuelto * TipoCambio.TipoCambio))) : (item2.Monto - (banderaDevuelto))));
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;


                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.TransferenciasDolares -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto - (Math.Round(banderaDevuelto / TipoCambio.TipoCambio))) : (item2.Monto - (banderaDevuelto))));
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }


                                                            break;
                                                        }
                                                    case "Cheque":
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.ChequesColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto;
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = item2.NumCheque;
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;


                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.ChequesFC -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto - (Math.Round(banderaDevuelto / TipoCambio.TipoCambio))) : (item2.Monto - (banderaDevuelto))));
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = item2.NumCheque;
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }


                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            if (item2.Moneda == "CRC")
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 * TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.OtrosMediosColones -= devuelto;
                                                                CierreCajaM.TotalVendidoColones -= devuelto;
                                                                CierreCajaM.NotasCreditoColones -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto / TipoCambio.TipoCambio) : devuelto;


                                                            }
                                                            else
                                                            {
                                                                var montoadevolverC = Math.Round(montoadevolver2 / TipoCambio.TipoCambio);
                                                                var devuelto = Math.Round((documento.Moneda != item2.Moneda ? (item2.Monto > montoadevolverC ? (montoadevolverC) : item2.Monto) : (item2.Monto > montoadevolver2 ? montoadevolver2 : item2.Monto)));

                                                                CierreCajaM.OtrosMediosFC -= devuelto;
                                                                CierreCajaM.TotalVendidoFC -= devuelto;
                                                                CierreCajaM.NotasCreditoFC -= devuelto;

                                                                MetodosPagos MetodosPagos = new MetodosPagos();
                                                                MetodosPagos.idEncabezado = DocumentoG.id;
                                                                MetodosPagos.Monto = -devuelto;
                                                                MetodosPagos.BIN = "";
                                                                MetodosPagos.NumCheque = "";
                                                                MetodosPagos.NumReferencia = "";
                                                                MetodosPagos.Metodo = item2.Metodo;
                                                                MetodosPagos.idCuentaBancaria = item2.idCuentaBancaria;
                                                                MetodosPagos.Moneda = item2.Moneda;
                                                                MetodosPagos.idCaja = Documento.idCaja;
                                                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                                                MetodosPagos.Fecha = DateTime.Now.Date;
                                                                db.MetodosPagos.Add(MetodosPagos);
                                                                db.SaveChanges();
                                                                montoadevolver2 -= documento.Moneda != item2.Moneda ? Math.Round(devuelto * TipoCambio.TipoCambio) : devuelto;


                                                            }


                                                            break;
                                                        }
                                                }
                                                db.SaveChanges();
                                            }
                                            montoadevolver -= Math.Round(PagadoMismaMoneda + PagadoMismaMonedaConvertido); //item.Sum(a => a.Monto);
                                            banderaDevuelto += Math.Round(PagadoMismaMoneda + PagadoMismaMonedaConvertido);//item.Sum(a => a.Monto);


                                        }
                                    }
                                }
                            }

                            if ((MontoDevuelto + documento.TotalCompra) >= DocumentoG.TotalCompra)
                            {
                                db.Entry(DocumentoG).State = EntityState.Modified;
                                DocumentoG.Status = "1";
                                db.SaveChanges();
                            }

                            //

                        }
                        var time = DateTime.Now.Date;
                        var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == time && a.idCaja == documento.idCaja && a.idUsuario == Documento.idUsuarioCreador).FirstOrDefault();
                        var Asiento = db.Asientos.Where(a => a.Fecha == time && a.idCaja == documento.idCaja && a.idUsuario == Documento.idUsuarioCreador && a.CodSuc == Documento.CodSuc && a.ProcesadoSAP == false).FirstOrDefault();
                        if (documento.MetodosPagos != null && documento.MetodosPagos.Count() > 0)
                        {

                            foreach (var item in documento.MetodosPagos.Where(a => a.Metodo != "Pago a Cuenta"))
                            {
                                var Usuario = db.Usuarios.Where(a => a.id == Documento.idUsuarioCreador).FirstOrDefault() == null ? "0" : db.Usuarios.Where(a => a.id == Documento.idUsuarioCreador).FirstOrDefault().Nombre;
                                BitacoraMovimientos bm = new BitacoraMovimientos();
                                bm.Descripcion = "El usuario " + Usuario + " ha pagado a cuenta la factura #" + Documento.id + " con el saldo " + item.Monto + ", favor conciliar en SAP";
                                bm.idUsuario = Documento.idUsuarioCreador;
                                bm.Fecha = DateTime.Now;
                                bm.Metodo = "Insercion de Pago a Cuenta de Documento";
                                db.BitacoraMovimientos.Add(bm);
                                db.SaveChanges();

                            }

                            foreach (var item in documento.MetodosPagos.Where(a => a.Metodo != "Pago a Cuenta"))
                            {
                                MetodosPagos MetodosPagos = new MetodosPagos();
                                MetodosPagos.idEncabezado = Documento.id;
                                MetodosPagos.Monto = item.Monto;
                                MetodosPagos.BIN = item.BIN;
                                MetodosPagos.NumCheque = item.NumCheque;
                                MetodosPagos.NumReferencia = item.NumReferencia;
                                MetodosPagos.Metodo = item.Metodo;
                                MetodosPagos.idCuentaBancaria = item.idCuentaBancaria;
                                MetodosPagos.Moneda = item.Moneda;
                                MetodosPagos.idCaja = Documento.idCaja;
                                MetodosPagos.idCajero = Documento.idUsuarioCreador;
                                MetodosPagos.Fecha = DateTime.Now.Date;
                                MetodosPagos.MonedaVuelto = item.MonedaVuelto;
                                MetodosPagos.PagadoCon = item.PagadoCon;
                                db.MetodosPagos.Add(MetodosPagos);
                                db.SaveChanges();
                                if (CierreCaja != null)
                                {
                                    db.Entry(CierreCaja).State = EntityState.Modified;
                                    if (MetodosPagos.Moneda == "CRC")
                                    {
                                        switch (item.Metodo)
                                        {
                                            case "Efectivo":
                                                {
                                                    if (MetodosPagos.Moneda != MetodosPagos.MonedaVuelto)
                                                    {
                                                        var Fecha = DateTime.Now.Date;
                                                        var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                                                        var MontoDevuelto = (MetodosPagos.PagadoCon - MetodosPagos.Monto) / TipoCambio.TipoCambio;
                                                        CierreCaja.EfectivoFC -= MontoDevuelto;
                                                        CierreCaja.TotalVendidoFC -= MontoDevuelto;

                                                        CierreCaja.EfectivoColones += MetodosPagos.PagadoCon;
                                                        CierreCaja.TotalVendidoColones += MetodosPagos.PagadoCon;
                                                    }
                                                    else
                                                    {
                                                        CierreCaja.EfectivoColones += item.Monto;
                                                        CierreCaja.TotalVendidoColones += item.Monto;
                                                    }


                                                    break;
                                                }
                                            case "Tarjeta":
                                                {
                                                    CierreCaja.TarjetasColones += item.Monto;
                                                    CierreCaja.TotalVendidoColones += item.Monto;

                                                    break;
                                                }
                                            case "Transferencia":
                                                {
                                                    CierreCaja.TransferenciasColones += item.Monto;
                                                    CierreCaja.TotalVendidoColones += item.Monto;

                                                    break;
                                                }
                                            case "Cheque":
                                                {
                                                    CierreCaja.ChequesColones += item.Monto;
                                                    CierreCaja.TotalVendidoColones += item.Monto;

                                                    break;
                                                }

                                            default:
                                                {
                                                    CierreCaja.OtrosMediosColones += item.Monto;
                                                    CierreCaja.TotalVendidoColones += item.Monto;

                                                    break;
                                                }

                                        }

                                    }
                                    else
                                    {
                                        switch (item.Metodo)
                                        {
                                            case "Efectivo":
                                                {
                                                    if (MetodosPagos.Moneda != MetodosPagos.MonedaVuelto)
                                                    {
                                                        var Fecha = DateTime.Now.Date;
                                                        var TipoCambio = db.TipoCambios.Where(a => a.Moneda == "USD" && a.Fecha == Fecha).FirstOrDefault();
                                                        var MontoDevuelto = (MetodosPagos.PagadoCon - MetodosPagos.Monto) * TipoCambio.TipoCambio;
                                                        CierreCaja.EfectivoColones -= MontoDevuelto;
                                                        CierreCaja.TotalVendidoColones -= MontoDevuelto;

                                                        CierreCaja.EfectivoFC += MetodosPagos.PagadoCon;
                                                        CierreCaja.TotalVendidoFC += MetodosPagos.PagadoCon;

                                                        var Debito = MetodosPagos.PagadoCon - MetodosPagos.Monto;
                                                        var Credito = Debito * TipoCambio.TipoCambio;

                                                        if (Debito > 0)
                                                        {
                                                            if (Asiento == null)
                                                            {
                                                                Asientos Asientos = new Asientos();
                                                                Asientos.idUsuario = Documento.idUsuarioCreador;
                                                                Asientos.idCaja = documento.idCaja;
                                                                Asientos.Fecha = DateTime.Now.Date;
                                                                Asientos.CodSuc = Documento.CodSuc;
                                                                Asientos.idCuentaCredito = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "CRC").FirstOrDefault() == null ? 0 : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "CRC").FirstOrDefault().id;
                                                                Asientos.idCuentaDebito = db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault() == null ? 0 : db.CuentasBancarias.Where(a => a.Tipo.ToLower().Contains("efectivo") && a.CodSuc == Documento.CodSuc && a.Moneda == "USD").FirstOrDefault().id;
                                                                Asientos.Referencia = "Asiento compra de Dolares";
                                                                Asientos.ProcesadoSAP = false;
                                                                Asientos.Credito = Credito;
                                                                Asientos.Debito = Debito;
                                                                db.Asientos.Add(Asientos);
                                                                db.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                db.Entry(Asiento).State = EntityState.Modified;
                                                                Asiento.Credito += Credito;
                                                                Asiento.Debito += Debito;
                                                                db.SaveChanges();
                                                            }

                                                        }



                                                    }
                                                    else
                                                    {
                                                        CierreCaja.EfectivoFC += item.Monto;
                                                        CierreCaja.TotalVendidoFC += item.Monto;
                                                    }

                                                    break;
                                                }
                                            case "Tarjeta":
                                                {
                                                    CierreCaja.TarjetasFC += item.Monto;
                                                    CierreCaja.TotalVendidoFC += item.Monto;

                                                    break;
                                                }
                                            case "Transferencia":
                                                {
                                                    CierreCaja.TransferenciasDolares += item.Monto;
                                                    CierreCaja.TotalVendidoFC += item.Monto;

                                                    break;
                                                }
                                            case "Cheque":
                                                {
                                                    CierreCaja.ChequesFC += item.Monto;
                                                    CierreCaja.TotalVendidoFC += item.Monto;

                                                    break;
                                                }

                                            default:
                                                {
                                                    CierreCaja.OtrosMediosFC += item.Monto;
                                                    CierreCaja.TotalVendidoFC += item.Monto;

                                                    break;
                                                }

                                        }
                                    }
                                    db.SaveChanges();
                                }


                            }



                            foreach (var item in documento.MetodosPagos.Where(a => a.Metodo == "Pago a Cuenta"))
                            {
                                var Cliente = db.Clientes.Where(a => a.id == documento.idCliente).FirstOrDefault();
                                if (Cliente != null)
                                {
                                    db.Entry(Cliente).State = EntityState.Modified;
                                    Cliente.Saldo += item.Monto;
                                    db.SaveChanges();

                                }
                            }

                        }
                        else
                        {
                            if (Documento.TipoDocumento != "03")
                            {
                                var Condicion = db.CondicionesPagos.Where(a => a.id == Documento.idCondPago).FirstOrDefault();

                                if (Condicion != null)
                                {
                                    if (Condicion.Dias > 0)
                                    {
                                        var Cliente = db.Clientes.Where(a => a.id == documento.idCliente).FirstOrDefault();
                                        var Aprobados = db.AprobacionesCreditos.Where(a => a.idCliente == Documento.idCliente && a.Status == "A" && a.Activo == true && a.FechaCreacion == time).FirstOrDefault();
                                        if (Cliente != null)
                                        {
                                            db.Entry(Cliente).State = EntityState.Modified;
                                            Cliente.Saldo += Documento.TotalCompra;
                                            db.SaveChanges();

                                        }
                                        if (Aprobados != null)
                                        {
                                            db.Entry(Aprobados).State = EntityState.Modified;
                                            Aprobados.Total -= Documento.TotalCompra;

                                            if (Aprobados.Total <= 1) // puede que la resta de 0.05 y no lo cierre
                                            {
                                                Aprobados.Activo = false;
                                            }
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var DocumentoBase = db.EncDocumento.Where(a => a.id == Documento.BaseEntry).FirstOrDefault() == null ? throw new Exception("Nota de credito mal referenciada") : db.EncDocumento.Where(a => a.id == Documento.BaseEntry).FirstOrDefault();
                                var Condicion = db.CondicionesPagos.Where(a => a.id == DocumentoBase.idCondPago).FirstOrDefault();
                                if (Condicion != null)
                                {
                                    if (Condicion.Dias > 0)
                                    {
                                        var Cliente = db.Clientes.Where(a => a.id == documento.idCliente).FirstOrDefault();
                                        if (Cliente != null)
                                        {
                                            db.Entry(Cliente).State = EntityState.Modified;
                                            Cliente.Saldo += Documento.TotalCompra;
                                            db.SaveChanges();

                                        }
                                    }
                                }
                            }
                        }




                        documento.id = Documento.id;
                        BitacoraMovimientos btm = new BitacoraMovimientos();
                        btm.idUsuario = documento.idUsuarioCreador;
                        btm.Descripcion = "Se crea un documento para el cliente con el id: " + documento.idCliente;
                        btm.Fecha = DateTime.Now;
                        btm.Metodo = "Insercion de Documento";
                        db.BitacoraMovimientos.Add(btm);
                        db.SaveChanges();
                        t.Commit();

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            // Attempt to roll back the transaction.
                            t.Rollback();
                        }
                        catch (Exception exRollback)
                        {
                            // Throws an InvalidOperationException if the connection
                            // is closed or the transaction has already been rolled
                            // back on the server. 
                            BitacoraErrores be2 = new BitacoraErrores();
                            be2.Descripcion = "Error en el rollback: " + exRollback.Message;
                            be2.StrackTrace = exRollback.StackTrace;
                            be2.Fecha = DateTime.Now;
                            be2.JSON = JsonConvert.SerializeObject(exRollback);
                            db.BitacoraErrores.Add(be2);
                            db.SaveChanges();
                        }

                        BitacoraErrores be = new BitacoraErrores();
                        be.Descripcion = ex.Message;
                        be.StrackTrace = ex.StackTrace;
                        be.Fecha = DateTime.Now;
                        be.JSON = JsonConvert.SerializeObject(ex);
                        db.BitacoraErrores.Add(be);
                        db.SaveChanges();

                        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, be);

                    }



                    ////Mandar al api de facturacion
                    ///

                    var Parametros = db.Parametros.FirstOrDefault();
                    HttpClient cliente = new HttpClient();

                    try
                    {

                        var Url = Parametros.UrlFacturaElectronica.Replace("@DocNumR", Documento.id.ToString()).Replace("@ObjTypeR", (Documento.TipoDocumento != "03" ? "13" : "14")).Replace("@SucursalR", "099");

                        cliente.Timeout = TimeSpan.FromMinutes(30);
                        HttpResponseMessage response = await cliente.GetAsync(Url);
                        if (response.IsSuccessStatusCode)
                        {
                            response.Content.Headers.ContentType.MediaType = "application/json";
                            var res = await response.Content.ReadAsAsync<RecibidoFacturacion>();

                            db.Entry(Documento).State = EntityState.Modified;
                            Documento.ClaveHacienda = res.ClaveHacienda;
                            Documento.ConsecutivoHacienda = res.ConsecutivoHacienda;

                            documento.ClaveHacienda = res.ClaveHacienda;
                            documento.ConsecutivoHacienda = res.ConsecutivoHacienda;
                            db.SaveChanges();


                            try
                            {
                                HttpClient cliente2 = new HttpClient();

                                var Url2 = Parametros.UrlConsultaFacturas.Replace("@ClaveR", Documento.ClaveHacienda.ToString()).Replace("@SucursalR", "099");

                                HttpResponseMessage response2 = await cliente2.GetAsync(Url2);
                                if (response2.IsSuccessStatusCode)
                                {
                                    response2.Content.Headers.ContentType.MediaType = "application/json";
                                    var res2 = await response2.Content.ReadAsStringAsync();
                                }

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
                            }

                        }

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
                    }


                    //Se termina el api de facturacion 



                }




                else
                {
                    throw new Exception("Ya existe un documento con este ID");
                }
                documento.Fecha = Documento.Fecha;
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, documento);
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

                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, be);
            }
        }


        [Route("api/Documentos/Actualizar")]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] Documentos documento)
        {
            var t = db.Database.BeginTransaction();
            try
            {
                EncDocumento Documento = db.EncDocumento.Where(a => a.id == documento.id).FirstOrDefault();
                if (Documento != null)
                {
                    db.Entry(Documento).State = EntityState.Modified;
                    Documento.idCliente = documento.idCliente;
                    Documento.idUsuarioCreador = documento.idUsuarioCreador;
                    Documento.Fecha = DateTime.Now;
                    Documento.FechaVencimiento = DateTime.Now;//documento.FechaVencimiento;

                    Documento.Comentarios = documento.Comentarios;
                    Documento.Subtotal = documento.Subtotal;
                    Documento.TotalImpuestos = documento.TotalImpuestos;
                    Documento.TotalDescuento = documento.TotalDescuento;
                    Documento.TotalCompra = documento.TotalCompra;
                    Documento.PorDescto = documento.PorDescto;
                    Documento.Moneda = documento.Moneda;
                    Documento.TipoDocumento = documento.TipoDocumento;
                    Documento.idVendedor = documento.idVendedor;

                    // Documento.Status = documetno.Status;


                    db.SaveChanges();

                    var Detalles = db.DetDocumento.Where(a => a.idEncabezado == Documento.id).ToList();

                    foreach (var item in Detalles)
                    {
                        var prod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                        if (prod != null)
                        {

                            db.Entry(prod).State = EntityState.Modified;
                            if (Documento.TipoDocumento == "01" || Documento.TipoDocumento == "04")
                            {
                                prod.Stock += item.Cantidad;

                            }
                            else
                            {
                                prod.Stock -= item.Cantidad;
                            }
                            db.SaveChanges();
                        }

                        db.DetDocumento.Remove(item);
                        db.SaveChanges();
                    }


                    var i = 0;
                    foreach (var item in documento.Detalle)
                    {
                        DetDocumento det = new DetDocumento();
                        det.idEncabezado = Documento.id;
                        det.idProducto = item.idProducto;
                        det.NumLinea = i;
                        det.PorDescto = item.PorDescto;
                        det.PrecioUnitario = item.PrecioUnitario;
                        det.TotalImpuesto = item.TotalImpuesto;
                        det.Cantidad = item.Cantidad;
                        det.Descuento = item.Descuento;
                        det.Cabys = item.Cabys;
                        det.TotalLinea = item.TotalLinea;//((det.PrecioUnitario * det.Cantidad) - det.Descuento) + det.TotalImpuesto;
                        det.idExoneracion = item.idExoneracion;
                        db.DetDocumento.Add(det);
                        db.SaveChanges();
                        i++;

                        var prod = db.Productos.Where(a => a.id == item.idProducto).FirstOrDefault();
                        if (prod != null)
                        {

                            db.Entry(prod).State = EntityState.Modified;
                            if (Documento.TipoDocumento == "01" || Documento.TipoDocumento == "04")
                            {
                                prod.Stock -= item.Cantidad;

                            }
                            else
                            {
                                prod.Stock += item.Cantidad;
                            }
                            db.SaveChanges();
                        }
                    }

                    var MetodosPagos2 = db.MetodosPagos.Where(a => a.idEncabezado == Documento.id).ToList();
                    var time = DateTime.Now.Date;
                    var CierreCaja = db.CierreCajas.Where(a => a.FechaCaja == time && a.idCaja == documento.idCaja).FirstOrDefault();

                    foreach (var item in MetodosPagos2)
                    {
                        if (CierreCaja != null)
                        {
                            db.Entry(CierreCaja).State = EntityState.Modified;
                            if (Documento.Moneda == "CRC")
                            {
                                switch (item.Metodo)
                                {
                                    case "Efectivo":
                                        {
                                            CierreCaja.EfectivoColones -= item.Monto;
                                            CierreCaja.TotalVendidoColones -= item.Monto;
                                            break;
                                        }
                                    case "Tarjeta":
                                        {
                                            CierreCaja.TarjetasColones -= item.Monto;
                                            CierreCaja.TotalVendidoColones -= item.Monto;

                                            break;
                                        }
                                    case "Cheque":
                                        {
                                            CierreCaja.ChequesColones -= item.Monto;
                                            CierreCaja.TotalVendidoColones -= item.Monto;

                                            break;
                                        }

                                    default:
                                        {
                                            CierreCaja.OtrosMediosColones -= item.Monto;
                                            CierreCaja.TotalVendidoColones -= item.Monto;

                                            break;
                                        }

                                }

                            }
                            else
                            {
                                switch (item.Metodo)
                                {
                                    case "Efectivo":
                                        {
                                            CierreCaja.EfectivoFC -= item.Monto;
                                            CierreCaja.TotalVendidoFC -= item.Monto;
                                            break;
                                        }
                                    case "Tarjeta":
                                        {
                                            CierreCaja.TarjetasFC -= item.Monto;
                                            CierreCaja.TotalVendidoFC -= item.Monto;

                                            break;
                                        }
                                    case "Cheque":
                                        {
                                            CierreCaja.ChequesFC -= item.Monto;
                                            CierreCaja.TotalVendidoFC -= item.Monto;

                                            break;
                                        }

                                    default:
                                        {
                                            CierreCaja.OtrosMediosFC -= item.Monto;
                                            CierreCaja.TotalVendidoFC -= item.Monto;

                                            break;
                                        }

                                }
                            }
                            db.SaveChanges();
                        }
                        db.MetodosPagos.Remove(item);
                        db.SaveChanges();
                    }
                    if (documento.MetodosPagos != null)
                    {
                        foreach (var item in documento.MetodosPagos)
                        {
                            MetodosPagos MetodosPagos = new MetodosPagos();
                            MetodosPagos.idEncabezado = Documento.id;
                            MetodosPagos.Monto = item.Monto;
                            MetodosPagos.BIN = item.BIN;
                            MetodosPagos.NumCheque = item.NumCheque;
                            MetodosPagos.NumReferencia = item.NumReferencia;
                            MetodosPagos.idCuentaBancaria = item.idCuentaBancaria;
                            MetodosPagos.Metodo = item.Metodo;

                            db.MetodosPagos.Add(MetodosPagos);
                            db.SaveChanges();

                            if (CierreCaja != null)
                            {
                                db.Entry(CierreCaja).State = EntityState.Modified;
                                if (Documento.Moneda == "CRC")
                                {
                                    switch (item.Metodo)
                                    {
                                        case "Efectivo":
                                            {
                                                CierreCaja.EfectivoColones += item.Monto;
                                                CierreCaja.TotalVendidoColones += item.Monto;
                                                break;
                                            }
                                        case "Tarjeta":
                                            {
                                                CierreCaja.TarjetasColones += item.Monto;
                                                CierreCaja.TotalVendidoColones += item.Monto;

                                                break;
                                            }
                                        case "Cheque":
                                            {
                                                CierreCaja.ChequesColones += item.Monto;
                                                CierreCaja.TotalVendidoColones += item.Monto;

                                                break;
                                            }

                                        default:
                                            {
                                                CierreCaja.OtrosMediosColones += item.Monto;
                                                CierreCaja.TotalVendidoColones += item.Monto;

                                                break;
                                            }

                                    }

                                }
                                else
                                {
                                    switch (item.Metodo)
                                    {
                                        case "Efectivo":
                                            {
                                                CierreCaja.EfectivoFC += item.Monto;
                                                CierreCaja.TotalVendidoFC += item.Monto;
                                                break;
                                            }
                                        case "Tarjeta":
                                            {
                                                CierreCaja.TarjetasFC += item.Monto;
                                                CierreCaja.TotalVendidoFC += item.Monto;

                                                break;
                                            }
                                        case "Cheque":
                                            {
                                                CierreCaja.ChequesFC += item.Monto;
                                                CierreCaja.TotalVendidoFC += item.Monto;

                                                break;
                                            }

                                        default:
                                            {
                                                CierreCaja.OtrosMediosFC += item.Monto;
                                                CierreCaja.TotalVendidoFC += item.Monto;

                                                break;
                                            }

                                    }
                                }
                                db.SaveChanges();
                            }

                        }
                    }



                    BitacoraMovimientos btm = new BitacoraMovimientos();
                    btm.idUsuario = documento.idUsuarioCreador;
                    btm.Descripcion = "Se edito el documento: " + Documento.id + " del cliente con el id: " + documento.idCliente;
                    btm.Fecha = DateTime.Now;
                    btm.Metodo = "Edicion de Documento";
                    db.BitacoraMovimientos.Add(btm);
                    db.SaveChanges();
                    t.Commit();
                }
                else
                {
                    throw new Exception("NO existe un documento con este ID");
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


        [Route("api/Documentos/ActualizarConsecutivos")]
        public HttpResponseMessage GetActualizarConsecutivos()
        {
            try
            {
                Parametros parametros = db.Parametros.FirstOrDefault();
                var conexion = G.DevuelveCadena(db);

                var Datos = db.ConexionSAP.FirstOrDefault();




                var SQL = "EXEC dbo.SincronizaNOVAAPPFEHACIENDA";

                db.Database.ExecuteSqlCommand(SQL);

                return Request.CreateResponse(System.Net.HttpStatusCode.OK);

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
    }
}
