using CRMs.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Web.Mvc;

namespace CRMs.DataAccess
{
    public class HomeDataAccess : BaseDataAccess
    {
        private DbCommand   db;
        private DbDataReader dr;
        private int salida;
        public HomeDataAccess()
        {
            db = null;
            dr = null;
            salida = 0;
        }

        public int SetOQUT(OQUT model)
        {
            db = GetCommand("SP_OQUT");
            db.AddParameter(this.Factory, "accion", 1);
            db.AddParameter(this.Factory, "UserId", model.UserId.TrimEnd());
            db.AddParameter(this.Factory, "CardCode", model.CardCode.TrimEnd());
            db.AddParameter(this.Factory, "DocDate", model.DocDate);
            db.AddParameter(this.Factory, "DocDueDate", model.DocDueDate);
            db.AddParameter(this.Factory, "NumAtCard", model.NumAtCard.TrimEnd());
            db.AddParameter(this.Factory, "Comments", model.Comments.TrimEnd());
            db.AddParameter(this.Factory, "Status", model.Status.ToCharArray());
            db.AddParameter(this.Factory, "RefInternas", model.RefInternas.TrimEnd());
            if (!string.IsNullOrEmpty(model.Moneda))
                db.AddParameter(this.Factory, "Moneda", model.Moneda.ToCharArray());
            if (!string.IsNullOrEmpty(model.Contacto))
                db.AddParameter(this.Factory, "Contacto", model.Contacto.ToCharArray());
            db.AddParameter(this.Factory, "DireccionEntrega", model.DireccionEntrega.TrimEnd());
            if (!string.IsNullOrEmpty(model.SlpCode))
                db.AddParameter(this.Factory, "SlpCode", model.SlpCode.ToCharArray());

            if (model.DocEntry!=0)
                db.AddParameter(this.Factory, "DocEntry", model.DocEntry);
            db.AddParameter(this.Factory, "salida", salida).Direction = ParameterDirection.Output;
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                salida = segura(db);
                return salida;
            }
        }

