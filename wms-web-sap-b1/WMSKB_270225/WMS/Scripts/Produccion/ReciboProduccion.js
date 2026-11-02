var reciboProdApp = angular.module('reciboProdApp', ['angular-growl', 'ui.bootstrap', 'xeditable']);

reciboProdApp.factory('Ruta', function () {
    return { path: '' }
});

reciboProdApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

reciboProdApp.service('Servicio', function ($http, Ruta, $log) {
    this.ObtenerDetalle = function (_qr) {
        return $http({
            url: Ruta.path + '/Produccion/ValidarProd',
            method: 'POST',
            data: {
                QR: _qr
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 1 ": response } }
            return error;
        });
    },
    this.Actualizar = function (Lineas) {
        return $http({
            url: Ruta.path + '/Produccion/ActualizarOrden',
            method: 'POST',
            data: {
                ListaAct: Lineas
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function errorCallback(response) {
            var error = { codigo: response.status, mensaje: response.statusText }
            return error;
        });
    }
});

reciboProdApp.controller('reciboProdController', function ($scope, $window, Ruta, $log, Servicio, $uibModal, growl) {
    $scope.ReciboProd = {
        Num: '',
        Lineas: []
    }
    $scope.Lectura = {
        Etiqueta: ''
    }
    $scope.Memoria = {
        Procesando: false,
        LineasOrden: []
    }

    $scope.LecturaQR = function () {
        if ($scope.ExisteEnTabla($scope.Lectura.Etiqueta)) {
            $scope.Alertas('e', 'La etiqueta ya fue leída');
            $scope.Lectura.Etiqueta = '';
            $("#Entradas").focus();
        }
        else {
            Servicio.ObtenerDetalle($scope.Lectura.Etiqueta).then(function (response) {
                if (response.codigo == 0) {

                    if (response.mensaje[0].Cancelado == 'Y') {
                        $scope.Alertas('e', 'La etiqueta fue cancelada');
                        $scope.Lectura.Etiqueta = '';
                        return;
                    }

                    if (response.mensaje[0].U_stCal == "X") {
                        $scope.Alertas('e', 'La etiqueta esta BLOQUEADA');
                        $scope.Lectura.Etiqueta = '';
                        $("#Entradas").focus();
                    }
                    else {
                        if (!$scope.EstadoEtiqueta(response.mensaje[0].U_stEti)) {
                            $scope.ReciboProd.Lineas.push(response.mensaje[0]);
                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();
                        }
                    }                    
                }
                else {
                    $scope.Alertas('e', 'No se recupero información de la etiqueta');
                    $scope.Lectura.Etiqueta = '';
                }
            });
        }
    }

    $scope.GenerarReciboProd = function () {
        $scope.Memoria.Procesando = true;
        Servicio.Actualizar($scope.ReciboProd.Lineas).then(function (response) {
            if (response.codigo == 0) {
                $scope.Alertas('s', response.mensaje);
                $scope.Limpiar();
                $scope.Memoria.Procesando = false;
            }
            else {
                $scope.Alertas('e', response.mensaje);
                $scope.Memoria.Procesando = false;
            }
        })
    }
    $scope.Eliminar = function (i) {
        $scope.ReciboProd.Lineas.splice(i, 1);
    }
    $scope.Limpiar = function () {
        $scope.ReciboProd = {
            Num: '',
            Lineas: []
        }
        $scope.Lectura = {
            Etiqueta: ''
        }
        $scope.Memoria = {
            Procesando: false,
            LineasOrden: []
        }
    }

    //validaciones
    $scope.ExisteEnDoc = function (qr) {
        var existe = -1;
        for (var n = 0; n < $scope.Memoria.LineasOrden.length; n++) {
            if ($scope.Memoria.LineasOrden[n].Name == qr.split('&')[0]) {
                existe = n;
                break;
            }
        }
        return existe;
    }
    $scope.ExisteEnTabla = function (qr) {
        var existe = false;
        for (var n = 0; n < $scope.ReciboProd.Lineas.length; n++) {
            if ($scope.ReciboProd.Lineas[n].Name == qr.split('&')[0]) {
                existe = true;
                break;
            }
        }
        return existe;
    }
    $scope.EstadoEtiqueta = function (indice) {
        //switch ($scope.Memoria.LineasOrden[indice].U_stEti) {
        switch (indice) {
            case 'L':
                $scope.Alertas('e', 'La etiqueta ya fue LIBERADA');
                $scope.Lectura.Etiqueta = '';
                return true;
            case 'R':
                $scope.Alertas('e', 'La etiqueta ya fue REGISTRADA');
                $scope.Lectura.Etiqueta = '';
                return true;
            case 'E':
                $scope.Alertas('e', 'La etiqueta ya fue EMBARCADA');
                $scope.Lectura.Etiqueta = '';
                return true;
            default:
                return false;
        }
        
    }

    //mensajes
    $scope.Alertas = function (Tipo, Texto) {
        switch (Tipo) {
            case 'w':
                growl.warning(Texto, { title: 'Aviso Sistema' });
                break;
            case 'i':
                growl.info(Texto, { title: 'Aviso Sistema' });
                break;
            case 's':
                growl.success(Texto, { title: 'Aviso Sistema' });
                break;
            case 'e':
                growl.error(Texto, { title: 'Aviso Sistema' });
                break;
        }
    }
});