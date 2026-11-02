using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static WMS.Models.Produccion;

namespace WMS.Models
{
    public class Produccion
    {
        public class Documento
        {
            public int NumOrdr { get; set; }
            public int NumST { get; set; }
            public List<Etiqueta> Lineas { get; set; }
        }
        public class Datos
        {
            public List<DetalleOrden> LineasOF { get; set; }
            public List<DetalleSolicitud> LineasSO { get; set; }
        }
        public class OrdenFabricacion
        {
            public int DocNum { get; set; }
            public string DocDate { get; set; }
            public string ItemCode { get; set; }
            public string Warehouse { get; set; }
        }
        public class DetalleOrden
        {
            public int ItemType { get; set; }
            public string ItemCode { get; set; }
            public string Warehouse { get; set; }
        }
        public class DetalleSolicitud
        {
            public int DocEntry { get; set; }
            public int DocNum { get; set; }
            public string Filler { get; set; }
            public string ToWhsCode { get; set; }
            public int LineNum { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Granel { get; set; }
            public double Quantity { get; set; }
            public int OrdFab { get; set; }
        }
        public class Comparacion
        {
            public string Orden { get; set; }
            public string Solicitud { get; set; }
            public string Coinciden { get; set; }
        }
        public class Ubicacion
        {
            public int AbsEntry { get; set; }
            public string BinCode { get; set; }
        }
        public class EstatusOrden
        {
            public string Name { get; set; }
            public string U_ItemCode { get; set; }
            public double U_QtyEti { get; set; }
            public string U_stEti { get; set; }
            public string U_stCal { get; set; }
            public string Tipo { get; set; }
            public string WhsCode { get; set; }
            public char Cancelado { get; set; }
            public int BinAbs { get; set; }
            public string BinCode { get; set; }
        }
        public class EmisionRecibo
        {
            public string DistNumber { get; set; }
            public string ItemCode { get; set; }
            public double OnHandQty { get; set; }
            public int OF { get; set; }
            public int AbsEntry { get; set; }
            public string BinCode { get; set; }
            public string Warehouse { get; set; }

            //Actualizacion: se agrega Turno y Fecha de Recibo - 12-01-2021 
            public int U_TurnoRecibo { get; set; } 
            public string U_FechaRecibo { get; set; }
            
            public string EtqMaster { get; set; }
            public string TipoLectura { get; set; }
            public string TipoReg { get; set; }
        }
        public class DocumentoAProduccion {
            public string Batch { get; set; }
            public int DocEntry { get; set; }
            public int DocNum { get; set; }
            public string Warehouse { get; set; }
            public string ItemCode { get; set; }
            public double Quantity { get; set; }
            public int AbsEntry { get; set; }

            //Actualizacion: se agrega Turno y Fecha de Recibo - 12-01-2021 
            public int Turno { get; set; }
            public string Fecha { get; set; }

            public string StatusOF { get; set; } //C=Canceled, L=Closed, P=Planned, R=Released
            public string EtqMaster { get; set; } //Registro de Master | solo para registros tipo M
        }
        public class ConsumoOrdenFabricacion
        {
            public int LineNum { get; set; }
            public int ItemType { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public double BaseQty { get; set; }
            public decimal CantReq { get; set; }
            public string Warehouse { get; set; }
            public List<LoteConsumo> Batch { get; set; }

            public string ManBtchNum { get; set; } //Gestionado por Lotes > Y:Si | N:No
        }
        public class LoteConsumo
        {
            public string DistNumber { get; set; }
            public double Quantity { get; set; }
            public decimal Cantidad { get; set; }
            public string MnfSerial { get; set; }
            public int Status { get; set; }
            public int MdAbsEntry { get; set; }
            public double QtyRequerida { get; set; }

            public double Stock { get; set; } //Stock del Lote en SAP
            public double QtyDiferencia { get; set; } //Cantidad faltante
            public double QtyLotes { get; set; } //Cantidad total de los lotes | Acumulado stock
            public string Procede { get; set; } //Procede OF : Y | N
        }
        public class Operadores
        {
            public int empID { get; set; }
            public string Operador { get; set; }

