var appTrsManual = angular.module('appTrsManual', ['angular-growl', 'ui.bootstrap', 'xeditable']);

appTrsManual.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

appTrsManual.factory('Ruta', function () {
    return { path: '' }
});

appTrsManual.service('Servicio', function ($http, Ruta, $log)
{
    this.ObtenerInformacion = function () {
        return $http({
            url: Ruta.path + '/TraspasoManual/ClientesConAlmacen',
            method: 'GET',
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

    this.ObtenerAlmacenes = function (TipoWhs, WhsCode) {
        return $http({
            url: Ruta.path + '/TraspasoManual/ObtenerAlmacenes',
            method: 'GET',
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            },
            params: {
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

    this.ListarUbicaciones = function (Whs) {
        return $http({
            url: Ruta.path + '/TraspasoManual/ListaUbicaciones',
            method: 'GET',
            params: {
                WhsCode: Whs
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 8 ": response } }
            return error;
        });
    },
        
    this.GrabarLectura = function (etq, nlectura) {
        return $http({
            url: Ruta.path + '/TraspasoManual/GrabarLectura',
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

    this.VEtqLiberada = function (Etq) {
        return $http({
            url: Ruta.path + '/TraspasoManual/VerificarEtq',
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
    
    this.Crear = function (Traspaso, Guia, RegMaster) {
        return $http({
            url: Ruta.path + '/TraspasoManual/Crear',
            method: 'POST',
            data: {
                Documento: Traspaso,
                CodigoGuia: Guia,
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

appTrsManual.controller('appTrsManualController', function ($scope, $window, Ruta, $log, Servicio, growl, $uibModal) {
    $scope.Lectura = {
        Etiqueta: '',
        UbicacionDef: '',
        Movimiento: false
    }

    $scope.Traspaso = {
        Clientes: [],
        ClienteSel: '',

        AlmacenesO: [],
        AlmacenOSel: '',

        Almacenes: [],
        AlmacenSel: '',

        Lineas: [],

        AbsEntry: 0,
        BinCode: '',
    }

    $scope.DtsTemp = {
        AbsEntry: 0,
        BinCode: ''
    }

    $scope.Memoria = {
        CodeGuia: '', //Variable para el código de seguimiento y colocado en el Documento
        Nlectura: 1,

        ListaMP: [],  //Lista de etiquetas MP, para verificar que no se haya leido la etiqueta con en el mismo numero

        TipoWhs: 'User', //User: Predeterminados para el usuario | Todos
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

    //********************************************************************************************/
    //Carga de Clientes
    $scope.CargarClientes = function () {
        Servicio.ObtenerInformacion().then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Traspaso.Clientes = response.mensaje;
                    break;

                case -1:
                    $scope.Alertas('w', "No se obtuvieron Clientes, verifique con su administrador");
                    break

                case -2:
                    $scope.Alertas('e', "Error al consultar información de Clientes, contacte al administrador de sistema");
                    break;
            }
        });
    }

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

                    $scope.AsignarAlms("I");
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

    //Generación de código guia
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

    //********************************************************************************************/
    //Cambio de almacén
    $scope.AsignarAlms = function (Accion) {
        switch (Accion) {
            case "I": //Carga inicial
                console.log("Almacen carga inicial");
                $scope.BorrarUbicacion();
                $scope.Traspaso.Lineas = [];
                $scope.Memoria.ListaMP = [];

                $scope.Traspaso.AlmacenesO = []; //Lista almacenes Origen
                $scope.Traspaso.AlmacenOSel = ""; //Origen seleccionado
                $scope.Traspaso.Almacenes = []; //Lista almacenes Destino
                $scope.Traspaso.AlmacenSel = ""; //Destino seleccionado

                //Almacén origen: Predeterminado
                if ($scope.Memoria.WhsOrigen.length == 0) {
                    $scope.Alertas('w', "El usuario no tiene configurado Almacén Origen, verifique con su administrador");
                }
                else {
                    $scope.Traspaso.AlmacenesO = $scope.Memoria.WhsOrigen;
                    //$scope.Traspaso.AlmacenOSel = $scope.Traspaso.AlmacenesO[0].WhsCode;
                }

                //Almacen Destino: Predeterminado
                if ($scope.Memoria.WhsDestino.length == 0) {
                    $scope.Alertas('w', "El usuario no tiene configurado Almacén Destino, verifique con su administrador");
                }
                else {
                    $scope.Traspaso.Almacenes = $scope.Memoria.WhsDestino;
                    //$scope.Traspaso.AlmacenSel = $scope.Traspaso.Almacenes[0].WhsCode;
                }

                //$scope.ListarUbicaciones("I"); //Ubicación inicial del almacen origen
                break;

            case "S": //Cambio de almacen Origen
                console.log("NUEVO Almacén seleccionado: " + $scope.Traspaso.AlmacenOSel);
                $scope.Traspaso.Lineas = [];
                $scope.Memoria.ListaMP = [];
                break;

            case "D": //Cambio de almacen Destino
                console.log("NUEVO Almacén Destino seleccionado: " + $scope.Traspaso.AlmacenSel);
                $scope.BorrarUbicacion();
                $scope.Traspaso.Lineas = [];
                $scope.Memoria.ListaMP = [];
                $scope.ListarUbicaciones("I"); //Ubicación inicial del almacen origen
                break;
        }
    }

    //Listar ubicaciones de almacén seleccionado
    $scope.ListarUbicaciones = function (Accion) {
        Servicio.ListarUbicaciones($scope.Traspaso.AlmacenSel).then(function (response) {
            switch (response.codigo) {
                case 0:
                    switch (Accion) {
                        case "I":
                            $scope.BorrarUbicacion();
                            $scope.Traspaso.BinCode = response.mensaje[0].BinCode;
                            $scope.DtsTemp.BinCode = response.mensaje[0].BinCode;
                            $scope.DtsTemp.AbsEntry = response.mensaje[0].AbsEntry;
                            break;

                        case "S":
                            $scope.AbrirModalUbicación(response.mensaje, -1);
                            break;
                    }
                    break;

                case -1:
                    switch (Accion) {
                        case "S":
                            $scope.Alertas('w', "El Almacén " + $scope.Traspaso.AlmacenSel + " no tiene ubicaciones");
                            break;
                    }                    
                    break;

                case -2:
                    switch (Accion) {
                        case "S":
                            $scope.Alertas('e', "Error al consultar información de Ubicaciones de almacén " + $scope.Traspaso.AlmacenSel + ", contacte al administrador de sistema");
                            break;
                    }                    
                    break;
            }
        });
    }

    //Borrar ubicación
    $scope.BorrarUbicacion = function () {
        $scope.Traspaso.AbsEntry = 0;
        $scope.Traspaso.BinCode = '';
        $scope.DtsTemp.BinCode = '';
        $scope.DtsTemp.AbsEntry = 0;
    }

    //Asignar ubicacion seleccionada
    $scope.AsignarDtsTemp = function (response) {
        $scope.BorrarUbicacion();

        $scope.Traspaso.BinCode = response.BinCode;
        $scope.DtsTemp.BinCode = response.BinCode;
        $scope.DtsTemp.AbsEntry = response.AbsEntry;

        $scope.Alertas('i', "Ubicación definida");
    }

    //********************************************************************************************/
    //Lectura de Etiqueta
    $scope.LecturaQR = function () {
        //Grabar informacion
        Servicio.GrabarLectura($scope.Lectura.Etiqueta, $scope.Memoria.Nlectura).then(function (response) //Revisa el estatus de la etiqueta 
        {
            console.log(response.mensaje.length + ' --- # ' + $scope.Memoria.Nlectura);
            $scope.Memoria.Nlectura++;
        });

        if ($scope.Traspaso.AlmacenSel !== '' || $scope.Traspaso.AlmacenOSel !== '') {
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
            $scope.Alertas('w', 'Seleccione un almacén destino y origen');
            $scope.Lectura.Etiqueta = '';
        }
    }

    //***************************************************************************************/
    $scope.ProcesaEtiquetaUnica = function () {
        //Validar cadena: 7 segmentos OK, menos FAIL, más de 8 FAIL
        if ($scope.Lectura.Etiqueta.split('&').length < 7 || $scope.Lectura.Etiqueta.split('&').length > 8) {
            $scope.Alertas('w', 'Lectura Erronea, vuelva a escaner etiqueta');
            $scope.Lectura.Etiqueta = '';
            $("#Entradas").focus();
            return;
        }

        Servicio.VEtqLiberada($scope.Lectura.Etiqueta).then(function (_response) {
            switch (_response.codigo) {
                case -2:
                    $scope.Alertas('e', _response.mensaje);
                    $scope.Lectura.Etiqueta = '';
                    break;

                case -1:
                    $scope.Alertas('w', _response.mensaje);
                    $scope.Lectura.Etiqueta = '';
                    break;

                case 0:
                    Servicio.ObtenerLote($scope.Lectura.Etiqueta).then(function (response) {
                        switch (response.codigo) {
                            case 0:
                                switch (response.mensaje[0].Estatus) {
                                    case "0":
                                        var ListaL = $scope.Remover(response.mensaje);
                                        switch (ListaL.length) {
                                            case 0:
                                                $scope.Alertas('e', 'NO SE ENCONTRÓ información de la etiqueta EN EL ALMACÉN ' + $scope.Traspaso.AlmacenOSel);
                                                $scope.Lectura.Etiqueta = '';
                                                $("#Entradas").focus();
                                                break

                                            case 1:
                                                $scope.AgregarLinea(ListaL);
                                                break;

                                            default:
                                                $scope.AbrirModalLotes(ListaL);
                                                break;
                                        }
                                        break;

                                    case "1":
                                        $scope.Alertas('w', "El Lote " + response.mensaje[0].DistNumber + " se encuentra No Accesible");
                                        $scope.Lectura.Etiqueta = '';
                                        $("#Entradas").focus();
                                        break;

                                    case "2":
                                        $scope.Alertas('w', "El Lote " + response.mensaje[0].DistNumber + " se encuentra Bloqueado");
                                        $scope.Lectura.Etiqueta = '';
                                        $("#Entradas").focus();
                                        break;
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
            }
        });
    }
    //***************************************************************************************/
    $scope.ProcesaEtiquetaMaster = function () {

        if (!$scope.CondicionLinea("M", $scope.Lectura.Etiqueta.split('&')[0], 0)) {
            Servicio.ObtenerMaster($scope.Lectura.Etiqueta, $scope.Traspaso.AlmacenOSel).then(function (response) {
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
                        4.1.- Validar estatus: 0 > Liberado | 1 > No accesible | 2 > Bloquedo | Solo permite Liberados
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
                                            if (response.mensaje[i].LoteEstatus === "0") {
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
                                                $scope.Master.ErrorLotesMaster.push({
                                                    "DistNumber": response.mensaje[i].Lote,
                                                    "Error": "No se encuentra Liberado"
                                                });
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
                                                        , "NewBinCode": $scope.DtsTemp.BinCode
                                                        , "NewAbsEntry": $scope.DtsTemp.AbsEntry
                                                        , "Estatus": $scope.Master.LotesMaster[r].LoteEstatus
                                                        , "Cancelado": "N"
                                                        , "Tipo": $scope.Master.LotesMaster[r].Tipo
                                                        , "EtqMaster": response.mensaje[0].Codigo
                                                        , "QtySAP": $scope.Master.LotesMaster[r].OnHandQty

                                                        , "Dispositivo": $scope.Master.LotesMaster[r].Dispositivo
                                                        , "PesoBruto": "0"
                                                        , "PesoNeto": "0"
                                                        , "NumEtq": "1"
                                                        , "QtyEtq": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "QtyTraspaso": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
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
                                                        , "NewBinCode": $scope.DtsTemp.BinCode
                                                        , "NewAbsEntry": $scope.DtsTemp.AbsEntry
                                                        , "Estatus": $scope.Master.LotesMaster[r].LoteEstatus
                                                        , "Cancelado": "N"
                                                        , "Tipo": $scope.Master.LotesMaster[r].Tipo
                                                        , "EtqMaster": response.mensaje[0].Codigo
                                                        , "QtySAP": $scope.Master.LotesMaster[r].OnHandQty

                                                        , "Dispositivo": $scope.Master.LotesMaster[r].Dispositivo
                                                        , "PesoBruto": "0"
                                                        , "PesoNeto": "0"
                                                        , "NumEtq": "1"
                                                        , "QtyEtq": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "QtyTraspaso": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                        , "Etqs": 1
                                                        , "TipoLectura": "M"
                                                    });
                                                }
                                                console.log($scope.Master.RegMaster);
                                                console.log($scope.Master.MasterRegUnica);

                                                $scope.Traspaso.Lineas.push({
                                                    "DistNumber": response.mensaje[0].Codigo
                                                    , "ItemCode": $scope.Master.MasterRegUnica[0].ItemCode
                                                    , "WhsCode": $scope.Master.MasterRegUnica[0].WhsCode
                                                    , "BinCode": $scope.Master.MasterRegUnica[0].BinCode
                                                    , "AbsEntry": $scope.Master.MasterRegUnica[0].AbsEntry
                                                    , "NewBinCode": $scope.DtsTemp.BinCode
                                                    , "NewAbsEntry": $scope.DtsTemp.AbsEntry
                                                    , "OnHandQty": response.mensaje[0].MasterOnHandQty
                                                    , "Etqs": $scope.Master.MasterRegUnica.length
                                                    , "TipoLectura": "M"
                                                });

                                                console.log($scope.Traspaso.Lineas);
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

    //***************************************************************************************/

    //Validar informacion de Lote SAP, Etiqueta y Almacen Origen
    $scope.Remover = function (list) {
        var nArray = [];
        for (var r = 0; r < list.length; r++) {
            if ($scope.Traspaso.AlmacenOSel === list[r].WhsCode && $scope.Lectura.Etiqueta.split('&')[1] === list[r].ItemCode) {
                nArray.push(list[r]);
            }
        }
        return nArray;
    }

    $scope.AgregarLinea = function (ListaL) {

        console.log('Tipo: ' + ListaL[0].Tipo);

        //Verificar si etiqueta ha sido leida anteriormente en caso de ser MP | Comparar con la lista de etiquetas MP
        var idx = -3;
        console.log($scope.Memoria.ListaMP);
        console.log($scope.Memoria.ListaMP.length);

        if ($scope.Memoria.ListaMP.length > 0 && ListaL[0].Tipo === "MP") {
            console.log('Verificar MP');
            idx = $scope.GruposLineas($scope.Lectura.Etiqueta, ListaL, 'M');
        }
        else {
            console.log('Verificar PT');
            if (!$scope.CondicionLinea("LT", "", 0)) {
                idx = -3;
            }
            else {
                idx = -2;
            }
        }

        //Si la etiqieta no ha sido leida, se procede
        if (idx === -3) {
            var index = $scope.GruposLineas($scope.Lectura.Etiqueta, ListaL, 'O');
            if (index !== -2) {
                var indice = $scope.GruposLineas($scope.Lectura.Etiqueta, ListaL, 'D');
                if (indice !== -1) {
                    var qty = 0;
                    if ($scope.Lectura.Etiqueta.split('&')[2].indexOf(",") === 1)
                        qty = $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                    else
                        qty = $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");

                    if (!$scope.CondicionLinea("CS", "", indice)) {
                        $scope.Traspaso.Lineas[indice].OnHandQty = parseFloat($scope.Traspaso.Lineas[indice].OnHandQty) + parseFloat(qty);
                        $scope.Traspaso.Lineas[indice].QtyTraspaso = $scope.Traspaso.Lineas[indice].OnHandQty;
                        $scope.Traspaso.Lineas[indice].Etqs++;

                        if (ListaL[0].Tipo == 'MP') {
                            Object.assign(ListaL[0], {
                                "NumEtq": $scope.Lectura.Etiqueta.split('&')[4]
                                , "QtyEtq": $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                            })

                            ListaL[0].NewAbsEntry = $scope.DtsTemp.AbsEntry;
                            ListaL[0].NewBinCode = $scope.DtsTemp.BinCode;

                            $scope.Memoria.ListaMP.push(ListaL[0]); //Se agrega información de etiqueta a lista de etiquetas MP
                            console.log($scope.Memoria.ListaMP);
                        }

                        console.log($scope.Traspaso.Lineas);
                        $scope.Lectura.Etiqueta = '';
                    }
                }
                else {
                    //nueva linea
                    Object.assign(ListaL[0], {
                        "Etqs": 1
                        , "QtySAP": ListaL[0].OnHandQty
                        , "Dispositivo": $scope.Lectura.Etiqueta.split('&')[4]
                        , "PesoBruto": $scope.Lectura.Etiqueta.split('&')[5].replace(',', '')
                        , "PesoNeto": $scope.Lectura.Etiqueta.split('&')[6].replace(',', '')
                        , "NumEtq": 1
                        , "QtyEtq": $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                        , "QtyTraspaso": 0.0
                        , "TipoLectura": "I"
                    });

                    if (ListaL[0].Tipo == 'MP') {
                        ListaL[0].NumEtq = $scope.Lectura.Etiqueta.split('&')[4];
                    }

                    if (ListaL[0].OnHandQty == parseFloat($scope.Lectura.Etiqueta.split('&')[2].replace(",", ""))) {
                        console.log("Cantidad igual");
                        ListaL[0].OnHandQty = $scope.Lectura.Etiqueta.split('&')[2].indexOf(",") === 1 ? $scope.Lectura.Etiqueta.split('&')[2].replace(",", "") : $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");
                    }
                    else if (ListaL[0].OnHandQty > parseFloat($scope.Lectura.Etiqueta.split('&')[2].replace(",", ""))) {
                        ListaL[0].OnHandQty = $scope.Lectura.Etiqueta.split('&')[2].indexOf(",") === 1 ? $scope.Lectura.Etiqueta.split('&')[2].replace(",", "") : $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");                        
                    }
                    else {
                        $scope.Alertas("w",
                            "La etiqueta con lote: " + $scope.Lectura.Etiqueta.split("&")[0] +
                            " y artículo: " + $scope.Lectura.Etiqueta.split("&")[1] +
                            " NO PUEDE superar el Stock de: " + ListaL[0].OnHandQty); 
                    }

                    ListaL[0].QtyTraspaso = ListaL[0].OnHandQty;
                    ListaL[0].NewAbsEntry = $scope.DtsTemp.AbsEntry;
                    ListaL[0].NewBinCode = $scope.DtsTemp.BinCode;

                    $scope.Traspaso.Lineas.push(ListaL[0]);

                    if (ListaL[0].Tipo == 'MP') {
                        $scope.Memoria.ListaMP.push(ListaL[0]); //Se agrega información de etiqueta a lista de etiquetas MP
                        console.log($scope.Memoria.ListaMP);
                    }

                    console.log($scope.Traspaso.Lineas);
                    $scope.Lectura.Etiqueta = '';
                }
            }
        }
        else {
            $scope.Alertas('e', 'La etiqueta ya ha sido Leida');
            $scope.Lectura.Etiqueta = '';
            return;
        }

    }

    $scope.GruposLineas = function (qr, response, tipo) {
        let index = -1;
        switch (tipo) {
            case 'D'://Articulo, Lote, Origen, Destino
                let Ls = $scope.Traspaso.Lineas;
                index = Ls.findIndex(linea => linea.DistNumber === response[0].DistNumber &&
                    linea.ItemCode === response[0].ItemCode &&
                    linea.AbsEntry === response[0].AbsEntry &&
                    linea.NewAbsEntry === $scope.DtsTemp.AbsEntry);
                break;

            case 'O'://Articulo, Lote, Origen
                let Lineas = $scope.Traspaso.Lineas;
                let sum = 0;

                for (let m = 0; m < Lineas.length; m++) {
                    if (Lineas[m].DistNumber === qr.split('&')[0] &&
                        Lineas[m].ItemCode === qr.split('&')[1] &&
                        Lineas[m].AbsEntry === response[0].AbsEntry) {

                        sum += parseFloat(Lineas[m].OnHandQty);
                    }
                }

                let j = Lineas.findIndex(linea => linea.DistNumber === qr.split('&')[0] && linea.ItemCode === qr.split('&')[1] && linea.AbsEntry === response[0].AbsEntry)
                let acum = parseFloat(sum) + parseFloat(qr.split('&')[2].replace(",", ""));

                if (j >= 0) {
                    if (trunc(acum) > Lineas[j].QtySAP) {
                        //$scope.Alertas('e', 'Esta excediendo el stock disponible para el artículo ' + Lineas[j].ItemCode);
                        $scope.Alertas("w",
                            "La etiqueta con lote: " + $scope.Lectura.Etiqueta.split("&")[0] +
                            " y artículo: " + $scope.Lectura.Etiqueta.split("&")[1] +
                            " NO PUEDE superar la cantidad de: " + $scope.Traspaso.Lineas[j].QtySAP);

                        $scope.Lectura.Etiqueta = '';
                        index = -2;
                        break;
                    }
                }
                break;

            //Verificar: Existe => Se indica el indice del registro encntrado en Lista de Lotes a Transferir
            case 'M'://Articulo, Lote, Origen, Destino, [NumEtq(unico)]
                console.log($scope.Memoria.ListaMP);
                console.log(qr.split('&')[4]);
                index = -3;

                for (var t = 0; t < $scope.Memoria.ListaMP.length; t++) {

                    console.log($scope.Memoria.ListaMP[t].DistNumber + ' --- ' + response[0].DistNumber + ' --- ' + $scope.Memoria.ListaMP[t].ItemCode + ' --- ' + response[0].ItemCode + ' --- ' + $scope.Memoria.ListaMP[t].NumEtq + ' --- ' + qr.split('&')[4])

                    if ($scope.Memoria.ListaMP[t].DistNumber === response[0].DistNumber && $scope.Memoria.ListaMP[t].ItemCode === response[0].ItemCode) {

                        if ($scope.Memoria.ListaMP[t].NumEtq == qr.split('&')[4]) {
                            index = t;
                            console.log('index 1: ' + index);

                            break;
                        }

                    }
                }

                console.log('index 2: ' + index);

                break;

        }

        return index;
    }

    $scope.CondicionLinea = function (_tipoVal, value, i) {
        var status = false;
        switch (_tipoVal) {
            case "CS"://No superar cantidad de SAP para MP
                //verificamos cantidad a superar
                var acum = parseFloat($scope.Traspaso.Lineas[i].OnHandQty) + parseFloat($scope.Lectura.Etiqueta.split("&")[2].replace(",", ""));
                if (trunc(acum) > $scope.Traspaso.Lineas[i].QtySAP) {
                    status = true;
                    $scope.Alertas("e", "La etiqueta con lote: " + $scope.Lectura.Etiqueta.split("&")[0] + " y artículo: " + $scope.Lectura.Etiqueta.split("&")[1] + " NO PUEDE superar la cantidad de: " + $scope.Traspaso.Lineas[i].QtySAP);
                    $scope.Lectura.Etiqueta = '';
                    break;
                }
                break;

            case "LT"://Lote de PT unico en la tabla
                for (var t = 0; t < $scope.Traspaso.Lineas.length; t++) {
                    if ($scope.Traspaso.Lineas[t].DistNumber === $scope.Lectura.Etiqueta.split("&")[0]) {
                        status = true;
                        //$scope.Alertas("e", "La etiqueta ya existe");
                        $scope.Lectura.Etiqueta = '';
                        break;
                    }
                }

                //Validar si existe Master leida | validar lote individual en master leida
                if ($scope.Master.RegMaster.length > 0) {
                    for (var m = 0; m < $scope.Master.RegMaster.length; m++) {
                        if ($scope.Master.RegMaster[m].DistNumber === $scope.Lectura.Etiqueta.split("&")[0]) {
                            status = true;
                            $scope.Alertas("w", "La etiqueta ya existe en Master Leida " + $scope.Master.RegMaster[m].EtqMaster);
                            $scope.Lectura.Etiqueta = '';
                            break;
                        }
                    }
                }
                break;

            case "ML"://Lote de PT unico en la tabla
                for (var t = 0; t < $scope.Traspaso.Lineas.length; t++) {
                    if ($scope.Traspaso.Lineas[t].DistNumber === value) {
                        status = true;
                        $scope.Lectura.Etiqueta = '';
                        break;
                    }
                }
                break;

            case "M"://Master unica en la tabla
                for (var t = 0; t < $scope.Traspaso.Lineas.length; t++) {
                    if ($scope.Traspaso.Lineas[t].DistNumber === value) {
                        status = true;
                        $scope.Lectura.Etiqueta = '';
                        break;
                    }
                }
                break;
        }
        return status;
    }

    //********************************************************************************************/
    //Acciones por registro
    $scope.ConfirmarUbicacion = function (idx) {
        //Listar ubicaciones de Almacen Destino
        Servicio.ListarUbicaciones($scope.Traspaso.AlmacenSel).then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.AbrirModalUbicación(response.mensaje, idx);
                    break;

                case -1:
                    $scope.Alertas('w', "El Almacén " + $scope.Traspaso.AlmacenSel + " no tiene ubicaciones");
                    break;

                case -2:
                    $scope.Alertas('e', "Error al consultar información de Ubicaciones de almacén " + $scope.Traspaso.AlmacenSel + ", contacte al administrador de sistema");
                    break;
            }
        });
    }

    $scope.ConfirmarCantidad = function (dt, indx) {
        console.log('ACTUAL > Indice: ' + indx +
            ' ---- Cantidad en SAP: ' + $scope.Traspaso.Lineas[indx].QtySAP +
            ' ---- Cantidad Inicial: ' + $scope.Traspaso.Lineas[indx].QtyTraspaso +
            ' ---- Cantidad Etiqueta: ' + $scope.Traspaso.Lineas[indx].QtyEtq +
            ' ---- Cantidad NUEVA: ' + parseFloat(dt) +
            ' ---- Linea ST: ' + $scope.Traspaso.Lineas[indx].LineNum);

        let QtyFinal = 0;
        let QtyRestante = 0;
        let LineST = $scope.Traspaso.Lineas[indx].LineNum;

        let sum = 0;
        let acum;
        let valorQtyFinal;
        let valorQtyTraspaso;
        let valorSumTraspaso;
        let valorActualizar;
        let valorQtySAP;
        let valorQtyLeido;

        console.log($scope.Traspaso.Lineas);
        let Lineas = $scope.Traspaso.Lineas;

        //Acumulado total actual + cantidad de etiqueta
        valorQtyTraspaso = new Decimal($scope.Traspaso.Lineas[indx].QtyTraspaso);
        valorActualizar = new Decimal(dt);
        console.log('Acumulado previo | Cantidad Traspaso: ' + $scope.Traspaso.Lineas[indx].QtyTraspaso + ' --- Cantidad: ' + parseFloat(dt));

        valorQtySAP = new Decimal($scope.Traspaso.Lineas[indx].QtySAP);
        let QtySAP = valorQtySAP.toNumber();
        let NewQty = valorActualizar.toNumber();

        if (NewQty === QtySAP) {
            QtyFinal = parseFloat(dt);
        }
        else if (NewQty > QtySAP) {
            $scope.Alertas("w", "La etiqueta con lote individual: " + $scope.Traspaso.Lineas[indx].DistNumber +
                " y artículo: " + $scope.Traspaso.Lineas[indx].ItemCode +
                " NO PUEDE superar el Stock de: " + QtySAP);

            QtyFinal = QtySAP;
        }
        else {
            //No: Cantidad a embarcar será la cantidad ingresada
            QtyFinal = parseFloat(dt);
        }

        //Se actualiza informacion de cantidad a embarcar del Lote 
        $scope.Traspaso.Lineas[indx].OnHandQty = parseFloat(QtyFinal);

        //Se actualiza la cantidad a traspasar [Cantidad inicial para antes de actualizar cantidad]
        $scope.Traspaso.Lineas[indx].QtyTraspaso = parseFloat(QtyFinal);

        console.log('ACTUALIZACION > Lote: ' + $scope.Traspaso.Lineas[indx].DistNumber +
            ' --- Cantidad a Traspasar Lote: ' + $scope.Traspaso.Lineas[indx].OnHandQty +
            ' --- Cantidad a Traspasar QtyTraspaso: ' + $scope.Traspaso.Lineas[indx].QtyTraspaso +
            ' --- Cantidad a Traspasar QtyEtiqueta: ' + $scope.Traspaso.Lineas[indx].QtyEtq +
            ' --- Cantidad en SAP: ' + $scope.Traspaso.Lineas[indx].QtySAP);
    }

    $scope.Eliminar = function (i) {
        //Eliminar registros
        // i => reg a eliminar en lineas leídas
        let TipoLectura = $scope.Traspaso.Lineas[i].TipoLectura; // I: Individual | M: Master

        if (TipoLectura === "I") {
            console.log('Eliminar Lote: ' + $scope.Traspaso.Lineas[i].DistNumber);

            //Por cada uno de los valores buscar y eliminar lotes MP
            for (var h = $scope.Memoria.ListaMP.length - 1; h >= 0; h--) {
                if ($scope.Memoria.ListaMP[h].DistNumber === $scope.Traspaso.Lineas[i].DistNumber) {
                    console.log('Lote: ' + $scope.Traspaso.Lineas[i].DistNumber + ' -- Posicion: ' + h);
                    console.log("El lote " + $scope.Memoria.ListaMP[h].DistNumber + " -- con Num. " + $scope.Memoria.ListaMP[h].NumEtq + ", ELIMINADO");

                    //Eliminar lote MP
                    $scope.Memoria.ListaMP.splice(h, 1);
                }
            }

            $scope.Traspaso.Lineas.splice(i, 1);

            if ($scope.Traspaso.Lineas.length === 0) {
                $scope.Memoria.Nlectura = 1;
            }
        }
        else {
            //Eliminar registros individuales de Master
            let MasterCodigo = $scope.Traspaso.Lineas[i].DistNumber;

            //Por cada uno de los valores buscar y eliminar lotes de Master
            for (var h = $scope.Master.RegMaster.length - 1; h >= 0; h--) {
                if ($scope.Master.RegMaster[h].EtqMaster === MasterCodigo) {
                    console.log("Lote " + $scope.Master.RegMaster[h].DistNumber + ", ELIMINADO");

                    //Eliminar lote 
                    $scope.Master.RegMaster.splice(h, 1);
                }
            }

            $scope.Traspaso.Lineas.splice(i, 1);

            if ($scope.Traspaso.Lineas.length === 0) {
                $scope.Memoria.Nlectura = 1;
            }

            console.log("Lotes de Master restantes >> " + JSON.stringify($scope.Master.RegMaster));
            console.log("Registros leídos restantes >> " + JSON.stringify($scope.Traspaso.Lineas));            
        }
    }

    //********************************************************************************************/
    //Generar Movimiento
    $scope.Documento = function () {
        console.log('Codigo guia enviado: ' + $scope.Memoria.CodeGuia);
        console.log('Lineas leidas: >>>>>>');
        console.log($scope.Traspaso.Lineas); //Lineas actuales a procesar
        console.log('Registros master');
        console.log($scope.Master.RegMaster); //Lineas master a procesar

        $scope.Lectura.Movimiento = true;
        //Validación de comunicación con servidor aprox entre 3 y 20 seg respuesta
        //cambiar por URL de instalación http://10.0.0.1/wmsKB2
        fetch('http://10.0.0.1')
        .then(response => {
            if (response.ok) {
                console.log('VALIDACION SERVER ::: Comunicación exitosa');

                Servicio.Crear($scope.Traspaso, $scope.Memoria.CodeGuia, $scope.Master.RegMaster).then(function (response) {
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
                $scope.Lectura.Movimiento = false;
                $scope.Alertas('e', "Error de comunicación con servidor, intente nuevamente en 10seg, de lo contrario contacte a su administrador de red");
                console.log('VALIDACION SERVER ::: Error en la comunicación');
            }
        })
        .catch(error => {
            $scope.Lectura.Movimiento = false;
            $scope.Alertas('e', "Error al conectar con servidor, " + error + ", verifique conexión y salida a internet, intente nuevamente, de lo contrario contacte a su administrador de red");
            console.log('VALIDACION SERVER ::: No se pudo conectar al servidor:', error);
        });
    }

    //Recargar
    $scope.Limpiar = function () {
        $scope.Lectura = {
            Etiqueta: '',
            UbicacionDef: '',
            Movimiento: false
        }

        $scope.Traspaso = {
            Clientes: [],
            ClienteSel: '',

            AlmacenesO: [],
            AlmacenOSel: '',

            Almacenes: [],
            AlmacenSel: '',

            Lineas: [],

            AbsEntry: 0,
            BinCode: '',
        }

        $scope.DtsTemp = {
            AbsEntry: 0,
            BinCode: ''
        }

        $scope.Memoria = {
            CodeGuia: '', //Variable para el código de seguimiento y colocado en el Documento
            Nlectura: 1,

            ListaMP: [],  //Lista de etiquetas MP, para verificar que no se haya leido la etiqueta con en el mismo numero

            TipoWhs: 'User', //User: Predeterminados para el usuario | Todos
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

        //Carga de Clientes
        $scope.CargarClientes();
        //Generación de código guia
        $scope.CodigoGuia();
        //Carga de Almacenes | obtener los almacenes predefinidos para el usuario
        $scope.Almacenes();
        //Verificar manejo de Master
        $scope.ValidaMaster();
    }

    //********************************************************************************************/
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
            if (result !== null && result !== '' && result !== undefined) {
                $scope.AgregarLinea([result]);
            }
            $scope.Lectura.Etiqueta = '';
        });
    }
    var ListLotesController = function (LotesList, $scope, $uibModalInstance, $log) {
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion !== null ? $scope.Seleccion : "");
        }
        $scope.LotesList = LotesList;
        $scope.LoteSeleccionado = function (val) {
            $scope.Seleccion = val;
            $scope.ok();
        }
    }

    $scope.AbrirModalUbicación = function (ListUbicacion, reg) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaUbicacion.html',
            controller: ListUbicacionesController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                UbicacionesList: function () {
                    return ListUbicacion;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result !== null && result !== '' && result !== undefined) {
                if (reg === -1) {
                    $scope.AsignarDtsTemp(result);
                }
                else {
                    //Actualizar ubicación en registro de tabla | registro leído
                    //reg => id del registro en Lineas
                    console.log('BinCode NUEVO: ' + result.BinCode + ' - ' + result.AbsEntry);
                    let TipoLectura = $scope.Traspaso.Lineas[reg].TipoLectura; // I: Individual | M: Master

                    if (TipoLectura === "I") {
                        $scope.Traspaso.Lineas[reg].NewBinCode = result.BinCode;
                        $scope.Traspaso.Lineas[reg].NewAbsEntry = result.AbsEntry;

                        console.log("Lotes a Transferir al Momento >> " + JSON.stringify($scope.Traspaso.Lineas[reg]));
                    }
                    else {
                        let MasterCodigo = $scope.Traspaso.Lineas[reg].DistNumber;

                        //Actualización a nivel master
                        $scope.Traspaso.Lineas[reg].NewBinCode = result.BinCode;
                        $scope.Traspaso.Lineas[reg].NewAbsEntry = result.AbsEntry;

                        console.log("Master a Transferir al Momento >> " + JSON.stringify($scope.Traspaso.Lineas[reg]));

                        //Actualización a nivel registros de master
                        for (var i = 0; i < $scope.Master.RegMaster.length; i++) {
                            if ($scope.Master.RegMaster[i].EtqMaster === MasterCodigo) {
                                $scope.Master.RegMaster[i].NewBinCode = result.BinCode;
                                $scope.Master.RegMaster[i].NewAbsEntry = result.AbsEntry;

                                console.log("Lotes de Master a Transferir al Momento >> " + JSON.stringify($scope.Master.RegMaster[i]));
                            }
                        }
                    }
                }
            }
        });
    }
    var ListUbicacionesController = function (UbicacionesList, $scope, $uibModalInstance, $log) {
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion !== null ? $scope.Seleccion : "");
        }

        $scope.UbiList = UbicacionesList;

        $scope.UbiSeleccionado = function (val) {
            $scope.Seleccion = val;
            $scope.ok();
        }
    }

    //********************************************************************************************/
    //Alertas
    $scope.Alertas = function (Tipo, Texto) {
        switch (Tipo) {
            case 'w':
                growl.warning(Texto, { title: '¡Mensaje de Sistema!' });
                break;
            case 'i':
                growl.info(Texto, { title: 'Información' });
                break;
            case 's':
                growl.success(Texto, { title: ' - Completado - ' });
                break;
            case 'e':
                growl.error(Texto, { title: '¡¡Mensaje de Sistema!!' });
                break;
        }
    }

    //********************************************************************************************/
    //Acciones iniciales
    //Carga de Clientes
    $scope.CargarClientes();
    //Generación de código guia
    $scope.CodigoGuia();
    //Carga de Almacenes | obtener los almacenes predefinidos para el usuario
    $scope.Almacenes();
    //Verificar manejo de Master
    $scope.ValidaMaster();

});