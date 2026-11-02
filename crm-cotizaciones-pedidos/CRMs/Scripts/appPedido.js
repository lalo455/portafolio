var appPedidos = angular.module('appPedidos', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

//https://ciphertrick.com/2015/06/01/search-sort-and-pagination-ngrepeat-angularjs/
//https://fritz.ninja/angular-growl-2/
appPedidos.factory('Ruta', function () {
    return { path: '' }//AQUI VA ALGO carpeta de proyecto en IIS
});

appPedidos.config(['growlProvider', function (growlProvider) {

    growlProvider.globalPosition('bottom-center');
    //growlProvider.globalReversedOrder(true);
    growlProvider.globalTimeToLive({ success: 20000, error: 10000, warning: 10000 })
}]);


appPedidos.run(['editableOptions', 'editableThemes', function (editableOptions, editableThemes) {
    editableOptions.theme = 'bs3';
    editableThemes.bs3.dateTpl = '<input type="text" class="form-control input-sm" uib-datepicker-popup="yyyy-MM-dd">';
}]);

appPedidos.service('Servicio', function ($http, Ruta, $log) {
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

//////::::::::::INICIO:::::::::::SECCION CONSULTA PEDIDO ::::::::::::::::::::::::::::
appPedidos.controller('consultaPedidoController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {
    //VARIABLES
    $scope.TipoConsulta = { Tipo: 'Todos' };
    $scope.ListaTipoConsulta = [
        { value: 'Todos', text: 'Todos' },
        { value: 'DocNum', text: 'No. Documento' },
        { value: 'Articulo', text: 'Por Artículo' },
        { value: 'Fecha', text: 'Por Fecha' }
    ];

    $scope.ValorConsulta = "";
    $scope.ListaPedidos = [];
    $scope.VisualizarFecha = false;
    $scope.QryConsultaPedido = "";
    $scope.QtyConsultaPedido = 8;
    $scope.rol = ''; //Obtene y almacena el rol del usuario | se ejecuta cada se recarga la pagina
    $scope.NumeroDocumento = 0;

    $scope.ImprimirEstadoPedido = function (docnum, user) {
        var UrlString = Ruta.path + '/IntegracionSap/ImprimirEstadoPedido?DocNum=' + docnum + '&Usuario=' + user;
        $("#pdf_preview_e_p").attr('src', UrlString);
        modalEstadoPedido.style.display = "block";
    }

    //::::::ZONA MODAL DE ESTADO PEDIDO::::::::://
    $scope.AbrirModalEstadoPedido = function (docNum, user) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + "/Modales/EstadoPedido.html",
            controller: EstadoPedidoModalController,//Recibe ITEMList, es el nombre de su parametro.
            keyboard: false,
            backdrop: "static",
            resolve: {
                URLParameters: function () {
                    return { Document: docNum, User: user };//Este es el parametro que recibo en la funcion AbrirModalArticulos
                }
            }
        }).result.then(function (result) { });
    }

    var EstadoPedidoModalController = function (Ruta, $scope, $uibModalInstance, $log, URLParameters) {
        $scope.SrcParameters = URLParameters;
        $scope.UrlString = Ruta.path + '/IntegracionSap/ImprimirEstadoPedido?DocNum=' + $scope.SrcParameters.Document;

        $scope.ok = function () {
            $uibModalInstance.close($scope.ItemSeleccion);//cuando se cierre regresa estos datos
        }
    }
    //::::::FIN DE MODAL ESTADO PEDIDO:::::::::://

    //::::::ZONA MODAL DE ESTADO Cotizacion::::::::://

    $scope.AbrirModalEstadoCotizacion = function () {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + "/Modales/EstadoPedido.html",
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
    //FUNCIONES
    $scope.VisualizarTipoConsulta = function () {
        var selected = $filter('filter')($scope.ListaTipoConsulta, { value: $scope.TipoConsulta.Tipo });
        return ($scope.TipoConsulta.Tipo && selected.length) ? selected[0].text : 'Sin Tipo de Consulta';
    }

    $scope.ConsultaPedidos = function () {
        $scope.ListaPedidos = [];

        if ($scope.ValorConsulta == "" || $scope.ValorConsulta == undefined) {
            $scope.ValorConsulta = "Todos";
        }

        if ($scope.TipoConsulta.Tipo == 'Fecha') {
            $scope.ParametrosConsulta = {
                DocNum: -1,
                UserId: '',
                Tipo: $scope.TipoConsulta.Tipo,
                Fecha: ($scope.ValorConsulta).toISOString().substring(0, 10),
                Articulo: ''
            }
        }
        else {
            $scope.ParametrosConsulta = {
                DocNum: $scope.TipoConsulta.Tipo != 'DocNum' ? -1 : $scope.ValorConsulta,
                UserId: '',
                Tipo: $scope.TipoConsulta.Tipo,
                Fecha: $scope.ValorConsulta,
                Articulo: $scope.ValorConsulta
            }
        }

        Servicio.ConsultaPedido($scope.ParametrosConsulta.DocNum, $scope.ParametrosConsulta.UserId, $scope.ParametrosConsulta.Tipo, $scope.ParametrosConsulta.Fecha, $scope.ParametrosConsulta.Articulo).then(function (respuesta) {
            $scope.ValorConsulta = "";

            if (respuesta != undefined && respuesta[0].NoPedido == -33) {
                growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
                return;
            }

            if (respuesta != undefined && respuesta.length > 0) {
                $scope.ValorConsulta = "";
                angular.forEach(respuesta, function (resp) {
                    $scope.ListaPedidos.push({
                        NoPedido: resp.NoPedido,
                        DocDate: resp.DocDate,
                        DocDueDate: resp.DocDueDate,
                        Cliente: resp.Cliente,
                        ItemCode: resp.ItemCode,
                        ItemName: resp.ItemName,
                        QtyPedida: resp.QtyPedida,
                        QtyPendiente: resp.QtyPendiente,
                        QtyEntregada: resp.QtyEntregada,
                        QtyFacturada: resp.QtyFacturada,
                        UMVendida: resp.UMVendida,
                        Precio: resp.Precio,
                        Total: resp.Total,
                        DocFactura: resp.DocFactura,
                        FechaCont: resp.FechaCont,
                        Guia: resp.Guia,
                        Estado: resp.Estado
                    });
                });

                $scope.ValorConsulta = "";
            }
            else {
                $scope.ValorConsulta = "";
                $scope.MensajesDeUsuario('info', "¡No se recupero ninguna información con los parametros especificados!");
            }
        });
    }

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

    //----------------------------------------------------------------------------------------------------
    $scope.InicialMaster = function () {
        console.log("Datos iniciales en consultaPedidoController************");
        //growl.warning("Carga Inicial", { title: 'Atención!' });
        Servicio.DatosIniciales().then(function (respuesta) {
            if (respuesta != null) {
                $scope.rol = respuesta;
            }
            else {
                console.log('SIN Datos iniciales');
            }
        });
    }
    //----------------------------------------------------------------------------------------------------
    //Funciones Iniciales
    $scope.InicialMaster();
});
//////::::::::::FIN:::::::::::SECCION CONSULTA PEDIDO ::::::::::::::::::::::::::::

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
appPedidos.directive('jqdatepicker', function () {
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