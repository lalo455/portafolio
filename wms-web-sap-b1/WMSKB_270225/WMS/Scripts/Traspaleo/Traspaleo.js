var traspaleoApp = angular.module('traspaleoApp', ['angular-growl', 'ui.bootstrap', 'xeditable']);

traspaleoApp.factory('Ruta', function () {
    return { path: '' }
});

traspaleoApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

traspaleoApp.service('Servicio', function ($http, Ruta, $log) {
    this.Etiqueta = function (qr) {
        return $http({
            url: Ruta.path + '/Traspaleo/Etiqueta',
            method: 'POST',
            data: {
                QR: qr
            },
            headers: {
                'content-type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "error en servicio 1 ": response } }
            return error;
        });
    },
    this.Crear = function (doc) {
        return $http({
            url: Ruta.path + '/Traspaleo/Crear',
            method: 'POST',
            data: {
                Lineas: doc
            },
            headers: {
                'content-type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function errorCallback(response) {
            var error = { codigo: response.status, mensaje: response.statusText }
            return error;
        });
    }, 
    this.GrabarLectura = function (etq, nlectura) {
        return $http({
            url: Ruta.path + '/Traspaleo/GrabarLectura',
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
    }
});

traspaleoApp.controller('traspaleoController', function ($scope, $window, Ruta, $log, Servicio, $uibModal, growl) {
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

    $scope.Traspaleo = {
        Lineas: []
    }

    $scope.Lectura = {
        Etiqueta: '',
        Movimiento: false
    }

    $scope.Memoria = {
        Nlectura: 1
    }

    //funciones
    $scope.LecturaQR = function () {
        //Grabar informacion
        Servicio.GrabarLectura($scope.Lectura.Etiqueta, $scope.Memoria.Nlectura).then(function (response) //Revisa el estatus de la etiqueta 
        {
            console.log(response.mensaje.length + ' --- # ' + $scope.Memoria.Nlectura);
            $scope.Memoria.Nlectura++;
        });

        //Verificar el tipo de etiqueta permitida: L, U - Actualizacion 03-02-2021
        if ($scope.EsPermitida($scope.Lectura.Etiqueta)) //$scope.Lectura.Etiqueta.match("^L")
        {
            //Verifica que no haya sido leida con anterioridad
            if ($scope.Validar('E', $scope.Lectura.Etiqueta.split('&')[0], '') !== -1)
            {
                $scope.Alertas('e', 'La ETIQUETA leida YA EXISTE');
                $scope.Lectura.Etiqueta = '';
            }
            else
            {
                //if ($scope.Validar('P', $scope.Lectura.Etiqueta.split('&')[1], '')) {//No existe pero se valida que el articulo sea igual que el primer registro
                Servicio.Etiqueta($scope.Lectura.Etiqueta.split('&')[0]).then(function (response) {
                    switch (response.codigo) {
                        case 0://Correcto
                            //Verificar que el ALMACEN sea 02 - Update 17-08-2021 IUO - Candado para evitar Lotes de Almacen Distinto al 02 para Traspaleo
                            if (response.mensaje[0].WhsCode === 'APT00007' || response.mensaje[0].WhsCode === 'AIP00013' || response.mensaje[0].WhsCode === 'AI-00005' ) {
                                $scope.Traspaleo.Lineas.push(response.mensaje[0]);
                            }
                            else {
                                $scope.Alertas('e', 'El Lote ' + $scope.Lectura.Etiqueta.split('&')[0] + ' se encuentra en almacen ' + response.mensaje[0].WhsCode + ', solo se acepta Almacen APT00007, AIP00013, AI-00005');
                            }
                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();
                            break;

                        case -1://Error
                            $scope.Alertas('e', response.mensaje);
                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();
                            break;

                        case -2://Warning
                            $scope.Alertas('w', response.mensaje);
                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();
                            break;

                        default://desconocido
                            $scope.Alertas('e', response.mensaje);
                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();
                            break;
                    }
                });
            }
        }
        else
        {
            //$scope.Alertas('e', 'SOLO se permiten ETIQUETAS de PT');
            $scope.Alertas('e', 'La etiqueta NO es permitida.');
            $scope.Lectura.Etiqueta = '';
        }
    }

    $scope.Crear = function () {
        //Recorrer la lista de Lotes de Traspaleo
        for (var e = 0; e < $scope.Traspaleo.Lineas.length; e++) {
            var lote = $scope.Traspaleo.Lineas[e].Name;
            var unico = e;

            //Por cada uno de los valores buscar y eliminar lotes repetidos en la misma Lista
            for (var h = $scope.Traspaleo.Lineas.length - 1; h >= 0; h--) {
                if (h !== unico && $scope.Traspaleo.Lineas[h].Name === lote) {
                    console.log('Lote: ' + lote + ' -- Posicion: ' + h);
                    $scope.Traspaleo.Lineas.splice(h, 1);
                }
            }
        }        

        if ($scope.Traspaleo.Lineas.length > 0) {
            $scope.Lectura.Movimiento = true;
            Servicio.Crear($scope.Traspaleo.Lineas).then(function (response) {
                switch (response.codigo) {
                    case 0:
                        $scope.Alertas('s', response.mensaje);
                        $scope.Lectura.Movimiento = false;
                        $scope.Reiniciar();
                        return;
                    case -1:
                        $scope.Alertas('e', response.mensaje);
                        $scope.Lectura.Movimiento = false;
                        return;
                }
            });
        }
        else {
            $scope.Alertas('s', "Debe leer al menos una etiqueta");
        }
    }

    $scope.Eliminar = function (i) {
        $scope.Traspaleo.Lineas.splice(i, 1);

        if ($scope.Traspaleo.Lineas.length === 0) {
            $scope.Memoria.Nlectura = 1;
        }        
    }

    $scope.Reiniciar = function () {
        $scope.Traspaleo =  {
            Lineas: []
        }
        $scope.Lectura = {
            Etiqueta: '',
            Movimiento: false
        }

        $scope.Memoria = {
            Nlectura: 1
        }
    }

    //Validaciones
    $scope.Validar = function (tipo, param1, param2) {
        switch (tipo) {
            case 'E':
                var result = -1;
                for (var e = 0; e < $scope.Traspaleo.Lineas.length; e++) {
                    if ($scope.Traspaleo.Lineas[e].Name == param1) {
                        result = e;
                        break;
                    }
                }
                return result;

            case 'P':
                var result = false;
                if ($scope.Traspaleo.Lineas.length > 0) {
                    if ($scope.Traspaleo.Lineas[0].U_ItemCode == param1) {
                        result = true;
                    }
                    return result;
                } else {
                    result = true;
                }
                return result;
            default:
        }
    }

    //Valida el tipo de Eiqueta LEIDA, esto con el 1er caracter del LOTE L000000 => L etiqueta PT, U000 => U etiqueta traspaleo || T0000 => T etiqueta traspaleo, S0000 => S etiqueta Scrap - 3-02-2021
    $scope.EsPermitida = function (etqs) {

        switch (etqs.substring(0, 1)) {
            case 'L':
                //$scope.Alertas('e', 'La etiqueta es de PT');
                return true;

            case 'U': //Permite etiquetas de Traspaleo U - Agregado IUO 3/02/2021
                return true;

            case 'S':
                //$scope.Alertas('e', 'La etiqueta es de Scrapt');
                return false;
            case 'T': //Traspaleo
                //$scope.Alertas('e', 'La etiqueta no esta permitida');
                return true;            
            default:
                //$scope.Alertas('e', 'La etiqueta no esta permitida');
                return false;
        }
    }


   
});