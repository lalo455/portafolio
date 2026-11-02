USE [CLIENTADB2PRUEBAS]
GO
/****** Object:  StoredProcedure [dbo].[SP_Consulta_Cotizaciones]    Script Date: 28/06/2022 01:53:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[SP_Consulta_Cotizaciones](
	@DocNum int,
	@UserId nvarchar(20),
	@Tipo nvarchar(10),
	@Fecha DATETIME,
	@Articulo nvarchar(20)
	) AS

/*Eliminacion de Tablas Temporales en caso de Existir*/
DECLARE @Cotizaciones UTT_COTIZACIONES

--Cotizaciones abiertas
INSERT INTO @Cotizaciones
SELECT
  "DocNum"
, "DocStatus"
, "DocDate"
, "CardCode"
, "ItemCode"
, "Dscription"
, "Quantity"
, "UnitMsr"
, "Price"
, "LineTotal"
, "Address"
, "U_NumPedido"
, "Comments"
, "DocDueDate"
, "U_Desc1"
, "U_Desc2"
, "LineNum"
FROM
(
	SELECT 
	P."DocEntry" as "DocNum"
	, P."DocStatus"
	, P."DocDate"
	, (P."CardCode"+' - '+P."CardName") "CardCode" --Se agrega Nombre de Cliente IUO 30/10/2020
	, P1."ItemCode"
	, P1."Dscription"
	, P1."Quantity"
	, CASE WHEN ISNULL(P1."UomEntry", -1) > 0 THEN REPLACE(REPLACE(REPLACE(P1."UomCode", CHAR(9), ' '), CHAR(10), ' '), CHAR(13), ' ')
			WHEN LEN(ISNULL(P1."unitMsr", '')) > 0 THEN REPLACE(REPLACE(REPLACE(P1."unitMsr", CHAR(9), ' '), CHAR(10), ' '), CHAR(13), ' ')
		ELSE 
		REPLACE(REPLACE(REPLACE(UPPER(I."SalUnitMsr"), CHAR(9), ' '), CHAR(10), ' '), CHAR(13), ' ')
		END "UnitMsr"
	, P1."Price"
	, P1."LineTotal"
	, P."Address"
	, P."U_NumPedido"
	, P."Comments"
	, P."DocDueDate"
	, P1."U_Desc1"
	, P1."U_Desc2"
	, P1."LineNum"
	FROM "OQUT" P
	INNER JOIN	"QUT1" P1	ON P."DocEntry"		= P1."DocEntry"
	INNER JOIN	"OITM" I	ON P1."ItemCode"	= I."ItemCode"
	WHERE		--P."DocStatus" = 'O' --Se quita condicion para mostrar solo Ordenes Abiertas, mostrar todas las Ordenes
	--AND			
	P."U_UserPortal" = @UserId
) P
GROUP BY
  "DocNum"
, "DocDate"
, "DocStatus"
, "CardCode"
, "ItemCode"
, "Dscription"
, "Quantity"
, "UnitMsr"
, "Price"
, "LineTotal"
, "Address"
, "U_NumPedido"
, "Comments"
, "DocDueDate"
, "U_Desc1"
, "U_Desc2"
, "LineNum"
ORDER BY "DocDate"


if @Tipo = 'Todos'
begin
	SELECT * FROM @Cotizaciones
end
if @Tipo = 'DocNum'
begin
	SELECT * FROM @Cotizaciones WHERE NoCotizacionSAP = @DocNum
end
if @Tipo = 'Articulo'
begin
	SELECT * FROM @Cotizaciones WHERE "Itemcode" = @Articulo
end

if @Tipo = 'Fecha'
begin
	SELECT * FROM @Cotizaciones WHERE "DocDate" >= @Fecha
end
