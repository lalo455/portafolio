var validacionEmbApp = angular.module('validacionEmbApp', ['angular-growl']);

validacionEmbApp.factory('Ruta', function () {
    return { path: '' }
});

validacionEmbApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

validacionEmbApp.service('Servicio', function ($http, Ruta, $log) {
    this.ObtenerDetalleDoc = function (NoDoc) {
        return $http({
            url: Ruta.path + '/Embarque/CargarDocumento',
            method: 'POST',
            data: {
                Doc: NoDoc
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 1": response } }
            return error;
        });
    },
    this.obtenerDetalleEtiqueta = function (Etiqueta, CodigoDeSN) {
        return $http({
            url: Ruta.path + '/Embarque/ObtenerParteSN',
            method: 'POST',
            data: {
                Etiqueta: Etiqueta,
                CodigoSN: CodigoDeSN
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 2": response } }
            return error;
        });
    },
    this.ValidarDocumento = function (DocEmbarque, Lineas) {
        return $http({
            url: Ruta.path + '/Embarque/ValidarDocumento',
            method: 'POST',
            data: {
                NumEmbarque: DocEmbarque,
                LineasDoc : Lineas
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 3": response } }
            return error;
        });
    },
    this.BPEmbarques = function (DocEmbarque) {
        return $http({
            url: Ruta.path + '/Embarque/SociosEmbarques',
            method: 'GET',
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 4": response } }
            return error;
        });
    },
    this.Serial = function (NoSerie, Doc) {
        return $http({
            url: Ruta.path + '/Embarque/NoParte',
            method: 'POST',
            data: {
                Serial: NoSerie,
                Documento: Doc
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 5": response } }
            return error;
        });
    }
});

validacionEmbApp.controller('validacionEmbController', function ($scope, $window, Ruta, $log, Servicio, growl) {
    $('.mega').on('scroll', function () { $('.header').css('top', $(this).scrollTop()); });//scroll
    $scope.Entrada = '';
    $scope.Valores = {
        LicPlate: '',
        CardCode: '',
        Documento: '',
        LineasDoc: [],
        Clientes: [],//[{ "CardCode": "C091", "CardName": "Ford" }, { "CardCode": "C073", "CardName": "Audi-73" }, { "CardCode": "C018", "CardName": "Audi-18" }, { "CardCode": "C100", "CardName": "Audi-100" }],
        Cliente: ''
    }
    $scope.Turno = {
        Audi: true,
        AP: false
    }

    //Funciones y Servicios
    $scope.CargarDocumento = function () {
        if ($scope.Valores.Documento != "") {
            Servicio.ObtenerDetalleDoc($scope.Valores.Documento).then(function (response) {
                if (response.mensaje.length > 0) {
                    if (response.mensaje[0].U_CardCode == $scope.Valores.Cliente) {
                        $scope.Valores.LicPlate = response.mensaje[0].U_LicPlate;
                        $scope.Valores.CardCode = response.mensaje[0].U_CardCode
                        $scope.Valores.LineasDoc = response.mensaje;
                        //$log.debug("Lineas: " + JSON.stringify($scope.Valores.LineasDoc));
                        $scope.addSpecialWarnMessage('s', 'Información recuperada correctamente');
                    }
                    else {
                        $scope.addSpecialWarnMessage('e', 'El embarque no coincide con el cliente seleccionado');
                    }
                }
                else
                    $scope.addSpecialWarnMessage('e', 'El documento que ingreso no existe');
            });
        }
        else {
            $scope.addSpecialWarnMessage('e', 'Ingrese el número de embarque');
        }
    }

    $scope.ObternerSocios = function () {
        Servicio.BPEmbarques().then(function (response) {
            if (response.codigo == 0)
                $scope.Valores.Clientes = response.mensaje;
            else
                $scope.Alertas('e', response.mensaje);
        })
    }

    $scope.ObternerSocios();

    var Encontrado = -1;
    $scope.LeerCB = function () {
        var ValExt = $scope.ValidarEtiquetaExterna();
        var ValExi = $scope.ValidarEtiquetaLeida();
        if (ValExi == false && ValExt == false) {
            if ($scope.Turno.Audi && $scope.Entrada != "") {//Turno de etiqueta externa
                $scope.Turno.AP = false;
                switch ($scope.Valores.Cliente) {
                    case 'C073':
                    case 'C018':
                    case 'C100':
                        var Rango = $scope.Entrada.trim().replace($scope.Valores.LicPlate, "");
                        for (var a = 0; a < $scope.Valores.LineasDoc.length; a++) {
                            if (Rango >= $scope.Valores.LineasDoc[a].U_EtiDesde && Rango <= $scope.Valores.LineasDoc[a].U_EtiHasta) {
                                if ($scope.Valores.LineasDoc[a].U_Quantity == $scope.Valores.LineasDoc[a].U_QtyDisp) {
                                    $scope.addSpecialWarnMessage('e', 'Ya se ha completado el total para la linea ' + (a + 1));
                                    Encontrado = -2;
                                }
                                else {
                                    $scope.Turno.AP = true;
                                    $scope.Turno.Audi = false;
                                    Encontrado = a;
                                    if ($scope.Valores.LineasDoc[a].EtiExterno == "")
                                        $scope.Valores.LineasDoc[a].EtiExterno = $scope.Entrada.trim();
                                    else
                                        $scope.Valores.LineasDoc[a].EtiExterno += "," + $scope.Entrada.trim();
                                }
                                $scope.Entrada = '';
                                break;
                            }
                        }
                        if (Encontrado == -1) {
                            $scope.addSpecialWarnMessage('e', 'La etiqueta no se encuentra en el rango de las permitidas');
                            $scope.Entrada = '';
                        }
                        break;
                    case 'C091':
                        //for (var f = 0; f < $scope.Valores.LineasDoc.length; f++) {
                        //    if ($scope.Entrada.substring(1) == $scope.Valores.LineasDoc[f].U_ItemSN) {
                        //        if ($scope.Valores.LineasDoc[f].U_Quantity == $scope.Valores.LineasDoc[f].U_QtyDisp) {
                        //            $scope.addSpecialWarnMessage('e', 'Ya se completo el total para la linea ' + (f + 1));
                        //            Encontrado = -2;
                        //        }
                        //        else {
                        //            Encontrado = f;
                        //            $scope.Turno.AP = true;
                        //            $scope.Turno.Audi = false;
                        //            if ($scope.Valores.LineasDoc[f].EtiExterno == "")
                        //                $scope.Valores.LineasDoc[f].EtiExterno = $scope.Entrada.trim();
                        //            else
                        //                $scope.Valores.LineasDoc[f].EtiExterno += "," + $scope.Entrada.trim();
                        //        }
                        //    }
                        //}
                        //$scope.Entrada = '';
                        //if (Encontrado == -1) {
                        //    $scope.addSpecialWarnMessage('e', 'La etiqueta no se encuentró en el embarque');
                        //    $scope.Entrada = '';
                        //}
                        Servicio.Serial($scope.Entrada.substring(1), $scope.Valores.Documento).then(function (response) {
                            if (response.codigo == 0) {
                                for (var s = 0; s < $scope.Valores.LineasDoc.length; s++) {
                                    if ($scope.Valores.LineasDoc[s].LineId == response.mensaje) {
                                        Encontrado = s;
                                        break;
                                    }
                                }
                                if ($scope.Valores.LineasDoc[Encontrado].U_Quantity == $scope.Valores.LineasDoc[Encontrado].U_QtyDisp) {
                                    $scope.addSpecialWarnMessage('e', 'Ya se completo el total para la linea ' + (Encontrado + 1));
                                }
                                else {
                                    $scope.Turno.AP = true;
                                    $scope.Turno.Audi = false;
                                    if ($scope.Valores.LineasDoc[Encontrado].EtiExterno == "")
                                        $scope.Valores.LineasDoc[Encontrado].EtiExterno = $scope.Entrada.trim();
                                    else
                                        $scope.Valores.LineasDoc[Encontrado].EtiExterno += "," + $scope.Entrada.trim();
                                    $scope.Entrada = '';
                                }
                            }
                            else {
                                $scope.addSpecialWarnMessage('e', response.mensaje);
                                $scope.Entrada = '';
                            }
                        });
                        break;
                    case 'C037':
                    case 'C048':
                    case 'C077':
                        Servicio.Serial($scope.Entrada.substring(2), $scope.Valores.Documento).then(function (response) {
                            if (response.codigo == 0) {
                                for (var s = 0; s < $scope.Valores.LineasDoc.length; s++) {
                                    if ($scope.Valores.LineasDoc[s].LineId == response.mensaje) {
                                        Encontrado = s;
                                        break;
                                    }
                                }
                                if ($scope.Valores.LineasDoc[Encontrado].U_Quantity == $scope.Valores.LineasDoc[Encontrado].U_QtyDisp) {
                                    $scope.addSpecialWarnMessage('e', 'Ya se completo el total para la linea ' + (Encontrado + 1));
                                }
                                else {
                                    $scope.Turno.AP = true;
                                    $scope.Turno.Audi = false;
                                    if ($scope.Valores.LineasDoc[Encontrado].EtiExterno == "")
                                        $scope.Valores.LineasDoc[Encontrado].EtiExterno = $scope.Entrada.substring(2).trim();
                                    else
                                        $scope.Valores.LineasDoc[Encontrado].EtiExterno += "," + $scope.Entrada.substring(2).trim();
                                    $scope.Entrada = '';
                                }
                            }
                            else {
                                $scope.addSpecialWarnMessage('e', response.mensaje);
                                $scope.Entrada = '';
                            }
                        });
                        break;
                    case 'C069':

                        break;
                }
            }
            if ($scope.Turno.AP && $scope.Entrada != "") {
                $scope.Turno.Audi = false;
                if ($scope.Entrada.includes('&')) {
                    Servicio.obtenerDetalleEtiqueta($scope.Entrada.split('&')[1].trim(), $scope.Valores.CardCode).then(function (response) {
                        if (response.mensaje.length > 0) {
                            if (response.mensaje[0].Encontrado == -1 || response.mensaje[0].Encontrado == 0) {//no se encontro
                                $scope.addSpecialWarnMessage('e', 'El articulo de la etiqueta no se encontro en el catalogo de: ' + $scope.Valores.CardCode);
                                $scope.Entrada = '';
                            }
                            else {//se encontro artículo en el catalogo del socio de negocios
                                var ValPart = $scope.ValidarParte(response.mensaje[0].Parte, Encontrado);
                                if (ValPart == false) {
                                    $scope.addSpecialWarnMessage('e', 'El número de parte no coincide');
                                    $scope.Entrada = '';
                                    return;
                                }
                                else {
                                    $scope.Turno.AP = false;
                                    $scope.Turno.Audi = true;
                                    if ($scope.Valores.LineasDoc[Encontrado].U_Quantity == $scope.Valores.LineasDoc[Encontrado].U_QtyDisp) {
                                        $scope.addSpecialWarnMessage('e', 'Ya se ha completado el total para la linea ' + (Encontrado + 1));
                                    }
                                    else {
                                        $scope.Valores.LineasDoc[Encontrado].U_Quantity++;
                                        if ($scope.Valores.LineasDoc[Encontrado].EtiLeidas == "")
                                            $scope.Valores.LineasDoc[Encontrado].EtiLeidas = $scope.Entrada.trim();
                                        else
                                            $scope.Valores.LineasDoc[Encontrado].EtiLeidas += "," + $scope.Entrada.trim();
                                    }
                                    $scope.Entrada = '';
                                    Encontrado = -1;
                                }
                            }
                        }
                        else {
                            $scope.Entrada = ''
                            //$scope.addSpecialWarnMessage('e', 'No se encontro información sobre la etiqueta');
                            $scope.addSpecialWarnMessage('e', 'El articulo de la etiqueta no se encontro en el catalogo de: ' + $scope.Valores.CardCode);
                        }
                    });
                } else {
                    $scope.addSpecialWarnMessage('w', 'La etiqueta NO pertence a A&P Solution');
                    $scope.Entrada = '';
                }
            }
        }
        else {
            $scope.addSpecialWarnMessage('e', 'la etiqueta ya fue leída');
            $scope.Entrada = '';
        }
        //$("#Entradas").focus();
        document.getElementById("Entradas").focus(); 
    }

    $scope.ValidarParte = function (Part, indice) {
        var resp = false;
        for (var h = 0; h < $scope.Valores.LineasDoc.length; h++) {
            if ($scope.Valores.LineasDoc[h].U_ItemSN == Part && h == indice) {
                resp = true;
                break;
            }
        }
        return resp;
    }
    $scope.Refresh = function (i) {
        $scope.Valores.LineasDoc[i].EtiLeidas = "";
        $scope.Valores.LineasDoc[i].EtiExterno = "";
        $scope.Valores.LineasDoc[i].U_Quantity = 0;
        $scope.Turno = {
            Audi: true,
            AP: false
        }
    }
    $scope.Cambiar = function () {
        $scope.Valores.LineasDoc = [];
        $scope.Turno = {
            Audi: true,
            AP: false
        }
        $scope.Valores.Documento = '';
        //$scope.Valores.CardCode = '';
        //$scope.Valores.LicPlate = '';
        //$scope.Valores.Cliente = '';
        $scope.Entrada = '';
    }
    $scope.Limpiar = function () {
        $scope.Entrada = '';
        $scope.Valores = {
            LicPlate: '',
            CardCode: '',
            Documento: '',
            LineasDoc: [],
            Clientes: [],//[{ "CardCode": "C091", "CardName": "Ford" }, { "CardCode": "C073", "CardName": "Audi" }],
            Cliente: ''
        }
        $scope.Turno = {
            Audi: true,
            AP: false
        }
        Encontrado = -1;
        $scope.ObternerSocios();
    }
    $scope.ValidarEtiquetaLeida = function () {
        var existe = false;
        for (var l = 0; l < $scope.Valores.LineasDoc.length; l++) {
            var ArrEti = $scope.Valores.LineasDoc[l].EtiLeidas.split(',');
            for (var e = 0; e < ArrEti.length; e++) {
                if (ArrEti[e] == $scope.Entrada.trim()) {
                    existe = true;
                    break;
                }
            }
        }
        return existe;
    }
    $scope.ValidarEtiquetaExterna = function () {
        var ResExt = false;
        if ($scope.Valores.Cliente == 'C073' || $scope.Valores.Cliente == 'C018' || $scope.Valores.Cliente == 'C100') {//Si es audi
            for (var ex = 0; ex < $scope.Valores.LineasDoc.length; ex++) {
                var ArrExt = $scope.Valores.LineasDoc[ex].EtiExterno.split(',');
                for (var f = 0; f < ArrExt.length; f++) {
                    if (ArrExt[f] == $scope.Entrada.trim()) {
                        ResExt = true;
                        break;
                    }
                }
            }
        }
        if ($scope.Valores.Cliente == 'C091') {//Si es ford
            for (var ex = 0; ex < $scope.Valores.LineasDoc.length; ex++) {
                var ArrExt = $scope.Valores.LineasDoc[ex].EtiExterno.split(',');
                for (var f = 0; f < ArrExt.length; f++) {
                    if (ArrExt[f] == $scope.Entrada.substring(1).trim()) {
                        ResExt = true;
                        break;
                    }
                }
            }
        }
        if ($scope.Valores.Cliente == 'C037' || $scope.Valores.Cliente == 'C048' || $scope.Valores.Cliente == 'C077') {//Si es Chrysler
            for (var ex = 0; ex < $scope.Valores.LineasDoc.length; ex++) {
                var ArrExt = $scope.Valores.LineasDoc[ex].EtiExterno.split(',');
                for (var f = 0; f < ArrExt.length; f++) {
                    if (ArrExt[f] == $scope.Entrada.substring(2).trim()) {
                        ResExt = true;
                        break;
                    }
                }
            }
        }
        return ResExt;
    }

    $scope.ValidarDocumento = function () {
        var Generar = 0;
        for (var vd = 0; vd < $scope.Valores.LineasDoc.length; vd++) {
            if ($scope.Valores.LineasDoc[vd].U_Quantity != $scope.Valores.LineasDoc[vd].U_QtyDisp) {
                $scope.addSpecialWarnMessage('e', 'Existen lineas pendientes por validar');
                Generar = -1;
                break;
            }
        }
        if (Generar == 0) {
            Servicio.ValidarDocumento($scope.Valores.Documento, $scope.Valores.LineasDoc).then(function (response) {
                if (response.codigo == 0) {
                    $scope.addSpecialWarnMessage('s', 'Documento validado correctamente');
                    $scope.Limpiar();
                }
                else
                    $scope.addSpecialWarnMessage('e', 'Error: ' + response.mensaje);
            });
        }

    }
    //Carga de datos

    //Notificaciones
    $scope.addSpecialWarnMessage = function (tipo, mensaje) {
        switch (tipo) {
            case 'w':
                growl.warning(mensaje, { title: 'Aviso Sistema' });
                break;
            case 'i':
                growl.info(mensaje, { title: 'Aviso Sistema' });
                break;
            case 's':
                growl.success(mensaje, { title: 'Aviso Sistema' });
                break;
            case 'e':
                growl.error(mensaje, { title: 'Aviso Sistema' });
                break;
        }
    }
});