using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using WMS.Models;
using SAPbobsCOM;
using System.Security.Claims;
using System.Threading;
using WMS.Logic.Stores;
using System.IO;
using static WMS.Models.Recuento;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Mvc;

namespace WMS.Logic.Comun
{
    public class Integrar
    {
        Company oCompany;
        CompanyService sCompany;
        private SAP modeloSAp;
        private readonly ClaimsPrincipal Identity;
        private readonly Utilities.Log Logs;
        private readonly string CadenaSAP;
        private readonly Obtener Ob;
        private readonly StoreProcedure SP;

        public Integrar() {
            oCompany = null;
            sCompany = null;
            modeloSAp = null;
            Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            CadenaSAP = ConfigurationManager.ConnectionStrings["Sap"].ConnectionString;
            Logs = new Utilities.Log();
            Ob = new Obtener();
            SP = new StoreProcedure();
        }

        //Crear documento de Traspaso Manual
        public bool Documento(ref Mensajes RespSAP, TrsManual.Traspaso InDoc, string g, string CodeGuia) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            string horaDoc = DateTime.Now.ToString("HH:mm:ss");

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Creación del Traspaso", Usuario, 0, modulo, metodo); //Log Informativo

            try
            {
                string msj = string.Empty;
                if (Conectar(ref msj))
                {
                    int turnoDoc = Ob.ObtenerTurnoDocumento(horaDoc);

                    Logs.FullInfo("", "Encabezado: \r\n\t DocDate => " + DateTime.Now +
                                            "- \r\n\t CardCode => " + InDoc.ClienteSel +
                                            "- \r\n\t From WarehouseCode => " + InDoc.Lineas[0].WhsCode +
                                            "- \r\n\t To Warehouse => " + InDoc.AlmacenSel.ToString() +
                                            "- \r\n\t U_WMS_USERNAME => " + Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() +
                                            "- \r\n\t U_WMS_USERCODE => HH " + 
                                            "- \r\n\t U_Time => " + DateTime.Now.ToString("HH:mm:ss") +
                                            "- \r\n\t U_Turno => " + turnoDoc.ToString() + 
                                            "- \r\n\t U_WMS_CodeGuid => " + CodeGuia +

                                            "- \r\n\n  Integracion de Lineas del Traspaso"
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                    StockTransfer oTransfer = (StockTransfer)oCompany.GetBusinessObject(BoObjectTypes.oStockTransfer);
                    oTransfer.DocDate = DateTime.Now;
                    oTransfer.CardCode = String.IsNullOrEmpty(InDoc.ClienteSel) ? "" : InDoc.ClienteSel;
                    oTransfer.FromWarehouse = InDoc.Lineas[0].WhsCode;
                    oTransfer.ToWarehouse = InDoc.AlmacenSel.ToString();
                    oTransfer.UserFields.Fields.Item("U_WMS_USERNAME").Value = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                    oTransfer.UserFields.Fields.Item("U_WMS_USERCODE").Value = "HH";
                    oTransfer.UserFields.Fields.Item("U_Time").Value = DateTime.Now.ToString("HH:mm:ss");
                    oTransfer.UserFields.Fields.Item("U_Turno").Value = turnoDoc.ToString();
                    oTransfer.UserFields.Fields.Item("U_WMS_CodeGuid").Value = CodeGuia;

                    Logs.FullInfo("", "***Encabezado : Datos OK *********************************************\r\n", Usuario, 0, modulo, metodo); //Log Informativo

                    var Lineas = Ob.Lineas(g, "U"); //Obtener informacion guardada en tabla Movimiento con el idGuid
                    foreach (Recuperar.Line linea in Lineas)
                    {
                        Logs.FullInfo("********Datos de Integracion Linea " + linea.LineNum + "********", "", Usuario, 3, "", ""); //Log Informativo
                        oTransfer.Lines.ItemCode = linea.ItemCode;
                        oTransfer.Lines.Quantity = linea.Qty;
                        oTransfer.Lines.FromWarehouseCode = linea.Desde;
                        oTransfer.Lines.WarehouseCode = linea.Hasta;
                        oTransfer.Lines.UserFields.Fields.Item("U_Dispositivo").Value = linea.Dispositivo;
                        oTransfer.Lines.UserFields.Fields.Item("U_Etis_Emb2").Value = linea.Lotes;
                        oTransfer.Lines.UserFields.Fields.Item("U_Cant_Disp").Value = linea.NumDispositivos;
                        oTransfer.Lines.UserFields.Fields.Item("U_PesoBruto").Value = linea.PesoBruto;
                        oTransfer.Lines.UserFields.Fields.Item("U_PesoNeto").Value = linea.PesoNeto;

                        Logs.FullInfo("", "***Lineas Traspaso : Datos OK *************************************\r\n", Usuario, 0, modulo, metodo); //Log Informativo

                        //Lotes
                        var LotesLineas = Ob.LotesLineas(g, linea.ItemCode, linea.LineNum, 0, linea.Dispositivo, "U", linea.UbDestino);
                        foreach (Recuperar.Batch bt in LotesLineas) 
                        {
                            Logs.FullInfo("\t >>>>>> Datos de Integracion del Lote " + bt.Lote + " >>>>>>", "", Usuario, 3, "", ""); //Log Informativo

                            if (!string.IsNullOrEmpty(bt.Lote))
                            {
                                oTransfer.Lines.BatchNumbers.BaseLineNumber = linea.LineNum;
                                oTransfer.Lines.BatchNumbers.BatchNumber = bt.Lote;
                                oTransfer.Lines.BatchNumbers.Location = linea.Desde;
                                oTransfer.Lines.BatchNumbers.Quantity = bt.QtyLote;
                                
                                oTransfer.Lines.BatchNumbers.Add();
                                Logs.FullInfo("", "***Lotes para la Linea: " + linea.LineNum + " con Articulo: " + linea.ItemCode + " ::: Datos OK *************************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                            }

                            //Ubicacion Origen
                            var UbicacionOrigen = Ob.Ubicaciones(g, linea.ItemCode, linea.LineNum, bt.LineNumLote, bt.Lote, 0, linea.Dispositivo, "U", linea.UbDestino);
                            foreach (Recuperar.BinLocation blO in UbicacionOrigen) 
                            {
                                if (blO.UbOrigen > 0)
                                {
                                    oTransfer.Lines.BinAllocations.BinAbsEntry = blO.UbOrigen;
                                    oTransfer.Lines.BinAllocations.Quantity = blO.QtyUbOrigen;
                                    oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = bt.LineNumLote;
                                    oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batFromWarehouse;
                                    oTransfer.Lines.BinAllocations.BaseLineNumber = linea.LineNum;
                                    
                                    oTransfer.Lines.BinAllocations.Add();
                                    Logs.FullInfo("", "***Ubicacion Origen del Lote " + bt.Lote + " ::: Datos OK *********************************************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                                }
                            }

                            //Ubicacion Destino
                            var UbicacionDestino = Ob.UbicacionesDest(g, linea.ItemCode, linea.LineNum, bt.LineNumLote, bt.Lote, 0, linea.Dispositivo, "U", linea.UbDestino);
                            foreach (Recuperar.BinLocation blD in UbicacionDestino)
                            {
                                if (blD.UbDestino > 0)
                                {
                                    oTransfer.Lines.BinAllocations.BinAbsEntry = blD.UbDestino;
                                    oTransfer.Lines.BinAllocations.Quantity = blD.QtyUbDestino;
                                    oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = bt.LineNumLote;
                                    oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batToWarehouse;
                                    oTransfer.Lines.BinAllocations.BaseLineNumber = linea.LineNum;
                                    
                                    oTransfer.Lines.BinAllocations.Add();
                                    Logs.FullInfo("", "***Ubicacion Destino del Lote " + bt.Lote + " ::: Datos OK *********************************************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                                } 
                            }

                            Logs.FullInfo("\t <<<<<< Datos de Integracion del Lote " + bt.Lote + " <<<<<<< \r\n", "", Usuario, 4, "", ""); //Log Informativo
                        }

                        oTransfer.Lines.Add();
                        Logs.FullInfo("", "***Linea de Traspaso Agregada : Datos OK ************************************\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                        Logs.FullInfo("Datos de Integracion Linea " + linea.LineNum + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                    }
                    
                    if (oTransfer.Add() != 0)
                    {
                        RespSAP.Mensaje = oCompany.GetLastErrorDescription();

                        Logs.FullInfo("", "\r\n Error: NO se realizó Integración, No se Creó Traspaso", Usuario, 0, modulo, metodo); //Log Informativo
                        return false;
                    }
                    else
                    {
                        oTransfer.GetByKey(int.Parse(oCompany.GetNewObjectKey()));
                        RespSAP.DocEntry = oTransfer.DocEntry;
                        RespSAP.DocNum = oTransfer.DocNum;

                        Logs.FullInfo("", "Integración Realizada: Traspaso Realizado \r\n DocNum => " + oTransfer.DocNum + "\r\n DocEntry => " + oTransfer.DocEntry + " \r\n**Integracion OK *******************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                        return true;
                    }
                }
                else
                {
                    RespSAP.Mensaje = msj;
                    Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj + "\r\n\n", Usuario, 0, modulo, metodo); //Log Informativo

                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                RespSAP.Mensaje = ex.Message;

                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;

                    Logs.FullInfo("", "Conexion a SAP Cerrada" + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Crear Remision
        public bool Remision(ref Mensajes RespRem, Remision.Preliminar Preliminar, string g, string CodeGuia) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            string horaDoc = DateTime.Now.ToString("HH:mm:ss");

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Creación de Documento Preliminar para Remision", Usuario, 0, modulo, metodo); //Log Informativo

            try
            {
                string msj = string.Empty;
                if (Conectar(ref msj))
                {
                    int turnoDoc = Ob.ObtenerTurnoDocumento(horaDoc);
                    Logs.FullInfo("", "Encabezado: \r\n\t DocDate => " + DateTime.Now +
                                            "- \r\n\t CardCode => " + Preliminar.ClienteSel == "" ? "" : Preliminar.ClienteSel +
                                            "- \r\n\t DocType => Entrega(15)" +
                                            "- \r\n\t U_WMS_USERNAME => " + Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() + 
                                            "- \r\n\t U_WMS_USERCODE => HH " + 
                                            "- \r\n\t U_Time => " + DateTime.Now.ToString("HH:mm:ss") +
                                            "- \r\n\t U_Turno => " + turnoDoc.ToString() +
                                            "- \r\n\t NumAtCard => " + Preliminar.Referencia +
                                            "- \r\n\t Comments => Basado en el pedido " + Preliminar.Pedido +
                                            "- \r\n\t U_WMS_CodeGuid => " + CodeGuia +

                                            "- \r\n\n  Integracion de Lineas del Preliminar"
                                            , Usuario, 0, modulo, metodo); //Log Informativo


                    Documents oRemision = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oDrafts);
                    oRemision.CardCode = Preliminar.ClienteSel == "" ? "" : Preliminar.ClienteSel;
                    oRemision.DocDate = DateTime.Now;
                    oRemision.DocObjectCode = BoObjectTypes.oDeliveryNotes;//15
                    oRemision.DocType = BoDocumentTypes.dDocument_Items;
                    oRemision.UserFields.Fields.Item("U_WMS_USERNAME").Value = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                    oRemision.UserFields.Fields.Item("U_WMS_USERCODE").Value = "HH";
                    oRemision.UserFields.Fields.Item("U_Time").Value = DateTime.Now.ToString("HH:mm:ss");
                    oRemision.UserFields.Fields.Item("U_Turno").Value = turnoDoc.ToString();
                    oRemision.NumAtCard = Preliminar.Referencia;
                    oRemision.Comments = "Basado en el pedido " + Preliminar.Pedido.ToString();
                    oRemision.UserFields.Fields.Item("U_WMS_CodeGuid").Value = CodeGuia;

                    Logs.FullInfo("", "Encabezado: Informacion OK ******* ", Usuario, 0, modulo, metodo); //Log Informativo

                    foreach (Recuperar.Line lr in Ob.Lineas(g, "D")) 
                    {
                        Logs.FullInfo("********Datos de Integracion Linea " + lr.LineNum + "********", "", Usuario, 3, "", ""); //Log Informativo
                        Logs.FullInfo("", "\t Entrega Linea : " + lr.LineNum +
                                            "-\r\n\t ItemCode => " + lr.ItemCode +
                                            "-\r\n\t Quantity => " + lr.Qty +
                                            "-\r\n\t WarehouseCode => " + lr.Desde +
                                            "-\r\n\t U_Dispositivo => " + lr.Dispositivo +
                                            "-\r\n\t U_Etis_Emb2 => " + lr.Lotes +
                                            "-\r\n\t U_Cant_Disp => " + lr.NumDispositivos +
                                            "-\r\n\t U_CANT_EMPAQUE => " + lr.NumDispositivos +
                                            "-\r\n\t U_PesoBruto => " + lr.PesoBruto +
                                            "-\r\n\t U_PesoNeto => " + lr.PesoNeto +
                                            "-\r\n\t U_OC => " + lr.OC +
                                            "-\r\n\t U_Posicion => " + lr.LineOC.ToString() +
                                            "-\r\n\t FreeTxt - RemSubstitute => " + lr.RemSubstitute +
                                            "-\r\n\t U_Num_OC - RemPurchase => " + lr.RemPurchase +
                                            "-\r\n\t U_Empaque - RemEmpaquePallet => " + lr.RemEmpaquePallet 
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                        oRemision.Lines.ItemCode = lr.ItemCode;
                        oRemision.Lines.Quantity = lr.Qty;
                        oRemision.Lines.WarehouseCode = lr.Desde;
                        //oRemision.Lines.UserFields.Fields.Item("U_Tarima").Value = lr.Tarima;
                        oRemision.Lines.UserFields.Fields.Item("U_Etis_Emb2").Value = lr.Lotes;
                        oRemision.Lines.UserFields.Fields.Item("U_Dispositivo").Value = lr.Dispositivo;//SP.ObtenerValorCampo("@EVT_ETQSPT", "U_Dispositivo", lr.Lotes.Split(',')[0].ToString());
                        oRemision.Lines.UserFields.Fields.Item("U_Cant_Disp").Value = lr.NumDispositivos;//lr.Lotes.Split(',').Length;
                        oRemision.Lines.UserFields.Fields.Item("U_PesoBruto").Value = lr.PesoBruto;
                        oRemision.Lines.UserFields.Fields.Item("U_PesoNeto").Value = lr.PesoNeto;

                        //Actualización agregando OC y Posicion OC - 13-01-2021
                        oRemision.Lines.UserFields.Fields.Item("U_OC").Value = lr.OC;
                        oRemision.Lines.UserFields.Fields.Item("U_Posicion").Value = lr.LineOC.ToString();

                        //Adicionales
                        oRemision.Lines.FreeText = lr.RemSubstitute;
                        oRemision.Lines.UserFields.Fields.Item("U_CANT_EMPAQUE").Value = lr.NumDispositivos;
                        oRemision.Lines.UserFields.Fields.Item("U_NUM_OC").Value = lr.RemPurchase;
                        oRemision.Lines.UserFields.Fields.Item("U_EMPAQUE").Value = lr.RemEmpaquePallet;

                        Logs.FullInfo("", "***Linea Entrega : Datos OK ************ ", Usuario, 0, modulo, metodo); //Log Informativo

                        foreach (Recuperar.Batch bt in Ob.LotesLineas(g, lr.ItemCode, lr.LineNum, lr.Tarima, lr.Dispositivo, "D", 0)) 
                        {
                            Logs.FullInfo("\t >>>>>> Datos de Integracion del Lote " + bt.Lote + " >>>>>>", "", Usuario, 3, "", ""); //Log Informativo
                            Logs.FullInfo("", "\t INTEGRACION de Lote para la Linea: " + lr.LineNum + " con Articulo: " + lr.ItemCode +
                                                                    "\r\n\t BaseLineNumber => " + lr.LineNum +
                                                                    "\r\n\t BatchNumber => " + bt.Lote +
                                                                    "\r\n\t Location => " + lr.Desde +
                                                                    "\r\n\t Quantity => " + bt.QtyLote
                                                                    , Usuario, 0, modulo, metodo); //Log Informativo

                            oRemision.Lines.BatchNumbers.BaseLineNumber = lr.LineNum;
                            oRemision.Lines.BatchNumbers.BatchNumber = bt.Lote;
                            oRemision.Lines.BatchNumbers.Location = lr.Desde;
                            oRemision.Lines.BatchNumbers.Quantity = bt.QtyLote;

                            Logs.FullInfo("", "** Lotes para la Linea: " + lr.LineNum + " con Articulo: " + lr.ItemCode + " ::: Datos OK **** ", Usuario, 0, modulo, metodo); //Log Informativo

                            foreach (Recuperar.BinLocation bn in Ob.Ubicaciones(g, lr.ItemCode, lr.LineNum, bt.LineNumLote, bt.Lote, lr.Tarima, lr.Dispositivo, "D", 0)) 
                            {
                                Logs.FullInfo("", "INTEGRACION Ubicacion Origen para el Lote " + bt.Lote +
                                                                        "\r\n\t BinAbsEntry => " + bn.UbOrigen +
                                                                        "\r\n\t Quantity => " + bn.QtyUbOrigen +
                                                                        "\r\n\t SerialAndBatchNumbersBaseLine => " + bt.LineNumLote +
                                                                        "\r\n\t BaseLineNumber => " + lr.LineNum
                                                                        , Usuario, 0, modulo, metodo); //Log Informativo

                                oRemision.Lines.BinAllocations.BinAbsEntry = bn.UbOrigen;
                                oRemision.Lines.BinAllocations.Quantity = bn.QtyUbOrigen;
                                oRemision.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = bt.LineNumLote;
                                oRemision.Lines.BinAllocations.BaseLineNumber = lr.LineNum;
                                oRemision.Lines.BinAllocations.Add();

                                Logs.FullInfo("", "***Ubicacion Origen del Lote " + bt.Lote + " ::: Datos OK **********", Usuario, 0, modulo, metodo); //Log Informativo
                            }
                            oRemision.Lines.BatchNumbers.Add();
                            Logs.FullInfo("\t <<<<<< Datos de Integracion del Lote " + bt.Lote + " <<<<<<< \r\n", "", Usuario, 4, "", ""); //Log Informativo
                        }
                        oRemision.Lines.Add();
                        Logs.FullInfo("", "***Linea de Traspaso Agregada : Datos OK *******\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                        Logs.FullInfo("Datos de Integracion Linea " + lr.LineNum + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                    }

                    /*
                    Logs.FullInfo("", "INTEGRACION Linea Especial " +
                                                    "\r\n\t LineType => Texto " +
                                                    "\r\n\t LineText => " + Preliminar.Referencia +
                                                    "\r\n\t AfterLineNumber => " + oRemision.Lines.LineNum 
                                                    , Usuario, 0, modulo, metodo); //Log Informativo

                    oRemision.SpecialLines.LineType = SAPbobsCOM.BoDocSpecialLineType.dslt_Text;
                    oRemision.SpecialLines.LineText = Preliminar.Referencia;
                    oRemision.SpecialLines.AfterLineNumber = oRemision.Lines.LineNum;
                    oRemision.SpecialLines.Add();
                    Logs.FullInfo("", "*** Linea Especial ::: Datos OK **** ", Usuario, 0, modulo, metodo); //Log Informativo
                    */

                    if (oRemision.Add() != 0)
                    {
                        RespRem.Mensaje = oCompany.GetLastErrorDescription();
                        Logs.FullInfo("", "\r\n Error: NO se realizó Integración, No se Creó Remisión", Usuario, 0, modulo, metodo); //Log Informativo
                        return false;
                    }
                    else 
                    {
                        oRemision.GetByKey(int.Parse(oCompany.GetNewObjectKey()));
                        RespRem.DocEntry = oRemision.DocEntry;
                        RespRem.DocNum = oRemision.DocNum;
                        RespRem.Mensaje = "";

                        //Actualización: NO se actualiza el Status de la Etiqueta al Crear el Documento Preliminar - 13-01-2021
                        //SP.CambiarEstadoEts("@EVT_ETQSPT","E",Preliminar.Lineas, Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault());
                        SP.RemoverRegistros("Movimiento", g);

                        Logs.FullInfo("", "Integración Realizada: Documento Preliminar de Remision Realizada: \r\n DocNum => " + oRemision.DocNum + 
                                            "- \r\n DocEntry => " + oRemision.DocEntry + 
                                            " \r\n ** Integracion OK ****** "
                                            , Usuario, 0, modulo, metodo); //Log Informativo
                        return true;
                    }
                }
                else 
                {
                    RespRem.Mensaje = msj;
                    Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj + "\r\n\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                RespRem.Mensaje = ex.Message;

                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                    {
                        oCompany.Disconnect();
                        Marshal.ReleaseComObject(oCompany);
                        oCompany = null;

                        Logs.FullInfo("", "Conexion a SAP Cerrada" + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }                        
                }

                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }
        
        public bool EnvioProduccion(ref Mensajes RespProd, Produccion.Documento Doc, Produccion.Datos Dts) {
            try
            {
                string msj = string.Empty;
                string Recurso = string.Empty;
                Produccion.Ubicacion Ubicacion = new Produccion.Ubicacion();
                Produccion.Ubicacion Dftl = new Produccion.Ubicacion();
                int i = 0;
                if (Conectar(ref msj))
                {
                    StockTransfer oTransfer = (StockTransfer)oCompany.GetBusinessObject(BoObjectTypes.oStockTransfer);
                    oTransfer.DocDate = DateTime.Now;
                    oTransfer.FromWarehouse = Dts.LineasSO[0].Filler;
                    oTransfer.ToWarehouse = Dts.LineasSO[0].ToWhsCode;
                    oTransfer.UserFields.Fields.Item("U_WMS_USERNAME").Value = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                    oTransfer.UserFields.Fields.Item("U_WMS_USERCODE").Value = "HH";
                    oTransfer.UserFields.Fields.Item("U_Time").Value = DateTime.Now.ToString("HH:mm:ss");
                    oTransfer.UserFields.Fields.Item("U_OF").Value = Doc.NumOrdr;

                    foreach (var l in Doc.Lineas)
                    {
                        oTransfer.Lines.ItemCode = l.ItemCode;
                        oTransfer.Lines.Quantity = (double)l.OnHandQty;
                        oTransfer.Lines.FromWarehouseCode = l.WhsCode;
                        oTransfer.Lines.WarehouseCode = Dts.LineasSO[0].ToWhsCode;
                        oTransfer.Lines.BaseEntry = Doc.NumST;
                        oTransfer.Lines.BaseLine = Ob.ObtenerLineasSol(Dts.LineasSO, l.ItemCode);
                        oTransfer.Lines.BaseType = InvBaseDocTypeEnum.InventoryTransferRequest;

                        //si es granel se va a default de almacén 11
                        if (Ob.EsGranel(ref Dftl, l.ItemCode, Dts.LineasSO))
                        {
                            //Tiene lotes
                            if (!String.IsNullOrEmpty(l.DistNumber))
                            {
                                oTransfer.Lines.BatchNumbers.BaseLineNumber = i;
                                oTransfer.Lines.BatchNumbers.BatchNumber = l.DistNumber;
                                oTransfer.Lines.BatchNumbers.Location = l.WhsCode;
                                oTransfer.Lines.BatchNumbers.Quantity = (double)l.OnHandQty;

                                oTransfer.Lines.BinAllocations.BinAbsEntry = l.AbsEntry;
                                oTransfer.Lines.BinAllocations.Quantity = (double)l.OnHandQty;
                                oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                                oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batFromWarehouse;
                                oTransfer.Lines.BinAllocations.BaseLineNumber = i;
                                oTransfer.Lines.BinAllocations.Add();

                                oTransfer.Lines.BinAllocations.BinAbsEntry = Dftl.AbsEntry;
                                oTransfer.Lines.BinAllocations.Quantity = (double)l.OnHandQty;
                                oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                                oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batToWarehouse;
                                oTransfer.Lines.BinAllocations.BaseLineNumber = i;
                                oTransfer.Lines.BinAllocations.Add();

                                oTransfer.Lines.BatchNumbers.Add();
                            }
                            //No tiene lotes
                            else
                            {
                                oTransfer.Lines.BinAllocations.BinAbsEntry = Ob.ObtenerUbiSys(Dts.LineasSO[0].Filler).AbsEntry;
                                oTransfer.Lines.BinAllocations.Quantity = (double)l.OnHandQty;
                                oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                                oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batFromWarehouse;
                                oTransfer.Lines.BinAllocations.BaseLineNumber = i;
                                oTransfer.Lines.BinAllocations.Add();

                                oTransfer.Lines.BinAllocations.BinAbsEntry = Dftl.AbsEntry;
                                oTransfer.Lines.BinAllocations.Quantity = (double)l.OnHandQty;
                                oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                                oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batToWarehouse;
                                oTransfer.Lines.BinAllocations.BaseLineNumber = i;
                                oTransfer.Lines.BinAllocations.Add();
                            }
                        }
                        //si no es granel se va ubicacion de maquina
                        else
                        {
                            if (!String.IsNullOrEmpty(l.DistNumber)) {
                                oTransfer.Lines.BatchNumbers.BaseLineNumber = i;
                                oTransfer.Lines.BatchNumbers.BatchNumber = l.DistNumber;
                                oTransfer.Lines.BatchNumbers.Location = l.WhsCode;
                                oTransfer.Lines.BatchNumbers.Quantity = (double)l.OnHandQty;

                                if (Ob.ExisteRecurso(ref Recurso, Dts.LineasOF))
                                {//validamos que exista el recurso de tipo maquina en las lineas de la OF
                                    if (Ob.UbiRecurso(ref Ubicacion, Recurso))
                                    {//Obtenemos la ubicacion del recurso Maquina

                                        oTransfer.Lines.BinAllocations.BinAbsEntry = l.AbsEntry;
                                        oTransfer.Lines.BinAllocations.Quantity = (double)l.OnHandQty;
                                        oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                                        oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batFromWarehouse;
                                        oTransfer.Lines.BinAllocations.BaseLineNumber = i;
                                        oTransfer.Lines.BinAllocations.Add();

                                        oTransfer.Lines.BinAllocations.BinAbsEntry = Ubicacion.AbsEntry;
                                        oTransfer.Lines.BinAllocations.Quantity = (double)l.OnHandQty;
                                        oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                                        oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batToWarehouse;
                                        oTransfer.Lines.BinAllocations.BaseLineNumber = i;
                                        oTransfer.Lines.BinAllocations.Add();
                                    }
                                }
                                oTransfer.Lines.BatchNumbers.Add();
                            }
                            else {
                                if (Ob.ExisteRecurso(ref Recurso, Dts.LineasOF))
                                {//validamos que exista el recurso de tipo maquina en las lineas de la OF
                                    if (Ob.UbiRecurso(ref Ubicacion, Recurso))
                                    {//Obtenemos la ubicacion del recurso Maquina

                                        oTransfer.Lines.BinAllocations.BinAbsEntry = Ob.ObtenerUbiSys(Dts.LineasSO[0].Filler).AbsEntry;
                                        oTransfer.Lines.BinAllocations.Quantity = (double)l.OnHandQty;
                                        oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                                        oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batFromWarehouse;
                                        oTransfer.Lines.BinAllocations.BaseLineNumber = i;
                                        oTransfer.Lines.BinAllocations.Add();

                                        oTransfer.Lines.BinAllocations.BinAbsEntry = Ubicacion.AbsEntry;
                                        oTransfer.Lines.BinAllocations.Quantity = (double)l.OnHandQty;
                                        oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                                        oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batToWarehouse;
                                        oTransfer.Lines.BinAllocations.BaseLineNumber = i;
                                        oTransfer.Lines.BinAllocations.Add();
                                    }
                                }
                            }
                        }
                        oTransfer.Lines.Add();
                        i++;
                    }
                    if (oTransfer.Add() != 0)
                    {
                        RespProd.Mensaje = oCompany.GetLastErrorDescription();
                        return false;
                    }
                    else
                    {
                        //oCompany.GetNewObjectCode(out string num);
                        RespProd.DocEntry = int.Parse(oCompany.GetNewObjectKey()); //Se obtiene el DocEntry del Documento Creado
                        RespProd.DocNum = Ob.RecDocNum("T", RespProd.DocEntry.ToString()); //int.Parse(num); //Obtiene el DocNum del documento creado en la Transferencia de Inventario
                        RespProd.Mensaje = "";
                        return true;
                    }
                }
                else
                {
                    RespProd.Mensaje = msj;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                RespProd.Mensaje = ex.Message;
                return false;
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
            }
        }
        
        //Modulo Envio a Produccion
        public bool TraspasoAProduccion(ref Mensajes RespProd, Produccion.Datos Dts, int NumSol, int NumOrd, string g) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Creación de Traspaso", Usuario, 0, modulo, metodo); //Log Informativo
            try
            {
                string msj = string.Empty;
                if (Conectar(ref msj))
                {
                    Logs.FullInfo("", "Encabezado: \r\n\t DocDate => " + DateTime.Now +
                                            "- \r\n\t From WarehouseCode => " + ConfigurationManager.AppSettings["AlmSurtido"] +
                                            "- \r\n\t To Warehouse => " + Dts.LineasSO[0].ToWhsCode +
                                            "- \r\n\t U_WMS_USERNAME => " + Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() +
                                            "- \r\n\t U_WMS_USERCODE => HH " +
                                            "- \r\n\t U_Time => " + DateTime.Now.ToString("HH:mm:ss") +
                                            "- \r\n\t U_OF => " + NumOrd +

                                            "- \r\n\n  Integracion de Lineas del Traspaso"
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                    //Receipt doc = new Receipt();
                    StockTransfer oTransfer = (StockTransfer)oCompany.GetBusinessObject(BoObjectTypes.oStockTransfer);
                    oTransfer.DocDate = DateTime.Now;
                    oTransfer.FromWarehouse = ConfigurationManager.AppSettings["AlmSurtido"];// Dts.LineasSO[0].Filler;
                    oTransfer.ToWarehouse = Dts.LineasSO[0].ToWhsCode;
                    oTransfer.UserFields.Fields.Item("U_WMS_USERNAME").Value = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                    oTransfer.UserFields.Fields.Item("U_WMS_USERCODE").Value = "HH";
                    oTransfer.UserFields.Fields.Item("U_Time").Value = DateTime.Now.ToString("HH:mm:ss");
                    oTransfer.UserFields.Fields.Item("U_OF").Value = NumOrd;

                    Logs.FullInfo("", "***Encabezado : Datos OK ********* \r\n", Usuario, 0, modulo, metodo); //Log Informativo

                    foreach (Recuperar.Line l in Ob.Lineas(g, "D")) 
                    {
                        Logs.FullInfo("********Datos de Integracion Linea " + l.LineNum + "********", "", Usuario, 3, "", ""); //Log Informativo
                        Logs.FullInfo("", "\t Traspaso Linea : " + l.LineNum +
                                                            "\r\n\t NumLine => " + l.LineNum +
                                                            "\r\n\t ItemCode => " + l.ItemCode +
                                                            "\r\n\t Quantity => " + l.Qty +
                                                            "\r\n\t FromWarehouseCode => " + l.Desde +
                                                            "\r\n\t WarehouseCode => " + l.Hasta +
                                                            "\r\n\t BaseEntry => " + Dts.LineasSO[0].DocEntry +
                                                            "\r\n\t BaseLine => " + Ob.ObtenerLineasSol(Dts.LineasSO, l.ItemCode) +
                                                            "\r\n\t BaseType => Transferencia de Inventario" +
                                                            "\r\n\t U_Etis_Emb2 => " + l.Lotes 
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                        oTransfer.Lines.ItemCode = l.ItemCode;
                        oTransfer.Lines.Quantity = l.Qty;
                        oTransfer.Lines.FromWarehouseCode = l.Desde;
                        oTransfer.Lines.WarehouseCode = l.Hasta;//Dts.LineasSO[0].ToWhsCode;
                        oTransfer.Lines.BaseEntry = Dts.LineasSO[0].DocEntry;
                        oTransfer.Lines.BaseLine = Ob.ObtenerLineasSol(Dts.LineasSO, l.ItemCode);
                        oTransfer.Lines.BaseType = InvBaseDocTypeEnum.InventoryTransferRequest;
                        oTransfer.Lines.UserFields.Fields.Item("U_Etis_Emb2").Value = l.Lotes;

                        Logs.FullInfo("", "***Lineas Traspaso : Datos OK ******************** \r\n", Usuario, 0, modulo, metodo); //Log Informativo

                        foreach (Recuperar.Batch b in Ob.LotesLineas(g,l.ItemCode, l.LineNum, -1, "", "D", 0)) 
                        {
                            Logs.FullInfo("\t >>>>>> Datos de Integracion del Lote " + b.Lote + " >>>>>>", "", Usuario, 3, "", ""); //Log Informativo
                            if (!string.IsNullOrEmpty(b.Lote))
                            {
                                Logs.FullInfo("", "\t INTEGRACION de Lote para la Linea: " + l.LineNum + " con Articulo: " + l.ItemCode +
                                                                    "\r\n\t BaseLineNumber => " + l.LineNum +
                                                                    "\r\n\t BatchNumber => " + b.Lote +
                                                                    "\r\n\t Location => " + l.Desde +
                                                                    "\r\n\t Quantity => " + b.QtyLote
                                                                    , Usuario, 0, modulo, metodo); //Log Informativo

                                oTransfer.Lines.BatchNumbers.BaseLineNumber = l.LineNum;
                                oTransfer.Lines.BatchNumbers.BatchNumber = b.Lote;
                                oTransfer.Lines.BatchNumbers.Location = l.Desde;
                                oTransfer.Lines.BatchNumbers.Quantity = b.QtyLote;
                                
                                oTransfer.Lines.BatchNumbers.Add();
                                Logs.FullInfo("", "*** Lotes para la Linea: " + l.LineNum + " con Articulo: " + l.ItemCode + " ::: Datos OK ******** \r\n", Usuario, 0, modulo, metodo); //Log Informativo
                            }

                            foreach (Recuperar.BinLocation bl in Ob.Ubicaciones(g, l.ItemCode, l.LineNum,b.LineNumLote, b.Lote, -1, "", "D", 0)) 
                            {
                                Logs.FullInfo("", "INTEGRACION Ubicacion Origen para el Lote " + b.Lote +
                                                                        "\r\n\t BinAbsEntry => " + bl.UbOrigen +
                                                                        "\r\n\t Quantity => " + bl.QtyUbOrigen +
                                                                        "\r\n\t SerialAndBatchNumbersBaseLine => " + b.LineNumLote +
                                                                        "\r\n\t BinActionType => Salida " +
                                                                        "\r\n\t BaseLineNumber => " + l.LineNum
                                                                        , Usuario, 0, modulo, metodo); //Log Informativo

                                oTransfer.Lines.BinAllocations.BinAbsEntry = bl.UbOrigen;
                                oTransfer.Lines.BinAllocations.Quantity = bl.QtyUbOrigen;
                                oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = b.LineNumLote;
                                oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batFromWarehouse;
                                oTransfer.Lines.BinAllocations.BaseLineNumber = l.LineNum;
                                oTransfer.Lines.BinAllocations.Add();

                                Logs.FullInfo("", "***Ubicacion Origen del Lote " + b.Lote + " ::: Datos OK **********\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                            }
                            
                            foreach (Recuperar.BinLocation bld in Ob.UbicacionesDest(g, l.ItemCode, l.LineNum, b.LineNumLote, b.Lote, -1, "", "D", 0)) 
                            {
                                Logs.FullInfo("", "INTEGRACION Ubicacion Destino para el Lote " + b.Lote +
                                                                            "\r\n\t BinAbsEntry => " + bld.UbDestino +
                                                                            "\r\n\t Quantity => " + bld.QtyUbDestino +
                                                                            "\r\n\t SerialAndBatchNumbersBaseLine => " + b.LineNumLote +
                                                                            "\r\n\t BinActionType => Entrada" +
                                                                            "\r\n\t BaseLineNumber => " + l.LineNum
                                                                            , Usuario, 0, modulo, metodo); //Log Informativo

                                oTransfer.Lines.BinAllocations.BinAbsEntry = bld.UbDestino; //destino
                                oTransfer.Lines.BinAllocations.Quantity = bld.QtyUbDestino;
                                oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = b.LineNumLote;
                                oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batToWarehouse;
                                oTransfer.Lines.BinAllocations.BaseLineNumber = l.LineNum;
                                oTransfer.Lines.BinAllocations.Add();

                                Logs.FullInfo("", "***Ubicacion Destino del Lote " + b.Lote + " ::: Datos OK **********\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                            }

                            Logs.FullInfo("\t <<<<<< Datos de Integracion del Lote " + b.Lote + " <<<<<<< \r\n", "", Usuario, 4, "", ""); //Log Informativo
                        }

                        oTransfer.Lines.Add();
                        Logs.FullInfo("", "***Linea de Traspaso Agregada : Datos OK **************\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                        Logs.FullInfo("Datos de Integracion Linea " + l.LineNum + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                    }

                    if (oTransfer.Add() != 0)
                    {
                        RespProd.Mensaje = oCompany.GetLastErrorDescription();
                        Logs.FullInfo("", "\r\n Error: NO se realizó Integración, No se Creó Traspaso", Usuario, 0, modulo, metodo); //Log Informativo
                        return false;
                    }
                    else
                    {
                        //oCompany.GetNewObjectCode(out string num);
                        RespProd.DocEntry = int.Parse(oCompany.GetNewObjectKey()); //Se obtiene el DocEntry del Documento Creado
                        RespProd.DocNum = Ob.RecDocNum("T", RespProd.DocEntry.ToString()); //int.Parse(num); //Obtiene el DocNum del documento creado en la Transferencia de Inventario
                        RespProd.Mensaje = "";
                        //CerrarDocumento(NumSol);
                        SP.RemoverRegistros("Movimiento", g);

                        //Logs.FullInfo("", "Integración Realizada: Traspaso Realizado: DocNum => " + RespProd.DocNum + "- DocEntry => " + RespProd.DocEntry, Usuario, 0, modulo, metodo); //Log Informativo
                        Logs.FullInfo("", "Integración Realizada: Traspaso Realizado \r\n DocNum => " + RespProd.DocNum + 
                                                                    "\r\n DocEntry => " + RespProd.DocEntry + 
                                                                    " \r\n**Integracion OK *******\r\n"
                                                                    , Usuario, 0, modulo, metodo); //Log Informativo
                        return true;
                    }
                }
                else
                {
                    RespProd.Mensaje = msj;
                    //Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj, Usuario, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj + "\r\n\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                //Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                RespProd.Mensaje = ex.Message;
                return false;
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;

                    //Logs.FullInfo("", "Conexion a SAP Cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("", "Conexion a SAP Cerrada" + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Cerrar Solicitud de Traslado
        public void CerrarDocumento(int DocEntry)
        {
            try
            {
                int result = -100;
                StockTransfer _request = transfer(1250000001);
                _request.GetByKey(DocEntry);
                result = _request.Close();
                Logs.Registrar("Tipo de documento:", "Solicitud ", result.ToString(), this.GetType().FullName);
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
            }
        }

        //Cerrar ST adicional
        public StockTransfer transfer(int _objType)
        {
            switch (_objType)
            {
                case 1250000001:
                    return (StockTransfer)oCompany.GetBusinessObject(BoObjectTypes.oInventoryTransferRequest);
                default:
                    return null;
            }
        }

        //Modulo: Recibo de Produccion
        public void Aproduccion(ref Mensajes mensaje, ref List<MsjProduccion> DocsSap, Produccion.DocumentoAProduccion produccion, List<Produccion.ConsumoOrdenFabricacion> detalleOf, string guid, List<Produccion.DocumentoAProduccion> Recibo, string Maquina, string CodeGuia, List<EtqMaster.ListaMaster> regMaster)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Integración a SAP, Orden de Fabricacion: " + produccion.DocNum, "", Usuario, 3, "", ""); //Log Informativo
            
            MsjRecibo msj = new MsjRecibo();
            try
            {
                string msjS = string.Empty;
                DiApiConexion.Conectar(ref msjS); //Conexion a SAP
                if (string.IsNullOrEmpty(msjS))
                {
                    if (EmisionProduccion(ref msj, detalleOf, produccion, guid, Recibo, Maquina, CodeGuia, regMaster))
                    {
                        DocsSap.Add(new MsjProduccion
                        {
                            DocEntry = msj.DocEntry,
                            DocNum = msj.DocNum,
                            Integrado = true,
                            OF = produccion.DocNum,
                            MsjError = string.Empty
                        });

                        Logs.FullInfo("", "Integración a SAP Exitosa: DocNum " + msj.DocNum + " --- DocEntry " + msj.DocEntry, Usuario, 0, modulo, metodo);
                    }
                    else
                    {
                        DocsSap.Add(new MsjProduccion
                        {
                            DocEntry = "-1",
                            DocNum = "-1",
                            Integrado = false,
                            OF = produccion.DocNum,
                            MsjError = msj.Mensaje
                        });

                        Logs.FullInfo("", "Error de Integración: " + msj.Mensaje, Usuario, 0, modulo, metodo);
                    }
                }
                else
                {
                    mensaje.Mensaje = msjS;
                    DocsSap.Add(new MsjProduccion
                    {
                        DocEntry = "-1",
                        DocNum = "-1",
                        Integrado = false,
                        OF = produccion.DocNum,
                        MsjError = msjS
                    });

                    Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj, Usuario, 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                mensaje.Mensaje = ex.Message;
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                DiApiConexion.Desconectar();
                Logs.FullInfo("Integración a SAP, Orden de Fabricacion: " + produccion.DocNum, "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool EmisionProduccion(ref MsjRecibo ResEmision, List<Produccion.ConsumoOrdenFabricacion> Emision, Produccion.DocumentoAProduccion Recibo, string guid, List<Produccion.DocumentoAProduccion> reciboProd, string Maquina, string CodeGuia, List<EtqMaster.ListaMaster> regMaster)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            bool salida = false;
            Documents GoodIssue = null;
            int docnumEmision = 0;
            string docnumRecibo = "0";

            Logs.FullInfo("Creación de Emisión para OF " + Recibo.DocNum, "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                string msj = string.Empty;
                //Inicio de la Transaccion
                if (DiApiConexion.GetCompany().InTransaction)
                    DiApiConexion.GetCompany().EndTransaction(BoWfTransOpt.wf_RollBack);
                else
                    DiApiConexion.GetCompany().StartTransaction();

                Logs.FullInfo("", "Encabezado: \r\n\t DocDate => " + DateTime.Now +
                                            "- \r\n\t U_WMS_USERNAME => " + Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() +
                                            "- \r\n\t U_WMS_USERCODE => HH " +
                                            "- \r\n\t U_Time => " + DateTime.Now.ToString("HH:mm:ss") +
                                            "- \r\n\t U_OF => " + Recibo.DocNum +
                                            "- \r\n\t U_WMS_CodeGuid => " + CodeGuia +

                                            "- \r\n\n  Integracion de Lineas de Emisión"
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                GoodIssue = (Documents)DiApiConexion.GetCompany().GetBusinessObject(BoObjectTypes.oInventoryGenExit);
                GoodIssue.DocObjectCode = BoObjectTypes.oInventoryGenExit;
                GoodIssue.UserFields.Fields.Item("U_WMS_USERNAME").Value = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                GoodIssue.UserFields.Fields.Item("U_WMS_USERCODE").Value = "HH";
                GoodIssue.UserFields.Fields.Item("U_Time").Value = DateTime.Now.ToString("HH:mm:ss");
                GoodIssue.UserFields.Fields.Item("U_OF").Value = Recibo.DocNum;
                GoodIssue.UserFields.Fields.Item("U_Machine").Value = Maquina;
                GoodIssue.Reference2 = $"OF {Recibo.DocNum}";
                GoodIssue.Comments = $"Consumo de MP de Orden {Recibo.DocNum}";
                GoodIssue.UserFields.Fields.Item("U_WMS_CodeGuid").Value = CodeGuia;

                for (int e = 0; e < Emision.Count; e++)
                {
                    Logs.FullInfo("", "Lineas: " + e + " \r\n\t BaseEntry => " + Recibo.DocEntry +
                                    "- \r\n\t BaseType => 202" + 
                                    "- \r\n\t BaseLine => " + Emision[e].LineNum +
                                    "- \r\n\t Quantity => " + Emision[e].CantReq.ToString() +
                                    "- \r\n\t WarehouseCode => " + Emision[e].Warehouse
                                    , Usuario, 0, modulo, metodo); //Log Informativo

                    GoodIssue.Lines.BaseEntry = int.Parse(Recibo.DocEntry.ToString());
                    GoodIssue.Lines.BaseType = 202;
                    GoodIssue.Lines.BaseLine = Emision[e].LineNum;
                    GoodIssue.Lines.Quantity = double.Parse(Emision[e].CantReq.ToString());
                    GoodIssue.Lines.WarehouseCode = Emision[e].Warehouse;

                    //Recorrido de la Lista de Lotes del Articulo de la OF
                    for (int b = 0; b < Emision[e].Batch.Count; b++)
                    {
                        if (Emision[e].Batch[b].DistNumber.Length > 0)
                        {
                            Logs.FullInfo("", "Lineas: " + e + " \r\n\t BatchNumber => " + Emision[e].Batch[b].DistNumber +
                                    "- \r\n\t Quantity => " + Emision[e].Batch[b].Quantity +
                                    "- \r\n\t WarehouseCode => " + Emision[e].Warehouse
                                    , Usuario, 0, modulo, metodo); //Log Informativo

                            GoodIssue.Lines.BatchNumbers.BaseLineNumber = e;
                            GoodIssue.Lines.BatchNumbers.BatchNumber = Emision[e].Batch[b].DistNumber;
                            GoodIssue.Lines.BatchNumbers.Location = Emision[e].Warehouse;
                            GoodIssue.Lines.BatchNumbers.Quantity = Emision[e].Batch[b].Quantity;

                            GoodIssue.Lines.BatchNumbers.Add();
                        }
                    }
                    //Fin del recorrido de la Lista de Lotes del Articulo de la OF

                    GoodIssue.Lines.Add(); //Agregar linea
                }
                //Fin del recorrido de la Lista de Articulos de OF y realizar la integracion de informacion

                //Save the object as an xml file
                //string rutaLog = ConfigurationManager.AppSettings["RutaFullLog"];
                //string rutaArchivo = $"{rutaLog}\\Emision_{guid}.xml";
                
                //Integrar a SAP la informacion de la Emision
                if (GoodIssue.Add() != 0)
                {
                    ResEmision.Mensaje = DiApiConexion.GetCompany().GetLastErrorCode() + " - " + DiApiConexion.GetCompany().GetLastErrorDescription();
                    //GoodIssue.SaveXML(rutaArchivo);

                    ResEmision.DocEntry = "-1";
                    ResEmision.DocNum = "-1";
                    Logs.FullInfo("", "Error en generación de Emision, " + ResEmision.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo

                    //if (oCompany.InTransaction)
                    //    oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                }
                else
                {
                    MsjRecibo msjRecibo = new MsjRecibo();
                    GoodIssue.GetByKey(int.Parse(DiApiConexion.GetCompany().GetNewObjectKey()));//Objeto Emision
                    int msjEmisionE = GoodIssue.DocEntry;
                    int msjEmisionN = GoodIssue.DocNum;

                    docnumEmision = msjEmisionN;

                    Logs.FullInfo("", "Emision SIN PROBLEMAS :: Documento A CREAR: DocNum = " + msjEmisionN + ", DocEntry = " + msjEmisionE, Usuario, 0, modulo, metodo);
                    //GoodIssue.SaveXML(rutaArchivo);
                    //Creación del Recibo de Produccion de la Emision de la OF
                    //reciboProd : Lista de Lotes de Etiqueta de la OF
                    if (ReciboProduccion(ref msjRecibo, Recibo.DocNum, Recibo.DocEntry, reciboProd, Maquina, CodeGuia, guid))
                    {
                        ResEmision.DocEntry = $" Emisión : { msjEmisionE } - Recibo : { msjRecibo.DocEntry }";
                        ResEmision.DocNum = $" Emisión : { msjEmisionN } - Recibo : { msjRecibo.DocNum }";
                        docnumRecibo = msjRecibo.DocNum;

                        DiApiConexion.GetCompany().EndTransaction(BoWfTransOpt.wf_Commit);

                        string msjUpd = string.Empty;
                        int codeR = 0;
                        SP.EstatusReciboProduccion(ref msjUpd, ref codeR, "L", reciboProd); //Cambiar Estatus del Recibo de Produccion
                        if (codeR == -1)
                        {
                            ResEmision.Mensaje = "Error al actualizar estatus de etiquetas - " + msjUpd;
                        }

                        SP.EliminarBorrador(reciboProd); //Eliminar Etiquetas de Borrador, Esto cuando se ocupa la descarga de etiquetas

                        //Liberar EtqMaster
                        if (regMaster != null && regMaster.Count > 0)
                        {
                            foreach (var reg in regMaster)
                            {
                                if (reg.OrdenFab == Recibo.DocNum)
                                {
                                    SP.LiberarMaster(ref msjUpd, ref codeR, reg.Codigo, reg.Articulo, reg.OrdenFab); //Cambiar Estatus del Recibo de Produccion
                                    if (codeR == -1)
                                    {
                                        ResEmision.Mensaje = ResEmision.Mensaje + " Error al actualizar estatus de Etiqueta Master - " + msjUpd;
                                    }

                                    SP.EliminarMasterBorrador(reg.Codigo); //Eliminar Etiqueta Master de Borrador, Esto cuando se ocupa la descarga de etiquetas
                                }
                            }
                        }

                        salida =  true;
                    }
                    else
                    {
                        ResEmision.DocEntry = "-1";
                        ResEmision.DocNum = "-1";
                        ResEmision.Mensaje = msjRecibo.Mensaje;
                        Logs.FullInfo("", "Error: NO se realizó Integración --- Error en generación de Recibo de Produccion", Usuario, 0, modulo, metodo); //Log Informativo

                        if (oCompany.InTransaction)
                            oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                ResEmision.Mensaje = ex.InnerException == null ? ex.Message : ex.InnerException.ToString();

                Logs.FullInfo("ERROR de Integración OF " + Recibo.DocNum, "", Usuario, 4, "", ""); //Log Informativo

                //if (oCompany.InTransaction)
                //    oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
            }
            finally
            {
                DiApiConexion.LimpiarObjeto(GoodIssue);
                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }

            return salida;
        }

        public bool ReciboProduccion(ref MsjRecibo ResRecibo, int DocNumOF, int DocEntrtOF, List<Produccion.DocumentoAProduccion> Recibo, string Maquina, string CodeGuia, string guid)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            Documents GoodsReceipt = null;
            bool salida = false;

            Logs.FullInfo("Creacion de Recibo de Produccion para OF " + DocNumOF, "", Usuario, 3, "", ""); //Log Informativo            
            try
            {
                string msj = string.Empty;
                if (DiApiConexion.GetCompany().Connected)
                {
                    Logs.FullInfo("", "Encabezado: \r\n\t DocDate => " + DateTime.Now +
                                            "- \r\n\t U_WMS_USERNAME => " + Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() +
                                            "- \r\n\t U_WMS_USERCODE => HH " +
                                            "- \r\n\t U_Time => " + DateTime.Now.ToString("HH:mm:ss") +
                                            "- \r\n\t U_OF => " + DocNumOF +
                                            "- \r\n\t U_Machine => " + Maquina +
                                            "- \r\n\t U_WMS_CodeGuid => " + CodeGuia +

                                            "- \r\n\n  Integracion de Lineas de Recibo Produccion"
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                    Receipt doc = new Receipt();
                    GoodsReceipt = (Documents)DiApiConexion.GetCompany().GetBusinessObject(BoObjectTypes.oInventoryGenEntry);
                    GoodsReceipt.DocObjectCode = BoObjectTypes.oInventoryGenEntry;
                    GoodsReceipt.UserFields.Fields.Item("U_WMS_USERNAME").Value = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                    GoodsReceipt.UserFields.Fields.Item("U_WMS_USERCODE").Value = "HH";
                    GoodsReceipt.UserFields.Fields.Item("U_Time").Value = DateTime.Now.ToString("HH:mm:ss");
                    GoodsReceipt.UserFields.Fields.Item("U_OF").Value = DocNumOF;
                    GoodsReceipt.UserFields.Fields.Item("U_Machine").Value = Maquina;
                    GoodsReceipt.Reference2 = $"OF {DocNumOF}";
                    GoodsReceipt.Comments = $"Recibo de Producción de Orden {DocNumOF}";
                    GoodsReceipt.UserFields.Fields.Item("U_WMS_CodeGuid").Value = CodeGuia;

                    //Recorrido Lista de Lotes de Etiqueta de la OF 
                    for (int r = 0; r < Recibo.Count; r++)
                    {
                        Logs.FullInfo("", "Lineas: " + r + " \r\n\t BaseEntry => " + DocEntrtOF +
                                    "- \r\n\t BaseType => 202" +
                                    "- \r\n\t Quantity => " + Recibo[r].Quantity +
                                    "- \r\n\t WarehouseCode => " + Recibo[r].Warehouse +
                                    "- \r\n\t U_TurnoRecibo => " + Recibo[r].Turno.ToString() +
                                    "- \r\n\t U_FechaRecibo => " + DateTime.Parse(Recibo[r].Fecha) +
                                    "- \r\n\t BatchNumber => " + Recibo[r].Batch 
                                    , Usuario, 0, modulo, metodo); //Log Informativo

                        GoodsReceipt.Lines.BaseType = 202;
                        GoodsReceipt.Lines.BaseEntry = DocEntrtOF;
                        GoodsReceipt.Lines.Quantity = Recibo[r].Quantity;
                        GoodsReceipt.Lines.WarehouseCode = Recibo[r].Warehouse;
                        GoodsReceipt.Lines.UserFields.Fields.Item("U_TurnoRecibo").Value = Recibo[r].Turno.ToString();
                        GoodsReceipt.Lines.UserFields.Fields.Item("U_FechaRecibo").Value = DateTime.Parse(Recibo[r].Fecha);

                        GoodsReceipt.Lines.BatchNumbers.BaseLineNumber = r;
                        GoodsReceipt.Lines.BatchNumbers.BatchNumber = Recibo[r].Batch;
                        GoodsReceipt.Lines.BatchNumbers.Quantity = Recibo[r].Quantity;
                        GoodsReceipt.Lines.BatchNumbers.AddmisionDate = DateTime.Parse(Recibo[r].Fecha);
                        
                        if (Recibo[r].EtqMaster != null) {
                            Logs.FullInfo("", "Reg Master: U_EtqMaster=> " + Recibo[r].EtqMaster.ToString(), Usuario, 0, modulo, metodo); //Log Informativo

                            GoodsReceipt.Lines.BatchNumbers.UserFields.Fields.Item("U_EtqMaster").Value = Recibo[r].EtqMaster.ToString();
                        }

                        //Veriicar e Integrar la Ubicación del Lote
                        if (Recibo[r].AbsEntry > 0)
                        {
                            Logs.FullInfo("", "Lineas: " + r + " \r\n\t Ubicacion => " + Recibo[r].AbsEntry +
                                    "- \r\n\t Quantity => " + Recibo[r].Quantity 
                                    , Usuario, 0, modulo, metodo); //Log Informativo

                            GoodsReceipt.Lines.BinAllocations.BinAbsEntry = Recibo[r].AbsEntry;
                            GoodsReceipt.Lines.BinAllocations.Quantity = Recibo[r].Quantity;
                            GoodsReceipt.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = 0;
                            GoodsReceipt.Lines.BinAllocations.BaseLineNumber = r;
                            GoodsReceipt.Lines.BinAllocations.Add();
                        }

                        GoodsReceipt.Lines.BatchNumbers.Add();
                        GoodsReceipt.Lines.Add();                        
                    }

                    //Save the object as an xml file
                    string rutaLog = ConfigurationManager.AppSettings["RutaFullLog"];
                    string rutaArchivo = $"{rutaLog}\\Recibo_{guid}.xml";

                    if (GoodsReceipt.Add() != 0)
                    {
                        ResRecibo.Mensaje = DiApiConexion.GetCompany().GetLastErrorCode() + " - " + DiApiConexion.GetCompany().GetLastErrorDescription();
                        Logs.FullInfo("", "Error en generación de Recibo de Producción ", Usuario, 0, modulo, metodo); 
                        Logs.FullInfo("Creacion de Recibo de Produccion para OF " + DocNumOF, "", Usuario, 4, "", "");
                        GoodsReceipt.SaveXML(rutaArchivo);
                        //return false;
                    }
                    else
                    {
                        GoodsReceipt.GetByKey(int.Parse(DiApiConexion.GetCompany().GetNewObjectKey()));
                        ResRecibo.DocEntry = DiApiConexion.GetCompany().GetNewObjectKey();
                        ResRecibo.DocNum = GoodsReceipt.DocNum.ToString();
                        Logs.FullInfo("", "Recibo de Produccion SIN PROBLEMAS :: Documento A CREAR: DocNum " + ResRecibo.DocNum + " --- DocEntry " + ResRecibo.DocEntry, Usuario, 0, modulo, metodo); //Log Informativo
                        salida = true;
                        GoodsReceipt.SaveXML(rutaArchivo);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Conexion a SAP CERRADA, " + msj, Usuario, 0, modulo, metodo);
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                ResRecibo.Mensaje = ex.InnerException == null ? ex.Message : ex.InnerException.ToString();

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo);
            }
            finally
            {
                DiApiConexion.LimpiarObjeto(GoodsReceipt);
                Logs.FullInfo("Creacion de Recibo de Produccion para OF " + DocNumOF, "", Usuario, 4, "", "");
            }

            return salida;
        }

        public bool TraspasoCalidad(Company _oCompany, ref Mensajes result, string g, List<DetalleR> lista, string Estatus, string Destino, string Origen) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            string msj = string.Empty;
            StockTransfer oTransfer = null;

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Creación de Traspaso", Usuario, 0, modulo, metodo); //Log Informativo

            try
            {
                Logs.FullInfo("", "Encabezado: \r\n\t DocDate => " + DateTime.Now +
                                            "- \r\n\t From WarehouseCode => " + Origen +
                                            "- \r\n\t To Warehouse => " + Destino +
                                            "- \r\n\t U_WMS_USERNAME => " + Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() +
                                            "- \r\n\t U_WMS_USERCODE => HH " +
                                            "- \r\n\t U_Time => " + DateTime.Now.ToString("HH:mm:ss") +

                                            "- \r\n\n  Integracion de Lineas del Traspaso"
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                oTransfer = (StockTransfer)_oCompany.GetBusinessObject(BoObjectTypes.oStockTransfer);
                oTransfer.DocDate = DateTime.Now;
                oTransfer.FromWarehouse = Origen;
                oTransfer.ToWarehouse = Destino;
                oTransfer.UserFields.Fields.Item("U_WMS_USERNAME").Value = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                oTransfer.UserFields.Fields.Item("U_WMS_USERCODE").Value = "HH";
                oTransfer.UserFields.Fields.Item("U_Time").Value = DateTime.Now.ToString("HH:mm:ss");

                Logs.FullInfo("", "***Encabezado : Datos OK ************\r\n", Usuario, 0, modulo, metodo); //Log Informativo

                foreach (Recuperar.Line l in Ob.Lineas(g, "D"))
                {
                    Logs.FullInfo("********Datos de Integracion Linea " + l.LineNum + "********", "", Usuario, 3, "", ""); //Log Informativo
                    Logs.FullInfo("", "\t Traspaso Linea : " + l.LineNum +
                                                        "\r\n\t NumLine => " + l.LineNum +
                                                        "\r\n\t ItemCode => " + l.ItemCode +
                                                        "\r\n\t Quantity => " + l.Qty +
                                                        "\r\n\t FromWarehouseCode => " + l.Desde +
                                                        "\r\n\t WarehouseCode => " + l.Hasta 
                                        , Usuario, 0, modulo, metodo); //Log Informativo

                    oTransfer.Lines.ItemCode = l.ItemCode;
                    oTransfer.Lines.Quantity = (double)l.Qty;
                    oTransfer.Lines.FromWarehouseCode = l.Desde;
                    oTransfer.Lines.WarehouseCode = l.Hasta;//Dts.LineasSO[0].ToWhsCode;

                    Logs.FullInfo("", "***Lineas Traspaso : Datos OK ******\r\n", Usuario, 0, modulo, metodo); //Log Informativo

                    foreach (Recuperar.Batch b in Ob.LotesLineas(g, l.ItemCode, l.LineNum, 0, "", "D", 0))
                    {
                        Logs.FullInfo("\t >>>>>> Datos de Integracion del Lote " + b.Lote + " >>>>>>", "", Usuario, 3, "", ""); //Log Informativo

                        if (!string.IsNullOrEmpty(b.Lote))
                        {
                            Logs.FullInfo("", "\t INTEGRACION de Lote para la Linea: " + l.LineNum + " con Articulo: " + l.ItemCode +
                                                                    "\r\n\t BaseLineNumber => " + l.LineNum +
                                                                    "\r\n\t BatchNumber => " + b.Lote +
                                                                    "\r\n\t Location => " + l.Desde +
                                                                    "\r\n\t Quantity => " + b.QtyLote
                                                                    , Usuario, 0, modulo, metodo); //Log Informativo

                            oTransfer.Lines.BatchNumbers.BaseLineNumber = l.LineNum;
                            oTransfer.Lines.BatchNumbers.BatchNumber = b.Lote;
                            oTransfer.Lines.BatchNumbers.Location = l.Desde;
                            oTransfer.Lines.BatchNumbers.Quantity = b.QtyLote;

                            oTransfer.Lines.BatchNumbers.Add();

                            Logs.FullInfo("", "***Lotes para la Linea: " + l.LineNum + " con Articulo: " + l.ItemCode + " ::: Datos OK *******\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                        }
                        
                        foreach (Recuperar.BinLocation bl in Ob.Ubicaciones(g, l.ItemCode, l.LineNum, b.LineNumLote, b.Lote, 0, "", "D", 0))
                        {
                            if (bl.UbOrigen > 0)
                            {
                                Logs.FullInfo("", "INTEGRACION Ubicacion Origen para el Lote " + b.Lote +
                                                                        "\r\n\t BinAbsEntry => " + bl.UbOrigen +
                                                                        "\r\n\t Quantity => " + bl.QtyUbOrigen +
                                                                        "\r\n\t SerialAndBatchNumbersBaseLine => " + b.LineNumLote +
                                                                        "\r\n\t BinActionType => Salida " +
                                                                        "\r\n\t BaseLineNumber => " + l.LineNum
                                                                        , Usuario, 0, modulo, metodo); //Log Informativo

                                oTransfer.Lines.BinAllocations.BinAbsEntry = bl.UbOrigen;
                                oTransfer.Lines.BinAllocations.Quantity = bl.QtyUbOrigen;
                                oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = b.LineNumLote;
                                oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batFromWarehouse;
                                oTransfer.Lines.BinAllocations.BaseLineNumber = l.LineNum;
                                oTransfer.Lines.BinAllocations.Add();

                                Logs.FullInfo("", "***Ubicacion Origen del Lote " + b.Lote + " ::: Datos OK *******\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                            }
                        }
                        
                        foreach (Recuperar.BinLocation bld in Ob.UbicacionesDest(g, l.ItemCode, l.LineNum, b.LineNumLote, b.Lote, 0, "", "D", 0))
                        {
                            if (bld.UbOrigen > 0)
                            {
                                Logs.FullInfo("", "INTEGRACION Ubicacion Destino para el Lote " + b.Lote +
                                                                            "\r\n\t BinAbsEntry => " + bld.UbDestino +
                                                                            "\r\n\t Quantity => " + bld.QtyUbDestino +
                                                                            "\r\n\t SerialAndBatchNumbersBaseLine => " + b.LineNumLote +
                                                                            "\r\n\t BinActionType => Entrada" +
                                                                            "\r\n\t BaseLineNumber => " + l.LineNum
                                                                            , Usuario, 0, modulo, metodo); //Log Informativo

                                oTransfer.Lines.BinAllocations.BinAbsEntry = bld.UbDestino; //destino
                                oTransfer.Lines.BinAllocations.Quantity = bld.QtyUbDestino;
                                oTransfer.Lines.BinAllocations.SerialAndBatchNumbersBaseLine = b.LineNumLote;
                                oTransfer.Lines.BinAllocations.BinActionType = BinActionTypeEnum.batToWarehouse;
                                oTransfer.Lines.BinAllocations.BaseLineNumber = l.LineNum;
                                oTransfer.Lines.BinAllocations.Add();

                                Logs.FullInfo("", "***Ubicacion Destino del Lote " + b.Lote + " ::: Datos OK ******\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                            }
                        }

                        Logs.FullInfo("\t <<<<<< Datos de Integracion del Lote " + b.Lote + " <<<<<<< \r\n", "", Usuario, 4, "", ""); //Log Informativo
                    }
                    oTransfer.Lines.Add();
                    Logs.FullInfo("", "***Linea de Traspaso Agregada : Datos OK **********\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                    Logs.FullInfo("Datos de Integracion Linea " + l.LineNum + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                }

                if (oTransfer.Add() != 0)
                {
                    result.Mensaje = $"{_oCompany.GetLastErrorCode()}-{_oCompany.GetLastErrorDescription()}";
                    Logs.FullInfo("", "\r\n Error: NO se realizó Integración, No se Creó Traspaso", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }
                else
                {
                    oTransfer.GetByKey(int.Parse(_oCompany.GetNewObjectKey()));
                    result.DocEntry = oTransfer.DocEntry;
                    result.DocNum = oTransfer.DocNum;
                    result.Mensaje = string.Empty;

                    Logs.FullInfo("", "Integración Realizada: Traspaso Realizado \r\n DocNum => " + oTransfer.DocNum +
                                    "\r\n DocEntry => " + oTransfer.DocEntry +
                                    " \r\n**Integracion OK *************************************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                result.Mensaje = ex.Message;

                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (oTransfer != null)
                {
                    Marshal.ReleaseComObject(oTransfer);
                    oTransfer = null;

                    Logs.FullInfo("", "LIBERACION DE OBJETO SAP: TRASPASO", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Recuento de Inventario
        public bool CrearArchivo(ref MsjArchivo msj, string Whs, int Rec, List<LtsRecuento> Lineas) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            string UsrId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            string PathTxt = ConfigurationManager.AppSettings["RutaTxt"];
            string texto = string.Empty;

            Logs.FullInfo("Procesamiento", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Creación de Archivo de Recuento de Inventario", Usuario, 0, modulo, metodo); //Log Informativo
            Logs.FullInfo("", "Ruta de Archivo: " + PathTxt, Usuario, 0, modulo, metodo); //Log Informativo

            try
            {
                switch (PathTxt)
                {
                    case null:
                    case "":
                        msj.Codigo = -1;
                        msj.Mensaje = "No ha configurado la ruta para el archivo de recuento";

                        Logs.FullInfo("", "Error: No ha configurado la ruta para el archivo de recuento", Usuario, 0, modulo, metodo); //Log Informativo
                        return false;
                    default:
                        if (!Directory.Exists(Path.Combine(PathTxt)))
                        {
                            Directory.CreateDirectory(@"C:\TxtRecuento");
                            Logs.FullInfo("", "Se crea Directorio " + PathTxt, Usuario, 0, modulo, metodo); //Log Informativo
                        }

                        if (!Directory.Exists(Path.Combine(PathTxt, Whs)))
                        {
                            Directory.CreateDirectory(Path.Combine(PathTxt, Whs));
                            Logs.FullInfo("", "Se crea Directorio " + Whs + " en " + PathTxt, Usuario, 0, modulo, metodo); //Log Informativo
                        }


                        if (!Directory.Exists(Path.Combine(PathTxt, Whs, UsrId)))
                        {
                            Directory.CreateDirectory(Path.Combine(PathTxt, Whs, UsrId));
                            Logs.FullInfo("", "Se crea Directorio " + UsrId + " en " + Whs + " en " + PathTxt, Usuario, 0, modulo, metodo); //Log Informativo
                        }                            

                        string Pat = Path.Combine(PathTxt, Whs, UsrId, Whs + "_" + UsrId + "_R" + Rec + DateTime.Now.ToString("dd") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("yyyy") + ".txt");
                        Logs.FullInfo("", "Archivo a Crear:  " + Pat, Usuario, 0, modulo, metodo); //Log Informativo

                        if (File.Exists(Pat))
                        {
                            File.Delete(Pat);
                            Logs.FullInfo("", "El archivo Existe, se Elimina archivo para crear el Nuevo", Usuario, 0, modulo, metodo); //Log Informativo

                            foreach (LtsRecuento ln in Lineas)
                            {
                                string l = ln.DistNumber + "\t" + ln.Ctd_Ctda + "\t" + ln.BinCode + "\t" + ln.ItemCode + "\t" + ln.WhsCode + "\t" + ln.ItemName + "\r\n";
                                texto = texto + l;

                                Logs.FullInfo("", "Etiqueta procesada: " + ln.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
                            }
                            File.WriteAllText(Pat, texto);
                            msj.Codigo = 0;
                            msj.Mensaje = "Archivo creado correctamente";

                            Logs.FullInfo("", "Etiqueta procesadas, " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                            return true;
                        }
                        else
                        {
                            foreach (LtsRecuento ln in Lineas)
                            {
                                string l = ln.DistNumber + "\t" + ln.Ctd_Ctda + "\t" + ln.BinCode + "\t" + ln.ItemCode + "\t" + ln.WhsCode + "\t" + ln.ItemName + "\r\n";
                                texto = texto + l;

                                Logs.FullInfo("", "Etiqueta procesada: " + ln.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
                            }
                            File.WriteAllText(Pat, texto);
                            msj.Codigo = 0;
                            msj.Mensaje = "Archivo creado correctamente";

                            Logs.FullInfo("", "Etiqueta procesadas, " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                            return true;
                        }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msj.Codigo = -1;
                msj.Mensaje = ex.Message;

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                Logs.FullInfo("Procesamiento", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Crear Archivo TXT de Traspaleo -- Actualizacion 14-01-2021
        public bool CrearArchivoTraspaleo(List<Traspaleo> Lineas, ref MsjArchivo msj, string Etqs)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            string UsrId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            string PathTxt = ConfigurationManager.AppSettings["RutaTraspaleo"];
            string texto = string.Empty;

            Logs.FullInfo("I n i c i o   P r o c e s a m i e n t o   d e   E t i q u e t a s    T r a s p a l e o", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Creación de Archivo de Traspaleo", Usuario, 0, modulo, metodo); //Log Informativo
            Logs.FullInfo("", "Ruta de Archivo: " + PathTxt, Usuario, 0, modulo, metodo); //Log Informativo

            try
            {
                switch (PathTxt)
                {
                    case null:
                    case "":
                        msj.Codigo = -1;
                        msj.Mensaje = "No ha configurado la ruta para el archivo de Traspaleo";

                        Logs.FullInfo("", "Error: No ha configurado la ruta para el archivo de Traspaleo", Usuario, 0, modulo, metodo); //Log Informativo
                        return false;
                    default:
                        if (!Directory.Exists(Path.Combine(PathTxt)))
                        {
                            Directory.CreateDirectory(@"C:\Traspaleos");
                            Logs.FullInfo("", "Se crea Directorio " + PathTxt, Usuario, 0, modulo, metodo); //Log Informativo
                        }

                        if (!Directory.Exists(Path.Combine(PathTxt, UsrId)))
                        {
                            Directory.CreateDirectory(Path.Combine(PathTxt, UsrId));
                            Logs.FullInfo("", "Se crea Carpeta de Usuario: " + UsrId + " en Directorio: " + PathTxt, Usuario, 0, modulo, metodo); //Log Informativo
                        }

                        //Recorrido de los Numero de Parte de las Etiquetas Leida
                        foreach (Recuperar.Partes Parte in Ob.NumPartes(Etqs))
                        {
                            //Por cada Número de Parte se Creara un Archivo con las Etiquetas que correspondam
                            //Se verifica si ya existe un archivo igual, de ser así se creara un consecutivo
                            string Pat = Path.Combine(PathTxt, UsrId, DateTime.Now.ToString("dd") + "-" + DateTime.Now.ToString("MM") + "-" + DateTime.Now.ToString("yyyy") + "_" + UsrId + "_" + Parte.NoParte + ".txt");
                            string PatFile = getNextFileName(Pat);
                            Logs.FullInfo("", "Archivo a Crear:  " + PatFile, Usuario, 0, modulo, metodo); //Log Informativo

                            texto = string.Empty;
                            //Recorrido de las Etiquetas Leidas
                            foreach (Traspaleo ln in Lineas)
                            {
                                if (ln.U_ItemCode == Parte.NoParte)
                                {
                                    string l = ln.Name + "\r\n";
                                    texto = texto + l;

                                    Logs.FullInfo("", "Etiqueta procesada: " + ln.Name, Usuario, 0, modulo, metodo); //Log Informativo
                                }                                
                            }

                            File.WriteAllText(PatFile, texto);
                            msj.Codigo = 0;

                            if (msj.Mensaje == null)
                            {
                                msj.Mensaje = "Archivo(s) creado(s) correctamente: \r\n" + PatFile + "\r\n";
                            }
                            else
                            {
                                msj.Mensaje = msj.Mensaje + "" + PatFile + "\r\n";
                            }  
                        }

                        Logs.FullInfo("", "Etiquetas procesadas, " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                        return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msj.Codigo = -1;
                msj.Mensaje = ex.Message;

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                Logs.FullInfo("F i n   P r o c e s a m i e n t o   d e   E t i q u e t a s    T r a s p a l e o", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Obtener el archivo consecutivo en carpeta -- Modulo Traspaleo
        private string getNextFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            int i = 0;
            while (File.Exists(fileName))
            {
                if (i == 0)
                    //fileName = fileName.Replace(extension, "(" + ++i + ")" + extension);
                    fileName = fileName.Replace(extension, "_" + ++i + extension);
                else
                    //fileName = fileName.Replace("(" + i + ")" + extension, "(" + ++i + ")" + extension);
                    fileName = fileName.Replace("_" + i + extension, "_" + ++i + extension);
            }

            return fileName;
        }

        /******************************************************************************/
        //Scrap
        /******************************************************************************/
        public bool RegistroScrap(ref Mensajes Resp, string g)
        {
            GeneralService oGeneralService = null;
            GeneralData oGeneralData = null;
            //GeneralDataParams oGeneralParams = null;            
            GeneralData oSon = null;
            GeneralDataCollection oSons = null;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Registro de Scrap", Usuario, 0, modulo, metodo); //Log Informativo
            try
            {
                string msj = string.Empty;
                oCompany = sboConect(ref msj);
                if (oCompany != null && msj.Length == 0)
                {
                    //Inicio de la Transaccion
                    if (oCompany.InTransaction)
                    {
                        oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                    }
                    else 
                    {
                        oCompany.StartTransaction();
                    }

                    sCompany = oCompany.GetCompanyService();
                    Logs.FullInfo("", "Obtener información de Registros", Usuario, 0, modulo, metodo); //Log Informativo
                    List<Scrap.Detalle> RegScrap = new List<Scrap.Detalle>();
                    RegScrap = Ob.ScrapLineas(g);

                    Logs.FullInfo("", "Encabezado: \r\n\t U_stLib => O" +
                                            "- \r\n\t U_LibDate => " + RegScrap[0].Fecha +
                                            "- \r\n\t U_OFEntry => " + RegScrap[0].DocEntry +
                                            "- \r\n\t U_OFNum => " + RegScrap[0].DocNum +
                                            "- \r\n\t U_ItemCode => " + RegScrap[0].ItemCode +
                                            "- \r\n\t U_ItemName => " + RegScrap[0].ItemName +
                                            "- \r\n\t U_WhsCode => " + RegScrap[0].Almacen +
                                            "- \r\n\t U_Turno => " + RegScrap[0].Turno +
                                            "- \r\n\t U_Maquina => " + RegScrap[0].Maquina +
                                            "- \r\n\t U_Operador => " + RegScrap[0].IdOperador +
                                            "- \r\n\t U_OperName => " + RegScrap[0].Operador +
                                            "- \r\n\t U_QtyNoOk => " + RegScrap[0].Total +
                                            "- \r\n\t U_TotalScrap => " + RegScrap[0].Total +

                                            "- \r\n\n  Integracion de Lineas de Scrap"
                                            , Usuario, 0, modulo, metodo); //Log Informativo


                    oGeneralService = sCompany.GetGeneralService("LIBPT");
                    oGeneralData = ((GeneralData)(oGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData))); 
                    oGeneralData.SetProperty("U_LibDate", RegScrap[0].Fecha);
                    oGeneralData.SetProperty("U_stLib", "O");
                    oGeneralData.SetProperty("U_OFEntry", RegScrap[0].DocEntry);
                    oGeneralData.SetProperty("U_OFNum", RegScrap[0].DocNum);
                    oGeneralData.SetProperty("U_ItemCode", RegScrap[0].ItemCode);
                    oGeneralData.SetProperty("U_ItemName", RegScrap[0].ItemName);
                    oGeneralData.SetProperty("U_WhsCode", RegScrap[0].Almacen);
                    oGeneralData.SetProperty("U_Turno", RegScrap[0].Turno.ToString());
                    oGeneralData.SetProperty("U_Maquina", RegScrap[0].Maquina);
                    oGeneralData.SetProperty("U_Operador", RegScrap[0].IdOperador.ToString());
                    oGeneralData.SetProperty("U_OperName", RegScrap[0].Operador);
                    //oGeneralData.SetProperty("U_QtyNoOk", Int32.Parse(RegScrap[0].Total.ToString()));
                    //oGeneralData.SetProperty("U_TotalScrap", RegScrap[0].Total);

                    Logs.FullInfo("", "***Encabezado : Datos OK ********* \r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    int libNum = 1;
                    foreach (Scrap.Detalle l in RegScrap)
                    {
                        Logs.FullInfo("********Datos de Integracion Linea " + l.LineNum + "********", "", Usuario, 3, "", ""); //Log Informativo
                        Logs.FullInfo("",   "\t Scrap Linea : " + l.LineNum +
                                            "\r\n\t U_Tipo => S" +
                                            "\r\n\t U_IdScrap => " + l.TipoScrap +
                                            "\r\n\t U_ScrapDesc => " + l.DescScrap +
                                            "\r\n\t U_QtyScrap => " + l.Cantidad +
                                            "\r\n\t U_Validado => P" + 
                                            "\r\n\t Usuario => " + Usuario  
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                        oSons = oGeneralData.Child("EVT_LIBPT2");
                        oSon = oSons.Add();

                        oSon.SetProperty("U_Tipo", "S");
                        oSon.SetProperty("U_IdScrap", l.TipoScrap);
                        oSon.SetProperty("U_ScrapDesc", l.DescScrap);
                        oSon.SetProperty("U_QtyScrap", l.Cantidad);                        
                        oSon.SetProperty("U_Validado", "P");                  
                        
                        Logs.FullInfo("", "***Linea de Scrap Agregada : Datos OK **************\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                        Logs.FullInfo("Datos de Integracion Linea " + l.LineNum + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                        libNum++;
                    }

                    oGeneralService.Add(oGeneralData);
                    //Integrar datos
                    oCompany.EndTransaction(BoWfTransOpt.wf_Commit);
                    Logs.FullInfo("", "Integración Realizada: Scrap Registrado \r\n**Integracion OK *******************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return true;
                }
                else
                {
                    Resp.Mensaje = msj;
                    Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj + "\r\n\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                //Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                Resp.Mensaje = ex.Message;

                if (oCompany.InTransaction) {
                    oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                }                    
                return false;
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;

                    //Logs.FullInfo("", "Conexion a SAP Cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("", "Conexion a SAP Cerrada" + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool EliminarRegistroScrap(ref Mensajes Resp, string g)
        {
            GeneralService oGeneralService = null;
            GeneralData oGeneralData = null;
            GeneralDataParams oGeneralParams = null;
            //GeneralData oSon = null;
            GeneralDataCollection oSons = null;
            List<Scrap.DetalleRegistros> RegScrap = new List<Scrap.DetalleRegistros>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Eliminar Registro de Scrap", Usuario, 0, modulo, metodo); //Log Informativo
            try
            {
                string msj = string.Empty;
                oCompany = sboConect(ref msj);
                if (oCompany != null && msj.Length == 0)
                {   
                    RegScrap = Ob.DelScrapLineas(g);
                    sCompany = oCompany.GetCompanyService();
                    oGeneralService = sCompany.GetGeneralService("LIBPT");
                    oGeneralParams = ((GeneralDataParams)(oGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams)));
                    
                    Logs.FullInfo("", "***Encabezado : Datos OK ********* \r\n", Usuario, 0, modulo, metodo); //Log Informativo                    
                    foreach (Scrap.DetalleRegistros l in RegScrap)
                    {
                        Logs.FullInfo("********Datos de Integracion DocEntry " + l.SDocEntry + " - Linea " + l.VisOrder + "********", "", Usuario, 3, "", ""); //Log Informativo
                        Logs.FullInfo("", "\t Scrap Linea[VisOrder] : " + l.VisOrder +
                                            "\r\n\t DocEntry => " + l.SDocEntry +
                                            "\r\n\t Usuario => " + Usuario
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                        int Linea = Ob.LineaScrap(Int32.Parse(l.SDocEntry), l.LineId);

                        oGeneralParams.SetProperty("DocEntry", l.SDocEntry);
                        oGeneralData = oGeneralService.GetByParams(oGeneralParams);

                        oSons = oGeneralData.Child("EVT_LIBPT2");
                        oSons.Remove(Linea);
                        oGeneralService.Update(oGeneralData);

                        Logs.FullInfo("", "***Linea de Scrap Eliminadas : OK **************\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                        Logs.FullInfo("Datos de Eliminación Linea " + l.VisOrder + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                    }
                    
                    Logs.FullInfo("", "Integración [Eliminación] Realizada: Scrap Eliminado \r\n**Integracion OK *******************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return true;
                }
                else
                {
                    Resp.Mensaje = msj;
                    Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj + "\r\n\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                Resp.Mensaje = ex.Message;
                return false;
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;

                    Logs.FullInfo("", "Conexion a SAP Cerrada" + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        /******************************************************************************/
        //TPM
        /******************************************************************************/
        public bool RegistroTPM(ref Mensajes Resp, string g)
        {
            GeneralService oGeneralService = null;
            GeneralData oGeneralData = null;
            GeneralDataParams oGeneralParams = null;            
            GeneralData oSon = null;
            GeneralDataCollection oSons = null;
            List<TPM.Detalle> RegTPMActualizar;
            List<TPM.Detalle> RegTPMIngresar;
            //List<TPM.Detalle> RegTPMCorreo;
            //List<TPM.Detalle> RegTPM1;
            //List<TPM.Detalle> RegTPM2;
            //List<TPM.Detalle> RegTPM3;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Registro de TPM", Usuario, 0, modulo, metodo); //Log Informativo
            try
            {
                Logs.FullInfo("", "Obtener información de Registros", Usuario, 0, modulo, metodo); //Log Informativo
                RegTPMActualizar = new List<TPM.Detalle>();
                RegTPMActualizar = Ob.TPMLineas(g, 1);

                RegTPMIngresar = new List<TPM.Detalle>();
                RegTPMIngresar = Ob.TPMLineas(g, 2);

                string msj = string.Empty;
                oCompany = sboConect(ref msj);
                if (oCompany != null && msj.Length == 0)
                {
                    sCompany = oCompany.GetCompanyService();

                    //Agregar registros
                    if (RegTPMIngresar.Count > 0) {
                        
                        //Inicio de la Transaccion
                        if (oCompany.InTransaction)
                        {
                            oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }
                        else
                        {
                            oCompany.StartTransaction();
                        }

                        Logs.FullInfo("", "Encabezado: \r\n\t" +
                                                "- \r\n\t U_TPMDate => " + RegTPMIngresar[0].Fecha +
                                                "- \r\n\t U_Turno => " + RegTPMIngresar[0].Turno +
                                                "- \r\n\t U_Maquina => " + RegTPMIngresar[0].Maquina +
                                                "- \r\n\t U_Operador => " + RegTPMIngresar[0].IdOperador +
                                                "- \r\n\t U_OperName => " + RegTPMIngresar[0].Operador +
                                                //"- \r\n\t U_TotalTPM => " + RegTPMIngresar[0].Total +
                                                "- \r\n\t U_OFNum => " + RegTPMIngresar[0].DocNum +
                                                "- \r\n\t U_OFEntry => " + RegTPMIngresar[0].DocEntry +

                                                "- \r\n\n  Integracion de Lineas de TPM"
                                                , Usuario, 0, modulo, metodo); //Log Informativo


                        oGeneralService = sCompany.GetGeneralService("OTPM");
                        oGeneralData = ((GeneralData)(oGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData)));
                        oGeneralData.SetProperty("U_TPMDate", RegTPMIngresar[0].Fecha);
                        oGeneralData.SetProperty("U_Turno", RegTPMIngresar[0].Turno.ToString());
                        oGeneralData.SetProperty("U_Maquina", RegTPMIngresar[0].Maquina);
                        oGeneralData.SetProperty("U_Operador", RegTPMIngresar[0].IdOperador.ToString());
                        oGeneralData.SetProperty("U_OperName", RegTPMIngresar[0].Operador);
                        //oGeneralData.SetProperty("U_TotalTPM", RegTPMIngresar[0].Total);    
                        oGeneralData.SetProperty("U_OFNum", RegTPMIngresar[0].DocNum);
                        oGeneralData.SetProperty("U_OFEntry", RegTPMIngresar[0].DocEntry);

                        Logs.FullInfo("", "***Encabezado : Datos OK ********* \r\n", Usuario, 0, modulo, metodo); //Log Informativo
                        int libNum = 1;
                        foreach (TPM.Detalle l in RegTPMIngresar)
                        {
                            Logs.FullInfo("********Datos de Integracion Linea " + l.LineNum + "********", "", Usuario, 3, "", ""); //Log Informativo
                            Logs.FullInfo("", "\t TPM Linea : " + l.LineNum +
                                                "\r\n\t U_Categoria => MAQ | " + l.Categoria +
                                                "\r\n\t U_TPMCode => " + l.TipoTPM +
                                                "\r\n\t U_TPMName => " + l.DescTPM +
                                                "\r\n\t U_Tiempo => " + l.Tiempo +
                                                "\r\n\t U_Status => P" +
                                                "\r\n\t U_Estimado => " + l.Tiempo +
                                                "\r\n\t U_Programado => Y" +
                                                "\r\n\t Usuario => " + Usuario 
                                                //"\r\n\t U_AutoDate => " + DateTime.Now
                                                , Usuario, 0, modulo, metodo); //Log Informativo

                            oSons = oGeneralData.Child("EVT_OTPM1");
                            oSon = oSons.Add();

                            oSon.SetProperty("U_Categoria", "MAQ"); // l.Categoria
                            oSon.SetProperty("U_TPMCode", l.TipoTPM);
                            oSon.SetProperty("U_TPMName", l.DescTPM);
                            oSon.SetProperty("U_Tiempo", l.Tiempo.ToString());
                            oSon.SetProperty("U_Status", "P");
                            oSon.SetProperty("U_Estimado", l.Tiempo.ToString());
                            oSon.SetProperty("U_Programado", "Y");
                            //oSon.SetProperty("U_AutoDate", DateTime.Now);

                            Logs.FullInfo("", "***Linea de TPM Agregada : Datos OK **************\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                            Logs.FullInfo("Datos de Integracion Linea " + l.LineNum + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                            libNum++;
                        }

                        oGeneralService.Add(oGeneralData);
                        //Integrar datos
                        oCompany.EndTransaction(BoWfTransOpt.wf_Commit);
                        Logs.FullInfo("", "Integración Realizada: TPM Registrado \r\n**Integracion OK *******************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    //Registros a agregar por edición de OF
                    if (RegTPMActualizar.Count > 0)
                    {
                        oGeneralService = sCompany.GetGeneralService("OTPM");
                        oGeneralParams = ((GeneralDataParams)(oGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams)));
                        int libNum = 1;
                        foreach (TPM.Detalle l in RegTPMActualizar)
                        {
                            Logs.FullInfo("********Datos de Integracion DocEntry " + l.SDocEntry + "********", "", Usuario, 3, "", ""); //Log Informativo
                            Logs.FullInfo("", "\t TPM : " +
                                                "\r\n\t DocEntry => " + l.SDocEntry +
                                                "\r\n\t Usuario => " + Usuario
                                                , Usuario, 0, modulo, metodo); //Log Informativo

                            oGeneralParams.SetProperty("DocEntry", l.SDocEntry);
                            oGeneralData = oGeneralService.GetByParams(oGeneralParams);

                            oSons = oGeneralData.Child("EVT_OTPM1");
                            oSon = oSons.Add();

                            oSon.SetProperty("U_Categoria", "MAQ"); //l.Categoria
                            oSon.SetProperty("U_TPMCode", l.TipoTPM);
                            oSon.SetProperty("U_TPMName", l.DescTPM);
                            oSon.SetProperty("U_Tiempo", l.Tiempo);
                            oSon.SetProperty("U_Status", "P");
                            oSon.SetProperty("U_Estimado", l.Tiempo);
                            oSon.SetProperty("U_Programado", "Y");

                            oGeneralService.Update(oGeneralData);
                            Logs.FullInfo("", "***Linea de TPM Agregada a DocEntry " + l.SDocEntry + ": Datos OK **************\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                            Logs.FullInfo("Datos de Integracion Linea " + l.LineNum + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                            libNum++;
                        }

                        Logs.FullInfo("", "Integración Realizada: TPM Registrado \r\n**Integracion OK *******************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    //***************************************************************************
                    //***************************************************************************
                    /*
                    //Envio de correo
                    //RegTPMCorreo = new List<TPM.Detalle>();
                    //RegTPMCorreo = Ob.TPMLineas(g, 3);

                    string Correo = string.Empty;
                    string Mensaje = string.Empty;
                    foreach (TPM.Detalle l in RegTPMCorreo)
                    {
                        Logs.FullInfo("********Datos para notificación ********", "", Usuario, 3, "", ""); //Log Informativo
                        if (Correo == "")
                        {
                            Correo = l.Correo;

                            Mensaje = "Se asignó tiempo muerto: ";
                            Mensaje = Mensaje + "\n" + l.DescTPM;
                        }
                        else if (Correo == l.Correo)
                        {
                            Mensaje = Mensaje + "\n" + l.DescTPM;
                        }
                        else
                        {
                            Logs.FullInfo("", "\t Envio de Correo para : " +
                                            "\r\n\t Usuario => " + Correo +
                                            "\r\n\t Mensaje => " + Mensaje
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                            Correo = string.Empty;
                            Mensaje = string.Empty;
                            Correo = l.Correo;

                            Mensaje = "Se asignó tiempo muerto: ";
                            Mensaje = Mensaje + "\n" + l.DescTPM;
                        }
                    }

                    Logs.FullInfo("", "\t Fin registros - Envio de Correo para : " +
                                            "\r\n\t Usuario => " + Correo +
                                            "\r\n\t Mensaje => " + Mensaje
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                    Correo = string.Empty;
                    Mensaje = string.Empty;
                    */
                    //***************************************************************************
                    //***************************************************************************

                    return true;
                }
                else
                {
                    Resp.Mensaje = msj;
                    Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj + "\r\n\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                Resp.Mensaje = ex.Message;

                if (oCompany.InTransaction)
                {
                    oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                }
                return false;
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;

                    Logs.FullInfo("", "Conexion a SAP Cerrada" + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool EliminarRegistroTPM(ref Mensajes Resp, string g)
        {
            GeneralService oGeneralService = null;
            GeneralData oGeneralData = null;
            GeneralDataParams oGeneralParams = null;
            //GeneralData oSon = null;
            GeneralDataCollection oSons = null;
            List<TPM.DetalleRegistros> RegTPM = new List<TPM.DetalleRegistros>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Eliminar Registro de TPM", Usuario, 0, modulo, metodo); //Log Informativo
            try
            {
                string msj = string.Empty;
                oCompany = sboConect(ref msj);
                if (oCompany != null && msj.Length == 0)
                {
                    RegTPM = Ob.DelTPMLineas(g);
                    sCompany = oCompany.GetCompanyService();
                    oGeneralService = sCompany.GetGeneralService("OTPM");
                    oGeneralParams = ((GeneralDataParams)(oGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams)));

                    Logs.FullInfo("", "***Encabezado : Datos OK ********* \r\n", Usuario, 0, modulo, metodo); //Log Informativo                    
                    foreach (TPM.DetalleRegistros l in RegTPM)
                    {
                        Logs.FullInfo("********Datos de Integracion DocEntry " + l.SDocEntry + " - Linea " + l.VisOrder + "********", "", Usuario, 3, "", ""); //Log Informativo
                        Logs.FullInfo("", "\t TPM Linea[VisOrder] : " + l.VisOrder +
                                            "\r\n\t DocEntry => " + l.SDocEntry +
                                            "\r\n\t Usuario => " + Usuario
                                            , Usuario, 0, modulo, metodo); //Log Informativo

                        int Linea = Ob.LineaTPM(Int32.Parse(l.SDocEntry), l.LineId);

                        oGeneralParams.SetProperty("DocEntry", l.SDocEntry);
                        oGeneralData = oGeneralService.GetByParams(oGeneralParams);

                        oSons = oGeneralData.Child("EVT_OTPM1");
                        oSons.Remove(Linea);
                        oGeneralService.Update(oGeneralData);

                        Logs.FullInfo("", "***Linea de TPM Eliminadas : OK **************\r\n", Usuario, 0, modulo, metodo); //Log Informativo                        
                        Logs.FullInfo("Datos de Eliminación Linea " + l.VisOrder + "*******\r\n\n", "", Usuario, 4, "", ""); //Log Informativo
                    }

                    Logs.FullInfo("", "Integración [Eliminación] Realizada: TPM Eliminado \r\n**Integracion OK *******************\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return true;
                }
                else
                {
                    Resp.Mensaje = msj;
                    Logs.FullInfo("", "Conexion a SAP NO Realizada, " + msj + "\r\n\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                Resp.Mensaje = ex.Message;
                return false;
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;

                    Logs.FullInfo("", "Conexion a SAP Cerrada" + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        /******************************************************************************/
        //Etiqueta Master
        /******************************************************************************/
        public bool RegistroMaster(ref MensajesML Resp, string g)
        {
            List<Etiqueta> Registros;
            Etiqueta MasterWMS;
            string msjError = string.Empty;
            string CodigoMaster = string.Empty;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Registro de Master", Usuario, 0, modulo, metodo); //Log Informativo
            try
            {
                Logs.FullInfo("", "Obtener información de Registros", Usuario, 0, modulo, metodo); //Log Informativo
                Registros = new List<Etiqueta>();
                Registros = Ob.MLineas(ref msjError, g, 1);

                if (Registros.Count > 0)
                {
                    msjError = string.Empty;
                    if (SP.GuardarMasterWMS(ref msjError, Registros, g.ToString())) //Guarda Registro en Tabla Movimiento
                    {
                        //Creación de Master
                        int codigo = 0;
                        msjError = string.Empty;
                        MasterWMS = new Etiqueta();
                        MasterWMS = Ob.RegMaster(ref msjError, g);  

                        if (SP.GenerarMasterWMS(ref msjError, ref CodigoMaster, MasterWMS))
                        {
                            Logs.FullInfo("", "Master generada: " + CodigoMaster, Usuario, 0, modulo, metodo); //Log Informativo
                            Logs.FullInfo("", "Actualizar Master en Registros", Usuario, 0, modulo, metodo); //Log Informativo
                            
                            Resp.CodigoMaster = CodigoMaster;
                            SP.MasterReg(ref msjError, ref codigo, CodigoMaster, Registros, g);
                            if (codigo == 0)
                            {
                                Resp.Mensaje = "Master generada correctamente, registros actualizados";
                            }
                            else
                            {
                                Resp.Mensaje = "Master generada correctamente, error al actualizar registros, verificar. " + msjError;
                            }
                            
                            return true;
                        }
                        else
                        {
                            Logs.Registrar("Master no generada: ", msjError, modulo, metodo);
                            Logs.FullInfo("", "Error al generar Master: " + msjError, Usuario, 0, modulo, metodo); //Log Informativo
                            Resp.CodigoMaster = "-1";
                            Resp.Mensaje = msjError;
                            return false;
                        }
                    }
                    else
                    {
                        Resp.CodigoMaster = "-1";
                        Resp.Mensaje = msjError;
                        return false;
                    }
                }
                else
                {
                    Resp.CodigoMaster = "-1";
                    Resp.Mensaje = msjError;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                Resp.CodigoMaster = "-1";
                Resp.Mensaje = ex.Message;
                return false;
            }
            finally
            {
                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool EditarMaster(ref MensajesML Resp, string g)
        {
            List<Etiqueta> Registros;
            Etiqueta MasterWMS;
            string msjError = string.Empty;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Integración", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "Actualización de Master", Usuario, 0, modulo, metodo); //Log Informativo
            try
            {
                Logs.FullInfo("", "Obtener información de Registros", Usuario, 0, modulo, metodo); //Log Informativo
                Registros = new List<Etiqueta>();
                Registros = Ob.MLineas(ref msjError, g, 2); //Obtener registros de Master | SOLO registros nuevos

                if (Registros.Count > 0)
                {
                    msjError = string.Empty;
                    if (SP.GuardarMasterWMS(ref msjError, Registros, g.ToString())) //Guarda Registro en Tabla Movimiento
                    {
                        //Creación de Master
                        int codigo = 0;
                        msjError = string.Empty;
                        MasterWMS = new Etiqueta();
                        MasterWMS = Ob.RegMaster(ref msjError, g);

                        if (SP.ActualizarMasterWMS(ref msjError, Registros[0].EtqMaster, MasterWMS))
                        {
                            Logs.FullInfo("", "Master Actualizada: " + Registros[0].EtqMaster, Usuario, 0, modulo, metodo); //Log Informativo
                            Logs.FullInfo("", "Actualizar Master en Registros", Usuario, 0, modulo, metodo); //Log Informativo

                            Resp.CodigoMaster = Registros[0].EtqMaster;
                            SP.MasterReg(ref msjError, ref codigo, Registros[0].EtqMaster, Registros, g);
                            if (codigo == 0)
                            {
                                Resp.Mensaje = "Master actualizada correctamente, registros actualizados";
                            }
                            else
                            {
                                Resp.Mensaje = "Master actualizada correctamente, error al actualizar registros, verificar. " + msjError;
                            }
                            return true;
                        }
                        else
                        {
                            Logs.Registrar("Master no Actualizada: ", msjError, modulo, metodo);
                            Logs.FullInfo("", "Error al actualizar Master: " + msjError, Usuario, 0, modulo, metodo); //Log Informativo
                            Resp.CodigoMaster = "-1";
                            Resp.Mensaje = msjError;
                            return false;
                        }
                    }
                    else
                    {
                        Resp.CodigoMaster = "-1";
                        Resp.Mensaje = msjError;
                        return false;
                    }
                }
                else
                {
                    Resp.CodigoMaster = "-1";
                    Resp.Mensaje = msjError;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                Resp.CodigoMaster = "-1";
                Resp.Mensaje = ex.Message;
                return false;
            }
            finally
            {
                Logs.FullInfo("Integración", "", Usuario, 4, "", ""); //Log Informativo
            }
        }
        /******************************************************************************/
        //Conexiones SAP
        public bool Conectar(ref string msjCon) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;

            Logs.FullInfo("Conexion a SAP V1", "", Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 3, "", ""); //Log Informativo

            try
            {
                Logs.FullInfo("", "Iniciando Conexion | User SAP : " + Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault(), Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 0, modulo, metodo); //Log Informativo

                SAP param = modeloConexion();
                
                oCompany = new Company();
                oCompany.Server = $"{param.Server}";
                oCompany.DbServerType = param.ServerType == "MSSQL2016" ? BoDataServerTypes.dst_MSSQL2016 : BoDataServerTypes.dst_MSSQL2017;
                oCompany.CompanyDB = param.Database;
                oCompany.LicenseServer = param.LicenseServer;
                //oCompany.UserName = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                //oCompany.Password = Identity.Claims.Where(p => p.Type == ClaimTypes.Authentication).Select(p => p.Value).SingleOrDefault(); ;
                oCompany.UserName = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault();
                oCompany.Password = Identity.Claims.Where(p => p.Type == ClaimTypes.Anonymous).Select(p => p.Value).SingleOrDefault();
                oCompany.DbUserName = param.UserId;
                oCompany.DbPassword = param.Password;
                oCompany.language = BoSuppLangs.ln_Spanish_La;
                oCompany.UseTrusted = false;

                if (oCompany.Connect() != 0)
                {
                    msjCon = oCompany.GetLastErrorDescription();
                    Logs.FullInfo("", "Error: NO se pudo conectar ---- " + msjCon, Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 0, modulo, metodo); //Log Informativo
                    return false;
                }
                else
                {
                    Logs.FullInfo("", "Conexion a SAP Realizada", Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msjCon = ex.Message;

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                Logs.FullInfo("Conexion a SAP V1", "", Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 4, "", ""); //Log Informativo
            }
            return true;
        }
        
        public Company sboConect(ref string msjCon)
        {
            //string metodo = "sboConect";
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;

            Logs.FullInfo("Conexion a SAP", "", Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 3, "", ""); //Log Informativo

            try
            {
                Logs.FullInfo("", "Iniciando Conexion | User SAP : " + Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault(), Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 0, modulo, metodo); //Log Informativo

                SAP param = modeloConexion();
                oCompany = new Company();
                oCompany.Server = $"{param.Server}"; //"YOUR_SERVER";
                oCompany.DbServerType = param.ServerType == "MSSQL2016" ? BoDataServerTypes.dst_MSSQL2016 : BoDataServerTypes.dst_MSSQL2017;
                oCompany.CompanyDB = param.Database;
                oCompany.LicenseServer = param.LicenseServer;
                //oCompany.UserName = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                //oCompany.Password = Identity.Claims.Where(p => p.Type == ClaimTypes.Authentication).Select(p => p.Value).SingleOrDefault();
                oCompany.UserName = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault();
                oCompany.Password = Identity.Claims.Where(p => p.Type == ClaimTypes.Anonymous).Select(p => p.Value).SingleOrDefault();
                oCompany.DbUserName = param.UserId;
                oCompany.DbPassword = param.Password;
                oCompany.language = BoSuppLangs.ln_Spanish_La; // BoSuppLangs.ln_Spanish_La;
                oCompany.UseTrusted = false;

                if (oCompany.Connect() != 0)
                {
                    msjCon = oCompany.GetLastErrorDescription();
                    Logs.FullInfo("", "Error: NO se pudo conectar ---- " + msjCon, Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 0, modulo, metodo); //Log Informativo

                    return null;
                }
                else
                {
                    Logs.FullInfo("", "Conexion a SAP Realizada", Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msjCon = ex.Message;

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 0, modulo, metodo); //Log Informativo
                return null;
            }
            finally
            {
                Logs.FullInfo("Conexion a SAP", "", Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault(), 4, "", ""); //Log Informativo
            }
            return oCompany;
        }

        public Company sboConectPrueba(ref string msjCon, string usuario, string pssap, string usrsap)
        {
            //string metodo = "sboConect";
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;

            Logs.FullInfo("Conexion de Prueba a SAP: " + usrsap, "", usuario, 3, "", ""); //Log Informativo

            try
            {
                Logs.FullInfo("", "Iniciando Conexion", usuario, 0, modulo, metodo); //Log Informativo

                SAP param = modeloConexion();
                oCompany = new Company();
                oCompany.Server = $"{param.Server}"; //"YOUR_SERVER";
                oCompany.DbServerType = param.ServerType == "MSSQL2016" ? BoDataServerTypes.dst_MSSQL2016 : BoDataServerTypes.dst_MSSQL2017;
                oCompany.CompanyDB = param.Database;
                oCompany.LicenseServer = param.LicenseServer;
                oCompany.UserName = usrsap;
                oCompany.Password = pssap;
                oCompany.DbUserName = param.UserId;
                oCompany.DbPassword = param.Password;
                oCompany.language = BoSuppLangs.ln_Spanish_La; // BoSuppLangs.ln_Spanish_La;
                oCompany.UseTrusted = false;

                if (oCompany.Connect() != 0)
                {
                    msjCon = oCompany.GetLastErrorDescription();
                    Logs.FullInfo("", "Error: NO se pudo conectar ---- " + msjCon, usuario, 0, modulo, metodo); //Log Informativo

                    return null;
                }
                else
                {
                    Logs.FullInfo("", "Conexion a SAP Realizada", usuario, 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msjCon = ex.Message;

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, usuario, 0, modulo, metodo); //Log Informativo
                return null;
            }
            finally
            {
                Logs.FullInfo("Conexion de Prueba a SAP: " + usrsap, "", usuario, 4, "", ""); //Log Informativo
            }
            return oCompany;
        }

        public SAP modeloConexion() {
            try
            {
                String[] substrings = CadenaSAP.Split(';');
                modeloSAp = new SAP()
                {
                    Server = substrings[0].Split('=')[0] == "Server" ? substrings[0].Split('=')[1] : "",
                    Database = substrings[1].Split('=')[0] == "Database" ? substrings[1].Split('=')[1] : "",
                    UserId = substrings[2].Split('=')[0] == "UserId" ? substrings[2].Split('=')[1] : "",
                    Password = substrings[3].Split('=')[0] == "Password" ? substrings[3].Split('=')[1] : "",
                    UserIdSAP = substrings[4].Split('=')[0] == "UserIdSAP" ? substrings[4].Split('=')[1] : "",
                    PasswordSAP = substrings[5].Split('=')[0] == "PasswordSAP" ? substrings[5].Split('=')[1] : "",
                    ServerType = substrings[6].Split('=')[0] == "ServerType" ? substrings[6].Split('=')[1] : "",
                    LicenseServer = substrings[7].Split('=')[0] == "LicenseServer" ? substrings[7].Split('=')[1] : ""
                };

            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
            }
            return modeloSAp;
        }
        
    }
}