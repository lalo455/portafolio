using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WMS.Models;
using Dapper;
using WMS.Logic.Comun;
using SAPbobsCOM;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using static WMS.Models.Produccion;

using System.Drawing;
using Microsoft.Win32;

namespace WMS.Logic.Stores
{
    public class StoreProcedure
    {
        string CadReg;
        Utilities.Log Logs;
        Obtener Ob;
        private ClaimsPrincipal Identity;

        public StoreProcedure() { 
            this.CadReg = ConfigurationManager.ConnectionStrings["WmsSQL"].ConnectionString;
            this.Logs = new Utilities.Log();
            this.Ob = new Obtener();
            Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
        }

        public bool Login(ref Acceso.UsuarioLogeado Logeado, Acceso.Credenciales Cred)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logeado = dbCon.Query<Acceso.UsuarioLogeado>(
                        @"EXEC ""WMSSP_Login"" '" + Cred.Usuario + "','" + Cred.Password + "'"
                    ).FirstOrDefault();

                    // Verifica si se encontró un usuario
                    return Logeado != null;
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

        public bool ValidarPedido(string ordenVentaSAP, string numeroLineaPedido,
                          string descripcionArticulo, string numeroPiezas, string area,
                          out List<PedidoArquitectura> datosArquitectura, out string mensaje)
        {
            datosArquitectura = new List<PedidoArquitectura>();
            mensaje = string.Empty;
            IDbConnection dbCon = null;

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    var parametros = new
                    {
                        OrdenVentaSAP = ordenVentaSAP,
                        NumeroLineaPedido = numeroLineaPedido,
                        DescripcionArticulo = descripcionArticulo,
                        NumeroPiezas = numeroPiezas,
                        Area = area
                    };

                    if (area == "L")
                    {
                        // Para Laminado
                        var resultado = dbCon.QueryFirstOrDefault<PedidoArquitectura>(
                            "SP_WMS_VALIDAR_PEDIDO",
                            parametros,
                            commandType: CommandType.StoredProcedure);

                        if (resultado != null)
                        {
                            datosArquitectura.Add(resultado);
                            return true;
                        }
                        else
                        {
                            mensaje = "No se encontró el pedido en área Laminado";
                            return false;
                        }
                    }
                    else if (area == "A")
                    {
                        // Para Arquitectura
                        datosArquitectura = dbCon.Query<PedidoArquitectura>(
                            "SP_WMS_VALIDAR_PEDIDO",
                            parametros,
                            commandType: CommandType.StoredProcedure).ToList();

                        if (datosArquitectura.Any())
                        {
                            return true;
                        }
                        else
                        {
                            mensaje = "No se encontró el pedido en área Arquitectura";
                            return false;
                        }
                    }
                    else
                    {
                        mensaje = "Área inválida. Debe ser L (Laminado) o A (Arquitectura)";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException?.ToString() ?? "---", ex.Message, ex.StackTrace, MethodBase.GetCurrentMethod().DeclaringType.FullName);
                mensaje = $"Error al validar pedido: {ex.Message}";
                return false;
            }
            finally
            {
                dbCon?.Close();
            }
        }
        public bool Turnos(ref List<Produccion.Turnos> ListaTurnos) 
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ListaTurnos = dbCon.Query<Produccion.Turnos>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 13 ").ToList();

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

