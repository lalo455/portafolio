var appConsulta = angular.module('appConsulta', ['angular-growl', 'ui.bootstrap', 'xeditable']);

appConsulta.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

appConsulta.factory('Ruta', function () {
    return { path: '' }
});

appConsulta.service('Servicio', function ($http, Ruta, $log) {
    this.GrabarLectura = function (etq, nlectura) {
        return $http({
            url: Ruta.path + '/Consulta/GrabarLectura',
            method: 'POST',
            data: {
                Etiqueta: etq,
                Nlectura: nlectura
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 12 ": response } }
            return error;
        });
    },

    this.VEtqLiberada = function (Etq) {
        return $http({
            url: Ruta.path + '/Consulta/VerificarEtq',
            method: 'POST',
            data: {
                QR: Etq
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 4 ": response } }
            return error;
        });
    },

    this.ObtenerLote = function (Lectura, Tipo) {
        return $http({
            url: Ruta.path + '/Consulta/ObtenerLote',
            method: 'POST',
            data: {
                Lectura: Lectura
                , Tipo: Tipo
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 2 ": response } }
            return error;
        });
    }

});

appConsulta.controller('appConsulta', function ($scope, $window, Ruta, $log, Servicio, growl, $uibModal) {
    $scope.Lectura = {
        Etiqueta: '',
        Codigo: '',
        Movimiento: false,
        Lineas: [], 
        Tipo: '',
        Nlectura: 1
    }
    //********************************************************************************************/
    $scope.Inicial = function () {
        $scope.Lectura.Tipo = "Atm";
        $scope.Lectura.Nlectura = 1;

        console.log($scope.Lectura.Tipo);
    }
    
    //********************************************************************************************/
    //Tipo de Lectura
    $scope.CambiarLectura = function (tipo) {
        console.log('Cambio de lectura: ' + tipo);
        $scope.Lectura.Tipo = tipo;

        switch ($scope.Lectura.Tipo) {
            case "Atm":
                $scope.Lectura.Etiqueta = '';
                $scope.Lectura.Codigo = '';
                $("#Entradas").focus();
                break;

            case "Manual":
                $scope.Lectura.Etiqueta = '';
                $scope.Lectura.Codigo = '';
                $("#Entradas2").focus();
                break;
        }
    }

    //********************************************************************************************/
    //Lectura de Etiqueta | Lectura de Lote o Artículo
    $scope.Lectura = function () {
        $scope.Lectura.Lineas = [];
        console.log($scope.Lectura.Nlectura);

        Servicio.GrabarLectura($scope.Lectura.Etiqueta, $scope.Lectura.Nlectura).then(function (response) {
            console.log(response.mensaje.length + ' --- # ' + $scope.Lectura.Nlectura);
            $scope.Lectura.Nlectura++;
        });

        //Validar cadena: 7 segmentos OK, menos FAIL, más de 8 FAIL
        if ($scope.Lectura.Tipo === "Atm" && ($scope.Lectura.Etiqueta.split('&').length < 7 || $scope.Lectura.Etiqueta.split('&').length > 8)) {
            $scope.Alertas('w', 'Lectura Erronea, vuelva a escaner etiqueta');

            $scope.Lectura.Etiqueta = '';
            $scope.Lectura.Codigo = '';
            $("#Entradas").focus();
            return;
        }

        $scope.Lectura.Movimiento = true;
        switch ($scope.Lectura.Tipo) {
            case "Atm":
                Servicio.VEtqLiberada($scope.Lectura.Etiqueta).then(function (_response) {
                    switch (_response.codigo) {
                        case -2:
                            $scope.Alertas('e', _response.mensaje);
                            $scope.Lectura.Etiqueta = '';
                            $scope.Lectura.Movimiento = false;
                            break;

                        case -1:
                            $scope.Alertas('w', _response.mensaje);
                            $scope.Lectura.Etiqueta = '';
                            $scope.Lectura.Movimiento = false;
                            break;

                        case 0:
                            Servicio.ObtenerLote($scope.Lectura.Etiqueta, $scope.Lectura.Tipo).then(function (response_) {
                                switch (response_.codigo) {
                                    case 0:
                                        $scope.AgregarLinea(response_.mensaje);
                                        break;

                                    default:
                                        $scope.Alertas('e', response_.mensaje);
                                        $scope.Lectura.Etiqueta = '';
                                        $scope.Lectura.Codigo = '';
                                        $("#Entradas").focus();
                                        $scope.Lectura.Movimiento = false;
                                        break
                                }
                            });
                            break;
                    }
                });
                break;

            case "Manual":
                Servicio.ObtenerLote($scope.Lectura.Codigo, $scope.Lectura.Tipo).then(function (_response) {
                    switch (_response.codigo) {
                        case 0:
                            $scope.AgregarLinea(_response.mensaje);
                            break;

                        default:
                            $scope.Alertas('e', _response.mensaje);
                            $scope.Lectura.Etiqueta = '';
                            $scope.Lectura.Codigo = '';
                            $("#Entradas2").focus();
                            $scope.Lectura.Movimiento = false;
                            break
                    }
                });
                break;
        }
    }

    $scope.AgregarLinea = function (ListaL) {
        switch ($scope.Lectura.Tipo) {
            case "Atm":
                for (var i = 0; i < ListaL.length; i++) {

                    Object.assign(ListaL[i], {
                        "QtySAP": ListaL[i].OnHandQty
                        , "Dispositivo": $scope.Lectura.Etiqueta.split('&')[4]
                        , "PesoBruto": $scope.Lectura.Etiqueta.split('&')[5].replace(',', '')
                        , "PesoNeto": $scope.Lectura.Etiqueta.split('&')[6].replace(',', '')
                        , "QtyEtq": $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                    });

                    switch (ListaL[i].Estatus) {
                        case "0":
                            ListaL[i].Estatus = "Liberado";
                            break;

                        case "1":
                            ListaL[i].Estatus = "No Accesible";
                            break;

                        case "2":
                            ListaL[i].Estatus = "Bloqueado";
                            break;
                    }

                    $scope.Lectura.Lineas.push(ListaL[i]);
                }

                $scope.Lectura.Etiqueta = '';
                $scope.Lectura.Codigo = '';
                $("#Entradas").focus();
                $scope.Lectura.Movimiento = false;
                break;

            case "Manual":
                for (var i = 0; i < ListaL.length; i++) {

                    Object.assign(ListaL[i], {
                        "QtySAP": ListaL[i].OnHandQty
                    })

                    switch (ListaL[i].Estatus) {
                        case "0":
                            ListaL[i].Estatus = "Liberado";
                            break;

                        case "1":
                            ListaL[i].Estatus = "No Accesible";
                            break;

                        case "2":
                            ListaL[i].Estatus = "Bloqueado";
                            break;
                    }

                    $scope.Lectura.Lineas.push(ListaL[i]);
                }

                $scope.Lectura.Etiqueta = '';
                $scope.Lectura.Codigo = '';
                $("#Entradas2").focus();
                $scope.Lectura.Movimiento = false;
                break;
        }

        console.log('Lista de registros >>>>>>>>>>>>>>>');
        console.log($scope.Lectura.Lineas);
    }

    //********************************************************************************************/
    //Alertas
    $scope.Alertas = function (Tipo, Texto) {
        switch (Tipo) {
            case 'w':
                growl.warning(Texto, { title: '¡Advertencia!' });
                break;
            case 'i':
                growl.info(Texto, { title: 'Información' });
                break;
            case 's':
                growl.success(Texto, { title: ' - Completado - ' });
                break;
            case 'e':
                growl.error(Texto, { title: '¡¡Error!!' });
                break;
        }
    }

    //Iniciales
    $scope.Inicial();
});