            public string name { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public int teamID { get; set; }            
        }
        public class Turnos
        {
            public int Code { get; set; }
            public string Name { get; set; }
            public double U_Dias { get; set; }
            public double U_Horas { get; set; }
            public string U_Inicio { get; set; }
            public string U_Fin { get; set; }
        }
        public class ValidacionProd
        {
            public string Name { get; set; }
            public string U_ItemCode { get; set; }
            public double U_QtyEti { get; set; }
            public char U_stEti { get; set; }
            public char U_stCal { get; set; }
            public char Cancelado { get; set; }
        }
        public class SL1
        {
            public string SL1Code { get; set; }
        }
        public class SL2
        {
            public string SL2Code { get; set; }
        }
        public class EtiquetaTraspaleo
        {
            public string Batch { get; set; }
            public string FromWhsCode { get; set; }
            public string BinCode { get; set; }
            public int FromAbsEntry { get; set; }
            public int DocEntry { get; set; }
            public int DocNum { get; set; }
            public int ToWhsCode { get; set; }
            public string ItemCode { get; set; }
            public double Quantity { get; set; }
            public int ToAbsEntry { get; set; }
        }
        public class SubAlmacen
        {
            public int SL1Abs { get; set; }
            public string SL1Code { get; set; }
        }
        public class Linea
        {
            public int SL1Abs { get; set; }
            public int SL2Abs { get; set; }
            public string SL2Code { get; set; }
        }
        public class Fila
        {
            public int SL2Abs { get; set; }
            public int SL3Abs { get; set; }
            public string SL3Code { get; set; }
        }
        public class Borrador
        {
            public int OF { get; set; }
            public int TE { get; set; }
            public int P { get; set; }
            public int S { get; set; }
            public int T { get; set; }
        }

    }

    public class Issue { 
        public List<IssueLines> lin { get; set; }
        public Issue() {
            lin = new List<IssueLines>();
        }
    }
    public class IssueLines {
        public int BaseEntry { get; set; }
        public int BaseType { get; set; }
        public int BaseLine { get; set; }
        public double Quantity { get; set; }
        public string WarehouseCode { get; set; }
        public List<IssueBatch> Batchs { get; set; }
        public List<IssueLocation> Locations { get; set; }

        public IssueLines() {
            Batchs = new List<IssueBatch>();
            Locations = new List<IssueLocation>();
        }
    }
    public class IssueBatch {
        public int BaseLineNumber { get; set; }
        public string BatchNumber { get; set; }
        public double Quantity { get; set; }
        public string Location { get; set; }
    }
    public class IssueLocation {
        public int BinAbsEntry { get; set; }
        public double Quantity { get; set; }
        public int SerialAndBatchNumbersBaseLine { get; set; }
        public int BaseLineNumber { get; set; }
        public int Type { get; set; }
    }
    public class Receipt
    {
        public List<ReceiptLines> lin { get; set; }

        public Receipt() {
            lin = new List<ReceiptLines>();
        }
    }
    public class ReceiptLines
    {
        public int BaseType { get; set; }
        public int BaseEntry { get; set; }
        public double Quantity { get; set; }
        public string WarehouseCode { get; set; }
        public string ItemCode { get; set; }
        public string FromWarehouse { get; set; }
        public int Baseline { get; set; }
        public List<IssueBatch> Batchs { get; set; }
        public List<IssueLocation> Locations { get; set; }

        public ReceiptLines() {
            Batchs = new List<IssueBatch>();
            Locations = new List<IssueLocation>();
        }
    }
    public class Fifo
    {
        public DateTime InDate { get; set; }
        public string DistNumber { get; set; }
        public string ItemCode { get; set; }
        public char Etiqueta { get; set; }
        public int Indice { get; set; }
        public int DifFecha { get; set; }
    }
    public class Borrador
    {
        public string DistNumber { get; set; }
        public string ItemCode { get; set; }
        public double OnHandQty { get; set; }
        public int OF { get; set; }
        public int AbsEntry { get; set; }
        public string BinCode { get; set; }
        public string AlmacenSel { get; set; }
        public int Turno { get; set; }
        public string Usuario { get; set; }
        public string FechaCreacion { get; set; }
        public string FechaActualizacion { get; set; }


        //Actualizacion: se agrega Turno y Fecha de Recibo - 12-01-2021 
        public int U_TurnoRecibo { get; set; }
        public string U_FechaRecibo { get; set; }

        public string IdGuid { get; set; }

        public string EtqMaster { get; set; }
        public string TipoLectura { get; set; }
        public string TipoReg { get; set; }
    }

    public class BorradorEtiqueta
    {
        public int OF { get; set; }
        public string Usuario { get; set; }
        public string FechaCreacion { get; set; }
        public int U_TurnoRecibo { get; set; }
        public string TipoLectura {  get; set; }
        public string EtqMaster {  get; set; }
    }

    public class InicialEtiqueta
    {
        public string Estatus { get; set; }
        public string Turno { get; set; }
        public string Fecha { get; set; }
        public string Warehouse { get; set; }
        public string EtqMaster { get; set; }
    }

    public class ItemWhs
    {
        public int LineNum { get; set; }
        public int ItemType { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double BaseQty { get; set; }
        public decimal CantReq { get; set; }
        public string Warehouse { get; set; }
        public List<LoteConsumo> Batch { get; set; }

        public string ManBtchNum { get; set; } //Gestionado por Lotes > Y:Si | N:No
    }

}