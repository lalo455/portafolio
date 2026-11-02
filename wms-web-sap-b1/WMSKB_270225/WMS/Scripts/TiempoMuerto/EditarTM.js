var editTMApp = angular.module('editTMApp', ['xeditable', 'ui.bootstrap', 'angular-growl']);

editTMApp.factory('Ruta', function () {
    return { path: '' }
});

editTMApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

editTMApp.run(function (editableOptions) {
    editableOptions.theme = 'bs3';
});

editTMApp.service('Servicio', function ($http, Ruta, $log) {

    this.ValidaOF = function (NumOF) {
        return $http({
            url: Ruta.path + '/TiempoMuerto/ValidaOF',
            method: 'GET',
            params: {
                NumOF: NumOF
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 6 ": response } }
            return error;
        });
    },

    this.ListaRegistros = function (NumOF) {
        return $http({
            url: Ruta.path + '/TiempoMuerto/ListaRegistros',
            method: 'GET',
            params: {
                NumOF: NumOF
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 6 ": response } }
            return error;
        });
    },

    this.EliminarTPM = function (lineas) {
        return $http({
            url: Ruta.path + '/TiempoMuerto/EliminarTPM',
            method: 'POST',
            data: {
                Lineas: lineas
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

editTMApp.controller('editTMController', function ($scope, Ruta, $log, Servicio, $uibModal, growl) {
    $scope.Lectura = {
        Movimiento: false
    }

    $scope.TPM = {
        NumOrdr: ''
        , Lineas: []
    }

    $scope.Memoria = {
        Nlectura: 1
        , Estatus: ''
        , LineasReg: []
        , LineasEliminar: []
    }

    //****************************************************************
    $scope.Cargar = function () {
        console.log('OF: ' + $scope.TPM.NumOrdr);
        if ($scope.TPM.NumOrdr === "") {
            $scope.Alertas('w', "Ingrese Orden de Fabricación");
            $scope.TPM.NumOrdr = "";
            return;
        }

        var proceder = false;
        $scope.Memoria.Estatus = "";
        $scope.TPM.Lineas = [];
        $scope.Memoria.LineasReg = [];
        $scope.Memoria.LineasEliminar = [];

        Servicio.ValidaOF($scope.TPM.NumOrdr).then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.TPM.DocNum = response.mensaje[0].DocNum;
                    $scope.TPM.DocEntry = response.mensaje[0].DocEntry;
                    $scope.Memoria.Estatus = response.mensaje[0].Status;
                    console.log('OF OK: ' + $scope.TPM.DocNum + ' | Entry: ' + $scope.TPM.DocEntry + ' | Estatus: ' + $scope.Memoria.Estatus);

                    if ($scope.Memoria.Estatus === "R") {
                        //Obtener Registros de TPM de OF
                        Servicio.ListaRegistros($scope.TPM.NumOrdr).then(function (response) {
                            switch (response.codigo) {
                                case 0:
                                    $scope.Memoria.LineasReg = response.mensaje;
                                    console.log($scope.Memoria.LineasReg);
                                    break;

                                case -1:
                                    $scope.Alertas('w', response.mensaje);
                                    break;

                                case -2:
                                    $scope.Alertas('e', response.mensaje);
                                    break;
                            }
                        });
                    }
                    else {
                        if ($scope.Memoria.EstatusOF === "C") {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " se encuentra Cancelada");
                        }
                        else if ($scope.Memoria.EstatusOF === "L") {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " se encuentra Cerrada");
                        }
                        else if ($scope.Memoria.EstatusOF === "P") {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " se encuentra Planificada");
                        }
                        else {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " no se encuentra Abierta/Liberada");
                        }
                    }
                    break;

                case -1:
                    $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " no existe");
                    $scope.TPM.NumOrdr = "";
                    break;

                case -2:
                    $scope.Alertas('e', "Error al validar Orden de Fabricación " + $scope.TPM.NumOrdr + ", vuelva a intentar");
                    $scope.TPM.NumOrdr = "";
                    break;
            }
        });
    }

    //*****************************************************************
    $scope.Eliminar = function (i) {
        console.log($scope.Memoria.LineasReg[i]);
        $scope.Memoria.LineasEliminar.push($scope.Memoria.LineasReg[i]);
        console.log($scope.Memoria.LineasEliminar);
        $scope.Memoria.LineasReg.splice(i, 1);
    }

    //********************************************************************************************/
    //Generar Movimiento
    $scope.Crear = function () {
        $scope.TPM.Lineas = [];

        for (var i = 0; i < $scope.Memoria.LineasEliminar.length; i++) {
            //Agregar registro
            $scope.TPM.Lineas.push({
                "NumOrdr": $scope.Memoria.LineasEliminar[i].DocNum
                , "DocNum": $scope.Memoria.LineasEliminar[i].DocNum
                , "DocEntry": $scope.Memoria.LineasEliminar[i].DocEntry
                , "Maquina": $scope.Memoria.LineasEliminar[i].Maquina
                , "Turno": $scope.Memoria.LineasEliminar[i].Turno
                , "TipoTPM": $scope.Memoria.LineasEliminar[i].TipoTPM
                , "DescTPM": $scope.Memoria.LineasEliminar[i].DescTPM
                , "Tiempo": $scope.Memoria.LineasEliminar[i].Tiempo

                , "SDocEntry": $scope.Memoria.LineasEliminar[i].SDocEntry
                , "LineId": $scope.Memoria.LineasEliminar[i].LineId
                , "VisOrder": $scope.Memoria.LineasEliminar[i].VisOrder
            });
        }

        console.log($scope.TPM.Lineas);
        if ($scope.TPM.Lineas.length > 0 && ($scope.TPM.Lineas.length === $scope.Memoria.LineasEliminar.length)) {
            $scope.Lectura.Movimiento = true;
            Servicio.EliminarTPM($scope.TPM.Lineas).then(function (response) {
                if (response.codigo === 0) {
                    $scope.Alertas('s', response.mensaje);
                    $scope.Limpiar();
                    $scope.Lectura.Movimiento = false;
                }
                else {
                    $scope.Alertas('e', response.mensaje);
                    $scope.Lectura.Movimiento = false;
                }
            });
        }
        else {
            $scope.Alertas('w', "Ocurrio un error al procesar, vuelva a intentar");
        }
    }

    //Recargar
    $scope.Limpiar = function () {
        $scope.Lectura = {
            Movimiento: false
        }

        $scope.TPM = {
            NumOrdr: ''
            , Lineas: []
        }

        $scope.Memoria = {
            Nlectura: 1
            , Estatus: ''
            , LineasReg: []
            , LineasEliminar: []
        }
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

});