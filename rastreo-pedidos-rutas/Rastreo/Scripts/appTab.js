var appTab = angular.module('appTab', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

appTab.factory('Ruta', function () {
    return { path: '' }
});

appTab.config(['growlProvider', function (growlProvider) {
    
    growlProvider.globalPosition('bottom-center');
    //growlProvider.globalReversedOrder(true);
    growlProvider.globalTimeToLive({ success: 20000, error: 10000, warning: 10000 })
}]);


appTab.run(['editableOptions', function (editableOptions) {
    editableOptions.theme = 'bs3';
}]);


appTab.service('Servicio', function ($http, Ruta, $log) {
   //Servicios AppPedido
    return {
        ObtenerListaSocioDeNegocios: function () {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerSociosDeNegocios", {}).then(function (respuestaCustomer) {
                return respuestaCustomer.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtner los Socios de Negocios de SAP" }
                };
            })
        },
        ObtenerArticulosSAP: function (codigoCliente, codigoAlmacen, pLista) {
            
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerArticulosdeSAP", { cardCode: codigoCliente, whsCode: codigoAlmacen, priceList: pLista }).then(function (respuestaITEM) {
                return respuestaITEM.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener los Articulos de SAP" }
                };
            })
        },
        ObtenerTasasDeCambioSAP: function (date){
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerTasasDeCambio", { fecha: date }).then(function (respuestaTasas) {
                return respuestaTasas.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje:{"Error": "Error al obtener las Tasas de Cambio"}
                }
            })
        },
        ObtenerImpuestoSAP: function (taxCode) {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerImpuestoDeSAP", { TaxCode : taxCode}).then(function (respuestaTax) {
                return respuestaTax.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener el impuesto de SAP" }
                };
            })
        },
        ObtenerCodigoImpuestoBP: function(codeBP){
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerCodigoImpuestoBP", { CardCode: codeBP }).then(function (respuestaTaxBP) {
                return respuestaTaxBP.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener el impuesto del Socio de Negocios" }
                }
            })
        },
        ObtenerImpuestoDefault: function(){
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
        ConsultaPedido: function (docNum, userCode, type, dateOrder, item) {
            
            return $http.post(Ruta.path + "/Reportes/ConsultarPedidos", { DocNum: docNum, UserCode: userCode, Type: type, DateOrder: dateOrder, Item: item }).then(function (respuestaConsulta) {
                return respuestaConsulta.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al consultar pedidos" }
                };
            });
        },
        ConsultarInventario: function (GrpCode, ByItm, ByDesc, ItmCod) {

            return $http.post(Ruta.path + "/Reportes/ObtenerInventarioDeArticulo", { Grupo: GrpCode, PorItem: ByItm, PorDesc: ByDesc, ItemCode: ItmCod }).then(function (respuestaInventario) {
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
                    codigo: -1, mensaje: {"Error":"No se pudo imprimir el reporte"}
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

            return $http.post(Ruta.path + '/IntegracionSAP/ImprimirReporte', { docEntrySAP : docEntry}, config).then(function (response) {
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
        DatosUsr: function () {
            return $http.get(Ruta.path + "/LogicaPedido/Usuario").then(function (response) {
                return response.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Al recuperar información del usuario" }
                }
            });
        },
        ObtenerProducto: function (SplName, Sucursal, ListaPrecios) {

            return $http.post(Ruta.path + "/LogicaPedido/ObtenerProducto", { SplName: SplName, Sucursal: Sucursal, priceList: ListaPrecios }).then(function (result) {
                return result.data;
            }, function (error) {
                var data = {
                    codigo: -1, mensaje: { "Error": "Error al obtener el articulo de Sap" }
                };
            })
        }
    }
});

appTab.controller('TabController', function ($scope) {
    $scope.tab = 1;
  
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
})

appTab.controller('inventarioController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {

    $scope.ArticuloDeInventario = {};
        

    ///:::::REGION MODAL PARA ARTICULOS  ::::::::::////
    $scope.MostrarModalItemInvent = function () {

        Servicio.ObtenerArticulosSAP($scope.customerBP.CardCode, $scope.WareHoseCode, $scope.customerBP.ListaPrecios).then(function (respInvent) {
            if (respInvent != undefined) {
                //llamamos a la modal y mandamos valores: lista de Arítuclos
                $scope.AbrirModalArticulosInventario(respInvent);
            }
        });

        $scope.AbrirModalArticulosInventario = function (listaDeArticulos) {
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

        $scope.ok = function () {
            $uibModalInstance.close($scope.ItemSeleccion);
        }

        $scope.dataTableOpt = {
            "aLengthMenu": [[5, 10, 50, 100, -1], [5, 10, 50, 100, 'All']],
        };
        //ListaITEM sera alcanzable solo en la modal
        $scope.ListaITEMInvt = ITEMListInvt;//llena la tabla de la modal

        $scope.ItemSeleccionado = function (item) {
            $scope.ItemSeleccion = item;
            $scope.ok();
        }
    }
});

appTab.controller('reimpresionController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {

    //VARIABLES
    var regexInteger = /^[0-9]*$/g;
    $scope.TipoDocumento = { Tipo: 'order' };
    $scope.ListaTipoDocumento = [
     { value: 'order', text: 'Pedido' },
    /* { value: 'quot', text: '-----' }*/
    ];

    $scope.TipoNumero = { Tipo: 'DocNum' };
    $scope.ListaTipoNumero = [
        { value: 'DocNum', text: 'Núm. Documento' },
        { value: 'Folio', text: 'Folio' }
    ];
   
    //FUNCIONES
    $scope.VisualizarTipoDocumento = function () {
        var selected = $filter('filter')($scope.ListaTipoDocumento, { value: $scope.TipoDocumento.Tipo });
        return ($scope.TipoDocumento.Tipo && selected.length) ? selected[0].text : 'Sin Tipo de Consulta';
    }

    $scope.VisualizarTipoNumero = function () {
        var selected = $filter('filter')($scope.ListaTipoNumero, { value: $scope.TipoNumero.Tipo });
        return ($scope.TipoNumero.Tipo && selected.length) ? selected[0].text : '------'
    };


    $scope.ReimpresionDocumentos = function () {

        Servicio.VerificarSesion().then(function (sesion) {

            if (sesion != true) {
                growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
            }
            else {
                if ($scope.ValorDocumento != undefined && $scope.ValorDocumento != "") {

                    var isNumero = angular.isNumber($scope.ValorDocumento);
                    $('<form>', {
                        "method": "POST",
                        "action": Ruta.path + '/IntegracionSap/ImprimirPedido?docEntrySAP=' + $scope.ValorDocumento + "&tipo=" + $scope.TipoNumero.Tipo,
                        "target": "_blank"
                    }).appendTo(document.body).submit();
                }
                else {
                    growl.error("Introduzca un Número de Documento o Folio Válido", { title: 'Dato no Válido' });
                }
            }
            
        });     
    };
});

