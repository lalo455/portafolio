var produccionApp = angular.module('produccionApp', ['angular-growl', 'ui.bootstrap', 'xeditable']);

produccionApp.factory('Ruta', function () {
    return { path: '' }
});

produccionApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

produccionApp.service('Servicio', function ($http, Ruta, $log) {
    this.ObtenerDocs = function (Accion, Documento) {
        return $http({
            url: Ruta.path + '/Produccion/ObtenerDocumentos',
            method: 'GET',
            params: {
                Accion: Accion,
                Documento: Documento
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

    this.Lectura = function (QR) {
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

    this.Fifo = function (qr, _lineas) {
        return $http({
            url: Ruta.path + '/Produccion/Fifo',
            method: 'POST',
            data: {
                Qr: qr,
                lineas: _lineas
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 5 ": response } }
            return error;
        });
    },

    this.Produccion = function (Documento, Memoria) {
        return $http({
            url: Ruta.path + '/Produccion/TraspasoAProduccion',
            method: 'POST',
            data: {
                Lineas: Documento,
                IAdicional: Memoria
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

produccionApp.controller('produccionController', function ($scope, $window, Ruta, $log, Servicio, $uibModal, growl) {
    $scope.Lectura = {
        Etiqueta: ''
    }

    $scope.Produccion = {
        NumOrdr: '',
        NumST:'',
        Lineas: []
    }

    $scope.DatosMovimiento = {
        Filler: '',
        ToWhsCode: ''
    }

    $scope.Memoria = {
        LineasOF: [],
        LineasSO:[],
        LineasPd: [],
        Procesando: false,

        ListaMP: [], //Lista de etiquetas MP, para verificar que no se haya leido la etiqueta con en el mismo numero

        ListWhsMP: ['AI-00001', 'AMP00001', 'ALME0012', 'ACM00002']
    }

    //***************************************************************/
    //Lista | Carga de Solicitudes de Traslado
    /*Se elimina la verificacion de la OF con la ST, esto para poder realizar trasnferencias de stock parciales - 8-01-2021*/
    $scope.CargarS = function () {
        $scope.Produccion.Lineas = [];
        $scope.Memoria.LineasSO = [];
        $scope.Memoria.ListaMP = [];
        $scope.Memoria.LineasOF = [];
        $scope.Produccion.NumOrdr = "";

        if ($scope.Produccion.NumST != '') {
            Servicio.ObtenerDocs('SO', $scope.Produccion.NumST).then(function (response) {
                switch (response.codigo) {
                    case 0:
                        $scope.Memoria.LineasSO = response.mensaje;
                        $scope.Produccion.NumOrdr = response.mensaje[0].OrdFab;
                        $scope.CargarOP($scope.Produccion.NumOrdr);
                        break;

                    case -1:
                        $scope.Alertas('e', 'No se recuperaron lineas de la Solicitud : ' + $scope.Produccion.NumST);
                        $scope.Produccion.NumST = "";
                        break;

                    case -2:
                        $scope.Alertas('e', 'Error al obtener información de la Solicitud : ' + $scope.Produccion.NumST);
                        $scope.Produccion.NumST = "";
                        break;
                }                  
            });
        }
        else {
            $scope.Alertas('e', 'Debe ingresar un número de solicitud');
        }
    }

    //Mostrar detalle de la ST
    $scope.DetalleSolicitud = function () {
        $scope.AbrirModalSolicitud($scope.Memoria.LineasSO);
    }

    //Lista | Carga de Ordenes de Fabricación
    $scope.CargarOP = function (NumOrdr) {
        if (NumOrdr > 0) {
            Servicio.ObtenerDocs('DO', NumOrdr).then(function (response) {
                switch (response.codigo) {
                    case 0:
                        $scope.Memoria.LineasOF = response.mensaje;
                        break;

                    case -1:
                        $scope.Alertas('e', 'No se recuperaron lineas de la Orden : ' + NumOrdr);
                        $scope.Produccion.NumST = "";
                        break;

                    case -2:
                        $scope.Alertas('e', 'Error al obtener información de la Orden : ' + NumOrdr);
                        $scope.Produccion.NumST = "";
                        break;
                }              
            });
        }
        else {
            $scope.Alertas('e', 'La Solicitud ' + $scope.Produccion.NumST + 'no tiene OF relacionada.');
        }
    }

    //***************************************************************/
    $scope.LecturaQR = function () {
        Servicio.Fifo($scope.Lectura.Etiqueta, $scope.Produccion.Lineas).then(function (_response) {
            if (_response.codigo == 0) {
                if (_response.mensaje) {
                    Servicio.Lectura($scope.Lectura.Etiqueta).then(function (response) {
                        switch (response.codigo) {
                            case 0:
                                switch (response.mensaje[0].Estatus) {
                                    case "0":
                                        //Validar almacen de registros con almacenes de MP
                                        var res = $scope.RemoverEtiquetas(response.mensaje, $scope.Lectura.Etiqueta.split('&')[1]);
                                        switch (res.length) {
                                            case 0:
                                                $scope.Alertas('e', 'No se encontro ninguna etiqueta en el almacén de MP');
                                                $scope.Lectura.Etiqueta = '';
                                                $("#Entradas").focus();
                                                break

                                            case 1:
                                                $scope.AñadirLinea(res, $scope.Lectura.Etiqueta);
                                                break;

                                            default:
                                                $scope.AbrirModalLotes(res);
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
                }
                else {
                    $scope.Alertas("w", _response.mensaje);
                    $scope.Lectura.Etiqueta = '';
                }
            }
            else {
                $scope.Alertas("e", _response.mensaje);
                $scope.Lectura.Etiqueta = '';
            }
        });
    }

    //Validar Almacen Origen con Almacenes de MP
    $scope.RemoverEtiquetas = function (array, item) {
        var nArra = [];
        for (var a = 0; a < array.length; a++) {
            if ((array[a].WhsCode == 'AI-00001' ||
                array[a].WhsCode == 'AMP00001' ||
                array[a].WhsCode == 'ALME0012' ||
                array[a].WhsCode == 'ACM00002') &&
                array[a].ItemCode == item) {

                nArra.push(array[a]);
            }
        }

        return nArra;
    }

    //Procesar registro
    $scope.AñadirLinea = function (res, qr) {
        //Validar etiqueta : Cancelada > N | Y
        if (res[0].Cancelado == 'Y') {
            $scope.Alertas('e', 'La etiqueta fue cancelada');
            $scope.Lectura.Etiqueta = '';
            return;
        }

        //Agregar informacion
        Object.assign(res[0], { "StockSAP": res[0].OnHandQty });
        res[0].OnHandQty = parseFloat($scope.Lectura.Etiqueta.split('&')[2].replace(',', ''));

        var existe = $scope.ExisteEnDocumento(res);
        if (existe != -1) {
            if (res[0].WhsCode != $scope.Memoria.LineasSO[0].ToWhsCode) {

                if (qr.startsWith("L")) {//Es PT
                    var existePT = false;
                    for (var n = 0; n < $scope.Produccion.Lineas.length; n++) {
                        if ($scope.Produccion.Lineas[n].DistNumber == res[0].split('&')[0]) {
                            existePT = true;
                            break;
                        }
                    }
                    if (!existePT) {
                        let j = $scope.GruposLineas(qr, res, 'O', $scope.Memoria.LineasSO[existe].Quantity, 0);
                        if (j !== -2) {
                            $scope.Produccion.Lineas.push(res[0]);
                            $scope.Lectura.Etiqueta = '';
                        }
                    } else {
                        $scope.Alertas('e', 'La etiqueta ya fue agregada');
                        $scope.Lectura.Etiqueta = ''
                    }
                }
                else {//es de MP

                    //Verificar si etiqueta ha sido leida anteriormente en caso de ser MP | Comparar con la lista de etiquetas MP
                    var idx = -3;
                    console.log($scope.Memoria.ListaMP);
                    console.log($scope.Memoria.ListaMP.length);
                    if ($scope.Memoria.ListaMP.length > 0 && res[0].Tipo === "MP") {
                        console.log('Verificar MP');
                        idx = $scope.GruposLineas($scope.Lectura.Etiqueta, res, 'M');
                    }
                    else {
                        idx = -3;
                    }

                    if (idx === -3) {

                        let j = $scope.GruposLineas(qr, res, 'O', $scope.Memoria.LineasSO[existe].Quantity, 0);
                        if (j !== -2) {//cantidad sumanda del articulo no supera el stock

                            let i = $scope.GruposLineas(qr, res, 'D', 0);
                            if (i !== -1) {//verificamos si existe

                                //verificamos que no se supere la cantidad del lote en SAP
                                let l = $scope.GruposLineas(qr, res, 'L', $scope.Memoria.LineasSO[existe].Quantity, i);
                                if (l !== -5) {
                                    $scope.Produccion.Lineas[i].OnHandQty = parseFloat($scope.Produccion.Lineas[i].OnHandQty) + parseFloat(qr.split('&')[2].replace(",", ""));
                                    $scope.Produccion.Lineas[i].Etqs++;

                                    if (res[0].Tipo == 'MP') {
                                        Object.assign(res[0], {
                                            "NumEtq": $scope.Lectura.Etiqueta.split('&')[4]
                                            , "QtyEtq": $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                                        })

                                        $scope.Memoria.ListaMP.push(res[0]); //Se agrega información de etiqueta a lista de etiquetas MP
                                        console.log($scope.Memoria.ListaMP);
                                    }

                                    console.log($scope.Produccion.Lineas);
                                    $scope.Lectura.Etiqueta = '';
                                }
                            }
                            else {//nueva linea

                                Object.assign(res[0], {
                                    "Etqs": 1
                                })//añadimos nuevos campos

                                $scope.Produccion.Lineas.push(res[0]);

                                if (res[0].Tipo == 'MP') {
                                    Object.assign(res[0], {
                                        "NumEtq": $scope.Lectura.Etiqueta.split('&')[4]
                                        , "QtyEtq": $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                                    })

                                    $scope.Memoria.ListaMP.push(res[0]); //Se agrega información de etiqueta a lista de etiquetas MP
                                    console.log($scope.Memoria.ListaMP);
                                }

                                console.log($scope.Produccion.Lineas);

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
            }
            else {
                $scope.Alertas('e', 'El almacén destino de la solicitud es igual al almacén origen de la etiqueta');
                $scope.Lectura.Etiqueta = '';
                $("#Entradas").focus();
            }
        }
        else {
            $scope.Alertas('e', 'El Articulo no existe en el documento: ' + $scope.Produccion.NumST);
            $("#Entradas").focus();
            $scope.Lectura.Etiqueta = '';
        }
    }

    //Validar Registro en ST
    $scope.ExisteEnDocumento = function (qr) {
        var Existe = -1;
        for (var i = 0; i < $scope.Memoria.LineasSO.length; i++) {
            if ($scope.Memoria.LineasSO[i].ItemCode == qr[0].ItemCode) {
                Existe = i;
                break;
            }
        }
        return Existe;
    }

    //Validar información
    $scope.GruposLineas = function (qr, response, tipo, quantity, i) {
        let index = -1;
        switch (tipo) {

            case 'D'://Existe
                let Ls = $scope.Produccion.Lineas;
                index = Ls.findIndex(linea => linea.DistNumber === response[0].DistNumber && linea.ItemCode === response[0].ItemCode && linea.AbsEntry === response[0].AbsEntry);
                break;

            case 'O'://Cantidad a superar de la solicitud
                let Lineas = $scope.Produccion.Lineas;
                let sum = 0;
                for (let m = 0; m < Lineas.length; m++) {
                    if (Lineas[m].ItemCode == qr.split('&')[1]) {
                        sum += parseFloat(Lineas[m].OnHandQty);
                    }
                }
                if (Lineas.length > 0) {
                    let acum = parseFloat(sum) + parseFloat(qr.split('&')[2].replace(",", ""));
                    if (trunc(acum) > quantity) {
                        $scope.Alertas('e', `Esta excediendo la cantidad de ${quantity} para la solicitud ${$scope.Produccion.NumST} del artículo ${qr.split('&')[1]}`);
                        $scope.Lectura.Etiqueta = '';
                        index = -2;
                        break;
                    }
                } else {
                    if (parseFloat(qr.split('&')[2].replace(",", "")) > response) {
                        $scope.Alertas('e', `Esta excediendo la cantidad de ${quantity} para la solicitud ${$scope.Produccion.NumST} del artículo ${qr.split('&')[1]}`);
                        $scope.Lectura.Etiqueta = '';
                        index = -2;
                    }
                }
                break;

            case "L"://No superar cantidad de SAP para MP del lote
                //verificamos cantidad a superar
                var acum = parseFloat($scope.Produccion.Lineas[i].OnHandQty) + parseFloat(qr.split("&")[2].replace(",", ""));
                if (trunc(acum) > $scope.Produccion.Lineas[i].StockSAP) {
                    $scope.Alertas("e", "La etiqueta con lote: " + qr.split("&")[0] + " y artículo: " + qr.split("&")[1] + " NO PUEDE superar la cantidad de: " + $scope.Produccion.Lineas[i].StockSAP);
                    $scope.Lectura.Etiqueta = '';
                    index = -5;
                    break;
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

    //Eliminar registro
    $scope.Eliminar = function (i) {

        console.log('Lote: ' + $scope.Produccion.Lineas[i].DistNumber);

        //Por cada uno de los valores buscar y eliminar lotes MP
        for (var h = $scope.Memoria.ListaMP.length - 1; h >= 0; h--) {
            if ($scope.Memoria.ListaMP[h].DistNumber === $scope.Produccion.Lineas[i].DistNumber) {
                console.log('Lote: ' + $scope.Produccion.Lineas[i].DistNumber + ' -- Posicion: ' + h);

                console.log("El lote " + $scope.Memoria.ListaMP[h].DistNumber + " -- con Num. " + $scope.Memoria.ListaMP[h].NumEtq + ", ELIMINADO");

                //Eliminar lote MP
                $scope.Memoria.ListaMP.splice(h, 1);
            }
        }

        $scope.Produccion.Lineas.splice(i, 1);
    }

    //***************************************************************/
    //Generar movimiento
    $scope.EnvioProduccion = function () {
        $scope.Memoria.Procesando = true;
        Servicio.Produccion($scope.Produccion, $scope.Memoria).then(function (response) {
            if (response.codigo == 0) {
                $scope.Alertas('s', 'Documento creado correctamente: ' + response.mensaje);
                $scope.Limpiar();
                $scope.Memoria.Procesando = false;
            }
            else {
                $scope.Alertas('e', response.mensaje);
                $scope.Memoria.Procesando = false;
            }
        });
    }

    //Reinicio de módulo
    $scope.Limpiar = function () {
        $scope.Lectura = {
            Etiqueta: ''
        }
        $scope.Produccion = {
            NumOrdr: '',
            NumST: '',
            Lineas: []
        }
        $scope.DatosMovimiento = {
            Filler: '',
            ToWhsCode: ''
        }

        $scope.Memoria = {
            LineasOF: [],
            LineasSO: [],
            LineasPd: [],
            Procesando: false,

            ListaMP: [], //Lista de etiquetas MP, para verificar que no se haya leido la etiqueta con en el mismo numero

            ListWhsMP: ['AI-00001', 'AMP00001', 'ALME0012', 'ACM00002']
        }
    }

    //***********************************************************************************/
    $scope.AbrirModalOrdenes = function (ListOrd) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaOF.html',
            controller: ListOrdController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                OrdList: function () {
                    return ListOrd;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result != null && result != "") {
                $scope.Produccion.NumOrdr = result.DocNum;
                $scope.CargarOP();
            }
        });
    }
    var ListOrdController = function (OrdList, $scope, $uibModalInstance, $log) {
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion != null ? $scope.Seleccion : "");
        }
        $scope.Ordenes = OrdList;
        $scope.OrdSeleccionado = function (val) {
            $scope.Seleccion = val;
            $scope.ok();
        }
    }

    $scope.AbrirModalSolicitud = function (LineasSol) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaSolicitud.html',
            controller: SolicitudLineasController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                SolList: function () {
                    return LineasSol;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
        });
    }
    var SolicitudLineasController = function (SolList, $scope, $uibModalInstance, $log) {
        $scope.ok = function () {
            $uibModalInstance.close();
        }
        $scope.SolLineas = SolList;
    }

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
            if (result != null && result != "") {

                $scope.AñadirLinea([result], $scope.Lectura.Etiqueta);
            }
            $scope.Lectura.Etiqueta = '';
            $("#Entradas").focus();
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

    //************************************************************************************/
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

});