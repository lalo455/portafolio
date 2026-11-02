-- ================================
-- Create User-defined Table Type
-- ================================
USE		CLIENTADB2PRUEBAS
GO

DROP TYPE UTT_COTIZACIONES

-- Create the data type
CREATE TYPE UTT_COTIZACIONES AS TABLE 
(
	NoCotizacionSAP int NULL
	,DocStatus char(1)
	, DocDate datetime NULL
	, Cliente nvarchar(300)
	, Itemcode varchar(200) NULL
	, ItemName varchar(300) NULL
	, Cotizado numeric(19,6) NULL
	, UMVta varchar(100)
	, Precio float
	, Total float
	, Direccion nvarchar(254) null
	, NumPedido nvarchar(15) null
	, Comentarios nvarchar(254) null
	, DocDueDate datetime null
	, Descuento1 numeric(19,6) null
	, Descuento2 numeric(19,6) null
	, LineNum int null
	, UnitSize nvarchar(100) null
	, VATLiable char(1) null
	, IndirctTax char(1) null
	, TaxCodeAR nvarchar(8) null
	, ValorImpuesto float
	,Comprometido float
)
GO