//////::::::::::INICIO:::::::::::SECCION CONSULTA PEDIDO ::::::::::::::::::::::::::::
appTab.controller('consultaPedidoController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {


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
    
    $scope.NumeroDocumento = 0;

    $scope.ImprimirEstadoPedido = function (docnum, user) {
       
        var UrlString = Ruta.path + '/IntegracionSap/ImprimirEstadoPedido?DocNum=' + docnum + '&Usuario=' + user;
        $("#pdf_preview_e_p").attr('src',UrlString);
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
        }).result.then(function (result) {

        });
    }

    var EstadoPedidoModalController = function (Ruta, $scope, $uibModalInstance, $log, URLParameters) {
        $scope.SrcParameters = URLParameters;
        $scope.UrlString = Ruta.path + '/IntegracionSap/ImprimirEstadoPedido?DocNum=' + $scope.SrcParameters.Document;

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
        

        if ($scope.ValorConsulta == "" || $scope.ValorConsulta == undefined) {
            $scope.ValorConsulta = "Todos";
        }
        $scope.ListaPedidos = [];
        if ($scope.TipoConsulta.Tipo == 'Fecha') {
            $scope.ParametrosConsulta = {
                DocNum: -1,
                UserId: '',
                Tipo: $scope.TipoConsulta.Tipo,
                Fecha: ($scope.ValorConsulta).toISOString().substring(0, 10),
                Articulo: ''
            }
        } else {
            $scope.ParametrosConsulta = {
                DocNum: $scope.TipoConsulta.Tipo != 'DocNum' ? -1 : $scope.ValorConsulta,
                UserId: '',
                Tipo: $scope.TipoConsulta.Tipo,
                Fecha: $scope.ValorConsulta,
                Articulo: $scope.ValorConsulta
            }
        }

        Servicio.ConsultaPedido($scope.ParametrosConsulta.DocNum, $scope.customerBP.CardCode , $scope.ParametrosConsulta.Tipo, $scope.ParametrosConsulta.Fecha, $scope.ParametrosConsulta.Articulo).then(function (respuesta) {
                $scope.ValorConsulta = "";
                
                //if (respuesta != undefined && respuesta[0].NoPedido == -33) {
                //    growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
                //    return;
                //}
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
                        });
                    });
                    $scope.ValorConsulta = "";
                } else {
                    $scope.ValorConsulta = "";
                    $scope.MensajesDeUsuario('info', "No se recupero ninguna información con los parametros especificados!");
                }
        });
    }

    $scope.MensajesDeUsuario = function (type, message) {
        var config = {};
        switch (type) {
            case "success":
                growl.success(message, { title: "Éxito!" });
                break;
            case "info":
                growl.info(message, { title: "Informativo!", ttl:10000});
                break;
            case "warning":
                growl.warning(message, { title: "Atención!" });
                break;
            case "error":
                growl.error(message, { title: "Error!" });
                break;
        }
    };
});
//////::::::::::FIN:::::::::::SECCION CONSULTA PEDIDO ::::::::::::::::::::::::::::

appTab.controller('inventarioPorArticuloController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {

    //VARIABLES PARA Inventario por articulo
    $scope.ArticuloDeInventario = {};
    $scope.ListaArticuloDeInventario = [];
    $scope.TotalWhsForItemInvt = 0;
    $scope.QtyWhsByPage = 9; 

    ///:::::::::::region de funciones para reporte:::::::::::
    $scope.ConsultarArticulo = function (itemCode) {
        
        if ($scope.ArticuloDeInventario.ItemCode != "" && $scope.ArticuloDeInventario.ItemCode != undefined) {
            $scope.ListaArticuloDeInventario = [];
            Servicio.ConsultarInventario(0, '', '', itemCode).then(function (respInvent) {

                if (respInvent != undefined && respInvent.length != 0) {
                    angular.forEach(respInvent, function (item) {
                        $scope.ListaArticuloDeInventario.push({
                            WhsCode: item.WhsCode,
                            WhsName: item.WhsName,
                            OnHandQty: item.OnHandQty,
                            IsCommited: item.IsCommited,
                            OnOrder: item.OnOrder,
                            Available: item.Available
                        });
                    });
                    
                    $scope.TotalWhsForItemInvt = $scope.ListaArticuloDeInventario.length > 0 ? $scope.ListaArticuloDeInventario.length : 0;
                }
            });
        } else {
            $scope.MensajesDeUsuario('info', "Introduzca o Seleccione un código de Artículo.");
        }
        
        
    }
    ///::::::: fin de region de funcines para reportes::::::

    ///:::::REGION MODAL PARA ARTICULOS  ::::::::::////
    $scope.MostrarModalItemInvent = function () {

        Servicio.ObtenerArticulosSAP('', $scope.WareHoseCode, 0).then(function (respInvent) {
            if (respInvent != undefined) {
                //llamamos a la modal y mandamos valores: lista de Arítuclos
                $scope.AbrirModalArticulosInventario(respInvent);
            }
        });

        $scope.AbrirModalArticulosInventario = function (listaDeArticulos) {
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
                growl.success(message, { title: "Éxito!" });
                break;
            case "info":
                growl.info(message, { title: "Informativo!", ttl:10000});
                break;
            case "warning":
                growl.warning(message, { title: "Atención!" });
                break;
            case "error":
                growl.error(message, { title: "Error!" });
                break;
        }
    };

});

