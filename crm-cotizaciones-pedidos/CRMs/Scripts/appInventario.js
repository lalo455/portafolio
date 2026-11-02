var appInventario = angular.module('appInventario', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

//https://ciphertrick.com/2015/06/01/search-sort-and-pagination-ngrepeat-angularjs/
//https://fritz.ninja/angular-growl-2/
appInventario.factory('Ruta', function () {
    return { path: '' }//AQUI VA ALGO carpeta de proyecto en IIS
});

appInventario.config(['growlProvider', function (growlProvider) {

    growlProvider.globalPosition('bottom-center');
    //growlProvider.globalReversedOrder(true);
    growlProvider.globalTimeToLive({ success: 20000, error: 10000, warning: 10000 })
}]);


appInventario.run(['editableOptions', 'editableThemes', function (editableOptions, editableThemes) {
    editableOptions.theme = 'bs3';
    editableThemes.bs3.dateTpl = '<input type="text" class="form-control input-sm" uib-datepicker-popup="yyyy-MM-dd">';
}]);

appInventario.service('Servicio', function ($http, Ruta, $log) {
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

//////::::::::::INICIO:::::::::::SECCION INVENTARIO POR ARTCULO ::::::::::::::::::::::::::::
appInventario.controller('inventarioPorArticuloController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {
    //VARIABLES PARA Inventario por articulo
    $scope.ArticuloDeInventario = {};
    $scope.ListaArticuloDeInventario = [];
    $scope.TotalWhsForItemInvt = 0;
    $scope.QtyWhsByPage = 9;
    $scope.rol = "";

    ///:::::::::::region de funciones para reporte:::::::::::
    $scope.InicialMaster = function () {
        console.log("Datos iniciales en pedidoController************");
        //growl.warning("Carga Inicial", { title: 'Atención!' });
        Servicio.DatosIniciales().then(function (respuesta) {
            if (respuesta != null) {
                $scope.rol = respuesta;

                //if ($scope.rol != 'V') {
                //    $scope.mostrarModalCliente();
                //}
            }
            else {
                console.log('SIN Datos iniciales');
            }
        });
        console.log("Tipo cotizacion (pedidoController): " + $scope.tipo);
    }

    $scope.ConsultarArticulo = function (itemCode, bd) {
        if ($scope.ArticuloDeInventario.ItemCode != "" && $scope.ArticuloDeInventario.ItemCode != undefined) {
            $scope.ListaArticuloDeInventario = [];
            Servicio.ConsultarInventario(0, '', '', itemCode, $scope.rol, bd).then(function (respInvent) {
                if (respInvent != undefined && respInvent.length != 0) {
                    angular.forEach(respInvent, function (item) {
                        $scope.ListaArticuloDeInventario.push({
                            WhsCode: item.WhsCode,
                            WhsName: item.WhsName,
                            OnHandQty: item.OnHandQty,
                            IsCommited: item.IsCommited,
                            OnOrder: item.OnOrder,
                            Available: item.Available,
                            SalUnitMsr: item.SalUnitMsr,
                            InvntryUom: item.InvntryUom,
                            Ubicacion: item.Ubicacion
                        });
                    });
                    $scope.TotalWhsForItemInvt = $scope.ListaArticuloDeInventario.length > 0 ? $scope.ListaArticuloDeInventario.length : 0;
                }
            });
        }
        else {
            $scope.MensajesDeUsuario('info', "Introduzca o Seleccione un código de Artículo.");
        }
    }
    ///::::::: fin de region de funcines para reportes::::::

    ///:::::REGION MODAL PARA ARTICULOS  ::::::::::////

    $scope.MostrarModalItemInvent = function (itemCode, itemName, bd) {
        if ((itemCode == '' || itemCode == undefined) && (itemName == '' || itemName == undefined)) {
            growl.warning("Debe ingresar código o nombre del artículo ", { title: '¡Advertencia!' });
            return;
        }
        else {
            if (itemCode == undefined) {
                itemCode = '';
            }

            if (itemName == undefined) {
                itemName = '';
            }

            if ((itemName.length > 0 && itemName.length < 3)) {
                growl.warning("El Nombre del artículo debe ser igual o mayor a 3 dígitos ", { title: '¡Advertencia!' });
                return;
            }

            if ((itemCode.length > 0 && itemCode.length < 3)) {
                growl.warning("El codigo del artículo debe ser igual o mayor a 3 dígitos ", { title: '¡Advertencia!' });
                return;
            }
        }

        if ($scope.rol == "C") {
            Servicio.ObtenerArticulosdeSAP2('', $scope.rol, itemCode, itemName, bd).then(function (respInvent) {
                if (respInvent != undefined) {
                    //llamamos a la modal y mandamos valores: lista de Arítuclos
                    console.log(respInvent);
                    setTimeout($scope.AbrirModalArticulosInventario(respInvent), 7000);
                    console.log(respInvent);
                    console.log(respInvent);
                }
            });
        }
        else {
            Servicio.ObtenerArticulosdeSAP2('', $scope.rol, itemCode, itemName, bd).then(function (respInvent) {
                if (respInvent != undefined) {
                    //llamamos a la modal y mandamos valores: lista de Arítuclos
                    console.log(respInvent);
                    setTimeout($scope.AbrirModalArticulosInventario(respInvent), 7000);
                    console.log(respInvent);
                }
            });
        }

        $scope.AbrirModalArticulosInventario = function (listaDeArticulos) {
            console.log(listaDeArticulos);
            var modalInstance = $uibModal.open({
                templateUrl: Ruta.path + "/Modales/ListaItemInventario.html",
                controller: ListaItemInventarioController,//Recibe ITEMList, es el nombre de su parametro.
                keyboard: false,
                backdrop: "static",
                resolve: {
                    ITEMListInvt: function () {
                        return listaDeArticulos;//Este es el parametro que recibo en la funcion AbrirModalArticulos
                    }
                }
            }).result.then(function (result) {
                console.log(result);
                //cacho los valores seleccionados de la modal
                //$scope.LlenarDetallePedido(result);
                $scope.ArticuloDeInventario = {
                    ItemCode: result.ItemCode,
                    ItemName: result.ItemName
                }
            });
        }
    }

    var ListaItemInventarioController = function ($scope, $uibModalInstance, $log, ITEMListInvt) {
        $scope.QueryBusqueda = "";
        $scope.QtyPageView = 10;
        $scope.ok = function () {
            $uibModalInstance.close($scope.ItemSeleccion);
        }

        //ListaITEM sera alcanzable solo en la modal
        $scope.ListaITEMInvt = ITEMListInvt;//llena la tabla de la modal

        $scope.ItemSeleccionado = function (item) {
            $scope.ItemSeleccion = item;
            $scope.ok();
        }
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

    //Funciones Iniciales
    $scope.InicialMaster();
});
//////::::::::::FIN:::::::::::SECCION INVENTARIO POR ARTCULO ::::::::::::::::::::::::::::

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
appInventario.directive('jqdatepicker', function () {
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