var recuentoApp = angular.module('recuentoApp', ['angular-growl', 'ui.bootstrap', 'xeditable']);

recuentoApp.factory('Ruta', function () {
    return { path: '' }
});

recuentoApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

recuentoApp.service('Servicio', function ($http, Ruta, $log) {
    this.ObtenerAlm = function () {
        return $http({
            url: Ruta.path + '/Recuento/ObtenerAlms',
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

    this.ListarUbicaciones = function (Whs) {
        return $http({
            url: Ruta.path + '/Recuento/ListaUbicaciones',
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

    this.ObtenerLote = function (Almacen, Lote, Articulo) {
        return $http({
            url: Ruta.path + '/Recuento/LoteRecuento',
            method: 'POST',
            data: {
                WhsCode: Almacen,
                Batch: Lote,
                Item: Articulo
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

    this.CrearTxt = function (lineas, Whs, num) {
        return $http({
            url: Ruta.path + '/Recuento/CrearTxt',
            method: 'POST',
            data: {
                lineas: lineas,
                WhsCode: Whs,
                NumRec: num
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

recuentoApp.controller('reciboProdController', function ($scope, $window, Ruta, $log, Servicio, $uibModal, growl) {
    $scope.Recuento = {
        Num:1,
        Numeros: [{ Num: 1 }, { Num: 2 }, { Num: 3 }, { Num: 4}, { Num: 5 }],
        AlmacenSel: '',
        Almacenes: [],
        Lineas:[]
    }

    $scope.Lectura = {
        Etiqueta: ''
    }

    $scope.Memoria = {
        ListaMP: [], //Lista de etiquetas MP

        AbsEntry: 0, //Ubicación seleccionada del almacen
        BinCode: '' //Ubicación seleccionada del almacen
    }

    $scope.Master = {
        Existe: 'no',
        CodeML: '',
        LineasMaster: [], //Lista de etiquetas de la Master
        ErrorLineasMaster: [],
        LotesMaster: [], //Lista de Lotes de la Master en Almacén Origen seleccionado
        ErrorLotesMaster: [],
        RegMaster: []
    }

    //***************************************************************************************/
    //Carga de almacenes
    $scope.CargarDts = function () {
        Servicio.ObtenerAlm().then(function (response) {
            if (response.codigo == 0)
                $scope.Recuento.Almacenes = response.mensaje;
            else
                $scope.Alertas('e', response.mensaje);
        });
    }

    //Cambio de almacén
    $scope.AsignarAlms = function (Accion) {
        switch (Accion) {
            case "S": //Cambio de almacen Origen
                console.log("NUEVO Almacén seleccionado: " + $scope.Recuento.AlmacenSel);
                $scope.Recuento.Lineas = [];
                $scope.Memoria.ListaMP = [];
                $scope.Memoria.AbsEntry = 0;
                $scope.Memoria.BinCode = '';

                $scope.ListarUbicaciones("I"); //Ubicación inicial del almacen origen
                break;
        }
    }

    //Listar ubicaciones de almacén seleccionado
    $scope.ListarUbicaciones = function (Accion) {
        Servicio.ListarUbicaciones($scope.Recuento.AlmacenSel).then(function (response) {
            switch (response.codigo) {
                case 0:
                    switch (Accion) {
                        case "I":
                            console.log("Ubicaciónes de almacenes: " + $scope.Recuento.AlmacenSel);
                            $scope.Memoria.BinCode = response.mensaje[0].BinCode;
                            $scope.Memoria.AbsEntry = response.mensaje[0].AbsEntry;

                            console.log("Ubicación inicial: " + $scope.Memoria.BinCode + " | " + $scope.Memoria.AbsEntry);
                            break;

                        case "S":
                            $scope.AbrirModalUbicación(response.mensaje, -1);
                            break;
                    }
                    break;

                case -1:
                    switch (Accion) {
                        case "S":
                            $scope.Alertas('w', "El Almacén " + $scope.Recuento.AlmacenSel + " no tiene ubicaciones");
                            break;
                    }
                    break;

                case -2:
                    switch (Accion) {
                        case "S":
                            $scope.Alertas('e', "Error al consultar información de Ubicaciones de almacén " + $scope.Recuento.AlmacenSel + ", contacte al administrador de sistema");
                            break;
                    }
                    break;
            }
        });
    }

    //Asignar ubicacion seleccionada
    $scope.AsignarDtsTemp = function (response) {
        $scope.Memoria.AbsEntry = 0;
        $scope.Memoria.BinCode = '';

        $scope.Memoria.BinCode = response.BinCode;
        $scope.Memoria.AbsEntry = response.AbsEntry;

        $scope.Alertas('i', "Ubicación definida");
    }

    //***************************************************************************************/
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

    //***************************************************************************************/
    //Lectura de Etiqueta
    $scope.LecturaQR = function () {
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

    //***************************************************************************************/

    $scope.ProcesaEtiquetaUnica = function () {
        Servicio.ObtenerLote($scope.Recuento.AlmacenSel, $scope.Lectura.Etiqueta.split('&')[0], $scope.Lectura.Etiqueta.split('&')[1]).then(function (response) {
            switch (response.codigo) {
                case 0:

                    /*
                    Validar tipo etiqueta
                    PT.- Validar Artículo y Lote : no duplicados
                    2.- Agregar registro | PT no acumula cantidad

                    MP
                    1.- Validar Articulo, Lote y Num etiqueta: no duplicados
                    2.- Validar Articulo, Lote y Ubicación: existe > se acumula cantidad | no existe > se genera nueva línea
                    */

                    var QtyEtiqueta = 0;
                    if ($scope.Lectura.Etiqueta.split('&')[2].indexOf(",") === 1) {
                        QtyEtiqueta = $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");
                    }
                    else {
                        QtyEtiqueta = $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");
                    }                        

                    let QtyEti = new Decimal(QtyEtiqueta);

                    //Se agrega la ubicación
                    response.mensaje[0].WhsCode = $scope.Recuento.AlmacenSel;
                    response.mensaje[0].AbsEntry = $scope.Memoria.AbsEntry;
                    response.mensaje[0].BinCode = $scope.Memoria.BinCode;
                    response.mensaje[0].Ctd_Ctda = QtyEti.toNumber();

                    switch (response.mensaje[0].Tipo) {
                        case "PT":

                            if ($scope.Recuento.Lineas.length > 0 && $scope.ExisteEnTabla(response.mensaje[0].Tipo, response.mensaje[0].DistNumber, response.mensaje[0].ItemCode, 1)) {
                                $scope.Alertas('e', 'La etiqueta ya fue leída');
                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();
                                return;
                            }

                            $scope.Recuento.Lineas.push(response.mensaje[0]);
                            $scope.Lectura.Etiqueta = '';
                            $("#Entradas").focus();

                            console.log($scope.Recuento.Lineas);
                            break;

                        case "MP":
                            var NumEti = $scope.Lectura.Etiqueta.split('&')[4];
                            if ($scope.Memoria.ListaMP.length > 0 && $scope.ExisteEnTabla(response.mensaje[0].Tipo, response.mensaje[0].DistNumber, response.mensaje[0].ItemCode, NumEti)) {
                                $scope.Alertas('e', 'La etiqueta ya fue leída');
                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();
                                return;
                            }

                            //Validar si existe registro en ubicación | Lote, articulo y ubicación
                            let Ls = $scope.Recuento.Lineas;
                            let index = Ls.findIndex(linea => linea.DistNumber === response.mensaje[0].DistNumber &&
                                linea.ItemCode === response.mensaje[0].ItemCode &&
                                linea.AbsEntry === response.mensaje[0].AbsEntry);

                            if (index === -1) {
                                //Registro nuevo
                                console.log("MP registro nuevo");
                                $scope.Recuento.Lineas.push(response.mensaje[0]);
                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();

                                console.log($scope.Recuento.Lineas);

                                $scope.Memoria.ListaMP.push({
                                    "DistNumber": response.mensaje[0].DistNumber
                                    , "ItemCode": response.mensaje[0].ItemCode
                                    , "NumEti": NumEti
                                    , "AbsEntry": response.mensaje[0].AbsEntry
                                });

                                console.log($scope.Memoria.ListaMP);
                            }
                            else {
                                console.log("MP registro existente : se acumula");
                                let valorQtyActual = new Decimal($scope.Recuento.Lineas[index].Ctd_Ctda);
                                let valorQtyEti = new Decimal(response.mensaje[0].Ctd_Ctda);
                                $scope.Recuento.Lineas[index].Ctd_Ctda = valorQtyActual.plus(valorQtyEti).toNumber();

                                $scope.Memoria.ListaMP.push({
                                    "DistNumber": response.mensaje[0].DistNumber
                                    , "ItemCode": response.mensaje[0].ItemCode
                                    , "NumEti": NumEti
                                    , "AbsEntry": response.mensaje[0].AbsEntry
                                });

                                console.log($scope.Recuento.Lineas);
                                console.log($scope.Memoria.ListaMP);

                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();
                            }

                            break;
                    }
                    break;

                case -1: //No hay registro de etiqueta
                    $scope.Alertas('w', "No se encontro información de la etiqueta");
                    $scope.Lectura.Etiqueta = '';
                    $("#Entradas").focus();
                    break;

                case -2: //Error al obtener registro
                    $scope.Alertas('e', "Error al verificar etiqueta, volver a leer");
                    $scope.Lectura.Etiqueta = '';
                    $("#Entradas").focus();
                    break;
            }
        });
    }

    //***************************************************************************************/

    $scope.ProcesaEtiquetaMaster = function () {
        Servicio.ObtenerMaster($scope.Lectura.Etiqueta, $scope.Recuento.AlmacenSel).then(function (response) {
            $scope.Master.LineasMaster = [];
            $scope.Master.ErrorLineasMaster = [];
            $scope.Master.LotesMaster = [];
            $scope.Master.ErrorLotesMaster = [];
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

                            //let QtyEtqM = new Decimal($scope.Lectura.Etiqueta.split('&')[2].replace(",", ""));
                            //let qtyMaster = new Decimal(response.mensaje[0].MasterOnHandQty);
                            //if (QtyEtqM.toNumber() === qtyMaster.toNumber()) {

                            for (var i = 0; i < response.mensaje.length; i++) {
                                //Detalle de etiquetas
                                if (response.mensaje[i].TipoReg === "2") {
                                    if (response.mensaje[i].EtqLoteEstatus === 'L') {
                                        //Validar Lote no se haya leido de forma individualmente
                                        if (!$scope.ExisteEnTabla(response.mensaje[i].EtqLote)) {
                                            $scope.Master.LineasMaster.push(response.mensaje[i]); //Etiqueta individual correcta
                                        }
                                        //else {
                                        //    $scope.Master.ErrorLineasMaster.push({
                                        //        "DistNumber": response.mensaje[i].EtqLote,
                                        //        "Error": "se ha leído anteriormente",
                                        //        "Mostrar": "N"
                                        //    });
                                        //}
                                    }
                                    else {
                                        $scope.Master.ErrorLineasMaster.push({
                                            "DistNumber": response.mensaje[i].EtqLote,
                                            "Error": "No se encuentra Liberada",
                                            "Mostrar": "Y"
                                        });
                                    }
                                }

                                //Detalle de lotes
                                if (response.mensaje[i].TipoReg === "3") {
                                    //Validar Lote no se haya leido de forma individualmente
                                    if (!$scope.ExisteEnTabla(response.mensaje[i].Lote)) {
                                        $scope.Master.LotesMaster.push(response.mensaje[i]); //Lotes correctos
                                    }
                                    //else {
                                    //    $scope.Master.ErrorLotesMaster.push({
                                    //        "DistNumber": response.mensaje[i].Lote,
                                    //        "Error": "se ha leído anteriormente",
                                    //        "Mostrar": "N"
                                    //    });
                                    //}
                                }
                            }

                            console.log($scope.Master.LotesMaster);
                            //console.log($scope.Master.ErrorLotesMaster);

                            console.log($scope.Master.LineasMaster);
                            console.log($scope.Master.ErrorLineasMaster);

                            if ($scope.Master.ErrorLineasMaster.length === 0) {
                                if ($scope.Master.ErrorLineasMaster.length === $scope.Master.ErrorLotesMaster.length) {

                                    //let qtyMaster = new Decimal(response.mensaje[0].MasterOnHandQty);
                                    //let qtyLotes = new Decimal(QtyTotalLotes);
                                    //if (qtyMaster.toNumber() === qtyLotes.toNumber()) {
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
                                            , "Ctd_SAP": $scope.Master.LotesMaster[r].OnHandQty
                                            , "Ctd_Ctda": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                        });

                                        $scope.Recuento.Lineas.push({
                                            "DistNumber": $scope.Master.LotesMaster[r].Lote
                                            , "ItemCode": $scope.Master.LotesMaster[r].ItemCode
                                            , "ItemName": $scope.Master.LotesMaster[r].ItemName
                                            , "OnHandQty": $scope.Master.LotesMaster[r].EtqLoteOnHandQty

                                            , "WhsCodeSAP": $scope.Master.LotesMaster[r].WhsCode
                                            , "BinCodeSAP": $scope.Master.LotesMaster[r].BinCode
                                            , "AbsEntrySAP": $scope.Master.LotesMaster[r].AbsEntry

                                            , "WhsCode": $scope.Recuento.AlmacenSel
                                            , "BinCode": $scope.Memoria.BinCode
                                            , "AbsEntry": $scope.Memoria.AbsEntry
                                            
                                            , "Ctd_SAP": $scope.Master.LotesMaster[r].OnHandQty
                                            , "Ctd_Ctda": $scope.Master.LotesMaster[r].EtqLoteOnHandQty                                            

                                            , "Tipo": "PT"                                            
                                        });
                                    }
                                    
                                    console.log($scope.Master.RegMaster);
                                    console.log($scope.Recuento.Lineas);
                                    $scope.Lectura.Etiqueta = '';
                                    $("#Entradas").focus();
                                    //}
                                    //else {
                                    //    console.log("Validar etiqueta master, la cantidad no coincide con el acumulado de las etiquetas individuales");
                                    //    $scope.Alertas('e', "Verificar Etiqueta Master en SAP, cantidad Master " + response.mensaje[0].MasterOnHandQty + " no coincide con el acumulado de etiquetas individuales " + QtyTotalLotes);
                                    //    $scope.Lectura.Etiqueta = '';
                                    //    $("#Entradas").focus();
                                    //}
                                }
                                else {
                                    console.log("Verificar Lotes de etiqueta master, no estan completos en almacén origen");
                                    $scope.Alertas('e', "Etiqueta Master incompleta, verifique en SAP");
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

                                //for (var y = 0; y < $scope.Master.ErrorLotesMaster.length; y++) {
                                //    msjErrores = msjErrores + "<br/> > El Lote " + $scope.Master.ErrorLotesMaster[y].DistNumber + " - " + $scope.Master.ErrorLotesMaster[y].Error;
                                //}

                                $scope.Alertas('e', msjErrores);
                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();
                            }
                            //}
                            //else {
                            //    console.log("Validar etiqueta master, la cantidad de etiqueta no coincide con cantidad en SAP");
                            //    $scope.Alertas('e', "Verificar Etiqueta Master en SAP, cantidad etiqueta " + QtyEtqM.toNumber() + " no coincide con cantidad en SAP " + qtyMaster.toNumber());
                            //    $scope.Lectura.Etiqueta = '';
                            //    $("#Entradas").focus();
                            //}
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

    //***************************************************************************************/
    $scope.ExisteEnTabla = function (Tipo, Lote, Articulo, NumEti) {
        var Existe = false;

        switch (Tipo) {
            case "PT":
                for (var k = 0; k < $scope.Recuento.Lineas.length; k++) {
                    if ($scope.Recuento.Lineas[k].DistNumber === Lote && $scope.Recuento.Lineas[k].ItemCode === Articulo) {
                        Existe = true;
                        break;
                    }
                }
                break;

            case "MP": 
                for (var r = 0; r < $scope.Memoria.ListaMP.length; r++) {
                    if ($scope.Memoria.ListaMP[r].DistNumber === Lote && $scope.Memoria.ListaMP[r].ItemCode === Articulo && $scope.Memoria.ListaMP[r].NumEti == NumEti) {
                        Existe = true;
                        break;
                    }
                }
                break;
        }
                
        return Existe;
    }

    $scope.Eliminar = function (i) {
        var tipo = $scope.Recuento.Lineas[i].Tipo;

        switch (tipo) {
            case "PT":
                $scope.Recuento.Lineas.splice(i, 1);
                break;

            case "MP":
                //Por cada uno de los valores buscar y eliminar lotes MP
                for (var h = $scope.Memoria.ListaMP.length - 1; h >= 0; h--) {
                    if ($scope.Memoria.ListaMP[h].DistNumber === $scope.Recuento.Lineas[i].DistNumber &&
                        $scope.Memoria.ListaMP[h].ItemCode === $scope.Recuento.Lineas[i].ItemCode &&
                        $scope.Memoria.ListaMP[h].AbsEntry === $scope.Recuento.Lineas[i].AbsEntry) {

                        console.log("El lote " + $scope.Memoria.ListaMP[h].DistNumber + " -- con Num. " + $scope.Memoria.ListaMP[h].NumEti + ", ELIMINADO");

                        //Eliminar lote MP
                        $scope.Memoria.ListaMP.splice(h, 1);
                    }
                }

                $scope.Recuento.Lineas.splice(i, 1);
                break;
        }
    }

    $scope.ConfirmarUbicacion = function (idx) {
        //Listar ubicaciones de Almacen Destino
        Servicio.ListarUbicaciones($scope.Recuento.AlmacenSel).then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.AbrirModalUbicación(response.mensaje, idx);
                    break;

                case -1:
                    $scope.Alertas('w', "El Almacén " + $scope.Recuento.AlmacenSel + " no tiene ubicaciones");
                    break;

                case -2:
                    $scope.Alertas('e', "Error al consultar información de Ubicaciones de almacén " + $scope.Recuento.AlmacenSel + ", contacte al administrador de sistema");
                    break;
            }
        });
    }

    //***************************************************************************************/
    $scope.Documento = function () {
        if ($scope.ExistenCeros()) {
            $scope.Alertas('e', 'No puede dejar valores en 0');
        }
        else {
            Servicio.CrearTxt($scope.Recuento.Lineas, $scope.Recuento.AlmacenSel, $scope.Recuento.Num).then(function (response) {
                if (response.codigo == 0) {
                    $scope.Alertas('s', response.mensaje);
                    $scope.Limpiar();
                }
                else {
                    $scope.Alertas('e', response.mensaje);
                }
            })
        }
    }

    $scope.ExistenCeros = function () {
        var ceros = false;
        for (var c = 0; c < $scope.Recuento.Lineas.length; c++) {
            if ($scope.Recuento.Lineas[c].Ctd_Ctda == 0) {
                ceros = true;
                break;
            }
        }
        return ceros;
    }

    $scope.Limpiar = function () {
        $scope.Recuento = {
            Num: 1,
            Numeros: [{ Num: 1 }, { Num: 2 }, { Num: 3 }, { Num: 4 }, { Num: 5 }],
            AlmacenSel: '',
            Almacenes: [],
            Lineas: []
        }

        $scope.Lectura = {
            Etiqueta: ''
        }

        $scope.Memoria = {
            ListaMP: [], //Lista de etiquetas MP

            AbsEntry: 0, //Ubicación seleccionada del almacen
            BinCode: '' //Ubicación seleccionada del almacen
        }

        $scope.Master = {
            Existe: 'no',
            CodeML: '',
            LineasMaster: [], //Lista de etiquetas de la Master
            ErrorLineasMaster: [],
            LotesMaster: [], //Lista de Lotes de la Master en Almacén Origen seleccionado
            ErrorLotesMaster: [],
            RegMaster: []
        }

        //Carga de Almacenes
        $scope.CargarDts();
        //Verificar manejo de Master
        $scope.ValidaMaster();
    }

    //********************************************************************************************/
    //Modales
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
                    var tipo = $scope.Recuento.Lineas[reg].Tipo;

                    switch (tipo) {
                        case "PT":
                            $scope.Recuento.Lineas[reg].BinCode = result.BinCode;
                            $scope.Recuento.Lineas[reg].AbsEntry = result.AbsEntry;

                            console.log("Lote PT al Momento >> " + JSON.stringify($scope.Recuento.Lineas[reg]));
                            break;

                        case "MP":
                            //Validar si existe registro en ubicación | Lote, articulo y ubicación
                            let Ls = $scope.Recuento.Lineas;
                            let index = Ls.findIndex(linea => linea.DistNumber === $scope.Recuento.Lineas[reg].DistNumber &&
                                linea.ItemCode === $scope.Recuento.Lineas[reg].ItemCode &&
                                linea.AbsEntry === $scope.Recuento.Lineas[reg].AbsEntry);

                            if (index === -1) {
                                for (var h = 0; h < $scope.Memoria.ListaMP.length; h++) {
                                    if ($scope.Memoria.ListaMP[h].DistNumber === $scope.Recuento.Lineas[reg].DistNumber &&
                                        $scope.Memoria.ListaMP[h].ItemCode === $scope.Recuento.Lineas[reg].ItemCode &&
                                        $scope.Memoria.ListaMP[h].AbsEntry === $scope.Recuento.Lineas[reg].AbsEntry) {

                                        $scope.Memoria.ListaMP[h].AbsEntry = result.AbsEntry;
                                    }
                                }

                                console.log($scope.Memoria.ListaMP);

                                $scope.Recuento.Lineas[reg].BinCode = result.BinCode;
                                $scope.Recuento.Lineas[reg].AbsEntry = result.AbsEntry;

                                console.log("Lote MP al Momento >> " + JSON.stringify($scope.Recuento.Lineas[reg]));
                            }
                            else {
                                $scope.Alertas('w', 'Ya existen registro con ubicación seleccionada, eliminar lecturas y leer nuevamente en ubicación seleccionada');
                            }                      
                            break;
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
    //***************************************************************************************/
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

    //Iniciales
    $scope.CargarDts();
    //Verificar manejo de Master
    $scope.ValidaMaster();
        
});