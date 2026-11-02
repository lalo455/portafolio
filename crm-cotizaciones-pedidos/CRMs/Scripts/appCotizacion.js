var appCotizacion = angular.module('appCotizacion', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

//https://ciphertrick.com/2015/06/01/search-sort-and-pagination-ngrepeat-angularjs/
//https://fritz.ninja/angular-growl-2/
appCotizacion.factory('Ruta', function () {
    return { path: '' }//AQUI VA ALGO carpeta de proyecto en IIS
});

appCotizacion.config(['growlProvider', function (growlProvider) {
    growlProvider.globalPosition('bottom-center');
    //growlProvider.globalReversedOrder(true);
    growlProvider.globalTimeToLive({ success: 20000, error: 10000, warning: 10000 })
}]);


appCotizacion.run(['editableOptions', 'editableThemes', function (editableOptions, editableThemes) {
    editableOptions.theme = 'bs3';
    editableThemes.bs3.dateTpl = '<input type="text" class="form-control input-sm" uib-datepicker-popup="yyyy-MM-dd">';
}]);

appCotizacion.service('Servicio', function ($http, Ruta, $log) {
    //Servicios AppPedidoMostrarModalITEM
    return {

        ObtenerListaSocioDeNegocios: function (opcion) {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerSociosDeNegocios", { opcion: opcion }).then(function (respuestaCustomer) {
                return respuestaCustomer.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtner los Socios de Negocios de SAP" }
                };
            })
        },

        ObtenerArticulosdeSAP2: function (codigoCliente, rol, itemCode, itemName, bd) {

            return $http.post(Ruta.path + "/LogicaPedido/ObtenerArticulosdeSAP2", { cardCode: codigoCliente, rol: rol, itemCode: itemCode, itemName: itemName, bd: bd }).then(function (respuestaITEM) {
                return respuestaITEM.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener los Articulos de SAP" }
                };
            })
        },
        ObtenerArticulosSAP: function (codigoCliente, codigoAlmacen, pLista, descuento) {

            return $http.post(Ruta.path + "/LogicaPedido/ObtenerArticulosdeSAP", { cardCode: codigoCliente, whsCode: codigoAlmacen, priceList: pLista, descuento: descuento }).then(function (respuestaITEM) {
                return respuestaITEM.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener los Articulos de SAP" }
                };
            })
        },

        ObtenerArticulosSAPN: function (codigoCliente, codigoAlmacen, pLista, descuento, filtro) {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerArticulosdeSAPN", {
                cardCode: codigoCliente,
                whsCode: codigoAlmacen,
                priceList: pLista,
                descuento: descuento,
                filtro: filtro
            }).then(function (respuestaITEM) {
                return respuestaITEM.data;
            });
        },
        ObtenerArticulosSAPVendedor: function (codigoCliente, codigoAlmacen, pLista) {

            return $http.post(Ruta.path + "/LogicaPedido/ObtenerArticulosSAPVendedor", { cardCode: codigoCliente, whsCode: codigoAlmacen, priceList: pLista }).then(function (respuestaITEM) {
                return respuestaITEM.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener los Articulos de SAP" }
                };
            })
        },

        ObtenerTasasDeCambioSAP: function (date) {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerTasasDeCambio", { fecha: date }).then(function (respuestaTasas) {
                return respuestaTasas.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener las Tasas de Cambio" }
                }
            })
        },

        ObtenerImpuestoSAP: function (taxCode) {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerImpuestoDeSAP", { TaxCode: taxCode }).then(function (respuestaTax) {
                return respuestaTax.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener el impuesto de SAP" }
                };
            })
        },

        ObtenerCodigoImpuestoBP: function (codeBP) {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerCodigoImpuestoBP", { CardCode: codeBP }).then(function (respuestaTaxBP) {
                return respuestaTaxBP.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener el impuesto del Socio de Negocios" }
                }
            })
        },

        ObtenerImpuestoDefault: function () {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerImpuestoDefault").then(function (respuestaDefault) {
                return respuestaDefault.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener el impuesto Default" }
                }
            })
        },

        ObtenerAlmacenDefault: function () {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerAlmacen").then(function (respuestaDefault) {
                return respuestaDefault.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener el impuesto Default" }
                }
            })
        },

        IntegrarOrdenSAP: function (pedido) {
            return $http.post(Ruta.path + "/IntegracionSAP/IntegrarOrden", { jsonPedido: pedido }).then(function (respuestaIntegracion) {
                return respuestaIntegracion.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al integrar la orden" }
                };
            })
        },

        EnviarCierreCotizacion: function (motivo, docEntry) {
            return $http.post(Ruta.path + "/IntegracionSAP/CerrarOrden", { DocEntry: docEntry, Motivo: motivo }).then(function (respuestaIntegracion) {
                return respuestaIntegracion.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al integrar la orden" }
                };
            }
            )
        },

        ConvertirOrdenAPedido: function (pedido) {
            return $http.post(Ruta.path + "/IntegracionSAP/ConvertirCotizacionAPedido", { jsonPedido: pedido }).then(function (respuestaIntegracion) {
                return respuestaIntegracion.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al integrar la orden" }
                };
            })
        },
        actualizarOrdenSAP: function (pedido) {
            return $http.post(Ruta.path + "/IntegracionSAP/ActualizarOrden", { jsonPedido: pedido }).then(function (respuestaIntegracion) {
                return respuestaIntegracion.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al integrar la orden" }
                };
            })
        },

        ConsultaPedido: function (docNum, userCode, type, dateOrder, item) {

            return $http.post(Ruta.path + "/Reportes/ConsultarPedidos", { DocNum: docNum, UserCode: userCode, Type: type, DateOrder: dateOrder, Item: item }).then(function (respuestaConsulta) {
                return respuestaConsulta.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al consultar pedidos" }
                };
            });
        },
        ConsultaFactura: function (docNum, userCode, type, dateOrder, item, year, mes) {

            return $http.post(Ruta.path + "/Reportes/ConsultarFacturas", { DocNum: docNum, UserCode: userCode, Type: type, DateOrder: dateOrder, Item: item, Year: year, Mes: mes }).then(function (respuestaConsulta) {
                return respuestaConsulta.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al consultar facturas" }
                };
            });
        },

        ConsultaCotizacion: function (docNum, userCode, type, dateOrder, item, opcion) {

            return $http.post(Ruta.path + "/Reportes/ConsultarCotizaciones", { DocNum: docNum, UserCode: userCode, Type: type, DateOrder: dateOrder, Item: item, opcion: opcion }).then(function (respuestaConsulta) {
                return respuestaConsulta.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al consultar pedidos" }
                };
            });
        },
        ConsultaVentasDeUsuario: function (docNum, userCode, type, dateOrder, item) {

            return $http.post(Ruta.path + "/Reportes/ConsultarVentasPorUsuario", { DocNum: docNum, UserCode: userCode, Type: type, DateOrder: dateOrder, Item: item }).then(function (respuestaConsulta) {
                return respuestaConsulta.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al consultar pedidos" }
                };
            });
        },

        ConsultarVistaPrevia: function () {
            return $http.post(Ruta.path + "/Reportes/ImprimirVistaP").then(function (respuestaImpresion) {
                return respuestaImpresion.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al cargar vistaprevia" }
                }
            })
        },
        ConsultarInventario: function (GrpCode, ByItm, ByDesc, ItmCod, rol, bd) {

            return $http.post(Ruta.path + "/Reportes/ObtenerInventarioDeArticulo", { Grupo: GrpCode, PorItem: ByItm, PorDesc: ByDesc, ItemCode: ItmCod, rol: rol, bd: bd }).then(function (respuestaInventario) {
                return respuestaInventario.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al consultar información del artículo." }
                };
            });
        },

        ImprimirReportePedidos: function (idPedido) {
            return $http.post(Ruta.path + "/IntegracionSAP/ImprimirReporte", { DocEntry: idPedido }).then(function (respuestaReporte) {
                return respuestaReporte.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "No se pudo imprimir el reporte" }
                }
            });
        },

        imprimirReportePendientesSucursal: function (docEntry) {
            var config = {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded;charset=utf-8;'
                }
            };

            //var data =
            //    $.param(
            //    {
            //        "docEntrySAP": docEntry
            //    });

            return $http.post(Ruta.path + '/IntegracionSAP/ImprimirReporte', { docEntrySAP: docEntry }, config).then(function (response) {
                return response.data;
            });
        },

        VerificarSesion: function () {
            return $http.post(Ruta.path + "/IntegracionSAP/EstadoSesion").then(function (respSesion) {
                return respSesion.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "No se pudo imprimir el reporte" }
                }
            });
        },

        EnviarPdfStream: function (PdfObject) {

            return $http({
                url: Ruta.path + '/IntegracionSAP/RegresarStringBase64',
                method: 'POST',
                data: {
                    Obj: JSON.stringify(PdfObject)
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                }
            }).then(function (response) {
                return response.data;
            }, function (response) {
                var error = { codigo: -1, mensaje: { "Error en Servicio 4": response } }
                return error;
            });
        },

        ObtenerDirecciones: function (_cardcode) {
            return $http({
                url: Ruta.path + '/LogicaPedido/ListarDirecciones',
                method: 'POST',
                data: {
                    cardcode: _cardcode
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                }
            }).then(function (response) {
                return response.data;
            }, function (response) {
                var error = { codigo: -1, mensaje: { "Error en Servicio 4": response } }
                return error;
            });
        },
        DatosIniciales: function () {
            return $http.post(Ruta.path + "/LogicaPedido/DatosIniciales").then(function (respuestaDefault) {
                return respuestaDefault.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener dato inicial" }
                }
            })
        },

        ObtenerClineteDeNegocios: function () {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerClienteDeNegocios", {}).then(function (respuestaCustomer) {
                return respuestaCustomer.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtner los Socios de Negocios de SAP" }
                };
            })
        },

        ObtenerStockalamcen(codigoCliente, itemcode, rol) {

            return $http.post(Ruta.path + "/LogicaPedido/ObtenerStockalamcen", { cardCode: codigoCliente, itemcode: itemcode, rol: rol }).then(function (respuestaITEMS) {
                return respuestaITEMS.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener los almacénes con stock de SAP" }
                };
            })
        },

        ObtenerPorcentaje: function () {
            return $http.post(Ruta.path + "/Reportes/ObtenerPorcentajeVentas").then(function (PorcentajeVenta) {
                return PorcentajeVenta.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener el porcentaje de ventas año" }
                };
            })

        },
        VistaPrevia: function (pedido, g) {
            return $http.post(Ruta.path + "/Reportes/VistaPreviaOrden", { jsonPedido: pedido, g }).then(function (respuestaVistaPrevia) {
                return respuestaVistaPrevia.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al realizar la vista previa" }
                };
            })
        },
        ListarFormasPago: function (_cardcode) {
            return $http({
                url: Ruta.path + '/LogicaPedido/ListarFormasPago',
                method: 'POST',
                data: {
                    cardcode: _cardcode
                },
                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                }
            }).then(function (response) {
                return response.data;
            }, function (response) {
                var error = { codigo: -1, mensaje: { "Error en Servicio 4": response } }
                return error;
            });
        },
        ListarCFDI: function () {
            return $http({
                url: Ruta.path + '/LogicaPedido/ListarCDFI',
                method: 'POST',

                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                }
            }).then(function (response) {
                return response.data;
            }, function (response) {
                var error = { codigo: -1, mensaje: { "Error en Servicio 4": response } }
                return error;
            });
        },

        ListarFormaEnvio: function () {
            return $http({
                url: Ruta.path + '/LogicaPedido/ListarFormaEnvio',
                method: 'POST',

                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                }
            }).then(function (response) {
                return response.data;
            }, function (response) {
                var error = { codigo: -1, mensaje: { "Error en Servicio 4": response } }
                return error;
            });
        },
        ListarMetodoPago: function () {
            return $http({
                url: Ruta.path + '/LogicaPedido/ListarMetodoPago',
                method: 'POST',

                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                }
            }).then(function (response) {
                return response.data;
            }, function (response) {
                var error = { codigo: -1, mensaje: { "Error en Servicio 4": response } }
                return error;
            });
        },
        ListarTipoEnvio: function () {
            return $http({
                url: Ruta.path + '/LogicaPedido/ListarTipoEnvio',
                method: 'POST',

                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                }
            }).then(function (response) {
                return response.data;
            }, function (response) {
                var error = { codigo: -1, mensaje: { "Error en Servicio 4": response } }
                return error;
            });
        },

        //******************************************************************************/
        ValidarDocCliente(tipoDoc, DocNum, tipo, Cliente) {
            return $http.post(Ruta.path + "/LogicaPedido/ValidarDocCliente", { tipoDoc: tipoDoc, DocNum: DocNum, tipo: tipo, Cliente: Cliente }).then(function (respuesta) {
                return respuesta.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al validar Documento" }
                };
            })
        }
    }
});

