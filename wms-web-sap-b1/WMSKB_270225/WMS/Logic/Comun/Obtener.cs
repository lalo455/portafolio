using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WMS.Models;
using Dapper;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using SAPbobsCOM;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using static WMS.Models.TrsManual;

namespace WMS.Logic.Comun
{
    public class Obtener
    {
        string CadReg;
        Utilities.Log Logs;
        private ClaimsPrincipal Identity;
        //string modulo = "Obtener.cs";

        public Obtener() {
            this.CadReg = ConfigurationManager.ConnectionStrings["WmsSQL"].ConnectionString;
            this.Logs = new Utilities.Log();
            Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
        }

        //Modulo de Traspaso Manual
        public bool Lotes(string QR, ref List<Etiqueta> Etiqueta) {
            IDbConnection dbCon = null;
            decimal Quantity = 0;

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    if (QR.Split('&').Length < 7 || QR.Split('&').Length > 8)
                    {
                        return false;
                    }
                    else 
                    {
                        dbCon.Open();
                        switch (NoGestionable(QR, ref Quantity))
                        {
                            case true:
                                Etiqueta = dbCon.Query<Etiqueta>(@"EXEC ""WMSSP_ObtenerEtiEspecial"" '" + Lote(QR) + "','NGL' ").ToList();
                                Etiqueta[0].OnHandQty = Quantity;
                                break;
                            case false:
                                Etiqueta = dbCon.Query<Etiqueta>($"EXEC \"WMSSP_ObtenerLote\" '{Lote(QR)}','{Articulo(QR)}'").ToList();
                                break;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open) {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }
        
        public string Lote(string QR) {
            return QR.Split('&')[0];
        }
        
        public string Articulo(string QR) {
            return QR.Split('&')[1];
        }

        //Modulo de Traspaso Manual
        public bool NoGestionable(string QR, ref decimal Qty) {
            try
            {
                string[] Cont = QR.Split('&');
                if (Cont[0] == Cont[1])
                {
                    Qty = decimal.Parse(Cont[2]);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            //return QR.Split('&')[0] == QR.Split('&')[1] ? true : false;
        }
       
        public List<Recuperar.Line> Lineas(string guid, string tipo) {
            IDbConnection dbCon = null;
            List<Recuperar.Line> ln = new List<Recuperar.Line>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Lineas", "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Lineas por GUID => " + guid, Usuario, 0, modulo, metodo); //Log Informativo
                    ln = dbCon.Query<Recuperar.Line>(@"EXEC WMSSP_Lineas @guid = '" + guid + "', @accion = '" + tipo + "'").ToList();
                }

                foreach (Recuperar.Line lN in ln)
                {
                    Logs.FullInfo("",   "\t LineNum => " + lN.LineNum +
                                        ", \t ItemCode => " + lN.ItemCode +
                                        ", \t Desde => " + lN.Desde +
                                        ", \t Hasta => " + lN.Hasta +
                                        ", \t Ubicacion Dest => " + lN.UbDestino +
                                        ", \t Cantidad => " + lN.Qty +
                                        ", \t Lotes => " + lN.Lotes +
                                        ", \t Tarima => " + lN.Tarima +
                                        ", \t Dispositivo => " + lN.Dispositivo +
                                        ", \t NumDispositivo => " + lN.NumDispositivos +
                                        ", \t Peso Bruto => " + lN.PesoBruto +
                                        ", \t Peso Neto => " + lN.PesoNeto +
                                        ", \t Numero OC => " + lN.OC +
                                        ", \t Posicion OC => " + lN.LineOC +
                                        ", \t RemSubstitute => " + lN.RemSubstitute +
                                        ", \t RemPurchase => " + lN.RemPurchase +
                                        ", \t RemEmpaquePallet => " + lN.RemEmpaquePallet +
                                        "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lineas", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
            return ln;
        }
        
        public List<Recuperar.Batch> LotesLineas(string guid, string Item, int NumLinea, int Tarima, string dispositivo, string tipo, int UbDestino = 0)
        {
            IDbConnection dbCon = null;
            List<Recuperar.Batch> Lts = new List<Recuperar.Batch>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Lotes para la Linea " + NumLinea, "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Lotes para Linea " + NumLinea + 
                                                    ": \t GUID => " + guid + 
                                                    "- \t ItemCode => " + Item + 
                                                    "- \t NumLinea => " + NumLinea + 
                                                    "- \t Tarima => " + Tarima + 
                                                    "- \t Dispositivo => " + dispositivo +
                                                    "- \t Tipo => " + tipo +
                                                    "- \t Destino => " + UbDestino
                                    , Usuario, 0, modulo, metodo); //Log Informativo

                    Lts = dbCon.Query<Recuperar.Batch>(@"EXEC ""WMSSP_Lotes"" @guid = '"+guid+"', @Item = '"+Item+"', @NumLine = "+NumLinea+", @Tarima = "+Tarima+", @Dispositivo = '"+dispositivo+"', @accion = '"+tipo+"', @ubiDestino = "+UbDestino).ToList();
                }

                foreach (Recuperar.Batch lT in Lts)
                {
                    Logs.FullInfo("",   "\t BaseLineNumber => " + lT.BaseLine + 
                                        "- \t LineNumLote => " + lT.LineNumLote + 
                                        "- \t Lote => " + lT.Lote + 
                                        "- \t Cantidad => " + lT.QtyLote
                                        , Usuario, 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lotes para la Linea " + NumLinea, "", Usuario, 4, modulo, metodo); //Log Informativo
            }

            return Lts;
        }
        
        public List<Recuperar.BinLocation> Ubicaciones(string guid, string Item, int LineaBase, int LineaBaseLote, string Lote, int Tarima, string dispositivo, string tipo, int UbDestino = 0)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Ubicacion Origen", "", Usuario, 3, modulo, metodo); //Log Informativo

            IDbConnection dbCon = null;
            List<Recuperar.BinLocation> Ubs = new List<Recuperar.BinLocation>();
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "\t Obtener Ubicacion Origen para el Lote: " + Lote +
                                        ": \r\n WMSSP_Ubicaciones => \t IdGuid : " + guid +
                                        ", \t Articulo: " + Item + 
                                        ", \t Linea Base: " + LineaBase + 
                                        ", \t Linea Base Lote: " + LineaBaseLote + 
                                        ", \t Lote: " + Lote + 
                                        ", \t Tarima: " + Tarima + 
                                        ", \t Dispositivo: " + dispositivo +
                                        ", \t Tipo: " + tipo +
                                        ", \t Destino: " + UbDestino
                                        , Usuario, 0, modulo, metodo); //Log Informativo

                    Ubs = dbCon.Query<Recuperar.BinLocation>(@"EXEC ""WMSSP_Ubicaciones"" @guid = '"+guid+"', @Item = '"+Item+"', @BaseLine = "+LineaBase+", @BaseLineLote = "+LineaBaseLote+", @Lote = '"+Lote+"', @Tarima = "+Tarima+", @Dispositivo = '"+dispositivo+"', @accion = '"+tipo+"', @ubiDestino = "+UbDestino).ToList();
                }

                foreach (Recuperar.BinLocation bL in Ubs)
                {
                    Logs.FullInfo("", "Obtener Lotes:  \t BaseLine => " + bL.BaseLine + 
                                                        "- \t BaseLineLote => " + bL.BaseLineLote + 
                                                        "- \t UbOrigen => " + bL.UbOrigen + 
                                                        "- \t QtyUbOrigen => " + bL.QtyUbOrigen
                                                        , Usuario, 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Ubicacion Origen", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
            return Ubs;
        }
        
        public List<Recuperar.BinLocation> UbicacionesDest(string guid, string Item, int LineaBase, int LineaBaseLote, string Lote, int Tarima, string dispositivo, string tipo, int UbDestino = 0)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Ubicacion Destino", "", Usuario, 3, modulo, metodo); //Log Informativo

            IDbConnection dbCon = null;
            List<Recuperar.BinLocation> Ubs = new List<Recuperar.BinLocation>();
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open(); 
                    Logs.FullInfo("", "Obtener Ubicaciones Destino para el Lote: " + Lote +
                                        ": \r\n WMSSP_UbicacionesDes => \t IdGuid: " + guid + 
                                        ", \t Articulo: " + Item + 
                                        ", \t Linea Base: " + LineaBase + 
                                        ", \t Linea Base Lote: " + LineaBaseLote + 
                                        ", \t Lote: " + Lote + 
                                        ", \t Tarima: " + Tarima + 
                                        ", \t Dispositivo: " + dispositivo +
                                        ", \t Tipo: " + tipo +
                                        ", \t Destino: " + UbDestino
                                        , Usuario, 0, modulo, metodo); //Log Informativo
                    
                    Ubs = dbCon.Query<Recuperar.BinLocation>(@"EXEC ""WMSSP_UbicacionesDes"" @guid = '" + guid + "', @Item = '" + Item + "', @BaseLine = " + LineaBase + ", @BaseLineLote = " + LineaBaseLote + ", @Lote = '" + Lote + "', @Tarima = " + Tarima + ", @Dispositivo = '" + dispositivo + "', @accion = '" + tipo + "', @ubiDestino = " + UbDestino).ToList();
                }

                foreach (Recuperar.BinLocation bL in Ubs)
                {
                    Logs.FullInfo("",   "\t BaseLine => " + bL.BaseLine + 
                                        "- \t BaseLineLote => " + bL.BaseLineLote +
                                        "- \t UbDestino => " + bL.UbDestino +
                                        "- \t QtyUbDestino => " + bL.QtyUbDestino
                                        , Usuario, 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Ubicacion Destino", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
            return Ubs;
        }
        
        public int RecDocNum(string Tipo, string DocEntry)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener DocNum de Traspaso Generado", "", Usuario, 3, modulo, metodo); //Log Informativo
            IDbConnection dbCon = null;
            int DocNum = 0;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    DocNum = dbCon.Query<int>(@"EXEC ""WMSSP_RecuperarDocNum"" '" + DocEntry + "','" + Tipo + "'").SingleOrDefault();
                    Logs.FullInfo("", "WMSSP_RecuperarDocNum: DocEntry => " + DocEntry + "- Tipo => " + Tipo, Usuario, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("", "DocNum Recuperado: " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener DocNum de Traspaso Generado", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
            return DocNum;
        }

        public bool EsGranel(ref Produccion.Ubicacion Piso, string ItemCode, List<Produccion.DetalleSolicitud> Solicitud)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Es Granel", "", Usuario, 3, modulo, metodo); //Log Informativo
            IDbConnection dbCon = null;
            try
            {
                foreach (var j in Solicitud)
                {
                    if (j.ItemCode == ItemCode && j.Granel == "Y")
                    {
                        using (dbCon = new SqlConnection(CadReg))
                        {
                            dbCon.Open();
                            //Piso = dbCon.Query<Produccion.Ubicacion>(@"SELECT Top 1 AbsEntry, BinCode FROM OBIN WHERE SysBin = 'Y' AND WhsCode = '" + ConfigurationManager.AppSettings["WhsProd"] + "'").SingleOrDefault();
                            //AKSYS_DB => WMSSP_WMSConsultar
                            Piso = dbCon.Query<Produccion.Ubicacion>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 4, @Almacen = '{ ConfigurationManager.AppSettings["WhsProd"] }' ").SingleOrDefault();

                            Logs.FullInfo("", "Obtener Es Granel: WMSSP_WMSConsultar :: Accion = 4, Almacen = " + ConfigurationManager.AppSettings["WhsProd"], Usuario, 0, modulo, metodo); //Log Informativo
                            Logs.FullInfo("", "Obtener Es Granel Respuesta: AbsEntry => " + Piso.AbsEntry + "- BinCode => " + Piso.BinCode, Usuario, 0, modulo, metodo); //Log Informativo
                            return true;
                        }
                    }
                }
                //Piso = new Produccion.Ubicacion(-1,"");
                return false;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, "", ""); //Log Informativo
                }
                Logs.FullInfo("Es Granel", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
        }

        public bool ExisteRecurso(ref string NomRecurso, List<Produccion.DetalleOrden> LineasOrd)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("ExisteRecurso", "", Usuario, 3, modulo, metodo); //Log Informativo
            try
            {
                foreach (var Li in LineasOrd)
                {
                    if (Li.ItemType == 290)
                    {
                        NomRecurso = Li.ItemCode;
                        Logs.FullInfo("", "Recurso: " + NomRecurso, Usuario, 0, modulo, metodo); //Log Informativo
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                Logs.FullInfo("ExisteRecurso", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
        }
        
        public bool UbiRecurso(ref Produccion.Ubicacion Ubi, string Recurso)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("UbicacionRecurso", "", Usuario, 3, modulo, metodo); //Log Informativo
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    //Ubi = dbCon.Query<Produccion.Ubicacion>(@"SELECT Top 1 AbsEntry, BinCode FROM OBIN WHERE BinCode like '%" + Recurso.ToUpper() + "%'").SingleOrDefault();
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //AKSYS_DB => WMSSP_WMSConsultar
                    Ubi = dbCon.Query<Produccion.Ubicacion>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 5, @Recurso = '{ Recurso.ToUpper() }' ").SingleOrDefault();

                    Logs.FullInfo("", "WMSSP_WMSConsultar :: Accion = 5, Recurso = " + Recurso.ToUpper(), Usuario, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("", "Ubicacion Recurso: " + Ubi, Usuario, 0, modulo, metodo); //Log Informativo
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("UbicacionRecurso", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
        }

        public Produccion.Ubicacion ObtenerUbiSys(string Whs)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("ObtenerUbiSys", "", Usuario, 3, modulo, metodo); //Log Informativo
            IDbConnection dbCon = null;
            Produccion.Ubicacion UbWhs = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) {
                    dbCon.Open();
                    //UbWhs = dbCon.Query<Produccion.Ubicacion>(@"SELECT AbsEntry, BinCode FROM OBIN WHERE WhsCode = '" + ConfigurationManager.AppSettings["MP"] + "' AND SysBin = 'Y'").FirstOrDefault();
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //AKSYS_DB => WMSSP_WMSConsultar
                    UbWhs = dbCon.Query<Produccion.Ubicacion>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 6, @Almacen = '{  ConfigurationManager.AppSettings["MP"] }' ").FirstOrDefault();

                    Logs.FullInfo("", "WMSSP_WMSConsultar :: Accion = 6, Almacen = " + ConfigurationManager.AppSettings["MP"], Usuario, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("", "UbiSys: " + UbWhs, Usuario, 0, modulo, metodo); //Log Informativo
                    return UbWhs;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return UbWhs;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("ObtenerUbiSys", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
        }

        //********************************************************************************************************************
        //********************************************************************************************************************

        public int ObtenerTurnoDocumento(string hora)
        {
            IDbConnection dbCon = null;
            int turno = 1;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Turno de la Hora " + hora, "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Abrir Conexion a BD WMS", Usuario, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("", "Obtener turno => " + hora, Usuario, 0, modulo, metodo); //Log Informativo
                    turno = dbCon.Query<int>(@"EXEC ""WMS_TurnoDocumento"" @hora = '" + hora + "'").SingleOrDefault();
                }

                //---
                Logs.FullInfo("", "Turno obtenido => " + turno + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                //-----
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD WMS", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Turno de la Hora " + hora, "", Usuario, 4, modulo, metodo); //Log Informativo
            }

            return turno;
        }

        //********************************************************************************************************************
        //********************************************************************************************************************

        public int ObtenerLineasSol(List<Produccion.DetalleSolicitud> LineasSolicitud, string ItemCode)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            int NumLi = -1;
            foreach (Produccion.DetalleSolicitud s in LineasSolicitud)
            {
                if (s.ItemCode == ItemCode)
                {
                    NumLi = s.LineNum;
                    break;
                }
            }
            Logs.FullInfo("", "Obtener LineasSol => " + NumLi, Usuario, 0, modulo, metodo); //Log Informativo
            return NumLi;
        }

        public bool ObtenerValidacionProduccion(ref List<Produccion.ValidacionProd> Lineas, string lote, string item) {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Lineas = dbCon.Query<Produccion.ValidacionProd>($"EXEC \"WMSSP_DetalleCalidad\" '{lote}','{item}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }
        
        public bool ActualizarDtsOrden(ref string msj, List<Produccion.ValidacionProd> ListaActualizar, string Usr) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Actualizar Datos de Etiquetas - Liberaracion de Etiqueta", "", Usuario, 1, modulo, metodo); //Log Informativo

            IDbConnection dbCon = null;
            string Lotes = string.Empty;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (Produccion.ValidacionProd L in ListaActualizar)
                    {
                        if (string.IsNullOrEmpty(Lotes))
                            //Lotes = "''" + L.Name + "''";
                            Lotes = $"''{L.Name}''";
                        else
                            //Lotes += ",''" + L.Name + "''";
                            Lotes += $",''{L.Name}''";
                    }

                    Logs.FullInfo("", "Actualizacion :: WMSSP_WMSAgregar \t Accion = 91, \t Usuario = '" + Usr + 
                                                                            "', \t Fecha = '" + DateTime.Now.ToString("dd / MM / yyyy HH: mm:ss") + 
                                                                            ", \t Lotes = (" + Lotes + ")"
                                                                            , Usuario, 0, modulo, metodo); //Log Informativo
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //dbCon.Execute($"UPDATE \"@EVT_ETQSPT\" SET \"U_stEti\" = 'R', \"U_LiberaUser\" = '{Usr}', \"U_LibTime\" = '{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}' WHERE NAME IN (" + Lotes + ")");
                    //AKSYS_DB => WMSSP_WMSAgregar
                    dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 91, @Usuario = '{Usr}', @Fecha = '{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}', @Lotes = '{ Lotes }'");

                    //Logs.FullInfo("", "Actualizacion :: UPDATE @EVT_ETQSPT SET U_stEti = 'R', U_LiberaUser = '" + Usr + "', U_LibTime = '" + DateTime.Now.ToString("dd / MM / yyyy HH: mm:ss") + "' WHERE NAME IN (" + Lotes + ")", Usuario, 0, modulo, metodo); //Log Informativo
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                //Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                Logs.FullInfo("", "¡¡¡ERROR!!!: Mensaje: " + ex.Message + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo

                msj = ex.Message;
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    //Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("", "Conexion a SAP Cerrada" + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Actualizar Datos de Etiquetas - Liberaracion de Etiqueta", "", Usuario, 2, modulo, metodo); //Log Informativo
            }
        }
        

        //Modulo Calidad
        public bool ActualizarSAP(ref string msj, List<DetalleR> calidad, string estatus, Company _oCompany)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            //IDbConnection dbCon = null;
            string _LoteSap = string.Empty;
            string _ArticuloSAP = string.Empty;
            Recordset oRecordS = null;
            oRecordS = _oCompany.GetBusinessObject(BoObjectTypes.BoRecordset) as Recordset;

            Logs.FullInfo("Actualizar Lotes SAP", "", Usuario, 3, modulo, metodo); //Log Informativo
            Logs.FullInfo("", "Lotes a Actualizar", Usuario, 0, modulo, metodo); //Log Informativo
            
            foreach (DetalleR etq in calidad)
            {
                Logs.FullInfo("", "Lote: " + etq.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
            }
            
            try
            {
                foreach (var L in calidad)
                {
                    if (string.IsNullOrEmpty(_LoteSap))
                    {
                        _LoteSap = $"''{ L.DistNumber }''";
                        _ArticuloSAP = $"''{L.ItemCode}''";
                    }
                    else if (!string.IsNullOrEmpty(_LoteSap))
                    {
                        _LoteSap += $",''{ L.DistNumber }''";

                        if (!_ArticuloSAP.Contains(L.ItemCode))
                        {
                            _ArticuloSAP += $",''{ L.ItemCode }''";
                        }
                    }
                }

                Logs.FullInfo("", "WMSSP_Procesos Accion = 2, Estatus = '" + (estatus == "X" ? '2' : '0') + 
                                                                "', Lotes = (" + _LoteSap + "), Articulos = (" + _ArticuloSAP + ")"
                                                                , Usuario, 0, modulo, metodo); //Log Informativo

                oRecordS.DoQuery($"EXEC \"WMSSP_Procesos\" @Accion = 2, @Estatus = '{ (estatus == "X" ? '2' : '0') }', @Lotes = '{_LoteSap}', @Articulos = '{_ArticuloSAP}' "); //query
                return true;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                msj = ex.Message;
                return false;
            }
            finally
            {
                if (oRecordS != null)
                {
                    Marshal.ReleaseComObject(oRecordS);
                }

                Logs.FullInfo("Actualizar Lotes SAP", "", Usuario, 4, modulo, metodo); //Log Informativo                
            }
        }

        public bool ObtenerModulos(ref List<Menu.Modulos> ListMod, string Usr) {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ListMod = dbCon.Query<Menu.Modulos>($"EXEC \"WMSSP_Modulos\" '{Usr}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }
        
        public bool ObtenerAlm(ref List<TrsManual.Almacen> ListaAlmacen, string WhsCode, string TipoWhs) 
        {
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    dbCon.Open();
                    ListaAlmacen = dbCon.Query<TrsManual.Almacen>(@"EXEC ""WMSSP_Almacenes"" @Accion = '"+TipoWhs+"', @WhsCode = '"+WhsCode+"', @Usuario = '"+Usuario+"'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }

        //Recuento de Inventario
        public bool LotesRecuento(ref List<Recuento.LtsRecuento> Lts, string Almacen, string Lote, string Articulo) {

            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) {
                    dbCon.Open(); 
                    Lts = dbCon.Query<Recuento.LtsRecuento>(@"EXEC ""WMSSP_LotesRecuento"" @Batch = '" + Lote + "', @Item = '" + Articulo + "', @WhsCode = '" + Almacen + "'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }
        
        public bool EstadoEtiqueta(ref List<InicialEtiqueta> Lista, string QR) {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    dbCon.Open();
                    Lista = dbCon.Query<InicialEtiqueta>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 1, @LoteEtiqueta = '{ QR }'").ToList(); 
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }

        public bool GetDocNum(ref string DocNum, string CodeGuia, string Accion)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    switch (Accion)
                    {
                        case "Traspaso":
                            //AKSYS_DB => WMSSP_WMSConsultar
                            DocNum = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 15, @Subaccion = 1, @CodeGuia = '{ CodeGuia }'").FirstOrDefault();
                            break;

                        case "Produccion":
                            DocNum = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 15, @Subaccion = 2, @CodeGuia = '{ CodeGuia }'").FirstOrDefault();
                            break;

                        case "Remision":
                            DocNum = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 15, @Subaccion = 3, @CodeGuia = '{ CodeGuia }'").FirstOrDefault();
                            break;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                DocNum = ex.Message;
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }


        public bool ObtenerInformación<T>(ref List<T> Lista, string tipo, string condString, string usuario)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) {
                    Lista = dbCon.Query<T>($"EXEC \"WMSSP_ObtenerInformacion\" '{ tipo }','{ condString }'").ToList();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool InformacionTraspaleo(ref List<Traspaleo> rows, string Lote) //Llamada en Traspaleo - Informacion de Etiqueta
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    //rows = dbCon.Query<Traspaleo>($"SELECT U_ItemCode,U_ItemName,Name,U_stImp,U_stEti,U_stCal,U_Cancel,U_QtyEti,U_Dispositivo,U_OF,U_GenDate FROM [@EVT_ETQSPT] WHERE Name = '{Lote}'").ToList();
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //AKSYS_DB => WMSSP_WMSConsultar
                    rows = dbCon.Query<Traspaleo>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 7, @LoteEtiqueta = '{ Lote }' ").ToList();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool UbicacionesLista(string Warehouse, string BinCode, ref List<Models.Comun.Location> Location) 
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    if (!string.IsNullOrEmpty(BinCode))
                    {
                        Location = dbCon.Query<Models.Comun.Location>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 8, @Almacen = '{ Warehouse }', @Ubicacion = '{ BinCode }' ").ToList();
                    }
                    else
                    {
                        Location = dbCon.Query<Models.Comun.Location>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 8, @Almacen = '{ Warehouse }', @Ubicacion = '0' ").ToList();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }


        //Modulo: Recibo de Produccion

        /*
        Actualización: Obtener Turno y Fecha de Etiqueta Leida - 12-01-2021
        Codigo YA NO UTILIZADO
        */
        public bool TurnoEtq(string Lote, ref string TurnoR)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    //Actualización 5-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //TurnoR = dbCon.Query<string>($"SELECT TOP 1 \"U_Turno\" FROM \"@EVT_ETQSPT\" WHERE \"Name\" = '" + Lote + "' AND \"U_Cancel\" = 'N'").SingleOrDefault();
                    //AKSYS_DB => WMSSP_WMSConsultar
                    TurnoR = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 2, @LoteEtiqueta = '{ Lote }'").SingleOrDefault();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                TurnoR = ex.Message;
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }

        public bool FechaEtq(string Lote, ref string FechaR)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    //Actualización 5-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //FechaR = dbCon.Query<string>($"SELECT TOP 1 CONVERT(VARCHAR,\"U_GenDate\", 23) FROM \"@EVT_ETQSPT\" WHERE \"Name\" = '" + Lote + "' AND \"U_Cancel\" = 'N'").SingleOrDefault();
                    //AKSYS_DB => WMSSP_WMSConsultar
                    FechaR = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 3, @LoteEtiqueta = '{ Lote }'").SingleOrDefault();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                FechaR = ex.Message;
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }
        /**************************************************************************/

        public List<Produccion.DocumentoAProduccion> ObtenerDocumentoAProduccion(string g, string Accion, int DocNum)
        {
            IDbConnection dbCon = null;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    List<Produccion.DocumentoAProduccion> docProduccion = new List<Produccion.DocumentoAProduccion>();
                    if (dbCon.State == ConnectionState.Open)
                    {
                        switch (Accion)
                        {
                            case "LOF":
                                Logs.FullInfo("", ">>> EXEC WMSSP_Documento @Accion = '" + Accion + "', @IdGuid = '" + g + "'", Usuario, 0, modulo, metodo); //Log Informativo
                                docProduccion = dbCon.Query<Produccion.DocumentoAProduccion>($"EXEC \"WMSSP_Documento\" @IdGuid = '{ g }', @Accion = '{Accion}'").ToList();
                                break;

                            case "EPM":
                                Logs.FullInfo("", ">>> EXEC WMSSP_Documento @Accion = '" + Accion + "', @IdGuid = '" + g + "', @DocNum = " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                                docProduccion = dbCon.Query<Produccion.DocumentoAProduccion>($"EXEC \"WMSSP_Documento\" @IdGuid = '{ g }', @Accion = '{Accion}', @DocNum = {DocNum}").ToList();
                                break;
                        }
                        return docProduccion;
                    }
                    else
                    {
                        Logs.FullInfo("", "Error: La conexión a la base de datos no esta disponible para el documento: Conexion Cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                        throw new Exception($"La conexión a la base de datos no esta disponible para el documento");
                    }
                } 
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo);
                throw new Exception(ex.InnerException == null ? ex.Message : ex.InnerException.ToString());
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    //Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                //Logs.FullInfo("Obtener Documento a Produccion", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
        }

        public List<Produccion.ConsumoOrdenFabricacion> ObtenerConsumoPorOrden(string g, int DocNum, double CantLiberar)
        {
            IDbConnection dbCon = null;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            //Logs.FullInfo("Obtener Detalle de Orden de Fabricacion: " + DocNum, "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    List<Produccion.ConsumoOrdenFabricacion> consumoOF = new List<Produccion.ConsumoOrdenFabricacion>();
                    if (dbCon.State == ConnectionState.Open)
                    {
                        //Logs.FullInfo("", "Conexion a BD Abierta", Usuario, 0, modulo, metodo); //Log Informativo
                        Logs.FullInfo("", ">>> EXEC WMSSP_Documento @Accion = 'ACO', @IdGuid = '"+ g + "', @DocNum = " + DocNum + ", @CantLiberar = " + CantLiberar, Usuario, 0, modulo, metodo); //Log Informativo

                        consumoOF = dbCon.Query<Produccion.ConsumoOrdenFabricacion>($"EXEC \"WMSSP_Documento\" @IdGuid = '{ g }', @Accion = 'ACO', @DocNum = { DocNum }, @CantLiberar = { CantLiberar }").ToList();

                        ////Logs.FullInfo("", "Detalle de la OF \r\n", Usuario, 0, modulo, metodo); //Log Informativo
                        //foreach (Produccion.ConsumoOrdenFabricacion cOF in consumoOF)
                        //{
                        //    Logs.FullInfo("", "\t >>> Linea => " + cOF.LineNum +
                        //                        "- \t Articulo => " + cOF.ItemCode +
                        //                        "- \t Tipo Articulo => " + cOF.ItemType +
                        //                        "- \t cantidad Base => " + cOF.BaseQty +
                        //                        "- \t Cant. Requerida => " + cOF.CantReq +
                        //                        "- \t Almacen => " + cOF.Warehouse
                        //                        , Usuario, 0, modulo, metodo); //Log Informativo
                        //}
                        return consumoOF;
                    }
                    else
                    {
                        Logs.FullInfo("", "Error: Conexion a BD Cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                        throw new Exception($"La conexión a la base de datos no esta disponible para la Orde de Fabricación");
                    }
                }  
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo

                throw new Exception(ex.InnerException == null ? ex.Message : ex.InnerException.ToString());
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    //Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                //Logs.FullInfo("Obtener Detalle de Orden de Fabricacion: " + DocNum, "", Usuario, 4, modulo, metodo); //Log Informativo
            }
        }
        
        public List<Produccion.LoteConsumo> ObtenerLotesAConsumir(string g, string ItemCode, string Warehouse, double CantReq, int DocNum)
        {
            IDbConnection dbCon = null;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            //Logs.FullInfo("Obtener Lotes a Consumir para el Articulo: " + ItemCode, "", Usuario, 3, "", ""); //Log Informativo            
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    List<Produccion.LoteConsumo> lotesEncontrados = new List<Produccion.LoteConsumo>();
                    if (dbCon.State == ConnectionState.Open)
                    {
                        //Logs.FullInfo("", "Conexion a BD Abierta", Usuario, 0, modulo, metodo); //Log Informativo
                        Logs.FullInfo("", ">>> EXEC WMSSP_Documento @Accion = 'LAC', @IdGuid = '" + g + "', @ItemCode = '" + ItemCode + "', @Warehouse = '" + Warehouse + "', @CantLiberar = " + CantReq + ", @DocNum = " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                        lotesEncontrados = dbCon.Query<Produccion.LoteConsumo>($"EXEC \"WMSSP_Documento\" @IdGuid = '{ g }', @Accion = 'LAC', @ItemCode = '{ ItemCode }', @Warehouse = '{ Warehouse }', @CantLiberar = {CantReq}, @DocNum = {DocNum}").ToList();

                        ////----
                        //if (lotesEncontrados.Count() > 0)
                        //{
                        //    //Logs.FullInfo("", "Lotes a Consumir para el Articulo " + ItemCode, Usuario, 0, modulo, metodo); //Log Informativo
                        //    foreach (Produccion.LoteConsumo lE in lotesEncontrados)
                        //    {
                        //        Logs.FullInfo("", "\t >>> Lote => " + lE.DistNumber +
                        //                            " - \t Cantidad => " + lE.Quantity +
                        //                            " - \t Estatus => " + lE.Status +
                        //                            " - \t MnfSerial => " + lE.MnfSerial +
                        //                            " - \t MdAbsEntry => " + lE.MdAbsEntry
                        //                            , Usuario, 0, modulo, metodo); //Log Informativo
                        //    }
                        //}
                        //else
                        //{
                        //    //Obtener informacion de lotes, mediante consulta a BD, para saber la información de los lotes - Pendiente
                        //    Logs.FullInfo("", "NO SE OBTUVIERON LOTES A CONSUMIR, VERIFICAR SP CON DATOS: EXEC WMSSP_Documento @IdGuid = '" + g + "', @Accion = 'LAC', @ItemCode = '" + ItemCode + "', @Warehouse = '" + Warehouse + "', @CantLiberar = " + CantReq + ", @DocNum = " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                        //}
                        ////----
                        return lotesEncontrados;
                    }
                    else
                    {
                        Logs.FullInfo("", "Error: Conexion a BD Cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                        throw new Exception($"La conexión a la base de datos no esta disponible para el consumo de lotes");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo

                throw new Exception(ex.InnerException == null ? ex.Message : ex.InnerException.ToString());
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    //Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                //Logs.FullInfo("Obtener Lotes a Consumir para el Articulo: " + ItemCode, "", Usuario, 4, modulo, metodo); //Log Informativo
            }
        }
        
        public string ObtenerMaquina(int DocNum)
        {
            IDbConnection dbCon = null;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            //Logs.FullInfo("Obtener Maquina de la Orden de Fabricacion " + DocNum, "", Usuario, 3, "", ""); //Log Informativo            
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    string recurso = string.Empty;
                    if (dbCon.State == ConnectionState.Open)
                    {
                        //Logs.FullInfo("", "Conexion a BD Abierta", Usuario, 0, modulo, metodo); //Log Informativo
                        Logs.FullInfo("", ">>> WMSSP_Documento => Accion: ORM, DocNum: " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                        recurso = dbCon.Query<string>($"EXEC \"WMSSP_Documento\" @IdGuid = '', @Accion = 'ORM', @DocNum = { DocNum }").SingleOrDefault();

                        if (string.IsNullOrEmpty(recurso))
                        {
                            //Logs.FullInfo("", "\t >>> La OF " + DocNum + " NO TIENE asignado un recurso maquina", Usuario, 0, modulo, metodo); //Log Informativo
                            throw new Exception($"La OF {DocNum} NO TIENE asignado un recurso maquina");
                        }

                        //Logs.FullInfo("", "\t >>> La OF " + DocNum + " tiene asignada la Maquina " + recurso, Usuario, 0, modulo, metodo); //Log Informativo
                        return recurso;
                    }
                    else
                    {
                        Logs.FullInfo("", "Conexion a BD Cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                        throw new Exception($"La conexión no esta disponible para obtener el recurso maquina de la orden");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo

                throw new Exception(ex.InnerException == null ? ex.Message : ex.InnerException.ToString());
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    //Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                //Logs.FullInfo("Obtener Maquina de la Orden de Fabricacion " + DocNum, "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public string ObtenerTurno()
        {
            DateTime Hoy = DateTime.Now;
            TimeSpan Hora = DateTime.Now.TimeOfDay;
            string Turno = "1";
            DateTime Inicio1, Fin1, Inicio2, Fin2, Inicio3, Fin3;

            Inicio1 = Hoy.Date;
            Inicio1 = Inicio1.Add(TimeSpan.Parse(ObtenerInicioTurno("1", "I", Inicio1, null)));
            Fin1 = Hoy.Date; Fin1 = Fin1.Add(TimeSpan.Parse(ObtenerInicioTurno("1", "F", Fin1, null)));

            Inicio2 = Hoy.Date; Inicio2 = Inicio2.Add(TimeSpan.Parse(ObtenerInicioTurno("2", "I", Inicio2, null)));
            Fin2 = Hoy.Date; Fin2 = Fin2.Add(TimeSpan.Parse(ObtenerInicioTurno("2", "F", Fin2, null)));

            if (Hoy >= Inicio1 && Hoy <= Fin1)
            {
                Turno = "1";
            }
            else if (Hoy >= Inicio2 && Hoy <= Fin2)
            {
                Turno = "2";
            }
            else
            {
                if (Hora >= TimeSpan.Parse(ObtenerInicioTurno("3", "I", Hoy, null)))
                {
                    Inicio3 = Hoy.Date; Inicio3 = Inicio3.Add(TimeSpan.Parse(ObtenerInicioTurno("3", "I", Inicio3, null)));
                    Fin3 = Hoy.Date.AddDays(1); ; Fin3 = Fin3.Add(TimeSpan.Parse(ObtenerInicioTurno("3", "F", Fin3, null)));
                    if (Hoy >= Inicio3 && Hoy <= Fin3)
                    {
                        Turno = "3";
                    }
                }
                else
                {
                    Inicio3 = Hoy.Date.AddDays(-1); Inicio3 = Inicio3.Add(TimeSpan.Parse(ObtenerInicioTurno("3", "I", Inicio3, null)));
                    Fin3 = Hoy.Date; Fin3 = Fin3.Add(TimeSpan.Parse(ObtenerInicioTurno("3", "F", Fin3, null)));
                    if (Hoy >= Inicio3 && Hoy <= Fin3)
                    {
                        Turno = "3";
                    }
                }

            }
            return Turno;
        }

        public string ObtenerInicioTurno(string Turno, string Tipo, DateTime Fecha, SAPbobsCOM.Company _oCompany = null)
        {
            IDbConnection dbCon = null;
            string Val = null;
            try
            {
                if (_oCompany == null)
                {
                    dbCon = new SqlConnection(CadReg);
                    dbCon.Open();
                    if (dbCon.State == ConnectionState.Open)
                    {
                        switch (Tipo)
                        {
                            case "I":
                                Val = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 10, @Subaccion = 1,  @Turno = { int.Parse(Turno) } ").SingleOrDefault();
                                break;
                            case "F":
                                Val = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 10, @Subaccion = 2, @Turno = { int.Parse(Turno) } ").SingleOrDefault();
                                break;
                            default:
                                break;
                        }
                    }
                }
                else {
                    SAPbobsCOM.UserTable oUT = _oCompany.UserTables.Item("evt_TURNOS");
                    if (oUT.GetByKey(Turno))
                    {
                        switch (Tipo)
                        {
                            case "I":
                                Val = oUT.UserFields.Fields.Item("U_Inicio").Value.ToString();
                                break;
                            case "F":
                                Val = oUT.UserFields.Fields.Item("U_Fin").Value.ToString();
                                break;
                        }
                    }
                }

                if ((int)Fecha.DayOfWeek == 6)
                {
                    DateTime Fecha2 = Fecha.Add(TimeSpan.Parse(Val)).AddMinutes(-30);
                    Val = Fecha2.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                }
                return Val;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException == null ? ex.Message : ex.InnerException.ToString());
            }
            finally {
                if (dbCon != null) {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool obtenerFIFOLote(ref List<Fifo> result, string lote, string articulo, List<Etiqueta> lineas)
        {
            IDbConnection dbCon = null;
            List<Fifo> ListaFifo = new List<Fifo>();
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    string lotes = string.Empty;
                    foreach (Etiqueta e in lineas)
                    {
                        if (string.IsNullOrEmpty(lotes))
                            lotes = e.DistNumber;
                        else
                            lotes += "," + e.DistNumber;
                    }
                    dbCon.Open();
                    ListaFifo = dbCon.Query<Fifo>($"EXEC \"WMSSP_Fifo\" '{lote}','{articulo}', '{lotes}'").ToList();
                    result = ListaFifo.Where(x => x.Etiqueta != 'N' || x.Indice == 1).ToList();
                    //index = dbCon.Query<int>($"EXEC \"WMSSP_Fifo\" '{lote}','{articulo}','{lotes}'").SingleOrDefault();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }
        
        public bool ObtenerNivelesUbicacion(ref List<Produccion.SubAlmacen> ListaSL1, ref List<Produccion.Linea> ListaSL2, ref List<Produccion.Fila> ListaSL3, string WhsCode)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ListaSL1 = dbCon.Query<Produccion.SubAlmacen>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 9, @Subaccion = 1, @Almacen = '{ WhsCode }' ").ToList(); //query1
                    ListaSL2 = dbCon.Query<Produccion.Linea>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 9, @Subaccion = 2, @Almacen = '{ WhsCode }' ").ToList(); //query2
                    ListaSL3 = dbCon.Query<Produccion.Fila>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 9, @Subaccion = 3, @Almacen = '{ WhsCode }' ").ToList(); //query3
                }
                return true;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool ListaBorradores(int turno, ref List<Produccion.Borrador> listaBorrador)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Lista de Borradores", "", Usuario, 3, "", ""); //Log Informativo

            IDbConnection dbCon = null;
            try
            {
                string usr = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    Logs.FullInfo("", "Listar Borradores: WMSSP_Borradores Accion = 1, Usuario = " + usr, Usuario, 0, modulo, metodo); //Log Informativo
                    listaBorrador = dbCon.Query<Produccion.Borrador>($"EXEC \"WMSSP_Borradores\" @Accion = 1, @Usuario = '{ usr }' ").ToList();

                    //---
                    if (listaBorrador.Count > 0)
                    {
                        foreach (Produccion.Borrador bR in listaBorrador)
                        {
                            Logs.FullInfo("", "Borrador del Usuario " + usr + ": Orden de Fabricacion => " + bR.OF + 
                                                "- Total Etiquetas => " + bR.TE + 
                                                "- 1er Turno => " + bR.P + 
                                                "- 2do Turno => " + bR.S + 
                                                "- 3er Turno => " + bR.T
                                                , Usuario, 0, modulo, metodo); //Log Informativo
                        }
                    }
                    else
                    {
                        Logs.FullInfo("", "No hay registros en Borradores del Usuario: " + usr, Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    //-----

                }
                return true;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cerrar conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lista de Borradores", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool RecuperarBorrador(int order, int turno, ref List<Produccion.EmisionRecibo> Lineas)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Etiquetas del Borrador", "", Usuario, 3, "", ""); //Log Informativo
            IDbConnection dbCon = null;
            string usr = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            
            try
            {
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Etiquetas de Borrador: WMSSP_Borradores Accion = 2, Usuario = " + usr + ", OrdenFab = " + order + ", Turno = " + turno, Usuario, 0, modulo, metodo); //Log Informativo
                    Lineas = dbCon.Query<Produccion.EmisionRecibo>($"EXEC \"WMSSP_Borradores\" @Accion = 2, @Usuario = '{ usr }', @OrdenFab = { order }, @Turno = { turno }").ToList();

                    //---
                    if (Lineas.Count > 0)
                    {
                        foreach (Produccion.EmisionRecibo eR in Lineas)
                        {
                            Logs.FullInfo("", "Etiqueta obtenida: Lote => " + eR.DistNumber + 
                                                "- Articulo => " + eR.ItemCode + 
                                                "- Cantidad => " + eR.OnHandQty + 
                                                "- OF => " + eR.OF + 
                                                "- Turno => " + eR.U_TurnoRecibo + 
                                                "- Fecha => " + eR.U_FechaRecibo +
                                                "- Almacen => " + eR.Warehouse +
                                                "- Master => " + eR.EtqMaster +
                                                "- Tipo Reg => " + eR.TipoReg +
                                                "- Tipo Lectura => " + eR.TipoLectura
                                                , Usuario, 0, modulo, metodo); //Log Informativo
                        }
                    }
                    else
                    {
                        Logs.FullInfo("", "No se obtuvieron etiquetas en Borrador del Usuario: " + usr + " de la Orden: " + order + " del Turno: " + turno, Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    //-----
                }

                return true;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cerrar conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Etiquetas del Borrador", "", Usuario, 4, "", ""); //Log Informativo
            }
        }
        

        //Modulo Traspaso Manual
        public bool ObtenerClienteAlmacen(ref List<TrsManual.ClienteAlmacen> ListCliAlm) 
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    if (dbCon.State == ConnectionState.Open)
                    {
                        ListCliAlm = dbCon.Query<TrsManual.ClienteAlmacen>($"EXEC \"WMSSP_ClienteAlmacen\"").ToList();
                        return true;
                    }
                    else
                    {
                        throw new Exception("La conexión no esta disponible");
                    }   
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally {
                if (dbCon != null) {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        //Traspaso Manual - Validación de Contendor Original o Contendor Original Desechable
        public int ContenedorOriginal(ref StringBuilder sinContenedor, ref StringBuilder LotesSC, ref string Archivos, List<Detalle> Lineas)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Contenedor Original", "", Usuario, 3, "", ""); //Log Informativo

            IDbConnection dbCon;
            List<TrsManual.ContenedorOriginal> ArtsContenedor;
            List<TrsManual.NumeröParte> NumeroParte;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    //**************************************************************************************
                    //Se obtienen Datos con los articulos y dispositivos de las etiquetas leidas
                    string listArticulos = CadenaArticulos(Lineas);
                    string listDispositivos = CadenaDispositivo(Lineas);

                    Logs.FullInfo("", "Obtener datos de Contenedor Original: WMSSP_WMSConsultar Accion = 11, Subaccion = 1, CadenaArticulos = (" + listArticulos + "), CadenaDispositivos = (" + listDispositivos + ")", Usuario, 0, modulo, metodo); //Log Informativo
                    ArtsContenedor = dbCon.Query<TrsManual.ContenedorOriginal>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 11, @Subaccion = 1, @CadenaArticulos = '{listArticulos}', @CadenaDispositivos = '{listDispositivos}' ").ToList();
                    foreach (TrsManual.ContenedorOriginal cO in ArtsContenedor)
                    {
                        Logs.FullInfo("", "Obtener Contenedor Original: Code => " + cO.Code + "- U_Dispositivo => " + cO.U_Dispositivo + "- U_Original => " + cO.U_Original + "- U_OrigDesechable => " + cO.U_OrigDesechable, Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    //**************************************************************************************
                    //Validar los Lotes verificando si tienen Contenedor Original
                    //ValidarContenedor(respuesta de la funcion, Datos de la consulta de Contendores, Lineas de Etiquetas Leidas);
                    ValidarContenedor(ref sinContenedor, ArtsContenedor, Lineas);

                    //Si alguna de las etiquetas NO tiene Contendor Original
                    //NO se sigue procesando, NO se crea Transferencia
                    //Se crea el archivo con los Lotes de Contenedor NO Original
                    if (sinContenedor.Length > 0)
                    {
                        //Se obtienen Numeros de Parte de las Etiquetas 
                        Logs.FullInfo("", "Obtener datos de Numero de Partes: WMSSP_WMSConsultar Accion = 11, Subaccion = 2, CadenaArticulos = (" + listArticulos + ")", Usuario, 0, modulo, metodo); //Log Informativo
                        NumeroParte = dbCon.Query<TrsManual.NumeröParte>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 11, @Subaccion = 2, @CadenaArticulos = '{listArticulos}' ").ToList();
                        foreach (TrsManual.NumeröParte nP in NumeroParte)
                        {
                            Logs.FullInfo("", "Numero de Parte: Parte => " + nP.Parte, Usuario, 0, modulo, metodo); //Log Informativo
                        }

                        string User = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                        LotesSC.Clear(); //Almacenamiento de los Lotes leidos que no son de contenedor original
                        string[] listaLotesSC = sinContenedor.ToString().Split(',');
                        string Archivo = "";

                        //Recorrido de los Numero de Parte
                        for (int i = 0; i < NumeroParte.Count; i++)
                        {
                            Logs.FullInfo("", "Numero de Parte a Procesar: Parte => " + NumeroParte[i].Parte, Usuario, 0, modulo, metodo); //Log Informativo
                                                                                                                                           //Recorrido de Lineas de Etiquetas a Procesar
                            for (int j = 0; j < Lineas.Count; j++)
                            {
                                //Verificar el Numero de Parte y el ItemCode de la Etiqueta a Procesar
                                if (NumeroParte[i].Parte == Lineas[j].ItemCode)
                                {
                                    //Recorrido de los Lotes SIN Contenedor Original
                                    for (int k = 0; k < listaLotesSC.Length; k++)
                                    {
                                        //Verificar el Lote de la etiqueta y el Lote de la Lita de Lotes SIN Contenedor Original
                                        if (Lineas[j].DistNumber == listaLotesSC[k].Trim())
                                        {
                                            //Si el Lote de Etiqueta es igual al Lote de la Lista de Lotes SIN Contenedor Original
                                            //Se agreara a los Lotes a grabar por Numero de Parte
                                            if (LotesSC.Length == 0)
                                            {
                                                LotesSC.Append(listaLotesSC[k].Trim());
                                            }
                                            else
                                            {
                                                if (!LotesSC.ToString().Contains(listaLotesSC[k].Trim()))
                                                {
                                                    LotesSC.Append(", ").Append(listaLotesSC[k].Trim());
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //Terminando de Procesar el numero de parte
                            //Se graba el archivo txt de los Lotes Sin Contenedor Original
                            Logs.FullInfo("", "Escribiendo archivo: Numero de Parte: " + NumeroParte[i].Parte + " - Usuario: " + User + " - Lotes a registrar: " + LotesSC, Usuario, 0, modulo, metodo); //Log Informativo

                            if (Logs.EscribirContenedor(LotesSC.ToString().Split(','), User, NumeroParte[i].Parte, ref Archivo))
                            {
                                if (Archivos == "")
                                {
                                    Archivos = "Archivo(s) creado(s) correctamente: \r\n" + Archivo + "\r\n";
                                }
                                else
                                {
                                    Archivos = Archivos + "" + Archivo + "\r\n";
                                }
                            }
                            else
                            {
                                Archivos = Archivos + "; Error en la creación de Archivo: " + Archivo + "\r\n";
                            }
                            //Logs.EscribirContenedor(LotesSC.ToString().Split(','), User, NumeroParte[i].Parte);
                            LotesSC.Clear(); //Almacenamiento de los Lotes leidos que no son de contenedor original
                        }

                        return 1;
                    }
                    //SI Todos los Lotes son de Contendor Original, se sigue el Procesamiento de la Transferencia
                    else
                    {
                        Logs.FullInfo("", "Todos los Lotes son de Contenedor Original", Usuario, 0, modulo, metodo); //Log Informativo
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                sinContenedor.Append(ex.Message);

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return -1;
            }
            finally
            {
                Logs.FullInfo("Obtener Contenedor Original", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        private bool ValidarContenedor(ref StringBuilder builder, List<TrsManual.ContenedorOriginal> listArticulos, List<Detalle> Lineas)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Valida Contenedor", "", Usuario, 3, "", ""); //Log Informativo

            builder.Clear(); //Almacenamiento de los Lotes leidos que no son de contenedor original

            //1 - Recorrido de Lineas de Etiquetas Leidas a Procesar
            for (int i = 0; i < Lineas.Count; i++)
            {
                Logs.FullInfo("", "Lote a Verificar: " + Lineas[i].DistNumber + " - Articulo: " + Lineas[i].ItemCode + " - Dispositivo: " + Lineas[i].Dispositivo, Usuario, 0, modulo, metodo);
                //2 - Recorrido de los Datos obtenidos de la consulta por Articulos y Dispositivos de las Etiquetas Leidas
                for (int j = 0; j < listArticulos.Count; j++)
                {
                    //3 - Verificar Articulo, Dispositivo
                    if (listArticulos[j].Code == Lineas[i].ItemCode && listArticulos[j].U_Dispositivo == Lineas[i].Dispositivo)
                    {
                        Logs.FullInfo("", "Contenedor a Verificar: " + " - Articulo: " + listArticulos[j].Code + " - Dispositivo: " + listArticulos[j].U_Dispositivo, Usuario, 0, modulo, metodo); //Log Informativo
                        //3.1 - Verificar Contenedor Original, Contenedor Original Desechable
                        if (listArticulos[j].U_Original == 'N' && listArticulos[j].U_OrigDesechable == 'N')
                        {
                            //Si NO tiene Contenedor Original ó Contenedor Original Desechable, es Contenedor Alterno, se agrega a Lista de Lotes Contenedor NO Original
                            Logs.FullInfo("", "L o t e   S I N   C o n t e n e d o r   O r i g i n a l: " + Lineas[i].DistNumber + " --- Contendor Original: " + listArticulos[j].U_Original + " - Contendor Original Desechable: " + listArticulos[j].U_OrigDesechable, Usuario, 0, modulo, metodo); //Log Informativo
                            if (builder.Length == 0)
                            {
                                builder.Append(Lineas[i].DistNumber);
                            }
                            else
                            {
                                if (!builder.ToString().Contains(Lineas[i].DistNumber))
                                {
                                    builder.Append(", ").Append(Lineas[i].DistNumber);
                                }
                            }
                        }
                        else
                        {
                            Logs.FullInfo("", "Lote con Contenedor Original: " + Lineas[i].DistNumber + " --- Contendor Original: " + listArticulos[j].U_Original + " - Contendor Original Desechable: " + listArticulos[j].U_OrigDesechable, Usuario, 0, modulo, metodo); //Log Informativo
                        }
                    }
                }
            }

            Logs.FullInfo("Valida Contenedor", "", Usuario, 4, "", ""); //Log Informativo
            return true;
        }

        private string CadenaArticulos(List<Detalle> Lineas) {
            StringBuilder condicion = new StringBuilder();
            foreach (Detalle ln in Lineas) {
                if (condicion.Length == 0)
                {
                    condicion.Append($"''{ln.ItemCode}''");
                }
                else
                {
                    if (!condicion.ToString().Contains(ln.ItemCode))
                    {
                        condicion.Append($",''{ln.ItemCode}''");
                    }
                }
            }
            return condicion.ToString();
        }

        private string CadenaDispositivo(List<Detalle> Lineas) {
            StringBuilder condicion = new StringBuilder();
            foreach (Detalle ln in Lineas) {
                if (condicion.Length == 0)
                {
                    condicion.Append($"''{ln.Dispositivo}''");
                }
                else
                {
                    if (!condicion.ToString().Contains(ln.Dispositivo))
                    {
                        condicion.Append($",''{ln.Dispositivo}''");
                    }
                }                    
            }
            return condicion.ToString();
        }
        
        //*****************************************************************************/

        //Remisión - Validación de Contendor Original o Contendor Original Desechable
        public int ContenedorOriginalRemision(ref StringBuilder sinContenedor, ref StringBuilder LotesSC, ref string Archivos, List<DetalleR> Lineas)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Contenedor Original -- Remision", "", Usuario, 3, "", ""); //Log Informativo

            IDbConnection dbCon;
            List<Remision.ContenedorOriginalR> ArtsContenedor;
            List<Remision.NumeröParteR> NumeroParte;

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    //Se obtienen Datos con los articulos y dispositivos de las etiquetas leidas
                    Logs.FullInfo("", "Obtener datos de Contenedor Original: WMSSP_WMSConsultar Accion = 11, Subaccion = 1, CadenaArticulos = (" + CadenaArticulosRemision(Lineas) + "), CadenaDispositivos = (" + CadenaDispositivoRemision(Lineas) + ")", Usuario, 0, modulo, metodo); //Log Informativo
                    ArtsContenedor = dbCon.Query<Remision.ContenedorOriginalR>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 11, @Subaccion = 1, @CadenaArticulos = '{ CadenaArticulosRemision(Lineas) }', @CadenaDispositivos = '{ CadenaDispositivoRemision(Lineas) }' ").ToList();

                    //---
                    foreach (Remision.ContenedorOriginalR cO in ArtsContenedor)
                    {
                        Logs.FullInfo("", "Contenedor Original: Code => " + cO.Code + "- U_Dispositivo => " + cO.U_Dispositivo + "- U_Original => " + cO.U_Original + "- U_OrigDesechable => " + cO.U_OrigDesechable, Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    //-----

                    //Validar los Lotes verificando si tienen Contenedor Original
                    //ValidarContenedor(respuesta de la funcion, Datos de la consulta de Contendores, Lineas de Etiquetas Leidas);
                    ValidarContenedorRemision(ref sinContenedor, ArtsContenedor, Lineas);

                    //*********************************************************************************************************************
                    //*********************************************************************************************************************
                    //Si alguna de las etiquetas NO tiene Contendor Original
                    //NO se sigue procesando, NO se crea Transferencia
                    //Se crea el archivo con los Lotes de Contenedor NO Original
                    if (sinContenedor.Length > 0)
                    {
                        //Se obtienen Numeros de Parte de las Etiquetas 
                        Logs.FullInfo("", "Obtener datos de Numero de Partes: WMSSP_WMSConsultar Accion = 11, Subaccion = 2, CadenaArticulos = (" + CadenaArticulosRemision(Lineas) + ")", Usuario, 0, modulo, metodo); //Log Informativo
                        NumeroParte = dbCon.Query<Remision.NumeröParteR>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 11, @Subaccion = 2, @CadenaArticulos = '{CadenaArticulosRemision(Lineas)}' ").ToList();

                        string User = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                        LotesSC.Clear(); //Almacenamiento de los Lotes leidos que no son de contenedor original
                        string[] listaLotesSC = sinContenedor.ToString().Split(',');
                        string Archivo = "";

                        //---
                        foreach (Remision.NumeröParteR nP in NumeroParte)
                        {
                            Logs.FullInfo("", "Numero de Parte: Parte => " + nP.Parte, Usuario, 0, modulo, metodo); //Log Informativo
                        }
                        //-----

                        //Recorrido de los Numero de Parte
                        for (int i = 0; i < NumeroParte.Count; i++)
                        {
                            Logs.FullInfo("", "Numero de Parte a Procesar: Parte => " + NumeroParte[i].Parte, Usuario, 0, modulo, metodo); //Log Informativo
                                                                                                                                           //Recorrido de Lineas de Etiquetas a Procesar
                            for (int j = 0; j < Lineas.Count; j++)
                            {
                                //Verificar el Numero de Parte y el ItemCode de la Etiqueta a Procesar
                                if (NumeroParte[i].Parte == Lineas[j].ItemCode)
                                {
                                    //Recorrido de los Lotes SIN Contenedor Original
                                    for (int k = 0; k < listaLotesSC.Length; k++)
                                    {
                                        //Verificar el Lote de la etiqueta y el Lote de la Lita de Lotes SIN Contenedor Original
                                        if (Lineas[j].DistNumber == listaLotesSC[k].Trim())
                                        {
                                            //Si el Lote de Etiqueta es igual al Lote de la Lista de Lotes SIN Contenedor Original
                                            //Se agreara a los Lotes a grabar por Numero de Parte
                                            if (LotesSC.Length == 0)
                                            {
                                                LotesSC.Append(listaLotesSC[k].Trim());
                                            }
                                            else
                                            {
                                                if (!LotesSC.ToString().Contains(listaLotesSC[k].Trim()))
                                                {
                                                    LotesSC.Append(", ").Append(listaLotesSC[k].Trim());
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //Terminando de Procesar el numero de parte
                            //Se graba el archivo txt de los Lotes Sin Contenedor Original
                            Logs.FullInfo("", "Escribiendo archivo: Numero de Parte: " + NumeroParte[i].Parte + " - Usuario: " + User + " - Lotes a registrar: " + LotesSC, Usuario, 0, modulo, metodo); //Log Informativo

                            if (Logs.EscribirContenedorRemision(LotesSC.ToString().Split(','), User, NumeroParte[i].Parte, ref Archivo))
                            {
                                if (Archivos == "")
                                {
                                    Archivos = "Archivo(s) creado(s) correctamente: \r\n" + Archivo + "\r\n";
                                }
                                else
                                {
                                    Archivos = Archivos + "" + Archivo + "\r\n";
                                }
                            }
                            else
                            {
                                Archivos = Archivos + "; Error en la creación de Archivo: " + Archivo + "\r\n";
                            }
                            //Logs.EscribirContenedor(LotesSC.ToString().Split(','), User, NumeroParte[i].Parte);
                            LotesSC.Clear(); //Almacenamiento de los Lotes leidos que no son de contenedor original
                        }

                        return 1;
                    }
                    //SI Todos los Lotes son de Contendor Original, se sigue el Procesamiento de la Transferencia
                    else
                    {
                        Logs.FullInfo("", "Todos los Lotes son de Contenedor Original", Usuario, 0, modulo, metodo); //Log Informativo
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                sinContenedor.Append(ex.Message);

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return -1;
            }
            finally
            {
                Logs.FullInfo("Obtener Contenedor Original", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        private bool ValidarContenedorRemision(ref StringBuilder builder, List<Remision.ContenedorOriginalR> listArticulos, List<DetalleR> Lineas)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Valida Contenedor", "", Usuario, 3, "", ""); //Log Informativo

            builder.Clear(); //Almacenamiento de los Lotes leidos que no son de contenedor original

            //1 - Recorrido de Lineas de Etiquetas Leidas a Procesar
            for (int i = 0; i < Lineas.Count; i++)
            {
                Logs.FullInfo("", "Lote a Verificar: " + Lineas[i].DistNumber + " - Articulo: " + Lineas[i].ItemCode + " - Dispositivo: " + Lineas[i].Dispositivo, Usuario, 0, modulo, metodo); //Log Informativo
                //2 - Recorrido de los Datos obtenidos de la consulta por Articulos y Dispositivos de las Etiquetas Leidas
                for (int j = 0; j < listArticulos.Count; j++)
                {
                    //3 - Verificar Articulo, Dispositivo
                    if (listArticulos[j].Code == Lineas[i].ItemCode && listArticulos[j].U_Dispositivo == Lineas[i].Dispositivo ) //Validar Articulo, Contenedor Original, Contenedor Original Desechable
                    {
                        Logs.FullInfo("", "Contenedor a Verificar: " + " - Articulo: " + listArticulos[j].Code + " - Dispositivo: " + listArticulos[j].U_Dispositivo, Usuario, 0, modulo, metodo); //Log Informativo
                        //3.1 - Verificar Contenedor Original, Contenedor Original Desechable
                        if (listArticulos[j].U_Original == 'N' && listArticulos[j].U_OrigDesechable == 'N')
                        {
                            //Si NO tiene Contenedor Original ó Contenedor Original Desechable, es Contenedor Alterno, se agrega a Lista de Lotes Contenedor NO Original
                            Logs.FullInfo("", "L o t e   S I N   C o n t e n e d o r   O r i g i n a l: " + Lineas[i].DistNumber + " --- Contendor Original: " + listArticulos[j].U_Original + " - Contendor Original Desechable: " + listArticulos[j].U_OrigDesechable, Usuario, 0, modulo, metodo); //Log Informativo
                            if (builder.Length == 0)
                            {
                                builder.Append(Lineas[i].DistNumber);
                            }
                            else
                            {
                                if (!builder.ToString().Contains(Lineas[i].DistNumber))
                                {
                                    builder.Append(", ").Append(Lineas[i].DistNumber);
                                }
                            }
                        }
                        else 
                        {
                            Logs.FullInfo("", "Lote con Contenedor Original: " + Lineas[i].DistNumber + " --- Contendor Original: " + listArticulos[j].U_Original + " - Contendor Original Desechable: " + listArticulos[j].U_OrigDesechable, Usuario, 0, modulo, metodo); //Log Informativo
                        }
                    }
                }
            }

            Logs.FullInfo("", "Valida Contenedor: Lotes SIN Contenedor Original " + builder, Usuario, 0, modulo, metodo); //Log Informativo
            Logs.FullInfo("Valida Contenedor", "", Usuario, 4, "", ""); //Log Informativo
            return true;
        }

        private string CadenaArticulosRemision(List<DetalleR> Lineas)
        {
            StringBuilder condicion = new StringBuilder();
            foreach (DetalleR ln in Lineas)
            {
                if (condicion.Length == 0)
                {
                    condicion.Append($"''{ln.ItemCode}''");
                }
                else
                {
                    if (!condicion.ToString().Contains(ln.ItemCode))
                    {
                        condicion.Append($",''{ln.ItemCode}''");
                    }
                }
            }
            return condicion.ToString();
        }

        private string CadenaDispositivoRemision(List<DetalleR> Lineas)
        {
            StringBuilder condicion = new StringBuilder();
            foreach (DetalleR ln in Lineas)
            {
                if (condicion.Length == 0)
                {
                    condicion.Append($"''{ln.Dispositivo}''");
                }
                else 
                {
                    if (!condicion.ToString().Contains(ln.Dispositivo))
                    {
                        condicion.Append($",''{ln.Dispositivo}''");
                    }
                }                    
            }
            return condicion.ToString();
        }

        //***************************************************************************************/
        public IDbConnection conSql() {
            IDbConnection dbCon = new SqlConnection(CadReg);
            dbCon.Open();
            return dbCon;
        }

        //Obtener los numero de Parte de las Etiquetas de Traspaleo Procesadas -- 14-01-2021
        public List<Recuperar.Partes> NumPartes(string Etiquetas)
        {
            IDbConnection dbCon = null;
            List<Recuperar.Partes> ln = new List<Recuperar.Partes>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Numero de Partes de Etiquetas de Traspaleo", "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //string query = $"SELECT DISTINCT \"U_ItemCode\" NoParte FROM \"@EVT_ETQSPT\" WHERE \"Name\" IN ({Etiquetas}) AND \"U_Cancel\" = 'N'";
                    //ln = dbCon.Query<Recuperar.Partes>(query).ToList();
                    //Logs.FullInfo("", "Obtener Numero de Parte", Usuario, 0, modulo, metodo); //Log Informativo
                    //AKSYS_DB => WMSSP_WMSConsultar
                    Logs.FullInfo("", "Obtener Numero de Parte: WMSSP_WMSConsultar Accion = 12, ListaEtiquetas = (" + Etiquetas + ")", Usuario, 0, modulo, metodo); //Log Informativo
                    ln = dbCon.Query<Recuperar.Partes>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 12, @ListaEtiquetas = '{ Etiquetas }' ").ToList();

                    //---
                    foreach (Recuperar.Partes nP in ln)
                    {
                        Logs.FullInfo("", "Numero de Parte Encontrado: " + nP.NoParte, Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    //-----
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Numero de Partes de Etiquetas de Traspaleo", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
            return ln;
        }

        //Verificar si la etiqueta existe en un borrador - IUO 30/08/2020
        public bool EtqBorrador(ref List<BorradorEtiqueta> lista, string Lote, string Articulo)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Verificar Etiqueta en Borrador", "", Usuario, 3, "", ""); //Log Informativo

            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Verificar Borrador: WMSSP_WMSAgregar Accion = 9, Lote = " + Lote + ", Articulo = " + Articulo, Usuario, 0, modulo, metodo); //Log Informativo
                    lista = dbCon.Query<BorradorEtiqueta>($"EXEC \"WMSSP_WMSAgregar\" @Accion = 9, @Lote = '{ Lote }', @Articulo = '{ Articulo }' ").ToList();

                    //---
                    if (lista.Count > 0)
                    {
                        foreach (BorradorEtiqueta bR in lista)
                        {
                            Logs.FullInfo("", "Registro de Borrador " +
                                                "- Usuario => " + bR.Usuario +
                                                "- Fecha => " + bR.FechaCreacion +
                                                "- Tipo Lectura => " + bR.TipoLectura + 
                                                "- Etq Master => " + bR.EtqMaster 
                                                , Usuario, 0, modulo, metodo); //Log Informativo
                        }
                    }
                    else
                    {
                        Logs.FullInfo("", "No hay registros en Borradores de la etiqueta: " + Lote, Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    //-----

                }
                return true;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cerrar conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Verificar Etiqueta en Borrador", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //**********************************************************************//
        //**********************************************************************//
        //Codigo que al Parecer NO se ocupa
        //Al parecer NO se ocupa
        public bool ActualizarEstatus(ref string msj, List<Produccion.EstatusOrden> Estatus, string Estate, string Usr)
        {
            IDbConnection dbCon = null;
            string _LoteAcme = string.Empty;
            string _LoteSap = string.Empty;
            string _LotesAm = string.Empty;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (Produccion.EstatusOrden L in Estatus)
                    {
                        if (string.IsNullOrEmpty(_LoteAcme) && L.Tipo == "E")
                            _LoteAcme = "'" + L.Name + "'";
                        else if (!string.IsNullOrEmpty(_LoteAcme) && L.Tipo == "E")
                            _LoteAcme += ",'" + L.Name + "'";

                        if (string.IsNullOrEmpty(_LoteSap) && L.Tipo == "S")
                            _LoteSap = "'" + L.Name + "'";
                        else if (!string.IsNullOrEmpty(_LoteSap) && L.Tipo == "S")
                            _LoteSap += ",'" + L.Name + "'";

                        if (string.IsNullOrEmpty(_LotesAm) && L.Tipo == "A")
                            _LotesAm = "'" + L.Name + "'";
                        else if (!string.IsNullOrEmpty(_LotesAm) && L.Tipo == "A")
                            _LotesAm += ",'" + L.Name + "'";
                    }
                    if (_LoteAcme.Length > 0)
                        dbCon.Execute($"UPDATE \"@EVT_ETQSPT\" SET \"U_stCal\" = '{Estate}' WHERE NAME IN (" + _LoteAcme + ")");
                    if (_LoteSap.Length > 0)
                        dbCon.Execute($"UPDATE \"OBTN\" SET \"Status\" = '{(Estate == "X" ? '2' : '0')}' WHERE DistNumber IN (" + _LoteSap + ")");
                    if (_LotesAm.Length > 0)
                    {
                        dbCon.Execute($"UPDATE \"@EVT_ETQSPT\" SET \"U_stCal\" = '{Estate}' WHERE NAME IN (" + _LotesAm + ")");
                        dbCon.Execute($"UPDATE \"OBTN\" SET \"Status\" = '{(Estate == "X" ? '2' : '0')}' WHERE DistNumber IN (" + _LotesAm + ")");
                    }

                    //dbCon.Execute($"UPDATE \"@EVT_ETQSPT\" SET \"U_stCal\" = '{Estate}' WHERE NAME = '{L.Name}'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msj = ex.Message;
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool ObtenerDetalleCalidad(ref List<Produccion.EstatusOrden> LineasOrd, string lote, string item)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    LineasOrd = dbCon.Query<Produccion.EstatusOrden>($"EXEC \"WMSSP_DetalleCalidad\" '{lote}','{item}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        //Traspaleo --- Este ya no se utiliza, se creará archivo txt en lugar de guardar las etiquetas -- 14-01-2021
        /*public bool EtiquetasTraspaleo(List<Traspaleo> Traspaleo, ref string result, string Etqs) //Al parecer no se Utiliza
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;

            Logs.FullInfo("Traspaleo de Etiquetas", "", Usuario, 3, modulo, metodo); //Log Informativo
            Logs.FullInfo("", "Etiquetas: " + Etqs, Usuario, 0, modulo, metodo); //Log Informativo    
            
            try
            {
                int inserts = 0;
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    TablaEvt rows = new TablaEvt();
                    rows = dbCon.Query<TablaEvt>($"SELECT TOP 1 COUNT(*) Etqs, U_Etiqueta FROM [@EVT_TRASPALEO] WHERE U_Etiqueta IN ({Etqs}) GROUP BY U_Etiqueta").FirstOrDefault();
                    Logs.FullInfo("", "Verificar si Alguna Etiqueta ya fue Registrada: SELECT TOP 1 COUNT(*) Etqs, U_Etiqueta FROM [@EVT_TRASPALEO] WHERE U_Etiqueta IN (" + Etqs + ") GROUP BY U_Etiqueta", Usuario, 0, modulo, metodo); //Log Informativo 

                    if (rows != null)
                    {
                        Logs.FullInfo("", "La etiqueta " + rows.U_Etiqueta + " ya fue AGREGADA", Usuario, 0, modulo, metodo); //Log Informativo 
                        result = "La etiqueta " + rows.U_Etiqueta + " ya fue AGREGADA";
                        return false;
                    }
                    else 
                    {
                        for (int i = 0; i < Traspaleo.Count; i++)
                        {
                            inserts += dbCon.Execute($"INSERT INTO [@EVT_TRASPALEO] (Name, U_Etiqueta) VALUES ('{Traspaleo[i].U_ItemCode}','{Traspaleo[i].Name}')");
                            Logs.FullInfo("", "Registrar Etiquetas en Traspaleo: Name => " + Traspaleo[i].U_ItemCode + "-- U_Etiqueta => " + Traspaleo[i].Name, Usuario, 0, modulo, metodo); //Log Informativo 
                        }
                    }                    
                }
                if (inserts > 0)
                {
                    Logs.FullInfo("", "Etiquetas Registradas: " + inserts, Usuario, 0, modulo, metodo); //Log Informativo 
                    result = string.Empty;
                    return true;
                }
                else
                {
                    Logs.FullInfo("", "Se insertaron: " + inserts + " registros de: " + Traspaleo.Count, Usuario, 0, modulo, metodo); //Log Informativo 
                    result = $"Se insertaron: {inserts} registros de: {Traspaleo.Count}";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                result = ex.Message;

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cierre de Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Traspaleo de Etiquetas", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
        }*/

        public DateTime ObtenerFecha(SAPbobsCOM.Company _oCompany)
        {
            DateTime Hoy = DateTime.Now;
            TimeSpan Hora = DateTime.Now.TimeOfDay;
            DateTime FechaTurno = DateTime.Now;
            DateTime Inicio1, Fin1, Inicio2, Fin2, Inicio3, Fin3;

            Inicio1 = Hoy.Date; Inicio1 = Inicio1.Add(TimeSpan.Parse(ObtenerInicioTurno("1", "I", Inicio1, _oCompany)));
            Fin1 = Hoy.Date; Fin1 = Fin1.Add(TimeSpan.Parse(ObtenerInicioTurno("1", "F", Fin1, _oCompany)));
            Inicio2 = Hoy.Date; Inicio2 = Inicio2.Add(TimeSpan.Parse(ObtenerInicioTurno("2", "I", Inicio2, _oCompany)));
            Fin2 = Hoy.Date; Fin2 = Fin2.Add(TimeSpan.Parse(ObtenerInicioTurno("2", "F", Fin2, _oCompany)));

            if (Hoy >= Inicio1 && Hoy <= Fin1)
            {
                FechaTurno = DateTime.Now;
            }
            else if (Hoy >= Inicio2 && Hoy <= Fin2)
            {
                FechaTurno = DateTime.Now;
            }
            else
            {
                if (Hora >= TimeSpan.Parse(ObtenerInicioTurno("3", "I", Hoy, _oCompany)))
                {
                    Inicio3 = Hoy.Date; Inicio3 = Inicio3.Add(TimeSpan.Parse(ObtenerInicioTurno("3", "I", Inicio3, _oCompany)));
                    Fin3 = Hoy.Date.AddDays(1); ; Fin3 = Fin3.Add(TimeSpan.Parse(ObtenerInicioTurno("3", "F", Fin3, _oCompany)));
                    if (Hoy >= Inicio3 && Hoy <= Fin3)
                    {
                        FechaTurno = DateTime.Now;
                    }
                }
                else
                {
                    Inicio3 = Hoy.Date.AddDays(-1); Inicio3 = Inicio3.Add(TimeSpan.Parse(ObtenerInicioTurno("3", "I", Inicio3, _oCompany)));
                    Fin3 = Hoy.Date; Fin3 = Fin3.Add(TimeSpan.Parse(ObtenerInicioTurno("3", "F", Fin3, _oCompany)));
                    if (Hoy >= Inicio3 && Hoy <= Fin3)
                    {
                        FechaTurno = Hoy.Date.AddDays(-1);
                    }
                }
            }
            return FechaTurno;
        }

        public int NumeroDeBorradores(IDbConnection con)
        {
            int registros = 0;
            string usr = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            try
            {
                registros = con.Query<int>($"SELECT COUNT(*) FROM [Borrador] WHERE Usuario = '{usr}'").First();
                return registros;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return 0;
            }
        }

        public List<Produccion.EtiquetaTraspaleo> ObtenerEtiquetasTraspaleo(IDbConnection dbCon, string g) //Al parecer no se ocupa
        {
            try
            {
                List<Produccion.EtiquetaTraspaleo> TraspaleoProduccion = new List<Produccion.EtiquetaTraspaleo>();
                if (dbCon.State == ConnectionState.Open)
                {
                    TraspaleoProduccion = dbCon.Query<Produccion.EtiquetaTraspaleo>($"EXEC \"WMSSP_Documento\" @IdGuid = '{ g }', @Accion = 'LET'").ToList();
                    return TraspaleoProduccion;
                }
                else
                {
                    throw new Exception($"La conexión a la base de datos no esta disponible para obtener las etiquetas de traspaleo");
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return null;
            }
        }

        //**********************************************************************//
        //ETIQUETA MASTER
        //**********************************************************************//
        public bool Prefijo(ref string prefijo)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    prefijo = dbCon.Query<string>($"EXEC \"WMSSP_EtqMaster\" @Accion = 1").SingleOrDefault(); //NULL | Codigo Etq Master

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool MasterLoteInd(ref List<Etiqueta> Etiqueta, string Lote, string Articulo)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Etiqueta = dbCon.Query<Etiqueta>($"EXEC \"WMSSP_EtqMaster\" @Accion = 2, @Lote = '{Lote}', @ItemCode = '{Articulo}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public List<Etiqueta> MLineas(ref string msj, string guid, int Accion)
        {
            IDbConnection dbCon = null;
            List<Etiqueta> ln = new List<Etiqueta>();
            msj = string.Empty;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Lineas Master", "", Usuario, 3, modulo, metodo); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Lineas por GUID => " + guid, Usuario, 0, modulo, metodo); //Log Informativo
                    ln = dbCon.Query<Etiqueta>(@"EXEC WMSSP_EtqMaster @Accion = 3, @Subaccion = " + Accion + ", @IdGUID = '" + guid + "'").ToList();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                msj = ex.Message;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lineas", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
            return ln;
        }

        public Etiqueta RegMaster(ref string msj, string guid)
        {
            IDbConnection dbCon = null;
            Etiqueta ln = new Etiqueta();
            msj = string.Empty;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Registro Master", "", Usuario, 3, modulo, metodo); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Lineas por GUID => " + guid, Usuario, 0, modulo, metodo); //Log Informativo
                    ln = dbCon.Query<Etiqueta>(@"EXEC WMSSP_EtqMaster @Accion = 4, @IdGUID = '" + guid + "'").SingleOrDefault();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                msj = ex.Message;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lineas", "", Usuario, 4, modulo, metodo); //Log Informativo
            }

            return ln;
        }

        public string CodigoMaster(ref string msj)
        {
            IDbConnection dbCon = null;
            string CodigoML = string.Empty;
            msj = string.Empty;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Código Master", "", Usuario, 3, modulo, metodo); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Codigo Master", Usuario, 0, modulo, metodo); //Log Informativo
                    CodigoML = dbCon.Query<string>(@"EXEC WMSSP_EtqMaster @Accion = 5").SingleOrDefault();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                msj = ex.Message;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Codigo Master", "", Usuario, 4, modulo, metodo); //Log Informativo
            }

            return CodigoML;
        }

        //Listar Etiquetas Master Activas para su admnistracion | Cargar informacion de etiqueta master seleccionada | Validar Etiqueta Master Leida para su procesamiento
        public bool DetalleMaster(ref List<EtqMaster> Detalle, string Master, string Accion) //Listar | Cargar
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    switch (Accion)
                    {
                        case "Listar":
                            Detalle = dbCon.Query<EtqMaster>($"EXEC \"WMSSP_EtqMaster\" @Accion = 7").ToList();
                            break;

                        case "Cargar":
                            Detalle = dbCon.Query<EtqMaster>($"EXEC \"WMSSP_EtqMaster\" @Accion = 8, @CodigoMaster = '{Master}'").ToList();
                            break;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        //Obtener información de Master para su procesamiento en Módulos
        public bool DatosMaster(ref List<EtqMaster.Detalle> Detalle, ref string msj, string Etiqueta, string Almacen)
        {
            IDbConnection dbCon = null;
            msj = string.Empty;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Detalle = dbCon.Query<EtqMaster.Detalle>($"EXEC \"WMSSP_EtqMaster\" @Accion = 11, @CodigoMaster = '{Lote(Etiqueta)}', @ItemCode = '{Articulo(Etiqueta)}', @Almacen = '{Almacen}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msj = ex.Message.ToString();
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        //Listar Almacenes de una Master Activas
        public bool ListarMaster(ref List<EtqMaster.ListaMaster> Detalle, string MasterEtq)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Detalle = dbCon.Query<EtqMaster.ListaMaster>($"EXEC \"WMSSP_EtqMaster\" @Accion = 12, @CodigoMaster = '{Lote(MasterEtq)}', @ItemCode = '{Articulo(MasterEtq)}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        //Obtener información de Master para su Liberación
        public bool InfoMasterLiberar(ref List<EtqMaster.Detalle> Detalle, ref string msj, string Codigo, string Articulo, int OrdenFab)
        {
            IDbConnection dbCon = null;
            msj = string.Empty;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Detalle = dbCon.Query<EtqMaster.Detalle>($"EXEC \"WMSSP_EtqMaster\" @Accion = 13, @CodigoMaster = '{Codigo}', @ItemCode = '{Articulo}', @OrdenFab = {OrdenFab}").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msj = ex.Message.ToString();
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }
        //**********************************************************************//
        //CONSULTA DE ARTICULO
        //**********************************************************************//
        public bool ConsultaIL(string Codigo, ref List<Etiqueta> Etiqueta)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    Etiqueta = dbCon.Query<Etiqueta>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 20, @ItemBatch = '{Codigo}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        //**********************************************************************//
        //SCRAP
        //**********************************************************************//
        public bool ValidaOF(ref List<Scrap.Comun> RespOF, string NumOF)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    RespOF = dbCon.Query<Scrap.Comun>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 21, @NumOF = '{NumOF}' ").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool Maquinas(ref List<Scrap.Comun> ListaMaquinas)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ListaMaquinas = dbCon.Query<Scrap.Comun>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 22 ").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool ValidaMaq(ref List<Scrap.Comun> RespMaquina, string Maquina)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    RespMaquina = dbCon.Query<Scrap.Comun>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 23, @Maquina = '{Maquina}' ").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool TiposScrap(ref List<Scrap.Comun> ListaScrap)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ListaScrap = dbCon.Query<Scrap.Comun>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 24 ").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public List<Scrap.Detalle> ScrapLineas(string guid)
        {
            IDbConnection dbCon = null;
            List<Scrap.Detalle> ln = new List<Scrap.Detalle>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Lineas Scrap", "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Lineas por GUID => " + guid, Usuario, 0, modulo, metodo); //Log Informativo
                    ln = dbCon.Query<Scrap.Detalle>(@"EXEC WMSSP_ScrapLineas @guid = '" + guid + "'").ToList();
                }
                /*
                foreach (Scrap.Detalle lN in ln)
                {
                    Logs.FullInfo("", "\t LineNum => " + lN.LineNum +
                                        ", \t ItemCode => " + lN.ItemCode +
                                        ", \t ItemName => " + lN.ItemName +
                                        ", \t NumOrdr => " + lN.NumOrdr +
                                        ", \t DocNum => " + lN.DocNum +
                                        ", \t DocEntry => " + lN.DocEntry +
                                        ", \t Almacen => " + lN.Almacen +
                                        ", \t Maquina => " + lN.Maquina +
                                        ", \t Molde => " + lN.Molde +
                                        ", \t Turno => " + lN.Turno +
                                        ", \t TipoScrap => " + lN.TipoScrap +
                                        ", \t DescScrap => " + lN.DescScrap +
                                        ", \t Cantidad => " + lN.Cantidad +
                                        ", \t Lote => " + lN.DistNumber +
                                        "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                }
                */
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lineas", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
            return ln;
        }

        public bool ListaRegScrap(ref List<Scrap.DetalleRegistros> Registros, string NumOF)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Registros = dbCon.Query<Scrap.DetalleRegistros>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 25, @NumOF = '{NumOF}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public List<Scrap.DetalleRegistros> DelScrapLineas(string guid)
        {
            IDbConnection dbCon = null;
            List<Scrap.DetalleRegistros> ln = new List<Scrap.DetalleRegistros>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Lineas Scrap a Eliminar", "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Lineas por GUID => " + guid, Usuario, 0, modulo, metodo); //Log Informativo
                    ln = dbCon.Query<Scrap.DetalleRegistros>(@"EXEC WMSSP_EditLineasScrapTM @Accion = 'Scrap', @guid = '" + guid + "'").ToList();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lineas Scrap a Eliminar", "", Usuario, 4, modulo, metodo); //Log Informativo
            }

            return ln;
        }

        public int LineaScrap(int EntryOF, int LineId)
        {
            IDbConnection dbCon = null;
            int Linea = -1;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Linea Scrap a Eliminar de VisOrder", "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Linea de OF: " + EntryOF + " - LineId => " + LineId, Usuario, 0, modulo, metodo); //Log Informativo
                    Linea = dbCon.Query<int>(@"EXEC WMSSP_WMSConsultar @Accion = 27, @EntryOF = "+EntryOF+ ", @LineId = " + LineId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Linea Scrap a Eliminar de VisOrder", "", Usuario, 4, modulo, metodo); //Log Informativo
            }

            return Linea;
        }


        //**********************************************************************//
        //Tiempo Muerto
        //**********************************************************************//
        public bool ValidaOFTPM(ref List<TPM.Comun> RespOF, string NumOF)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    RespOF = dbCon.Query<TPM.Comun>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 21, @NumOF = '{NumOF}' ").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool MaquinasTPM(ref List<TPM.Comun> ListaMaquinas)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ListaMaquinas = dbCon.Query<TPM.Comun>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 22 ").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool ValidaMaqTPM(ref List<TPM.Comun> RespMaquina, string Maquina)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    RespMaquina = dbCon.Query<TPM.Comun>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 23, @Maquina = '{Maquina}' ").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool TiposTPM(ref List<TPM.Comun> ListaScrap)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ListaScrap = dbCon.Query<TPM.Comun>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 28 ").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public bool ListaTPMOF(ref List<TPM.DetalleRegistros> Registros, string DocEntry, string Maquina, int Turno, string Fecha)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Registros = dbCon.Query<TPM.DetalleRegistros>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 29, @EntryOF = {Int32.Parse(DocEntry)}, @Maquina = '{Maquina}', @Turno = {Turno}, @Fecha = '{Fecha}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public List<TPM.Detalle> TPMLineas(string guid, int Accion)
        {
            IDbConnection dbCon = null;
            List<TPM.Detalle> ln = new List<TPM.Detalle>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Lineas TPM", "", Usuario, 3, modulo, metodo); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Lineas por GUID => " + guid, Usuario, 0, modulo, metodo); //Log Informativo
                    ln = dbCon.Query<TPM.Detalle>(@"EXEC WMSSP_TPMLineas @Accion = " + Accion + ", @guid = '" + guid + "'").ToList();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lineas", "", Usuario, 4, modulo, metodo); //Log Informativo
            }
            return ln;
        }

        public bool ListaRegTPM(ref List<TPM.DetalleRegistros> Registros, string NumOF)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Registros = dbCon.Query<TPM.DetalleRegistros>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 30, @NumOF = '{NumOF}'").ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return false;
            }
            finally
            {
                if (dbCon != null && dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }
            }
        }

        public List<TPM.DetalleRegistros> DelTPMLineas(string guid)
        {
            IDbConnection dbCon = null;
            List<TPM.DetalleRegistros> ln = new List<TPM.DetalleRegistros>();

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Lineas TPM a Eliminar", "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Lineas por GUID => " + guid, Usuario, 0, modulo, metodo); //Log Informativo
                    ln = dbCon.Query<TPM.DetalleRegistros>(@"EXEC WMSSP_EditLineasScrapTM @Accion = 'TPM', @guid = '" + guid + "'").ToList();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Lineas TPM a Eliminar", "", Usuario, 4, modulo, metodo); //Log Informativo
            }

            return ln;
        }

        public int LineaTPM(int EntryOF, int LineId)
        {
            IDbConnection dbCon = null;
            int Linea = -1;

            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Obtener Linea TPM a Eliminar de VisOrder", "", Usuario, 3, modulo, metodo); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Obtener Linea de OF: " + EntryOF + " - LineId => " + LineId, Usuario, 0, modulo, metodo); //Log Informativo
                    Linea = dbCon.Query<int>(@"EXEC WMSSP_WMSConsultar @Accion = 31, @EntryOF = " + EntryOF + ", @LineId = " + LineId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Obtener Linea TPM a Eliminar de VisOrder", "", Usuario, 4, modulo, metodo); //Log Informativo
            }

            return Linea;
        }


        //**********************************************************************//
        //Adicionales para Remision
        //**********************************************************************//
        public bool RemCatCiente(ref Remision.RemAdicional Datos, string Articulo, string Cliente, IDbConnection dbCon)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            //Logs.FullInfo("Obtener adicionales del artículo", "", Usuario, 3, modulo, metodo); //Log Informativo
            //IDbConnection dbCon = null;
            try
            {
                //using (dbCon = new SqlConnection(CadReg))
                //{
                //dbCon.Open();
                if (dbCon != null)
                {
                    Logs.FullInfo("", "Obtener adicionales del artículo: " + Articulo + " con cliente: " + Cliente, Usuario, 0, modulo, metodo); //Log Informativo
                    Datos = dbCon.Query<Remision.RemAdicional>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 32, @Articulo = '{Articulo}', @Cliente = '{Cliente}' ").SingleOrDefault();
                    return true;
                }
                else
                {
                    Logs.FullInfo("", "Conexión a BD cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }                    
                //}
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            //finally
            //{
            //    if (dbCon != null)
            //    {
            //        dbCon.Close();
            //        dbCon.Dispose();
            //        dbCon = null;

            //        //Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, "", ""); //Log Informativo
            //    }
            //    //Logs.FullInfo("Obtener adicionales del artículo", "", Usuario, 4, modulo, metodo); //Log Informativo
            //}
        }

        public bool RemDispositivo(ref Remision.RemAdicional Datos, string Articulo, string Dispositivo, IDbConnection dbCon)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            //IDbConnection dbCon = null;
            try
            {
                //using (dbCon = new SqlConnection(CadReg))
                if (dbCon != null)
                {
                    //dbCon.Open();
                    Logs.FullInfo("", "Obtener información del artículo: " + Articulo + " con dispositivo: " + Dispositivo, Usuario, 0, modulo, metodo); //Log Informativo
                    Datos = dbCon.Query<Remision.RemAdicional>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 33, @Articulo = '{Articulo}', @Dispositivo = '{Dispositivo}' ").SingleOrDefault();
                    return true;
                }
                else
                {
                    Logs.FullInfo("", "Conexión a BD cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            //finally
            //{
            //    if (dbCon != null)
            //    {
            //        dbCon.Close();
            //        dbCon.Dispose();
            //        dbCon = null;

            //        //Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, "", ""); //Log Informativo
            //    }
            //}
        }


    }
}