        public bool HrTurno(ref string HrIni, ref string HrFin, ref string msgError, string Turno)
        {
            IDbConnection dbCon = null;
            msgError = string.Empty;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    HrIni = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 10, @Subaccion = 1, @Turno = {int.Parse(Turno)} ").SingleOrDefault();
                    HrFin = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 10, @Subaccion = 2, @Turno = {int.Parse(Turno)} ").SingleOrDefault();
                    return true;
                }
            }
            catch (Exception ex)
            {
                msgError = ex.Message.ToString();
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

        public bool Operadores(ref List<Produccion.Operadores> ListaOperadores)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ListaOperadores = dbCon.Query<Produccion.Operadores>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 26 ").ToList();
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

        public bool ConfirmarUbicacion(ref TrsManual.Ubicacion Ubicacion, string BinCode, string Tipo) 
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    dbCon.Open();
                    Ubicacion = dbCon.Query<TrsManual.Ubicacion>(@"EXEC ""WMSSP_Obtener"" '"+Tipo+"','"+BinCode+"'").SingleOrDefault();
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
        
        public bool ObtenerDtsRem<T> (ref List<T> Lista, string Tipo, string Param) {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) {
                    dbCon.Open();
                    Lista = dbCon.Query<T>(@"EXEC ""WMSSP_Clientes_Pedidos"" '"+Tipo+"','"+Param+"'").ToList();
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
                    dbCon = null;
                }
            }
        }
        
        public bool GuardarRegistros(Remision.Preliminar Pre, string g) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            Remision.RemAdicional catalogo = new Remision.RemAdicional();
            Remision.RemAdicional dispositivo = new Remision.RemAdicional();
            string RemSubstitute;
            string RemPurchase;
            string RemEmpaquePallet;

            IDbConnection dbCon = null;            
            int reg = 0;

            Logs.FullInfo("Guardar Registros Remision", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    dbCon.Open();
                    foreach(var i in Pre.Lineas) 
                    {
                        RemSubstitute = string.Empty;
                        RemPurchase = string.Empty;
                        RemEmpaquePallet = string.Empty;

                        //Obtener información adicional para Remision | RemSubstitute, RemPurchase
                        if (Ob.RemCatCiente(ref catalogo, i.ItemCode, Pre.ClienteSel, dbCon)) 
                        {
                            if (catalogo != null) 
                            {
                                RemSubstitute = catalogo.RemSubstitute;
                                RemPurchase = catalogo.RemPurchase;
                            }                            
                        }

                        //Obtener información de dispositivo para Remision | RemEmpaquePallet
                        if (i.Dispositivo != null && i.Dispositivo != "")
                        {
                            if (Ob.RemDispositivo(ref dispositivo, i.ItemCode, i.Dispositivo, dbCon))
                            {
                                if (dispositivo != null)
                                {
                                    RemEmpaquePallet = dispositivo.RemEmpaquePallet;
                                }                                
                            }
                        }                        

                        Logs.FullInfo("", "Guardar Movimiento: WMSSP_GuardarMovimiento => Articulo: " + i.ItemCode + 
                                                                                        ", Almacen: " + i.WhsCode + 
                                                                                        ", Lote: " + i.DistNumber + 
                                                                                        ", Almacen Lote: " + i.WhsCode + 
                                                                                        ", Ubicacion Lote: " + i.AbsEntry + 
                                                                                        ", Cantidad: " + i.OnHandQty + 
                                                                                        ", IdGUID: " + g + 
                                                                                        ", Tarima: " + i.Tarima + 
                                                                                        ", Dispositivo: " + i.Dispositivo + 
                                                                                        ", PesoBruto: " + i.PesoBruto + 
                                                                                        ", PesoNeto: " + i.PesoNeto +
                                                                                        ", Substitute: " + RemSubstitute +
                                                                                        ", Purchase: " + RemPurchase +
                                                                                        ", EmpaquePallet: " + RemEmpaquePallet
                                                                                        , Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_GuardarMovimiento"" @ItemCode = '" + i.ItemCode+ "'" +
                                                                                ", @Desde = '" + i.WhsCode + "'" +
                                                                                ", @Hasta = ''" +
                                                                                ", @Lote = '" + i.DistNumber+"'" +
                                                                                ", @WhsLote = '" + i.WhsCode+"'" +
                                                                                ", @UbOrigen = '" + i.AbsEntry+"'" +
                                                                                ", @UbDestino = ''" +
                                                                                ", @QtyUbDestino = " + i.OnHandQty+"" +
                                                                                ", @IdGUID = '" + g+"'" +
                                                                                ", @Tarima = '" + i.Tarima+"'" +
                                                                                ", @Dispositivo = '"+ i.Dispositivo +"'" +
                                                                                ", @PesoBruto = '"+i.PesoBruto+"'" +
                                                                                ", @PesoNeto = '"+i.PesoNeto+"'" +
                                                                                ", @RemSubstitute = '"+RemSubstitute+"'" +
                                                                                ", @RemPurchase = '"+RemPurchase+"'" +
                                                                                ", @RemEmpaquePallet = '"+RemEmpaquePallet+"'");
                        
                        
                        
                        Logs.FullInfo("", "Guardar Movimiento: Registro Guardado " + i.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    Logs.FullInfo("", "Guardar Movimiento: Registros Guardados " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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
                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }
                Logs.FullInfo("Guardar Registros Remision", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool GuardarRegistrosProduccion(Produccion.Documento lin, Produccion.Datos dts, string g) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            Produccion.Ubicacion Ubi = new Produccion.Ubicacion();
            Produccion.Ubicacion UbiR = new Produccion.Ubicacion();
            string NomRecur = string.Empty;

            Logs.FullInfo("Guardar Registros Envio a Produccion", "", Usuario, 3, "", ""); //Log Informativo

            try
            {
                int Insrt = 0;
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    dbCon.Open();
                    foreach (Etiqueta p in lin.Lineas) 
                    {
                        Logs.FullInfo("", "Etiqueta procesada: " + p.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo

                        Ob.EsGranel(ref Ubi, p.ItemCode, dts.LineasSO);
                        //Destinos
                        string Destino = string.Empty;
                        if (Ubi.AbsEntry > 0 && Ubi.BinCode != null) 
                        {
                            //Si es granel
                            Destino = Ubi.AbsEntry.ToString();
                            Ubi = new Produccion.Ubicacion();

                            Logs.FullInfo("", "Destino: " + Ubi.AbsEntry.ToString(), Usuario, 0, modulo, metodo); //Log Informativo
                        }
                        else
                        {
                            Ob.ExisteRecurso(ref NomRecur, dts.LineasOF);
                            Ob.UbiRecurso(ref UbiR, NomRecur);
                            Destino = UbiR.AbsEntry.ToString();

                            Logs.FullInfo("", "Destino: " + UbiR.AbsEntry.ToString(), Usuario, 0, modulo, metodo); //Log Informativo
                        }
                        //Ob.ExisteRecurso(ref NomRecur, dts.LineasOF);
                        //Ob.UbiRecurso(ref UbiR,NomRecur);
                        string UbOri = p.DistNumber == "" || p.DistNumber == null ? Ob.ObtenerUbiSys(dts.LineasSO[0].Filler).AbsEntry.ToString() : p.AbsEntry.ToString();
                        Logs.FullInfo("", "Origen: " + UbOri, Usuario, 0, modulo, metodo); //Log Informativo

                        Logs.FullInfo("", "Guardar Movimiento: WMSSP_GuardarMovimiento => Articulo: " + p.ItemCode + 
                                                                                         "', Almacen: '" + p.WhsCode + 
                                                                                         "', Almacen Destino SO: '" + dts.LineasSO[0].ToWhsCode + 
                                                                                         "', Lote: '" + p.DistNumber + 
                                                                                         "', Almacen Lote: '" + p.WhsCode + 
                                                                                         "', Ubicacion: '" + UbOri + 
                                                                                         "', Ubicacion Destino: '" + Destino + 
                                                                                         "', Cantidad: " + p.OnHandQty + 
                                                                                         ", IdGUID: '" + g + 
                                                                                         ", Tarima: -1"
                                                                                         , Usuario, 0, modulo, metodo); //Log Informativo

                        //string UbDes = Ubi.AbsEntry == 0 && Ubi.BinCode == null ? UbiR.AbsEntry.ToString() : Ubi.AbsEntry.ToString();
                        Insrt += dbCon.Execute(@"EXEC ""WMSSP_GuardarMovimiento"" '" + p.ItemCode + "','" + p.WhsCode + "','"+dts.LineasSO[0].ToWhsCode+"','" + p.DistNumber + "','" + p.WhsCode + "','" + UbOri + "','"+ Destino +"'," + p.OnHandQty + ",'" + g + "',-1");
                        Logs.FullInfo("", "Guardar Movimiento: Registro Guardado " + p.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    Logs.FullInfo("", "Guardar Movimiento: Registros Guardados " + Insrt, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros Envio a Produccion", "", Usuario, 4, "", ""); //Log Informativo
            }
        }
        
        public bool GuardarRegistrosTraspaso(TrsManual.Traspaso traspaso, string g)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;

            Logs.FullInfo("Guardar Registros del Traspaso para Seguimiento", "", Usuario, 3, "", ""); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (var i in traspaso.Lineas)
                    {
                        // string l = ln.DistNumber + "\t" + ln.Ctd_Ctda + "\t" + ln.BinCode + "\t" + ln.ItemCode + "\t" + ln.WhsCode + "\t" + ln.ItemName + "\r\n";
                        Logs.FullInfo("", "Guardar Movimiento: \r\n WMSSP_GuardarMovimiento => \t Articulo: " + i.ItemCode +
                                                                                            ", \t Almacen: " + i.WhsCode +
                                                                                            ", \t Almacen Destino: " + traspaso.AlmacenSel +
                                                                                            ", \t Lote: " + i.DistNumber + 
                                                                                            ", \t Almacen Lote: " + i.WhsCode + 
                                                                                            ", \t Ubicacion Lote: " + i.AbsEntry + 
                                                                                            ", \t Ubicacion Destino: " + i.NewAbsEntry + 
                                                                                            ", \t Cantidad: " + i.OnHandQty + 
                                                                                            ", \t IdGuid: " + g +
                                                                                            ", \t Tarima: '0', \r\n Dispositivo: " + i.Dispositivo +
                                                                                            ", \t PesoBruto: " + i.PesoBruto + 
                                                                                            ", \t PesoNeto: " + i.PesoNeto, 
                                                                                            Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_GuardarMovimiento"" '" + i.ItemCode + "','" + i.WhsCode + "','"+traspaso.AlmacenSel+"','" + i.DistNumber + "','" + i.WhsCode + "','" + i.AbsEntry + "','"+i.NewAbsEntry+"'," + i.OnHandQty + ",'" + g + "','0', @Dispositivo = '" + i.Dispositivo + "', @PesoBruto = '" + i.PesoBruto + "', @PesoNeto = '" + i.PesoNeto + "' ");
                        Logs.FullInfo("", "Registro Guardado => " + i.DistNumber + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    Logs.FullInfo("", "Registros Guardados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros del Traspaso para Seguimiento", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool GuardarRegistroCalidad(List<DetalleR> Pre, string g, string Estatus, string Destino) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;

            Logs.FullInfo("Guardar Registros Calidad", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (var i in Pre)
                    {
                        Logs.FullInfo("", "Etiqueta Registrada: WMSSP_GuardarMovimiento => \t Articulo: " + i.ItemCode + 
                                                                                            "\t Almacen: " + i.WhsCode + 
                                                                                            "\t Destino: " + Destino + 
                                                                                            "\t Lote: " + i.DistNumber + 
                                                                                            "\t Almacen Lote: " + i.WhsCode + 
                                                                                            "\t Ubicacion Lote: " + i.AbsEntry + 
                                                                                            "\t Estatus: " + (Estatus == "X" ? "0" : i.NewAbsEntry.ToString()) + 
                                                                                            "\t Cantidad: " + i.OnHandQty + 
                                                                                            "\t IdGUID: " + g + 
                                                                                            "\t Tarima: " + i.Tarima
                                                                                            , Usuario, 0, modulo, metodo); //Log Informativo
                        //reg += dbCon.Execute($"EXEC \"WMSSP_GuardarMovimiento\" '{ i.ItemCode }','{(Estatus == "X" ? i.WhsCode : ConfigurationManager.AppSettings["CB"]) }','{ (Estatus == "X" ? ConfigurationManager.AppSettings["CB"] : Destino) }','{ i.DistNumber }','{ i.WhsCode }','{ i.AbsEntry }','{ (Estatus == "X" ? "" : i.NewAbsEntry.ToString()) }',{ i.OnHandQty },'{ g }','{ i.Tarima }'");
                        reg += dbCon.Execute($"EXEC \"WMSSP_GuardarMovimiento\" '{ i.ItemCode }','{ i.WhsCode }','{ Destino }','{ i.DistNumber }','{ i.WhsCode }','{ i.AbsEntry }','{ (Estatus == "X" ? "0" : i.NewAbsEntry.ToString()) }',{ i.OnHandQty },'{ g }','{ i.Tarima }'");
                        Logs.FullInfo("", "Etiqueta Procesada: " + i.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    Logs.FullInfo("", "Etiquetas Procesadas: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cierre de Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros Calidad", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public void BorrarRegistros(string guid) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;
            Logs.FullInfo("Borrar Registros", "", Usuario, 3, "", ""); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "WMSSP_WMSAgregar Accion = 5, TablaUsuario = 'Remision', IdGuid = '" + guid + "'", Usuario, 0, modulo, metodo); //Log Informativo
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //reg = dbCon.Execute(@"DELETE FROM ""Remision"" WHERE ""IdGUID"" = "+guid+";");
                    //Logs.FullInfo("", "Borrado de Registros: DELETE FROM Remision WHERE IdGUID " + guid, Usuario, 0, modulo, metodo); //Log Informativo
                    //WMS_DB => WMSSP_WMSAgregar
                    reg = dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 5, @TablaUsuario = 'Remision', @IdGuid = '{guid}'");                    
                    Logs.FullInfo("", "Registros borrados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cierre de Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Borrar Registros", "", Usuario, 4, "", ""); //Log Informativo
            }
        }
        
        public bool ObtenerLists<T>(ref List<T> Lista, string Accion, int Documento) 
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) {
                    dbCon.Open();
                    Lista = dbCon.Query<T>(@"EXEC ""WMSSP_CargarDocs"" @Accion = '" + Accion + "', @Documento = " + Documento + "").ToList();
                    return true;
                    /*
                    switch (Accion)
                    {
                        case "OF":
                            Lista = dbCon.Query<T>(@"EXEC ""WMSSP_CargarDocs"" @Accion = '" + Accion + "', @Documento = " + Documento + "").ToList();
                            return true;

                        case "DO":
                            Lista = dbCon.Query<T>(@"EXEC ""WMSSP_CargarDocs"" @Accion = '" + Accion + "', @Documento = "+Documento+"").ToList();
                            return true;

                        case "SO":
                            Lista = dbCon.Query<T>(@"EXEC ""WMSSP_CargarDocs"" @Accion = '" + Accion + "', @Documento = "+Documento+"").ToList();
                            return true;

                        case "EM":
                            Lista = dbCon.Query<T>(@"EXEC ""WMSSP_CargarDocs"" @Accion = '" + Accion + "', @Documento = "+Documento+"").ToList();
                            return true;
                    }
                    */
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
                    dbCon = null;
                }
            }
        }
        
        public bool ParteSN(ref List<Embarque.NParte> Encontrados, string Eti, string CodSN) {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) {
                    dbCon.Open();
                    Encontrados = dbCon.Query<Embarque.NParte>(@"EXEC ""WMSSP_ArticuloEnCatalogo"" '"+Eti+"', '"+CodSN+"'").ToList();
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
                    dbCon = null;
                }
            }
        }
        
        public bool ValidarEmbarque(int Num, List<Embarque.ASN> Lineas) 
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    dbCon.Open();

                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //dbCon.Execute(@" UPDATE ""@ASN_EMBARQUE"" SET U_Validado = 'Y' WHERE DocNum =" + Num);
                    //AKSYS_DB => WMSSP_WMSAgregar
                    dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 94, @Documento = {Num}");

                    foreach (Embarque.ASN Ln in Lineas) 
                    {
                        //dbCon.Execute(@" UPDATE ""@ASN_EMBARQUE1"" SET U_EtqsAP = '" + Ln.EtiLeidas.TrimEnd(',') + "', U_EtqsCli = '" + Ln.EtiExterno.TrimEnd(',') + "' WHERE LineId = " + Ln.LineId);
                        //AKSYS_DB => WMSSP_WMSAgregar
                        dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 95, @EtqsAP = '{Ln.EtiLeidas.TrimEnd(',')}', @EtqsCli = '{Ln.EtiExterno.TrimEnd(',')}', @Linea = {Ln.LineId}");
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

        public void RemoverRegistros(string Tbl, string guid) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;

            Logs.FullInfo("Remover Registros Borrador", "", Usuario, 3, "", ""); //Log Informativo

            try
            {
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    dbCon.Open();
                    Logs.FullInfo("", "WMSSP_WMSAgregar Accion = 5, TablaUsuario = '" + Tbl + 
                                                            "', IdGuid = '" + guid + "'"
                                                            , Usuario, 0, modulo, metodo); //Log Informativo
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //reg = dbCon.Execute(@"DELETE FROM " + Tbl + " WHERE IdGUID = '" + guid + "'");
                    //WMS_DB => WMSSP_WMSAgregar
                    reg = dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 5, @TablaUsuario = '{ Tbl }', @IdGuid = '{guid}'");                    
                }
                Logs.FullInfo("", "Registros Eliminados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                Logs.FullInfo("Remover Registros Borrador", "", Usuario, 4, "", ""); //Log Informativo
            }
        }
                
        public bool SociosEmbarques(ref List<Embarque.Socios> ListSocios) {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) {
                    dbCon.Open();
                    ListSocios = dbCon.Query<Embarque.Socios>($"EXEC \"WMSSP_BPEmbarques\"").ToList();
                    return true;
                }
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
                    dbCon = null;
                }
            }
        }
        
        public bool ExisteNumeroSerie(ref string ItemCode, string Serial, string Documento)
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    ItemCode = dbCon.Query<string>($"EXEC \"WMSSP_Etiqueta\" '{Serial}','{Documento}'").SingleOrDefault();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                ItemCode = ex.Message;
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
        
        public bool GuardarProduccion(List<Produccion.EmisionRecibo> lineas, string guid, ref string result)
        {
            IDbConnection dbCon = null;
            int inserts = 0;
            //string metodo = "GuardarProduccion";
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            
            Logs.FullInfo("Guardar Registros de Emision y Recibo de Produccion", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    if (dbCon.State == ConnectionState.Open)
                    {
                        Logs.FullInfo("", "Conexion a BD Abierta", Usuario, 0, modulo, metodo); //Log Informativo
                        for (int l = 0; l < lineas.Count; l++) //Recorre cada etiqueta y la guarda para el seguimiento
                        {
                            //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                            Logs.FullInfo("", "Guardar Etiqueta Procesada: \r\n Lote" + lineas[l].DistNumber +
                                                                            ", \t Articulo: " + lineas[l].ItemCode +
                                                                            ", \t Cantidad: " + lineas[l].OnHandQty +
                                                                            ", \t Orden Fabricación: " + lineas[l].OF +
                                                                            ", \t Ubicacion Lote: " + lineas[l].AbsEntry +
                                                                            ", \t Turno: " + lineas[l].U_TurnoRecibo +
                                                                            ", \t Fecha: " + lineas[l].U_FechaRecibo +
                                                                            ", \t IdGuid: " + guid +
                                                                            ", \t Master: " + lineas[l].EtqMaster +
                                                                            ", \t Tipo Lectura: " + lineas[l].TipoLectura + 
                                                                            ", \t Tipo Registro: " + lineas[l].TipoReg 
                                                                            , Usuario, 0, modulo, metodo); //Log Informativo
                            inserts += dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 1, @Lote = '{lineas[l].DistNumber}', @Articulo = '{lineas[l].ItemCode}', @Cantidad = {lineas[l].OnHandQty}, @OrdenFab = {lineas[l].OF}, @AbsEntry = {lineas[l].AbsEntry}, @Turno = {lineas[l].U_TurnoRecibo}, @Fecha = '{lineas[l].U_FechaRecibo}', @IdGuid = '{guid}', @EtqMaster = '{lineas[l].EtqMaster}', @TipoLectura = '{lineas[l].TipoLectura}', @TipoReg = '{lineas[l].TipoReg}' ");
                            Logs.FullInfo("", "Registro Guardado => " + lineas[l].DistNumber + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                        }

                        if (inserts > 0)
                        {
                            result = string.Empty;
                            Logs.FullInfo("", "Etiquetas Registradas con GUID: " + guid + " --- Procesadas: " + inserts, Usuario, 0, modulo, metodo); //Log Informativo

                            return true;
                        }
                        else
                        {
                            result = $"Se insertaron: {inserts} registros de: {lineas.Count}";
                            Logs.FullInfo("", result + ", con el GUID: " + guid, Usuario, 0, modulo, metodo); //Log Informativo

                            return false;
                        }
                    }
                    else
                    {
                        result = "No se ha inicializado una conexion a la base de datos de WMS";
                        Logs.FullInfo("", "Guardar Registros de Emision y Recibo de Produccion: Conexión a BD NO Abierta", Usuario, 0, modulo, metodo); //Log Informativo

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                result = ex.Message;

                Logs.FullInfo("Guardar Registros de Emision y Recibo de Produccion: ", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros de Emision y Recibo de Produccion", "", Usuario, 4, "", ""); //Log Informativo
            }
        }   
        
        //Actualizar el Status de la Etiqueta Procesada
        public void EstatusReciboProduccion(ref string msj, ref int codeR, string valor, List<Produccion.DocumentoAProduccion> Recibo) 
        {
            int response = 0;
            msj = string.Empty;
            codeR = 0;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            try
            {
                string etqs = EtiquetasIN(Recibo);
                if (!string.IsNullOrEmpty(etqs))
                {
                    //string condicion = $" WHERE Name IN ({etqs})";
                    using (dbCon = new SqlConnection(CadReg))
                    {
                        dbCon.Open();
                        Logs.FullInfo("", ">>> WMSSP_WMSAgregar @Accion = 93, @Estatus = '"+valor+"', @Lotes = '"+etqs+"'", Usuario, 0, modulo, metodo); //Log Informativo
                        response = dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 93, @Estatus = '{ valor }', @Lotes = '{ etqs }'");
                        codeR = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                codeR = -1;
                msj = ex.Message.ToString();
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    //Logs.FullInfo("", "Cierre de BD", Usuario, 0, modulo, metodo); //Log Informativo
                    dbCon.Dispose();
                }
                //Logs.FullInfo("Actualizar Status Etiquetas a Liberadas", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public string EtiquetasIN(List<Produccion.DocumentoAProduccion> Lineas)
        {
            //string metodo = "EtiquetasIN";
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            
            //Logs.FullInfo("", "Etiquetas a Procesar", Usuario, 0, modulo, metodo); //Log Informativo

            string cadena = string.Empty;
            for (int i = 0; i < Lineas.Count; i++)
            {
                if (string.IsNullOrEmpty(cadena))
                    cadena = $"''{Lineas[i].Batch}''";
                else
                    cadena += $",''{Lineas[i].Batch}''";
            }
            //Logs.FullInfo("", "Etiquetas: " + cadena, Usuario, 0, modulo, metodo); //Log Informativo
            return cadena;
        }
        
        public bool CatalogoSocioDeNegocios(string CardCode, ref List<string> Catalogo) 
        {
            IDbConnection dbCon = null;
            try
            {
                using (dbCon = new SqlConnection(CadReg)) 
                {
                    //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
                    //Catalogo = dbCon.Query<string>($"SELECT ItemCode FROM OSCN WHERE CardCode = '{CardCode}'").ToList();
                    //AKSYS_DB => WMSSP_WMSConsultar
                    Catalogo = dbCon.Query<string>($"EXEC \"WMSSP_WMSConsultar\" @Accion = 14, @Cliente = '{ CardCode }'").ToList();
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

        public bool GuardarBorrador(List<Produccion.EmisionRecibo> Lineas, string guid, ref string result, ref int totalOk) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("", "G U A R D A R   B O R R A D O R", Usuario, 3, "", ""); //Log Informativo
            
            IDbConnection dbCon = null;
            string usr = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            totalOk = 0;
            int response = -1; //-1 => Error   
            StringBuilder sb = new StringBuilder();
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    IDbTransaction tran = dbCon.BeginTransaction();
                    Logs.FullInfo("", "****** Inicio de Transaccion *********** ", Usuario, 0, modulo, metodo); 

                    foreach (Models.Borrador rw in DatosBorrador(Lineas, usr, guid)) 
                    {
                        Logs.FullInfo("", "Buscar etiqueta " + rw.DistNumber + " en Borrador", Usuario, 0, modulo, metodo); 
                        
                        List<BorradorEtiqueta> Lista = new List<BorradorEtiqueta>();
                        Lista = dbCon.Query<BorradorEtiqueta>($"EXEC \"WMSSP_WMSAgregar\" @Accion = 3, @Lote = '{rw.DistNumber}'", null, transaction: tran).ToList();

                        if (Lista.Count > 0)
                        {
                            /*
                            POSIBLE MEJORA:
                            EN LA VERIFICACIÓN DE INFORMACION, VALIDAR SI EL USUARIO DEL REGISTRO ES EL MISMO QUE ESTA GUARDANDO
                            SI ES EL MISMO: ACTUALIZAR REGISTRO 
                            NO ES EL MISMO: SE MUESTRA EL MENSAJE DEL REGISTROS GUARDADO
                            */
                            foreach (BorradorEtiqueta bR in Lista)
                            {
                                //Si el usuario del registro es igual al usuario que esta guardando, actualizar registro
                                if (usr == bR.Usuario)
                                {
                                    //response = 0 => OK
                                    response = dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 8, @Lote = '{rw.DistNumber}', @AbsEntry = {rw.AbsEntry}, @Ubicacion = '{rw.BinCode}', @AlmacenSel = '{rw.AlmacenSel}', @Usuario = '{rw.Usuario}', @FechaActualizacion = '{rw.FechaActualizacion}', @TipoLectura = '{rw.TipoLectura}', @TipoReg = '{rw.TipoReg}', @EtqMaster = '{rw.EtqMaster}'", null, transaction: tran);
                                    Logs.FullInfo("", "\t Etiqueta Actualizada en Borrador: " + rw.DistNumber +
                                                            ",\t Usuario " + rw.Usuario +
                                                            ",\t Fecha de Actualizacion " + rw.FechaActualizacion
                                                            , Usuario, 0, modulo, metodo); //Log Informativo
                                    totalOk = totalOk + 1;
                                }
                                else
                                {
                                    //El usuario en registro es diferente al usuario que esta guardando
                                    Logs.FullInfo("", "\r\n\t La etiqueta: " + rw.DistNumber +
                                                        " de la OF " + bR.OF +
                                                        " con Turno " + bR.U_TurnoRecibo +
                                                        " ya Existe en Borrador de Usuario " + bR.Usuario +
                                                        ", con Fecha de Creación " + bR.FechaCreacion
                                                        , Usuario, 0, modulo, metodo); //Log Informativo

                                    sb.AppendLine("&La etiqueta: " + rw.DistNumber +
                                                    " de la OF " + bR.OF +
                                                    " Existe en Borrador de Usuario " + bR.Usuario +
                                                    " con Turno " + bR.U_TurnoRecibo +
                                                    " guardado el " + bR.FechaCreacion);

                                    response = -10; //El lote ya fue guardado en el borrador de otro usuario
                                }
                            }
                        }
                        else
                        {
                            response = dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 2, @Lote = '{rw.DistNumber}', @Articulo = '{rw.ItemCode}', @Cantidad = {rw.OnHandQty}, @OrdenFab = {rw.OF}, @AbsEntry = {rw.AbsEntry}, @Ubicacion = '{rw.BinCode}', @AlmacenSel = '{rw.AlmacenSel}', @Usuario = '{rw.Usuario}', @FechaCreacion = '{rw.FechaCreacion}', @FechaActualizacion = '{rw.FechaActualizacion}', @Turno = {rw.U_TurnoRecibo}, @Fecha = '{rw.U_FechaRecibo}', @IdGuid = '{rw.IdGuid}', @TipoLectura = '{rw.TipoLectura}', @TipoReg = '{rw.TipoReg}', @EtqMaster = '{rw.EtqMaster}'", null, transaction: tran);
                            Logs.FullInfo("", "\t Etiqueta Guardada en Borrador: " + rw.DistNumber +
                                                    ",\t OF " + rw.OF +
                                                    ",\t Usuario " + rw.Usuario +
                                                    ",\t Fecha de Creación " + rw.FechaCreacion
                                                    , Usuario, 0, modulo, metodo); //Log Informativo
                            totalOk = totalOk + 1;
                        }                            
                    }
                    tran.Commit();
                    Logs.FullInfo("", "**** Fin de Transaccion *********** ", Usuario, 0, modulo, metodo); //Log Informativo
                }
                result = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                result = ex.Message;
                Logs.FullInfo("", "Error: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                
                return false;
            }
            finally 
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                }

                Logs.FullInfo("", "G U A R D A R   B O R R A D O R", Usuario, 4, "", ""); //Log Informativo
            }
        }

        private List<Models.Borrador> DatosBorrador(List<Produccion.EmisionRecibo> Lineas, string Usuario, string guid)
        {
            List<Models.Borrador> draft = new List<Models.Borrador>();
            try
            {
                foreach (Produccion.EmisionRecibo tbl in Lineas)
                {
                    draft.Add(new Models.Borrador
                    {
                        AbsEntry = tbl.AbsEntry,
                        DistNumber = tbl.DistNumber,
                        FechaActualizacion = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                        FechaCreacion = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                        IdGuid = guid,
                        ItemCode = tbl.ItemCode,
                        OF = tbl.OF,
                        OnHandQty = tbl.OnHandQty,
                        Usuario = Usuario,
                        BinCode = tbl.BinCode,
                        AlmacenSel = tbl.Warehouse,
                        U_TurnoRecibo = tbl.U_TurnoRecibo,
                        U_FechaRecibo = tbl.U_FechaRecibo,
                        EtqMaster = tbl.EtqMaster,
                        TipoLectura = tbl.TipoLectura,
                        TipoReg = tbl.TipoReg
                    });
                }
                return draft;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return null;
            }
        }

        public bool EliminarRegistroBorrador(ref string msj, Produccion.EmisionRecibo registro)
        {
            msj = string.Empty;
            IDbConnection dbCon = null;

            //Actualización 13-03-2021 / IUrizar :: Se cambia la consulta directa por un SP, para tener un mejor control de la información.
            int response = -1;
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    response = dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 6, @Lote = '{registro.DistNumber}'");
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
                

        //Eliminar Etiquetas de Borrador 
        public void EliminarBorrador(List<Produccion.DocumentoAProduccion> Recibo) 
        {
            IDbConnection dbCon = null;
            int response = 0;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            try
            {
                string etqs = EtiquetasIN(Recibo);
                if (!string.IsNullOrEmpty(etqs))
                {
                    using (dbCon = new SqlConnection(CadReg))
                    {
                        dbCon.Open();
                        Logs.FullInfo("", ">>> WMSSP_WMSAgregar @Accion = 7, @Lote = '"+etqs+"', @Usuario = '"+Usuario+"'", Usuario, 0, modulo, metodo); //Log Informativo
                        response = dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 7, @Lote = '{ etqs }', @Usuario = '{ Usuario }'");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }

        public int EliminarTBorrador(List<Produccion.EmisionRecibo> Lineas) 
        {
            IDbConnection dbCon = null;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
            int registros = 0;
            
            try
            {
                if (Lineas.Count > 0)
                {
                    using (dbCon = new SqlConnection(CadReg))
                    {
                        dbCon.Open();
                        IDbTransaction tran = dbCon.BeginTransaction();
                        dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 7, @Lote = '{ CondicionDelete(Lineas) }', @Usuario = '{ Usuario }' ", transaction: tran);
                        tran.Commit();
                        registros = dbCon.Query<int>($"EXEC \"WMSSP_WMSAgregar\" @Accion = 4, @Usuario = '{ Usuario }'").SingleOrDefault();
                    }
                    return registros;
                }
                else
                    return registros;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return 0;
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
        
        private string CondicionDelete(List<Produccion.EmisionRecibo> Lineas)
        {
            string cadena = string.Empty;
            foreach (Produccion.EmisionRecibo r in Lineas)
            {
                if (string.IsNullOrEmpty(cadena))
                    cadena = $"''{r.DistNumber}''";
                else
                    cadena += $",''{r.DistNumber}''";
            }
            return cadena;
        }
        
        public bool GuardarRegistrosTraspaleo(List<Produccion.EmisionRecibo> Lineas, string ToWhsCode, string g, ref int reg, ref string mensaje) 
        {
            IDbConnection dbCon = null;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Guardar Registros Traspaleo", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    if (ConnectionState.Open == dbCon.State)
                    {
                        Logs.FullInfo("", "Conexion a BD Abierta", Usuario, 0, modulo, metodo); //Log Informativo
                        foreach (Produccion.EmisionRecibo t in Lineas)
                        {
                            Logs.FullInfo("", "Etiqueta Procesada: WMSSP_TTraspaleo => \t Lote => " + t.DistNumber +
                                                "- \t Almacen => " + t.Warehouse +
                                                "- \t AbsEntry => " + t.AbsEntry +
                                                "- \t OnHandQty => " + t.OnHandQty +
                                                "- \t GUID => " + g +
                                                "- \t OF => " + t.OF
                                                , Usuario, 0, modulo, metodo); //Log Informativo

                            reg += dbCon.Execute($"EXEC \"WMSSP_TTraspaleo\" '{t.DistNumber}','{t.Warehouse}',{t.AbsEntry},{t.OnHandQty},'{g}', {t.OF}");
                            Logs.FullInfo("", "Etiqueta Procesada: " + t.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
                        }

                        Logs.FullInfo("", "Total de Etiquetas Procesadas: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
                        return true;
                    }
                    else
                    {
                        Logs.FullInfo("", "GuardarProduccion: Conexión a BD NO Abierta", Usuario, 0, modulo, metodo); //Log Informativo
                        return false;
                    }
                }           
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                mensaje = ex.Message;

                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros Traspaleo", "", Usuario, 4, "", ""); //Log Informativo
            }
        }
        
        public string EtiquetasINTraspaleo(List<Produccion.EmisionRecibo> Lineas)
        {
            string cadena = string.Empty;
            for (int i = 0; i < Lineas.Count; i++)
            {
                if (string.IsNullOrEmpty(cadena))
                    cadena = $"'{Lineas[i].DistNumber}'";
                else
                    cadena += $",'{Lineas[i].DistNumber}'";
            }
            return cadena;
        }

        //Scrap ***********************************************************************************************
        public bool GuardarRegistrosScrap(List<Scrap.Detalle> Lineas, string g)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;

            Logs.FullInfo("Guardar Registros de Scrap para Seguimiento", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (Scrap.Detalle i in Lineas)
                    {
                        Logs.FullInfo("", "Guardar Scrap: \r\n WMSSP_GuardarScrap => \t Articulo: " + i.ItemCode +
                                                                                        ", \t NumOrdr: " + i.NumOrdr +
                                                                                        ", \t DocNum: " + i.DocNum +
                                                                                        ", \t DocEntry: " + i.DocEntry +
                                                                                        ", \t Almacen: " + i.Almacen +
                                                                                        ", \t Maquina: " + i.Maquina +
                                                                                        ", \t Molde: " + i.Molde +
                                                                                        ", \t Id Operador: " + i.IdOperador +
                                                                                        ", \t Operador: " + i.Operador +
                                                                                        ", \t Fecha: " + i.Fecha +
                                                                                        ", \t Turno: " + i.Turno +
                                                                                        ", \t TipoScrap: " + i.TipoScrap +
                                                                                        ", \t DescScrap: " + i.DescScrap +
                                                                                        ", \t Cantidad: " + i.Cantidad +
                                                                                        ", \t DistNumber: " + i.DistNumber +
                                                                                        ", \t Usuario: " + Usuario +
                                                                                        ", \t IdGuid: " + g ,
                                                                                        Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_GuardarScrap"" @ItemCode = '" + i.ItemCode + 
                            "', @Orden = '" + i.NumOrdr + 
                            "', @DocNum = '" + i.DocNum + 
                            "', @DocEntry = '" + i.DocEntry +
                            "', @Almacen = '" + i.Almacen + 
                            "', @Maquina = '" + i.Maquina + 
                            "', @Molde = '" + i.Molde +
                            "', @IdOperador = " + i.IdOperador +
                            ", @Operador = '" + i.Operador +
                            "', @Fecha = '" + i.Fecha +
                            "', @Turno = " + i.Turno + 
                            ", @TipoScrap = '" + i.TipoScrap + 
                            "', @Scrap = '" + i.DescScrap + 
                            "', @Cantidad = " + i.Cantidad + 
                            ", @Lote = '" + i.DistNumber + 
                            "', @IdGuia = '" + g + 
                            "', @Usuario = '" + Usuario + "'");

                        Logs.FullInfo("", "Registro Guardado => " + i.DistNumber + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    Logs.FullInfo("", "Registros Guardados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros de Scrap para Seguimiento", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool GuardarRegistrosEliminarScrap(List<Scrap.Detalle> Lineas, string g)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;

            Logs.FullInfo("Guardar Registros de Scrap a Eliminar para Seguimiento", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (Scrap.Detalle i in Lineas)
                    {
                        Logs.FullInfo("", "Guardar Scrap: \r\n WMSSP_ScrapTPMaEliminar => \t Accion = Scrap" +
                                                                                        ", \t NumOrdr: " + i.NumOrdr +
                                                                                        ", \t DocNum: " + i.DocNum +
                                                                                        ", \t DocEntry: " + i.DocEntry +
                                                                                        ", \t ItemCode: " + i.ItemCode +
                                                                                        ", \t ItemName: " + i.ItemName +
                                                                                        ", \t Maquina: " + i.Maquina +
                                                                                        ", \t Turno: " + i.Turno +
                                                                                        ", \t TipoScrap: " + i.TipoScrap +
                                                                                        ", \t DescScrap: " + i.DescScrap +
                                                                                        ", \t Cantidad: " + i.Cantidad +                                                                                        
                                                                                        ", \t LineNum: " + i.LineNum +
                                                                                        ", \t SDocEntry: " + i.SDocEntry +
                                                                                        ", \t LineId: " + i.LineId +
                                                                                        ", \t VisOrder: " + i.VisOrder +
                                                                                        ", \t Usuario: " + Usuario +
                                                                                        ", \t IdGuid: " + g,
                                                                                        Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_ScrapTPMaEliminar"" @Accion = 'Scrap'" +
                            ", @Orden = '" + i.NumOrdr +
                            "', @DocNum = '" + i.DocNum +
                            "', @DocEntry = '" + i.DocEntry +
                            "', @ItemCode = '" + i.ItemCode +
                            "', @ItemName = '" + i.ItemName +
                            "', @Maquina = '" + i.Maquina +
                            "', @Turno = " + i.Turno +
                            ", @TipoScrap = '" + i.TipoScrap +
                            "', @Scrap = '" + i.DescScrap +
                            "', @Cantidad = " + i.Cantidad +
                            ", @LineNum = " + i.LineNum +
                            ", @SDocEntry = '" + i.SDocEntry +
                            "', @LineId = " + i.LineId +
                            ", @VisOrder = " + i.VisOrder +
                            ", @IdGuia = '" + g +
                            "', @Usuario = '" + Usuario + "'");

                        Logs.FullInfo("", "Registro Guardado => " + i.DistNumber + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    Logs.FullInfo("", "Registros Guardados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros de Scrap a Eliminiar para Seguimiento", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Tiempo Muerto ***********************************************************************************************
        public bool GuardarRegistrosTPM(List<TPM.Detalle> Lineas, string g)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;

            Logs.FullInfo("Guardar Registros de TPM para Seguimiento", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (TPM.Detalle i in Lineas)
                    {
                        Logs.FullInfo("", "Guardar Scrap: \r\n WMSSP_GuardarTPM => \t Articulo: " + i.ItemCode +
                                                                                        ", \t NumOrdr: " + i.NumOrdr +
                                                                                        ", \t DocNum: " + i.DocNum +
                                                                                        ", \t DocEntry: " + i.DocEntry +
                                                                                        ", \t Almacen: " + i.Almacen +
                                                                                        ", \t Maquina: " + i.Maquina +
                                                                                        ", \t Molde: " + i.Molde +
                                                                                        ", \t Id Operador: " + i.IdOperador +
                                                                                        ", \t Operador: " + i.Operador +
                                                                                        ", \t Fecha: " + i.Fecha +
                                                                                        ", \t Turno: " + i.Turno +
                                                                                        ", \t TipoTPM: " + i.TipoTPM +
                                                                                        ", \t DescTPM: " + i.DescTPM +
                                                                                        ", \t Tiempo: " + i.Tiempo +
                                                                                        ", \t DistNumber: " + i.DistNumber +
                                                                                        ", \t Categoria: " + i.Categoria +
                                                                                        ", \t Correo: " + i.Correo +
                                                                                        ", \t Usuario: " + Usuario +
                                                                                        ", \t IdGuid: " + g,
                                                                                        Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_GuardarTPM"" @ItemCode = '" + i.ItemCode +
                            "', @Orden = '" + i.NumOrdr +
                            "', @DocNum = '" + i.DocNum +
                            "', @DocEntry = '" + i.DocEntry +
                            "', @Almacen = '" + i.Almacen +
                            "', @Maquina = '" + i.Maquina +
                            "', @Molde = '" + i.Molde +
                            "', @IdOperador = " + i.IdOperador +
                            ", @Operador = '" + i.Operador +
                            "', @Fecha = '" + i.Fecha +
                            "', @Turno = " + i.Turno +
                            ", @TipoTPM = '" + i.TipoTPM +
                            "', @DescTPM = '" + i.DescTPM +
                            "', @Tiempo = " + i.Tiempo +
                            ", @Lote = '" + i.DistNumber +
                            "', @IdGuia = '" + g +
                            "', @Usuario = '" + Usuario + 
                            "', @Categoria = '" + i.Categoria + 
                            "', @Correo = '" + i.Correo + "'");

                        Logs.FullInfo("", "Registro Guardado => " + i.DistNumber + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    Logs.FullInfo("", "Registros Guardados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros de TPM para Seguimiento", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool GuardarRegistrosEliminarTPM(List<TPM.Detalle> Lineas, string g)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;

            Logs.FullInfo("Guardar Registros de TPM a Eliminar para Seguimiento", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (TPM.Detalle i in Lineas)
                    {
                        Logs.FullInfo("", "Guardar Scrap: \r\n WMSSP_ScrapTPMaEliminar => \t Accion = TPM" +
                                                                                        ", \t NumOrdr: " + i.NumOrdr +
                                                                                        ", \t DocNum: " + i.DocNum +
                                                                                        ", \t DocEntry: " + i.DocEntry +
                                                                                        ", \t Maquina: " + i.Maquina +
                                                                                        ", \t Turno: " + i.Turno +
                                                                                        ", \t TipoTPM: " + i.TipoTPM +
                                                                                        ", \t TPM: " + i.DescTPM +
                                                                                        ", \t Tiempo: " + i.Tiempo +
                                                                                        ", \t SDocEntry: " + i.SDocEntry +
                                                                                        ", \t LineId: " + i.LineId +
                                                                                        ", \t VisOrder: " + i.VisOrder +
                                                                                        ", \t Usuario: " + Usuario +
                                                                                        ", \t IdGuid: " + g,
                                                                                        Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_ScrapTPMaEliminar"" @Accion = 'TPM'" +
                            ", @Orden = '" + i.NumOrdr +
                            "', @DocNum = '" + i.DocNum +
                            "', @DocEntry = '" + i.DocEntry +
                            "', @Maquina = '" + i.Maquina +
                            "', @Turno = " + i.Turno +
                            ", @TipoTPM = '" + i.TipoTPM +
                            "', @TPM = '" + i.DescTPM +
                            "', @Tiempo = " + i.Tiempo +
                            ", @SDocEntry = '" + i.SDocEntry +
                            "', @LineId = " + i.LineId +
                            ", @VisOrder = " + i.VisOrder +
                            ", @IdGuia = '" + g +
                            "', @Usuario = '" + Usuario + "'");

                        Logs.FullInfo("", "Registro Guardado => " + i.DistNumber + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    Logs.FullInfo("", "Registros Guardados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros de TPM a Eliminiar para Seguimiento", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Etiqueta Master **************************************************************************************************
        public bool GuardarRegistrosMaster(List<Etiqueta> Lineas, string Accion, string g)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;

            Logs.FullInfo("Guardar Registros de Etiqueta Master para Seguimiento", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                int TurnoMaster = int.Parse(Ob.ObtenerTurno());
                string EtqMaster = string.Empty;
                
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    foreach (var i in Lineas)
                    {
                        if (Accion == "Editar")
                        {
                            TurnoMaster = i.TurnoMaster;
                            EtqMaster = Lineas[0].EtqMaster;
                        }

                        Logs.FullInfo("", "Guardar Movimiento: \r\n WMSSP_GuardarMovimientoMaster => \t Lote: " + i.DistNumber +
                                                                                                    ", \t Artículo: " + i.ItemCode +
                                                                                                    ", \t Cantidad: " + i.OnHandQty +
                                                                                                    ", \t Turno Etq: " + i.TurnoEtq +
                                                                                                    ", \t Master: " + EtqMaster +
                                                                                                    ", \t Turno Master: " + TurnoMaster +
                                                                                                    ", \t Reg Master: " + i.RegMaster +
                                                                                                    ", \t IdGuid: " + g +
                                                                                                    ", \t Usuario: " + Usuario +
                                                                                                    ", \t Tipo Master: " + i.Tipo,
                                                                                                    Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_GuardarMovimientoMaster"" @Lote = '" + i.DistNumber + "', @ItemCode = '" + i.ItemCode + "', @Qty = " + i.OnHandQty + ", @TurnoEtq = " + i.TurnoEtq + ", @Master = '" + EtqMaster + "'" + 
                            ", @TurnoMaster = " + TurnoMaster + ", @Tipo = '" + i.Tipo + "', @IdGUID = '" + g + "', @Usuario = '" + Usuario + "', @RegMaster = '" + i.RegMaster + "'");

                        Logs.FullInfo("", "Registro Guardado => " + i.DistNumber + " | Reg Master => " + i.RegMaster + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    Logs.FullInfo("", "Registros Guardados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
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

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros de Etiqueta Master para Seguimiento", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool GuardarMasterWMS(ref string msj, List<Etiqueta> Lineas, string g)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            IDbTransaction transaction = null;
            int reg = 0;
            msj = string.Empty;

            Logs.FullInfo("Guardar Registros de Etiqueta Master WMS", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {                    
                    dbCon.Open();
                    transaction = dbCon.BeginTransaction();

                    foreach (var i in Lineas)
                    {
                        Logs.FullInfo("", "Guardar Master: \r\n WMSSP_GuardarMaster => \t Lote: " + i.DistNumber +
                                                                                        ", \t Artículo: " + i.ItemCode +
                                                                                        ", \t Cantidad: " + i.OnHandQty +
                                                                                        ", \t Turno Etq: " + i.TurnoEtq +
                                                                                        ", \t Master: " + i.EtqMaster +
                                                                                        ", \t Turno Master: " + i.TurnoMaster +
                                                                                        ", \t IdGuid: " + g +
                                                                                        ", \t Usuario: " + Usuario +
                                                                                        ", \t Tipo Master: " + i.Tipo,
                                                                                        Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_GuardarMaster"" @Lote = '" + i.DistNumber + "', @ItemCode = '" + i.ItemCode + "', @Qty = " + i.OnHandQty + ", @TurnoEtq = " + i.TurnoEtq + ", @Master = '" + i.EtqMaster + "'" +
                            ", @TurnoMaster = " + i.TurnoMaster + ", @Tipo = '" + i.Tipo + "', @IdGUID = '" + g + "', @Usuario = '" + Usuario + "'", null, transaction);

                        Logs.FullInfo("", "Registro Guardado => " + i.DistNumber + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    }
                    transaction.Commit();

                    Logs.FullInfo("", "Registros Guardados: " + reg, Usuario, 0, modulo, metodo); //Log Informativo
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                msj = ex.Message.ToString();
                transaction.Rollback();
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Guardar Registros de Etiqueta Master WMS", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        public bool GenerarMasterWMS(ref string msj, ref string MasterNueva, Etiqueta RegMaster)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;
            string CodigoMaster = string.Empty;
            msj = string.Empty;

            Logs.FullInfo("Generar Etiqueta Master WMS", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();

                    //Obtener código master
                    CodigoMaster = Ob.CodigoMaster(ref msj);

                    if (CodigoMaster != null || CodigoMaster != "")
                    {
                        Logs.FullInfo("", "Generar Master: \r\n WMSSP_GenerarMaster => \t Code: " + CodigoMaster +
                                                                                    ", \t Artículo: " + RegMaster.ItemCode +
                                                                                    ", \t Cantidad: " + RegMaster.OnHandQty +
                                                                                    ", \t StImp: I" + 
                                                                                    ", \t StEti: L" + 
                                                                                    ", \t StCal: O" + 
                                                                                    ", \t Turno Etq: " + RegMaster.TurnoMaster +
                                                                                    ", \t Usuario: " + Usuario,
                                                                                    Usuario, 0, modulo, metodo); //Log Informativo

                        reg += dbCon.Execute(@"EXEC ""WMSSP_GenerarMaster"" @CodigoMaster = '" + CodigoMaster + "', @ItemCode = '" + RegMaster.ItemCode + "', @Qty = " + RegMaster.OnHandQty + ", @TurnoMaster = " + RegMaster.TurnoMaster + ", @Usuario = '" + Usuario + "'");
                        
                        MasterNueva = CodigoMaster;
                        Logs.FullInfo("", "Master Creada => " + MasterNueva + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                        return true;
                    }
                    else
                    {
                        Logs.FullInfo("", "Error al obtener código master => " + msj + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                msj = ex.Message.ToString();
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Generar Etiqueta Master WMS", "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Actualizar Registros con Codigo Master
        public void MasterReg(ref string msj, ref int codigo, string CodigoMaster, List<Etiqueta> Registros, string g)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            int response = 0;
            msj = string.Empty;
            codigo = 0;
            string ListaLotes = string.Empty;
            string ListaArticulos = string.Empty;

            IDbConnection dbCon = null;
            try
            {
                ListaLotes = CadenaArticulos(Registros);
                ListaArticulos = CadenaLotes(Registros);

                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Actualizar Codigo Master en Registros: \r\n WMSSP_EtqMaster => \t Accion: 6 " + 
                                                                                                    ", \t Codigo Master: " + CodigoMaster +
                                                                                                    ", \t IdGuia: " + g
                                                                                                    ,Usuario, 0, modulo, metodo); //Log Informativo

                    response = dbCon.Execute($"EXEC \"WMSSP_EtqMaster\" @Accion = 6, @CodigoMaster = '{CodigoMaster}', @IdGUID = '{g}'");

                    codigo = 0;
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                codigo = -1;
                msj = ex.Message;
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

        private string CadenaArticulos(List<Etiqueta> Lineas)
        {
            StringBuilder condicion = new StringBuilder();
            foreach (Etiqueta ln in Lineas)
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

        private string CadenaLotes(List<Etiqueta> Lineas)
        {
            StringBuilder condicion = new StringBuilder();
            foreach (Etiqueta ln in Lineas)
            {
                if (condicion.Length == 0)
                {
                    condicion.Append($"''{ln.DistNumber}''");
                }
                else
                {
                    if (!condicion.ToString().Contains(ln.DistNumber))
                    {
                        condicion.Append($",''{ln.DistNumber}''");
                    }
                }
            }
            return condicion.ToString();
        }

        //Eliminar registro de Master en Lote | Actualizar cantidad Master
        public void EliminarRegM(ref string msj, ref int codigo, string loteReg, string articuloReg, double cantidadReg, string master, double cantidad)
        {
            IDbConnection dbCon = null;
            IDbTransaction tran = null;

            msj = string.Empty;
            codigo = 0;
            int reg = 0;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Actualizar Etiqueta Master " + master, "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    tran = dbCon.BeginTransaction();

                    Logs.FullInfo("", "Actualizar registros Master : \r\n WMSSP_EtqMaster => \t Accion: 9, Subaccion: 1-4," +
                                                                                            ", \t Codigo Master: " + master +
                                                                                            ", \t Lote: " + loteReg +
                                                                                            ", \t Articulo: " + articuloReg +
                                                                                            ", \t Cantidad Lote: " + cantidadReg 
                                                                                            , Usuario, 0, modulo, metodo); //Log Informativo

                    reg = dbCon.Execute($"EXEC \"WMSSP_EtqMaster\" @Accion = 9, @Subaccion = 1, @CodigoMaster = '{master}', @Lote = '{loteReg}', @QtyLote = {cantidadReg}", null, transaction: tran);
                    reg = dbCon.Execute($"EXEC \"WMSSP_EtqMaster\" @Accion = 9, @Subaccion = 2, @CodigoMaster = '{master}', @Lote = '{loteReg}', @QtyLote = {cantidadReg}", null, transaction: tran);
                    reg = dbCon.Execute($"EXEC \"WMSSP_EtqMaster\" @Accion = 9, @Subaccion = 3, @CodigoMaster = '{master}', @Lote = '{loteReg}', @QtyLote = {cantidadReg}", null, transaction: tran);
                    reg = dbCon.Execute($"EXEC \"WMSSP_EtqMaster\" @Accion = 9, @Subaccion = 4, @CodigoMaster = '{master}', @Lote = '{loteReg}', @QtyLote = {cantidadReg}", null, transaction: tran);

                    tran.Commit();
                    codigo = 0;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                codigo = -1;
                msj = ex.Message;
                tran.Rollback();
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cierre de Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Actualizar Etiqueta Master " + master, "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Actualizar información de Master
        public bool ActualizarMasterWMS(ref string msj, string Master, Etiqueta RegMaster)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            int reg = 0;
            msj = string.Empty;

            Logs.FullInfo("Actualizar Etiqueta Master WMS " + Master, "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                using (dbCon = new SqlConnection(CadReg))
                {
                    dbCon.Open();
                    Logs.FullInfo("", "Actualizar Master: \r\n WMSSP_EtqMaster => \t Accion = 10, CodigoMaster: " + Master +
                                                                                ", \t Artículo: " + RegMaster.ItemCode +
                                                                                ", \t Cantidad: " + RegMaster.OnHandQty +
                                                                                ", \t Turno Etq: " + RegMaster.TurnoMaster +
                                                                                ", \t Usuario: " + Usuario,
                                                                                   Usuario, 0, modulo, metodo); //Log Informativo

                    reg += dbCon.Execute(@"EXEC ""WMSSP_EtqMaster"" @Accion = 10, @CodigoMaster = '" + Master + "', @QtyLote = " + RegMaster.OnHandQty + ", @Usuario = '" + Usuario + "'");
                    Logs.FullInfo("", "Master Actualizada => " + Master + "\r\n", Usuario, 0, modulo, metodo); //Log Informativo
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                msj = ex.Message.ToString();
                return false;
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();

                    Logs.FullInfo("", "Cerrar Conexion a BD", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Actualizar Etiqueta Master WMS " + Master, "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Actualizar Etiqueta Master | Liberar Etiqueta Master
        public void LiberarMaster(ref string msj, ref int codeR, string EtqMaster, string Articulo, int OrdenFab)
        {
            int response = 0;
            msj = string.Empty;
            codeR = 0;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            IDbConnection dbCon = null;
            try
            {
                if (!string.IsNullOrEmpty(EtqMaster))
                {
                    using (dbCon = new SqlConnection(CadReg))
                    {
                        dbCon.Open();
                        Logs.FullInfo("", ">>> WMSSP_EtqMaster @Accion = 14, @CodigoMaster = " + EtqMaster + ", @ItemCode = " + Articulo + ", @OrdenFab = " + OrdenFab, Usuario, 0, modulo, metodo); //Log Informativo
                        response = dbCon.Execute($"EXEC \"WMSSP_EtqMaster\" @Accion = 14, @CodigoMaster = '{EtqMaster}', @ItemCode = '{Articulo}', @OrdenFab = {OrdenFab}");
                        codeR = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                msj = ex.Message.ToString();
                codeR = -1;
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

        public void EliminarMasterBorrador(string EtqMaster)
        {
            IDbConnection dbCon = null;
            int response = 0;
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = Identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            try
            {
                if (!string.IsNullOrEmpty(EtqMaster))
                {
                    using (dbCon = new SqlConnection(CadReg))
                    {
                        dbCon.Open();
                        Logs.FullInfo("", ">>> WMSSP_WMSAgregar @Accion = 94, @Lote = '" + EtqMaster + "', @Usuario = '" + Usuario + "'", Usuario, 0, modulo, metodo); //Log Informativo
                        response = dbCon.Execute($"EXEC \"WMSSP_WMSAgregar\" @Accion = 94, @Lote = '{EtqMaster}', @Usuario = '{Usuario}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
            }
            finally
            {
                if (dbCon != null || dbCon.State == ConnectionState.Open)
                {
                    dbCon.Close();
                    dbCon.Dispose();
                    dbCon = null;
                }
            }
        }

    }
}