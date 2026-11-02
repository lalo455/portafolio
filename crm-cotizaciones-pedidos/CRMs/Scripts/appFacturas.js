var appFacturas = angular.module('appFacturas', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

//https://ciphertrick.com/2015/06/01/search-sort-and-pagination-ngrepeat-angularjs/
//https://fritz.ninja/angular-growl-2/
appFacturas.factory('Ruta', function () {
    return { path: '' }//AQUI VA ALGO carpeta de proyecto en IIS
});

appFacturas.config(['growlProvider', function (growlProvider) {

    growlProvider.globalPosition('bottom-center');
    //growlProvider.globalReversedOrder(true);
    growlProvider.globalTimeToLive({ success: 20000, error: 10000, warning: 10000 })
}]);


appFacturas.run(['editableOptions', 'editableThemes', function (editableOptions, editableThemes) {
    editableOptions.theme = 'bs3';
    editableThemes.bs3.dateTpl = '<input type="text" class="form-control input-sm" uib-datepicker-popup="yyyy-MM-dd">';
}]);

appFacturas.service('Servicio', function ($http, Ruta, $log) {
    return {

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

        ListarAniosFactura: function (UserId) {

            return $http.post(Ruta.path + "/LogicaPedido/ListarAniosFactura", { UserId: UserId }).then(function (respuestaConsulta) {
                return respuestaConsulta.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al consultar los años facturas" }
                };
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

//////::::::::::INICIO:::::::::::SECCION CONSULTA FACTURA ::::::::::::::::::::::::::::
appFacturas.controller('consultaFacturaController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {
    //VARIABLES
    $scope.customerBP = {};//socio de negocios elejido desde modal
    $scope.TipoConsulta = { Tipo: 'Todos' };
    $scope.ListaTipoConsulta = [
        { value: 'Todos', text: 'Todos' },
        { value: 'NumFactura', text: 'No. Factura' },

    ];
    //ordenar 
    $scope.propertyName = 'DocFactura';
    $scope.reverse = true;
    $scope.friends = $scope.ListaFctura;

    $scope.sortBy = function (propertyName) {
        $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
        $scope.propertyName = propertyName;
    };

    $scope.years = [];
    $scope.year = new Date().getFullYear();
    $scope.months = [{ value: '01', text: 'Enero' }, { value: '02', text: 'Febrero' }, { value: '03', text: 'Marzo' }, { value: '04', text: 'Abril' }, { value: '05', text: 'Mayo' }, { value: '06', text: 'Junio' }, { value: '07', text: 'Julio' }, { value: '08', text: 'Agosto' }, { value: '09', text: 'Septiembre' }, { value: '10', text: 'Octubre' }, { value: '11', text: 'Noviembre' }, { value: '12', text: 'Diciembre' }]
    $scope.month = '01';

    for (var i = 0; i < 15; i++) {
        $scope.years.push($scope.year - i);
    }

    $scope.ValorConsulta = "";
    $scope.ListaFctura = [];
    $scope.VisualizarFecha = false;
    $scope.QryConsultaFacturas = "";
    $scope.QtyConsultaFacturas = 8;
    $scope.rol = ''; //Obtene y almacena el rol del usuario | se ejecuta cada se recarga la pagina
    $scope.NumeroDocumento = 0;
    $scope.NumeroDocumento = 0;

    $scope.ListaAniosFactura = [];
    $scope.AnioFacturaSeleccionado = "";

    $scope.ImprimirEstadoPedido = function (docnum, user) {
        var UrlString = Ruta.path + '/IntegracionSap/ImprimirEstadoPedido?DocNum=' + docnum + '&Usuario=' + user;
        $("#pdf_preview_e_p").attr('src', UrlString);
        modalEstadoPedido.style.display = "block";
    }

    //::::::ZONA MODAL DE ESTADO PEDIDO::::::::://
    $scope.AbrirModalEstadoFactura = function (docNum, user, rutaQr) {
        console.log(docNum, rutaQr);
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + "/Modales/EstadoFactura.html",
            controller: EstadoFacturaModalController,//Recibe ITEMList, es el nombre de su parametro.
            keyboard: false,
            backdrop: "static",
            resolve: {
                URLParameters: function () {
                    return { Document: docNum, User: user, RutaQr: rutaQr };//Este es el parametro que recibo en la funcion AbrirModalArticulos
                }
            }
        }).result.then(function (result) { });
    }

    var EstadoFacturaModalController = function (Ruta, $scope, $uibModalInstance, $log, URLParameters) {
        $scope.SrcParameters = URLParameters;
        $scope.UrlString = Ruta.path + '/IntegracionSap/ImprimirEstadoFactura?DocNum=' + $scope.SrcParameters.Document + '&RutaQr=' + $scope.SrcParameters.RutaQr;

        $scope.ok = function () {
            $uibModalInstance.close($scope.ItemSeleccion);//cuando se cierre regresa estos datos
        }
    }
    //::::::FIN DE MODAL ESTADO PEDIDO:::::::::://

    //FUNCIONES
    $scope.InicialMaster = function () {
        console.log("Datos iniciales en consultaFacturaController************");
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

    $scope.VisualizarTipoConsulta = function () {
        var selected = $filter('filter')($scope.ListaTipoConsulta, { value: $scope.TipoConsulta.Tipo });
        return ($scope.TipoConsulta.Tipo && selected.length) ? selected[0].text : 'Sin Tipo de Consulta';
    }

    ///:::::REGION MODAL SOCIO DE NEGOCIOS::::::::::////
    $scope.mostrarModalSN = function () {
        let opcion = ''
        if ($scope.rol == 'V') {
            opcion = '1'
        }
        if ($scope.rol == 'C') {
            opcion = '2'
        }
        $scope.respuestaSN = [];
        Servicio.ObtenerListaSocioDeNegocios(opcion).then(function (respuestaSN) {
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
                }
            });
        }
    }

    ///:::::REGION MODAL Cliente::::::::::////
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
                }
            }

            $scope.CargarAniosFactura();
        });
    }

    $scope.CargarAniosFactura = function () {
        console.log("Cargar Años Factura...");
        $scope.years = [];
        $scope.year = "";
        let UserId = '';

        if (!$scope.customerBP || !$scope.customerBP.CardCode) {
            growl.error("Cliente no seleccionado", { title: 'Error' });
            return;
        }

        if ($scope.rol == 'V') {
            UserId = $scope.customerBP.CardCode;
        }

        if ($scope.ListaAniosFactura && $scope.ListaAniosFactura.length > 0) {
            return;
        }

        Servicio.ListarAniosFactura(UserId).then(function (respuesta) {
            if (respuesta.codigo === 0) {
                // Mapear para obtener solo los años como string
                $scope.years = respuesta.mensaje.map(x => x.Anio);

                // Opcional: seleccionar el primer año (más reciente)
                if ($scope.years.length > 0) {
                    $scope.year = $scope.years[0];
                }
            } else {
                $scope.years = [];
                $scope.year = "";
                growl.error("Error al obtener años de facturación.", { title: 'Error' });
            }
        });
    }

    $scope.ConsultaFacturas = function () {
        let UserId = "";
        $scope.ListaFctura = [];

        if ($scope.ValorConsulta == "" || $scope.ValorConsulta == undefined) {
            $scope.ValorConsulta = "Todos";
        }

        if ($scope.TipoConsulta.Tipo == 'NumFactura') {
            $scope.ParametrosConsulta = {
                DocNum: $scope.ValorConsulta,
                UserId: ''
            }
        }
        else {
            $scope.ParametrosConsulta = {
                DocNum: -1,
                UserId: ''
            }
        }

        if ($scope.rol == 'V') {
            UserId = $scope.customerBP.CardCode;
        }
        else {
            UserId = $scope.ParametrosConsulta.UserId;
        }

        console.log("el usuario a buscar es:" + UserId);
        if ($scope.rol == 'V' && $scope.customerBP.CardCode == undefined) {
            growl.warning("Debe seleccionar un cliente para realizar la búsqueda de factura", { title: 'Cliente no seleccionado' });
            return;
        }

        if ($scope.customerBP.CardCode == undefined) {
            growl.warning("Debe seleccionar un cliente para realizar la búsqueda de factura", { title: 'Cliente no seleccionado' });
            return;
        }
        else {
            Servicio.ConsultaFactura($scope.ParametrosConsulta.DocNum, UserId, $scope.ParametrosConsulta.Tipo, $scope.ParametrosConsulta.Fecha, $scope.ParametrosConsulta.Articulo, $scope.year, $scope.month).then(function (respuesta) {
                $scope.ValorConsulta = "";

                if (respuesta != undefined && respuesta[0].NoPedido == -33) {
                    growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
                    return;
                }

                if (respuesta != undefined && respuesta.length > 0) {
                    $scope.ValorConsulta = "";
                    angular.forEach(respuesta, function (resp) {
                        $scope.ListaFctura.push({
                            NoPedidoSAP: resp.NoPedidoSAP,
                            DocDate: resp.DocDate,
                            DocDueDate: resp.DocDueDate,
                            Cliente: resp.Cliente,
                            FechaCont: resp.FechaCont,
                            DocFactura: resp.DocFactura,
                            EntryFactura: resp.EntryFactura,
                            DocEntry: resp.DocEntry,
                            rutaQr: resp.rutaQr,
                            TotalsinIva: resp.TotalsinIva,
                            Total: resp.Total,
                            Estado: resp.Estado
                        });
                    });
                    $scope.ValorConsulta = "";
                    console.log($scope.ListaFctura);
                }
                else {
                    $scope.ValorConsulta = "";
                    $scope.MensajesDeUsuario('info', "¡No se recupero ninguna información con los parametros especificados!");
                }
            });
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

    //----------------------------------------------------------------------------------------------------
    $scope.CargarAniosFac = function () {
        $scope.years = [];
        $scope.year = "";
        let UserId;

        if ($scope.rol == 'V') {
            UserId = $scope.customerBP.CardCode;
        }
        else {
            UserId = $scope.ParametrosConsulta.UserId;
        }

        Servicio.ObtenerAniosFac(UserId).then(function (response) {
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
    //----------------------------------------------------------------------------------------------------
    //Funciones Iniciales
    $scope.InicialMaster();
});
//////::::::::::FIN:::::::::::SECCION CONSULTA FACTURA ::::::::::::::::::::::::::::

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
appFacturas.directive('jqdatepicker', function () {
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