        public int EliminaOQUT(int id)
        {
            db = GetCommand("SP_OQUT");
            db.AddParameter(this.Factory, "accion", 4);
            db.AddParameter(this.Factory, "DocEntry", id);
            db.AddParameter(this.Factory, "salida", salida).Direction = ParameterDirection.Output;
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                salida = segura(db);
                return salida;
            }
        }

        public List<OQUT> ListaOQUT(int id=0)
        {
            db = GetCommand("SP_OQUT");
            if(id!=0)
            {
                db.AddParameter(this.Factory, "accion", 3);
                db.AddParameter(this.Factory, "DocEntry", id);
            }
            else
                db.AddParameter(this.Factory, "accion", 2);
            List<OQUT> lista = new List<OQUT>();
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                dr = seguraRegistro(db);
                while (dr.Read())
                {
                    OQUT m = new OQUT
                    {
                        DocEntry = Convert.ToInt32(dr.Get<decimal>("DocEntry")),
                        UserId = dr.GetString("UserId"),
                        CardCode = dr.GetString("CardCode"),
                        DocDate = dr.Get<DateTime>("DocDate"),
                        DocDueDate = dr.Get<DateTime>("DocDueDate"),
                        NumAtCard = dr.GetString("NumAtCard"),
                        Comments = dr.GetString("Comments"),
                        Status = dr.GetString("Status"),
                        RefInternas = dr.GetString("RefInternas"),
                        Moneda = dr.GetString("Moneda"),
                        Contacto = dr.GetString("Contacto"),
                        DireccionEntrega = dr.GetString("DireccionEntrega"),
                        SlpCode = dr.GetString("SlpCode")
                    };
                    lista.Add(m);
                }
                dr.Close();
                db.Connection.Close();
            }
            return lista;
        }

        public List<QUT1> ListaQUT1(int id = 0)
        {
            db = GetCommand("SP_OQUT");
            if (id != 0)
            {
                db.AddParameter(this.Factory, "accion", 6);
                db.AddParameter(this.Factory, "QUT1PK", id);
            }
            else
                db.AddParameter(this.Factory, "accion", 5);
            List<QUT1> lista = new List<QUT1>();
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                dr = seguraRegistro(db);
                while (dr.Read())
                {
                    QUT1 m = new QUT1
                    {
                        QUT1PK = dr.Get<int>("QUT1PK"),
                        DocEntry = Convert.ToInt32(dr.Get<decimal>("DocEntry")),
                        UserId = dr.GetString("UserId"),
                        ItemCode = dr.GetString("ItemCode"),
                        Subs = dr.GetString("Subs"),
                        Quantity = Convert.ToSingle(dr.GetString("Quantity")),
                        Almacen = dr.GetString("Almacen"),
                        Comments = dr.GetString("Comments"),
                        Price = Convert.ToSingle(dr.GetString("Price")),
                        Tax = Convert.ToSingle(dr.GetString("Tax")),
                        Total = Convert.ToSingle(dr.GetString("Total")),
                        Espe = dr.GetString("Espe"),
                        Name = dr.GetString("Name"),
                        EntregaVta = dr.Get<DateTime>("EntregaVta"),
                        EntregaEsti = dr.Get<DateTime>("EntregaEsti"),
                        Clave = dr.GetString("Clave"),
                        DescLarga = dr.GetString("DescLarga"),
                        DiasEntrega = dr.GetString("DiasEntrega"),
                        Status = dr.GetString("Status"),
                    };
                    lista.Add(m);
                }
                dr.Close();
                db.Connection.Close();
            }
            return lista;
        }

        public List<SelectListItem> SelectOQUT(int id = 0)
        {
            db = GetCommand("SP_OQUT");
            db.AddParameter(this.Factory, "accion", 2);
            List<SelectListItem> lista = new List<SelectListItem>();
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                dr = seguraRegistro(db);
                while (dr.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = Convert.ToInt32(dr.Get<decimal>("DocEntry")).ToString(),
                        Text = Convert.ToInt32(dr.Get<decimal>("DocEntry")).ToString()
                    });
                }
                dr.Close();
                db.Connection.Close();
            }
            return lista;
        }

        public int SetQUT1(QUT1 model)
        {
            db = GetCommand("SP_OQUT");
            db.AddParameter(this.Factory, "DocEntry", model.DocEntry);
            db.AddParameter(this.Factory, "ItemCode", model.ItemCode.TrimEnd());
            db.AddParameter(this.Factory, "Subs", model.Subs.ToCharArray());
            db.AddParameter(this.Factory, "Quantity", model.Quantity);
            db.AddParameter(this.Factory, "Almacen", model.Almacen.ToCharArray());
            db.AddParameter(this.Factory, "Comments", model.Comments.TrimEnd());
            db.AddParameter(this.Factory, "Status", model.Status.ToCharArray());
            db.AddParameter(this.Factory, "Price", model.Price);
            db.AddParameter(this.Factory, "Tax", model.Tax);
            db.AddParameter(this.Factory, "Total", model.Total);
            db.AddParameter(this.Factory, "Espe", model.Espe.TrimEnd());
            db.AddParameter(this.Factory, "Name", model.Name.TrimEnd());
            db.AddParameter(this.Factory, "EntregaVta", model.EntregaVta);
            db.AddParameter(this.Factory, "EntregaEsti", model.EntregaEsti);
            db.AddParameter(this.Factory, "Clave", model.Clave.ToCharArray());
            db.AddParameter(this.Factory, "DescLarga", model.DescLarga.ToCharArray());
            db.AddParameter(this.Factory, "DiasEntrega", model.DiasEntrega.ToCharArray());
            if(model.QUT1PK!=0)
                db.AddParameter(this.Factory, "QUT1PK", model.QUT1PK);
            db.AddParameter(this.Factory, "accion", 7);
            db.AddParameter(this.Factory, "salida", salida).Direction = ParameterDirection.Output;
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                salida = segura(db);
                return salida;
            }
        }

        public int EliminaQUT1(int id)
        {
            db = GetCommand("SP_OQUT");
            db.AddParameter(this.Factory, "accion", 8);
            db.AddParameter(this.Factory, "QUT1PK", id);
            db.AddParameter(this.Factory, "salida", salida).Direction = ParameterDirection.Output;
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                salida = segura(db);
                return salida;
            }
        }

        public List<ORDR> ListaORDR(int id = 0)
        {
            db = GetCommand("SP_ORDR");
            if (id != 0)
            {
                db.AddParameter(this.Factory, "accion", 2);
                db.AddParameter(this.Factory, "DocEntry", id);
            }
            else
                db.AddParameter(this.Factory, "accion", 1);
            List<ORDR> lista = new List<ORDR>();
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                dr = seguraRegistro(db);
                while (dr.Read())
                {
                    ORDR m = new ORDR
                    {
                        DocEntry = Convert.ToInt32(dr.Get<decimal>("DocEntry")),
                        UserId = dr.GetString("UserId"),
                        CardCode = dr.GetString("CardCode"),
                        DocDate = dr.Get<DateTime>("DocDate"),
                        DocDueDate = dr.Get<DateTime>("DocDueDate"),
                        NumAtCard = dr.GetString("NumAtCard"),
                        Comments = dr.GetString("Comments"),
                        Status = dr.GetString("Status"),
                        RefInternas = dr.GetString("RefInternas"),
                        Moneda = dr.GetString("Moneda"),
                        Contacto = dr.GetString("Contacto"),
                        DireccionEntrega = dr.GetString("DireccionEntrega"),
                        SlpCode = dr.GetString("SlpCode")
                    };
                    lista.Add(m);
                }
                dr.Close();
                db.Connection.Close();
            }
            return lista;
        }

        public int SetORDR(ORDR model)
        {
            db = GetCommand("SP_ORDR");
            db.AddParameter(this.Factory, "accion", 3);
            db.AddParameter(this.Factory, "UserId", model.UserId.TrimEnd());
            db.AddParameter(this.Factory, "CardCode", model.CardCode.TrimEnd());
            db.AddParameter(this.Factory, "DocDate", model.DocDate);
            db.AddParameter(this.Factory, "DocDueDate", model.DocDueDate);
            db.AddParameter(this.Factory, "NumAtCard", !string.IsNullOrEmpty(model.NumAtCard) ? model.NumAtCard.TrimEnd() : "");
            db.AddParameter(this.Factory, "Comments", !string.IsNullOrEmpty(model.Comments)   ? model.Comments.TrimEnd() : "");
            db.AddParameter(this.Factory, "Status", model.Status.ToCharArray());
            db.AddParameter(this.Factory, "RefInternas", !string.IsNullOrEmpty(model.RefInternas) ? model.RefInternas.TrimEnd() : "");
            if (!string.IsNullOrEmpty(model.Moneda))
                db.AddParameter(this.Factory, "Moneda", model.Moneda.ToCharArray());
            if (!string.IsNullOrEmpty(model.Contacto))
                db.AddParameter(this.Factory, "Contacto", model.Contacto.ToCharArray());
            db.AddParameter(this.Factory, "DireccionEntrega", model.DireccionEntrega.TrimEnd());
            if (!string.IsNullOrEmpty(model.SlpCode))
                db.AddParameter(this.Factory, "SlpCode", model.SlpCode.ToCharArray());

            if (model.DocEntry != 0)
                db.AddParameter(this.Factory, "DocEntry", model.DocEntry);
            db.AddParameter(this.Factory, "salida", salida).Direction = ParameterDirection.Output;
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                salida = segura(db);
                return salida;
            }
        }

        public int EliminaORDR(int id)
        {
            db = GetCommand("SP_ORDR");
            db.AddParameter(this.Factory, "accion", 4);
            db.AddParameter(this.Factory, "DocEntry", id);
            db.AddParameter(this.Factory, "salida", salida).Direction = ParameterDirection.Output;
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                salida = segura(db);
                return salida;
            }
        }

        public List<RDR1> ListaRDR1(int id = 0)
        {
            db = GetCommand("SP_ORDR");
            if (id != 0)
            {
                db.AddParameter(this.Factory, "accion", 6);
                db.AddParameter(this.Factory, "RDR1PK", id);
            }
            else
                db.AddParameter(this.Factory, "accion", 5);
            List<RDR1> lista = new List<RDR1>();
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                dr = seguraRegistro(db);
                while (dr.Read())
                {
                    RDR1 m = new RDR1
                    {
                        RDR1PK = dr.Get<int>("RDR1PK"),
                        DocEntry = Convert.ToInt32(dr.Get<decimal>("DocEntry")),
                        UserId = dr.GetString("UserId"),
                        ItemCode = dr.GetString("ItemCode"),
                        Subs = dr.GetString("Subs"),
                        Quantity = Convert.ToSingle(dr.GetString("Quantity")),
                        Almacen = dr.GetString("Almacen"),
                        Comments = dr.GetString("Comments"),
                        Status = dr.GetString("Status"),
                        Price = Convert.ToSingle(dr.GetString("Price")),
                        Tax = Convert.ToSingle(dr.GetString("Tax")),
                        Total = Convert.ToSingle(dr.GetString("Total")),
                        BaseEntry = Convert.ToInt32(dr.GetString("BaseEntry")),
                        Espe = dr.GetString("Espe"),
                        Name = dr.GetString("Name"),
                        EntregaVta = dr.Get<DateTime>("EntregaVta"),
                        EntregaEsti = dr.Get<DateTime>("EntregaEsti"),
                        Clave = dr.GetString("Clave"),
                        DescLarga = dr.GetString("DescLarga"),
                        DiasEntrega = dr.GetString("DiasEntrega"),
                
                    };
                    lista.Add(m);
                }
                dr.Close();
                db.Connection.Close();
            }
            return lista;
        }

        public List<SelectListItem> SelectRDR(int id = 0)
        {
            db = GetCommand("SP_ORDR");
            db.AddParameter(this.Factory, "accion", 1);
            List<SelectListItem> lista = new List<SelectListItem>();
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                dr = seguraRegistro(db);
                while (dr.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = Convert.ToInt32(dr.Get<decimal>("DocEntry")).ToString(),
                        Text = Convert.ToInt32(dr.Get<decimal>("DocEntry")).ToString()
                    });
                }
                dr.Close();
                db.Connection.Close();
            }
            return lista;
        }

        public int SetRDR1(RDR1 model)
        {
            db = GetCommand("SP_ORDR");
            db.AddParameter(this.Factory, "DocEntry", model.DocEntry);
            db.AddParameter(this.Factory, "ItemCode", !String.IsNullOrEmpty(model.ItemCode) ? model.ItemCode.TrimEnd():"");
            if(!String.IsNullOrEmpty(model.Subs))
                db.AddParameter(this.Factory, "Subs", model.Subs.ToCharArray());
            db.AddParameter(this.Factory, "Quantity", model.Quantity);
            if (!String.IsNullOrEmpty(model.Almacen))
                db.AddParameter(this.Factory, "Almacen", model.Almacen.ToCharArray());
            db.AddParameter(this.Factory, "Comments", !String.IsNullOrEmpty(model.Comments) ? model.Comments.TrimEnd() : "");
            if (!String.IsNullOrEmpty(model.Status))
                db.AddParameter(this.Factory, "Status", model.Status.ToCharArray());
            db.AddParameter(this.Factory, "Price", model.Price);
            db.AddParameter(this.Factory, "Tax", model.Tax);
            db.AddParameter(this.Factory, "Total", model.Total);
            db.AddParameter(this.Factory, "BaseEntry", model.BaseEntry);
            db.AddParameter(this.Factory, "Espe", !String.IsNullOrEmpty(model.Espe) ? model.Espe.TrimEnd() : "");
            db.AddParameter(this.Factory, "Name", !String.IsNullOrEmpty(model.Name) ? model.Name.TrimEnd() : "");
            db.AddParameter(this.Factory, "EntregaVta", model.EntregaVta);
            db.AddParameter(this.Factory, "EntregaEsti", model.EntregaEsti);
            if (!String.IsNullOrEmpty(model.Clave))
                db.AddParameter(this.Factory, "Clave", model.Clave.ToCharArray());
            if (!String.IsNullOrEmpty(model.DescLarga))
                db.AddParameter(this.Factory, "DescLarga", model.DescLarga.ToCharArray());
            if (!String.IsNullOrEmpty(model.Clave))
                db.AddParameter(this.Factory, "DiasEntrega", model.DiasEntrega.ToCharArray());
            if (model.RDR1PK != 0)
                db.AddParameter(this.Factory, "RDR1PK", model.RDR1PK);
            db.AddParameter(this.Factory, "accion", 7);
            db.AddParameter(this.Factory, "salida", salida).Direction = ParameterDirection.Output;
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                salida = segura(db);
                return salida;
            }
        }

        public int EliminaRDR1(int id)
        {
            db = GetCommand("SP_ORDR");
            db.AddParameter(this.Factory, "accion", 8);
            db.AddParameter(this.Factory, "RDR1PK", id);
            db.AddParameter(this.Factory, "salida", salida).Direction = ParameterDirection.Output;
            using (db.Connection = ConnectionManager.CreateNewConnection())
            {
                salida = segura(db);
                return salida;
            }
        }
    }
}
