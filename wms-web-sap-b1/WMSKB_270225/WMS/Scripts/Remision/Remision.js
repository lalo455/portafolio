/*
Remisión
Cliente obligatorio
Con o Sin pedido
El pedido solo valida articulos permitidos
El pedido no valida cantidad
Cantidad es libre
Cantidad NO debe ser mayor a Stock en almacén de registro
*/
var remisionApp = angular.module('remisionApp', ['angular-growl', 'ui.bootstrap', 'xeditable']);

remisionApp.factory('Ruta', function () {
    return { path: ''}
});

remisionApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

remisionApp.service('Servicio', function ($http, Ruta, $log) {
    this.ObtenerDts = function (Tipo, Parametros, Mth) {
        return $http({
            url: Ruta.path + '/Remision/' + Mth,
            method: 'POST',
            data: {
                Tipo: Tipo,
                Param: Parametros
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
    
    this.Crear = function (Documento, Guia, RegMaster) { 
        return $http({
            url: Ruta.path + '/Remision/Crear',
            method: 'POST',
            data: {
                Documento: Documento, 
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
    
    this.CatalogoSocioDeNegocios = function (_cardCode) {
        return $http({
            url: Ruta.path + '/Remision/NumerosDeCatalogos',
            method: 'GET',
            params: {
                CardCode: _cardCode
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
            url: Ruta.path + '/Remision/GrabarLectura',
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

    this.ObtenerListaMaster = function (QR) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ObtenerListaMaster',
            method: 'GET',
            params: {
                QR: QR
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
        },
        this.ValidarPedido = function (etiquetaData) {
            return $http.post(Ruta.path + '/Remision/ValidarPedido', {
                ordenVentaSAP: etiquetaData.ordenVentaSAP,
                numeroLineaPedido: etiquetaData.lineaPedido,
                descripcionArticulo: etiquetaData.descripcion,
                numeroPiezas: etiquetaData.piezas.toString(),
                area: etiquetaData.area
            }).then(function (response) {
                return response.data;
            });
        };


});

remisionApp.controller('remisionController', function ($scope, $window, Ruta, $log, Servicio, growl, $uibModal) {
    //Variables
    $scope.Lectura = {
        Etiqueta: ''
    }

    $scope.Dts = {
        Catalogo: []
    }

    $scope.Remision = {
        ClienteSel: '',
        Clientes: [],
        Pedido: '',
        Referencia: '',
        Tarima: 1,
        Lineas:[]
    }

    $scope.Memoria = {
        LineasPd: [],
        Procesando: false, 
        Nlectura: 1, 
        CodeGuia: '', //Variable para el código de seguimiento y colocado en el Documento
        WhsOrigen: 'APT00007'
    }

    $scope.Master = {
        Existe: 'no',
        CodeML: '',
        LineasMaster: [], //Lista de etiquetas de la Master
        ErrorLineasMaster: [],
        LotesMaster: [], //Lista de Lotes de la Master en Almacén Origen seleccionado
        ErrorLotesMaster: [],
        RegMaster: [], 
        Almacen: ''
    }
    $scope.etiquetasLeidas = [];
    $scope.lecturaActual = {
        area: '',
        huacal: '',
        ordenVentaSAP: '',
        ordenVenta: '',
        od: '',
        lineaPedido: '',
        color: '',
        descripcion: '',
        piezas: 0
    };

    //*********************************************************************************************

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
    //*********************************************************************************************
    $scope.ObtenerClientes = function () {
        Servicio.ObtenerDts('C', '', 'ObtenerClientes').then(function (response) {
            if (response.codigo === 0) 
                $scope.Remision.Clientes = response.mensaje;
            else 
                $scope.Alertas('e', response.mensaje);
        });
    }

    $scope.CatagaloCliente = function (CardCode) {
        Servicio.CatalogoSocioDeNegocios(CardCode).then(function (response) {
            if (response.codigo === 0) {
                $scope.Dts.Catalogo = response.mensaje;
            }
            else {
                $scope.Alertas('w', response.mensaje);
            }
        });
    }

    $scope.CargarPedido = function () {
        $scope.Memoria.Procesando = true;
        if ($scope.Remision.Pedido === '') {
            Servicio.ObtenerDts('P', $scope.Remision.ClienteSel, 'ObtenerPedidos').then(function (response) {
                if (response.codigo === 0) {
                    $scope.AbrirModalPedidos(response.mensaje);
                    $scope.Memoria.Procesando = false;
                }
                else {
                    $scope.Alertas('e', response.mensaje);
                    $scope.Memoria.Procesando = false;
                }
            });
        }
        else {
            Servicio.ObtenerDts('LP', $scope.Remision.Pedido, 'ObtenerDetallePed').then(function (response) {
                if (response.codigo === 0) {
                    $scope.Alertas('s', 'Pedido cargado correctamente');
                    $scope.Memoria.LineasPd = response.mensaje;
                    $scope.Memoria.Procesando = false;
                }
                else {
                    $scope.Alertas('e', response.mensaje);
                    $scope.Memoria.Procesando = false;
                }
            });
        }
    }

    //*********************************************************************************************
    $scope.LecturaQR = function () {
        //Grabar informacion
        Servicio.GrabarLectura($scope.Lectura.Etiqueta, $scope.Memoria.Nlectura).then(function (response) //Revisa el estatus de la etiqueta 
        {
            console.log(response.mensaje.length + ' --- # ' + $scope.Memoria.Nlectura);
            $scope.Memoria.Nlectura++;
        });

        var proceder = false;
        if (!$scope.lecturaActual.Etiqueta) return;

        try {
            const datos = $scope.lecturaActual.Etiqueta.split('|');

            if (datos.length !== 9) {
                growl.error('Formato de etiqueta inválido');
                return;
            }

            // Estructurar datos
            $scope.lecturaActual = {
                area: datos[0],
                huacal: datos[1],
                ordenVentaSAP: datos[2],
                ordenVenta: datos[3],
                od: datos[4],
                lineaPedido: datos[5],
                color: datos[6],
                descripcion: datos[7],
                piezas: parseInt(datos[8]) || 0
            };

            // Validar si ya fue leída
            if ($scope.etiquetasLeidas.some(e => e.huacal === $scope.lecturaActual.huacal)) {
                growl.warning(`El huacal ${$scope.lecturaActual.huacal} ya fue escaneado`);
                return;
            }

            // Validar pedido en SAP
            $scope.ValidarPedido();

        } catch (ex) {
            growl.error('Error al procesar etiqueta: ' + ex.message);
        }
    };

    // Función para validar pedido
    $scope.ValidarPedido = function () {
        var etiquetaData = {
            ordenVentaSAP: $scope.lecturaActual.ordenVentaSAP,
            lineaPedido: $scope.lecturaActual.lineaPedido,
            descripcion: $scope.lecturaActual.descripcion,
            piezas: $scope.lecturaActual.piezas,
            area: $scope.lecturaActual.area,
            huacal: $scope.lecturaActual.huacal
        };

        Servicio.ValidarPedido(etiquetaData).then(function (response) {
            if (response.codigo === 0) {
                if (response.area === 'L') {
                    // Procesar área Laminado
                    var pedidoExistente = $scope.pedidos.find(function (p) {
                        return p.OrdenVenta === response.ordenVenta &&
                            p.Huacal === response.huacal;
                    });

                    if (!pedidoExistente) {
                        // Crear nuevo pedido principal
                        var nuevoPedido = {
                            OrdenVenta: response.ordenVenta,
                            Huacal: response.huacal,
                            Area: response.area,
                            Articulo: response.articulo || '',
                            Piezas: response.piezas,
                            Cliente: response.cliente || '',
                            Detalles: [{
                                Huacal: response.huacal,
                                Descripcion: response.descripcion,
                                Piezas: response.piezas
                            }]
                        };
                        $scope.pedidos.push(nuevoPedido);
                    } else {
                        // Agregar al detalle del pedido existente
                        pedidoExistente.Detalles.push({
                            Huacal: response.huacal,
                            Descripcion: response.descripcion,
                            Piezas: response.piezas
                        });
                        // Actualizar total de piezas
                        pedidoExistente.Piezas = $scope.calcularTotalPiezas(pedidoExistente.Detalles);
                    }
                } else {
                    // Procesar área Arquitectura
                    if (response.datos && response.datos.length > 0) {
                        var totalPiezas = response.datos.reduce(function (total, item) {
                            return total + (item.NumeroPiezas || 0);
                        }, 0);

                        // Crear pedido principal con el primer elemento
                        var pedidoPrincipal = {
                            OrdenVenta: response.datos[0].OrdenVentaSAP,
                            Huacal: response.huacalPrincipal,
                            Area: response.area,
                            Articulo: response.articulo || '',
                            Piezas: totalPiezas,
                            Cliente: response.cliente || '',
                            Detalles: []
                        };

                        // Agregar todos los datos como detalles
                        response.datos.forEach(function (item) {
                            pedidoPrincipal.Detalles.push({
                                Huacal: item.OrdenVentaSAP, // O el campo adecuado para huacal
                                Descripcion: item.DescripcionArticulo,
                                Piezas: item.NumeroPiezas
                            });
                        });

                        $scope.pedidos.push(pedidoPrincipal);
                    }
                }

                growl.success(response.mensaje);
                $scope.lecturaActual.Etiqueta = '';
                console.log('Estructura actual:', $scope.pedidos);
            } else {
                growl.error(response.mensaje);
            }
        });
    };
        
    

    //***********************************************************************************************/
    
    //Validaciones
    $scope.Validacion = function (TipoVal, Param) {
        var Resultado = false;
        switch (TipoVal) {
            case 'B': //Validar lectura 
                for (var i = 0; i < $scope.Remision.Lineas.length; i++) {
                    if ($scope.Remision.Lineas[i].DistNumber === Param) {
                        Resultado = true;
                        $scope.Alertas('e', 'La etiqueta ya fue leída');
                        $scope.Lectura.Etiqueta = '';
                        break;
                    }

                    //Validar si existe Master leida | validar lote individual en master leida
                    if ($scope.Master.RegMaster.length > 0) {
                        for (var m = 0; m < $scope.Master.RegMaster.length; m++) {
                            if ($scope.Master.RegMaster[m].DistNumber === $scope.Lectura.Etiqueta.split("&")[0]) {
                                Resultado = true;
                                $scope.Alertas("w", "La etiqueta ya existe en Master Leida " + $scope.Master.RegMaster[m].EtqMaster);
                                $scope.Lectura.Etiqueta = '';
                                break;
                            }
                        }
                    }
                }
                break;

            case 'E': //Validar articulo en pedido cargado
                for (var j = 0; j < $scope.Memoria.LineasPd.length; j++) {
                    if (Param === $scope.Memoria.LineasPd[j].ItemCode) {
                        Resultado = true;
                        break;
                    }
                }
                break;

            case 'C'://Validar articulo en catálogo de cliente
                for (var c = 0; c < $scope.Dts.Catalogo.length; c++) {
                    if (Param === $scope.Dts.Catalogo[c]) {
                        Resultado = true;
                        break;
                    }
                }
                break;

            case 'M': //Validar lectura Master 
                for (var i = 0; i < $scope.Remision.Lineas.length; i++) {
                    if ($scope.Remision.Lineas[i].DistNumber === Param) {
                        Resultado = true;
                        $scope.Alertas('w', "La etiqueta master " + Param + " ya ha sido leída");
                        $scope.Lectura.Etiqueta = '';
                        $("#Entradas").focus();
                        break;
                    }
                }
                break;

            case 'ML': //Validar lectura lote master en lecturas individuales 
                for (var i = 0; i < $scope.Remision.Lineas.length; i++) {
                    if ($scope.Remision.Lineas[i].DistNumber === Param) {
                        Resultado = true;
                        break;
                    }
                }
                break;
        }

        return Resultado;
    }

    //Validar Almacen Origen
    $scope.RemoverEtiquetas = function (registros) {
        var arrayRegW = [];
        for (var a = 0; a < registros.length; a++) {
            if (registros[a].WhsCode == $scope.Memoria.WhsOrigen) {
                arrayRegW.push(registros[a]);
            }
        }

        return arrayRegW;
    }

    //*********************************************************************************************
    $scope.ProcesaEtiquetaMaster = function () {
        $scope.Master.Almacen = "";

        if (!$scope.Validacion("M", $scope.Lectura.Etiqueta.split('&')[0])) {
            //Previo a obtener información de Master se procede a validar si existe en más de 1 Almacén
            Servicio.ObtenerListaMaster($scope.Lectura.Etiqueta).then(function (_response) {
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
                        switch (_response.mensaje.length) {
                            case 1:
                                $scope.Master.Almacen = $scope.Memoria.WhsOrigen; // _response.mensaje[0].WhsCode;
                                $scope.EtiquetaMaster();
                                break;

                            default:
                                $scope.AbrirModalLista(_response.mensaje);
                                break;
                        }
                        break;
                        break;
                }
            });
        }
    }

    $scope.EtiquetaMaster = function () {
        Servicio.ObtenerMaster($scope.Lectura.Etiqueta, $scope.Master.Almacen).then(function (response) {
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

                            let QtyEtqM = new Decimal($scope.Lectura.Etiqueta.split('&')[2].replace(",", ""));
                            let qtyMaster = new Decimal(response.mensaje[0].MasterOnHandQty);
                            if (QtyEtqM.toNumber() === qtyMaster.toNumber()) {

                                for (var i = 0; i < response.mensaje.length; i++) {
                                    //Detalle de etiquetas
                                    if (response.mensaje[i].TipoReg === "2") {
                                        if (response.mensaje[i].EtqLoteEstatus === 'L' && response.mensaje[i].EtqLoteCalidad === 'O') {
                                            //Validar Lote no se haya leido de forma individualmente
                                            if (!$scope.Validacion("ML", response.mensaje[i].EtqLote)) {
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
                                                    "Error": "No cuenta con stock suficiente en Almacén " + $scope.Master.Almacen
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
                                                    , "Estatus": $scope.Master.LotesMaster[r].LoteEstatus
                                                    , "Cancelado": "N"
                                                    , "Tipo": $scope.Master.LotesMaster[r].Tipo
                                                    , "EtqMaster": response.mensaje[0].Codigo
                                                    , "QtySAP": $scope.Master.LotesMaster[r].OnHandQty

                                                    , "Tarima": $scope.Remision.Tarima
                                                    , "Dispositivo": $scope.Master.LotesMaster[r].Dispositivo
                                                    , "PesoBruto": "0"
                                                    , "PesoNeto": "0"
                                                    , "NumEtq": "1"
                                                    , "QtyEtq": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                    , "QtyEmbarque": $scope.Master.LotesMaster[r].EtqLoteOnHandQty
                                                    , "Etqs": 1
                                                    , "TipoLectura": "M"
                                                });
                                            }
                                            console.log($scope.Master.RegMaster);

                                            $scope.Remision.Lineas.push({
                                                "DistNumber": response.mensaje[0].Codigo
                                                , "ItemCode": $scope.Master.RegMaster[0].ItemCode
                                                , "WhsCode": $scope.Master.RegMaster[0].WhsCode
                                                , "BinCode": $scope.Master.RegMaster[0].BinCode
                                                , "AbsEntry": $scope.Master.RegMaster[0].AbsEntry
                                                , "OnHandQty": response.mensaje[0].MasterOnHandQty
                                                , "Etqs": $scope.Master.RegMaster.length
                                                , "TipoLectura": "M"
                                                , "Tarima": $scope.Remision.Tarima
                                            });

                                            console.log($scope.Remision.Lineas);
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
                                        $scope.Alertas('e', "Etiqueta Master incompleta, no todos los lotes se encuentran en Almacén " + $scope.Master.Almacen + ", verifique en SAP");
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

    //********************************************************************************************* PENDIENTE FUNCIONALIDAD ACTUALIZAR CANTIDAD
    $scope.ConfirmarCantidad = function (dt, indx) {
        console.log('ACTUAL > Indice: ' + indx +
            ' ---- Cantidad en SAP: ' + $scope.Remision.Lineas[indx].QtySAP +
            ' ---- Cantidad Inicial: ' + $scope.Remision.Lineas[indx].QtyEmbarque +
            ' ---- Cantidad Etiqueta: ' + $scope.Remision.Lineas[indx].QtyEtq +
            ' ---- Cantidad NUEVA: ' + parseFloat(dt));

        let QtyFinal = 0;
        //let valorQtyEmbarque;
        let valorActualizar;
        let valorQtySAP;

        //console.log($scope.Remision.Lineas);
        //let Lineas = $scope.Remision.Lineas;

        //Acumulado total actual + cantidad de etiqueta
        //valorQtyEmbarque = new Decimal($scope.Remision.Lineas[indx].QtyEmbarque);
        valorActualizar = new Decimal(dt);
        console.log('Acumulado previo | Cantidad Traspaso: ' + $scope.Remision.Lineas[indx].QtyEmbarque + ' --- Cantidad: ' + parseFloat(dt));

        valorQtySAP = new Decimal($scope.Remision.Lineas[indx].QtySAP);
        let QtySAP = valorQtySAP.toNumber();
        let NewQty = valorActualizar.toNumber();

        if (NewQty === QtySAP) {
            QtyFinal = parseFloat(dt);
        }
        else if (NewQty > QtySAP) {
            $scope.Alertas("w", "La etiqueta con lote individual: " + $scope.Remision.Lineas[indx].DistNumber +
                " y artículo: " + $scope.Remision.Lineas[indx].ItemCode +
                " NO PUEDE superar el Stock de: " + QtySAP);

            QtyFinal = QtySAP;
        }
        else {
            //No: Cantidad a embarcar será la cantidad ingresada
            QtyFinal = parseFloat(dt);
        }

        //Se actualiza informacion de cantidad a embarcar del Lote 
        $scope.Remision.Lineas[indx].OnHandQty = parseFloat(QtyFinal);

        //Se actualiza la cantidad a embarcar [Cantidad inicial para antes de actualizar cantidad]
        $scope.Remision.Lineas[indx].QtyEmbarque = parseFloat(QtyFinal);

        console.log('ACTUALIZACION > Lote: ' + $scope.Remision.Lineas[indx].DistNumber +
            ' --- Cantidad  Lote: ' + $scope.Remision.Lineas[indx].OnHandQty +
            ' --- Cantidad  QtyTraspaso: ' + $scope.Remision.Lineas[indx].QtyEmbarque +
            ' --- Cantidad  QtyEtiqueta: ' + $scope.Remision.Lineas[indx].QtyEtq +
            ' --- Cantidad en SAP: ' + $scope.Remision.Lineas[indx].QtySAP);
    }

    $scope.Eliminar = function (j) {
        let TipoLectura = $scope.Remision.Lineas[j].TipoLectura; // I: Individual | M: Master

        if (TipoLectura === "I") {
            console.log('Eliminar Lote: ' + $scope.Remision.Lineas[j].DistNumber);

            $scope.Remision.Lineas.splice(j, 1);

            if ($scope.Remision.Lineas.length === 0) {
                $scope.Memoria.Nlectura = 1;
            }
        }
        else {
            //Eliminar registros individuales de Master
            let MasterCodigo = $scope.Remision.Lineas[j].DistNumber;

            //Por cada uno de los valores buscar y eliminar lotes de Master
            for (var h = $scope.Master.RegMaster.length - 1; h >= 0; h--) {
                if ($scope.Master.RegMaster[h].EtqMaster === MasterCodigo) {
                    console.log("Lote " + $scope.Master.RegMaster[h].DistNumber + ", ELIMINADO");

                    //Eliminar lote 
                    $scope.Master.RegMaster.splice(h, 1);
                }
            }

            $scope.Remision.Lineas.splice(j, 1);

            if ($scope.Remision.Lineas.length === 0) {
                $scope.Memoria.Nlectura = 1;
            }

            console.log("Lotes de Master restantes >> " + JSON.stringify($scope.Master.RegMaster));
            console.log("Registros leídos restantes >> " + JSON.stringify($scope.Remision.Lineas));
        }        
    }

    //*********************************************************************************************
    //Generar Movimiento
    $scope.Documento = function () {
        $scope.Memoria.Procesando = true;
        console.log('Codigo guia enviado: ' + $scope.Memoria.CodeGuia);
        console.log('Lineas leidas: >>>>>>');
        console.log($scope.Remision.Lineas); //Lineas actuales a procesar
        console.log('Registros master');
        console.log($scope.Master.RegMaster); //Lineas master a procesar

        //Servicio.Crear($scope.Traspaso, $scope.Memoria.CodeGuia)
        Servicio.Crear({
            "ClienteSel": $scope.Remision.ClienteSel,
            "Pedido": $scope.Remision.Pedido,
            "Referencia": $scope.Remision.Referencia,
            "Lineas": $scope.Remision.Lineas
        }, $scope.Memoria.CodeGuia, $scope.Master.RegMaster).then(function (response) {
            if (response.codigo === 0) {
                $scope.Memoria.Procesando = false;
                $scope.Alertas('s', response.mensaje);
                $scope.Limpiar();
            }
            else {
                $scope.Alertas('e', response.mensaje);
                $scope.Memoria.Procesando = false;
            }
        })
    }
    
    $scope.Limpiar = function () {
        $scope.Lectura = {
            Etiqueta: ''
        }

        $scope.Dts = {
            Catalogo: []
        }

        $scope.Remision = {
            ClienteSel: '',
            Clientes: [],
            Pedido: '',
            Referencia: '',
            Tarima: 1,
            Lineas: []
        }

        $scope.Memoria = {
            LineasPd: [],
            Procesando: false,
            Nlectura: 1,
            CodeGuia: '', //Variable para el código de seguimiento y colocado en el Documento
            WhsOrigen: 'APT00007'
        }

        $scope.Master = {
            Existe: 'no',
            CodeML: '',
            LineasMaster: [], //Lista de etiquetas de la Master
            ErrorLineasMaster: [],
            LotesMaster: [], //Lista de Lotes de la Master en Almacén Origen seleccionado
            ErrorLotesMaster: [],
            RegMaster: [],
            Almacen: ''
        }

        $scope.ObtenerClientes();
        $scope.CodigoGuia();
        //Verificar manejo de Master
        $scope.ValidaMaster();
    }
    
    //*********************************************************************************************
    //Modales
    $scope.AbrirModalPedidos = function (ListaPedidos) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/Pedidos.html',
            controller: ListaPedidosController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                ListaPed: function () {
                    return ListaPedidos;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result !== "") {
                $scope.Remision.Pedido = result.DocNum;
                $scope.CargarPedido();
            }
            else {
                $scope.Alertas('e', 'Error al recuperar datos');
            }
        });
    }
    var ListaPedidosController = function ($scope, $uibModalInstance, $log, ListaPed) {
        $scope.Pedidos = ListaPed;
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion != null ? $scope.Seleccion : "");
        }

        $scope.PedidoSeleccionado = function (SelecValue) {
            $scope.Seleccion = SelecValue;
            $scope.ok();
        }
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
            if (result !== null && result !== "") {

                if (result.Cancelado === 'Y') {
                    $scope.Alertas('e', 'La etiqueta fue cancelada');
                    $scope.Lectura.Etiqueta = '';
                    return;
                }

                if (!$scope.Validacion('B', result.DistNumber)) {

                    //Agregar etiqueta a registros a procesar
                    Object.assign(result, {
                        "QtySAP": result.OnHandQty,
                        "Tarima": $scope.Remision.Tarima,
                        "Dispositivo": $scope.Lectura.Etiqueta.split('&')[4],
                        "PesoBruto": $scope.Lectura.Etiqueta.split('&')[5].replace(',', ''),
                        "PesoNeto": $scope.Lectura.Etiqueta.split('&')[6].replace(',', ''),
                        "TipoLectura": "I",
                        "QtyEmbarque": 0.0,
                        "QtyEtq": $scope.Lectura.Etiqueta.split('&')[2].replace(",", "")
                    });

                    //Validar cantidad de etiqueta contra stock en sap
                    if (result.OnHandQty == parseFloat($scope.Lectura.Etiqueta.split('&')[2].replace(",", ""))) {
                        console.log("Cantidad igual");
                        result.OnHandQty = $scope.Lectura.Etiqueta.split('&')[2].indexOf(",") === 1 ? $scope.Lectura.Etiqueta.split('&')[2].replace(",", "") : $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");
                    }
                    else if (result.OnHandQty > parseFloat($scope.Lectura.Etiqueta.split('&')[2].replace(",", ""))) {
                        result.OnHandQty = $scope.Lectura.Etiqueta.split('&')[2].indexOf(",") === 1 ? $scope.Lectura.Etiqueta.split('&')[2].replace(",", "") : $scope.Lectura.Etiqueta.split('&')[2].replace(",", "");
                    }
                    else {
                        $scope.Alertas("w",
                            "La etiqueta con lote: " + $scope.Lectura.Etiqueta.split("&")[0] +
                            " y artículo: " + $scope.Lectura.Etiqueta.split("&")[1] +
                            " NO PUEDE superar el Stock de: " + result.OnHandQty);
                    }

                    //Cantidad a embarcar | permite validar la actualización de cantidad
                    result.QtyEmbarque = result.OnHandQty;

                    console.log("Registro >> " + JSON.stringify(result));
                    $scope.Remision.Lineas.push(result);
                    console.log("Total registros >> " + JSON.stringify($scope.Remision.Lineas));
                }
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

    $scope.AbrirModalLista = function (ListaMaster) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaMasterWhs.html',
            controller: ListaMasterController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                ListaM: function () {
                    return ListaMaster;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result !== "") {
                $scope.Master.Almacen = result.WhsCode;
                console.log('Almacen origen de Master seleccionada: ' + $scope.Master.Almacen);
                $scope.EtiquetaMaster();
            }
            else {
                //$scope.Alertas('e', 'Error al recuperar datos');
                console.log('Error al recuperar datos o cierre de ventana');
            }
        });
    }
    var ListaMasterController = function ($scope, $uibModalInstance, $log, ListaM) {
        $scope.ListaM = ListaM;
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion != null ? $scope.Seleccion : "");
        }

        $scope.MasterSeleccionada = function (SelecValue) {
            $scope.Seleccion = SelecValue;
            $scope.ok();
        }
    }
    //*********************************************************************************************
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

    //*********************************************************************************************
    //Funciones Iniciales
    $scope.ObtenerClientes();

    $scope.CodigoGuia();
    //Verificar manejo de Master
    $scope.ValidaMaster();

});