//////::::::::::INICIO:::::::::::SECCION GENERAR PEDIDO ::::::::::::::::::::::::::::
appTab.controller('pedidoController', function ($scope, $document, $uibModal, Servicio, Ruta, $rootScope, $log, $filter, growl) {

    ///Variables globales dentro del alcance de VistaPedio.cshtml///
   
    //calculo de Fecha del SISTEMA
    var fecha = new Date();
    var mes = fecha.getMonth() + 1;
    var dia = fecha.getDate();
    var fechaCompleta = fecha.getFullYear() + '-' + (mes < 10 ? '0' : '') + mes + '-' + (dia < 10 ? '0' : '') + dia;

    $scope.searchTextPedido;

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
    $scope.TotalAPagarDisplay = "";
    $scope.SbttlAPagarDisplay = "";
    $scope.FechaSistema = "";
    $scope.ComentariosPedido = "";
    $scope.DocEntryPedido = 0;
    $scope.MensajeDelSistema = "";
    $scope.MostrarMensaje = false;
    //ordenamiento
    $scope.sortType = 'name'; //tipo de ordenamiento default
    $scope.sortReverse = false;  // orden default
    $scope.NombreItem = ''; 
    // termindo de busqueda default
    //$scope.regexDecimalValido = true;
    $scope.ValidParmeter = '/\d+/';

    $scope.NumPedido = "";
    //monedas
    $scope.TasasDeCambio = [];//almacena las tasas de cambio
    $scope.VisualizarMoneda = false; //Indicador para mostrar combo moneda. Por defecto no se muestra
    $scope.MonedaDelSN = { Moneda: '$' };//default pesos mexicanos
    $scope.Monedas = [ 
      { value: '$', text: 'Pesos' },
      { value: 'USD', text: 'Dolares' },
      { value: 'EUR', text: 'Euros' },
      { value: 'GPB', text: 'Libra' }
    ];
    $scope.ImprimirDesde = new Date();
    $scope.ImprimirDesdeTxt = "";
    $scope.ImprimirHasta = new Date();
    $scope.ImprimirHastaTxt = "";
    
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
        SalesPersonCode: "",
        ShipToCode: "",
        Address: "",
        JournalMemo: "",
        DocumentStatus: "",
        DocType: "",
        Discount: "",
        DocumentLines: []
    };
 
    $scope.EstadoPedido = 'O';
    $scope.EstadoPedido = { Tipo: 'O' };//default pesos mexicanos
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

    ObtenerTsaDeCambio();
    ObtenerAlmacen();

    ///:::::REGION MODAL PREVISUALIZADOR::::::::::::::///

    $scope.PrevisualizarPedido = function () {

     
        if (($scope.ListaItemDetalle).length <= 0) {
            growl.error("No ha cargado los Artículos del Pedido", { title: 'Sin Datos en el Detalle' });
            return;
        }
   
        //if ($scope.NumPedido <= 0 || $scope.NumPedido == "") {
        //    growl.error("No ha ingresado el Folio del Pedido", { title: 'Sin Folio de Pedido' });
        //    return;
        //}
       
        if ($scope.FechaSistema == "" || $scope.FechaSistema == undefined) {
            growl.error("No ha ingresado la Fecha de Entrega", { title: 'Sin Fecha de Entrega' });
            return;
        }

        if ($scope.FechaSistema == "" || $scope.FechaSistema == null || $scope.FechaSistema == undefined) {
            growl.error('No se ha establecido la Fecha del Pedido.', { title: 'Error!' });
            return;
        }
        
        
       $("#pdf_preview").attr("src", '');
        //llanando llenado de json
        $scope.ExtraerJsonDetalle();
        //creando documento pdf
        var doc = new jsPDF();
        
        //para añadir imagenes
        //var niceimage = 'data:image/jpeg;base64,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';
        //doc.addImage(niceimage, 'JPEG', 25, 200, 150, 76);
        //MARCA DE AGUA
        doc.setFontType("normal");
        doc.setTextColor(204, 51, 0);
        doc.text('Este documento es una previsualización, no es un documento en SAP', 15, 10);
        doc.setTextColor(0, 0, 0);
        // ZONA ENCABEZADOS: Datos Sociedad
        doc.rect(5, 15, 130, 20);
        doc.setFontType("bold");
        doc.setFontSize(14);
        doc.text('YOUR COMPANY', 45, 20);
        doc.text('YOUR ADDRESS LINE 1', 15, 26);
        doc.text('YOUR ADDRESS LINE 2', 40, 32);
        

        //Etiquetas Datos Fechas y Folio
        doc.rect(135, 15, 35, 20);
        doc.text('NO. PED: ', 137, 20);
        doc.text('-----------------', 137, 26);
        doc.text('ENTREGA: ', 137, 32);
        //Valores Datos Fechas y Folio
        doc.rect(170, 15, 35, 20);
        doc.setFontType("normal");
        doc.setTextColor(204,51,0);
        doc.text(($scope.JsonPedido.U_NumPed).toString(), 172, 20);
        doc.setTextColor(0, 0, 0);
        doc.text('-----------------', 172, 26);
        doc.setTextColor(204, 51, 0);
        doc.text($scope.JsonPedido.DocDueDate, 172, 32);
        doc.setTextColor(0, 0, 0);
        //Datos Cliente
        doc.rect(5, 35, 30, 7);
        doc.setFontType("bold");
        doc.text('NOMBRE: ', 7, 40);
        //Valores Cliente
        doc.rect(35, 35, 170, 7);
        doc.text($scope.JsonPedido.CardName,40, 40);
        doc.setFontType("normal");
        var rxH = 0;
        //PONER ENCABEZADOS
        doc.rect(5, 47, 9, 7);//header para 1ra col. Num. Registro
        doc.rect(14, 47, 25, 7);//header para 2da  ITEMCODE
        doc.rect(39, 47, 65, 7);//header para 3ra col. DESCRIPCION
        doc.rect(104, 47, 18, 7);//header para 4ta col. CANTIDAD
        doc.rect(122, 47, 16, 7);//header para 5ta col. DESC. ESC. 
        doc.rect(138, 47, 14, 7);//header para 6ta col. PROM.
        doc.rect(152, 47, 10, 7);//impuestos
        doc.rect(162, 47, 16, 7);//header para 7ma col. PRECIO
        doc.rect(178, 47, 27, 7);//header para 8va col. TOTAL DE LINEA

        //Valores de encabezados
        doc.setFontSize(12);
        doc.text('#', 7, 52);
        doc.text('Artículo', 20, 52);
        doc.text('Descripción', 60, 52);
        doc.text('Cantidad', 105, 52);
        doc.text('D. Esc.',123,52);
        doc.text('Prom.', 139, 52);
        doc.text('Imp.', 153,52);
        doc.text('Precio', 163, 52);
        doc.text('Total', 179,52);
        //CICLAR LAS LINEAS DEL DOCUMENTO
        //variables de inicio de cordedas (x,y) y dimensiones w, h
        //pRIMER PAR DE COLUMNAS
        var maxRow = 25; //maximo numero de registros por hoja
        var r1X = 5; var r1Y = 54; var r1W = 9; var r1H = 8; 
        var r2X = r1W + r1X;
        var r2Y = r1Y;// * 1; //posicion del detalle 0,1,2,3 etc
        var r2W = 25;
        var r2H = 8;
        //SEGUNDO PAR DE COLUMNAS
        var r3X = r2W + r2X; var r3Y = r1Y; var r3W = 65; var r3H = 8;
        var r4X = r3W + r3X; var r4Y = r3Y; var r4W = 18; var r4H = 8;
        //TERCER PAR DE COLUMNAS
        var r5X = r4X + r4W; var r5Y = r4Y; var r5W = 16; var r5H = r4H;
        var r6X = r5X + r5W; var r6Y = r5Y; var r6W = 14; var r6H = r5H;
        //CUARTO PAR DE COLUMNAS
        var r7X = r6X + r6W; var r7Y = r6Y; var r7W = 10; var r7H = r6H;
        var r8X = r7X + r7W; var r8Y = r7Y; var r8W = 16; var r8H = r7H;
        var r9X = r8X + r8W; var r9Y = r8Y; var r9W = 27; var r9H = r8H;
        //variable para coordenadas de impresion de texto de 1ER PAR DE COLUMNAS
        var t1X = r1X + 1; var t1Y = r1Y + 6; var t2X = r2X + 2; var t2Y = r2Y + 6;
        //variable para coordendasd de impresion de text 2DA PAR DE COLUMNAS
        var t3X = r3X + 1; var t3Y = r3Y + 6; var t4X = r4X + 5; var t4Y = r4Y + 6;
        //variables para coordenadas de impresion de texto para 3er PAR DE COLUMNAS
        var t5X = r5X + 2; var t5Y = r5Y + 6; var t6X = r6X + 2; var t6Y = r6Y + 6;
        //variables de coordendadas de impresion de texto para 4to PAR DE COLUMNAS
        var t7X = r7X + 1; var t7Y = r7Y + 6; var t8X = r8X + 1; var t8Y = r8Y + 6;
        var t9X = r9X + 1; var t9Y = r9Y + 6;

        //coordenadas de texto y rectangulos de pie de pagina TOTALES
        var r10X = 15; var r10Y = r9Y + 10; var r10W = 30; var r10H = 15;
        var r11X = r10X + r10W; var r11Y = r10Y; var r11W = 15; var r11H = r10H;
        var r12X = r11X + r11W; var r12Y = r10Y; var r12W = 28; var r12H = r10H;
        var r13X = r12X + r12W; var r13Y = r10Y; var r13W = 35; var r13H = r10H;
        var r14X = r13X + r13W; var r14Y = r10Y; var r14W = 28; var r14H = r10H;
        var r15X = r14X + r14W; var r15Y = r10Y; var r15W = 40; var r15H = r10H;

        //coordenadas de texto de totales
        var t10X = r10X + 2; var t10Y = r10Y + 5;
        var t11X = r11X + 2; var t11Y = r11Y + 10;
        var t12X = r12X + 1; var t12Y = r12Y + 5;
        var t13X = r13X + 1; var t13Y = r13Y + 10;
        var t14X = r14X + 1; var t14Y = r14Y + 5;
        var t15X = r15X + 1; var t15Y = r15Y + 10;
        

        //Numero de Hojas
        var numHojas = ($scope.ListaItemDetalle).length / 60;
        var numRows = ($scope.ListaItemDetalle).length;
        //Numero de Columnas por Hoja
        var numColumnas = 1; //default
        var posActualRecorido = 0;
        var contadorLineasPorHoja = 0;
        var contadorLineasRecorridas = 0;
        var contadorAuxLineasPorHoja = 0;
        var contadorColmImpresas = 1;
        var primerHoja = true;

            for (var pos = 0; pos < numRows; pos++) {
                if (!primerHoja) {
                    maxRow = 28;
                }
                if (contadorLineasPorHoja < maxRow) {
                    
                        
                    doc.setFontSize(10);
                    doc.setFontStyle('italic');
                    //Numero de linea
                    doc.rect(r1X, r1Y, r1W, r1H);
                    doc.text((pos + 1).toString(), t1X, t1Y);
                    //ItemCode
                    doc.rect(r2X, r2Y, r2W, r2H);
                    doc.text($scope.ListaItemDetalle[pos].Codigo, t2X, t2Y);
                    //Descripcion
                    doc.rect(r3X, r3Y, r3W, r3H);
                    if (($scope.ListaItemDetalle[pos].Nombre).length < 28) {
                        doc.text($scope.ListaItemDetalle[pos].Nombre, t3X, t3Y);
                    } else {
                        doc.text(($scope.ListaItemDetalle[pos].Nombre).substring(0, 28) + ' ...', t3X, t3Y);
                    }
                    //Cantidad
                    doc.rect(r4X, r4Y, r4W, r4H);
                    doc.text(($scope.ListaItemDetalle[pos].Cantidad).toString(), t4X, t4Y);
                    //Desc. Escalar
                    doc.rect(r5X, r5Y, r5W, r5H);
                    doc.text(($scope.JsonPedido.Discount).toString() + "%", t5X, t5Y);
                    //Promocion
                    doc.rect(r6X, r6Y, r6W, r6H);
                    doc.text(($scope.ListaItemDetalle[pos].Descuento).toString() + "%", t6X, t6Y);
                    //Precio
                    doc.rect(r7X, r7Y, r7W, r7H);
                    doc.text(($scope.ListaItemDetalle[pos].ValorImpuesto).toString() + "%", t7X, t7Y);
                    //Total
                    doc.rect(r8X, r8Y, r8W, r8H);
                    doc.text("$" + ($scope.ListaItemDetalle[pos].PrecioUnitario).toString(), t8X, t8Y);
                    doc.rect(r9X, r9Y, r9W, r8H);
                    doc.text("$" + ($scope.ListaItemDetalle[pos].Total).toString(), t9X, t9Y);

                    //recalculmos coordenadas y dimensiones para primer rectanculo
                    r1X = r1X; r1Y = r1Y + r1H; r1W = r1W; r1H = r1H;
                    //recalculamos coordenadas y dimeneiones para segundo rectangulo
                    r2X = r2X; r2Y = r2Y + r2H; r2W = r2W; r2H = r2H;
                    //recalculmos coordenadas y dimensiones para rectanculo
                    r3X = r3X; r3Y = r3Y + r3H; r3W = r3W; r3H = r3H;
                    //recalculamos coordenadas y dimeneiones para rectangulo
                    r4X = r4X; r4Y = r4Y + r4H; r4W = r4W; r4H = r4H;
                    //recalclamos coordenadas y dimensiones para 5to rectangulo
                    r5X = r5X; r5Y = r5Y + r5H; r5W = r5W; r5H = r5H;
                    r6X = r6X; r6Y = r6Y + r6H; r6W = r6W; r6H = r6H;
                    r7X = r7X; r7Y = r7Y + r7H; r7W = r7W; r7H = r7H;
                    r8X = r8X; r8Y = r8Y + r8H; r8W = r8W; r8H = r8H;
                    r9X = r9X; r9Y = r9Y + r9H; r9W = r9W;  r9H = r9H;
                    //recalculamos coordenadas y para textos
                    t1X = t1X; t1Y = r1Y + 6; t2X = t2X; t2Y = r2Y + 6;
                    //recalculamos coordenadas y para textos
                    t3X = t3X; t3Y = r3Y + 6; t4X = t4X; t4Y = r4Y + 6;
                    //recalculamos coordendas para textos
                    t5X = t5X; t5Y = r5Y + 6; t6X = t6X; t6Y = r6Y + 6;
                    //recalculamos coordenadas para textos
                    t7X = t7X; t7Y = r7Y + 6; t8X = t8X; t8Y = r8Y + 6;
                    t9X = t9X; t9Y = r9Y + 6;

                    //recalculando coordenadas para rectangulos de coordenadas
                    r10X = 15; r10Y = r9Y + 10; r10W = 30; r10H = 15;
                    r11X = r10X + r10W; r11Y = r10Y; r11W = 15; r11H = r10H;
                    r12X = r11X + r11W; r12Y = r10Y; r12W = 28; r12H = r10H;
                    r13X = r12X + r12W; r13Y = r10Y; r13W = 35; r13H = r10H;
                    r14X = r13X + r13W; r14Y = r10Y; r14W = 28; r14H = r10H;
                    r15X = r14X + r14W; r15Y = r10Y; r15W = 40; r15H = r10H;

                    //recalculando coordenadas para textos de totales
                    t10X = r10X + 2; t10Y = r10Y + 5;
                    t11X = r11X + 2; t11Y = r11Y + 10;
                    t12X = r12X + 1; t12Y = r12Y + 5;
                    t13X = r13X + 1; t13Y = r13Y + 10;
                    t14X = r14X + 1; t14Y = r14Y + 5;
                    t15X = r15X + 1; t15Y = r15Y + 10;

                    
                    if ((contadorLineasRecorridas + 1) == numRows) {

                        //PIE DE PAGINA
                        doc.setFontSize(12);
                        doc.rect(r10X, r10Y, r10W, r10H);
                        doc.text('Total', t10X, t10Y);
                        doc.text('Unidades', t10X, t10Y + 6);
                        doc.rect(r11X, r11Y, r11W, r11H);
                        doc.text(($scope.TotalUnidades).toString(), t11X, t11Y);
                        doc.rect(r12X, r12Y, r12W, r12H);
                        doc.text('Importe', t12X, t12Y);
                        doc.text('Impuesto', t12X, t12Y + 6);
                        doc.rect(r13X, r13Y, r13W, r13H);
                        doc.text("$"+$scope.TotalEnImpuestoStr, t13X, t13Y);
                        doc.rect(r14X, r14Y, r14W, r14H);
                        doc.text('Importe', t14X, t14Y);
                        doc.text('Total', t14X, t14Y + 6);
                        doc.rect(r15X, r15Y, r15W, r15H);
                        doc.text("$" + $scope.TotalAPagarDisplay, t15X, t15Y);
                    }

                    if (contadorLineasPorHoja == (maxRow - 1)) {

                        //PIE DE PAGINA
                        doc.setFontSize(12);
                        doc.rect(r10X, r10Y, r10W, r10H);
                        doc.text('Total', t10X, t10Y);
                        doc.text('Unidades', t10X, t10Y + 6);
                        doc.rect(r11X, r11Y, r11W, r11H);
                        doc.text(($scope.TotalUnidades).toString(), t11X, t11Y);
                        doc.rect(r12X, r12Y, r12W, r12H);
                        doc.text('Importe', t12X, t12Y);
                        doc.text('Impuesto', t12X, t12Y + 6);
                        doc.rect(r13X, r13Y, r13W, r13H);
                        doc.text("$" + $scope.TotalEnImpuestoStr, t13X, t13Y);
                        doc.rect(r14X, r14Y, r14W, r14H);
                        doc.text('Importe', t14X, t14Y);
                        doc.text('Total', t14X, t14Y + 6);
                        doc.rect(r15X, r15Y, r15W, r15H);
                        doc.text("$"+$scope.TotalAPagarDisplay, t15X, t15Y);


                        primerHoja = false;
                        contadorLineasPorHoja = 0;
                        
                        doc.addPage();
                        doc.setFontType("normal");
                        doc.setFontSize(15);
                        doc.setTextColor(204, 51, 0);
                        doc.text('Este documento es una previsualización, no es un documento en SAP', 15, 10);
                        doc.setTextColor(0, 0, 0);
                        //PONER ENCABEZADOS
                        var rTY = 20;
                        var tTY = 25;

                        doc.rect(5, rTY, 9, 7);//header para 1ra col. Num. Registro
                        doc.rect(14, rTY, 25, 7);//header para 2da  ITEMCODE
                        doc.rect(39, rTY, 65, 7);//header para 3ra col. DESCRIPCION
                        doc.rect(104, rTY, 18, 7);//header para 4ta col. CANTIDAD
                        doc.rect(122, rTY, 16, 7);//header para 5ta col. DESC. ESC. 
                        doc.rect(138, rTY, 14, 7);//header para 6ta col. PROM.
                        doc.rect(152, rTY, 10, 7);//impuestos
                        doc.rect(162, rTY, 16, 7);//header para 7ma col. PRECIO
                        doc.rect(178, rTY, 27, 7);//header para 8va col. TOTAL DE LINEA
                        //Valores de encabezados

                        doc.setFontSize(12);
                        doc.text('#', 7, tTY);
                        doc.text('Artículo', 20, tTY);
                        doc.text('Descripción', 60, tTY);
                        doc.text('Cantidad', 105, tTY);
                        doc.text('D. Esc.', 123, tTY);
                        doc.text('Prom.', 139, tTY);
                        doc.text('Imp.', 153, tTY);
                        doc.text('Precio', 163, tTY);
                        doc.text('Total', 179, tTY);

                        
                        //pRIMER PAR DE COLUMNAS
                        maxRow = 30; //maximo numero de registros por hoja
                        r1X = 5; r1Y = 27; r1W = 9; r1H = 8;
                        r2X = r1W + r1X;
                        r2Y = r1Y;// * 1; //posicion del detalle 0,1,2,3 etc
                        r2W = 25;
                        r2H = 8;
                        //SEGUNDO PAR DE COLUMNAS
                        r3X = r2W + r2X; r3Y = r1Y; r3W = 65; r3H = 8;
                        r4X = r3W + r3X; r4Y = r3Y; r4W = 18; r4H = 8;
                        //TERCER PAR DE COLUMNAS
                        r5X = r4X + r4W; r5Y = r4Y; r5W = 16; r5H = r4H;
                        r6X = r5X + r5W; r6Y = r5Y; r6W = 14; r6H = r5H;
                        //CUARTO PAR DE COLUMNAS
                        r7X = r6X + r6W; r7Y = r6Y; r7W = 10; r7H = r6H;
                        r8X = r7X + r7W; r8Y = r7Y; r8W = 16; r8H = r7H;
                        r9X = r8X + r8W; r9Y = r8Y; r9W = 27; r9H = r8H;
                        //variable para coordenadas de impresion de texto de 1ER PAR DE COLUMNAS
                        t1X = r1X + 1; t1Y = r1Y + 6; t2X = r2X + 2; t2Y = r2Y + 6;
                        //variable para coordendasd de impresion de text 2DA PAR DE COLUMNAS
                        t3X = r3X + 1; t3Y = r3Y + 6; t4X = r4X + 5; t4Y = r4Y + 6;
                        //variables para coordenadas de impresion de texto para 3er PAR DE COLUMNAS
                        t5X = r5X + 2; t5Y = r5Y + 6; t6X = r6X + 2; t6Y = r6Y + 6;
                        //variables de coordendadas de impresion de texto para 4to PAR DE COLUMNAS
                        t7X = r7X + 1; t7Y = r7Y + 6; t8X = r8X + 1; t8Y = r8Y + 6;
                        t9X = r9X + 1; t9Y = r9Y + 6;

                        //ACTUALIZANDO POSICIONES DE TOTQALES DE PIE DE PAGINA
                        r10X = 15; r10Y = r9Y + 10; r10W = 30; r10H = 15;
                        r11X = r10X + r10W; r11Y = r10Y; r11W = 15; r11H = r10H;
                        r12X = r11X + r11W; r12Y = r10Y; r12W = 28; r12H = r10H;
                        r13X = r12X + r12W; r13Y = r10Y; r13W = 35; r13H = r10H;
                        r14X = r13X + r13W; r14Y = r10Y; r14W = 28; r14H = r10H;
                        r15X = r14X + r14W; r15Y = r10Y; r15W = 40; r15H = r10H;

                        t10X = r10X + 2; t10Y = r10Y + 5;
                        t11X = r11X + 2; t11Y = r11Y + 10;
                        t12X = r12X + 1; t12Y = r12Y + 5;
                        t13X = r13X + 1; t13Y = r13Y + 10;
                        t14X = r14X + 1; t14Y = r14Y + 5;
                        t15X = r15X + 1; t15Y = r15Y + 10;


                    } else {
                        
                        contadorLineasPorHoja++;
                    }

                    contadorLineasRecorridas++;
                } 
            }
        
        $scope.JsonPedido.DocumentLines = [];
        modal.style.display = "block";
        $("#pdf_preview").attr("src", doc.output('datauristring'));//añadimos al iframe
    }
    ///::::FIN REGION MODAL PREVISUALIZAR:::::::::::::///

    ///:::::REGION MODAL SOCIO DE NEGOCIOS::::::::::////
    $scope.customerBP = {
        CardCode: '',
        CardName: '',
        ListaPrecios: '',
        Moneda: '',
        Descuento: '',
        DiscInDoc: '',
        SplName: '',
        Sucursal: ''
    }

    $scope.SocioDeNegocios = function () {
        Servicio.DatosUsr().then(function (response) {
          
            $scope.customerBP = response;
            console.log(response);
            $scope.ObtenerProdu(response.SplName, response.Sucursal, response.ListaPrecios);
       
           
        })
        //Una vez obtenido los datos del socio de negocios se obtiene el producto del cliente dependiendo de sus parametros 
           
        
    }
   

   

    $scope.ObtenerProdu = function (SlpName, Sucursal, ListaPrecios) {
        console.log('Empieza el obtener prodcuto');
      
        Servicio.ObtenerProducto(SlpName, Sucursal, ListaPrecios).then(function (result) {
            console.log(result);
            $scope.LlenarDetallePedido(result);

        })
        //Una vez obtenido los datos del socio de negocios se obtiene el producto del cliente dependiendo de sus parametros 


    }

    $scope.SocioDeNegocios();
    //$scope.mostrarModalSN = function () {

    //    Servicio.ObtenerListaSocioDeNegocios().then(function (respuestaSN) {
    //        if (respuestaSN != null) {
    //            if (respuestaSN[0].CardCode == "-33") {
    //                growl.error("Se agotó el tiempo de sesión. Vuelva a ingresar con sus Datos de Usuario o Actualice la Página", { title: 'Sesión Agotada' });
    //                return;
    //            }
    //            $scope.AbrirModalSN(respuestaSN);
    //        }
    //    });

    //    $scope.AbrirModalSN = function (ListSN) {
    //        var modalInstance = $uibModal.open({
    //            templateUrl: Ruta.path + '/Modales/ListaSN.html',
    //            controller: ListaPedidoController,
    //            //windowClass: 'app-modal-window-md',
    //            keyboard: false,
    //            backdrop: "static",
    //            resolve: {
    //                SNList: function () {
    //                    return ListSN;
    //                }
    //            }
    //        }).result.then(function (result) {//aqui es cachado el valor que seleccione en la modal
    //            //cachar el valor arrojado aqui
                
    //            if (result != undefined && result.CardCode != undefined) {

    //                $scope.customerBP = {
    //                    CardCode: result.CardCode,
    //                    CardName: result.CardName,
    //                    ListaPrecios: result.ListNum,
    //                    Moneda: result.Moneda,
    //                    Descuento: parseFloat(result.Discount),
    //                    DiscInDoc: result.DiscInRet
    //                }
    //                if ($scope.customerBP.Moneda == "##") {
    //                    $scope.VisualizarMoneda = true;
    //                }
    //                else {
    //                    $scope.VisualizarMoneda = false;
    //                }
    //            }
                
    //            //$scope.ObtenerTsaDeCambio();
    //            $scope.BorrarDetallePedido();
    //            $scope.ActualizarTotalAPagar();
    //            $scope.ActualizarTotalUnidades();
    //        });
    //    }
    //}

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

        if ($scope.customerBP.CardCode == undefined ) {
            growl.error("No ha seleccionado al Socio de Negocios", { title: 'Error' });
            return;
        }

        Servicio.ObtenerArticulosSAP($scope.customerBP.CardCode, $scope.WareHoseCode, $scope.customerBP.ListaPrecios).then(function (respuestaITEM) {
            if (respuestaITEM != null) {
                //llamamos a la modal y mandamos valores: lista de Arituclos
                $scope.AbrirModalArticulos(respuestaITEM);
            }
        });

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

        $scope.ItemSeleccionado = function (item) {
            $scope.ItemSeleccion.push(item);
            if ($scope.Multiseleccion == false) {
                $scope.ok();
            }
           //$scope.ok();
        }
    }

    /////// REGION OPERACIOENS 
    $scope.LlenarDetallePedido = function (articulos) {

        $scope.ivaUnitario = 0;
        $scope.precioMasIva = 0;
        $scope.ValorDelImpuesto = 0;
        angular.forEach(articulos, function (articulo) {
            if (articulo.VATLiable == "Y") {//traer el impuesto si el articulo aplica
                //$log.debug("Sujeto a impuesto" + articulo.VATLiable);
                if (articulo.IndirectTax == "Y") {//si tiene activo IEPS en sap
                    //$log.debug("Activo IEPS" + articulo.IndirectTax);
                    Servicio.ObtenerImpuestoSAP(articulo.TaxCodeAR).then(function (respuestaTax) {
                        if (respuestaTax == -1) {
                            growl.warning("No se logró recuperar el impuesto del Artículo.", { title: 'Atención!' });
                            return;
                        }
                        if (respuestaTax != undefined) {
                            $scope.ValorDelImpuesto = respuestaTax;
                            $scope.ObtenerImpuesto(articulo, $scope.ValorDelImpuesto);
                        }
                    });
                }
                else {//si no tiene IEPS, traer el impuesto del socio de negocios
                    growl.warning("El artículo no tiene configurado un impuesto");
                    //$log.debug("Impuesto BP");
                    //Servicio.ObtenerCodigoImpuestoBP($scope.customerBP.CardCode).then(function (respuestaTaxCodeBP) {
                    //    if (respuestaTaxCodeBP != undefined) {
                    //        if (respuestaTaxCodeBP.Rate == -1) {
                    //            growl.warning("No se logró recuperar el Impuesto del Cliente.", { title: 'Atención!' });
                    //            return;
                    //        } else {
                    //            $log.debug("Impuesto "+ respuestaTaxCodeBP.Rate);
                    //            $scope.ValorDelImpuesto = respuestaTaxCodeBP.Rate;
                    //            $scope.ObtenerImpuesto(articulo, $scope.ValorDelImpuesto);
                    //        }
                            
                    //    }
                    //})
                }
            }
            else {//cuando el articulo no aplica a impuesto
                Servicio.ObtenerImpuestoDefault().then(function (respuestaTaxDefault) {
                    if (respuestaTaxDefault != null) {
                        $log.debug("Impuesto deft " + respuestaTaxDefault);
                        $scope.ValorDelImpuesto = respuestaTaxDefault;
                        $scope.ObtenerImpuesto(articulo, $scope.ValorDelImpuesto);
                    }
                })
            }
        });
    }

    //ACTUALIZA CANTIDAD DEL ARTICULO EN DETALLE
    $scope.ActualizarCantidadEnArticulo = function (nuevaCantidad, indice) {
  
        if (nuevaCantidad == null || nuevaCantidad == undefined) {
            nuevaCantidad = 1;
        } else if(nuevaCantidad < 1){
            nuevaCantidad = 1;
        }

        $scope.ListaItemDetalle[indice].Cantidad = nuevaCantidad;//asignando nueva cantidad
        
        //recalculando impuesto por total de articulos
        var impuestoArticulo = (parseFloat($scope.ListaItemDetalle[indice].ValorImpuesto) * parseFloat($scope.ListaItemDetalle[indice].Precio)) / 100;
        
        $scope.ListaItemDetalle[indice].IVA = (parseFloat(impuestoArticulo) * parseFloat(nuevaCantidad)).toFixed(4);
        //recalculando total por linea
        
        var ttl = parseFloat($scope.ListaItemDetalle[indice].Cantidad) * parseFloat($scope.ListaItemDetalle[indice].Precio) + parseFloat($scope.ListaItemDetalle[indice].IVA);
        var total = (ttl * 1).toFixed(4);
        
        $scope.ListaItemDetalle[indice].Total = total;//$scope.ListaItemDetalle[indice].Cantidad * $scope.ListaItemDetalle[indice].Precio + $scope.ListaItemDetalle[indice].IVA;
        
        $scope.ActualizarTotalAPagar();//actualizarTotal a Pagar
        $scope.ActualizarTotalUnidades();
    }

    $scope.ActualizarDescuento = function (nuevoDescuento, indice) {
        
        if (nuevoDescuento == null || nuevoDescuento == undefined) {
            nuevoDescuento = 0;
        } else if (nuevoDescuento < 0) {
            nuevoDescuento = 0;
        } else if (nuevoDescuento > 100) {
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

        //var totalDescuento2 = 0;
        //var desc = 100 - parseFloat($scope.customerBP.Descuento);
        //var descProm = 100 - nuevoDescuento;
        //var promDesc = (desc * descProm) / 100;
        //totalDescuento2 = 100 - promDesc;
        //var importeDescuento = (parseFloat($scope.ListaItemDetalle[indice].PrecioUnitario) * parseFloat(totalDescuento2)) / 100;
        //var priceItemWithoutDiscount = parseFloat($scope.ListaItemDetalle[indice].PrecioUnitario) - importeDescuento;

        //$scope.ListaItemDetalle[indice].PrecioConDescuento = priceItemWithoutDiscount;
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
        var TotalImpuestoDeLinea1 = NuevoImporteImpuesto1;//parseInt($scope.ListaItemDetalle[indice].Cantidad) * NuevoImporteImpuesto;
        var TotalDeLineaSinIVA1 = parseInt($scope.ListaItemDetalle[indice].Cantidad) * PrecioArticuloConDescuento1;

        $scope.ListaItemDetalle[indice].Precio = parseFloat(PrecioArticuloConDescuento1);
        $scope.ListaItemDetalle[indice].IVA = NuevoImporteImpuesto1.toFixed(4);
        $scope.ListaItemDetalle[indice].Total = (TotalDeLineaSinIVA1 + TotalImpuestoDeLinea1).toFixed(4);

        $scope.ActualizarTotalAPagar();//actualizarTotal a Pagar
        $scope.ActualizarTotalUnidades();
    };
    //ACTUALIZA EL PRECIO DEL ARTICULO
    $scope.ActualizarPrecioArticuloDetalle = function (nuevoPrecio, indice) {

        //asignando nuevo precio al articulo
        $scope.ListaItemDetalle[indice].Precio = parseFloat(nuevoPrecio);
        //calcular impuesto por articulo
        $scope.nuevoIvaUnitario = (parseFloat($scope.Impuesto) * parseFloat(nuevoPrecio)) / 100;
        //asignando nuevo iva total por linea
        $scope.ListaItemDetalle[indice].IVA = $scope.nuevoIvaUnitario * $scope.ListaItemDetalle[indice].Cantidad;
        //calculando nuevo total por linea
        $scope.nuevoPrecioMasIVA = $scope.ListaItemDetalle[indice].IVA + parseFloat(nuevoPrecio)*$scope.ListaItemDetalle[indice].Cantidad;
        //asignando nuevo total al linea detalle
        $scope.ListaItemDetalle[indice].Total = $scope.nuevoPrecioMasIVA;

        $scope.ActualizarTotalAPagar();//actualizarTotal a Pagar
        $scope.ActualizarTotalUnidades();
    }


    //ACTUALIZA LA FECHA DE ENTREGA EN EL ARTICULO DEL DETALLE
    $scope.ActFechaEntregaDetalle = function (nuevaFecha, indice) {
        //asigna nuevo objeto de fecha
        $scope.ListaItemDetalle[indice].FechaEntrega = nuevaFecha;
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
    $scope.ActualizarFechaDelPedido = function (nuevaFechaPedido) {
        $scope.FechaSistema = nuevaFechaPedido;
    }
    //REMUEVE EL ARTICULO SELECIONADO DEL DETALLE
    $scope.RemoverArticuloEnDetalle = function (indice){
        
        //remueve articulo en la posicion indicada
        $scope.ListaItemDetalle.splice(indice, 1);
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

        $log.debug("Precio mas IVA" + $scope.precioMasIva);

        var PrecioArticulo = articulo.Price;//precio auxiliar
        var ValorDeDescuento = (parseFloat($scope.customerBP.Descuento) * parseFloat(PrecioArticulo)) / 100; //precio calculado con descuento
        var PrecioArticuloConDescuento = parseFloat(PrecioArticulo) - ValorDeDescuento;
        var Impuesto = $scope.Impuesto;

        $log.debug("Precio articulo con descuento " + PrecioArticuloConDescuento);

        var NuevoImporteImpuesto = (PrecioArticuloConDescuento * $scope.Impuesto) / 100;
        NuevoImporteImpuesto = parseFloat(NuevoImporteImpuesto) * 1;
        var TotalImpuestoDeLinea = NuevoImporteImpuesto;//parseInt($scope.ListaItemDetalle[indice].Cantidad) * NuevoImporteImpuesto;
        var TotalDeLineaSinIVA = parseInt(1) * PrecioArticuloConDescuento;

        $log.debug("Nuevo importe " + NuevoImporteImpuesto);

        //$scope.ListaItemDetalle[indice].Precio = parseFloat(PrecioArticuloConDescuento);
       // $scope.ListaItemDetalle[indice].IVA = NuevoImporteImpuesto.toFixed(4);
        //$scope.ListaItemDetalle[indice].Total = (TotalDeLineaSinIVA + TotalImpuestoDeLinea).toFixed(4);
        
        var row = $scope.ListaItemDetalle.length <= 0 ? 0 : $scope.ListaItemDetalle.length

        var totalDescuento = 0;
        var desc1 = 100 - parseFloat($scope.customerBP.Descuento);
        var desc2 = 100 - 0;
        var promDesc = (desc1 * desc2) / 100;
        totalDescuento = 100 - promDesc;
        var importeDescuento = (parseFloat(articulo.Price) * parseFloat(totalDescuento))/100;
        var priceItemWithoutDiscount = parseFloat(articulo.Price) - importeDescuento;
        
        //Añadir articulo a la lista
        $scope.ListaItemDetalle.push({
            Codigo: articulo.ItemCode,
            Nombre: articulo.ItemName,
            Cantidad: 1,
            Precio: parseFloat(PrecioArticuloConDescuento),//articulo.Price,
            PrecioLinea: 0.0,
            SujetoImpuesto:articulo.VATLiable,
            PrecioUnitario: articulo.Price,
            Descuento: 0.0,
            PrtgeDisc: 0,
            PrecioConDescuento: priceItemWithoutDiscount,
            ValorImpuesto: articulo.VATLiable == 'Y' ? valorImpuesto : 0,//porcentual
            CodigoVAT: articulo.TaxCodeAR,
            IVA: articulo.VATLiable == 'Y' ? NuevoImporteImpuesto.toFixed(4) : 0, 
            Total: articulo.VATLiable == 'Y' ? (TotalDeLineaSinIVA + TotalImpuestoDeLinea).toFixed(4) : TotalDeLineaSinIVA.toFixed(4),
            FechaEntrega: $scope.FechaSistema,
            MetrosExtra: "METROS_EXTRA",
            NumberLine: row
        });

        //calculo de total a pagar
        $scope.TotalAPagar = 0;
        $scope.SubttlAPagar = 0;
        $scope.TotalEnImpuesto = 0;

        for (var i = 0; i < $scope.ListaItemDetalle.length; i++) {
            $scope.SubttlAPagar = $scope.SubttlAPagar + parseFloat($scope.ListaItemDetalle[i].Total);
            $scope.TotalEnImpuesto = $scope.TotalEnImpuesto + parseFloat($scope.ListaItemDetalle[i].IVA);
        }

        $scope.TotalEnImpuestoStr = formatThousandsWithRounding($scope.TotalEnImpuesto, 4);


        $scope.TotalAPagar = parseFloat($scope.SubttlAPagar);// - parseFloat(PctjeDiscount);
        var cantidadFormateadaSbttl = formatThousandsWithRounding($scope.SubttlAPagar, 4);
        $scope.SbttlAPagarDisplay = cantidadFormateadaSbttl;
        
        var cantidadFormateadaTtl = formatThousandsWithRounding($scope.TotalAPagar, 4);
        $scope.TotalAPagarDisplay = cantidadFormateadaTtl;

        //calculo de total de unidades
        $scope.TotalUnidades = 0;
        for (var i = 0; i < $scope.ListaItemDetalle.length; i++) {
            $scope.TotalUnidades = parseInt($scope.TotalUnidades) + parseInt($scope.ListaItemDetalle[i].Cantidad);
        }        
    }

    //ACTUALIZA EL TOTAL DEL DOCUMENTO
    $scope.ActualizarTotalAPagar = function () {
       
        $scope.TotalEnImpuesto = 0;
        $scope.TotalAPagar = 0;
        $scope.SubttlAPagar = 0;

        for (var i = 0; i < $scope.ListaItemDetalle.length; i++) {
            $scope.SubttlAPagar = parseFloat($scope.SubttlAPagar) + parseFloat($scope.ListaItemDetalle[i].Total);
            $scope.TotalEnImpuesto = parseFloat($scope.TotalEnImpuesto) + parseFloat($scope.ListaItemDetalle[i].IVA);
        }
        
        $scope.TotalEnImpuestoStr = formatThousandsWithRounding($scope.TotalEnImpuesto,4);

        $scope.TotalAPagar = $scope.SubttlAPagar;// - PrtgeDiscount;

        var cantidadConFormato = formatThousandsWithRounding($scope.SubttlAPagar, 4);
        $scope.SbttlAPagarDisplay = cantidadConFormato;

        var totalConFormato = formatThousandsWithRounding($scope.TotalAPagar, 4);
        $scope.TotalAPagarDisplay = totalConFormato;
    }

    $scope.ActualizarTotalUnidades = function () {

        $scope.TotalUnidades = 0;
        for (var i = 0; i < $scope.ListaItemDetalle.length; i++) {
            $scope.TotalUnidades = parseInt($scope.TotalUnidades) + parseInt($scope.ListaItemDetalle[i].Cantidad);
        }
    }

    //forma el JSON que se enviará para integrar el pedido: encabezado y detalle
    $scope.ExtraerJsonDetalle = function () {

        //Obtner el detalle del pedido a partir de ListaItemDetalle
        for (var linea = 0; linea < $scope.ListaItemDetalle.length; linea++) {

            //DETALLE: agregamos al detalle del pedido
            $scope.JsonPedido.DocumentLines.push({
                ItemCode: $scope.ListaItemDetalle[linea].Codigo,
                ItemDescription: $scope.ListaItemDetalle[linea].Nombre,
                Precio: $scope.ListaItemDetalle[linea].Precio,
                PrecioLinea: $scope.ListaItemDetalle[linea].PrecioLinea,
                PrecioUnitario: $scope.ListaItemDetalle[linea].PrecioUnitario,
                Quantity: $scope.ListaItemDetalle[linea].Cantidad,
                Currency: $scope.MonedaDelSN.Moneda,
                PrtgDiscount: $scope.ListaItemDetalle[linea].PrtgeDisc,
                VatGroup: $scope.ListaItemDetalle[linea].CodigoVAT,
                Rate: "",
                WarehouseCode: $scope.WareHoseCode,
                MetrosExtra: $scope.ListaItemDetalle[linea].MetrosExtra,
                SalesPersonCode: -1,
                LineStatus: "O",
                TaxCode: ""
                //ShipDate: shipDate
            });
        }

        //descomponer fecha en formato aceptable
        var mesOrden = $scope.FechaSistema.getMonth() + 1;
        var diaOrden = $scope.FechaSistema.getDate();
        var fechaDeOrden = $scope.FechaSistema.getFullYear() + '-' + (mesOrden < 10 ? '0' : '') + mesOrden + '-' + (diaOrden < 10 ? '0' : '') + diaOrden;
        
        
        //ENCABEZADO: formando el json del pedido
        $scope.JsonPedido.DocDate = fechaDeOrden;
        $scope.JsonPedido.DocDueDate = fechaDeOrden;
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
    }

    function ObtenerAlmacen() {
        Servicio.ObtenerAlmacenDefault().then(function (respuesta) {
            if (respuesta != null) {
                $scope.WareHoseCode = respuesta;
            } else {
                $scope.WareHoseCode = "COMGEN"
            }
        })
    }
    //OBTIENEN LAS TAZAS DE CAMBIO
     function ObtenerTsaDeCambio() {

        var fecha = new Date();
        var mes = fecha.getMonth() + 1;
        var dia = fecha.getDate();
        var fechaCompleta = fecha.getFullYear() + '-' + (mes < 10 ? '0' : '') + mes + '-' + (dia < 10 ? '0' : '') + dia;
         $log.debug("Fecha " + fechaCompleta);
        Servicio.ObtenerTasasDeCambioSAP(fechaCompleta).then(function (respuestaTasas) {
            if (respuestaTasas != null) {
                for (var i = 0; i < respuestaTasas.length; i++) {
                    $scope.TasasDeCambio.push({
                        RateDate: respuestaTasas[i].RateDate,
                        Currency: respuestaTasas[i].Currency,
                        Rate: respuestaTasas[i].Rate
                    })
                }

            }
            if ($scope.TasasDeCambio.length <= 0) {
                growl.warning("Las Tasas de Cambio No Estan Actualizadas", { title: 'Atención!' });
            }
        });    
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
    $scope.BorrarDetallePedido = function () {
        $scope.ListaItemDetalle = [];
        $scope.JsonPedido.DocumentLines = [];
        $scope.ActualizarTotalAPagar();
        $scope.ActualizarTotalUnidades();
    };

    $scope.EnviarOrdenASAP = function () {

        

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

        if (($scope.customerBP.CardCode == "" || $scope.customerBP.CardCode == undefined))
        {
            growl.error('No ha completado el formulario.', { title: 'Error!' });
            return;
        }
        //if ($scope.NumPedido == "") {
        //    growl.error('No se ha indicado el Número del Pedido.', { title: 'Error!' });
        //    return;
        //}
        if ($scope.FechaSistema == "" || $scope.FechaSistema == null || $scope.FechaSistema == undefined) {
            growl.error('No se ha establecido la Fecha del Pedido.', { title: 'Error!' });
        }
        else if ($scope.FechaAtrasada() == false) {
            growl.error('Fecha del Pedido no Válida', { title: 'Error!' });
        }
        else if ($scope.FechaAtrasada() != false) {
            $scope.ExtraerJsonDetalle();

            if ($scope.JsonPedido.DocumentLines.length <= 0) {
                growl.error('No ha completado el formulario.', { title: 'Error!' });
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
            console.log(JSON.stringify($scope.JsonPedido));
            Servicio.IntegrarOrdenSAP(JSON.stringify($scope.JsonPedido)).then(function (respuestaDeIntegracion) {
                clearInterval(mySet);
                dialog.hide();
                if (respuestaDeIntegracion != undefined) {
                    $("#postOrderSAP").prop('disabled', false);
                    $("#openBtnPreview").prop('disabled', false);
                    if (respuestaDeIntegracion.codigo == 0) {
                        //clearInterval(waitForResponse);
                        growl.success("Número de Documento en SAP: "+respuestaDeIntegracion.mensaje, { title: 'Orden de Venta Generada!' });
                        $scope.ReporteImpresionPedido(parseInt(respuestaDeIntegracion.mensaje),'DocNum');
                        //$scope.JsonPedido.DocumentLines = [];
                        $scope.LimpiarFormularioPedido();
                    } else {
                        //clearInterval(waitForResponse);
                        growl.error(respuestaDeIntegracion.mensaje, { title: 'Error!' });
                        $scope.JsonPedido.DocumentLines = [];
                    }
                } else {
                    $("#postOrderSAP").prop('disabled', false);
                    $("#openBtnPreview").prop('disabled', false);
                    //clearInterval(waitForResponse);
                    growl.error("No se obtuvo respuesta de Integración", { title: 'Error!' });
                    $scope.JsonPedido.DocumentLines = [];
                }
                
            })
        }
    };

    $scope.FechaAtrasada = function () {

        $scope.FechaActual = new Date();
        var fechaActual = new Date();
        var yearActual = fechaActual.getFullYear();
        var mesActual = fechaActual.getMonth() + 1;
        var diaActual = fechaActual.getDate();
        var yearPedido;
        var mesPedido;
        var diaPedido;

        if ($scope.FechaSistema != "" || $scope.FechaSistema != null || $scope.FechaSistema != undefined) {
            yearPedido = $scope.FechaSistema.getFullYear();
            mesPedido = $scope.FechaSistema.getMonth() + 1;
            diaPedido = $scope.FechaSistema.getDate();
        }

        if (yearPedido < yearActual) {
            return false;
        }
        else if ((mesPedido < mesActual) && (yearPedido >= yearActual)) {
            return false;
        }
        else if ((diaPedido < diaActual) && (mesPedido == mesActual) && (yearPedido >= yearActual)) {
            return false;
        }
        else if ((diaPedido >= diaActual) && (mesPedido >= mesActual) && (yearPedido >= yearActual)) {
            return true;
        }
    };

    $scope.ReporteImpresionPedido = function (docEntry, tipo) {
            $('<form>', {
                "method": "POST",
                "action": Ruta.path + '/IntegracionSap/ImprimirPedido?docEntrySAP=' + docEntry+"&tipo="+tipo,
                "target": "_blank"
            }).appendTo(document.body).submit();
    };

    $scope.ImprimirVariosPedidos = function () {

        $('<form>', {
            "method": "POST",
            "action": Ruta.path + '/IntegracionSap/GenerarImpresion?fi=' + '01/01/2018' + '&ff=' + '26/02/2018' + '&e=' + 'O',
            "target": "_blank"
        }).appendTo(document.body).submit();
    }


    $scope.LimpiarFormularioPedido = function () {

        //vaciado de modelos
     /*   $scope.customerBP = {};*/
        $scope.NumPedido = "";
        
        $scope.ComentariosPedido = "";
        $scope.JsonPedido.DocDate = "";
        $scope.JsonPedido.DocDueDate = "";
        $scope.JsonPedido.U_NumPed = "";
       /* $scope.JsonPedido.CardCode = "";*/
       /* $scope.JsonPedido.CardName = "";*/
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
        $scope.FechaSistema = "";
        $scope.ActualizarTotalAPagar();
        $scope.ActualizarTotalUnidades();
        $scope.SocioDeNegocios();
    };
    //se modifico esta linea
    // en el css: growl-container.growl-fixed.top-right 10 y 15 px respect
    function AlertadeTasasDeCambio(mensaje) {
        $scope.showWarning = function () {
            growl.warning("Sin tasas de Cambio", { title: 'Atención!' });
        }
    };



    $scope.showWarning = function () {
        growl.warning("hola", { title: 'Warning!' });
    }
    $scope.showError = function () {
        growl.error('This is error message.', { title: 'Error!' });
    }
    $scope.showSuccess = function () {
        growl.success('This is success message.', { title: 'Success!' });
    }
    $scope.showInfo = function () {
        growl.info('This is an info message.', { title: 'Info!' });
    }

});


function formatThousandsWithRounding(cantidad, decimales) {
    
    var w = cantidad.toFixed(decimales), k = w | 0, b = cantidad < 0 ? 1 : 0,
        u = Math.abs(w - k), d = ('' + u.toFixed(decimales)).substr(2, decimales),
        s = '' + k, i = s.length, r = '';
    while ((i -= 3) > b) { r = ',' + s.substr(i, 3) + r; }
    return s.substr(0, i + 3) + r + (d ? '.' + d : '');
};

//Directiva para input calendario
appTab.directive('jqdatepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            element.datepicker({
                dateFormat: "dd/mm/yy",
                onSelect: function (date) {
                    scope.date = date;
                    scope.$apply();
                }
            });
        }
    };
});