var appVentas = angular.module('appVentas', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

//https://ciphertrick.com/2015/06/01/search-sort-and-pagination-ngrepeat-angularjs/
//https://fritz.ninja/angular-growl-2/
appVentas.factory('Ruta', function () {
    return { path: '' }//AQUI VA ALGO carpeta de proyecto en IIS
});

appVentas.config(['growlProvider', function (growlProvider) {

    growlProvider.globalPosition('bottom-center');
    //growlProvider.globalReversedOrder(true);
    growlProvider.globalTimeToLive({ success: 20000, error: 10000, warning: 10000 })
}]);


appVentas.run(['editableOptions', 'editableThemes', function (editableOptions, editableThemes) {
    editableOptions.theme = 'bs3';
    editableThemes.bs3.dateTpl = '<input type="text" class="form-control input-sm" uib-datepicker-popup="yyyy-MM-dd">';
}]);

appVentas.service('Servicio', function ($http, Ruta, $log) {
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

//////::::::::::INICIO:::::::::::SECCION CONSULTA FACTURAS POR USUARIO ::::::::::::::::::::::::::::
appVentas.controller('consultarTotalVentasController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {
    //VARIABLES
    $scope.FechaActual = new Date();
    var fechaActual = new Date();
    var yearActual = fechaActual.getFullYear();
    var mesActual = fechaActual.getMonth() + 1;
    $scope.ValorConsulta = "";
    $scope.ListaFacturasVisualizacion = [];
    $scope.ListaFacturasPorMes = [];
    $scope.ListaFacturasPorAnio = [];
    $scope.NumeroDocumento = 0;
    $scope.TotalMes = 0;
    $scope.TotalAnio = 0;
    $scope.QryConsultaVentas = "";
    $scope.QtyConsultaVentas = 10;
    $scope.TotalMesPorcentaje = 0;
    $scope.TotalAnioPorcentaje = 0;
    $scope.rol = "";

    //::::::FIN DE MODAL ESTADO PEDIDO:::::::::://
    //FUNCIONES
    $scope.visualizarVentasPorMes = function () {
        $scope.ListaFacturasVisualizacion = $scope.ListaFacturasPorMes;
    }

    $scope.visualizarVentasPorAnio = function () {
        $scope.ListaFacturasVisualizacion = $scope.ListaFacturasPorAnio;
    }

    $scope.consultarFacturasUsuario = function () {
        Servicio.ConsultaVentasDeUsuario().then(function (respuesta) {
            console.log(respuesta);
            $scope.ValorConsulta = "";

            if (respuesta != undefined) { //&& respuesta.VentasMes.length > 0) {
                $scope.ListaFacturasPorAnio = respuesta.VentasAnio;
                $scope.ListaFacturasPorMes = respuesta.VentasMes;

                angular.forEach($scope.ListaFacturasPorAnio, function (factura) {
                    $scope.TotalAnio += factura.ImporteTotal;
                });

                angular.forEach($scope.ListaFacturasPorMes, function (factura) {
                    $scope.TotalMes += factura.ImporteTotal;
                });

                $scope.ValorConsulta = "";
                $scope.visualizarPocerntaje($scope.TotalAnio, $scope.TotalMes);
            }
            else {
                //($scope.visualizarPocerntaje(55000,800000);               
                $scope.ValorConsulta = "";
                //$scope.MensajesDeUsuario('info', "No se recupero ninguna información!");
                console.log('info', "No se recupero ninguna información de factura del vendedor en este año y mes");
            }
        });
    }

    //funcion para saber el porcentaje de ventas por mes
    $scope.visualizarPocerntaje = function (Mes, Anio) {
        Servicio.ObtenerPorcentaje().then(function (PorcentajeVenta) {
            var Pmes = 0;
            var Panio = 0;

            console.log("Porcentaje ventas: " + PorcentajeVenta);
            if (PorcentajeVenta != undefined) {
                if (PorcentajeVenta == 0) {
                    $scope.TotalMesPorcentaje = Pmes.toFixed(2);
                    $scope.TotalAnioPorcentaje = Panio.toFixed(2);
                }
                else {
                    Pmes = (Mes / PorcentajeVenta[0].PorcentajeVenta) * 100;
                    $scope.TotalMesPorcentaje = Pmes.toFixed(2);
                    Panio = (Anio / PorcentajeVenta[1].PorcentajeVenta) * 100;
                    $scope.TotalAnioPorcentaje = Panio.toFixed(2);
                }
            }
            else {
                $scope.TotalMesPorcentaje = Pmes.toFixed(2);
                $scope.TotalAnioPorcentaje = Panio.toFixed(2);
            }

            console.log($scope.TotalMesPorcentaje);
            console.log($scope.TotalAnioPorcentaje);
        });
    }
    //funcion para saber el porcentaje de ventas por año

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
});
//////::::::::::FIN:::::::::::SECCION CONSULTA FACTURAS POR USUARIO::::::::::::::::::::::::::::

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
appVentas.directive('jqdatepicker', function () {
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

