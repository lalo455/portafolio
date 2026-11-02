var envioApp = angular.module('envioApp', ['angular-growl', 'ui.bootstrap', 'xeditable']);

envioApp.factory('Ruta', function () {
    return { path: '' }
});

envioApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

envioApp.service('Servicio', function ($http, Ruta, $log) {
    this.ObtenerInformacion = function (_tipo, _params) {
        return $http({
            url: Ruta.path + '/Calidad/ObtenerInformacion',
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            },
            data: {
                tipo: _tipo,
                valString: _params
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 1 ": response } }
            return error;
        });
    },
    this.ObtenerLote = function (QR) {
        return $http({
            url: Ruta.path + '/TraspasoManual/ObtenerLote',
            method: 'POST',
            data: {
                QR: QR
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
    },
        this.Crear = function (Traspaso, Guia) {
        return $http({
            url: Ruta.path + '/Calidad/Crear',
            method: 'POST',
            data: {
                Documento: Traspaso, 
                CodigoGuia: Guia
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
    },
    this.ConfirmarDatos = function (param, tipo) {
        return $http({
            url: Ruta.path + '/Calidad/ConfirmarDatos',
            method: 'POST',
            data: {
                parametro: param,
                tipo: tipo
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

envioApp.controller('envioController', function ($scope, $window, Ruta, $log, Servicio, growl) {
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

    //variables
    $scope.Lectura = {
        Etiqueta: '',
        Movimiento: false,
        Proceso: false
    }
    $scope.Envio = {
        Filler: '',
        ToWhsCode: '',
        Fillers: [],
        ToWhsCodes: [],
        Lineas: []
    }
    $scope.DtsTemp = {
        AbsEntry: '',
        BinCode: ''
    }

    $scope.Memoria = {
            CodeGuia: '', //Variable para el código de seguimiento y colocado en el Documento
        }

    //Funciones
    $scope.CodigoGuia = function () {
        var today = new Date();
        var dd = String(today.getDate()).padStart(2, '0');
        var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
        var yyyy = today.getFullYear();
        var curHour = today.getHours(); // > 12 ? today.getHours() - 12 : (today.getHours() < 10 ? "0" + today.getHours() : today.getHours());
        var curMinute = today.getMinutes(); // < 10 ? "0" + today.getMinutes() : today.getMinutes();
        var curSeconds = today.getSeconds(); // < 10 ? "0" + today.getSeconds() : today.getSeconds();
        var curMiliSec = today.getMilliseconds();

        $scope.Memoria.CodeGuia = dd + mm + yyyy + curHour + curMinute + curSeconds + curMiliSec;
        console.log('Codigo guia creado: ' + $scope.Memoria.CodeGuia);
    };

    $scope.Eliminar = function (i) {
        $scope.Envio.Lineas.splice(i, 1);
    }

    $scope.Filtrar = function (list) {
        var nArray = [];
        for (var r = 0; r < list.length; r++) {
            if ($scope.Envio.Filler == list[r].WhsCode && $scope.Lectura.Etiqueta.split('&')[1] == list[r].ItemCode) {
                nArray.push(list[r]);
            }
        }
        return nArray;
    }

    $scope.Almacenes = function (combo, _tipo, _params) {
        Servicio.ObtenerInformacion(_tipo, _params).then(function (response) {
            if (response.codigo == 0) {
                if (combo == 'WF') {
                    $scope.Envio.Fillers = response.mensaje;
                    $scope.Envio.Filler = '11';
                }
                if (combo == 'WT') 
                    $scope.Envio.ToWhsCodes = response.mensaje;
            }
            else 
                $scope.Alertas('e', response.mensaje);
        });
    }

    $scope.ObtenerUbicacionAlm = function () {
        Servicio.ConfirmarDatos($scope.Envio.ToWhsCode, 'UW').then(function (response) {
            if (response.codigo == 0) {
                $scope.DtsTemp.AbsEntry = response.mensaje.AbsEntry;
                $scope.DtsTemp.BinCode = response.mensaje.BinCode;
            }
        });
    }

    $scope.LecturaQR = function () {
        if ($scope.Lectura.Etiqueta.match("^S")) {
            var r = $scope.Validacion("R", $scope.Lectura.Etiqueta.split('&')[0]);
            if (r == -1) {
                Servicio.ObtenerLote($scope.Lectura.Etiqueta).then(function (response) {
                    if (response.codigo == 0) {
                        if (response.mensaje[0].Cancelado == 'Y') {
                            $scope.Alertas('e', 'La etiqueta fue cancelada');
                            $scope.Lectura.Etiqueta = '';
                            return;
                        }

                        var Lotes = $scope.Filtrar(response.mensaje);
                        if (Lotes.length > 1) {
                            $scope.AbrirModalLotes(Lotes);
                        }
                        if (Lotes.length > 0) 
                        {
                            if (Lotes[0].OnHandQty > $scope.Lectura.Etiqueta.split('&')[2].replace(",", ""))
                                Lotes[0].OnHandQty = $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");

                            response.mensaje[0].NewAbsEntry = $scope.DtsTemp.AbsEntry;
                            response.mensaje[0].NewBinCode = $scope.DtsTemp.BinCode;
                            $scope.Envio.Lineas.push(response.mensaje[0]);
                            $scope.Lectura.Etiqueta = "";
                        }
                        else {
                            $scope.Alertas('e', 'No se encontró ninguna etiqueta en almacén ' + $scope.Envio.Filler);
                            $scope.Lectura.Etiqueta = "";
                        }
                    }
                    else {
                        $scope.Alertas('e', response.mensaje);
                        $scope.Lectura.Etiqueta = "";
                    }
                });
            }
            else {
                $scope.Alertas('e', 'La etiqueta ya existe');
                $scope.Lectura.Etiqueta = "";
            }
        }
        else {
            $scope.Alertas('e', 'La etiqueta no es de Scrapt');
            $scope.Lectura.Etiqueta = "";
        }
    }

    $scope.Generar = function () {
        $scope.Lectura.Movimiento = true;
        console.log('Codigo guia enviado: ' + $scope.Memoria.CodeGuia);
        Servicio.Crear($scope.Envio, $scope.Memoria.CodeGuia).then(function (response) {
            if (response.codigo === 0) {
                $scope.Lectura.Movimiento = false;
                $scope.Alertas('s', 'Documento creado correctamente: '+ response.mensaje);
                $scope.Limpiar();
            }
            else {
                $scope.Lectura.Movimiento = false;
                $scope.Alertas('e', response.mensaje);
            }
        })
    }

    $scope.Limpiar = function () {
        $scope.Lectura = {
            Etiqueta: '',
            Movimiento: false,
            Proceso: false
        }
        $scope.Memoria = {
            CodeGuia: '', //Variable para el código de seguimiento y colocado en el Documento
        }
        $scope.Envio.Filler = '11';
        $scope.Envio.ToWhsCode = '';
        $scope.Envio.Lineas = [];
        $scope.CodigoGuia();
    }

    //Validaciones
    $scope.Validacion = function (tipo, param1, param2) {
        var indice = -1;
        switch (tipo) {
            case "R":
                for (var i = 0; i < $scope.Envio.Lineas.length; i++) {
                    if ($scope.Envio.Lineas[i].DistNumber == param1) {
                        indice = i;
                        break;
                    }
                }
                return indice;
            case "LI":
                for (var li = 0; li < $scope.Envio.Lineas.length; li++) {
                    if ($scope.Envio.Lineas[li].ItemCode == param2 && $scope.Envio.Lineas[li].DistNumber == param1) {
                        indice = li;
                        break;
                    }
                }
                return indice;
            default:
                return indice;
        }
    }

    //Modal
    $scope.AbrirModalLotes = function (ListLotes) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaLotes.html',
            controller: ListLotesController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                LotesList: function () {
                    return ListLotes;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result != null && result != '') {
                var i = $scope.Validacion("LI", result.DistNumber, result.ItemCode)
                if (i == -1) {
                    if (result.OnHandQty > $scope.Lectura.Etiqueta.split('&')[2].replace(",", ""))
                        result.OnHandQty = $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");
                   
                    result.NewAbsEntry = $scope.DtsTemp.AbsEntry;
                    result.NewBinCode = $scope.DtsTemp.BinCode;
                    $scope.Envio.Lineas.push(result);
                    $scope.Lectura.Etiqueta = '';
                }
                else {
                    $scope.Alertas('e', 'El lote y artículo ya existen');
                    $scope.Lectura.Etiqueta = '';
                }
            }
            $scope.Lectura.Etiqueta = '';
        });
    }
    //Controladores para modales
    var ListLotesController = function (LotesList, $scope, $uibModalInstance, $log) {
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion != null ? $scope.Seleccion : "");
        }
        $scope.LotesList = LotesList;
        $scope.LoteSeleccionado = function (val) {
            $scope.Seleccion = val;
            $scope.ok();
        }
    }


    //Ejecucion
    $scope.Almacenes('WF', 'W', []);
    $scope.Almacenes('WT', 'W', [{ "field": 'WhsName', "value": 'Scrap', "symbol": 'L' }]);
    $scope.CodigoGuia();

});