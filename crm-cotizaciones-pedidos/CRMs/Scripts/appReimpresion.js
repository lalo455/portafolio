var appReimpresion = angular.module('appReimpresion', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

//https://ciphertrick.com/2015/06/01/search-sort-and-pagination-ngrepeat-angularjs/
//https://fritz.ninja/angular-growl-2/
appReimpresion.factory('Ruta', function () {
    return { path: '' }//AQUI VA ALGO carpeta de proyecto en IIS
});

appReimpresion.config(['growlProvider', function (growlProvider) {

    growlProvider.globalPosition('bottom-center');
    //growlProvider.globalReversedOrder(true);
    growlProvider.globalTimeToLive({ success: 20000, error: 10000, warning: 10000 })
}]);


appReimpresion.run(['editableOptions', 'editableThemes', function (editableOptions, editableThemes) {
    editableOptions.theme = 'bs3';
    editableThemes.bs3.dateTpl = '<input type="text" class="form-control input-sm" uib-datepicker-popup="yyyy-MM-dd">';
}]);

appReimpresion.service('Servicio', function ($http, Ruta, $log) {
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

//////::::::::::INICIO:::::::::::SECCION REIMPRESIÓN PEDIDO ::::::::::::::::::::::::::::
appReimpresion.controller('reimpresionController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {
    //VARIABLES
    var regexInteger = /^[0-9]*$/g;
    $scope.TipoDocumento = { Tipo: 'cotizacion' };
    $scope.ListaTipoDocumento = [
        { value: 'order', text: 'Pedido' },
        { value: 'cotizacion', text: 'Cotización' }
        /*     { value: 'quot', text: '-----' }*/
    ];

    $scope.TipoNumero = { Tipo: 'DocNum' };
    $scope.ListaTipoNumero = [
        { value: 'DocNum', text: 'Núm. Documento' },
        { value: 'Folio', text: 'Folio' }
    ];

    $scope.rol = "";
    $scope.customerBP = {};//socio de negocios elejido desde modal

    //FUNCIONES
    $scope.InicialMaster = function () {
        console.log("Datos iniciales en pedidoController************");
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
        console.log("Tipo cotizacion (pedidoController): " + $scope.tipo);
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
                        CardName: respuestaSN.CardName
                    }
                }
            }
        });
    }

    $scope.VisualizarTipoDocumento = function () {
        var selected = $filter('filter')($scope.ListaTipoDocumento, { value: $scope.TipoDocumento.Tipo });
        return ($scope.TipoDocumento.Tipo && selected.length) ? selected[0].text : 'Sin Tipo de Consulta';
    }

    $scope.VisualizarTipoNumero = function () {
        var selected = $filter('filter')($scope.ListaTipoNumero, { value: $scope.TipoNumero.Tipo });
        return ($scope.TipoNumero.Tipo && selected.length) ? selected[0].text : '------'
    };

    $scope.ReimpresionDocumentos = function (rol, cliente) {
        Servicio.VerificarSesion().then(function (sesion) {
            if (sesion != true) {
                growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
            }
            else {
                if ($scope.ValorDocumento != undefined && $scope.ValorDocumento != "") {
                    //var isNumero = angular.isNumber($scope.ValorDocumento);
                    console.log($scope.TipoDocumento.Tipo);
                    console.log("Reimpresion => ROL: " + rol + " | Cliente: " + cliente);

                    //Previo a generar reimpresión, validar si pedido o cotización pertenece a cliente | solo para rol cliente, asistente, consultor
                    if (rol === "V") {
                        switch ($scope.TipoDocumento.Tipo) {
                            case "order":
                                var url = Ruta.path + '/IntegracionSap/ReimprimePedido?docEntrySAP=' + $scope.ValorDocumento + "&tipo=" + $scope.TipoNumero.Tipo;
                                fetch(url, { method: 'HEAD' })
                                    .then(response => {
                                        var contentLength = response.headers.get("content-length");
                                        if (contentLength && parseInt(contentLength) > 50688) { // Ajusta este valor según el tamaño esperado de un PDF válido
                                            $('<form>', {
                                                "method": "POST",
                                                "action": url,
                                                "target": "_blank"
                                            }).appendTo(document.body).submit();
                                        }
                                        else {
                                            alert("No existe documento relacionado con el cliente actual.");
                                        }
                                    })
                                    .catch(error => {
                                        console.error("Error al verificar el reporte:", error);
                                    });
                                break;

                            case "cotizacion":
                                var url = Ruta.path + '/IntegracionSap/ReimprimeCotizacion?docEntrySAP=' + $scope.ValorDocumento + "&tipo=" + $scope.TipoNumero.Tipo;

                                fetch(url, { method: 'HEAD' })
                                    .then(response => {
                                        var contentLength = response.headers.get("content-length");
                                        if (contentLength && parseInt(contentLength) > 54784) { // Ajusta este valor según el tamaño esperado de un PDF válido
                                            $('<form>', {
                                                "method": "POST",
                                                "action": url,
                                                "target": "_blank"
                                            }).appendTo(document.body).submit();
                                        }
                                        else {
                                            alert("No existe documento relacionado con el cliente actual.");
                                        }
                                    })
                                    .catch(error => {
                                        console.error("Error al verificar el reporte:", error);
                                    });
                                break;
                        }
                    }
                    else {
                        Servicio.ValidarDocCliente($scope.TipoDocumento.Tipo, $scope.ValorDocumento, $scope.TipoNumero.Tipo, cliente).then(function (respuesta) {
                            console.log(respuesta);
                            switch (respuesta.codigo) {
                                case 0:
                                    switch ($scope.TipoDocumento.Tipo) {
                                        case "order":
                                            $scope.ReimprimirPedido(respuesta.tokenP, $scope.TipoNumero.Tipo);
                                            break;

                                        case "cotizacion":
                                            $scope.ReimprimirCotizacion(respuesta.tokenP, $scope.TipoNumero.Tipo);
                                            break;
                                    }
                                    break;

                                case -1:
                                    alert(respuesta.mensaje);
                                    break;

                                case -2:
                                    alert("No existe documento relacionado con el cliente.");
                                    break;
                            }
                        });
                    }
                }
                else {
                    growl.error("Introduzca un Número de Documento o Folio Válido", { title: 'Dato no Válido' });
                }
            }
        });
    };

    $scope.ReimprimirPedido = function (docEntry, tipo) {
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

    $scope.ReimprimirCotizacion = function (docEntry, tipo) {
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

    //Funciones Iniciales
    $scope.InicialMaster();
});
//////::::::::::FIN:::::::::::SECCION REIMPRESIÓN PEDIDO ::::::::::::::::::::::::::::

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
appReimpresion.directive('jqdatepicker', function () {
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