/*
Master
1 Solo articulo
Solo PT
Solo lecturas individuales
*/
var etqMasterApp = angular.module('etqMasterApp', ['angular-growl', 'ui.bootstrap', 'xeditable']);

etqMasterApp.factory('Ruta', function () {
    return { path: '' }
});

etqMasterApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

etqMasterApp.service('Servicio', function ($http, Ruta, $log) {

    this.ObtenerPrefijo = function () {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ObtenerPrefijo',
            method: 'GET',
            data: {},
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

    this.GrabarLectura = function (etq, nlectura) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/GrabarLectura',
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

    this.VEtqLiberada = function (Lote) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/VerificarEtq',
            method: 'GET',
            params: {
                Lote: Lote
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

    this.ObtenerDetalle = function (Lote, Articulo) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ObtenerLote',
            method: 'GET',
            params: {
                Lote: Lote
                , Articulo: Articulo
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

    this.GenerarM = function (Lineas) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/GenerarMaster',
            method: 'POST',
            data: {
                ListaEtq: Lineas
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


    this.ListarMaster = function () {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ListarMaster',
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

    this.ObtenerMaster = function (master) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ObtenerMaster',
            method: 'GET',
            params: {
                master: master
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

    this.EliminarLoteM = function (loteReg, articuloReg, cantidadReg, master, cantidad) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/EliminarLoteM',
            method: 'POST',
            data: {
                loteReg: loteReg
                , articuloReg: articuloReg
                , cantidadReg: cantidadReg
                , master: master
                , cantidad: cantidad
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

    this.ActualizarM = function (Lineas) {
        return $http({
            url: Ruta.path + '/EtiquetaMaster/ActualizarMaster',
            method: 'POST',
            data: {
                ListaEtq: Lineas
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

etqMasterApp.controller('etqMasterController', function ($scope, $window, Ruta, $log, Servicio, $uibModal, growl) {
    $scope.Lectura = {
        Etiqueta: '',
        Base: 'crearM',
        AccBase: [{ Tipo: 'crearM', Accion: 'Crear' }, { Tipo: 'adminM', Accion: 'Administrar' }]
    }

    $scope.Etiquetas = {
        Lineas: []
    }

    $scope.Memoria = {
        Procesando: false,
        ArticuloMaster: '',
        Nlectura: 1,
        Existe: '', //Variable que indica si se tiene Prefijo de Etiqueta Master
        CodeML: '', //Variable para obtener el Prefijo de la Etiqueta Master
        
        Master: '', //Nuevo codigo Master para actualizar los lotes leidos, opcional, vacio
        LineasMaster: [], //Lotes de etiqueta master de la NewMaster, para comparar que los lotes leidos sean del mismo articulo, prevenir que se lea un lote que ya existe en la master
        MasterCargada: 'N',
        TurnoMaster: 1
    }

    $scope.Racks = {
        Mensajes: ''
    }

    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    $scope.CargarInformacion = function () {
        //Obtener prefijo de la Etiqueta Master
        Servicio.ObtenerPrefijo().then(function (_response) {
            if (_response.codigo === 0) {
                $scope.Memoria.Existe = 'si';
                $scope.Memoria.CodeML = _response.mensaje;
                console.log('Codigo ML: ' + $scope.Memoria.CodeML);
            }
            else {
                $scope.Memoria.Existe = 'no';
                $scope.Memoria.CodeML = '';
                console.log('Error ::: ' + _response.mensaje);
            }
        });
    }

    $scope.CambiarAccion = function (accion) {
        //Se limpian campos para las nuevas lecturas
        console.log('Accion: ' + $scope.Lectura.Base + ' ::: accion:' + accion + '::: LIMPIAR LECTURAS Y VALORES PARA LA NUEVA INFORMACION');
        $scope.Memoria.Master = '';
        $scope.Memoria.ArticuloMaster = "";
        $scope.Memoria.TurnoMaster = 1;
        $scope.Memoria.LineasMaster = [];
        $scope.Etiquetas.Lineas = [];
        $scope.Memoria.MasterCargada = 'N';
    }
    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    //Listar Etiquetas Master disponibles 
    $scope.ListarMaster = function () {
        Servicio.ListarMaster().then(function (_response) {
            if (_response.codigo === 0) {
                $scope.AbrirModalLista(_response.mensaje);
            }
            else {
                $scope.Alertas('e', _response.mensaje);
            }
        });
    }

    //Cargar información completa de Master Seleccionada
    $scope.CargarEtMaster = function () {
        console.log('Master a Cargar: ' + $scope.Memoria.Master);
        $scope.Memoria.LineasMaster = [];
        $scope.Etiquetas.Lineas = [];
        $scope.Memoria.ArticuloMaster = "";
        $scope.Memoria.TurnoMaster = 1;
        var msj = "";

        Servicio.ObtenerMaster($scope.Memoria.Master).then(function (response) {
            if (response.codigo === 0) {
                console.log(response.mensaje);
                $scope.Memoria.MasterCargada = 'Y';

                //Validar Calidad de Master | Liberada: O | Bloqueada: X
                if (response.mensaje[0].Calidad !== "O") {
                    $scope.Alertas('w', 'La Etiqueta Master ' + $scope.Memoria.Master + ' se encuentra Bloqueda');
                    $scope.Memoria.MasterCargada = 'N';
                    return;
                }

                var error = 0;
                msj = "Verificar los siguientes registros individuales de master leída: " + $scope.Memoria.Master;
                var qtyFinal = 0;
                //Recorrido y validación de Lotes Individuales
                for (var r = 0; r < response.mensaje.length; r++) {
                    //Validar Estatus de Lote | 0: Liberado | 1: No accesible | 2: Bloqueado || Cancelado || Calidad || Estatus Etiqueta
                    if (response.mensaje[r].LoteEstatus === "0") {
                        if (response.mensaje[r].EtqLoteCancelado === "N") {
                            if (response.mensaje[r].EtqLoteCalidad === "O") {
                                if (response.mensaje[r].EtqLoteEstatus === "L") {

                                    let valorQtyFinal = new Decimal(qtyFinal);
                                    let valorQtyEtq = new Decimal(response.mensaje[r].EtqLoteOnHandQty);
                                    qtyFinal = valorQtyFinal.plus(valorQtyEtq).toNumber();

                                    $scope.Memoria.LineasMaster.push(response.mensaje[r]);
                                    $scope.Etiquetas.Lineas.push({
                                        "DistNumber": response.mensaje[r].EtqLote,
                                        "ItemCode": response.mensaje[r].Articulo,
                                        "OnHandQty": response.mensaje[r].EtqLoteOnHandQty,
                                        "Turno": response.mensaje[r].EtqLoteTurno,
                                        "Tipo": "PT",
                                        "EtqMaster": response.mensaje[r].Codigo,
                                        "TurnoMaster": response.mensaje[r].Turno,
                                        "RegMaster": "Y" //Indica si es registro de master, al eliminar solicitar confirmación de eliminación de lote de la master
                                    });

                                }
                                else {
                                    error++;
                                    msj = msj + "<br/> > Etiqueta con Lote " + response.mensaje[r].EtqLote + " no se encuentra Liberada";
                                }
                            }
                            else {
                                error++;
                                msj = msj + "<br/> > Etiqueta con Lote " + response.mensaje[r].EtqLote + " se encuentra Bloqueda";
                            }
                        }
                        else {
                            error++;
                            msj = msj + "<br/> > Etiqueta con Lote " + response.mensaje[r].EtqLote + " se encuentra Cancelada";
                        }
                    }
                    else {
                        error++;
                        if (response.mensaje[r].LoteEstatus === "1") {
                            msj = msj + "<br/> > Lote " + response.mensaje[r].EtqLote + " tiene Estatus: No Accesible";
                        }
                        else {
                            msj = msj + "<br/> > Lote " + response.mensaje[r].EtqLote + " tiene Estatus: Bloqueado";
                        }                        
                    }
                }

                //Validar errores encontrados
                if (error > 0) {
                    $scope.Alertas('w', msj);
                    $scope.Memoria.Master = "";
                    $scope.Memoria.MasterCargada = 'N';
                    $scope.Memoria.LineasMaster = [];
                    $scope.Etiquetas.Lineas = [];
                    return;
                }

                //Validar Cantidad Lotes y Cantidad Master
                let qtyAcum = new Decimal(qtyFinal);
                let qtyMaster = new Decimal(response.mensaje[0].MasterOnHandQty);
                if (qtyAcum.toNumber() !== qtyMaster.toNumber()) {
                    $scope.Alertas('w', "Verificar registros de Etiqueta Master, la cantidad de la etiqueta " + qtyMaster.toNumber() + " no coincide con el acumulado " + qtyAcum.toNumber() + " de sus etiquetas individuales");
                    $scope.Memoria.Master = "";
                    $scope.Memoria.MasterCargada = 'N';
                    $scope.Memoria.LineasMaster = [];
                    return;
                }

                //***********************************************************************/
                $scope.Memoria.ArticuloMaster = response.mensaje[0].Articulo; //Articulo de la Master | Solo se permiten etiquetas individuales de dicho articulo
                $scope.Memoria.TurnoMaster = response.mensaje[0].Turno;

                console.log("Master cargada para su edición: " + $scope.Memoria.Master);
                console.log($scope.Memoria.LineasMaster);
                console.log($scope.Etiquetas.Lineas);

                $scope.Lectura.Etiqueta = '';
                $("#Entradas").focus();
            }
            else {
                $scope.Alertas('e', response.mensaje);
                $scope.Memoria.NewMaster = '';
                $("#EtMt").focus();
            }
        });
    }

    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    //Lectura de Etiqueta
    $scope.LecturaQR = function () {
        Servicio.GrabarLectura($scope.Lectura.Etiqueta, $scope.Memoria.Nlectura).then(function (response) //Revisa el estatus de la etiqueta 
        {
            console.log(response.mensaje.length + ' --- # ' + $scope.Memoria.Nlectura);
            $scope.Memoria.Nlectura++;
        });
        console.log('Prefijo etiqueta leida: ' + $scope.Lectura.Etiqueta.split('&')[0].substring(0, $scope.Memoria.CodeML.length));

        //Verificar si la etiqueta es master o unica por el prefijo
        if ($scope.Lectura.Etiqueta.split('&')[0].substring(0, $scope.Memoria.CodeML.length) === $scope.Memoria.CodeML) {
            $scope.Alertas('e', 'No se permite lectura de Etiqueta Master, solo etiquetas individuales que no esten contenidas en alguna Master');
            $scope.Lectura.Etiqueta = '';
            $("#Entradas").focus();
        }
        else {
            //Procesamiento de ETIQUETA UNICA
            console.log('Procesamiento de ETIQUETA UNICA');
            $scope.ProcesaEtiquetaUnica();
        }
    }

    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    //Procesa de Etiqueta Unica
    $scope.ProcesaEtiquetaUnica = function () {
        //Verificar si la Etiqueta Leida es PERMITIDA: Solo se permiten etiquetas de PT (inicial con L) => L12345AB 
        if ($scope.Permitida($scope.Lectura.Etiqueta.split('&')[0])) {

            //Validar si la etiqueta ya fue leida
            if ($scope.ExisteEnTabla($scope.Lectura.Etiqueta.split('&')[0])) {
                $scope.Alertas('e', 'La etiqueta ya fue leída');
                $scope.Lectura.Etiqueta = '';
                $("#Entradas").focus();
                return;
            }

            //Validar estatus de Etiqueta leída
            Servicio.VEtqLiberada($scope.Lectura.Etiqueta.split('&')[0]).then(function (_response) {
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
                        //Obtener detalle de Lote/etiqueta leída
                        Servicio.ObtenerDetalle($scope.Lectura.Etiqueta.split('&')[0], $scope.Lectura.Etiqueta.split('&')[1]).then(function (response) {
                            if (response.codigo === 0) {
                                console.log(response.mensaje[0]);

                                switch (response.mensaje[0].Estatus) {
                                    case "0": //Lote Liberado
                                        //Validar que el el Articulo de la Etiqueta sea igual al Articulo de la Primera Etiqueta
                                        if ($scope.ArticuloValido(response.mensaje[0].ItemCode)) {
                                            //Validación: Existe en Master | NO se procesa | se manda alerta indicando que pertenece a otra Master
                                            switch (response.mensaje[0].EtqMaster) {
                                                case "WM": //Lote sin master
                                                    switch ($scope.Lectura.Base) {
                                                        case "crearM":
                                                            //Se agrega a las lineas la informacion de la etiqueta
                                                            Object.assign(response.mensaje[0], {
                                                                "RegMaster": "N"
                                                            });

                                                            $scope.Etiquetas.Lineas.push(response.mensaje[0]);
                                                            console.log($scope.Etiquetas.Lineas);
                                                            $scope.Lectura.Etiqueta = '';
                                                            $("#Entradas").focus();
                                                            break;

                                                        case "adminM":
                                                            //Se agrega a las lineas la informacion de la etiqueta
                                                            Object.assign(response.mensaje[0], {
                                                                "RegMaster": "N"
                                                            });

                                                            response.mensaje[0].NewEtqMaster = $scope.Memoria.Master;
                                                            response.mensaje[0].TurnoMaster = $scope.Memoria.TurnoMaster;

                                                            $scope.Etiquetas.Lineas.push(response.mensaje[0]);
                                                            console.log($scope.Etiquetas.Lineas);
                                                            $scope.Lectura.Etiqueta = '';
                                                            $("#Entradas").focus();
                                                            break;
                                                    }
                                                    break;

                                                default:
                                                    $scope.Alertas('e', 'La etiqueta ya se encuentra registrada en la Master ' + response.mensaje[0].EtqMaster);
                                                    $scope.Lectura.Etiqueta = '';
                                                    $("#Entradas").focus();
                                                    break;
                                            }
                                        }
                                        else {
                                            $scope.Alertas('e', 'El Articulo de la Etiqueta no es Valido');
                                            $scope.Lectura.Etiqueta = '';
                                            $("#Entradas").focus();
                                        }
                                        break;

                                    case "1":
                                        $scope.Alertas('e', 'El lote de la etiqueta esta NO Accesible');
                                        $scope.Lectura.Etiqueta = '';
                                        $("#Entradas").focus();
                                        break;

                                    case "2":
                                        $scope.Alertas('e', 'El lote de la etiqueta esta Bloqueado');
                                        $scope.Lectura.Etiqueta = '';
                                        $("#Entradas").focus();
                                        break;

                                    default:
                                        $scope.Alertas('e', 'Error en validacion de Lote de Etiqueta, volver a leer');
                                        $scope.Lectura.Etiqueta = '';
                                        $("#Entradas").focus();
                                        break;
                                }

                            }
                            else {
                                $scope.Alertas('e', response.mensaje);
                                $scope.Lectura.Etiqueta = '';
                                $("#Entradas").focus();
                            }
                        });
                        break;
                }
            });            
        }
        else {
            $scope.Lectura.Etiqueta = '';
            $("#Entradas").focus();
        }
    }

    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    //Validar si la Etiqueta es Permitida: Solo se permiten etiquetas PT (Inicial con L) => L12345ABCDE
    $scope.Permitida = function (etqs) {
        switch (etqs.substring(0, 1)) {
            case 'L':
                return true;

            default:
                $scope.Alertas('e', 'La etiqueta no esta permitida.');
                return false;
        }
    }

    //Validar que la Etiqueta NO haya sido leido antes
    $scope.ExisteEnTabla = function (lote) {
        var existe = false;

        //Recorrer la lista de Etiquetas leidas
        for (var n = 0; n < $scope.Etiquetas.Lineas.length; n++) {
            //Comparar Lote en lineas leidas con el Lote de la etiqueta actual
            if ($scope.Etiquetas.Lineas[n].DistNumber === lote) {
                existe = true;
                break;
            }
        }

        return existe;
    }

    //Validar que el Articulo de la Etiqueta sea IGUAL al Articulo de la Etiqueta Inicial
    $scope.ArticuloValido = function (Articulo) {
        console.log('Articulo de la Etiqueta: ' + Articulo);
        console.log('Articulo de la Master: ' + $scope.Memoria.ArticuloMaster);

        var valido = false;
        if ($scope.Memoria.ArticuloMaster === "") {
            $scope.Memoria.ArticuloMaster = Articulo;
            console.log('Articulo de la Master: ' + $scope.Memoria.ArticuloMaster);
            valido = true;
        }
        else {
            if (Articulo === $scope.Memoria.ArticuloMaster) {
                valido = true;
            }
        }

        return valido;
    }

    //----------------------------------------------------------------------------
    //Eliminar Registro de las lecturas leidas
    $scope.Eliminar = function (i) {
        //Validar datos del registro
        var tipoReg = $scope.Etiquetas.Lineas[i].RegMaster; //Y: pertenece a Master | N: nuevo registro
        var loteReg = $scope.Etiquetas.Lineas[i].DistNumber;
        var articuloReg = $scope.Etiquetas.Lineas[i].ItemCode;
        var cantidadReg = $scope.Etiquetas.Lineas[i].OnHandQty;
        var master = $scope.Memoria.Master;        
        var cantidad = $scope.Memoria.LineasMaster[0].MasterOnHandQty; 

        console.log('Reg Master: ' + tipoReg + ' | Lote: ' + loteReg + ' | Articulo: ' + articuloReg + ' | Cantidad: ' + cantidadReg + ' | Master: ' + master + ' | Cantidad: ' + cantidad);
        if (tipoReg === 'N') {
            //Eliminar registro de lista leida
            $scope.Etiquetas.Lineas.splice(i, 1);

            if ($scope.Etiquetas.Lineas.length === 0) {
                $scope.Memoria.ArticuloMaster = "";
            }
        }
        else {
            $scope.Memoria.Procesando = true;
            //Eliminar 
            var dialog = new Messi(
                "¿Desea eliminar el Lote " + loteReg + " de la Etiqueta Master " + master + "?",
                {
                    title: 'Mensaje de Confirmación', modal: true,
                    buttons: [
                        { id: "btnSi", label: "Si", val: "Y", class: 'btn-success' },
                        { id: "btnNo", label: "No", val: "N", class: 'btn-danger' }
                    ],
                    titleClass: "warning",
                    callback: function (val) {
                        if (val === 'Y') {
                            console.log('Eliminar lote de master | actualizar registro master | actualizar cantidad master');
                            Servicio.EliminarLoteM(loteReg, articuloReg, cantidadReg, master, cantidad).then(function (_response) {
                                if (_response.codigo === 0) {
                                    $scope.Alertas('s', _response.mensaje);

                                    //Eliminar registro de lista leida
                                    $scope.Etiquetas.Lineas.splice(i, 1);
                                    $scope.Memoria.Procesando = false;

                                    if ($scope.Etiquetas.Lineas.length === 0) {
                                        $scope.Alertas('i', "La etiqueta " + $scope.Memoria.Master + " ya no se encuentra disponible");

                                        $scope.Memoria.Master = "";
                                        $scope.Memoria.ArticuloMaster = "";
                                        $scope.Memoria.TurnoMaster = 1;
                                        $scope.Memoria.LineasMaster = [];
                                        $scope.Etiquetas.Lineas = [];
                                        $scope.Memoria.MasterCargada = 'N';
                                    }
                                }
                                else {
                                    $scope.Alertas('e', _response.mensaje);
                                    $scope.Memoria.Procesando = false;
                                }
                            });
                        }
                    }
                });
        }
    }

    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    //Generar Etiqueta Master
    $scope.GenerarMovimiento = function () {
        $scope.Memoria.Procesando = true;
        console.log('Accion de Master: ' + $scope.Lectura.Base);

        //Crear Master
        if ($scope.Lectura.Base === "crearM") {
            Servicio.GenerarM($scope.Etiquetas.Lineas).then(function (response) {
                if (response.codigo === 0) {
                    //Creacion de Etiqueta Master 
                    $scope.Alertas('s', "Creación de Etiqueta Master " + response.mlcodigo + ", Realizada");
                    $scope.Alertas('i', response.mensaje);

                    //Imprimir Etiqueta Master
                    $scope.ImprimirEtqMaster(response.mlcodigo);

                    //---------------------------------------------
                    $scope.Limpiar();
                    $scope.Memoria.Procesando = false;
                }
                else {
                    //Error en creacion de Etiqueta Master
                    $scope.Alertas('e', "Error al generar master: " + response.mensaje);
                    $scope.Memoria.Procesando = false;
                }
            });
        }
        else {
            //Administrar Master
            let Lineas = $scope.Etiquetas.Lineas;
            let index = Lineas.findIndex(linea => linea.RegMaster === 'N');

            if (index === -1) {
                //No hay registros nuevos | solo se reimprime etiqueta
                console.log('Reimprimir Master');
                $scope.Alertas('i', "Reimpresión de Etiqueta " + $scope.Memoria.Master);

                //Imprimir Etiqueta Master
                $scope.ImprimirEtqMaster($scope.Memoria.Master);
                $scope.Limpiar();
                $scope.Memoria.Procesando = false;
            }
            else {
                //Existen registros nuevos | solo procesar los lotes nuevos | obtener acumulado de lotes nuevos, sumar acumulado a la cantidad actual de master 
                console.log('Actualizar master con nuevos registros');
                Servicio.ActualizarM($scope.Etiquetas.Lineas).then(function (response) {
                    if (response.codigo === 0) {
                        //Creacion de Etiqueta Master 
                        $scope.Alertas('s', "Actualización de Etiqueta Master " + response.mlcodigo + ", Realizada");
                        $scope.Alertas('i', response.mensaje);

                        //Imprimir Etiqueta Master
                        $scope.ImprimirEtqMaster(response.mlcodigo);

                        //---------------------------------------------
                        $scope.Limpiar();
                        $scope.Memoria.Procesando = false;
                    }
                    else {
                        //Error en creacion de Etiqueta Master
                        $scope.Alertas('e', "Error al generar master: " + response.mensaje);
                        $scope.Memoria.Procesando = false;
                    }
                });
            }
        }
    }

    $scope.Limpiar = function () {
        $scope.Lectura = {
            Etiqueta: '',
            Base: 'crearM',
            AccBase: [{ Tipo: 'crearM', Accion: 'Crear' }, { Tipo: 'adminM', Accion: 'Administrar' }]
        }

        $scope.Etiquetas = {
            Lineas: []
        }

        $scope.Memoria = {
            Procesando: false,
            ArticuloMaster: '',
            Nlectura: 1,
            Existe: '', //Variable que indica si se tiene Prefijo de Etiqueta Master
            CodeML: '', //Variable para obtener el Prefijo de la Etiqueta Master

            Master: '', //Nuevo codigo Master para actualizar los lotes leidos, opcional, vacio
            LineasMaster: [], //Lotes de etiqueta master de la NewMaster, para comparar que los lotes leidos sean del mismo articulo, prevenir que se lea un lote que ya existe en la master
            MasterCargada: 'N',
            TurnoMaster: 1
        }

        $scope.Racks = {
            Mensajes: ''
        }

        $scope.CargarInformacion();
    }

    //Mostrar EtqMaster
    $scope.ImprimirEtqMaster = function (EtqMaster) {

        console.log('Impresion de Etiqueta: EtqMaster: ' + EtqMaster);
        if (EtqMaster !== undefined && EtqMaster !== "") {
            $('<form>', {
                "method": "POST",
                "action": Ruta.path + '/EtiquetaMaster/ImprimirEtqMaster?Codigo=' + EtqMaster,
                "target": "_blank"
            }).appendTo(document.body).submit();
        }
        else {
            $scope.Alertas('e', "No se ha enviado Informacion completa para Imprimir Etiqueta Master");
        }
    }

    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------
    //Modales
    $scope.AbrirModalLista = function (ListaMaster) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaMaster.html',
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
                $scope.Memoria.Master = result.Codigo;
                console.log('Master seleccionada: ' + $scope.Memoria.Master);
                $scope.CargarEtMaster();
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

    //----------------------------------------------------------------------------
    //----------------------------------------------------------------------------

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

    //Funciones Iniciales
    $scope.CargarInformacion();

});
