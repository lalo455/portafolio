var calidadApp = angular.module('calidadApp', ['angular-growl', 'ui.bootstrap']);

calidadApp.factory('Ruta', function () {
    return { path: '' }
});

calidadApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

calidadApp.service('Servicio', function ($http, Ruta, $log) {
    this.ObtenerAlmacenes = function (TipoWhs, WhsCode) {
        return $http({
            url: Ruta.path + '/Calidad/ObtenerAlmacenes',
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            },
            data: {
                TipoWhs: TipoWhs
                , WhsCode: WhsCode
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 1 ": response } }
            return error;
        });
    },

    this.GrabarLectura = function (etq, nlectura) {
        return $http({
            url: Ruta.path + '/Calidad/GrabarLectura',
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
    
    this.Actualizar = function (Tipo, Dts, Est, _Destino, _Origen, RegMaster) {
        return $http({
            url: Ruta.path + '/Calidad/Actualizar',
            method: 'POST',
            data: {
                Tipo: Tipo,
                ListaBloq: Dts,
                Estatus: Est,
                Destino: _Destino,
                Origen: _Origen,
                RegMaster: RegMaster
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

    this.ObtenerMaster = function (QR, Origen) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ObtenerMasterDetalle',
            method: 'GET',
            params: {
                QR: QR
                , Origen: Origen
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

calidadApp.controller('calidadController', function ($scope, $window, Ruta, $log, Servicio, growl, $uibModal) {

    $scope.Lectura = {
        Etiqueta: '',
        Procesando: false
    }

    $scope.Calidad = {
        Estatus: '',
        Acciones: [{ Code: 'X', Name: 'Bloquear' }, { Code: 'O', Name: 'Desbloquear' }],
        BinCode: '',
        AbsEntry: 0,

        AlmacenDSel: '',
        ToWhsCodes: [],

        AlmacenOSel: '',
        FromWhsCodes:[],

        Lineas: [], 

        Mover: 0, //Indica si se realizará Trasferencia de Stock o solo se Cambia el Estatus de los Lotes, 0: No Mover | 1: Mover
        Traspaso: 'Y', //Traspaso | Sin Traspaso
        MoverLote: [{ Code: 'N', Name: 'No' }, { Code: 'Y', Name: 'Si' }],

        Bloqueo: 0, //Indica si se realizará Bloqueo o solo se hace la Trasferencia de Stock, 0: No Bloquear | 1: Bloquear
        Bloquear: 'Y', //Bloquear | NO bloquear
        TrBloqueo: [{ Code: 'N', Name: 'No' }, { Code: 'Y', Name: 'Si' }]
    }

    $scope.Memoria = {
        ListaMP: [],  //Lista de etiquetas MP, para verificar que no se haya leido la etiqueta con en el mismo numero
        TipoWhs: 'User', //User: Predeterminados para el usuario | Todos
        Nlectura: 1,
        WhsCode: '', 
        WhsOrigen: [], 
        WhsDestino: [], 
        WhsCuarentena: []
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

    //**********************************************************************************************************************
    //Carga de Almacenes
    $scope.Almacenes = function () {
        Servicio.ObtenerAlmacenes($scope.Memoria.TipoWhs, $scope.Memoria.WhsCode).then(function (response) {
            let tipoWhs;

            switch (response.codigo) {
                case 0:
                    for (var i = 0; i < response.mensaje.length; i++) {
                        tipoWhs = response.mensaje[i].Tipo; //Origen | Destino | Cuarentena

                        switch (tipoWhs) {
                            case "Origen":
                                $scope.Memoria.WhsOrigen.push({
                                    "WhsCode": response.mensaje[i].WhsCode,
                                    "WhsName": response.mensaje[i].WhsName,
                                    "Locked": response.mensaje[i].Locked,
                                    "Tipo": response.mensaje[i].Tipo
                                });
                                break;

                            case "Destino":
                                $scope.Memoria.WhsDestino.push({
                                    "WhsCode": response.mensaje[i].WhsCode,
                                    "WhsName": response.mensaje[i].WhsName,
                                    "Locked": response.mensaje[i].Locked,
                                    "Tipo": response.mensaje[i].Tipo
                                });
                                break;

                            case "Cuarentena":
                                $scope.Memoria.WhsCuarentena.push({
                                    "WhsCode": response.mensaje[i].WhsCode,
                                    "WhsName": response.mensaje[i].WhsName,
                                    "Locked": response.mensaje[i].Locked,
                                    "Tipo": response.mensaje[i].Tipo
                                });
                                break;
                        }
                    }

                    console.log($scope.Memoria.WhsOrigen);
                    console.log($scope.Memoria.WhsDestino);
                    console.log($scope.Memoria.WhsCuarentena);
                    break;

                case -1: //Advertencia: No hay almacenes configurados
                    $scope.Alertas('w', "El usuario no tiene configurado Almacenes, verifique con su administrador");
                    break

                case -2: //Error: Error al consultar
                    $scope.Alertas('e', "Error al consultar información de Almacenes, contacte al administrador de sistema");
                    break;
            }               
        });
    }

    //Cambio de Status a Actualizar
    $scope.limpiarLineas = function () {
        console.log("Cambio de estatus a actualizar: " + $scope.Calidad.Estatus);

        $scope.Calidad.Lineas = [];
        $scope.Memoria.ListaMP = [];
        
        $scope.Calidad.FromWhsCodes = []; //Lista almacenes Origen
        $scope.Calidad.AlmacenOSel = ""; //Origen seleccionado
        $scope.Calidad.ToWhsCodes = []; //Lista almacenes Destino
        $scope.Calidad.AlmacenDSel = ""; //Destino seleccionado

        switch ($scope.Calidad.Estatus) {
            case 'X': //Bloquear
                //Almacén origen: Predeterminado
                if ($scope.Memoria.WhsOrigen.length == 0) {
                    $scope.Alertas('w', "El usuario no tiene configurado Almacén Origen, verifique con su administrador");
                }
                else {
                    $scope.Calidad.FromWhsCodes = $scope.Memoria.WhsOrigen;
                    //$scope.Calidad.AlmacenOSel = $scope.Calidad.FromWhsCodes[0].WhsCode;
                }

                //Almacen Destino: Cuarentena
                if ($scope.Memoria.WhsCuarentena.length == 0) {
                    $scope.Alertas('w', "El usuario no tiene configurado Almacén de Cuarentena, verifique con su administrador");
                }
                else {
                    $scope.Calidad.ToWhsCodes = $scope.Memoria.WhsCuarentena;
                    $scope.Calidad.AlmacenDSel = $scope.Calidad.ToWhsCodes[0].WhsCode;
                }
                break;

            case 'O': //Desbloquear
                //Almacen Origen: Cuarentena
                if ($scope.Memoria.WhsCuarentena.length == 0) {
                    $scope.Alertas('w', "El usuario no tiene configurado Almacén de Cuarentena, verifique con su administrador");
                }
                else {
                    for (var r = 0; r < $scope.Memoria.WhsCuarentena.length; r++) {
                        $scope.Calidad.FromWhsCodes.push({
                            "WhsCode": $scope.Memoria.WhsCuarentena[r].WhsCode,
                            "WhsName": $scope.Memoria.WhsCuarentena[r].WhsName,
                            "Locked": $scope.Memoria.WhsCuarentena[r].Locked,
                            "Tipo": $scope.Memoria.WhsCuarentena[r].Tipo
                        });
                    }
                    //$scope.Calidad.AlmacenOSel = $scope.Calidad.FromWhsCodes[0].WhsCode;
                }

                //Almacén origen: Predeterminado
                if ($scope.Memoria.WhsOrigen.length == 0) {
                    $scope.Alertas('w', "El usuario no tiene configurado Almacén Origen, verifique con su administrador");
                }
                else {
                    for (var w = 0; w < $scope.Memoria.WhsOrigen.length; w++) {
                        $scope.Calidad.FromWhsCodes.push({
                            "WhsCode": $scope.Memoria.WhsOrigen[w].WhsCode,
                            "WhsName": $scope.Memoria.WhsOrigen[w].WhsName,
                            "Locked": $scope.Memoria.WhsOrigen[w].Locked,
                            "Tipo": $scope.Memoria.WhsOrigen[w].Tipo
                        });
                    }
                    //$scope.Calidad.AlmacenOSel = $scope.Calidad.FromWhsCodes[0].WhsCode;
                }

                //Almacen Destino: Predeterminado
                if ($scope.Memoria.WhsDestino.length == 0) {
                    $scope.Alertas('w', "El usuario no tiene configurado Almacén Destino, verifique con su administrador");
                }
                else {
                    $scope.Calidad.ToWhsCodes = $scope.Memoria.WhsDestino;
                    //$scope.Calidad.AlmacenDSel = $scope.Calidad.ToWhsCodes[0].WhsCode;
                }
                break;
        }
    }

    //Cambio de Traspaso | si | no
    $scope.TraspasoLt = function () {
        if ($scope.Calidad.Estatus == 'O' && $scope.Calidad.Traspaso == 'N') {
            $scope.Calidad.AlmacenDSel = "";
        }
    }

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
    //**********************************************************************************************************************
    //Funciones
    $scope.LecturaQR = function () {
        //Grabar informacion
        Servicio.GrabarLectura($scope.Lectura.Etiqueta, $scope.Memoria.Nlectura).then(function (response) //Revisa el estatus de la etiqueta 
        {
            console.log(response.mensaje.length + ' --- # ' + $scope.Memoria.Nlectura);
            $scope.Memoria.Nlectura++;
        });


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
                $scope.ObtenerInfo();
            }
        }
        else {
            //Procesamiento de ETIQUETA UNICA
            console.log('Procesamiento de ETIQUETA UNICA');
            $scope.ObtenerInfo();
        }
    }

    //******************************************************************************************************/
    //Etiqueta Individual
    $scope.ObtenerInfo = function () {
        Servicio.ObtenerLote($scope.Lectura.Etiqueta).then(function (response) {
            if (response.codigo === 0) {
                console.log('Almacen origen: ' + $scope.Calidad.AlmacenOSel);

                //Se valida si la Etiqueta esta cancelada
                if (response.mensaje[0].Cancelado === 'Y') {
                    $scope.Alertas('w', 'La etiqueta esta cancelada');
                    $scope.Lectura.Etiqueta = '';
                    $("#Entradas").focus();
                    return;
                }

                //Validar Informacion SAP, Etiqueta con respecto al Almacen Origen Seleccionado
                var result = $scope.FiltrarLotes(response.mensaje, $scope.Lectura.Etiqueta);
                switch (result.length) {
                    case 0:
                        $scope.Alertas('w', "No se encontro información de etiqueta en Almacén Origen " + $scope.Calidad.AlmacenOSel);
                        $scope.Lectura.Etiqueta = '';
                        $("#Entradas").focus();
                        break;

                    case 1:
                        if (!$scope.ConfirmarStatus(response.mensaje[0].Estatus)) {
                            $scope.AgregarLinea(result, $scope.Lectura.Etiqueta);
                        }
                        else {
                            if ($scope.Calidad.Estatus === "O") {
                                $scope.Alertas('w', 'El Lote ya se encuentra DESBLOQUEADO.');
                            }
                            else {
                                $scope.Alertas('w', 'El Lote ya se encuentra BLOQUEADO.');
                            }
                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();
                        }
                        break;

                    default:
                        $scope.AbrirModalLotes(result);
                        break;
                }
            }
            else {
                $scope.Alertas('e', response.mensaje);
                $scope.Lectura.Etiqueta = '';
                $("#Entradas").focus();
            }
        }); 
    }
    //******************************************************************************************************/
    $scope.ProcesaEtiquetaMaster = function () {

        if (!$scope.CondicionLinea("M", $scope.Lectura.Etiqueta.split('&')[0], 0)) {
            Servicio.ObtenerMaster($scope.Lectura.Etiqueta, $scope.Calidad.AlmacenOSel).then(function (response) {
                $scope.Master.LineasMaster = [];
                $scope.Master.ErrorLineasMaster = [];
                $scope.Master.LotesMaster = [];
                $scope.Master.ErrorLotesMaster = [];
                $scope.Master.MasterRegUnica = [];
                let QtyTotalLotes = 0;

                switch (response.codigo) {
                    case 0:
                        //Información de master obtenida | Master Existe | TipoReg 1: Info Master | TipoReg 2: Etiquetas de Master | TipoReg 3: Lotes de Master en Almacén origen
                        /*
                        MASTER SE LEE COMPLETA, NO ES EDITABLE, NO HAY PARCIALES
                        1.- Validar Master : Liberada 
                        2.- Validar Master : No bloqueda
                        
                        3.- Recorrer lista de Etiquetas
                        3.1.- Validar estatus : Liberada
                        3.2.- Validar calidad : No bloqueada
                        3.3.- Validar etiquetas completas y correctas | Falla alguna : NO se procesa Master
                        
                        4.- Sobre recorrido de Lista de etiquetas, revisar Lotes
                        4.1.- Validar estatus: 0 > Liberado | 1 > No accesible | 2 > Bloquedo | TODOS LOS LOTES DEBEN TENER EL MISMO ESTATUS
                        4.2.- Validar Lotes completos y correctos | Falla alguno : NO se procesa Master
                        4.3.- Validar Cantidad Etiqueta y Stock SAP | Cantidad etiqueta menor o igual : Se deja Cantidad Etiqueta | Cantidad etiqueta mayor: No se procesa Lote | Opcional: se guarda info de lote y stock insuficiente
                        
                        5.- Validar Etiquetas y Lotes | No son iguales : NO se procesa Master
                        5.1.- Validar Cantidad Master contra Cantidad Lotes | No es igual : NO se procesa Master
                        */

                        if (response.mensaje[0].TipoReg === "1") {
                            if (response.mensaje[0].Estatus === 'L' && response.mensaje[0].Calidad === 'O') {

                                let QtyEtqM = new Decimal($scope.Lectura.Etiqueta.split('&')[2].replace(",", ""));
                                let qtyMaster = new Decimal(response.mensaje[0].MasterOnHandQty);
                                if (QtyEtqM.toNumber() === qtyMaster.toNumber()) {

                                    for (var i = 0; i < response.mensaje.length; i++) {
                                        //Detalle de etiquetas
                                        if (response.mensaje[i].TipoReg === "2") {
                                            if (response.mensaje[i].EtqLoteEstatus === 'L' && response.mensaje[i].EtqLoteCalidad === 'O') {
                                                //Validar Lote no se haya leido de forma individualmente
                                                if (!$scope.CondicionLinea("ML", response.mensaje[i].EtqLote, 0)) {
                                                    $scope.Master.LineasMaster.push(response.mensaje[i]); //Etiqueta individual correcta
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
                                                    "Error": "No se encuentra Liberada o se encuentra Bloqueada"
                                                });
                                            }
                                        }

                                        //Detalle de lotes
                                        if (response.mensaje[i].TipoReg === "3") {
                                            //Validar estatus de cada lote de acuerdo con el estatus a actualizar
                                            if (!$scope.ConfirmarStatus(response.mensaje[i].LoteEstatus)) {
                                                //Validar cantidades
                                                let valorQtyEtiqueta = new Decimal(response.mensaje[i].EtqLoteOnHandQty);
                                                let valorQtySAP = new Decimal(response.mensaje[i].OnHandQty);
                                                let valorTotalQty = new Decimal(QtyTotalLotes);

                                                if (valorQtyEtiqueta.toNumber() === valorQtySAP.toNumber()) {
                                                    $scope.Master.LotesMaster.push(response.mensaje[i]); //Lotes correctos
                                                    QtyTotalLotes = valorTotalQty.plus(valorQtyEtiqueta).toNumber();
                                                }
                                                else if (valorQtyEtiqueta.toNumber() < valorQtySAP.toNumber()) {
                                                    $scope.Master.LotesMaster.push(response.mensaje[i]); //Lotes correctos
                                                    QtyTotalLotes = valorTotalQty.plus(valorQtyEtiqueta).toNumber();
                                                }
                                                else {
                                                    $scope.Master.ErrorLotesMaster.push({
                                                        "DistNumber": response.mensaje[i].Lote,
                                                        "Error": "No cuenta con stock suficiente en Almacén Origen"
                                                    });
                                                }
                                            }
                                            else {
                                                if ($scope.Calidad.Estatus === "O") {
                                                    console.log('w', 'El Lote ya se encuentra DESBLOQUEADO.');
                                                    $scope.Master.ErrorLotesMaster.push({
                                                        "DistNumber": response.mensaje[i].Lote,
                                                        "Error": "ya se encuentra DESBLOQUEADO."
                                                    });
                                                }
                                                else {
                                                    console.log('w', 'El Lote ya se encuentra BLOQUEADO.');
                                                    $scope.Master.ErrorLotesMaster.push({
                                                        "DistNumber": response.mensaje[i].Lote,
                                                        "Error": "ya se encuentra BLOQUEADO."
                                                    });
                                                }
                                            }
                                        }
                                    }

                                    console.log($scope.Master.LotesMaster);
                                    console.log($scope.Master.ErrorLotesMaster);

                                    console.log($scope.Master.LineasMaster);
                                    console.log($scope.Master.ErrorLineasMaster);

                                    if ($scope.Master.ErrorLineasMaster.length === 0 && $scope.Master.ErrorLotesMaster.length === 0) {
                                        if ($scope.Master.LineasMaster.length === $scope.Master.LotesMaster.length) {

                                            let qtyLotes = new Decimal(QtyTotalLotes);
                                            if (qtyMaster.toNumber() === qtyLotes.toNumber()) {
                                                console.log('PROCESAR MASTER | agregar información de master visual | no se puede editar');

                                                for (var r = 0; r < $scope.Master.LotesMaster.length; r++) {
                                                    $scope.Master.RegMaster.push({
                                                        "DistNumber": $scope.Master.LotesMaster[r].Lote
                                                        , "BinCode": $scope.Master.LotesMaster[r].BinCode
                                                        , "AbsEntry": $scope.Master.LotesMaster[r].AbsEntry
                                                        , "ItemCode": $scope.Master.LotesMaster[r].ItemCode
                                                        , "ItemName": $scope.Master.LotesMaster[r].ItemName
                                                        , "OnHandQty": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "WhsCode": $scope.Master.LotesMaster[r].WhsCode
                                                        , "NewBinCode": $scope.Calidad.BinCode
                                                        , "NewAbsEntry": $scope.Calidad.AbsEntry
                                                        , "Estatus": $scope.Master.LotesMaster[r].LoteEstatus
                                                        , "Cancelado": "N"
                                                        , "TipoLote": $scope.Master.LotesMaster[r].Tipo
                                                        , "EtqMaster": response.mensaje[0].Codigo
                                                        , "QtySAP": $scope.Master.LotesMaster[r].OnHandQty
                                                        , "StockSAP": $scope.Master.LotesMaster[r].OnHandQty

                                                        , "Dispositivo": $scope.Master.LotesMaster[r].Dispositivo
                                                        , "PesoBruto": "0"
                                                        , "PesoNeto": "0"
                                                        , "NumEtq": "1"
                                                        , "QtyEtq": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "QtyTraspaso": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "QtyEmbarque": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "Etqs": 1
                                                        , "TipoLectura": "M"
                                                    });

                                                    $scope.Master.MasterRegUnica.push({
                                                        "DistNumber": $scope.Master.LotesMaster[r].Lote
                                                        , "BinCode": $scope.Master.LotesMaster[r].BinCode
                                                        , "AbsEntry": $scope.Master.LotesMaster[r].AbsEntry
                                                        , "ItemCode": $scope.Master.LotesMaster[r].ItemCode
                                                        , "ItemName": $scope.Master.LotesMaster[r].ItemName
                                                        , "OnHandQty": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "WhsCode": $scope.Master.LotesMaster[r].WhsCode
                                                        , "NewBinCode": $scope.Calidad.BinCode
                                                        , "NewAbsEntry": $scope.Calidad.AbsEntry
                                                        , "Estatus": $scope.Master.LotesMaster[r].LoteEstatus
                                                        , "Cancelado": "N"
                                                        , "TipoLote": $scope.Master.LotesMaster[r].Tipo
                                                        , "EtqMaster": response.mensaje[0].Codigo
                                                        , "QtySAP": $scope.Master.LotesMaster[r].OnHandQty
                                                        , "StockSAP": $scope.Master.LotesMaster[r].OnHandQty

                                                        , "Dispositivo": $scope.Master.LotesMaster[r].Dispositivo
                                                        , "PesoBruto": "0"
                                                        , "PesoNeto": "0"
                                                        , "NumEtq": "1"
                                                        , "QtyEtq": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "QtyTraspaso": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "QtyEmbarque": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "Etqs": 1
                                                        , "TipoLectura": "M"
                                                    });
                                                }
                                                console.log($scope.Master.RegMaster);
                                                console.log($scope.Master.MasterRegUnica);

                                                $scope.Calidad.Lineas.push({
                                                    "DistNumber": response.mensaje[0].Codigo
                                                    , "ItemCode": $scope.Master.MasterRegUnica[0].ItemCode
                                                    , "WhsCode": $scope.Master.MasterRegUnica[0].WhsCode
                                                    , "BinCode": $scope.Master.MasterRegUnica[0].BinCode
                                                    , "AbsEntry": $scope.Master.MasterRegUnica[0].AbsEntry
                                                    , "NewBinCode": $scope.Calidad.BinCode
                                                    , "NewAbsEntry": $scope.Calidad.AbsEntry
                                                    , "OnHandQty": response.mensaje[0].MasterOnHandQty
                                                    , "Etqs": $scope.Master.MasterRegUnica.length
                                                    , "TipoLectura": "M"
                                                });

                                                console.log($scope.Calidad.Lineas);
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
                                            console.log("Verificar Lotes de etiqueta master, no estan completos en almacén origen");
                                            $scope.Alertas('e', "Etiqueta Master incompleta, no todos los lotes se encuentran en Almacén Origen, verifique en SAP");
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

                                        for (var y = 0; y < $scope.Master.ErrorLotesMaster.length; y++) {
                                            msjErrores = msjErrores + "<br/> > El Lote " + $scope.Master.ErrorLotesMaster[y].DistNumber + " - " + $scope.Master.ErrorLotesMaster[y].Error;
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
                                $scope.Alertas('e', "La Etiqueta Master no se encuentra Liberada o se encuentra Bloqueda, verificar en SAP y volver a leer");
                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();
                            }
                        }
                        else {
                            $scope.Alertas('e', "Error en lectura e información obtenida, favor de contactar a Administrador y verificar Etiqueta Master en SAP");
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
        }
        else {
            $scope.Alertas('w', "La etiqueta master " + $scope.Lectura.Etiqueta.split('&')[0] + " ya ha sido leída");
            $scope.Lectura.Etiqueta = '';
            $("#Entradas").focus();
        }
    }

    $scope.CondicionLinea = function (_tipoVal, value, i) {
        var status = false;
        switch (_tipoVal) {
            case "ML"://Lote de PT unico en la tabla
                for (var t = 0; t < $scope.Calidad.Lineas.length; t++) {
                    if ($scope.Calidad.Lineas[t].DistNumber === value) {
                        status = true;
                        $scope.Lectura.Etiqueta = '';
                        break;
                    }
                }
                break;

            case "M"://Master unica en la tabla
                for (var t = 0; t < $scope.Calidad.Lineas.length; t++) {
                    if ($scope.Calidad.Lineas[t].DistNumber === value) {
                        status = true;
                        $scope.Lectura.Etiqueta = '';
                        break;
                    }
                }
                break;
        }
        return status;
    }
    //******************************************************************************************************/
    //Validacion de Informacion SAP y Etiqueta, de acuerdo al Almacen Seleccionado
    $scope.FiltrarLotes = function (response, qr) {
        var result = [];
        for (var r = 0; r < response.length; r++) {
            if (qr.split("&")[0] == response[r].DistNumber &&
                qr.split("&")[1] == response[r].ItemCode &&
                response[r].WhsCode == $scope.Calidad.AlmacenOSel) {
                    result.push(response[r]);
            }    
        }

        return result; //Lista de registros a procesar
    }

    //Procesar información de registro
    $scope.AgregarLinea = function (response, qr) {
        //*********************************************
        // 1: Validar que la etiqueta no haya sido leida anteriormente PT[Lote] | MP[Lote, Articulo, Origen, Destino y Num Etq]
        // 2: Verificar que el acumulado no sobrepase el total de stock del lote
        // 3: Agregar informacion de lista de lotes leidos
        //*********************************************
        var indice = -1;
        var procede = -1;
        var reg = -1;
        switch (response[0].Tipo) {
            case "PT":
                indice = $scope.GruposLineas(qr, response, 'D');
                if (indice !== -1) {
                    $scope.Alertas("e", "La etiqueta ya ha sido leida");
                    $scope.Lectura.Etiqueta = '';
                    $("#Entradas").focus();
                }
                else {
                    Object.assign(response[0], {
                        "StockSAP": response[0].OnHandQty,
                        "Etqs": 1,
                        "TipoLectura": "I"
                    });

                    if (response[0].OnHandQty > $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")) {
                        response[0].OnHandQty = $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");
                    }

                    //Destino
                    response[0].NewBinCode = $scope.Calidad.BinCode;
                    response[0].NewAbsEntry = $scope.Calidad.AbsEntry;

                    $scope.Calidad.Lineas.push(response[0]);
                    $scope.Lectura.Etiqueta = '';
                    $("#Entradas").focus();
                }                
                break;

            case "MP":
                if ($scope.Memoria.ListaMP.length > 0) {
                    indice = $scope.GruposLineas(qr, response, 'D');
                }

                if (indice !== -1) {
                    $scope.Alertas("e", "La etiqueta ya ha sido leida");
                    $scope.Lectura.Etiqueta = '';
                    $("#Entradas").focus();
                }
                else {
                    //Registro MP en Lista de registros
                    reg = $scope.GruposLineas(qr, response, 'D');
                    switch (reg) {
                        case -1: //Nuevo registro | verificamos si cumple las 4 condiciones para genera una nueva linea
                            Object.assign(response[0], {
                                "Etqs": 1
                                , "StockSAP": response[0].OnHandQty
                                , "NumEtq": $scope.Lectura.Etiqueta.split('&')[4]
                                , "TipoLectura": "I"
                            });

                            //Validar cantidad no sobrepase stock                            
                            procede = $scope.GruposLineas(qr, response, 'O'); //-1: Procede | //-2 NO procede [Supera stock]
                            if (procede === -1) {
                                //Destino
                                response[0].NewBinCode = $scope.Calidad.BinCode;
                                response[0].NewAbsEntry = $scope.Calidad.AbsEntry;

                                $scope.Calidad.Lineas.push(response[0]);
                                console.log($scope.Calidad.Lineas);

                                //*****************************************/
                                Object.assign(response[0], {
                                    "NumEtq": $scope.Lectura.Etiqueta.split('&')[4],
                                    "QtyEtq": $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                                });

                                $scope.Memoria.ListaMP.push(response[0]); //Se agrega información de etiqueta a lista de etiquetas MP
                                console.log($scope.Memoria.ListaMP);
                                //*****************************************/
                            }

                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();
                            break;

                        default: //Registro existente
                            //Validar cantidad no sobrepase stock                            
                            procede = $scope.GruposLineas(qr, response, 'O'); //-1: Procede | //-2 NO procede [Supera stock]
                            if (procede === -1) {
                                //$scope.Calidad.Lineas[i].OnHandQty = parseFloat($scope.Calidad.Lineas[i].OnHandQty) + parseFloat(qr.split('&')[2].replace(",", ""));                                
                                let valorOnHandQty = new Decimal($scope.Calidad.Lineas[reg].OnHandQty);
                                let valorQtyEtiqueta = new Decimal(qr.split('&')[2].indexOf(",") != -1 ? qr.split('&')[2].replace(",", "") : qr.split('&')[2]);

                                $scope.Calidad.Lineas[reg].OnHandQty = valorOnHandQty.plus(valorQtyEtiqueta).toNumber();
                                $scope.Calidad.Lineas[reg].Etqs++;
                                console.log($scope.Calidad.Lineas);

                                //*****************************************/
                                Object.assign(response[0], {
                                    "NumEtq": $scope.Lectura.Etiqueta.split('&')[4]
                                    , "QtyEtq": $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                                })

                                $scope.Memoria.ListaMP.push(response[0]); //Se agrega información de etiqueta a lista de etiquetas MP
                                console.log($scope.Memoria.ListaMP);         
                                //*****************************************/
                            }

                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();
                            break;
                    }
                }
                break;
        }
    }

    //Verificar STATUS del Lote en SAP, y comparar con el status seleccionado
    $scope.ConfirmarStatus = function (value) {
        var st = false;
        switch (value) {
            case "0"://Liberado
                if ($scope.Calidad.Estatus == "O") {
                    st = true;
                }
                break;

            case "2"://Bloqueado
                if ($scope.Calidad.Estatus == "X") {
                    st = true;
                }
                break;
        }

        return st;
    }

    //Validaciones de registros
    $scope.GruposLineas = function (qr, response, tipo) {
        let index = -1;
        let Lineas = $scope.Calidad.Lineas;
        let sum = 0;
        let StockSap = 0;
        let j = -1;
        let acum = 0;

        let valorQtyEtiqueta;
        let valorOnHandQty;
        let acumOnHandQty;
        let valorQtySAP;
        let QtySAP;

        switch (tipo) {
            //Verificar: Existe => Se indica el indice del registro encntrado en Lista de Lotes a Transferir
            case 'D': //Articulo, Lote, Origen, Destino | [Lote Individual(unica)]
                console.log($scope.Calidad.Lineas);
                switch (response[0].Tipo) {
                    case "PT":
                        index = Lineas.findIndex(linea => linea.DistNumber === response[0].DistNumber && linea.ItemCode === response[0].ItemCode);

                        //Validar si existe Master leida | validar lote individual en master leida
                        if ($scope.Master.RegMaster.length > 0) {
                            for (var m = 0; m < $scope.Master.RegMaster.length; m++) {
                                if ($scope.Master.RegMaster[m].DistNumber === response[0].DistNumber) {
                                    index = m;
                                    $scope.Alertas("w", "La etiqueta ya existe en Master Leida " + $scope.Master.RegMaster[m].EtqMaster);
                                    break;
                                }
                            }
                        }
                        break;

                    case "MP":
                        console.log($scope.Memoria.ListaMP);
                        console.log('Num Etq: ' + qr.split('&')[4]);

                        for (var t = 0; t < $scope.Memoria.ListaMP.length; t++) {
                            console.log($scope.Memoria.ListaMP[t].DistNumber + ' --- ' +
                                        response[0].DistNumber + ' --- ' +
                                        $scope.Memoria.ListaMP[t].ItemCode + ' --- ' +
                                        response[0].ItemCode + ' --- ' +
                                        $scope.Memoria.ListaMP[t].NumEtq + ' --- ' +
                                        qr.split('&')[4]);

                            if ($scope.Memoria.ListaMP[t].DistNumber === response[0].DistNumber &&
                                $scope.Memoria.ListaMP[t].ItemCode === response[0].ItemCode) {

                                if ($scope.Memoria.ListaMP[t].NumEtq == qr.split('&')[4]) {
                                    index = t;
                                    break;
                                }

                            }
                        }
                        break;
                }
                break;

            
            case 'R'://MP : Articulo, Lote, Origen, Destino
                index = Lineas.findIndex(linea => linea.DistNumber === response[0].DistNumber &&
                    linea.ItemCode === response[0].ItemCode &&
                    linea.AbsEntry === response[0].AbsEntry &&
                    linea.NewAbsEntry === response[0].NewAbsEntry);
                break;


            case 'O'://Articulo, Lote, Origen

                for (let m = 0; m < Lineas.length; m++) {
                    valorOnHandQty = new Decimal(Lineas[m].OnHandQty);
                    acumOnHandQty = new Decimal(sum);

                    if (Lineas[m].DistNumber == qr.split('&')[0] &&
                        Lineas[m].ItemCode == qr.split('&')[1] &&
                        Lineas[m].AbsEntry == response[0].AbsEntry) {

                        //En caso de existir, se acumula la Cantidad de los Registros
                        sum = acumOnHandQty.plus(valorOnHandQty).toNumber();

                        if (StockSap === 0) {
                            StockSap = Lineas[m].StockSAP;

                            valorQtySAP = new Decimal(Lineas[m].StockSAP);
                            QtySAP = valorQtySAP.toNumber();

                            j = m;
                        }
                    }
                }

                console.log('StockSAP: ' + QtySAP +
                    ' | Acumulado: ' + sum +
                    ' | Qty Etq: ' + qr.split('&')[2]);

                //Se agrega al acumulado anterior la cantidad de la etiqueta
                valorQtyEtiqueta = new Decimal(qr.split('&')[2].indexOf(",") != -1 ? qr.split('&')[2].replace(",", "") : qr.split('&')[2]);
                acumOnHandQty = new Decimal(sum);
                //acum = parseFloat(sum) + parseFloat(qr.split('&')[2].indexOf(",") != -1 ? qr.split('&')[2].replace(",", "") : qr.split('&')[2]);
                acum = acumOnHandQty.plus(valorQtyEtiqueta).toNumber();

                if (j >= 0) {
                    //Se valida que la cantidad Acumulada NO SOBREPASE la cantidad del Lote SAP
                    if (trunc(acum) > parseFloat(QtySAP)) {
                        $scope.Alertas("w", "Esta excediendo el stock disponible para el artículo" + response[0].ItemCode +
                            ", NO PUEDE superar la cantidad de: " + QtySAP);
                        index = -2;
                        break;
                    }
                }
                break;
        }

        return index;
    }

    //Borrar registro
    $scope.Eliminar = function (i) {
        //Eliminar registros
        // i => reg a eliminar en lineas leídas
        let TipoLectura = $scope.Calidad.Lineas[i].TipoLectura; // I: Individual | M: Master

        if (TipoLectura === "I") {
            console.log('Lote: ' + $scope.Calidad.Lineas[i].DistNumber);

            //Por cada uno de los valores buscar y eliminar lotes MP
            for (var h = $scope.Memoria.ListaMP.length - 1; h >= 0; h--) {
                if ($scope.Memoria.ListaMP[h].DistNumber === $scope.Calidad.Lineas[i].DistNumber) {
                    console.log('Lote: ' + $scope.Calidad.Lineas[i].DistNumber + ' -- Posicion: ' + h);
                    console.log("El lote " + $scope.Memoria.ListaMP[h].DistNumber + " -- con Num. " + $scope.Memoria.ListaMP[h].NumEtq + ", ELIMINADO");

                    //Eliminar lote MP
                    $scope.Memoria.ListaMP.splice(h, 1);
                }
            }

            $scope.Calidad.Lineas.splice(i, 1);

            if ($scope.Calidad.Lineas.length === 0) {
                $scope.Memoria.Nlectura = 1;
            }
        }
        else {
            //Eliminar registros individuales de Master
            let MasterCodigo = $scope.Calidad.Lineas[i].DistNumber;

            //Por cada uno de los valores buscar y eliminar lotes de Master
            for (var h = $scope.Master.RegMaster.length - 1; h >= 0; h--) {
                if ($scope.Master.RegMaster[h].EtqMaster === MasterCodigo) {
                    console.log("Lote " + $scope.Master.RegMaster[h].DistNumber + ", ELIMINADO");

                    //Eliminar lote 
                    $scope.Master.RegMaster.splice(h, 1);
                }
            }

            $scope.Calidad.Lineas.splice(i, 1);

            if ($scope.Calidad.Lineas.length === 0) {
                $scope.Memoria.Nlectura = 1;
            }

            console.log("Lotes de Master restantes >> " + JSON.stringify($scope.Master.RegMaster));
            console.log("Registros leídos restantes >> " + JSON.stringify($scope.Calidad.Lineas));
        }        
    }

    //**********************************************************************************************************************
    $scope.Actualizar = function () {
        console.log('Lineas leidas: >>>>>>');
        console.log($scope.Calidad.Lineas); //Lineas actuales a procesar
        console.log('Registros master');
        console.log($scope.Master.RegMaster); //Lineas master a procesar

        if ($scope.Calidad.Estatus != '') {
            console.log("Generar Movimiento >> " + JSON.stringify($scope.Calidad.Lineas));
            $scope.Lectura.Procesando = true;

            //Desbloqueo
            if ($scope.Calidad.Estatus === 'O') {
                //Traspaso
                if ($scope.Calidad.Traspaso === 'Y') {
                    console.log('Proceso: Desbloqueo con Traspaso');

                    //Validar que se haya seleccionado Almacen Destino
                    if ($scope.Calidad.AlmacenDSel !== "") {
                        //SI
                        //Se realiza Desbloqueo y Traspaso | Registros , Estatus , Destino , Origen
                        Servicio.Actualizar('Tipo1', $scope.Calidad.Lineas, $scope.Calidad.Estatus, $scope.Calidad.AlmacenDSel, $scope.Calidad.AlmacenOSel, $scope.Master.RegMaster).then(function (response) {
                            if (response.codigo == 0) {
                                $scope.Alertas('s', response.mensaje);
                                $scope.Lectura.Procesando = false;
                                $scope.Limpiar();
                            }
                            else {
                                $scope.Alertas('e', response.mensaje);
                                $scope.Lectura.Procesando = false;
                            }
                        });
                    }
                    else {
                        //NO
                        //Se notifica que se debe seleccionar Almacen Destino
                        $scope.Alertas('w', 'Debe seleccionar Almacen Destino.');
                        $scope.Lectura.Procesando = false;
                    }
                }
                else {
                    //Solo se procesa el Desbloqueo
                    console.log('Proceso: SOLO Desbloqueo');
                    Servicio.Actualizar('Tipo2', $scope.Calidad.Lineas, $scope.Calidad.Estatus, $scope.Calidad.AlmacenDSel, $scope.Calidad.AlmacenOSel, $scope.Master.RegMaster).then(function (response) {
                        if (response.codigo == 0) {
                            $scope.Alertas('s', response.mensaje);
                            $scope.Lectura.Procesando = false;
                            $scope.Limpiar();
                        }
                        else {
                            $scope.Alertas('e', response.mensaje);
                            $scope.Lectura.Procesando = false;
                        }
                    });
                }
            }
            else {
                //Bloqueo
                //Traspaso
                if ($scope.Calidad.Traspaso === 'Y') {
                    //Con Bloqueo
                    if ($scope.Calidad.Bloquear === 'Y') {
                        console.log('Proceso: Bloqueo con Traspaso');

                        //SI
                        //Se realiza Bloqueo y Traspaso

                        //Se agrega la validación de Lotes | Solo para el BLOQUEO
                        //OnHandQty debe ser igual al StockSAP			
                        for (var c = 0; c < $scope.Calidad.Lineas.length; c++) {
                            console.log('Lote: ' + $scope.Calidad.Lineas[c].DistNumber +
                                ' --- OnHandQty: ' + $scope.Calidad.Lineas[c].OnHandQty +
                                ' ---- StockSAP: ' + $scope.Calidad.Lineas[c].StockSAP +
                                ' ---- Tipo Lectura: ' + $scope.Calidad.Lineas[c].TipoLectura);
                                
                            if ($scope.Calidad.Lineas[c].TipoLectura === "I" && ($scope.Calidad.Lineas[c].OnHandQty !== $scope.Calidad.Lineas[c].StockSAP)) {
                                $scope.Alertas('w', 'El Lote ' + $scope.Calidad.Lineas[c].DistNumber +
                                    ' cuenta con ' + $scope.Calidad.Lineas[c].StockSAP +
                                    ', se esta enviando ' + $scope.Calidad.Lineas[c].OnHandQty);

                                return;
                            }
                        }

                        //**************/
                        for (var m = 0; m < $scope.Master.RegMaster.length; m++) {
                            console.log('Lote: ' + $scope.Master.RegMaster[m].DistNumber +
                                ' --- OnHandQty: ' + $scope.Master.RegMaster[m].OnHandQty +
                                ' ---- StockSAP: ' + $scope.Master.RegMaster[m].StockSAP);

                            if ($scope.Master.RegMaster[m].OnHandQty !== $scope.Master.RegMaster[m].StockSAP) {
                                $scope.Alertas('w', 'El Lote ' + $scope.Master.RegMaster[m].DistNumber + ' de la Master ' + $scope.Master.RegMaster[m].EtqMaster +
                                    ' cuenta con ' + $scope.Master.RegMaster[m].StockSAP +
                                    ', se esta enviando ' + $scope.Master.RegMaster[m].OnHandQty);

                                return;
                            }
                        }

                        Servicio.Actualizar('Tipo1', $scope.Calidad.Lineas, $scope.Calidad.Estatus, $scope.Calidad.AlmacenDSel, $scope.Calidad.AlmacenOSel, $scope.Master.RegMaster).then(function (response) {
                            if (response.codigo == 0) {
                                $scope.Alertas('s', response.mensaje);
                                $scope.Lectura.Procesando = false;
                                $scope.Limpiar();
                            }
                            else {
                                $scope.Alertas('e', response.mensaje);
                                $scope.Lectura.Procesando = false;
                            }
                        });
                    }
                    else {
                        console.log('Proceso: Traspaso SIN Bloqueo');
                        //NO
                        //Se realiza SOLO Traspaso
                        Servicio.Actualizar('Tipo3', $scope.Calidad.Lineas, $scope.Calidad.Estatus, $scope.Calidad.AlmacenDSel, $scope.Calidad.AlmacenOSel, $scope.Master.RegMaster).then(function (response) {
                            if (response.codigo == 0) {
                                $scope.Alertas('s', response.mensaje);
                                $scope.Lectura.Procesando = false;
                                $scope.Limpiar();
                            }
                            else {
                                $scope.Alertas('e', response.mensaje);
                                $scope.Lectura.Procesando = false;
                            }
                        });
                    }
                }
                else {
                    //Con Bloqueo
                    if ($scope.Calidad.Bloquear === 'Y') {
                        console.log('Proceso: SOLO Bloqueo');
                        //SI
                        //Se realiza SOLO Bloqueo
                        Servicio.Actualizar('Tipo2', $scope.Calidad.Lineas, $scope.Calidad.Estatus, $scope.Calidad.AlmacenDSel, $scope.Calidad.AlmacenOSel, $scope.Master.RegMaster).then(function (response) {
                            if (response.codigo == 0) {
                                $scope.Alertas('s', response.mensaje);
                                $scope.Lectura.Procesando = false;
                                $scope.Limpiar();
                            }
                            else {
                                $scope.Alertas('e', response.mensaje);
                                $scope.Lectura.Procesando = false;
                            }
                        });
                    }
                    else {
                        //NO
                        //Se Notifica que debe de elegir Traspaso y/o Bloqueo
                        $scope.Alertas('w', 'Debe seleccionar Traspaso y/o Bloqueo');
                        $scope.Lectura.Procesando = false;
                    }
                }
            }			
        }
        else {
            $scope.Alertas('e','No ha seleccionado un estatus');
        }
    }
    
    $scope.Limpiar = function () {
        $scope.Lectura = {
            Etiqueta: '',
            Procesando: false
        }

        $scope.Calidad = {
            Estatus: '',
            Acciones: [{ Code: 'X', Name: 'Bloquear' }, { Code: 'O', Name: 'Desbloquear' }],
            BinCode: '',
            AbsEntry: 0,

            AlmacenDSel: '',
            ToWhsCodes: [],

            AlmacenOSel: '',
            FromWhsCodes: [],

            Lineas: [],

            Mover: 0, //Indica si se realizará Trasferencia de Stock o solo se Cambia el Estatus de los Lotes, 0: No Mover | 1: Mover
            Traspaso: 'Y', //Traspaso | Sin Traspaso
            MoverLote: [{ Code: 'N', Name: 'No' }, { Code: 'Y', Name: 'Si' }],

            Bloqueo: 0, //Indica si se realizará Bloqueo o solo se hace la Trasferencia de Stock, 0: No Bloquear | 1: Bloquear
            Bloquear: 'Y', //Bloquear | NO bloquear
            TrBloqueo: [{ Code: 'N', Name: 'No' }, { Code: 'Y', Name: 'Si' }]
        }

        $scope.Memoria = {
            ListaMP: [],  //Lista de etiquetas MP, para verificar que no se haya leido la etiqueta con en el mismo numero
            TipoWhs: 'User',
            Nlectura: 1,
            WhsCode: '',
            WhsOrigen: [],
            WhsDestino: [],
            WhsCuarentena: []
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

        //Carga de Almacenes | obtener los almacenes predefinidos para el usuario
        $scope.Almacenes();
        //Verificar manejo de Master
        $scope.ValidaMaster();
    }
    
    //**********************************************************************************************************************
    //Modales
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
                $scope.AgregarLinea([result]);
            }
            $scope.Lectura.Etiqueta = '';
        });
    }
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

    //**********************************************************************************************************************
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

    //**********************************************************************************************************************
    //Acciones iniciales
    //Carga de Almacenes | obtener los almacenes predefinidos para el usuario
    $scope.Almacenes();
    //Verificar manejo de Master
    $scope.ValidaMaster();

});