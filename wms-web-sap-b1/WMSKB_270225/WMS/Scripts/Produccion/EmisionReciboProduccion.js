var emisionReciboApp = angular.module('emisionReciboApp', ['xeditable', 'ui.bootstrap', 'angular-growl']);

emisionReciboApp.factory('Ruta', function () {
    return { path: '' }
});

emisionReciboApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

emisionReciboApp.run(function (editableOptions) {
    editableOptions.theme = 'bs3';
});

emisionReciboApp.service('Servicio', function ($http, Ruta, $log) {
    this.ListaTurnos = function () {
        return $http({
            url: Ruta.path + '/Produccion/Turnos',
            method: 'GET',
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

    this.GrabarLectura = function (etq, nlectura) {
        return $http({
            url: Ruta.path + '/Produccion/GrabarLectura',
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

    this.ValidaEtiqueta = function (lote) {
        return $http({
            url: Ruta.path + '/Produccion/ValidaEtiqueta',
            method: 'GET',
            params: {
                Lote: lote
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

    this.Lote = function (lote) {
        return $http({
            url: Ruta.path + '/Produccion/EstatusEtiqueta',
            method: 'POST',
            data: {
                Lote: lote
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
    
    this.EliminarRBorrador = function (row) {
        return $http({
            url: Ruta.path + '/Produccion/BorrarRBorrador',
            method: 'POST',
            data: {
                registro: row
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
    
    this.GuardarBorrador = function (_lineas, _guid, _RegMaster) {
        return $http({
            url: Ruta.path + '/Produccion/GuardarBorrador',
            method: 'POST',
            data: {
                Lineas: _lineas,
                guid: _guid, 
                RegMaster: _RegMaster
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 10 ": response } }
            return error;
        });
    },

    this.RecuperarBorrador = function (_type, _order, _turno) {
        return $http({
            url: Ruta.path + '/Produccion/RecuperarBorrador',
            method: 'GET',
            params: {
                type: _type,
                order: _order,
                turno: _turno
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 11 ": response } }
            return error;
        });
    },

    this.Crear = function (lineas, _Guia, _RegMaster) {
        return $http({
            url: Ruta.path + '/Produccion/CrearEmisionRecibo',
            method: 'POST',
            data: {
                Lineas: lineas
                , CodigoGuia: _Guia
                , RegMaster: _RegMaster
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


    //Master
    this.ObtenerPrefijo = function () {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ObtenerPrefijo',
            method: 'GET',
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

    this.MasterLiberar = function (Codigo, Articulo, Orden) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ObtenerMasterLiberar',
            method: 'GET',
            params: {
                Codigo: Codigo
                , Articulo: Articulo
                , Orden: Orden
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

emisionReciboApp.controller('emisionReciboController', function ($scope, Ruta, $log, Servicio, $uibModal, growl) {
    //Variables
    $scope.ReciboEmision = {
        Turnos: [],
        Turno: '',
        
        AlmacenSel: '',

        BinCode: '',
        AbsEntry: 0,

        Lineas: []
    }

    $scope.Lectura = {
        Etiqueta: '',
        Movimiento: false
    }

    $scope.Borrador = {
        Lineas: [],
        AlmacenSel: '',
        guid: UUID()
    }

    $scope.Memoria = {
        TurnoEtq: 1,
        FechaErq: '', 
        Marcados: [], 
        Nlectura: 1, 
        CodeGuia: '', 
        msgEtq: ''
    }

    $scope.Master = {
        Existe: 'no',
        CodeML: '',
        LineasMaster: [], //Lista de etiquetas de la Master
        ErrorLineasMaster: [],
        LotesMaster: [], //Lista de Lotes de la Master en Almacén Origen seleccionado
        ErrorLotesMaster: [],
        MasterRegUnica: [], //Lista de registros de Master actual leída
        RegMaster: [] //Lista de registros totales de Master leidas
    }

    //************************************************************************/
    //Lista de Turnos
    $scope.Turnos = function () {
        Servicio.ListaTurnos().then(function (response) {
            if (response.codigo === 0) {
                $scope.ReciboEmision.Turnos = response.mensaje;
                $scope.ReciboEmision.Turno = response.turno;
            }
            else
                $scope.Alertas('w', response.mensaje);
        });
    }

    //Generación de codigo guia
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

    //Validar manejo de Master
    $scope.ValidaMaster = function () {
        //Obtener prefijo de la Etiqueta Master
        Servicio.ObtenerPrefijo().then(function (_response) {
            if (_response.codigo === 0) {
                $scope.Master.Existe = 'si';
                $scope.Master.CodeML = _response.mensaje;
                console.log('Manejo de Master: ' + $scope.Master.Existe + ' | Codigo ML: ' + $scope.Master.CodeML);
            }
            else {
                $scope.Master.Existe = 'no';
                $scope.Master.CodeML = '';
                console.log('Manejo de Master: ' + $scope.Master.Existe + ' | Error ::: ' + _response.mensaje);
            }
        });
    }
    //*********************************************************************/    
    //Funciones
    $scope.LecturaQR = function () {
        //Grabar informacion
        Servicio.GrabarLectura($scope.Lectura.Etiqueta, $scope.Memoria.Nlectura).then(function (response) //Revisa el estatus de la etiqueta 
        {
            console.log(response.mensaje.length + ' --- # ' + $scope.Memoria.Nlectura);
            $scope.Memoria.Nlectura++;
        });

        //Validar si es correcta la etiqueta
        if ($scope.Lectura.Etiqueta.indexOf("&") !== -1) {

            //Validar si se puede leer Master
            if ($scope.Master.Existe === "si") {
                //Validar tipo de lectura
                console.log('Prefijo etiqueta leida: ' + $scope.Lectura.Etiqueta.split('&')[0].substring(0, $scope.Master.CodeML.length));

                //Verificar si la etiqueta es master o unica por el prefijo
                if ($scope.Lectura.Etiqueta.split('&')[0].substring(0, $scope.Master.CodeML.length) === $scope.Master.CodeML) {
                    //Procesamiento de ETIQUETA MASTER | se procesa como Etiqueta Unica
                    console.log('Procesamiento de ETIQUETA MASTER');
                    $scope.ProcesaEtiquetaMaster();
                }
                else {
                    //Procesamiento de ETIQUETA UNICA
                    console.log('Procesamiento de ETIQUETA UNICA');
                    $scope.ProcesaEtiquetaUnica();
                }
            }
            else {
                //Procesamiento de ETIQUETA UNICA
                console.log('Procesamiento de ETIQUETA UNICA');
                $scope.ProcesaEtiquetaUnica();
            }
        }
        else {
            $scope.Alertas('e', "La etiqueta leída no corresponde a ninguna de las permitidas");
            $scope.Lectura.Etiqueta = '';
            $("#Entradas").focus();
        }
    }

    $scope.ProcesaEtiquetaUnica = function () {
        //Verificar si la etiqueta leida es Permitida (L, T, U) y si la etiqueta no ha sido leida anteriormente
        if ($scope.EsPermitida($scope.Lectura.Etiqueta) && !$scope.Existe('T', $scope.Lectura.Etiqueta.split('&')[0], '')) {
            //Verificar si la etiqueta se encuentra en algun borrador
            Servicio.ValidaEtiqueta($scope.Lectura.Etiqueta).then(function (_response) {

                $scope.Memoria.msgEtq = '';
                switch (_response.codigo) {
                    case 0:

                        switch (_response.mensaje.length) {
                            case 0:
                                console.log('No existen coincidencias en Borradores');
                                //Inicio - Obtener Informacion de la Etiqueta
                                Servicio.Lote($scope.Lectura.Etiqueta).then(function (response) //Revisa el estatus de la etiqueta 
                                {
                                    if (response.codigo === 0) {
                                        console.log(response.mensaje);
                                        console.log('Se encontraron: ' + response.mensaje.length + ' Etiqueta');

                                        if (response.mensaje.length > 1) {
                                            //En caso de existir varios registros iguales en SAP a lo leido
                                            $scope.Alertas('e', "Se encontraron Registros repetidos al Obtener Turno y Fecha de Etiqueta.");
                                            $scope.Lectura.Etiqueta = '';
                                            $("#Entradas").focus();
                                            return;
                                        }

                                        if (response.mensaje.length < 1) {
                                            $scope.Alertas('e', "NO se encontraron Registros al Obtener Turno y Fecha de Etiqueta.");
                                            $scope.Lectura.Etiqueta = '';
                                            $("#Entradas").focus();
                                            return;
                                        }

                                        //Solo procesar cuando no tenga Registro Master || Update: 111124 Sam indica que se quite validación
                                        //if (response.mensaje[0].EtqMaster === "WM") {
                                        //Turno y la Fecha de Creacion de la Etiqueta                            
                                        $scope.Memoria.TurnoEtq = response.mensaje[0].Turno;
                                        $scope.Memoria.FechaErq = response.mensaje[0].Fecha;
                                        console.log('Turno: ' + $scope.Memoria.TurnoEtq + ' --- Fecha: ' + $scope.Memoria.FechaErq);

                                        $scope.ReciboEmision.Lineas.push({
                                            "DistNumber": $scope.Lectura.Etiqueta.split('&')[0],
                                            "ItemCode": $scope.Lectura.Etiqueta.split('&')[1],
                                            "OnHandQty": $scope.Lectura.Etiqueta.split('&')[2].replace(",", ""),
                                            "OF": $scope.Lectura.Etiqueta.split('&')[3],
                                            "AbsEntry": $scope.ReciboEmision.AbsEntry,
                                            "BinCode": $scope.ReciboEmision.BinCode,
                                            "Warehouse": response.mensaje[0].Warehouse,
                                            "U_TurnoRecibo": $scope.Memoria.TurnoEtq,
                                            "U_FechaRecibo": $scope.Memoria.FechaErq,
                                            "TipoLectura": "I",
                                            "TipoReg": "I"
                                        });

                                        $scope.Lectura.Etiqueta = '';
                                        $("#Entradas").focus();
                                        //}
                                        //else {
                                        //    $scope.Alertas('w', "La etiqueta indivdual existe en Master " + response.mensaje[0].EtqMaster);
                                        //    $scope.Lectura.Etiqueta = '';
                                        //    $("#Entradas").focus();
                                        //}
                                    }
                                    else {
                                        $scope.Alertas('e', response.mensaje);
                                        $scope.Lectura.Etiqueta = '';
                                        $("#Entradas").focus();
                                    }
                                });
                                break;

                            default:
                                for (var i = 0; i < _response.mensaje.length; i++) {
                                    if ($scope.Memoria.msgEtq === '') {
                                        $scope.Memoria.msgEtq = "La Etiqueta con Lote " + $scope.Lectura.Etiqueta.split('&')[0] + ", ya existe en Borrador: ";

                                        if (_response.mensaje[i].TipoLectura === "I") {
                                            $scope.Memoria.msgEtq = $scope.Memoria.msgEtq + " <br /> > Usuario " + _response.mensaje[i].Usuario + ", con Fecha " + _response.mensaje[i].FechaCreacion;
                                        }
                                        else {
                                            $scope.Memoria.msgEtq = $scope.Memoria.msgEtq + " <br /> > Usuario " + _response.mensaje[i].Usuario + ", con Fecha " + _response.mensaje[i].FechaCreacion + " y Etiqueta Master " + _response.mensaje[i].EtqMaster;
                                        }
                                    }
                                    else {
                                        if (_response.mensaje[i].TipoLectura === "I") {
                                            $scope.Memoria.msgEtq = $scope.Memoria.msgEtq + " <br /> > Usuario " + _response.mensaje[i].Usuario + ", con Fecha " + _response.mensaje[i].FechaCreacion;
                                        }
                                        else {
                                            $scope.Memoria.msgEtq = $scope.Memoria.msgEtq + " <br /> > Usuario " + _response.mensaje[i].Usuario + ", con Fecha " + _response.mensaje[i].FechaCreacion + " y Etiqueta Master " + _response.mensaje[i].EtqMaster;
                                        }
                                    }
                                }

                                if ($scope.Memoria.msgEtq !== '') {
                                    $scope.Alertas('w', $scope.Memoria.msgEtq);
                                }

                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();
                                break;
                        }
                        break;

                    default:
                        $scope.Alertas('e', _response.mensaje);
                        $scope.Lectura.Etiqueta = '';
                        $("#Entradas").focus();
                        break;
                }
            });
        }
    }
    //***************************************************************************************/
    $scope.ProcesaEtiquetaMaster = function () {

        if (!$scope.Existe("M", $scope.Lectura.Etiqueta.split('&')[0], 0)) {

            //Validar que Etiqueta no se encuentre en Borrador
            Servicio.ValidaEtiqueta($scope.Lectura.Etiqueta).then(function (_response) {

                $scope.Memoria.msgEtq = '';
                switch (_response.codigo) {
                    case 0:

                        switch (_response.mensaje.length) {
                            case 0: //No existe en borrador, se puede procesar
                                console.log('No existen coincidencias en Borradores');
                                //Inicio - Obtener Informacion de la Etiqueta Master | Master | Articulo | Orden de fabricacion
                                Servicio.MasterLiberar($scope.Lectura.Etiqueta.split('&')[0], $scope.Lectura.Etiqueta.split('&')[1], $scope.Lectura.Etiqueta.split('&')[3]).then(function (response) {
                                    $scope.Master.LineasMaster = [];
                                    $scope.Master.ErrorLineasMaster = [];
                                    $scope.Master.LotesMaster = [];
                                    $scope.Master.ErrorLotesMaster = [];
                                    $scope.Master.MasterRegUnica = [];
                                    let QtyTotalLotes = 0;

                                    switch (response.codigo) {
                                        case 0:
                                            //Información de master obtenida | Master Existe
                                            /*
                                            MASTER SE LEE COMPLETA, NO ES EDITABLE, NO HAY PARCIALES
                                            1.- Validar Master : Generada 
                                            2.- Validar Master : No bloqueda
                                            
                                            3.- Recorrer lista de Etiquetas
                                            3.1.- Validar estatus : Generada
                                            3.2.- Validar borrador : No exista en borrador
                                            3.3.- Validar etiquetas completas y correctas | Falla alguna : NO se procesa Master

                                            4.- Validar Cantidad Total de etiquetas vs Cantidad Master | NO igual : No se procesa
                                            */

                                            if (response.mensaje[0].Estatus === 'G' && response.mensaje[0].Calidad === 'O') {

                                                let QtyEtqM = new Decimal($scope.Lectura.Etiqueta.split('&')[2].replace(",", ""));
                                                let qtyMaster = new Decimal(response.mensaje[0].MasterOnHandQty);
                                                if (QtyEtqM.toNumber() === qtyMaster.toNumber()) {

                                                    //Validar etiquetas individuales de la master
                                                    for (var i = 0; i < response.mensaje.length; i++) {
                                                        //Detalle de etiquetas
                                                        if (response.mensaje[i].TipoReg === "2") {
                                                            if (response.mensaje[i].EtqLoteEstatus === 'G') {
                                                                //Validar Lote no se haya leido de forma individualmente
                                                                if (!$scope.Existe("ML", response.mensaje[i].EtqLote, 0)) {

                                                                    let valorQtyEtiqueta = new Decimal(response.mensaje[i].EtqLoteOnHandQty);
                                                                    let valorTotalQty = new Decimal(QtyTotalLotes);

                                                                    $scope.Master.LineasMaster.push(response.mensaje[i]); //Etiqueta individual correcta
                                                                    QtyTotalLotes = valorTotalQty.plus(valorQtyEtiqueta).toNumber();
                                                                }
                                                                else {
                                                                    $scope.Master.ErrorLineasMaster.push({
                                                                        "DistNumber": response.mensaje[i].EtqLote,
                                                                        "Error": "se ha leído de forma individual, eliminelo para leer Master completa"
                                                                    });
                                                                }
                                                            }
                                                            else {
                                                                $scope.Master.ErrorLineasMaster.push({
                                                                    "DistNumber": response.mensaje[i].EtqLote,
                                                                    "Error": "ya se encuentra Liberada"
                                                                });
                                                            }
                                                        }

                                                        //Detalle de etiquetas en Borrador
                                                        if (response.mensaje[i].TipoReg === "3") {
                                                            $scope.Master.ErrorLineasMaster.push({
                                                                "DistNumber": response.mensaje[i].EtqLote,
                                                                "Error": " se encuentra en Borrador de Usuario " + response.mensaje[i].Usuario + " con Fecha " + response.mensaje[i].FechaCreacion
                                                            });
                                                        }                                                        
                                                    }

                                                    console.log($scope.Master.LineasMaster);
                                                    console.log($scope.Master.ErrorLineasMaster);

                                                    if ($scope.Master.ErrorLineasMaster.length === 0) {
                                                        let qtyLotes = new Decimal(QtyTotalLotes);
                                                        if (qtyMaster.toNumber() === qtyLotes.toNumber()) {
                                                            console.log('PROCESAR MASTER | agregar información de master visual | no se puede editar');

                                                            for (var r = 0; r < $scope.Master.LineasMaster.length; r++) {
                                                                $scope.Master.RegMaster.push({
                                                                    "DistNumber": $scope.Master.LineasMaster[r].EtqLote
                                                                    , "ItemCode": $scope.Master.LineasMaster[r].Articulo
                                                                    , "OnHandQty": $scope.Master.LineasMaster[r].EtqLoteOnHandQty
                                                                    , "OF": $scope.Master.LineasMaster[r].OrdenFab                                                                    
                                                                    , "BinCode": $scope.ReciboEmision.BinCode
                                                                    , "AbsEntry": $scope.ReciboEmision.AbsEntry
                                                                    , "Warehouse": $scope.Master.LineasMaster[r].WhsCode
                                                                    , "U_TurnoRecibo": $scope.Master.LineasMaster[r].EtqLoteTurno
                                                                    , "U_FechaRecibo": $scope.Master.LineasMaster[r].EtqLoteFecha
                                                                    , "EtqMaster": $scope.Master.LineasMaster[r].Codigo
                                                                    , "MasterOnHandQty": $scope.Master.LineasMaster[r].MasterOnHandQty
                                                                    , "TipoLectura": "M"
                                                                    , "TipoReg": "M"
                                                                });

                                                                $scope.Master.MasterRegUnica.push({
                                                                    "DistNumber": $scope.Master.LineasMaster[r].EtqLote
                                                                    , "ItemCode": $scope.Master.LineasMaster[r].Articulo
                                                                    , "OnHandQty": $scope.Master.LineasMaster[r].EtqLoteOnHandQty
                                                                    , "OF": $scope.Master.LineasMaster[r].OrdenFab
                                                                    , "BinCode": $scope.ReciboEmision.BinCode
                                                                    , "AbsEntry": $scope.ReciboEmision.AbsEntry
                                                                    , "Warehouse": $scope.Master.LineasMaster[r].WhsCode
                                                                    , "U_TurnoRecibo": $scope.Master.LineasMaster[r].EtqLoteTurno
                                                                    , "U_FechaRecibo": $scope.Master.LineasMaster[r].EtqLoteFecha
                                                                    , "EtqMaster": $scope.Master.LineasMaster[r].Codigo
                                                                    , "MasterOnHandQty": $scope.Master.LineasMaster[r].MasterOnHandQty
                                                                    , "TipoLectura": "M"
                                                                    , "TipoReg": "M"
                                                                });
                                                            }
                                                            console.log($scope.Master.RegMaster);
                                                            console.log($scope.Master.MasterRegUnica);

                                                            $scope.ReciboEmision.Lineas.push({
                                                                "DistNumber": $scope.Master.MasterRegUnica[0].EtqMaster
                                                                , "ItemCode": $scope.Master.MasterRegUnica[0].ItemCode
                                                                , "OnHandQty": $scope.Master.MasterRegUnica[0].MasterOnHandQty
                                                                , "OF": $scope.Master.MasterRegUnica[0].OF     
                                                                , "BinCode": $scope.ReciboEmision.BinCode
                                                                , "AbsEntry": $scope.ReciboEmision.AbsEntry
                                                                , "Warehouse": $scope.Master.MasterRegUnica[0].Warehouse
                                                                , "U_TurnoRecibo": $scope.Master.MasterRegUnica[0].U_TurnoRecibo
                                                                , "U_FechaRecibo": $scope.Master.MasterRegUnica[0].U_FechaRecibo
                                                                , "EtqMaster": $scope.Master.MasterRegUnica[0].EtqMaster
                                                                , "TipoLectura": "M"
                                                                , "TipoReg": "I"
                                                            });

                                                            console.log($scope.ReciboEmision.Lineas);
                                                            $scope.Lectura.Etiqueta = '';
                                                            $("#Entradas").focus();
                                                        }
                                                        else {
                                                            console.log("Validar etiqueta master, la cantidad no coincide con el acumulado de las etiquetas individuales");
                                                            $scope.Alertas('e', "Verificar Etiqueta Master en SAP, cantidad Master " + response.mensaje[0].MasterOnHandQty + " no coincide con el acumulado de etiquetas individuales " + QtyTotalLotes);
                                                            $scope.Lectura.Etiqueta = '';
                                                            $("#Entradas").focus();
                                                        }
                                                    }
                                                    else {
                                                        console.log('Verificar los errores');
                                                        var msjErrores = "Validar los siguientes detalles de etiqueta Master:";

                                                        for (var x = 0; x < $scope.Master.ErrorLineasMaster.length; x++) {
                                                            msjErrores = msjErrores + "<br/> > Etiqueta con Lote individual " + $scope.Master.ErrorLineasMaster[x].DistNumber + " - " + $scope.Master.ErrorLineasMaster[x].Error;
                                                        }

                                                        $scope.Alertas('e', msjErrores);
                                                        $scope.Lectura.Etiqueta = '';
                                                        $("#Entradas").focus();
                                                    }
                                                }
                                                else {
                                                    console.log("Validar etiqueta master, la cantidad de etiqueta no coincide con cantidad en SAP");
                                                    $scope.Alertas('e', "Verificar Etiqueta Master en SAP, cantidad etiqueta " + QtyEtqM.toNumber() + " no coincide con cantidad en SAP " + qtyMaster.toNumber());
                                                    $scope.Lectura.Etiqueta = '';
                                                    $("#Entradas").focus();
                                                }
                                            }
                                            else {
                                                if (response.mensaje[0].Estatus !== 'G') {
                                                    $scope.Alertas('w', "La Etiqueta Master ya se encuentra Liberada, verifique en SAP");
                                                }

                                                if (response.mensaje[0].Calidad !== 'O') {
                                                    $scope.Alertas('w', "La Etiqueta Master se encuentra Bloqueada, verifique en SAP");
                                                }                                                
                                                $scope.Lectura.Etiqueta = '';
                                                $("#Entradas").focus();
                                            }
                                            break;

                                        default:
                                            $scope.Alertas('e', response.mensaje);
                                            $scope.Lectura.Etiqueta = '';
                                            $("#Entradas").focus();
                                            break
                                    }
                                });
                                break;

                            default: //Master existe en algun borrador
                                for (var i = 0; i < _response.mensaje.length; i++) {
                                    if ($scope.Memoria.msgEtq === '') {
                                        $scope.Memoria.msgEtq = "La Etiqueta Master " + $scope.Lectura.Etiqueta.split('&')[0] + ", ya existe en Borrador: ";
                                        $scope.Memoria.msgEtq = $scope.Memoria.msgEtq + " <br /> > Usuario " + _response.mensaje[i].Usuario + ", con Fecha " + _response.mensaje[i].FechaCreacion;                                        
                                    }
                                    else {
                                        $scope.Memoria.msgEtq = $scope.Memoria.msgEtq + " <br /> > Usuario " + _response.mensaje[i].Usuario + ", con Fecha " + _response.mensaje[i].FechaCreacion;
                                    }
                                }

                                if ($scope.Memoria.msgEtq !== '') {
                                    $scope.Alertas('w', $scope.Memoria.msgEtq);
                                }

                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();
                                break;
                        }
                        break;

                    default:
                        $scope.Alertas('e', _response.mensaje);
                        $scope.Lectura.Etiqueta = '';
                        $("#Entradas").focus();
                        break;
                }
            });
        }
        else {
            $scope.Alertas('w', "La etiqueta master " + $scope.Lectura.Etiqueta.split('&')[0] + " ya ha sido leída");
            $scope.Lectura.Etiqueta = '';
            $("#Entradas").focus();
        }
    }

    //***************************************************************************************/
    //Valida el tipo de Eiqueta Individual LEIDA, esto con el 1er caracter del LOTE L000000 => L etiqueta PT, T0000 => T etiqueta traspaleo, U000 => U etiqueta traspaleo, S0000 => S etiqueta Scrap
    $scope.EsPermitida = function (etqs) {
        switch (etqs.substring(0, 1)) {
            case 'L':
                return true;

            case 'S':
                $scope.Alertas('w', 'La etiqueta de Scrapt no es permitida');
                $scope.Lectura.Etiqueta = '';
                return false;

            case 'T': //Etiquetas de Traspaleo T NO es PERMITIDA - Agregado IUO 6/11/2020
            case 'U': 
                $scope.Alertas('e', 'La etiqueta de Traspaleo no es permitida');
                $scope.Lectura.Etiqueta = '';
                return false;
        }
    }

    $scope.Existe = function (tipo, param1, param2) {
        var result = false;
        switch (tipo) {
            case "T": //Lectura individual                
                for (var t = 0; t < $scope.ReciboEmision.Lineas.length; t++) {
                    if (param1 === $scope.ReciboEmision.Lineas[t].DistNumber) {
                        $scope.Alertas('e', 'La etiqueta YA FUE LEIDA');
                        $scope.Lectura.Etiqueta = '';
                        result = true;
                        break;
                    }
                }

                //Validar si existe Master leida | validar lote individual en master leida
                if ($scope.Master.RegMaster.length > 0) {
                    for (var m = 0; m < $scope.Master.RegMaster.length; m++) {
                        if ($scope.Master.RegMaster[m].DistNumber === param1) {
                            result = true;
                            $scope.Alertas("w", "La etiqueta ya existe en Master Leida " + $scope.Master.RegMaster[m].EtqMaster);
                            $scope.Lectura.Etiqueta = '';
                            break;
                        }
                    }
                }
                break;

            case "M"://Master unica en la tabla
                for (var t = 0; t < $scope.ReciboEmision.Lineas.length; t++) {
                    if ($scope.ReciboEmision.Lineas[t].DistNumber === param1) {
                        result = true;
                        $scope.Lectura.Etiqueta = '';
                        break;
                    }
                }
                break;

            case "ML"://Lote Master de PT unico en la tabla
                for (var t = 0; t < $scope.ReciboEmision.Lineas.length; t++) {
                    if ($scope.ReciboEmision.Lineas[t].DistNumber === param1) {
                        result = true;
                        $scope.Lectura.Etiqueta = '';
                        break;
                    }
                }
                break;
        }

        return result;
    }

    //*****************************************************************
    //Acciones dentro del Grid
    $scope.EliminarBorrador = function (i) {
        // i => reg a eliminar en lineas leídas
        let TipoLectura = $scope.ReciboEmision.Lineas[i].TipoLectura; // I: Individual | M: Master

        var dialog = new Messi(
            "¿Desea borrar la etiqueta?",
            {
                title: 'Mensaje de Confirmación', modal: true,
                buttons: [
                    { id: "btnSi", label: "Si", val: "Y", class: 'btn-success' },
                    { id: "btnNo", label: "No", val: "N", class: 'btn-danger' }
                ],
                titleClass: "warning",
                callback: function (val) {
                    if (val === 'Y') {
                        Servicio.EliminarRBorrador($scope.ReciboEmision.Lineas[i]).then(function (response) {
                            switch (response.codigo) {
                                case 0:
                                    if (TipoLectura === "I") {
                                        console.log('Eliminar Lote: ' + $scope.ReciboEmision.Lineas[i].DistNumber);

                                        $scope.ReciboEmision.Lineas.splice(i, 1);

                                        if ($scope.ReciboEmision.Lineas.length === 0) {
                                            $scope.Borrador = {
                                                Lineas: [],
                                                guid: UUID()
                                            }
                                        }
                                    }
                                    else {
                                        //Eliminar registros individuales de Master
                                        let MasterCodigo = $scope.ReciboEmision.Lineas[i].DistNumber;

                                        //Por cada uno de los valores buscar y eliminar lotes de Master
                                        for (var h = $scope.Master.RegMaster.length - 1; h >= 0; h--) {
                                            if ($scope.Master.RegMaster[h].EtqMaster === MasterCodigo) {
                                                console.log("Lote " + $scope.Master.RegMaster[h].DistNumber + ", ELIMINADO");

                                                //Eliminar lote 
                                                $scope.Master.RegMaster.splice(h, 1);
                                            }
                                        }

                                        $scope.ReciboEmision.Lineas.splice(i, 1);

                                        if ($scope.ReciboEmision.Lineas.length === 0) {
                                            $scope.Borrador = {
                                                Lineas: [],
                                                guid: UUID()
                                            }
                                        }

                                        console.log("Lotes de Master restantes >> " + JSON.stringify($scope.Master.RegMaster));
                                        console.log("Registros leídos restantes >> " + JSON.stringify($scope.ReciboEmision.Lineas));
                                    }
                                    break;

                                case -1:
                                    $scope.Alertas("e", response.mensaje);
                                    break;
                            }
                        });
                    }
                }
        });
    }

    //*****************************************************************
    //Borrador
    $scope.SalvarBorrador = function () {
        Servicio.GuardarBorrador($scope.ReciboEmision.Lineas, $scope.Borrador.guid, $scope.Master.RegMaster).then(function (response) {
            if (response.codigo === 0) {
                $scope.Alertas("s", response.mensaje);
                $scope.Reiniciar();
            }
            else if (response.codigo === -2) {
                arrTags = response.mensaje.split('&').map(function (item) {
                    return { name: item };
                });
                console.log(arrTags);

                var msjEtq = "";
                for (var i in arrTags) {
                    if (msjEtq === "") {
                        msjEtq = arrTags[i].name;
                    }
                    else {
                        msjEtq = msjEtq + "<br /><br />" + arrTags[i].name;
                    }
                }
                $scope.Alertas("w", msjEtq);
                $scope.Reiniciar();
            }
            else {
                $scope.Alertas("e", response.mensaje);
            }
        });
    }

    $scope.Reiniciar = function () {
        $scope.ReciboEmision.Lineas = [];
        $scope.Lectura = {
            Etiqueta: '',
            Movimiento: false
        }
        $scope.Borrador = {
            Lineas: [],
            AlmacenSel: '',
            guid: UUID()
        }
        $scope.Memoria = {
            TurnoEtq: 1,
            FechaErq: '',
            Marcados: [],
            Nlectura: 1,
            CodeGuia: '',
            msgEtq: ''
        }

        $scope.Master = {
            Existe: 'no',
            CodeML: '',
            LineasMaster: [], //Lista de etiquetas de la Master
            ErrorLineasMaster: [],
            LotesMaster: [], //Lista de Lotes de la Master en Almacén Origen seleccionado
            ErrorLotesMaster: [],
            MasterRegUnica: [], //Lista de registros de Master actual leída
            RegMaster: [] //Lista de registros totales de Master leidas
        }

        $scope.CodigoGuia();
        //Verificar manejo de Master
        $scope.ValidaMaster();
    }

    $scope.ListarBorradores = function () {
        Servicio.RecuperarBorrador('L', 0, $scope.ReciboEmision.Turno).then(function (response) {
            if (response.codigo === 0) {
                $scope.AbrirModalBorrador(response.mensaje, $scope.ReciboEmision.Turno);
            }
            else {
                $scope.Alertas('e', response.mensaje);
            }
        });
    }

    $scope.CargarBorrador = function (order) {
        $scope.Borrador.Lineas = [];
        Servicio.RecuperarBorrador('R', order, $scope.ReciboEmision.Turno).then(function (response) {
            if (response.codigo === 0) {
                $scope.Memoria.Marcados = [];
                $scope.Borrador.Lineas = response.mensaje;
                $scope.CargarBorradorEnLineas();
            }
            else {
                $scope.Alertas('e', response.mensaje);
            }
        });
    }

    $scope.CargarBorradorEnLineas = function () {
        //console.log('Cargar borrador....');
        $scope.ReciboEmision.Lineas = []; //Lista de etiquetas leídas | individual | master | tipoReg : I
        $scope.Master.RegMaster = []; //Lista de registros de Master leídas | tipoReg : M

        //console.log($scope.Borrador.Lineas);

        for (var i = 0; i < $scope.Borrador.Lineas.length; i++) {
            if ($scope.Borrador.Lineas[i].TipoReg === "I") {
                $scope.ReciboEmision.Lineas.push($scope.Borrador.Lineas[i]);
            }

            if ($scope.Borrador.Lineas[i].TipoReg === "M") {
                $scope.Master.RegMaster.push($scope.Borrador.Lineas[i]);
            }
        }

        //console.log("Registros leídos recuperados >> " + JSON.stringify($scope.ReciboEmision.Lineas));
        //console.log("Lotes de Master recuperados >> " + JSON.stringify($scope.Master.RegMaster));
    }

    //*****************************************************************
    //Generar Movimiento
    $scope.Crear = function () {
        if ($scope.ReciboEmision.Lineas.length > 0) {

            //Validación de comunicación con servidor aprox entre 3 y 20 seg respuesta
            //cambiar por URL de instalación http://10.0.0.1/wmsKB2
            //fetch('http://10.0.0.1')
            //    .then(response => {
            //        if (response.ok) {
            //            console.log('VALIDACION SERVER ::: Comunicación exitosa');

                        console.log('Codigo guia enviado: ' + $scope.Memoria.CodeGuia);
                        $scope.Lectura.Movimiento = true;
                        Servicio.Crear($scope.ReciboEmision.Lineas, $scope.Memoria.CodeGuia, $scope.Master.RegMaster).then(function (response) {
                            if (response.codigo === 0) {
                                var _integrados = $scope.DocumentosIntegrados(response.documentos);
                                if (_integrados !== "") {
                                    $scope.Alertas('s', _integrados);
                                }

                                var _noIntegrados = $scope.DocumentosErroneos(response.documentos);
                                if (_noIntegrados !== "") {
                                    $scope.Alertas('e', _noIntegrados);
                                }

                                $scope.Lectura.Movimiento = false;

                                if ($scope.ReciboEmision.Lineas.length === 0) {
                                    $scope.Reiniciar();
                                }
                            }
                            else {
                                $scope.Alertas('e', response.mensaje);
                                $scope.Lectura.Movimiento = false;
                            }
                        });

                //    }
                //    else {
                //        $scope.Lectura.Movimiento = false;
                //        $scope.Alertas('e', "Error de comunicación con servidor, intente nuevamente en 10seg, de lo contrario contacte a su administrador de red");
                //        console.log('VALIDACION SERVER ::: Error en la comunicación');
                //    }
                //})
                //.catch(error => {
                //    $scope.Lectura.Movimiento = false;
                //    $scope.Alertas('e', "Error al conectar con servidor, " + error + ", verifique conexión y salida a internet, intente nuevamente, de lo contrario contacte a su administrador de red");
                //    console.log('VALIDACION SERVER ::: No se pudo conectar al servidor:', error);
                //});
        }
    }

    $scope.DocumentosIntegrados = function (Lista) {
        console.log(Lista);
        var Integrados = "";
        angular.forEach(Lista, function (obj, i) {
            if (obj.Integrado == true) {
                Integrados.length > 0 ? Integrados += "," + obj.DocNum : Integrados = obj.DocNum;
                $scope.CoincidenciasDocBase(obj.OF);
            }
        });
        return Integrados;
    }

    $scope.DocumentosErroneos = function (Lista) {
        var Error = "";
        angular.forEach(Lista, function (val, key) {
            if (val.Integrado == false) {
                Error.length > 0 ? Error += " <br /> OF " + val.OF + " - " + val.MsjError : Error = "OF " + val.OF + " - " + val.MsjError;
            }
        });
        return Error;
    }

    $scope.CoincidenciasDocBase = function (DocBase) {
        console.log('DocBase: ' + DocBase);
        console.log($scope.ReciboEmision.Lineas);

        $scope.ReciboEmision.Lineas.map(function (provider, index) {
            return provider.OF == DocBase ? index : -1;
        }).filter(function (index) {
            return index >= 0;
        }).reverse().forEach(function (index) {
            $scope.ReciboEmision.Lineas.splice(index, 1);
        });
    }

    $scope.Limpiar = function () {
        $scope.ReciboEmision = {
            Turnos: [],
            Turno: '',

            AlmacenSel: '',

            BinCode: '',
            AbsEntry: 0,

            Lineas: []
        }

        $scope.Lectura = {
            Etiqueta: '',
            Movimiento: false
        }

        $scope.Borrador = {
            Lineas: [],
            AlmacenSel: '',
            guid: UUID()
        }

        $scope.Memoria = {
            TurnoEtq: 1,
            FechaErq: '',
            Marcados: [],
            Nlectura: 1,
            CodeGuia: '',
            msgEtq: ''
        }

        $scope.Turnos();
        $scope.CodigoGuia();
    }

    //***********************************************************************/
    $scope.AbrirModalBorrador = function (ListBorradores, turno) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaBorradores.html',
            controller: ListBorradoresController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                BorradoresList: function () {
                    return ListBorradores;
                },
                Turno: function () {
                    return turno;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result !== null && result !== '') {
                $scope.CargarBorrador(result.OF);
            }
        });
    }
    var ListBorradoresController = function (BorradoresList, Turno, $scope, $uibModalInstance) {
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion != null ? $scope.Seleccion : "");
        }
        $scope.Borradores = BorradoresList;
        $scope.Turno = Turno;
        $scope.BorradorSeleccionado = function (val) {
            $scope.Seleccion = val;
            $scope.ok();
        }
    }

    //*****************************************************************
    //Alertas
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

    //******************************************************************************/
    //UUID
    function UUID() {
        var dt = new Date().getTime();
        var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (dt + Math.random() * 16) % 16 | 0;
            dt = Math.floor(dt / 16);
            return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
        return uuid;
    }

    //******************************************************************************/
    //Carga de turnos
    $scope.Turnos();

    //Generacion de codigo guia
    $scope.CodigoGuia();
    //Verificar manejo de Master
    $scope.ValidaMaster();

});