//////::::::::::INICIO:::::::::::SECCION CONSULTA COTIZACIONES ::::::::::::::::::::::::::::
appCotizacion.controller('consultaCotizacionController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl, $window) {
    //VARIABLES
    $scope.accion = '1';
    $scope.TipoConsultaC = { Tipo: 'Todos' };
    $scope.ListaTipoConsultaC = [
        { value: 'Todos', text: 'Todos' },
        { value: 'DocNum', text: 'No. Documento' },
        { value: 'Articulo', text: 'Por Artículo' },
        { value: 'Fecha', text: 'Por Fecha' }
    ];

    $scope.ValorConsultaC = "";
    $scope.ListaCotizaciones = [];
    $scope.VisualizarFecha = false;
    $scope.QryConsultaCotizacion = "";
    $scope.QtyConsultaCotizacion = 8;

    $scope.rol = ''; //Obtene y almacena el rol del usuario | se ejecuta cada se recarga la pagina
    $scope.tipo = 'N';

    //*************************************************************************************************/
    ///Variables globales dentro del alcance de VistaPedio.cshtml///   
    //calculo de Fecha del SISTEMA
    var fecha = new Date();
    var mes = fecha.getMonth() + 1;
    var dia = fecha.getDate();
    var hora = fecha.getHours();

    //console.log(hora);
    //console.log(mes);

    var fechaCompleta = fecha.getFullYear() + '-' + (mes < 10 ? '0' : '') + mes + '-' + (dia < 10 ? '0' : '') + dia;
    var FechaMaxEnvio = "";
    $scope.searchTextPedido;
    $scope.MotivoCierreCotizacion = "";
    $scope.DocStatus = "";
    $scope.customerBP = {};//socio de negocios elejido desde modal
    $scope.itemOITM = {};//articulo elejido desde la modal
    $scope.WareHoseCode = "";//variable para el codigo de almacen
    $scope.ListaItemDetalle = [];
    $scope.Impuesto = 0;
    $scope.Descuento = 0;
    $scope.TotalAPagar = 0;
    $scope.SubttlAPagar = 0;
    $scope.TotalEnImpuesto = 0;
    $scope.TotalEnImpuestoStr = "";
    $scope.TotalUnidades = 0;
    $scope.TotalAPagarDisplay = 0;
    $scope.SbttlAPagarDisplay = "";
    $scope.FechaSistema = ""; //Fecha de entrega Pedido
    $scope.FechaSistemaCot = ""; //Fecha de vencimiento Cotizacion
    $scope.ComentariosPedido = "";
    $scope.MensajeDelSistema = "";
    $scope.MostrarMensaje = false;
    $scope.DirCotizacionCargada = "";
    $scope.rol = ''; //Obtene y almacena el rol del usuario | se ejecuta cada se recarga la pagina
    $scope.saldo = 0;
    $scope.credito = 0;
    $scope.saldoSt = 0;
    $scope.creditoSt = 0;
    $scope.statusMetodoPago = "";
    $scope.respuestaSN = [{ CardCode: "C0001", CardName: "Demo Customer", Credito: 0, DiscInRet: "N", Discount: 0, EstatusMetodPago: null, ListNum: 19, Moneda: "##" }, { CardCode: "C0001", CardName: "Demo Customer", Credito: 0, DiscInRet: "N", Discount: 0, EstatusMetodPago: null, ListNum: 13, Moneda: "MXP", Saldo: 0 }];

    //-------------------------------------------------------------
    $scope.Base = 'Cotizacion';
    $scope.DocBase = [{ Tipo: 'Pedido', Documento: 'Orden de Venta' }, { Tipo: 'Cotizacion', Documento: 'Cotización' }]; //Opciones de Tipo de Documento
    $scope.DocBaseEnvio = [{ Tipo: 'C', Documento: 'Completo' }, { Tipo: 'p', Documento: 'Parcial' }]; //Opciones de forma de envio
    $scope.DocBasePago = [{ Tipo: 'PUE', Documento: 'Pago en una exibición' }, { Tipo: 'PPD', Documento: 'Pago en parcialidades' }]; //Opciones de metodo de pago
    $scope.DocBaseTPago = [{ Tipo: '09', Documento: 'Efectivo' }, { Tipo: '03', Documento: 'Transferencia de fondos ' }]; //Opciones de Tipo de pago
    $scope.DocBaseFEnvio = [{ Tipo: 'O', Documento: 'Ocurrente' }, { Tipo: 'D', Documento: 'Domicilio' }]; //Opciones de forma de envio
    $scope.DocBaseCDFI = [{ Tipo: 'G01', Documento: 'Adquicisión de mercancias' }, { Tipo: 'G03', Documento: 'Gastos en general' }]; //Opciones de forma de envio
    $scope.DocEntryPedido = 0; //DocEntry del Documento Cotización
    //-------------------------------------------------------------

    //-------------------------------------------------------------
    //-------------------------------------------------------------
    $scope.FormaPago = ""; //Tipo de Envio aqui se guarda la variable
    $scope.DocFormaPago = [{ Tipo: '3', Documento: '03--Transferencia Electrónica de Fondos' }, { Tipo: '99', Documento: '99--Por Definir' }]; //Opciones de Tipo de Documento
    //-------------------------------------------------------------
    //DocEntry del Documento Cotización
    //-------------------------------------------------------------

    //ordenamiento
    $scope.sortType = 'name'; //tipo de ordenamiento default
    $scope.sortReverse = false;  // orden default
    $scope.NombreItem = '';

    // termindo de busqueda default
    //$scope.regexDecimalValido = true;
    $scope.ValidParmeter = '/\d+/';
    $scope.NumPedido = "";
    $scope.NumCotizacion = "";

    //monedas
    $scope.TasasDeCambio = [];//almacena las tasas de cambio
    $scope.VisualizarMoneda = false; //Indicador para mostrar combo moneda. Por defecto no se muestra
    $scope.MonedaDelSN = { Moneda: 'MXP' };//default pesos mexicanos
    $scope.Monedas = [
        { value: 'MXP', text: 'Pesos' },
        { value: 'USD', text: 'Dolares' },
        { value: 'EUR', text: 'Euros' },
        { value: 'GPB', text: 'Libra' }
    ];
    $scope.ImprimirDesde = new Date();
    $scope.ImprimirDesdeTxt = "";
    $scope.ImprimirHasta = new Date();
    $scope.ImprimirHastaTxt = "";
    $scope.DatosBP = {
        direccion: '',
        DirCompleta: '',
        ListaDireccion: [],
        ListaAlmacenes: []

    }

    //Datos forma de pago
    $scope.DatosFP = {
        Forma: '',
        Codigo: '',
        ListaFormaPago: [],
        statusMetodoPago: '0'
    }

    $scope.DatosCDFI = {
        Forma: '',
        Codigo: '',
        ListaCDFI: []
    }

    $scope.EnvioTipo = {
        Forma: '',
        Codigo: '',
        ListaFormaEnvio: []
    }

    $scope.TipoEnvio = {
        Forma: '',
        Codigo: '',
        ListaTipoEnvio: []
    }

    $scope.MetodoPago = {
        Forma: '',
        Codigo: '',
        ListaMetodoPago: []
    }

    $scope.DatosAlmacen = {
        almacen: '',
        ListaAlmacenes: []

    }

    $scope.Timbrado = 1,
        $scope.radioOptions = [
            { name: '  Si  ', value: 1, checked: 'false' },
            { name: '  No ', value: 0, checked: 'false' },
        ]

    //json para el Pedido
    $scope.JsonPedido = {
        DocDate: "",
        DocDueDate: "",
        U_NumPed: "",
        CardCode: "",
        CardName: "",
        Reference1: "",
        Comments: "",
        DocCurrency: "",
        DocRate: "",
        DocRate: "",
        SalesPersonCode: "",
        ShipToCode: "",
        Address: "",
        JournalMemo: "",
        DocumentStatus: "",
        DocType: "",
        Discount: "",
        DocumentLines: [],
        DocBase: "", //Tipo de documento: Orden de Venta | Cotización
        DocNumber: "",
        MotivoCierreCotizacion: "",
        DocDueDateCot: "" //Fecha Vencimiento Cotizacion

    };

    $scope.EstadoPedido = 'O';
    $scope.EstadoPedido = { Tipo: 'O' };//default 
    $scope.EstadosPedido = [
        { value: 'O', text: 'Abiertos' },
        { value: 'P', text: 'Cerrados' }
    ];

    $scope.timeToLive = 0;

    //extreamos el bojeto 
    var modal = document.getElementById('customerModal');

    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
    //*************************************************************************************************/
    //FUNCIONES
    $scope.InicialMaster = function () {
        $scope.tipo = 'N';
        console.log("Datos iniciales en consultaCotizacionController************");
        //growl.warning("Carga Inicial", { title: 'Atención!' });
        Servicio.DatosIniciales().then(function (respuesta) {
            if (respuesta != null) {
                $scope.rol = respuesta;

                if ($scope.rol != 'V') {
                    $scope.mostrarModalCliente();
                }
            }
            else {
                console.log('SIN Datos iniciales');
            }
        });
    }

    $scope.VisualizarTipoConsultaC = function () {
        var selected = $filter('filter')($scope.ListaTipoConsultaC, { value: $scope.TipoConsultaC.Tipo });
        return ($scope.TipoConsultaC.Tipo && selected.length) ? selected[0].text : 'Sin Tipo de Consulta';
    }

    $scope.ConsultaCotizaciones = function () {
        $scope.ListaCotizaciones = [];
        var opcion = 1;

        if ($scope.ValorConsultaC == "" || $scope.ValorConsultaC == undefined) {
            $scope.ValorConsultaC = "Todos";
        }

        if ($scope.TipoConsultaC.Tipo == 'Fecha') {
            $scope.ParametrosConsulta = {
                DocNum: -1,
                UserId: '',
                Tipo: $scope.TipoConsultaC.Tipo,
                Fecha: ($scope.ValorConsultaC).toISOString().substring(0, 10),
                Articulo: ''
            }
        }
        else {
            $scope.ParametrosConsulta = {
                DocNum: $scope.TipoConsultaC.Tipo != 'DocNum' ? -1 : $scope.ValorConsultaC,
                UserId: '',
                Tipo: $scope.TipoConsultaC.Tipo,
                Fecha: $scope.ValorConsultaC,
                Articulo: $scope.ValorConsultaC
            }
        }

        if ($scope.rol != 'V') {
            opcion = 2;
            $scope.ParametrosConsulta.UserId = $scope.customerBP.CardCode;
        }

        console.log('DocNum: ' + $scope.ParametrosConsulta.DocNum + ' --- UserID: ' + $scope.ParametrosConsulta.UserId + ' ---- Tipo Consulta: ' + $scope.ParametrosConsulta.Tipo + ' ---- Fecha: ' + $scope.ParametrosConsulta.Fecha + ' ----- Articulo: ' + $scope.ParametrosConsulta.Articulo);
        Servicio.ConsultaCotizacion($scope.ParametrosConsulta.DocNum, $scope.ParametrosConsulta.UserId, $scope.ParametrosConsulta.Tipo, $scope.ParametrosConsulta.Fecha, $scope.ParametrosConsulta.Articulo, opcion).then(function (respuesta) {
            $scope.ValorConsultaC = "";
            console.log(respuesta);

            if (respuesta != undefined && respuesta[0].NoCotizacion == -33) {
                growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
                return;
            }

            if (respuesta != undefined && respuesta.length > 0) {
                $scope.ValorConsultaC = "";

                angular.forEach(respuesta, function (resp) {
                    $scope.ListaCotizaciones.push({
                        NoCotizacion: resp.NoCotizacion,
                        DocStatus: resp.DocStatus == 'C' ? 'Cerrada' : "Abierta",
                        DocDate: resp.DocDate,
                        Cliente: resp.Cliente,
                        ItemCode: resp.ItemCode,
                        ItemName: resp.ItemName,
                        QtyCotizada: resp.QtyCotizada,
                        UMVendida: resp.UMVendida,
                        Precio: resp.Precio,
                        Total: resp.Total,
                    });
                });

                $scope.ValorConsultaC = "";
            }
            else {
                $scope.ValorConsultaC = "";
                $scope.MensajesDeUsuario('info', "¡No se recupero ninguna información con los parametros especificados!");
            }
        });
    }

    $scope.mostrarModalCliente = function () {
        Servicio.ObtenerClineteDeNegocios().then(function (respuestaSN) {
            if (respuestaSN != null) {
                if (respuestaSN.CardCode == "-33") {
                    growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
                    return;
                }

                if (respuestaSN != undefined && respuestaSN.CardCode != undefined) {
                    $scope.customerBP = {
                        CardCode: respuestaSN.CardCode,
                        CardName: respuestaSN.CardName,
                        ListaPrecios: respuestaSN.ListNum,
                        Moneda: respuestaSN.Moneda,
                        Descuento: parseFloat(respuestaSN.Discount),
                        DiscInDoc: respuestaSN.DiscInRet,
                        Saldo: respuestaSN.Saldo,
                        SaldoSt: respuestaSN.Saldo.toLocaleString('en') + ' MXN',
                        Credito: respuestaSN.Credito,
                        CreditoSt: respuestaSN.Credito.toLocaleString('en') + ' MXN'
                    }

                    if ($scope.customerBP.Moneda == "##") {
                        $scope.VisualizarMoneda = true;
                    }
                    else {
                        $scope.VisualizarMoneda = false;
                    }

                    /*$scope.CargarDirecciones();
                    $scope.CargarInputCDFI();
                    $scope.CargarInputFP();
                    $scope.CargarFormaEnvio();
                    $scope.CargarMetodoPago();
                    $scope.CargarTipoEnvio();*/
                }
            }
        });
    }

    $scope.ObtenerTsaDeCambio = function () {
        $scope.TasasDeCambio = [];
        var fecha = new Date();
        var mes = fecha.getMonth() + 1;
        var dia = fecha.getDate();
        var fechaCompleta = fecha.getFullYear() + '-' + (mes < 10 ? '0' : '') + mes + '-' + (dia < 10 ? '0' : '') + dia;

        Servicio.ObtenerTasasDeCambioSAP(fechaCompleta).then(function (respuestaTasas) {
            if (respuestaTasas != null) {
                for (var i = 0; i < respuestaTasas.length; i++) {
                    $scope.TasasDeCambio.push({
                        RateDate: respuestaTasas[i].RateDate,
                        Currency: respuestaTasas[i].Currency,
                        Rate: respuestaTasas[i].Rate
                    });
                }

                console.log("Tasa de cambio 1");
                console.log($scope.TasasDeCambio);
            }
            if ($scope.TasasDeCambio.length <= 0) {
                growl.warning("Las Tasas de Cambio No Estan Actualizadas", { title: '¡Atención!' });
            }
        });
    }

    $scope.ObtenerAlmacen = function () {
        Servicio.ObtenerAlmacenDefault().then(function (respuesta) {
            if (respuesta != null) {
                $scope.WareHoseCode = respuesta;
            } else {
                /// revisar
                $scope.WareHoseCode = "PT"
            }
        });
    }

    $scope.BorrarDetallePedido = function () {
        $scope.ListaItemDetalle = [];
        $scope.JsonPedido.DocumentLines = [];
        $scope.ActualizarTotalAPagar();
    };


    //ACTUALIZA EL TOTAL DEL DOCUMENTO
    $scope.ActualizarTotalAPagar = function () {
        $scope.TotalEnImpuesto = 0;
        $scope.TotalAPagar = 0;
        $scope.SubttlAPagar = 0;

        for (var i = 0; i < $scope.ListaItemDetalle.length; i++) {
            if ($scope.ListaItemDetalle[i].Delete != true) {
                $scope.SubttlAPagar = parseFloat($scope.SubttlAPagar) + parseFloat($scope.ListaItemDetalle[i].Total);
                $scope.TotalEnImpuesto = parseFloat($scope.TotalEnImpuesto) + parseFloat($scope.ListaItemDetalle[i].IVA);
            }
        }

        $scope.TotalEnImpuestoStr = formatThousandsWithRounding($scope.TotalEnImpuesto, 2);
        $scope.TotalAPagar = $scope.SubttlAPagar;// - PrtgeDiscount;

        var cantidadConFormato = formatThousandsWithRounding($scope.SubttlAPagar, 2);
        $scope.SbttlAPagarDisplay = cantidadConFormato;

        var totalConFormato = formatThousandsWithRounding($scope.TotalAPagar, 2);
        $scope.TotalAPagarDisplay = totalConFormato.toLocaleString('en');

        $scope.ActualizarTotalUnidades();
    }

    $scope.ActualizarTotalUnidades = function () {
        $scope.TotalUnidades = 0;
        for (var i = 0; i < $scope.ListaItemDetalle.length; i++) {
            if ($scope.ListaItemDetalle[i].Delete == false) {
                $scope.TotalUnidades = parseInt($scope.TotalUnidades) + parseInt($scope.ListaItemDetalle[i].Cantidad);
            }
        }
    }

    ///:::::REGION CARGAR COTIZACION::::::::::::::///
    $scope.cargarCotizacion = async function (numCotizacion, fecha) {
        await $scope.LimpiarFormularioPedido();

        $scope.tipo = 'E'; //Editar cotización
        $scope.accion = '2';
        let op = 1;
        let usuario = "";
        console.log("Tipo cotizacion: " + $scope.tipo);

        //Rol
        console.log("El rol es: " + $scope.rol);
        if ($scope.rol != 'V') {
            console.log("entre al if de cotizacion");
            op = 2
            usuario = $scope.customerBP.CardCode;

            console.log("Cliente: " + $scope.customerBP.CardCode + " | Usuario: " + usuario);
        }

        console.log("Tasa de cambio 2");
        console.log($scope.TasasDeCambio);

        $scope.ConsultaInfoC(numCotizacion, fecha, usuario, op);
    }

    $scope.ConsultaInfoC = function (numCotizacion, fecha, usuario, op) {
        Servicio.ConsultaCotizacion(numCotizacion, usuario, "DocNum", fecha, "Todos", op).then(function (respuesta) {
            console.log("Respuesta de consulta cotizacion: ");
            console.log(respuesta);
            let opcion = '1';

            if (respuesta != undefined && respuesta[0].NoCotizacion == -33) {
                growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
                return;
            }

            if (respuesta != undefined && respuesta.length > 0) {
                $scope.ValorConsulta = "";
                if ($scope.rol != 'V') {
                    opcion = '2'
                }
                console.log("Oppcion: " + opcion);

                $scope.DocEntryPedido = numCotizacion;
                $scope.InfoCotizacion(opcion, respuesta);
            }
            else {
                $scope.ValorConsulta = "";
                $scope.MensajesDeUsuario('info', "¡No se recupero ninguna información con los parametros especificados!");
            }
        });
    }

    $scope.InfoCotizacion = function (opcion, respuesta) {
        console.log("Obtener información completa de Cotización");
        console.log(respuesta);
        Servicio.ObtenerListaSocioDeNegocios(opcion).then(function (respuestaSN) {
            if (respuestaSN != null) {
                console.log(respuestaSN);
                console.log(respuestaSN[0].CardCode);

                if (respuestaSN[0].CardCode == "-33") {
                    growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
                    return;
                }

                console.log(respuesta[0]);
                $scope.DocStatus = respuesta[0].DocStatus == 'C' ? 'Cerrada' : "Abierta";
                $scope.RealCardCode = respuesta[0].Cliente.slice(0, respuesta[0].Cliente.indexOf(' ')); //CardCode del cliente
                $scope.DirCotizacionCargada = respuesta[0].Direccion;

                $scope.NumPedido = respuesta[0].Fol;
                $scope.NumCotizacion = respuesta[0].NoCotizacion;
                $scope.ComentariosPedido = respuesta[0].Comentarios;
                $scope.FechaSistema = new Date(respuesta[0].ShipDate);
                $scope.FechaSistemaCot = new Date(respuesta[0].FechaEntrega);
                $scope.EnvioTipo.Forma = respuesta[0].Envio;
                $scope.DatosBP.direccion = respuesta[0].Direccion; //Limpiar Combo
                $scope.TipoEnvio.Forma = respuesta[0].Paqueteria;

                $scope.MetodoPago.Forma = "PPD";
                $scope.DatosFP.Forma = '99';
                $scope.DatosCDFI.Forma = "G01";
                //$scope.MetodoPago.Forma = respuesta[0].MetodoPago;
                //$scope.DatosFP.Forma = respuesta[0].FormaPago;
                //$scope.DatosCDFI.Forma = respuesta[0].CDFI;

                angular.forEach(respuestaSN, function (resp) {
                    if (resp.CardCode == $scope.RealCardCode) {
                        console.log("Impuesto Cliente:" + parseFloat(resp.Discount));
                        $scope.customerBP = {
                            CardCode: resp.CardCode,
                            CardName: resp.CardName,
                            ListaPrecios: resp.ListNum,
                            Moneda: resp.Moneda,
                            Descuento: parseFloat(resp.Discount),
                            DiscInDoc: resp.DiscInRet,
                            Saldo: resp.Saldo,
                            SaldoSt: (resp.Saldo.toLocaleString('en')) + ' MXN',
                            Credit: resp.Credito,
                            CreditSt: resp.Credito.toLocaleString('en') + ' MXN'
                        }
                        console.log($scope.customerBP.Descuento);
                        if ($scope.customerBP.Moneda == "##") {
                            $scope.VisualizarMoneda = true;
                        }
                        else {
                            $scope.VisualizarMoneda = false;
                        }
                    }
                });

                //le damos el tipo de envio Pendiente                      
                //pendiente carga este dato
                console.log(respuesta[0].Envio);
                //Lista de Articulos
                for (i = 0; i < respuesta.length; i++) {
                    console.log(respuesta[i]);
                    $scope.ivaUnitario = 0;
                    $scope.precioMasIva = 0;
                    $scope.ValorDelImpuesto = 0;

                    //**************************************************
                    //Verificar Impuesto
                    //**************************************************
                    if (respuesta[i].VATLiable == "Y") {//traer el impuesto si el articulo aplica
                        $scope.ValorDelImpuesto = respuesta[i].ValorImpuesto;
                    }
                    else {//cuando el articulo no aplica a impuesto
                        Servicio.ObtenerImpuestoDefault().then(function (respuestaTaxDefault) {
                            if (respuestaTaxDefault != null) {
                                $scope.ValorDelImpuesto = respuestaTaxDefault;
                            }
                        });
                    }

                    //**************************************************
                    //Calcular Impuesto
                    //**************************************************
                    var RateChange = 1;
                    if ($scope.customerBP.Moneda == "##") {
                        for (var j = 0; j < $scope.TasasDeCambio.length; j++) {
                            if ($scope.TasasDeCambio[j].Currency == $scope.MonedaDelSN.Moneda) {
                                RateChange = $scope.TasasDeCambio[j].Rate;
                            }
                        }
                        respuesta[i].Precio = respuesta[i].Precio / RateChange;
                    }

                    $scope.Impuesto = $scope.ValorDelImpuesto; //Obtiene el impuesto

                    //calcular impuesto por articulo
                    $scope.ivaUnitario = (parseFloat($scope.Impuesto) * parseFloat(respuesta[i].Precio)) / 100;
                    $scope.precioMasIva = $scope.ivaUnitario + parseFloat(respuesta[i].Precio);

                    console.log(parseFloat($scope.customerBP.Descuento));

                    var PrecioArticulo = respuesta[i].Precio; //precio auxiliar
                    var ValorDeDescuento = (parseFloat($scope.customerBP.Descuento) * parseFloat(PrecioArticulo)) / 100; //precio calculado con descuento del cliente
                    var ValorDeDescuento2 = (parseFloat(respuesta[i].Descuento2) * parseFloat(PrecioArticulo)) / 100; //precio calculado con descuento del articulo
                    var PrecioArticuloConDescuento = parseFloat(PrecioArticulo) - ValorDeDescuento - ValorDeDescuento2;
                    var Impuesto = $scope.Impuesto;
                    var NuevoImporteImpuesto = ((parseInt(respuesta[i].QtyCotizada) * PrecioArticuloConDescuento) * $scope.Impuesto) / 100;
                    NuevoImporteImpuesto = parseFloat(NuevoImporteImpuesto) * 1;

                    var TotalImpuestoDeLinea = NuevoImporteImpuesto;
                    var TotalDeLineaSinIVA = parseInt(respuesta[i].QtyCotizada) * PrecioArticuloConDescuento;
                    var totalDescuento = 0;
                    var desc1 = 100 - parseFloat($scope.customerBP.Descuento);
                    var desc2 = 100 - parseFloat(respuesta[i].Descuento2);
                    var promDesc = (desc1 * desc2) / 100;
                    totalDescuento = 100 - promDesc;

                    var importeDescuento = (parseFloat(respuesta[i].Precio) * parseFloat(totalDescuento)) / 100;
                    var priceItemWithoutDiscount = parseFloat(respuesta[i].Precio) - importeDescuento;

                    //**************************************************
                    //Agergar Datos de la Linea a Lista de Articulos de Cotizacion
                    //**************************************************                       
                    Object.assign(respuesta[i], {
                        "Codigo": respuesta[i].ItemCode,
                        "Nombre": respuesta[i].ItemName,
                        "Cantidad": respuesta[i].QtyCotizada,
                        "Precio": respuesta[i].Precio, //parseFloat(PrecioArticuloConDescuento),
                        "PrecioLinea": 0.0,
                        "SujetoImpuesto": respuesta[i].VATLiable,
                        "PrecioUnitario": PrecioArticulo, //respuesta[i].Precio,
                        "Descuento": parseFloat(respuesta[i].Descuento2),
                        "PrtgeDisc": parseFloat(respuesta[i].Descuento2),
                        "PrecioConDescuento": priceItemWithoutDiscount, //parseFloat(PrecioArticuloConDescuento),
                        "ValorImpuesto": respuesta[i].VATLiable == 'Y' ? $scope.ValorDelImpuesto : 0,
                        "CodigoVAT": respuesta[i].TaxCodeAR,
                        "IVA": respuesta[i].VATLiable == 'Y' ? NuevoImporteImpuesto.toFixed(2) : 0,
                        "Total": respuesta[i].VATLiable == 'Y' ? (TotalDeLineaSinIVA + TotalImpuestoDeLinea).toFixed(2) : TotalDeLineaSinIVA.toFixed(2),
                        "FechaEntrega": $scope.FechaSistema,
                        "MetrosExtra": "METROS_EXTRA",
                        "NumberLine": respuesta[i].LineNum,
                        "UM": respuesta[i].UnitSize,
                        "LineNum": respuesta[i].LineNum,
                        "Delete": false,
                        "Almacen": respuesta[i].Almacen,
                        "whsCode": respuesta[i].WhsCode,
                        "stock": respuesta[i].stock,
                        "comprometido": respuesta[i].comprometido,
                        "stockReal": 0,
                    });

                    $scope.ListaItemDetalle.push(respuesta[i]);

                    //**************************************************
                    //**************************************************
                    //obtenemos stock hacer funcion. temporalmente inavilitado                            
                    $scope.SubttlAPagar = $scope.SubttlAPagar + parseFloat(respuesta[i].Total);
                    $scope.TotalEnImpuesto = $scope.TotalEnImpuesto + parseFloat(respuesta[i].IVA);
                    $scope.TotalEnImpuestoStr = formatThousandsWithRounding($scope.TotalEnImpuesto, 2);

                    //calculo de total de unidades                    
                    $scope.TotalUnidades = parseInt($scope.TotalUnidades) + parseInt(respuesta[i].Cantidad);
                }

                //**********************************************************************************
                $scope.TotalAPagar = parseFloat($scope.SubttlAPagar);
                var cantidadFormateadaSbttl = formatThousandsWithRounding($scope.SubttlAPagar, 2);
                $scope.SbttlAPagarDisplay = cantidadFormateadaSbttl;

                var cantidadFormateadaTtl = formatThousandsWithRounding($scope.TotalAPagar, 2);
                $scope.TotalAPagarDisplay = cantidadFormateadaTtl.toLocaleString('en');
                //**********************************************************************************

                //Funciones                       
                $scope.ActualizarTotalAPagar();
                $scope.ActualizarTotalUnidades();
                $scope.ValorConsulta = "";
            }
        });
    }

    $scope.LimpiarFormularioPedido = function () {
        //-------------------------------------------------------------
        $scope.Base = 'Cotizacion'; //Tipo de Documento

        $scope.DocBase = [{ Tipo: 'Pedido', Documento: 'Orden de Venta' }, { Tipo: 'Cotizacion', Documento: 'Cotizacion' }]; //Opciones de Tipo de Documento
        $scope.MotivoCierre = [
            { Tipo: 'Precio', Motivo: 'Precio' },
            { Tipo: 'Inventario', Motivo: 'Inventario' },
            { Tipo: 'Tiempo de Entrega', Motivo: 'Tiempo de Entrega' },
            { Tipo: 'Cancelacion', Motivo: 'Cancelacion' }
        ];
        //-------------------------------------------------------------

        if ($scope.rol == 'C') {
            $scope.DocStatus = "";
            $scope.MotivoCierreCotizacion = "";
            //vaciado de modelos
            $scope.DatosBP.DirCompleta = "";
            $scope.DatosBP.ListaDireccion = [];
            $scope.FechaSistema = "";
            $scope.NumPedido = "";
            $scope.DocEntryPedido = 0;
            $scope.ComentariosPedido = "";
            $scope.JsonPedido.MotivoCierreCotizacion = "";
            $scope.JsonPedido.DocNumber = "";
            $scope.JsonPedido.DocDate = "";
            $scope.JsonPedido.DocDueDate = "";
            $scope.JsonPedido.DocDueDateCot = "";
            $scope.JsonPedido.U_NumPed = "";
            $scope.JsonPedido.Reference1 = "";
            $scope.JsonPedido.Comments = "";
            $scope.JsonPedido.DocCurrency = "";
            $scope.JsonPedido.DocRate = "";
            $scope.JsonPedido.SalesPersonCode = "";
            $scope.JsonPedido.ShipToCode = "";
            $scope.JsonPedido.Address = "";
            $scope.JsonPedido.JournalMemo = "";
            $scope.JsonPedido.DocumentStatus = "";
            $scope.JsonPedido.DocType = "";
            $scope.JsonPedido.Discount = "";
            $scope.JsonPedido.DocumentLines = [];
            $scope.ListaItemDetalle = [];

            $scope.EnvioTipo = { Forma: '', Codigo: '', ListaTipoEnvio: [] };
            $scope.TipoEnvio = { Forma: '', Codigo: '', ListaTipoEnvio: [] };
            $scope.MetodoPago = { Forma: '', Codigo: '', ListaTipoEnvio: [] };
        }
        else {

            $scope.DocStatus = "";
            $scope.MotivoCierreCotizacion = "";
            $scope.DatosBP.DirCompleta = "";
            //vaciado de modelos
            $scope.customerBP = {};
            $scope.NumPedido = "";
            $scope.NumCotizacion = "";
            $scope.DocEntryPedido = 0;
            $scope.ComentariosPedido = "";
            $scope.JsonPedido.MotivoCierreCotizacion = "";
            $scope.JsonPedido.DocNumber = "";
            $scope.JsonPedido.DocDate = "";
            $scope.JsonPedido.DocDueDate = "";
            $scope.JsonPedido.DocDueDateCot = "";
            $scope.JsonPedido.U_NumPed = "";
            $scope.JsonPedido.CardCode = "";
            $scope.JsonPedido.CardName = "";
            $scope.JsonPedido.Reference1 = "";
            $scope.JsonPedido.Comments = "";
            $scope.JsonPedido.DocCurrency = "";
            $scope.JsonPedido.DocRate = "";
            $scope.JsonPedido.SalesPersonCode = "";
            $scope.JsonPedido.ShipToCode = "";
            $scope.JsonPedido.Address = "";
            $scope.JsonPedido.JournalMemo = "";
            $scope.JsonPedido.DocumentStatus = "";
            $scope.JsonPedido.DocType = "";
            $scope.JsonPedido.Discount = "";
            $scope.JsonPedido.DocumentLines = [];
            $scope.ListaItemDetalle = [];
            $scope.DatosFP.Forma = "99"; //Limpiar FormaPago
            $scope.DatosFP.ListaFormaPago = ""; //Limpiar FormaPago
            $scope.DatosFP.statusMetodoPago = 1;
            $scope.DatosCDFI.Forma = "";
            $scope.DatosCDFI.ListaFormaPago = ""; //Limpiar FormaPago
            $scope.DatosCDFI.statusMetodoPago = "";

            $scope.EnvioTipo = { Forma: '', Codigo: '', ListaTipoEnvio: [] };
            $scope.TipoEnvio = { Forma: '', Codigo: '', ListaTipoEnvio: [] };

            $scope.MetodoPago = { Forma: '', Codigo: '', ListaTipoEnvio: [] };
            $scope.DatosBP.ListaDireccion = [];
            $scope.FechaSistema = "";
        }

        $scope.ActualizarTotalAPagar();
        $scope.InicialMaster();

        $scope.CargarDirecciones();
        $scope.CargarInputCDFI();
        $scope.CargarInputFP();
        $scope.CargarFormaEnvio();
        $scope.CargarMetodoPago();
        $scope.CargarTipoEnvio();
    };

    $scope.ValidarTipo = function () {
        console.log("Tipo documento: " + $scope.Base);
        if ($scope.Base === "Pedido") {
            // Redirigir a la vista PaginaInicio
            $window.location.href = '/MasterPage/PaginaInicio';
        }
    }

    //*************************************************************************************************/
    //::::::ZONA MODAL DE ESTADO Cotizacion::::::::://
    $scope.AbrirModalEstadoCotizacion = function () {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + "/Modales/EstadoCotizacion.html",
            controller: EstadoCotizacionModalController,//Recibe ITEMList, es el nombre de su parametro.
            keyboard: false,
            backdrop: "static",
            resolve: {
                URLParameters: function () {
                    return { Document: $scope.DocEntryPedido, User: "Carlos" };//Este es el parametro que recibo en la funcion AbrirModalArticulos
                }
            }
        }).result.then(function (result) { });
    }

    var EstadoCotizacionModalController = function (Ruta, $scope, $uibModalInstance, $log, URLParameters) {
        $scope.SrcParameters = URLParameters;
        $scope.UrlString = Ruta.path + '/IntegracionSap/ImprimirEstadoCotizacion?DocNum=' + $scope.SrcParameters.Document;

        $scope.ok = function () {
            $uibModalInstance.close($scope.ItemSeleccion);//cuando se cierre regresa estos datos
        }
    }
    //::::::FIN DE MODAL ESTADO PEDIDO:::::::::://

    $scope.cerrarCotizacion = function () {
        if (($scope.MotivoCierreCotizacion == "")) {
            growl.error('Se requiere un motivo para cerrar la cotización.', { title: '¡Error!' });
            return;
        }

        var dialog = new Messi(
            "<script>var incremento = 0;var mySet = setInterval(setColor, 1000);setColor();function setColor() {" +
            "incremento++;" +
            "document.getElementById('counter').style.color='red';" +
            "document.getElementById('counter').innerHTML += '.' ;" +

            "}</script><div id='waitingMsg'><br/><br/><strong>¡Orden Enviada!</strong><br/><br/>Esperando Respuesta del Servidor <p id='counter'></p></div>",
            {
                width: 400,
                height: 150,
                closeButton: false
            }
        );

        //var waitForResponse = setInterval(function () { growl.info("Esperando respuesta del Servidor... ", { title: 'Orden de Venta Enviada!', ttl: $scope.timeToLive }); }, 1000);
        $("#postOrderSAP").prop('disabled', true);
        $("#openBtnPreview").prop('disabled', true);
        Servicio.EnviarCierreCotizacion($scope.MotivoCierreCotizacion, $scope.DocEntryPedido).then(function (respuestaDeIntegracion) {
            clearInterval(mySet);
            dialog.hide();

            if (respuestaDeIntegracion != undefined) {
                $("#postOrderSAP").prop('disabled', false);
                $("#openBtnPreview").prop('disabled', false);

                if (respuestaDeIntegracion.codigo == 0) {
                    growl.success("Número de Documento en SAP: " + respuestaDeIntegracion.mensaje, { title: '¡Oferta de Venta Cerrada!' });
                    //$scope.ReporteImpresionCotizacion(parseInt(respuestaDeIntegracion.mensaje), 'DocNum');
                    $scope.Limpiar();
                }
                else {

                    //clearInterval(waitForResponse);
                    growl.error(respuestaDeIntegracion.mensaje, { title: '¡Error!' });
                    $scope.JsonPedido.DocumentLines = [];
                }
            }
            else {
                $("#postOrderSAP").prop('disabled', false);
                $("#openBtnPreview").prop('disabled', false);
                //clearInterval(waitForResponse);
                growl.error("No se obtuvo respuesta de Integración", { title: '¡Error!' });
                $scope.JsonPedido.DocumentLines = [];
            }
        });
    }

    $scope.convertirAPedido = function () {
        if (($scope.ListaItemDetalle).length <= 0) {
            growl.error("No ha cargado los Artículos del Pedido", { title: 'Sin Datos en el Detalle' });
            return;
        }

        $scope.Base = "Pedido";
        $scope.FechaActual = new Date();
        var fechaActual = new Date();
        var yearActual = fechaActual.getFullYear();
        var mesActual = fechaActual.getMonth() + 1;
        var diaActual = fechaActual.getDate();
        var yearPedido;
        var mesPedido;
        var diaPedido;

        if (($scope.customerBP.CardCode == "" || $scope.customerBP.CardCode == undefined || $scope.EnvioTipo.Forma == "")) {
            growl.error('No ha completado el formulario.', { title: '¡Error!' });
            return;
        }

        //if ($scope.NumPedido == "") {
        //    growl.error('No se ha indicado el Número del Pedido.', { title: 'Error!' });
        //    return;
        //}
        if ($scope.DatosFP.Forma == '' || $scope.DatosFP.Forma == undefined || $scope.DatosFP.Forma <= 0) {
            growl.error("Falta método de pago en SN", { title: 'Sin Forma de pago' });
            return;
        }

        if ($scope.EnvioTipo.Forma == '' || $scope.EnvioTipo.Forma == undefined || $scope.EnvioTipo.Forma <= 0) {
            growl.error("Falta Forma de Envío  ", { title: 'Sin Forma de Envío' });
            return;
        }

        if ($scope.TipoEnvio.Forma == '' || $scope.TipoEnvio.Forma == undefined || $scope.TipoEnvio.Forma <= 0) {
            growl.error("Falta Tipo de Envío  ", { title: 'Sin Tipo de Envío' });
            return;
        }

        if ($scope.MetodoPago.Forma == '' || $scope.MetodoPago.Forma == undefined || $scope.MetodoPago.Forma <= 0) {
            growl.error("Falta Método de Pago  ", { title: 'Sin Método de Pago' });
            return;
        }

        if ($scope.FechaSistema == "" || $scope.FechaSistema == null || $scope.FechaSistema == undefined) {
            growl.error('No se ha establecido la Fecha del Pedido.', { title: '¡Error!' });
        }
        else if ($scope.FechaAtrasada() == false) {
            growl.error('Fecha del Pedido no Válida', { title: '¡Error!' });
        }
        else if ($scope.FechaAtrasada() != false) {
            //growl.success("OK", { title: 'Orden de Venta Generada!' });
            $scope.ExtraerJsonDetalle();

            if ($scope.JsonPedido.DocumentLines.length <= 0) {
                growl.error('No ha completado el formulario.', { title: '¡Error!' });
                return;
            }

            var dialog = new Messi(
                "<script>var incremento = 0;var mySet = setInterval(setColor, 1000);setColor();function setColor() {" +
                "incremento++;" +
                "document.getElementById('counter').style.color='red';" +
                "document.getElementById('counter').innerHTML += '.' ;" +

                "}</script><div id='waitingMsg'><br/><br/><strong>¡Orden Enviada!</strong><br/><br/>Esperando Respuesta del Servidor <p id='counter'></p></div>",
                {
                    width: 400,
                    height: 150,
                    closeButton: false
                }
            );

            //var waitForResponse = setInterval(function () { growl.info("Esperando respuesta del Servidor... ", { title: 'Orden de Venta Enviada!', ttl: $scope.timeToLive }); }, 1000);
            $("#postOrderSAP").prop('disabled', true);
            $("#openBtnPreview").prop('disabled', true);
            $scope.JsonPedido.Address = $scope.DatosBP.direccion;
            console.log(">> " + JSON.stringify($scope.JsonPedido));

            Servicio.ConvertirOrdenAPedido(JSON.stringify($scope.JsonPedido)).then(function (respuestaDeIntegracion) {
                clearInterval(mySet);
                dialog.hide();
                console.log(respuestaDeIntegracion);

                if (respuestaDeIntegracion != undefined) {
                    $("#postOrderSAP").prop('disabled', false);
                    $("#openBtnPreview").prop('disabled', false);

                    if (respuestaDeIntegracion.codigo == 0) {
                        //clearInterval(waitForResponse);
                        growl.success("Número de Documento en SAP: " + respuestaDeIntegracion.mensaje, { title: '¡Oferta de Venta Generada!' });
                        $scope.ReporteImpresionPedido(parseInt(respuestaDeIntegracion.docEntry), 'DocNum');
                        //$scope.JsonPedido.DocumentLines = [];
                        $scope.Limpiar();
                    }
                    else {
                        //clearInterval(waitForResponse);
                        growl.error(respuestaDeIntegracion.mensaje, { title: '¡Error!' });
                        $scope.JsonPedido.DocumentLines = [];
                    }
                }
                else {
                    $("#postOrderSAP").prop('disabled', false);
                    $("#openBtnPreview").prop('disabled', false);
                    //clearInterval(waitForResponse);
                    growl.error("No se obtuvo respuesta de Integración", { title: '¡Error!' });
                    $scope.JsonPedido.DocumentLines = [];
                }
            });
        }
    }

    //*************************************************************************************************/
    ///:::::REGION MODAL PREVISUALIZADOR::::::::::::::///
    $scope.PrevisualizarPedido = function () {
        if (($scope.ListaItemDetalle).length <= 0) {
            growl.error("No ha cargado los Artículos del Pedido", { title: 'Sin datos en el detalle' });
            return;
        }

        if ($scope.DatosBP.DirCompleta == '' || $scope.DatosBP.DirCompleta == undefined) {
            growl.error("No ha cargado la dirección de entrega ", { title: 'Sin datos de entrega' });
            return;
        }

        if ($scope.DatosBP.direccion == '' || $scope.DatosBP.direccion == undefined || $scope.DatosBP.direccion.length == 0) {
            growl.error("No ha cargado la dirección de entrega ", { title: 'Sin datos de entrega' });
            return;
        }

        for (l = 0; l < $scope.ListaItemDetalle.length; l++) {
            var stockReal = 0;
            stockReal = parseFloat($scope.ListaItemDetalle[l].Cantidad) + parseFloat($scope.ListaItemDetalle[l].comprometido);
            stockReal = $scope.ListaItemDetalle[l].stock - stockReal
            $scope.ListaItemDetalle[l].stockReal = stockReal;

            if ($scope.ListaItemDetalle[l].stockReal < 0) {
                growl.warning("El articulo:" + $scope.ListaItemDetalle[l].Nombre + " No cuenta con stock suficiente.", { title: 'Sin Existencias' });
                return;
            }
        }
        //condicional para saber si no hay stock

        //if ($scope.NumPedido <= 0 || $scope.NumPedido == "") {
        //    growl.error("No ha ingresado el Folio del Pedido", { title: 'Sin Folio de Pedido' });
        //    return;
        //}

        //#################################################################################################
        //#################################################################################################
        //validaciones 
        if ($scope.Base == "Pedido") {
            if ($scope.FechaSistema == "" || $scope.FechaSistema == undefined) {
                growl.error("No ha ingresado la fecha de entrega", { title: 'Sin fecha de entrega' });
                return;
            }

            if ($scope.FechaSistema == "" || $scope.FechaSistema == null || $scope.FechaSistema == undefined) {
                growl.error('No se ha establecido la fecha del pedido.', { title: '¡Error!' });
                return;
            }
        }
        else {
            if ($scope.FechaSistemaCot == "" || $scope.FechaSistemaCot == undefined) {
                growl.error("No ha ingresado la fecha de entrega", { title: 'Sin fecha de entrega' });
                return;
            }

            if ($scope.FechaSistemaCot == "" || $scope.FechaSistemaCot == null || $scope.FechaSistemaCot == undefined) {
                growl.error('No se ha establecido la fecha del pedido.', { title: '¡Error!' });
                return;
            }
        }

        //#################################################################################################
        //#################################################################################################
        //llanando llenado de json
        g = Date.now().toString(36) + Math.random().toString(36).substr(2);
        console.log("Nuevo Id" + g);
        $scope.ExtraerJsonDetalle();

        //creando documento pdf
        Servicio.VistaPrevia(JSON.stringify($scope.JsonPedido), g).then(function (respuestaDeIntegracion) {
            if (respuestaDeIntegracion != undefined) {
                //$('<form>', {
                //    "method": "POST",
                //    "action": Ruta.path + '/Reportes/ImprimirVistaP?id=' + g,
                //    "target": "_blank"
                //}).appendTo(document.body).submit();           

                var modalInstance = $uibModal.open({
                    templateUrl: Ruta.path + "/Modales/EstadoPedido.html",
                    controller: EstadoPedidoModalControllerr,//Recibe ITEMList, es el nombre de su parametro.
                    keyboard: false,
                    backdrop: "static",
                    resolve: {
                        URLParameters: function () {
                            return { id: g };//Este es el parametro que recibo en la funcion AbrirModalArticulos
                        }
                    }
                });
                console.log("se guardo registro vista previa");
            }
            else {
                growl.error("Error al crear la vista previa", { title: '¡Error!' });
            }
        });

        $scope.JsonPedido.DocumentLines = [];
        // modal.style.display = "block";
        //$("#pdf_preview").attr("src", doc.output('datauristring'));//añadimos al iframe
    }

    var EstadoPedidoModalControllerr = function (Ruta, $scope, $uibModalInstance, $log, URLParameters) {
        $scope.SrcParameters = URLParameters;
        $scope.UrlString = Ruta.path + '/Reportes/ImprimirVistaP?id=' + g;

        $scope.ok = function () {
            $uibModalInstance.close();
        }
    }
    ///::::FIN REGION MODAL PREVISUALIZAR:::::::::::::///

    ///:::::REGION MODAL SOCIO DE NEGOCIOS::::::::::////
    $scope.mostrarModalSN = function () {
        let opcion = ''
        if ($scope.rol == 'V') {
            opcion = '1'
        }
        if ($scope.rol == 'C') {
            opcion = '2'
        }

        /*var respuestaSN = $scope.respuestaSN
        console.log(respuestaSN);
        AbrirModalSN(respuestaSN);*/
        $scope.respuestaSN = [];
        ; Servicio.ObtenerListaSocioDeNegocios(opcion).then(function (respuestaSN) {
            if (respuestaSN != null) {
                if (respuestaSN[0].CardCode == "-33") {
                    growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
                    return;
                }
                AbrirModalSN(respuestaSN);
            }
        });

        function AbrirModalSN(ListSN) {
            var modalInstance = $uibModal.open({
                templateUrl: Ruta.path + '/Modales/ListaSN.html',
                controller: ListaPedidoController,
                //windowClass: 'app-modal-window-md',
                keyboard: false,
                backdrop: "static",
                resolve: {
                    SNList: function () {
                        return ListSN;
                    }
                }
            }).result.then(function (result) {//aqui es cachado el valor que seleccione en la modal
                //cachar el valor arrojado aqui
                console.log(result);
                if (result != undefined && result.CardCode != undefined) {

                    $scope.customerBP = {
                        CardCode: result.CardCode,
                        CardName: result.CardName,
                        ListaPrecios: result.ListNum,
                        Moneda: result.Moneda,
                        Descuento: parseFloat(result.Discount),
                        DiscInDoc: result.DiscInRet,
                        Saldo: result.Saldo,
                        SaldoSt: result.Saldo.toLocaleString('en') + ' MXN',
                        Credit: result.Credito,
                        CreditSt: result.Credito.toLocaleString('en') + ' MXN'
                    }

                    if ($scope.customerBP.Moneda == "##") {
                        $scope.VisualizarMoneda = true;
                    }
                    else {
                        $scope.VisualizarMoneda = false;
                    }
                    //saber si tiene cargado el metodo de pago 03 o 99
                }
                $scope.BorrarDetallePedido();
                $scope.ActualizarTotalAPagar();
                $scope.ActualizarTotalUnidades();
            });
        }
    }

    $scope.$watch('customerBP.CardCode', function (newV, oldV) {
        console.log("Monitor de cliente: new:" + newV + " | " + oldV);
        if (newV != oldV) {
            console.log("Cambio de cliente: ");
            console.log($scope.customerBP.CardCode);
            console.log($scope.customerBP.CardName);

            if ($scope.customerBP.CardCode != undefined) {
                $scope.CargarDirecciones();
                $scope.CargarInputCDFI();
                $scope.CargarInputFP();
                $scope.CargarFormaEnvio();
                $scope.CargarMetodoPago();
                $scope.CargarTipoEnvio();
            }
        }
    });

    $scope.ArmarDireccion = function (i) {
        var j = $scope.DatosBP.ListaDireccion.findIndex(x => x.Address == i);
        var dir = $scope.DatosBP.ListaDireccion[j];
        console.log(j);
        console.log(dir);
        $scope.DatosBP.DirCompleta = `${dir.Street} ${dir.Block} ${dir.City}, ${dir.ZipCode}, ${dir.State} ${dir.Country}`;

        console.log($scope.DatosBP.direccion);
    }

    $scope.CargarInputFPP = function () {
        /*Cada que se selecciones un Nuevo Cliente, se limpia Combo de Forma de pago y  - JERM 30/1/2024 */
        $scope.DatosFP.Forma = ""; //Limpiar FormaPago
        $scope.DatosFP.ListaFormaPago = ""; //Limpiar FormaPago
        $scope.DatosFP.statusMetodoPago = 0;
        $scope.ListaItemDetalle = [];//limpia el arreglo del pedido        

        Servicio.ListarFormasPago($scope.customerBP.CardCode).then(function (response) {
            if (response.codigo == 0) {
                $scope.DatosFP.ListaFormaPago = response.mensaje;
                $scope.DatosFP.statusMetodoPago = ($scope.DatosFP.ListaFormaPago.findIndex(a => a.PymCode === '03')) + ($scope.DatosFP.ListaFormaPago.findIndex(x => x.PymCode === '99'));

                if ($scope.DatosFP.ListaFormaPago.findIndex(x => x.PymCode === '99') == 0) {
                    $scope.DatosFP.Forma = '99';
                }

                if ($scope.DocEntryPedido != 0) {
                    angular.forEach($scope.DatosFP.ListaFormaPago, function (dir) {
                        if ($scope.DirCotizacionCargada.includes(`${dir.Descrip}`)) {
                            $scope.DatosFP.Forma = dir.PymCode;
                            $scope.ArmarFormaDepago(dir.PymCode);
                        }
                    });
                }

                //if ($scope.Base === 'Cotizacion') {
                //    $scope.DatosFP.statusMetodoPago = 1;
                //    $scope.DatosFP.Forma = '0';
                //}
            }
            else {
                $scope.DatosFP.ListaFormaPago = []
                $scope.DatosFP.Forma = ""
                growl.error(`No se recuperarón las formas de pago  para el cliente ${$scope.customerBP.CardCode}`, { title: 'Sesión Agotada' });
            }
        });
    }

    $scope.CargarInputFP = function () {
        /*Cada que se selecciones un Nuevo Cliente, se limpia Combo de Forma de pago y  - JERM 30/1/2024 */
        $scope.DatosFP.ListaFormaPago = ""; //Limpiar FormaPago
        $scope.DatosFP.statusMetodoPago = 0;

        if ($scope.rol == 'V') {
            $scope.DatosFP.Forma = ""; //Limpiar FormaPago
        }
        else {
            $scope.DatosFP.Forma = "99"; //Limpiar FormaPago
        }

        Servicio.ListarFormasPago($scope.customerBP.CardCode).then(function (response) {
            if (response.codigo == 0) {
                $scope.DatosFP.ListaFormaPago = response.mensaje;
                $scope.DatosFP.statusMetodoPago = ($scope.DatosFP.ListaFormaPago.findIndex(a => a.PymCode === '03')) + ($scope.DatosFP.ListaFormaPago.findIndex(x => x.PymCode === '99'));

                if ($scope.DatosFP.ListaFormaPago.findIndex(x => x.PymCode === '99') == 0) {
                    $scope.DatosFP.Forma = '99';
                }

                if ($scope.DocEntryPedido != 0) {
                    angular.forEach($scope.DatosFP.ListaFormaPago, function (dir) {
                        if ($scope.DirCotizacionCargada.includes(`${dir.Descrip}`)) {
                            $scope.DatosFP.Forma = dir.PymCode;
                            $scope.ArmarFormaDepago(dir.PymCode);
                        }
                    });
                }

                //if ($scope.Base === 'Cotizacion') {
                //    $scope.DatosFP.statusMetodoPago = 1;
                //    $scope.DatosFP.Forma = '0';
                //}
            }
            else {
                $scope.DatosFP.ListaFormaPago = []
                $scope.DatosFP.Forma = ""
                growl.error(`No se recuperarón las formas de pago  para el cliente ${$scope.customerBP.CardCode}`, { title: 'Sesión Agotada' });
            }
        });
    }

    $scope.CargarInputCDFI = function () {
        /*Cada que se selecciones un Nuevo Cliente, se limpia Combo de Forma de pago y  - JERM 30/1/2024 */
        $scope.DatosCDFI.Forma = ""; //Limpiar CDFI
        $scope.DatosCDFI.ListaCDFI = ""; //Limpiar CDFI

        Servicio.ListarCFDI().then(function (respuesta) {
            if (respuesta.codigo == 0) {
                $scope.DatosCDFI.ListaCDFI = respuesta.mensaje;
                $scope.DatosCDFI.Forma = "G01";
                if ($scope.DocEntryPedido != 0) {
                    angular.forEach($scope.DatosCDFI.ListaCDFI, function (dir) {
                        if ($scope.DirCotizacionCargada.includes(`${dir.Code}`)) {
                            $scope.DatosCDFI.Forma = dir.Code;
                            $scope.ArmarListaCDFI(dir.Code);
                        }
                    });
                }
            }
            else {
                $scope.DatosCDFI.ListaCDFI = []
                $scope.DatosCDFI.Forma = ""
                growl.error(`No se recuperarón las formas de CDFI  para el cliente ${$scope.customerBP.CardCode}`, { title: 'Sesión Agotada' });
            }
        });
    }

    $scope.CargarFormaEnvio = function () {
        // JERM 07/05/2024 */
        /*EnvioTipo*/
        $scope.EnvioTipo.Forma = ""; //Limpiar FormaEnvio
        $scope.EnvioTipo.ListaFormaEnvio = ""; //Limpiar FormaEnvio

        Servicio.ListarFormaEnvio().then(function (respuesta) {
            if (respuesta.codigo == 0) {
                $scope.EnvioTipo.ListaFormaEnvio = respuesta.mensaje;
                if ($scope.DocEntryPedido != 0) {
                    angular.forEach($scope.EnvioTipo.ListaFormaEnvio, function (dir) {
                        if ($scope.DirCotizacionCargada.includes(`${dir.Code}`)) {
                            $scope.EnvioTipo.Forma = dir.Code;
                            $scope.ArmarListaFormaEnvio(dir.Code);
                        }
                    });
                }
            }
            else {
                $scope.$scope.EnvioTipo.ListaFormaEnvio = []
                $scope.EnvioTipo.Forma = ""
                growl.error(`No se recuperarón las formas de envio  para el cliente ${$scope.customerBP.CardCode}`, { title: 'Sesión Agotada' });
            }
        });
    }

    $scope.CargarTipoEnvio = function () {
        // JERM 07/05/2024 */
        /*TipoEnvio*/
        $scope.TipoEnvio.Forma = ""; //Limpiar FormaEnvio
        $scope.TipoEnvio.ListaTipoEnvio = ""; //Limpiar FormaEnvio

        Servicio.ListarTipoEnvio().then(function (respuesta) {
            if (respuesta.codigo == 0) {
                $scope.TipoEnvio.ListaTipoEnvio = respuesta.mensaje;

                if ($scope.DocEntryPedido != 0) {
                    angular.forEach($scope.TipoEnvio.ListaTipoEnvio, function (dir) {
                        if ($scope.DirCotizacionCargada.includes(`${dir.Code}`)) {
                            $scope.TipoEnvio.Forma = dir.Code;
                            $scope.ArmarListaTipoEnvio(dir.Code);
                        }
                    });
                }
            }
            else {
                $scope.$scope.TipoEnvio.ListaTipoEnvio = []
                $scope.TipoEnvio.Forma = ""
                growl.error(`No se recuperarón las tipos de envio  para el cliente ${$scope.customerBP.CardCode}`, { title: 'Sesión Agotada' });
            }
        });
    }

    $scope.CargarMetodoPago = function () {
        // JERM 07/05/2024 */
        /*EnvioTipo*/
        if ($scope.rol == 'V') {
            $scope.MetodoPago.Forma = ""; //Limpiar FormaEnvio
        }
        else {
            $scope.MetodoPago.Forma = "PPD"; //Limpiar FormaEnvio
        }

        $scope.MetodoPago.ListaMetodoPago = ""; //Limpiar FormaEnvio
        Servicio.ListarMetodoPago().then(function (respuesta) {
            if (respuesta.codigo == 0) {
                $scope.MetodoPago.ListaMetodoPago = respuesta.mensaje;

                if ($scope.DocEntryPedido != 0) {
                    angular.forEach($scope.MetodoPago.ListaMetodoPago, function (dir) {
                        if ($scope.DirCotizacionCargada.includes(`${dir.Code}`)) {
                            $scope.MetodoPago.Forma = dir.Code;
                            $scope.ArmarListaMetodoPago(dir.Code);
                        }
                    });
                }
            }
            else {
                $scope.$scope.MetodoPago.ListaMetodoPago = []
                $scope.MetodoPago.Forma = ""
                growl.error(`No se recuperarón las formas de envio  para el cliente ${$scope.customerBP.CardCode}`, { title: 'Sesión Agotada' });
            }
        });
    }

    $scope.ArmarFormaDepago = function (i) {
        var j = $scope.DatosFP.ListaFormaPago.findIndex(x => x.Descrip == i);
        var dir = $scope.DatosFP.ListaFormaPago[j];
    }

    $scope.ArmarListaCDFI = function (a) {
        var m = $scope.DatosCDFI.ListaCDFI.findIndex(x => x.Descrip == a);
        var dir = $scope.DatosCDFI.ListaCDFI[m];
        console.log($scope.DatosCDFI.Forma);
    }

    $scope.ArmarListaFormaEnvio = function (a) {
        var m = $scope.EnvioTipo.ListaFormaEnvio.findIndex(x => x.Documento == a);
        var dir = $scope.EnvioTipo.ListaFormaEnvio[m];
    }

    $scope.ArmarListaTipoEnvio = function (a) {
        var m = $scope.TipoEnvio.ListaTipoEnvio.findIndex(x => x.Documento == a);
        var dir = $scope.TipoEnvio.ListaTipoEnvio[m];
    }

    $scope.ArmarListaMetodoPago = function (a) {
        var m = $scope.MetodoPago.ListaMetodoPago.findIndex(x => x.Documento == a);
        var dir = $scope.MetodoPago.ListaMetodoPago[m];
        console.log($scope.MetodoPago.ListaMetodoPago);
        console.log($scope.MetodoPago.Forma);
    }

    var ListaPedidoController = function ($scope, $uibModalInstance, $log, SNList) {
        $scope.QueryBusquedaSN = "";
        $scope.QtyPageViewSN = 9;
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion);
        }
        //$scope.dataTableOpt = {
        //    //custom datatable options 
        //    // or load data through ajax call also
        //    "aLengthMenu": [[5, 10, 50, 100, -1], [5, 10, 50, 100, 'All']],
        //};

        $scope.ListaSN = SNList;
        $scope.SocioSeleccionado = function (socioN) {
            $scope.Seleccion = socioN;
            $scope.ok();
        }
    }

    ///:::::REGION MODAL PARA ARTICULOS  ::::::::::////
    $scope.MostrarModalITEM = function () {
        console.log("Entró a MostrarModalITEM");
        console.log("Valor de rol:", $scope.rol);

        if ($scope.customerBP.CardCode == undefined) {
            growl.error("No ha seleccionado al Socio de Negocios", { title: 'Error' });
            return;
        }

        console.log($scope.rol);
        var cardCodeN = $scope.customerBP.CardCode;
        var whsCodeN = $scope.WareHoseCode;
        //var priceList: ($scope.rol == 'A' || $scope.rol == 'O') ? 13 : $scope.customerBP.ListaPrecios,
        //var descuento: ($scope.Base == 'Cotizacion') ? 'No' : 'Si'

        let listaPrecio = $scope.customerBP.ListaPrecios;
        let descuento = 'Si'
        if ($scope.Base == 'Cotizacion') {
            descuento = 'No'
        }

        if ($scope.rol == 'V') {
            Servicio.ObtenerArticulosSAPVendedor($scope.customerBP.CardCode, $scope.WareHoseCode, $scope.customerBP.ListaPrecios).then(function (respuestaITEM) {

                if (respuestaITEM != null) {
                    //llamamos a la modal y mandamos valores: lista de Arituclos
                    $scope.AbrirModalArticulos(respuestaITEM);
                }
            });

        }
        else {
            if ($scope.rol == 'A' || $scope.rol == 'O') {
                listaPrecio = 13
            }

            $scope.AbrirModalArticulosN = function () {
                var modalInstance = $uibModal.open({
                    templateUrl: Ruta.path + "/Modales/ListaArticulosN.html",
                    controller: ListaItemControllerN,
                    keyboard: false,
                    backdrop: "static",
                    resolve: {
                        cardCodeN: function () {
                            return cardCodeN;
                        },
                        listaPrecio: function () {
                            return listaPrecio;
                        },
                        descuento: function () {
                            return descuento;
                        }
                    }
                });

                modalInstance.result.then(function (itemSeleccionado) {
                    if (itemSeleccionado && itemSeleccionado.length > 0) {

                        $scope.ItemSeleccionado = itemSeleccionado[0]; // Guardar el ítem seleccionado
                        $scope.LlenarDetallePedido(itemSeleccionado);
                    }
                });
            };

            $scope.AbrirModalArticulosN();
        }
    }

    $scope.AbrirModalArticulos = function (listaDeArticulos) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + "/Modales/ListaArticulos.html",
            controller: ListaItemController,//Recibe ITEMList, es el nombre de su parametro.
            keyboard: false,
            backdrop: "static",
            resolve: {
                ITEMList: function () {

                    return listaDeArticulos;//Este es el parametro que recibo en la funcion AbrirModalArticulos
                }
            }
        }).result.then(function (result) {
            //cacho los valores seleccionados de la modal

            $scope.LlenarDetallePedido(result);
        });
    }

    $scope.AbrirModalArticulosN = function () {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + "/Modales/ListaArticulosN.html",
            controller: ListaItemControllerN,
            keyboard: false,
            backdrop: "static"
        });

        modalInstance.result.then(function (itemSeleccionado) {
            if (itemSeleccionado && itemSeleccionado.length > 0) {
                console.log("Artículo seleccionado:", itemSeleccionado[0]);
                $scope.ItemSeleccionado = itemSeleccionado[0]; // Guardar el ítem seleccionado
                $scope.LlenarDetallePedido(itemSeleccionado);
            }
        });
    }

    var ListaItemController = function ($scope, $uibModalInstance, $log, ITEMList) {
        $scope.Multiseleccion = false;
        $scope.QtyPageViewItem = 9;
        $scope.QueryBusquedaItem = "";
        $scope.ItemSeleccion = [];

        $scope.ok = function () {
            $uibModalInstance.close($scope.ItemSeleccion);
        }

        //$scope.dataTableOpt = {
        //    "aLengthMenu": [[5, 10, 50, 100, -1], [5, 10, 50, 100, 'All']],
        //};
        //ListaITEM sera alcanzable solo en la modal
        $scope.ListaITEM = ITEMList;//llena la tabla de la modal
        $scope.ListaITEM

        $scope.ItemSeleccionado = function (item) {
            $scope.ItemSeleccion.push(item);

            if ($scope.Multiseleccion == false) {
                $scope.ok();
            }
            //$scope.ok();
        }
    }

    var ListaItemControllerN = function ($scope, $uibModalInstance, $log, $timeout, Servicio, cardCodeN, listaPrecio, descuento) {
        $scope.Multiseleccion = false;
        $scope.QtyPageViewItem = 9;
        $scope.QueryBusquedaItem = "";
        $scope.ItemSeleccion = [];
        $scope.ListaITEM = [];
        $scope.loading = false;
        $scope.searchTimeout = null;

        $scope.ok = function () {
            $uibModalInstance.close($scope.ItemSeleccion);
        }

        $scope.buscarArticulos = function () {
            if ($scope.QueryBusquedaItem.length < 2) {
                $scope.ListaITEM = [];
                return;
            }

            $scope.loading = true;
            // Cancelar búsqueda anterior si existe
            if ($scope.searchTimeout) {
                $timeout.cancel($scope.searchTimeout);
            }

            // Nueva búsqueda con debounce de 500ms
            $scope.searchTimeout = $timeout(function () {
                Servicio.ObtenerArticulosSAPN(
                    cardCodeN,
                    '',
                    listaPrecio,
                    descuento,
                    $scope.QueryBusquedaItem
                ).then(function (respuesta) {
                    $scope.ListaITEM = respuesta;
                    $scope.loading = false;
                }).catch(function () {
                    $scope.loading = false;
                });
            }, 500);
        }

        $scope.ItemSeleccionado = function (item) {
            $scope.ItemSeleccion.push(item);
            if ($scope.Multiseleccion == false) {
                $scope.ok();
            }
        }
    }

    ///:::::REGION MODAL Cliente::::::::::////
    $scope.CargarDirecciones = function () {
        /*Cada que se selecciones un Nuevo Cliente, se limpia Combo de Direcciones y Campo de Direccion - IUO 30/10/2020 */
        $scope.DatosBP.direccion = ""; //Limpiar Combo
        $scope.DatosBP.DirCompleta = ""; //Limpiar Dirección

        Servicio.ObtenerDirecciones($scope.customerBP.CardCode).then(function (response) {
            if (response.codigo == 0) {
                $scope.DatosBP.ListaDireccion = response.mensaje;

                if ($scope.DocEntryPedido != 0) {
                    angular.forEach($scope.DatosBP.ListaDireccion, function (dir) {
                        if ($scope.DirCotizacionCargada.includes(`${dir.Address}`)) {
                            $scope.DatosBP.direccion = dir.Address;
                            $scope.ArmarDireccion(dir.Address);
                        }
                    });
                }
            }
            else {
                $scope.DatosBP.ListaDireccion = []
                $scope.DatosBP.direccion = ""
                growl.error(`No se recuperarón direcciones para el cliente ${$scope.customerBP.CardCode}`, { title: 'Sesión Agotada' });
            }
        });
    }

    $scope.ArmarDireccion = function (i) {
        var j = $scope.DatosBP.ListaDireccion.findIndex(x => x.Address == i);
        var dir = $scope.DatosBP.ListaDireccion[j];
        $scope.DatosBP.DirCompleta = `${dir.Street} ${dir.Block} ${dir.City}, ${dir.ZipCode}, ${dir.State} ${dir.Country}`;
    }

    var ListaPedidoController = function ($scope, $uibModalInstance, $log, SNList) {
        $scope.QueryBusquedaSN = "";
        $scope.QtyPageViewSN = 9;

        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion);
        }
        //$scope.dataTableOpt = {
        //    //custom datatable options 
        //    // or load data through ajax call also
        //    "aLengthMenu": [[5, 10, 50, 100, -1], [5, 10, 50, 100, 'All']],
        //};

        $scope.ListaSN = SNList;
        $scope.SocioSeleccionado = function (socioN) {
            $scope.Seleccion = socioN;
            $scope.ok();
        }
    }

    /////// REGION OPERACIOENS 
    $scope.LlenarDetallePedido = function (articulos) {
        console.log("Llenar detalle pedido: Articulo");
        console.log(articulos);

        $scope.ivaUnitario = 0;
        $scope.precioMasIva = 0;
        $scope.ValorDelImpuesto = 0;

        angular.forEach(articulos, function (articulo) {
            if (articulo.VATLiable == "Y") {//traer el impuesto si el articulo aplica
                if (articulo.IndirectTax == "Y") {//si tiene activo IEPS en sap
                    Servicio.ObtenerImpuestoSAP(articulo.TaxCodeAR).then(function (respuestaTax) {
                        if (respuestaTax == -1) {
                            growl.warning("No se logró recuperar el impuesto del Artículo.", { title: '¡Atención!' });
                            return;
                        }
                        if (respuestaTax != undefined) {
                            $scope.ValorDelImpuesto = respuestaTax;
                            $scope.ObtenerImpuesto(articulo, $scope.ValorDelImpuesto);
                        }
                    });
                }
                else {//si no tiene IEPS, traer el impuesto del socio de negocios
                    Servicio.ObtenerCodigoImpuestoBP($scope.customerBP.CardCode).then(function (respuestaTaxCodeBP) {
                        if (respuestaTaxCodeBP != undefined) {
                            if (respuestaTaxCodeBP.Rate == -1) {
                                growl.warning("No se logró recuperar el Impuesto del Cliente.", { title: '¡Atención!' });
                                return;
                            }
                            else if (respuestaTaxCodeBP.Rate == null) {
                                growl.warning("El Cliente no tiene configurado el Impuesto.", { title: '¡Atención!' });
                                return;
                            }
                            else {
                                $scope.ValorDelImpuesto = respuestaTaxCodeBP.Rate;
                                $scope.ObtenerImpuesto(articulo, $scope.ValorDelImpuesto);
                            }

                        }
                    })
                }
            }
            else {//cuando el articulo no aplica a impuesto
                Servicio.ObtenerImpuestoDefault().then(function (respuestaTaxDefault) {
                    if (respuestaTaxDefault != null) {
                        $scope.ValorDelImpuesto = respuestaTaxDefault;
                        $scope.ObtenerImpuesto(articulo, $scope.ValorDelImpuesto);
                    }
                })
            }
        });
    }

    //////////////////////////PROCESO PARA MOSTRAR ALMACENES CON EL STOCK SELECCIONADO ////////////
    $scope.MostrarModalAlmacen = function (indice) {
        if ($scope.ListaItemDetalle[indice] == undefined) {
            growl.error("No se ha encontrado el articulo lista detalle", { title: 'Error' });
            return;
        }

        Servicio.ObtenerStockalamcen($scope.customerBP.CardCode, $scope.ListaItemDetalle[indice].Codigo, $scope.rol).then(function (respuestaITEM) {
            if (respuestaITEM != null) {
                //llamamos a la modal y mandamos valores: lista de Arituclos
                $scope.AbrirModalAlmacen(respuestaITEM);

            }
        });

        $scope.AbrirModalAlmacen = function (listaDeArticulos) {
            console.log(listaDeArticulos);
            var modalInstance = $uibModal.open({
                templateUrl: Ruta.path + "/Modales/ListaInventarioStock.html",
                controller: ListaInventarioController,//Recibe ITEMList, es el nombre de su parametro.
                keyboard: false,
                backdrop: "static",

                resolve: {
                    ITEMList: function () {

                        return listaDeArticulos;//Este es el parametro que recibo en la funcion AbrirModalArticulos
                    }
                }
            }).result.then(function (result) {
                //cacho los valores seleccionados de la modal
                //console.log(result.Almacen);
                //console.log(result.stock);
                for (i = 0; i < $scope.ListaItemDetalle.length; i++) {
                    //si el articulo esta en uno de los almacenes que tiene envio en dos dias habiles entra al if
                    if ($scope.ListaItemDetalle[i].Codigo == $scope.ListaItemDetalle[indice].Codigo && $scope.ListaItemDetalle[i].whsCode == result[0].whsCode) {
                        // si la fecha es menor o igual a 3 entra al if para darle valor esto se hace para que no tome el valor del arreglo actual
                        console.log(" ya existe este articulo y almacén por favor cambie de almacén")
                        growl.warning('ya existe este articulo y almacén por favor cambie de almacén.', { title: '¡Advertencia!' });
                        return;
                    }
                }

                $scope.ListaItemDetalle[indice].whsCode = result[0].whsCode;
                $scope.ListaItemDetalle[indice].Almacen = result[0].Almacen;
                /*$scope.ListaItemDetalle[indice].DiaEntrega = result[0].DiaEntrega;*/
                $scope.ListaItemDetalle[indice].stock = result[0].stock;
                $scope.ListaItemDetalle[indice].comprometido = result[0].comprometido;
                $scope.ListaItemDetalle[indice].stockReal = result[0].stock - result[0].comprometido;
                //llamo a llamar la funcion para calcular la fecha
                //fecha entrega articulo individual
                $scope.ListaItemDetalle[indice].FechaEntrega = $scope.FechaEntregaInd(result[0].whsCode);

                //Solo se activa para clientes la actualizacion de fecha entrega
                if ($scope.rol == 'C') {
                    $scope.FechaSistema = $scope.FechaEntregaC();
                }

                if ($scope.ListaItemDetalle[indice].Cantidad > $scope.ListaItemDetalle[indice].stock) {
                    $scope.ListaItemDetalle[indice].Cantidad = $scope.ListaItemDetalle[indice].stock;
                }
                //Validar que el stock y el comprometido no sea menor o igual a 0

                for (l = 0; l < $scope.ListaItemDetalle.length; l++) {
                    var stockReal = 0;
                    stockReal = parseFloat($scope.ListaItemDetalle[l].Cantidad) + parseFloat($scope.ListaItemDetalle[l].comprometido);
                    stockReal = $scope.ListaItemDetalle[l].stock - stockReal
                    $scope.ListaItemDetalle[l].stockReal = stockReal;

                    if ($scope.ListaItemDetalle[l].stockReal < 0) {
                        growl.warning("El articulo:" + $scope.ListaItemDetalle[l].Nombre + " No cuenta con stock suficiente.", { title: 'Sin Existencias' });
                        return;
                    }
                }
            });
        }
    }

    var ListaInventarioController = function ($scope, $uibModalInstance, $log, ITEMList) {
        $scope.QtyPageViewItem = 9;
        $scope.QueryBusquedaItem = "";
        $scope.ItemSeleccion = [];

        $scope.ok = function () {
            $uibModalInstance.close($scope.ItemSeleccion);
        }

        //$scope.dataTableOpt = {
        //    "aLengthMenu": [[5, 10, 50, 100, -1], [5, 10, 50, 100, 'All']],
        //};
        //ListaITEM sera alcanzable solo en la modal
        $scope.ListaITEM = ITEMList;//llena la tabla de la modal
        $scope.ListaITEM

        $scope.ItemSeleccionado = function (item) {
            $scope.ItemSeleccion.push(item);
            $scope.ok();
        }
    }

    //ACTUALIZA CANTIDAD DEL ARTICULO EN DETALLE
    $scope.ActualizarCantidadEnArticulo = function (nuevaCantidad, indice) {
        if (nuevaCantidad > $scope.ListaItemDetalle[indice].stock) {
            growl.warning('No se puede superar la cantidad del almacén.', { title: '¡Advertencia!' });
            nuevaCantidad = 0;
            //Nueva Cantidad de Articulos
            $scope.ListaItemDetalle[indice].Cantidad = $scope.ListaItemDetalle[indice].stock;

            //Precio de Articulo
            var PrecioArticulo = $scope.ListaItemDetalle[indice].PrecioUnitario;
            //Descuento del cliente
            var Descuento = parseFloat($scope.ListaItemDetalle[indice].ValorImpuesto);
            //Descuento del articulo
            var Descuento2 = parseFloat($scope.ListaItemDetalle[indice].Descuento);

            //******************************************************************************
            //Calculo de Importes
            //******************************************************************************
            //Subtotal : Cantidad * Precio
            var subtotalInicial = nuevaCantidad * PrecioArticulo;
            //Descuento de importe : Subtotal * Descuento Articulo / 100
            var descImporte = (parseFloat(subtotalInicial) * parseFloat(Descuento2)) / 100;
            //Total importe : Subtotal - Descuento
            var totalImporte = (parseFloat(subtotalInicial) - parseFloat(descImporte));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Calculo de Impuesto
            //******************************************************************************
            //Impuesto : Subtotal * Descuento de cliente / 100
            var impuestoArt = (parseFloat(subtotalInicial) * parseFloat(Descuento)) / 100;
            //Descuento de impuesto : Impuesto * Descuento Articulo / 100
            var descImpuesto = (parseFloat(impuestoArt) * parseFloat(Descuento2)) / 100;
            //Total impuesto : Subtotal - Descuento
            var totalImpuesto = (parseFloat(impuestoArt) - parseFloat(descImpuesto));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Total Final : total importe + total impuesto
            //******************************************************************************
            var TotalFinal = (parseFloat(totalImporte) + parseFloat(totalImpuesto));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Agregar valores actualizados
            //******************************************************************************
            $scope.ListaItemDetalle[indice].IVA = parseFloat(totalImpuesto).toFixed(4);
            $scope.ListaItemDetalle[indice].Total = parseFloat(TotalFinal).toFixed(4);

            $scope.ActualizarTotalAPagar();
            $scope.ActualizarTotalUnidades();
            return;
        }

        if ($scope.ListaItemDetalle[indice].multiplo > 0) {
            //saber si es multiplo actualizacion 31 de mayo 2024 
            let resto = nuevaCantidad % $scope.ListaItemDetalle[indice].multiplo;
            if (resto != 0) {
                growl.warning('Debe escoger una cantidad que sea multiplo de ' + $scope.ListaItemDetalle[indice].multiplo + '.', { title: '¡Advertencia!' });
                nuevaCantidad = 0;
                $scope.ListaItemDetalle[indice].Cantidad = 0;
                return;
            }
        }

        if (nuevaCantidad == null || nuevaCantidad == undefined || nuevaCantidad == "" || nuevaCantidad == 0) {
            nuevaCantidad == 1;
            //Nueva Cantidad de Articulos
            $scope.ListaItemDetalle[indice].Cantidad = 1;
            console.log($scope.ListaItemDetalle[indice].Cantidad)
            //Precio de Articulo
            var PrecioArticulo = $scope.ListaItemDetalle[indice].PrecioUnitario;
            //Descuento del cliente
            var Descuento = parseFloat($scope.ListaItemDetalle[indice].ValorImpuesto);
            //Descuento del articulo
            var Descuento2 = parseFloat($scope.ListaItemDetalle[indice].Descuento);

            //******************************************************************************
            //Calculo de Importes
            //******************************************************************************
            //Subtotal : Cantidad * Precio
            var subtotalInicial = nuevaCantidad * PrecioArticulo;
            //Descuento de importe : Subtotal * Descuento Articulo / 100
            var descImporte = (parseFloat(subtotalInicial) * parseFloat(Descuento2)) / 100;
            //Total importe : Subtotal - Descuento
            var totalImporte = (parseFloat(subtotalInicial) - parseFloat(descImporte));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Calculo de Impuesto
            //******************************************************************************
            //Impuesto : Subtotal * Descuento de cliente / 100
            var impuestoArt = (parseFloat(subtotalInicial) * parseFloat(Descuento)) / 100;
            //Descuento de impuesto : Impuesto * Descuento Articulo / 100
            var descImpuesto = (parseFloat(impuestoArt) * parseFloat(Descuento2)) / 100;
            //Total impuesto : Subtotal - Descuento
            var totalImpuesto = (parseFloat(impuestoArt) - parseFloat(descImpuesto));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Total Final : total importe + total impuesto
            //******************************************************************************
            var TotalFinal = (parseFloat(totalImporte) + parseFloat(totalImpuesto));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Agregar valores actualizados
            //******************************************************************************
            $scope.ListaItemDetalle[indice].IVA = parseFloat(totalImpuesto).toFixed(4);
            $scope.ListaItemDetalle[indice].Total = parseFloat(TotalFinal).toFixed(4);

            $scope.ActualizarTotalAPagar();
            $scope.ActualizarTotalUnidades();
            return;
        }

        if (nuevaCantidad < 1) {
            nuevaCantidad = 1;
            growl.warning('No se puede agregar menor a 1.', { title: '¡Advertencia!' });
            //Nueva Cantidad de Articulos
            $scope.ListaItemDetalle[indice].Cantidad = 1;

            //Precio de Articulo
            var PrecioArticulo = $scope.ListaItemDetalle[indice].PrecioUnitario;
            //Descuento del cliente
            var Descuento = parseFloat($scope.ListaItemDetalle[indice].ValorImpuesto);
            //Descuento del articulo
            var Descuento2 = parseFloat($scope.ListaItemDetalle[indice].Descuento);

            //******************************************************************************
            //Calculo de Importes
            //******************************************************************************
            //Subtotal : Cantidad * Precio
            var subtotalInicial = nuevaCantidad * PrecioArticulo;
            //Descuento de importe : Subtotal * Descuento Articulo / 100
            var descImporte = (parseFloat(subtotalInicial) * parseFloat(Descuento2)) / 100;
            //Total importe : Subtotal - Descuento
            var totalImporte = (parseFloat(subtotalInicial) - parseFloat(descImporte));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Calculo de Impuesto
            //******************************************************************************
            //Impuesto : Subtotal * Descuento de cliente / 100
            var impuestoArt = (parseFloat(subtotalInicial) * parseFloat(Descuento)) / 100;
            //Descuento de impuesto : Impuesto * Descuento Articulo / 100
            var descImpuesto = (parseFloat(impuestoArt) * parseFloat(Descuento2)) / 100;
            //Total impuesto : Subtotal - Descuento
            var totalImpuesto = (parseFloat(impuestoArt) - parseFloat(descImpuesto));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Total Final : total importe + total impuesto
            //******************************************************************************
            var TotalFinal = (parseFloat(totalImporte) + parseFloat(totalImpuesto));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Agregar valores actualizados
            //******************************************************************************
            $scope.ListaItemDetalle[indice].IVA = parseFloat(totalImpuesto).toFixed(4);
            $scope.ListaItemDetalle[indice].Total = parseFloat(TotalFinal).toFixed(4);

            $scope.ActualizarTotalAPagar();
            $scope.ActualizarTotalUnidades();
            return;
        }

        if (nuevaCantidad <= ($scope.ListaItemDetalle[indice].stock) && nuevaCantidad >= 1) {
            //Nueva Cantidad de Articulos

            $scope.ListaItemDetalle[indice].Cantidad = nuevaCantidad;
            console.log($scope.ListaItemDetalle[indice].stockReal);
            //Precio de Articulo
            var PrecioArticulo = $scope.ListaItemDetalle[indice].PrecioUnitario;
            //Descuento del cliente
            var Descuento = parseFloat($scope.ListaItemDetalle[indice].ValorImpuesto);
            //Descuento del articulo
            var Descuento2 = parseFloat($scope.ListaItemDetalle[indice].Descuento);

            //******************************************************************************
            //Calculo de Importes
            //******************************************************************************
            //Subtotal : Cantidad * Precio
            var subtotalInicial = nuevaCantidad * PrecioArticulo;
            //Descuento de importe : Subtotal * Descuento Articulo / 100
            var descImporte = (parseFloat(subtotalInicial) * parseFloat(Descuento2)) / 100;
            //Total importe : Subtotal - Descuento
            var totalImporte = (parseFloat(subtotalInicial) - parseFloat(descImporte));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Calculo de Impuesto
            //******************************************************************************
            //Impuesto : Subtotal * Descuento de cliente / 100
            var impuestoArt = (parseFloat(subtotalInicial) * parseFloat(Descuento)) / 100;
            //Descuento de impuesto : Impuesto * Descuento Articulo / 100
            var descImpuesto = (parseFloat(impuestoArt) * parseFloat(Descuento2)) / 100;
            //Total impuesto : Subtotal - Descuento
            var totalImpuesto = (parseFloat(impuestoArt) - parseFloat(descImpuesto));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Total Final : total importe + total impuesto
            //******************************************************************************
            var TotalFinal = (parseFloat(totalImporte) + parseFloat(totalImpuesto));
            //******************************************************************************
            //******************************************************************************
            //******************************************************************************
            //Agregar valores actualizados
            //******************************************************************************
            $scope.ListaItemDetalle[indice].IVA = parseFloat(totalImpuesto).toFixed(4);
            $scope.ListaItemDetalle[indice].Total = parseFloat(TotalFinal).toFixed(4);

            $scope.ActualizarTotalAPagar();
            $scope.ActualizarTotalUnidades();
            return;
        }
    }

    $scope.ActualizarDescuento = function (nuevoDescuento, indice) {
        if (nuevoDescuento == null || nuevoDescuento == undefined) {
            nuevoDescuento = 0;
        }
        else if (nuevoDescuento < 0) {
            nuevoDescuento = 0;
        }
        else if (nuevoDescuento > 100) {
            nuevoDescuento = 100;
        }
        var descuentoAnterior = $scope.ListaItemDetalle[indice].PrtgeDisc;

        //APLICA DESCUENTO DE ARTICULO
        var PrecioArticulo = $scope.ListaItemDetalle[indice].PrecioUnitario;//precio auxiliar
        var ValorDeDescuento = (parseFloat(nuevoDescuento) * parseFloat(PrecioArticulo)) / 100; //precio calculado con descuento
        var PrecioArticuloConDescuento = parseFloat(PrecioArticulo) - ValorDeDescuento;
        var Impuesto = $scope.ListaItemDetalle[indice].ValorImpuesto;

        var NuevoImporteImpuesto = (PrecioArticuloConDescuento * $scope.ListaItemDetalle[indice].ValorImpuesto) / 100;
        NuevoImporteImpuesto = parseFloat(NuevoImporteImpuesto) * parseInt($scope.ListaItemDetalle[indice].Cantidad);
        var TotalImpuestoDeLinea = NuevoImporteImpuesto;//parseInt($scope.ListaItemDetalle[indice].Cantidad) * NuevoImporteImpuesto;
        var TotalDeLineaSinIVA = parseInt($scope.ListaItemDetalle[indice].Cantidad) * PrecioArticuloConDescuento;

        $scope.ListaItemDetalle[indice].Descuento = parseFloat(nuevoDescuento);
        $scope.ListaItemDetalle[indice].PrtgeDisc = parseFloat(nuevoDescuento);
        $scope.ListaItemDetalle[indice].PrecioLinea = parseFloat(PrecioArticuloConDescuento);
        $scope.ListaItemDetalle[indice].Precio = parseFloat(PrecioArticuloConDescuento);
        $scope.ListaItemDetalle[indice].IVA = NuevoImporteImpuesto.toFixed(4);
        $scope.ListaItemDetalle[indice].Total = (TotalDeLineaSinIVA + TotalImpuestoDeLinea).toFixed(4);

        $scope.ActualizarTotalAPagar();//actualizarTotal a Pagar
        $scope.ActualizarTotalUnidades();

        //APLICA DESCUENTO DE CLIENTE
        var PrecioArticulo1 = $scope.ListaItemDetalle[indice].Precio;//precio auxiliar
        var ValorDeDescuento1 = (parseFloat($scope.customerBP.Descuento) * parseFloat(PrecioArticulo1)) / 100; //precio calculado con descuento
        var PrecioArticuloConDescuento1 = parseFloat(PrecioArticulo1) - ValorDeDescuento1;
        var Impuesto1 = $scope.ListaItemDetalle[indice].ValorImpuesto;

        var NuevoImporteImpuesto1 = (PrecioArticuloConDescuento1 * $scope.ListaItemDetalle[indice].ValorImpuesto) / 100;
        NuevoImporteImpuesto1 = parseFloat(NuevoImporteImpuesto1) * parseInt($scope.ListaItemDetalle[indice].Cantidad);
        var TotalImpuestoDeLinea1 = NuevoImporteImpuesto1; //parseInt($scope.ListaItemDetalle[indice].Cantidad) * NuevoImporteImpuesto;
        var TotalDeLineaSinIVA1 = parseInt($scope.ListaItemDetalle[indice].Cantidad) * PrecioArticuloConDescuento1;

        $scope.ListaItemDetalle[indice].Precio = parseFloat(PrecioArticuloConDescuento1);
        $scope.ListaItemDetalle[indice].IVA = NuevoImporteImpuesto1.toFixed(4);
        $scope.ListaItemDetalle[indice].Total = (TotalDeLineaSinIVA1 + TotalImpuestoDeLinea1).toFixed(4);

        $scope.ActualizarTotalAPagar();//actualizarTotal a Pagar
        $scope.ActualizarTotalUnidades();
    };

    //ACTUALIZA EL PRECIO DEL ARTICULO
    $scope.ActualizarPrecioArticuloDetalle = function (nuevoPrecio, indice) {
        //Nuevo Precio de Articulo
        $scope.ListaItemDetalle[indice].PrecioUnitario = nuevoPrecio;
        var PrecioArticulo = parseFloat($scope.ListaItemDetalle[indice].PrecioUnitario);
        //Cantidad de Articulos
        var CantidadArticulo = $scope.ListaItemDetalle[indice].Cantidad;
        //Descuento del cliente
        var Descuento = parseFloat($scope.ListaItemDetalle[indice].ValorImpuesto);
        //Descuento del articulo
        var Descuento2 = parseFloat($scope.ListaItemDetalle[indice].Descuento);

        //******************************************************************************
        //Calculo de Importes
        //******************************************************************************
        //Subtotal : Cantidad * Precio
        var subtotalInicial = CantidadArticulo * PrecioArticulo;
        //Descuento de importe : Subtotal * Descuento Articulo / 100
        var descImporte = (parseFloat(subtotalInicial) * parseFloat(Descuento2)) / 100;
        //Total importe : Subtotal - Descuento
        var totalImporte = (parseFloat(subtotalInicial) - parseFloat(descImporte));
        //******************************************************************************
        //******************************************************************************
        //******************************************************************************
        //Calculo de Impuesto
        //******************************************************************************
        //Impuesto : Subtotal * Descuento de cliente / 100
        var impuestoArt = (parseFloat(subtotalInicial) * parseFloat(Descuento)) / 100;
        //Descuento de impuesto : Impuesto * Descuento Articulo / 100
        var descImpuesto = (parseFloat(impuestoArt) * parseFloat(Descuento2)) / 100;
        //Total impuesto : Subtotal - Descuento
        var totalImpuesto = (parseFloat(impuestoArt) - parseFloat(descImpuesto));
        //******************************************************************************
        //******************************************************************************
        //******************************************************************************
        //Total Final : total importe + total impuesto
        //******************************************************************************
        var TotalFinal = (parseFloat(totalImporte) + parseFloat(totalImpuesto));
        //******************************************************************************
        //******************************************************************************
        //******************************************************************************
        //Agregar valores actualizados
        //******************************************************************************
        $scope.ListaItemDetalle[indice].IVA = parseFloat(totalImpuesto).toFixed(4);
        $scope.ListaItemDetalle[indice].Total = parseFloat(TotalFinal).toFixed(4);
        $scope.ListaItemDetalle[indice].PrecioUnitario = parseFloat(PrecioArticulo).toFixed(4);
        $scope.ListaItemDetalle[indice].Precio = parseFloat(PrecioArticulo).toFixed(4);

        $scope.ActualizarTotalAPagar();
        $scope.ActualizarTotalUnidades();
        //******************************************************************************
        //******************************************************************************

        //asignando nuevo precio al articulo
        //$scope.ListaItemDetalle[indice].Precio = parseFloat(nuevoPrecio);
        //calcular impuesto por articulo
        //$scope.nuevoIvaUnitario = (parseFloat($scope.Impuesto) * parseFloat(nuevoPrecio)) / 100;
        //asignando nuevo iva total por linea
        //$scope.ListaItemDetalle[indice].IVA = $scope.nuevoIvaUnitario * $scope.ListaItemDetalle[indice].Cantidad;
        //calculando nuevo total por linea
        //$scope.nuevoPrecioMasIVA = $scope.ListaItemDetalle[indice].IVA + parseFloat(nuevoPrecio)*$scope.ListaItemDetalle[indice].Cantidad;
        //asignando nuevo total al linea detalle
        //$scope.ListaItemDetalle[indice].Total = $scope.nuevoPrecioMasIVA;
        //$scope.ActualizarTotalAPagar();//actualizarTotal a Pagar
        //$scope.ActualizarTotalUnidades();		
    }

    //ACTUALIZA LA FECHA DE ENTREGA EN EL ARTICULO DEL DETALLE
    $scope.ActFechaEntregaDetalle = function (nuevaFecha, indice) {
        //asigna nuevo objeto de fecha
        $scope.ListaItemDetalle[indice].FechaEntrega = nuevaFecha;
    }

    $scope.FechaEntregaC = function () {
        var FechaEnv = 0;
        //recorremos el arreglo de lista detalle  para saber el almacen con fecha maxima
        for (i = 0; i < $scope.ListaItemDetalle.length; i++) {
            //si el articulo esta en uno de los almacenes que tiene envio en dos dias habiles entra al if
            if ($scope.ListaItemDetalle[i].whsCode == "01" || "03" || "04") {
                // si la fecha es menor o igual a 3 entra al if para darle valor esto se hace para que no tome el valor del arreglo actual
                if (FechaEnv <= 3) {
                    FechaEnv = 3;
                }
            }

            //si el articulo esta en uno de los almacenes que tiene envio en 4 dias habiles entra al if
            if ($scope.ListaItemDetalle[i].whsCode == "02") {
                if (FechaEnv <= 7) {
                    FechaEnv = 7;
                }
            }

            if ($scope.ListaItemDetalle[i].whsCode == "02") {
                if (FechaEnv <= 7) {
                    FechaEnv = 7;
                }
            }
        }
        $scope.FechaSistema = null;

        //se descompone la fecha 
        var arrFecha = fechaCompleta.split('-');
        //se separa en arreglos los dias, mes y año
        var fecha = new Date(arrFecha[0], arrFecha[1] - 1, arrFecha[2]);
        var festivos = [ // Agregamos los festivos (dia, mes)
            [25, 12],
            [1, 1],
            [1, 5]
        ];

        //la hora se calcula mas arriba y si es mayor o igual a las 14 se suma un dia
        if (hora >= 14) {
            FechaEnv++;
        }

        //se recorre el for en vace a la fechaenv
        for (var i = 0; i < FechaEnv; i++) {
            var diaInvalido = false;
            fecha.setDate(fecha.getDate() + 1); // Sumamos de dia en dia
            for (var j = 0; j < festivos.length; j++) { // Verificamos si el dia + 1 es festivo
                var mesDia = festivos[j];
                if (fecha.getMonth() + 1 == mesDia[1] && fecha.getDate() == mesDia[0]) {
                    /* console.log(fecha.getDate() + ' es dia festivo (Sumamos un dia)');*/
                    diaInvalido = true;
                    break;
                }
            }

            if (fecha.getDay() == 0) { // Verificamos si es  domingo
                /*console.log(fecha.getDate() + ' es sábado o domingo (Sumamos un dia)');*/
                diaInvalido = true;
            }

            if (diaInvalido)
                FechaEnv++; // Si es fin de semana o festivo le sumamos un dia
        }
        /* return fecha.getFullYear() + '-' + (fecha.getMonth() + 1).toString().padStart(2, '0') + '-' + fecha.getDate().toString().padStart(2, '0');*/
        return fecha;
    }

    $scope.FechaEntregaInd = function (whsCode) {
        if ($scope.rol == "C") {
            var FechaEnv = 0;
            //recorremos el arreglo de lista detalle  para saber el almacen con fecha maxima
            //si el articulo esta en uno de los almacenes que tiene envio en dos dias habiles entra al if
            if (whsCode == "01" || "03" || "04") {
                // si la fecha es menor o igual a 3 entra al if para darle valor esto se hace para que no tome el valor del arreglo actual
                FechaEnv = 3;
            }

            //si el articulo esta en uno de los almacenes que tiene envio en 4 dias habiles entra al if
            if (whsCode == "02") {
                if (FechaEnv <= 7) {
                    FechaEnv = 7;
                }
            }

            $scope.FechaSistema = null;
            //se descompone la fecha
            var arrFecha = fechaCompleta.split('-');
            //se separa en arreglos los dias, mes y año
            var fecha = new Date(arrFecha[0], arrFecha[1] - 1, arrFecha[2]);
            var festivos = [ // Agregamos los festivos (dia, mes)
                [25, 12],
                [1, 1],
                [1, 5]
            ];

            //la hora se calcula mas arriba y si es mayor o igual a las 14 se suma un dia 
            if (hora >= 14) {
                FechaEnv++;
            }

            //se recorre el for en vace a la fechaenv
            for (var i = 0; i < FechaEnv; i++) {
                var diaInvalido = false;
                fecha.setDate(fecha.getDate() + 1); // Sumamos de dia en dia
                for (var j = 0; j < festivos.length; j++) { // Verificamos si el dia + 1 es festivo
                    var mesDia = festivos[j];
                    if (fecha.getMonth() + 1 == mesDia[1] && fecha.getDate() == mesDia[0]) {
                        /* console.log(fecha.getDate() + ' es dia festivo (Sumamos un dia)');*/
                        diaInvalido = true;
                        break;
                    }
                }

                if (fecha.getDay() == 0) { // Verificamos si es  domingo
                    /*console.log(fecha.getDate() + ' domingo (Sumamos un dia)');*/
                    diaInvalido = true;
                }

                if (diaInvalido)
                    FechaEnv++; // Si es fin de semana o festivo le sumamos un dia
            }

            return fecha.getDate().toString().padStart(2, '0') + '-' + (fecha.getMonth() + 1).toString().padStart(2, '0') + '-' + fecha.getFullYear();
            /*return fecha;*/
        }
        else {
            var fecha = $scope.FechaSistema;
            return fecha;
        }
    }

    $scope.ActualizarFechaInicioRpt = function (nuevaFechaDes) {
        $scope.ImprimirDesde = nuevaFechaDes;
        var fechaRpt = $scope.ImprimirDesde;
        var mesrpt = fechaRpt.getMonth() + 1;
        var diarpt = fechaRpt.getDate();
        $scope.ImprimirDesdeTxt = (diarpt < 10 ? '0' : '') + diarpt + '/' + (mesrpt < 10 ? '0' : '') + mesrpt + '/' + fechaRpt.getFullYear();
    }

    $scope.ActualizarFechaFinRpt = function (nuevaFechaFin) {
        $scope.ImprimirHasta = nuevaFechaFin;
        var fechaRpt = $scope.ImprimirHasta;
        var mesrpt = fechaRpt.getMonth() + 1;
        var diarpt = fechaRpt.getDate();
        $scope.ImprimirHastaTxt = (diarpt < 10 ? '0' : '') + diarpt + '/' + (mesrpt < 10 ? '0' : '') + mesrpt + '/' + fechaRpt.getFullYear();
    }

    $scope.ActualizarEstadoPedidoRpt = function (estado) {
        $scope.EstadoPedido = estado;
    }

    //ACTUALIZA LA FECHA DEL PEDIDO
    Date.prototype.addDays = function (days) {
        var date = new Date(this.valueOf());
        date.setDate(date.getDate() + days);
        return date;
    }

    //#############################################################
    $scope.ActualizarFechaDelPedido = function (nuevaFechaPedido) {
        $scope.FechaSistema = null;
        var date = new Date();
        if (nuevaFechaPedido < date.addDays(2)) {
            growl.warning("La fecha debe ser dos días posteriores a la fecha actual", { title: 'Mensaje Sistema' });
            $scope.FechaSistema = null;
        }
        else {
            $scope.FechaSistema = nuevaFechaPedido;
            for (i = 0; i < $scope.ListaItemDetalle.length; i++) {
                //si el articulo esta en uno de los almacenes que tiene envio en dos dias habiles entra al if
                $scope.ListaItemDetalle[i].FechaEntrega = $scope.FechaSistema;
            }
        }
    }

    $scope.ActualizarFechaDeCotiza = function (nuevaFechaCotizacion) {
        $scope.FechaSistemaCot = null;
        var date = new Date();
        if (nuevaFechaCotizacion < date.addDays(2)) {
            growl.warning("La fecha debe ser dos días posteriores a la fecha actual", { title: 'Mensaje Sistema' });
            $scope.FechaSistemaCot = null;
        }
        else {
            $scope.FechaSistemaCot = nuevaFechaCotizacion;
        }
    }

    //#############################################################

    //REMUEVE EL ARTICULO SELECIONADO DEL DETALLE
    $scope.RemoverArticuloEnDetalle = function (indice) {
        //remueve articulo en la posicion indicada
        $scope.ListaItemDetalle.splice(indice, 1);
        $scope.ActualizarTotalAPagar();
        $scope.ActualizarTotalUnidades();
    }

    $scope.RemoverArticuloExistenteEnDetalle = function (indice) {
        //remueve articulo en la posicion indicada
        $scope.ListaItemDetalle[indice].Delete = true;
        $scope.ActualizarTotalAPagar();
        $scope.ActualizarTotalUnidades();
    }

    $scope.ObtenerImpuesto = function (articulo, valorImpuesto) {
        var RateChange = 1;
        if ($scope.customerBP.Moneda == "##") {
            for (var i = 0; i < $scope.TasasDeCambio.length; i++) {
                if ($scope.TasasDeCambio[i].Currency == $scope.MonedaDelSN.Moneda) {
                    RateChange = $scope.TasasDeCambio[i].Rate;
                }
            }
            articulo.Price = articulo.Price / RateChange;
        }

        $scope.Impuesto = valorImpuesto;//Obtiene el impuesto 
        //calcular impuesto por articulo
        $scope.ivaUnitario = (parseFloat($scope.Impuesto) * parseFloat(articulo.Price)) / 100;
        $scope.precioMasIva = $scope.ivaUnitario + parseFloat(articulo.Price);

        var PrecioArticulo = articulo.Price;//precio auxiliar
        //if para si es asesor poner el precio del mostrador y no con descuento        
        var ValorDeDescuento = (parseFloat($scope.customerBP.Descuento) * parseFloat(PrecioArticulo)) / 100; //precio calculado con descuento
        var PrecioArticuloConDescuento = parseFloat(PrecioArticulo) - ValorDeDescuento;
        var Impuesto = $scope.Impuesto;
        var NuevoImporteImpuesto = (PrecioArticuloConDescuento * $scope.Impuesto) / 100;
        NuevoImporteImpuesto = parseFloat(NuevoImporteImpuesto) * 1;

        var TotalImpuestoDeLinea = NuevoImporteImpuesto;//parseInt($scope.ListaItemDetalle[indice].Cantidad) * NuevoImporteImpuesto;
        var TotalDeLineaSinIVA = parseInt(1) * PrecioArticuloConDescuento;

        //$scope.ListaItemDetalle[indice].Precio = parseFloat(PrecioArticuloConDescuento);
        // $scope.ListaItemDetalle[indice].IVA = NuevoImporteImpuesto.toFixed(4);
        //$scope.ListaItemDetalle[indice].Total = (TotalDeLineaSinIVA + TotalImpuestoDeLinea).toFixed(4);

        var row = $scope.ListaItemDetalle.length <= 0 ? 0 : $scope.ListaItemDetalle.length
        var totalDescuento = 0;
        var desc1 = 100 - parseFloat($scope.customerBP.Descuento);
        var desc2 = 100 - 0;
        var promDesc = (desc1 * desc2) / 100;
        totalDescuento = 100 - promDesc;

        var importeDescuento = (parseFloat(articulo.Price) * parseFloat(totalDescuento)) / 100;
        var priceItemWithoutDiscount = parseFloat(articulo.Price) - importeDescuento;

        //Añadir articulo a la lista
        $scope.ListaItemDetalle.push({
            Codigo: articulo.ItemCode,
            Nombre: articulo.ItemName,
            Cantidad: 0,
            Precio: parseFloat(PrecioArticuloConDescuento),//articulo.Price,
            PrecioLinea: 0.0,
            SujetoImpuesto: articulo.VATLiable,
            PrecioUnitario: articulo.Price,
            Descuento: 0.0,
            PrtgeDisc: 0,
            PrecioConDescuento: priceItemWithoutDiscount,
            ValorImpuesto: articulo.VATLiable == 'Y' ? valorImpuesto : 0,//porcentual
            CodigoVAT: articulo.TaxCodeAR,
            IVA: articulo.VATLiable == 'Y' ? NuevoImporteImpuesto.toFixed(2) : 0,
            Total: articulo.VATLiable == 'Y' ? (TotalDeLineaSinIVA + TotalImpuestoDeLinea).toFixed(2) : TotalDeLineaSinIVA.toFixed(2),
            FechaEntrega: $scope.FechaSistema,
            MetrosExtra: "METROS_EXTRA",
            NumberLine: row,
            UM: articulo.UnitSize,
            LineNum: -1,
            Stock: 0.0,
            Almacen: "",
            whsCode: "",
            PrecioMostrador: articulo.PrecioMostrador,
            comprometido: 0.0,
            multiplo: articulo.U_VtasPiezas,
            //nuevo
            U_Leyenda: articulo.U_Leyenda,
            Delete: false
        });

        console.log($scope.ListaItemDetalle);
        //calculo de total a pagar
        $scope.TotalAPagar = 0;
        $scope.SubttlAPagar = 0;
        $scope.TotalEnImpuesto = 0;

        for (var i = 0; i < $scope.ListaItemDetalle.length; i++) {
            $scope.SubttlAPagar = $scope.SubttlAPagar + parseFloat($scope.ListaItemDetalle[i].Total);
            $scope.TotalEnImpuesto = $scope.TotalEnImpuesto + parseFloat($scope.ListaItemDetalle[i].IVA);
        }

        $scope.TotalEnImpuestoStr = formatThousandsWithRounding($scope.TotalEnImpuesto, 2);
        $scope.TotalAPagar = parseFloat($scope.SubttlAPagar);// - parseFloat(PctjeDiscount);

        var cantidadFormateadaSbttl = formatThousandsWithRounding($scope.SubttlAPagar, 2);
        $scope.SbttlAPagarDisplay = cantidadFormateadaSbttl;

        var cantidadFormateadaTtl = formatThousandsWithRounding($scope.TotalAPagar, 2);
        $scope.TotalAPagarDisplay = cantidadFormateadaTtl.toLocaleString('en');

        //calculo de total de unidades
        $scope.TotalUnidades = 0;
        for (var i = 0; i < $scope.ListaItemDetalle.length; i++) {
            $scope.TotalUnidades = parseInt($scope.TotalUnidades) + parseInt($scope.ListaItemDetalle[i].Cantidad);
        }
    }

    //forma el JSON que se enviará para integrar el pedido: encabezado y detalle
    $scope.ExtraerJsonDetalle = function () {
        var fechaDeOrden = "";
        var fechaCot = "";

        $scope.FechaActual = new Date();
        var fechaActual = new Date();
        var yearActual = fechaActual.getFullYear();
        var mesActual = fechaActual.getMonth() + 1;
        var diaActual = fechaActual.getDate();

        fechaDeOrden = yearActual + '-' + (mesActual < 10 ? '0' : '') + mesActual + '-' + (diaActual < 10 ? '0' : '') + diaActual;
        fechaCot = yearActual + '-' + (mesActual < 10 ? '0' : '') + mesActual + '-' + (diaActual < 10 ? '0' : '') + diaActual;

        //Obtner el detalle del pedido a partir de ListaItemDetalle
        for (var linea = 0; linea < $scope.ListaItemDetalle.length; linea++) {
            if ($scope.ListaItemDetalle[linea].LineNum != null) {
                $scope.JsonPedido.DocumentLines.push({
                    ItemCode: $scope.ListaItemDetalle[linea].Codigo,
                    ItemDescription: $scope.ListaItemDetalle[linea].Nombre,
                    DiasEntrega: $scope.ListaItemDetalle[linea].FechaEntrega,
                    Precio: $scope.ListaItemDetalle[linea].Precio,
                    PrecioLinea: $scope.ListaItemDetalle[linea].PrecioLinea,
                    PrecioUnitario: $scope.ListaItemDetalle[linea].PrecioUnitario,
                    Quantity: $scope.ListaItemDetalle[linea].Cantidad,
                    Currency: $scope.MonedaDelSN.Moneda,
                    PrtgDiscount: $scope.ListaItemDetalle[linea].PrtgeDisc,
                    VatGroup: $scope.ListaItemDetalle[linea].CodigoVAT,
                    Rate: "",
                    WarehouseCode: $scope.ListaItemDetalle[linea].whsCode,
                    MetrosExtra: $scope.ListaItemDetalle[linea].MetrosExtra,
                    SalesPersonCode: -1,
                    LineStatus: "O",
                    TaxCode: "",
                    LineNum: $scope.ListaItemDetalle[linea].LineNum,
                    Delete: $scope.ListaItemDetalle[linea].Delete
                    //ShipDate: shipDate
                });
            }
            else {
                //DETALLE: agregamos al detalle del pedido
                $scope.JsonPedido.DocumentLines.push({
                    ItemCode: $scope.ListaItemDetalle[linea].Codigo,
                    ItemDescription: $scope.ListaItemDetalle[linea].Nombre,
                    Precio: $scope.ListaItemDetalle[linea].Precio,
                    DiasEntrega: $scope.ListaItemDetalle[linea].FechaEntrega,
                    PrecioLinea: $scope.ListaItemDetalle[linea].PrecioLinea,
                    PrecioUnitario: $scope.ListaItemDetalle[linea].PrecioUnitario,
                    Quantity: $scope.ListaItemDetalle[linea].Cantidad,
                    Currency: $scope.MonedaDelSN.Moneda,
                    PrtgDiscount: $scope.ListaItemDetalle[linea].PrtgeDisc,
                    VatGroup: $scope.ListaItemDetalle[linea].CodigoVAT,
                    Rate: "",
                    WarehouseCode: $scope.ListaItemDetalle[linea].whsCode,
                    MetrosExtra: $scope.ListaItemDetalle[linea].MetrosExtra,
                    SalesPersonCode: -1,
                    LineStatus: "O",
                    TaxCode: "",
                    LineNum: -1,
                    //ShipDate: shipDate
                });
            }
        }

        if ($scope.Base == "Pedido") {
            //descomponer fecha en formato aceptable
            var mesOrden = $scope.FechaSistema.getMonth() + 1;
            var diaOrden = $scope.FechaSistema.getDate();
            fechaDeOrden = $scope.FechaSistema.getFullYear() + '-' + (mesOrden < 10 ? '0' : '') + mesOrden + '-' + (diaOrden < 10 ? '0' : '') + diaOrden;
        }
        else {
            //descomponer fecha en formato aceptable
            var mesCot = $scope.FechaSistemaCot.getMonth() + 1;
            var diaCot = $scope.FechaSistemaCot.getDate();
            fechaCot = $scope.FechaSistemaCot.getFullYear() + '-' + (mesCot < 10 ? '0' : '') + mesCot + '-' + (diaCot < 10 ? '0' : '') + diaCot;
        }

        //ENCABEZADO: formando el json del pedido
        $scope.JsonPedido.DocNumber = $scope.DocEntryPedido;
        $scope.JsonPedido.DocDate = fechaDeOrden;
        $scope.JsonPedido.DocDueDate = fechaDeOrden;
        $scope.JsonPedido.DocDueDateCot = fechaCot;
        $scope.JsonPedido.CardCode = $scope.customerBP.CardCode;
        $scope.JsonPedido.CardName = $scope.customerBP.CardName;
        $scope.JsonPedido.Reference1 = "";
        $scope.JsonPedido.U_NumPed = $scope.NumPedido;
        $scope.JsonPedido.Comments = $scope.ComentariosPedido;
        $scope.JsonPedido.DocCurrency = "";
        $scope.JsonPedido.DocRate = "";
        $scope.JsonPedido.SalesPersonCode = -1;
        $scope.JsonPedido.ShipToCode = "";
        $scope.JsonPedido.Address = "";
        $scope.JsonPedido.JournalMemo = "Orden de Venta";
        $scope.JsonPedido.DocumentStatus = "O";//con DI API se pondra en automatico este valor en SAP
        $scope.JsonPedido.DocType = "I"; // se pondra en automatico este valor en SAP
        $scope.JsonPedido.Discount = $scope.customerBP.Descuento;
        $scope.JsonPedido.U_envio = $scope.EnvioTipo.Forma;
        console.log($scope.TipoEnvio.Forma);
        $scope.JsonPedido.U_Paquetería = $scope.TipoEnvio.Forma;
        console.log($scope.JsonPedido.U_Paquetería);
        $scope.JsonPedido.DocBase = $scope.Base;
        $scope.JsonPedido.MetodoPago = $scope.MetodoPago.Forma;
        $scope.JsonPedido.FormaPago = $scope.DatosFP.Forma;
        $scope.JsonPedido.CDFI = $scope.DatosCDFI.Forma;

    }

    //Llena el combo de monedas disponibles
    $scope.LlenarComboMoneda = function () {
        var selected = $filter('filter')($scope.Monedas, { value: $scope.MonedaDelSN.Moneda });
        return ($scope.MonedaDelSN.Moneda && selected.length) ? selected[0].text : 'Sin moneda';
    };

    $scope.LlenarComboEstadoPedido = function () {
        var selected = $filter('filter')($scope.EstadosPedido, { value: $scope.EstadoPedido.Tipo });
        return ($scope.EstadoPedido.Tipo && selected.length) ? selected[0].text : '-----'
    }

    $scope.EnviarOrdenASAP = function () {
        if (($scope.ListaItemDetalle).length <= 0) {
            growl.error("No ha cargado los Artículos del Pedido", { title: 'Sin Datos en el Detalle' });
            return;
        }

        if ($scope.DatosBP.DirCompleta == '' || $scope.DatosBP.DirCompleta == undefined || $scope.DatosBP.DirCompleta.length == 0) {
            growl.error("No ha cargado la dirección de entrega ", { title: 'Sin Datos de entrega' });
            return;
        }

        if ($scope.DatosBP.direccion == '' || $scope.DatosBP.direccion == undefined || $scope.DatosBP.direccion.length == 0) {
            growl.error("No ha cargado la dirección de entrega ", { title: 'Sin Datos de entrega' });
            return;
        }

        stockReal = $scope.ListaItemDetalle.indexOf(x => x.stockReal == 0);
        for (l = 0; l < $scope.ListaItemDetalle.length; l++) {
            var stockReal = 0;
            stockReal = parseFloat($scope.ListaItemDetalle[l].Cantidad) + parseFloat($scope.ListaItemDetalle[l].comprometido);
            stockReal = $scope.ListaItemDetalle[l].stock - stockReal
            $scope.ListaItemDetalle[l].stockReal = stockReal;

            if ($scope.ListaItemDetalle[l].stockReal < 0) {
                growl.warning("El articulo:" + $scope.ListaItemDetalle[l].Nombre + " No cuenta con stock suficiente.", { title: 'Sin Existencias' });
                return;
            }
        }

        if ($scope.Base == 'Pedido') {
            if ($scope.DatosFP.Forma == '' || $scope.DatosFP.Forma == undefined || $scope.DatosFP.Forma <= 0) {
                growl.error("Falta método de pago en SN", { title: 'Sin Forma de pago' });
                return;
            }

            if ($scope.EnvioTipo.Forma == '' || $scope.EnvioTipo.Forma == undefined || $scope.EnvioTipo.Forma <= 0) {
                growl.error("Falta Forma de Envío  ", { title: 'Sin Forma de Envío' });
                return;

            }

            if ($scope.TipoEnvio.Forma == '' || $scope.TipoEnvio.Forma == undefined || $scope.TipoEnvio.Forma <= 0) {
                growl.error("Falta Tipo de Envío  ", { title: 'Sin Tipo de Envío' });
                return;

            }

            if ($scope.MetodoPago.Forma == '' || $scope.MetodoPago.Forma == undefined || $scope.MetodoPago.Forma <= 0) {
                growl.error("Falta Método de Pago  ", { title: 'Sin Método de Pago' });
                return;
            }
        }

        $scope.FechaActual = new Date();
        var fechaActual = new Date();
        var yearActual = fechaActual.getFullYear();
        var mesActual = fechaActual.getMonth() + 1;
        var diaActual = fechaActual.getDate();
        var yearPedido;
        var mesPedido;
        var diaPedido;

        if (($scope.customerBP.CardCode == "" || $scope.customerBP.CardCode == undefined)) {
            growl.error('No ha completado el formulario.', { title: '¡Error!' });
            return;
        }

        if ($scope.rol === "C") {
            var totalPago = 0.0;
            var Apagar = 0.0;
            var Saldo = 0.0;
            var Credito = 0.0;
            Apagar = $scope.TotalAPagar;
            Saldo = $scope.customerBP.Saldo;
            Credito = $scope.customerBP.Credito;
            totalPago = Apagar + Saldo
            if (totalPago > Credito) {
                growl.warning("El Importe del Pedido creado rebasa su Limite de Crédito, un asesor se comunicará con ustedes", { title: '¡Aviso!' });
            }
        }

        //if ($scope.NumPedido == "") {
        //    growl.error('No se ha indicado el Número del Pedido.', { title: 'Error!' });
        //    return;
        //}		
        if ($scope.Base == "Pedido" && $scope.FechaSistema == "" || $scope.FechaSistema == null || $scope.FechaSistema == undefined) {
            growl.error('No se ha establecido la Fecha del Pedido.', { title: '¡Error!' });
        }
        else if ($scope.Base == "Cotizacion" && $scope.FechaSistemaCot == "" || $scope.FechaSistemaCot == null || $scope.FechaSistemaCot == undefined) {
            growl.error('No se ha establecido la Fecha de la Cotización.', { title: '¡Error!' });
        }
        else if ($scope.FechaAtrasada() == false) {
            if ($scope.Base == "Pedido") {
                growl.error('Fecha del Pedido no Válida', { title: '¡Error!' });
            }
            else {
                growl.error('Fecha de Cotización no Válida', { title: '¡Error!' });
            }
        }
        else if ($scope.FechaAtrasada() != false) {
            //growl.success("OK", { title: 'Orden de Venta Generada!' });
            $scope.ExtraerJsonDetalle();

            if ($scope.JsonPedido.DocumentLines.length <= 0) {
                growl.error('No ha completado el formulario.', { title: '¡Error!' });
                return;
            }
            var dialog = new Messi(
                "<script>var incremento = 0;var mySet = setInterval(setColor, 1000);setColor();function setColor() {" +
                "incremento++;" +
                "document.getElementById('counter').style.color='red';" +
                "document.getElementById('counter').innerHTML += '.' ;" +

                "}</script><div id='waitingMsg'><br/><br/><strong>¡Orden Enviada!</strong><br/><br/>Esperando Respuesta del Servidor <p id='counter'></p></div>",
                {
                    width: 400,
                    height: 150,
                    closeButton: false
                }
            );

            //var waitForResponse = setInterval(function () { growl.info("Esperando respuesta del Servidor... ", { title: 'Orden de Venta Enviada!', ttl: $scope.timeToLive }); }, 1000);
            $("#postOrderSAP").prop('disabled', true);
            $("#openBtnPreview").prop('disabled', true);

            $scope.JsonPedido.Address = $scope.DatosBP.direccion;
            if ($scope.JsonPedido.DocBase == "Cotizacion") {
                //Se manda a la fecha de vencimiento la fecha de vencimiento de Cotizacion | Valido Hasta
                $scope.JsonPedido.DocDueDate = $scope.JsonPedido.DocDueDateCot;
                $scope.JsonPedido.DocDate = $scope.JsonPedido.DocDueDateCot;
            }

            console.log(">> " + JSON.stringify($scope.JsonPedido));
            Servicio.IntegrarOrdenSAP(JSON.stringify($scope.JsonPedido)).then(function (respuestaDeIntegracion) {
                clearInterval(mySet);
                dialog.hide();

                if (respuestaDeIntegracion != undefined) {
                    $("#postOrderSAP").prop('disabled', false);
                    $("#openBtnPreview").prop('disabled', false);

                    if (respuestaDeIntegracion.codigo == 0) {
                        if ($scope.JsonPedido.DocBase === "Pedido") {
                            //clearInterval(waitForResponse);
                            growl.success("Número de Documento en SAP: " + respuestaDeIntegracion.mensaje, { title: '¡Orden de Venta Generada!' });
                            if ($scope.rol === "C") {
                                growl.info("Te seguimos atendiendo, si tienes alguna duda o aclaración en tu pedido: " + respuestaDeIntegracion.mensaje, { title: '¡Aviso!' });
                            }
                            //$scope.ReporteImpresionPedido(parseInt(respuestaDeIntegracion.tokenP), 'DocNum');
                            $scope.ReporteImpresionPedido(respuestaDeIntegracion.tokenP, 'DocNum');
                        }
                        else if ($scope.JsonPedido.DocBase === "Cotizacion") {
                            //clearInterval(waitForResponse);
                            growl.success("Número de Documento en SAP: " + respuestaDeIntegracion.mensaje, { title: '¡Oferta de Venta Generada!' });
                            //$scope.ReporteImpresionCotizacion(parseInt(respuestaDeIntegracion.tokenP), 'DocNum');
                            $scope.ReporteImpresionCotizacion(respuestaDeIntegracion.tokenP, 'DocNum');
                        }
                        //$scope.JsonPedido.DocumentLines = [];
                        $scope.Limpiar();
                    }
                    else {
                        //clearInterval(waitForResponse);
                        growl.error(respuestaDeIntegracion.mensaje, { title: '¡Error!' });
                        $scope.JsonPedido.DocumentLines = [];
                    }
                }
                else {

                    $("#postOrderSAP").prop('disabled', false);
                    $("#openBtnPreview").prop('disabled', false);
                    //clearInterval(waitForResponse);
                    growl.error("No se obtuvo respuesta de Integración", { title: '¡Error!' });
                    $scope.JsonPedido.DocumentLines = [];
                }
            });
        }
    };

    //#####################################################
    //Actualización de Cotizacion
    //#####################################################
    $scope.actualizarOrdenASAP = function () {

        if (($scope.ListaItemDetalle).length <= 0) {
            growl.error("No ha cargado los Artículos del Pedido", { title: 'Sin Datos en el Detalle' });
            return;
        }

        $scope.FechaActual = new Date();
        var fechaActual = new Date();
        var yearActual = fechaActual.getFullYear();
        var mesActual = fechaActual.getMonth() + 1;
        var diaActual = fechaActual.getDate();
        var yearPedido;
        var mesPedido;
        var diaPedido;

        if (($scope.customerBP.CardCode == "" || $scope.customerBP.CardCode == undefined || $scope.EnvioTipo.Forma == "")) {
            growl.error('No ha completado el formulario.', { title: '¡Error!' });
            return;
        }

        //if ($scope.NumPedido == "") {
        //    growl.error('No se ha indicado el Número del Pedido.', { title: 'Error!' });
        //    return;
        //}

        if ($scope.FechaSistemaCot == "" || $scope.FechaSistemaCot == null || $scope.FechaSistemaCot == undefined) {
            growl.error('No se ha establecido la Fecha de la Cotización.', { title: '¡Error!' });
        }
        else if ($scope.FechaAtrasada() == false) {
            growl.error('Fecha de Cotización no Válida', { title: '¡Error!' });
        }
        else if ($scope.FechaAtrasada() != false) {
            //growl.success("OK", { title: 'Orden de Venta Generada!' });
            $scope.ExtraerJsonDetalle();

            if ($scope.JsonPedido.DocumentLines.length <= 0) {
                growl.error('No ha completado el formulario.', { title: '¡Error!' });
                return;
            }

            if ($scope.Base == "Pedido" && $scope.FechaSistema == "" || $scope.FechaSistema == null || $scope.FechaSistema == undefined) {
                growl.error('No se ha establecido la Fecha del Pedido.', { title: '¡Error!' });
            }
            else if ($scope.Base == "Cotizacion" && $scope.FechaSistemaCot == "" || $scope.FechaSistemaCot == null || $scope.FechaSistemaCot == undefined) {
                growl.error('No se ha establecido la Fecha de la Cotización.', { title: '¡Error!' });
            }
            else if ($scope.FechaAtrasada() == false) {
                if ($scope.Base == "Pedido") {
                    growl.error('Fecha del Pedido no Válida', { title: '¡Error!' });
                }
                else {
                    growl.error('Fecha de Cotización no Válida', { title: '¡Error!' });
                }
            }
            else if ($scope.FechaAtrasada() != false)

                var dialog = new Messi(
                    "<script>var incremento = 0;var mySet = setInterval(setColor, 1000);setColor();function setColor() {" +
                    "incremento++;" +
                    "document.getElementById('counter').style.color='red';" +
                    "document.getElementById('counter').innerHTML += '.' ;" +

                    "}</script><div id='waitingMsg'><br/><br/><strong>¡Orden Enviada!</strong><br/><br/>Esperando Respuesta del Servidor <p id='counter'></p></div>",
                    {
                        width: 400,
                        height: 150,
                        closeButton: false
                    }
                );

            //var waitForResponse = setInterval(function () { growl.info("Esperando respuesta del Servidor... ", { title: 'Orden de Venta Enviada!', ttl: $scope.timeToLive }); }, 1000);
            $("#postOrderSAP").prop('disabled', true);
            $("#openBtnPreview").prop('disabled', true);

            $scope.JsonPedido.Address = $scope.DatosBP.direccion;
            if ($scope.JsonPedido.DocBase == "Cotizacion") {
                //Se manda a la fecha de vencimiento la fecha de vencimiento de Cotizacion | Valido Hasta
                $scope.JsonPedido.DocDueDate = $scope.JsonPedido.DocDueDateCot;
                $scope.JsonPedido.DocDate = $scope.JsonPedido.DocDueDateCot;
            }

            console.log(">> " + JSON.stringify($scope.JsonPedido));
            Servicio.actualizarOrdenSAP(JSON.stringify($scope.JsonPedido)).then(function (respuestaDeIntegracion) {
                clearInterval(mySet);
                dialog.hide();

                if (respuestaDeIntegracion != undefined) {

                    $("#postOrderSAP").prop('disabled', false);
                    $("#openBtnPreview").prop('disabled', false);

                    if (respuestaDeIntegracion.codigo == 0) {
                        if ($scope.JsonPedido.DocBase === "Pedido") {
                            //clearInterval(waitForResponse);
                            growl.success("Número de Documento en SAP: " + respuestaDeIntegracion.mensaje, { title: '¡Orden de Venta Generada!' });
                            $scope.ReporteImpresionPedido(parseInt(respuestaDeIntegracion.mensaje), 'DocNum');
                        }
                        else if ($scope.JsonPedido.DocBase === "Cotizacion") {
                            //clearInterval(waitForResponse);
                            growl.success("Número de Documento en SAP: " + respuestaDeIntegracion.mensaje, { title: '¡Oferta de Venta Generada!' });
                            $scope.ReporteImpresionCotizacion(parseInt(respuestaDeIntegracion.mensaje), 'DocNum');
                        }
                        //$scope.JsonPedido.DocumentLines = [];
                        $scope.Limpiar();
                    }
                    else {

                        //clearInterval(waitForResponse);
                        growl.error(respuestaDeIntegracion.mensaje, { title: '¡Error!' });
                        $scope.JsonPedido.DocumentLines = [];
                    }
                }
                else {
                    $("#postOrderSAP").prop('disabled', false);
                    $("#openBtnPreview").prop('disabled', false);
                    //clearInterval(waitForResponse);
                    growl.error("No se obtuvo respuesta de Integración", { title: '¡Error!' });
                    $scope.JsonPedido.DocumentLines = [];
                }
            });
        }
    };

    $scope.FechaAtrasada = function () {
        $scope.FechaActual = new Date();
        var fechaActual = new Date();
        var yearActual = fechaActual.getFullYear();
        var mesActual = fechaActual.getMonth() + 1;
        var diaActual = fechaActual.getDate();
        var fechaPedidoW;
        var yearPedido;
        var mesPedido;
        var diaPedido;

        if ($scope.Base == "Pedido") {
            if ($scope.FechaSistema != "" || $scope.FechaSistema != null || $scope.FechaSistema != undefined) {
                fechaPedidoW = new Date($scope.FechaSistema);
                yearPedido = $scope.FechaSistema.getFullYear();
                mesPedido = $scope.FechaSistema.getMonth() + 1;
                diaPedido = $scope.FechaSistema.getDate();
            }
        }
        else {
            if ($scope.FechaSistemaCot != "" || $scope.FechaSistemaCot != null || $scope.FechaSistemaCot != undefined) {
                fechaPedidoW = new Date($scope.FechaSistemaCot);
                yearPedido = $scope.FechaSistemaCot.getFullYear();
                mesPedido = $scope.FechaSistemaCot.getMonth() + 1;
                diaPedido = $scope.FechaSistemaCot.getDate();
            }
        }

        if (fechaPedidoW > fechaActual) {
            //growl.success("ok", { title: 'Demo' });
            return true;
        }
        else {
            //growl.error("falla", { title: 'Demo Error' });
            return false;
        }
    };

    //Impresion de Pedido creado
    $scope.ReporteImpresionPedido = function (docEntry, tipo) {
        // Primero: forzar la descarga
        var downloadForm = $('<form>', {
            method: 'POST',
            action: Ruta.path + '/IntegracionSap/ImprimirPedido',
            target: '_self' // descarga en la misma ventana
        });

        downloadForm.append($('<input>', { type: 'hidden', name: 'SO', value: docEntry }));
        downloadForm.append($('<input>', { type: 'hidden', name: 'T', value: tipo }));
        downloadForm.append($('<input>', { type: 'hidden', name: 'modo', value: 'attachment' }));

        // Luego: abrir en nueva pestaña (inline)
        var openForm = $('<form>', {
            method: 'POST',
            action: Ruta.path + '/IntegracionSap/ImprimirPedido',
            target: '_blank' // abre en nueva pestaña
        });

        openForm.append($('<input>', { type: 'hidden', name: 'SO', value: docEntry }));
        openForm.append($('<input>', { type: 'hidden', name: 'T', value: tipo }));
        openForm.append($('<input>', { type: 'hidden', name: 'modo', value: 'inline' }));

        // Agregar ambos formularios al DOM (invisibles)
        downloadForm.css('display', 'none').appendTo(document.body);
        openForm.css('display', 'none').appendTo(document.body);

        // Enviar el primer formulario (descarga)
        downloadForm.submit();

        // Usar setTimeout para dar tiempo a la descarga antes de abrir la pestaña
        setTimeout(function () {
            openForm.submit();
        }, 1500); // Espera 1.5 segundos antes de abrir el PDF
    };

    //Impresion de Cotizacion creada 
    $scope.ReporteImpresionCotizacion = function (docEntry, tipo) {
        // Primero: forzar la descarga
        var downloadForm = $('<form>', {
            method: 'POST',
            action: Ruta.path + '/IntegracionSap/ImprimirCotizacion',
            target: '_self' // descarga en la misma ventana
        });

        downloadForm.append($('<input>', { type: 'hidden', name: 'docEntrySAP', value: docEntry }));
        downloadForm.append($('<input>', { type: 'hidden', name: 'tipo', value: tipo }));
        downloadForm.append($('<input>', { type: 'hidden', name: 'modo', value: 'attachment' }));

        // Luego: abrir en nueva pestaña (inline)
        var openForm = $('<form>', {
            method: 'POST',
            action: Ruta.path + '/IntegracionSap/ImprimirCotizacion',
            target: '_blank' // abre en nueva pestaña
        });

        openForm.append($('<input>', { type: 'hidden', name: 'docEntrySAP', value: docEntry }));
        openForm.append($('<input>', { type: 'hidden', name: 'tipo', value: tipo }));
        openForm.append($('<input>', { type: 'hidden', name: 'modo', value: 'inline' }));

        // Agregar ambos formularios al DOM (invisibles)
        downloadForm.css('display', 'none').appendTo(document.body);
        openForm.css('display', 'none').appendTo(document.body);

        // Enviar el primer formulario (descarga)
        downloadForm.submit();

        // Usar setTimeout para dar tiempo a la descarga antes de abrir la pestaña
        setTimeout(function () {
            openForm.submit();
        }, 1500); // Espera 1.5 segundos antes de abrir el PDF
    };

    //*************************************************************************************************/
    $scope.Limpiar = function () {
        //VARIABLES
        $scope.accion = '1';
        $scope.TipoConsultaC = { Tipo: 'Todos' };
        $scope.ListaTipoConsultaC = [
            { value: 'Todos', text: 'Todos' },
            { value: 'DocNum', text: 'No. Documento' },
            { value: 'Articulo', text: 'Por Artículo' },
            { value: 'Fecha', text: 'Por Fecha' }
        ];

        $scope.ValorConsultaC = "";
        $scope.ListaCotizaciones = [];
        $scope.VisualizarFecha = false;
        $scope.QryConsultaCotizacion = "";
        $scope.QtyConsultaCotizacion = 8;

        $scope.rol = ''; //Obtene y almacena el rol del usuario | se ejecuta cada se recarga la pagina
        $scope.tipo = 'N';

        //*************************************************************************************************/
        ///Variables globales dentro del alcance de VistaPedio.cshtml///   
        //calculo de Fecha del SISTEMA
        var fecha = new Date();
        var mes = fecha.getMonth() + 1;
        var dia = fecha.getDate();
        var hora = fecha.getHours();

        //console.log(hora);
        //console.log(mes);

        var fechaCompleta = fecha.getFullYear() + '-' + (mes < 10 ? '0' : '') + mes + '-' + (dia < 10 ? '0' : '') + dia;
        var FechaMaxEnvio = "";
        $scope.searchTextPedido;
        $scope.MotivoCierreCotizacion = "";
        $scope.DocStatus = "";
        $scope.customerBP = {};//socio de negocios elejido desde modal
        $scope.itemOITM = {};//articulo elejido desde la modal
        $scope.WareHoseCode = "";//variable para el codigo de almacen
        $scope.ListaItemDetalle = [];
        $scope.Impuesto = 0;
        $scope.Descuento = 0;
        $scope.TotalAPagar = 0;
        $scope.SubttlAPagar = 0;
        $scope.TotalEnImpuesto = 0;
        $scope.TotalEnImpuestoStr = "";
        $scope.TotalUnidades = 0;
        $scope.TotalAPagarDisplay = 0;
        $scope.SbttlAPagarDisplay = "";
        $scope.FechaSistema = ""; //Fecha de entrega Pedido
        $scope.FechaSistemaCot = ""; //Fecha de vencimiento Cotizacion
        $scope.ComentariosPedido = "";
        $scope.MensajeDelSistema = "";
        $scope.MostrarMensaje = false;
        $scope.DirCotizacionCargada = "";
        $scope.rol = ''; //Obtene y almacena el rol del usuario | se ejecuta cada se recarga la pagina
        $scope.saldo = 0;
        $scope.credito = 0;
        $scope.saldoSt = 0;
        $scope.creditoSt = 0;
        $scope.statusMetodoPago = "";
        $scope.respuestaSN = [{ CardCode: "C0001", CardName: "Demo Customer", Credito: 0, DiscInRet: "N", Discount: 0, EstatusMetodPago: null, ListNum: 19, Moneda: "##" }, { CardCode: "C0001", CardName: "Demo Customer", Credito: 0, DiscInRet: "N", Discount: 0, EstatusMetodPago: null, ListNum: 13, Moneda: "MXP", Saldo: 0 }];

        //-------------------------------------------------------------
        $scope.Base = 'Cotizacion';
        $scope.DocBase = [{ Tipo: 'Pedido', Documento: 'Orden de Venta' }, { Tipo: 'Cotizacion', Documento: 'Cotización' }]; //Opciones de Tipo de Documento
        $scope.DocBaseEnvio = [{ Tipo: 'C', Documento: 'Completo' }, { Tipo: 'p', Documento: 'Parcial' }]; //Opciones de forma de envio
        $scope.DocBasePago = [{ Tipo: 'PUE', Documento: 'Pago en una exibición' }, { Tipo: 'PPD', Documento: 'Pago en parcialidades' }]; //Opciones de metodo de pago
        $scope.DocBaseTPago = [{ Tipo: '09', Documento: 'Efectivo' }, { Tipo: '03', Documento: 'Transferencia de fondos ' }]; //Opciones de Tipo de pago
        $scope.DocBaseFEnvio = [{ Tipo: 'O', Documento: 'Ocurrente' }, { Tipo: 'D', Documento: 'Domicilio' }]; //Opciones de forma de envio
        $scope.DocBaseCDFI = [{ Tipo: 'G01', Documento: 'Adquicisión de mercancias' }, { Tipo: 'G03', Documento: 'Gastos en general' }]; //Opciones de forma de envio
        $scope.DocEntryPedido = 0; //DocEntry del Documento Cotización
        //-------------------------------------------------------------

        //-------------------------------------------------------------
        //-------------------------------------------------------------
        $scope.FormaPago = ""; //Tipo de Envio aqui se guarda la variable
        $scope.DocFormaPago = [{ Tipo: '3', Documento: '03--Transferencia Electrónica de Fondos' }, { Tipo: '99', Documento: '99--Por Definir' }]; //Opciones de Tipo de Documento
        //-------------------------------------------------------------
        //DocEntry del Documento Cotización
        //-------------------------------------------------------------

        //ordenamiento
        $scope.sortType = 'name'; //tipo de ordenamiento default
        $scope.sortReverse = false;  // orden default
        $scope.NombreItem = '';

        // termindo de busqueda default
        //$scope.regexDecimalValido = true;
        $scope.ValidParmeter = '/\d+/';
        $scope.NumPedido = "";
        $scope.NumCotizacion = "";

        //monedas
        $scope.TasasDeCambio = [];//almacena las tasas de cambio
        $scope.VisualizarMoneda = false; //Indicador para mostrar combo moneda. Por defecto no se muestra
        $scope.MonedaDelSN = { Moneda: 'MXP' };//default pesos mexicanos
        $scope.Monedas = [
            { value: 'MXP', text: 'Pesos' },
            { value: 'USD', text: 'Dolares' },
            { value: 'EUR', text: 'Euros' },
            { value: 'GPB', text: 'Libra' }
        ];
        $scope.ImprimirDesde = new Date();
        $scope.ImprimirDesdeTxt = "";
        $scope.ImprimirHasta = new Date();
        $scope.ImprimirHastaTxt = "";
        $scope.DatosBP = {
            direccion: '',
            DirCompleta: '',
            ListaDireccion: [],
            ListaAlmacenes: []

        }

        //Datos forma de pago
        $scope.DatosFP = {
            Forma: '',
            Codigo: '',
            ListaFormaPago: [],
            statusMetodoPago: '0'
        }

        $scope.DatosCDFI = {
            Forma: '',
            Codigo: '',
            ListaCDFI: []
        }

        $scope.EnvioTipo = {
            Forma: '',
            Codigo: '',
            ListaFormaEnvio: []
        }

        $scope.TipoEnvio = {
            Forma: '',
            Codigo: '',
            ListaTipoEnvio: []
        }

        $scope.MetodoPago = {
            Forma: '',
            Codigo: '',
            ListaMetodoPago: []
        }

        $scope.DatosAlmacen = {
            almacen: '',
            ListaAlmacenes: []

        }

        $scope.Timbrado = 1,
            $scope.radioOptions = [
                { name: '  Si  ', value: 1, checked: 'false' },
                { name: '  No ', value: 0, checked: 'false' },
            ]

        //json para el Pedido
        $scope.JsonPedido = {
            DocDate: "",
            DocDueDate: "",
            U_NumPed: "",
            CardCode: "",
            CardName: "",
            Reference1: "",
            Comments: "",
            DocCurrency: "",
            DocRate: "",
            DocRate: "",
            SalesPersonCode: "",
            ShipToCode: "",
            Address: "",
            JournalMemo: "",
            DocumentStatus: "",
            DocType: "",
            Discount: "",
            DocumentLines: [],
            DocBase: "", //Tipo de documento: Orden de Venta | Cotización
            DocNumber: "",
            MotivoCierreCotizacion: "",
            DocDueDateCot: "" //Fecha Vencimiento Cotizacion

        };

        $scope.EstadoPedido = 'O';
        $scope.EstadoPedido = { Tipo: 'O' };//default 
        $scope.EstadosPedido = [
            { value: 'O', text: 'Abiertos' },
            { value: 'P', text: 'Cerrados' }
        ];

        $scope.timeToLive = 0;

        $scope.InicialMaster();
        $scope.ObtenerTsaDeCambio();
        $scope.ObtenerAlmacen();
    };

    //*************************************************************************************************/
    $scope.MensajesDeUsuario = function (type, message) {
        var config = {};
        switch (type) {
            case "success":
                growl.success(message, { title: "¡Éxito!" });
                break;
            case "info":
                growl.info(message, { title: "¡Informativo!", ttl: 10000 });
                break;
            case "warning":
                growl.warning(message, { title: "¡Atención!" });
                break;
            case "error":
                growl.error(message, { title: "¡Error!" });
                break;
        }
    };
    //*************************************************************************************************/
    //calcular credito cliente  
    //Funciones Iniciales
    $scope.InicialMaster();
    $scope.ObtenerTsaDeCambio();
    $scope.ObtenerAlmacen();

    $scope.CargarDirecciones();
    $scope.CargarInputCDFI();
    $scope.CargarInputFP();
    $scope.CargarFormaEnvio();
    $scope.CargarMetodoPago();
    $scope.CargarTipoEnvio();
});
//////::::::::::FIN:::::::::::SECCION CONSULTA COTIZACIONES ::::::::::::::::::::::::::::

//se obtiene el id para la exportación
var g = Date.now().toString(36) + Math.random().toString(36).substr(2);
//se imprime el valor de la variable id descomentar para pruebas de exportar etiquetas leidas
console.log(g);

function formatThousandsWithRounding(cantidad, decimales) {
    var w = cantidad.toFixed(decimales), k = w | 0, b = cantidad < 0 ? 1 : 0,
        u = Math.abs(w - k), d = ('' + u.toFixed(decimales)).substr(2, decimales),
        s = '' + k, i = s.length, r = '';
    while ((i -= 3) > b) { r = ',' + s.substr(i, 3) + r; }
    return s.substr(0, i + 3) + r + (d ? '.' + d : '');
};

//Directiva para input calendario
appCotizacion.directive('jqdatepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            element.datepicker({
                dateFormat: "yyyy/mm/dd",
                onSelect: function (date) {
                    scope.date = date;
                    scope.$apply();
                }
            });
        }
    };
});

