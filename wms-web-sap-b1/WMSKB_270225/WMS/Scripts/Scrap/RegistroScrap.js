var regScrapApp = angular.module('regScrapApp', ['xeditable', 'ui.bootstrap', 'angular-growl']);

regScrapApp.factory('Ruta', function () {
    return { path: '' }
});

regScrapApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

regScrapApp.run(function (editableOptions) {
    editableOptions.theme = 'bs3';
});

regScrapApp.service('Servicio', function ($http, Ruta, $log) {

    this.ListaTurnos = function () {
        return $http({
            url: Ruta.path + '/Scrap/Turnos',
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

        this.HorasTurno = function (Turno) {
            return $http({
                url: Ruta.path + '/Scrap/HorasTurno',
                method: 'GET',
                params: {
                    Turno: Turno
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

        this.ListaOperadores = function () {
            return $http({
                url: Ruta.path + '/Scrap/ListaOperadores',
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
                url: Ruta.path + '/Scrap/GrabarLectura',
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

        this.ValidaOF = function (NumOF) {
            return $http({
                url: Ruta.path + '/Scrap/ValidaOF',
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

        this.ListaMaquinas = function () {
            return $http({
                url: Ruta.path + '/Scrap/Maquinas',
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

        this.ValidaMaquina = function (Maquina) {
            return $http({
                url: Ruta.path + '/Scrap/ValidaMaquina',
                method: 'GET',
                params: {
                    Maquina: Maquina
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

        this.ListaTipoScrap = function () {
            return $http({
                url: Ruta.path + '/Scrap/TipoScrap',
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

        this.Crear = function (lineas) {
            return $http({
                url: Ruta.path + '/Scrap/CrearScrap',
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

regScrapApp.controller('regScrapController', function ($scope, Ruta, $log, Servicio, $uibModal, growl) {
    $scope.Lectura = {
        Etiqueta: ''
        , Movimiento: false
    }

    $scope.Scrap = {
        NumOrdr: ''
        , DocNum: ''
        , DocEntry: ''
        , Almacen: ''
        , Maquina: ''
        , Molde: ''
        , Fecha: ''
        , Turno: ''
        , Operador: ''
        , NomOperador: ''
        , TipoScrap: ''
        , DescScrap: ''
        , Cantidad: ''
        , Lineas: []
    }

    $scope.Memoria = {
        Nlectura: 1
        , NumOrdr: ''
        , Item: ''
        , Batch: ''
        , EstatusOF: ''
        , Maquinas: []
        , Turnos: []
        , Operadores: []
        , TiposScrap: []

        , HoraIni: ''
        , HoraFin: ''
        , ErrorTT: 0
        , msgErrorTT: ''
    }

    //************************************************************************/
    //Lista de Turnos
    $scope.Turnos = function () {
        Servicio.ListaTurnos().then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.Turnos = response.mensaje;
                    $scope.Scrap.Turno = response.turno;
                    $scope.ValidarHora();
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

    //Obtener Hora Inicial y Final del turno
    $scope.ValidarHora = function () {
        $scope.Memoria.HoraIni = "";
        $scope.Memoria.HoraFin = "";
        $scope.Memoria.ErrorTT = 0;
        $scope.Memoria.msgErrorTT = "";

        Servicio.HorasTurno("3").then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.HoraIni = response.HoraIni;
                    $scope.Memoria.HoraFin = response.HoraFin;
                    console.log("Horario turno 3 => " + $scope.Memoria.HoraIni + " - " + $scope.Memoria.HoraFin);
                    break;

                case -1:
                    $scope.Memoria.ErrorTT = -1;
                    $scope.Memoria.msgErrorTT = response.mensaje;
                    $scope.Alertas('w', response.mensaje);
                    break;

                case -2:
                    $scope.Memoria.ErrorTT = -1;
                    $scope.Memoria.msgErrorTT = response.mensaje;
                    $scope.Alertas('e', response.mensaje);
                    break;
            }
        });
    }

    //Lista de Turnos
    $scope.Operadores = function () {
        Servicio.ListaOperadores().then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.Operadores = response.mensaje;
                    $scope.Scrap.Operador = response.mensaje[0].empID;
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

    /****************************************************************/
    $scope.Cargar = function () {
        //Grabar informacion
        Servicio.GrabarLectura($scope.Scrap.NumOrdr, $scope.Memoria.Nlectura).then(function (_response) {
            console.log(_response.mensaje.length + ' --- # ' + $scope.Memoria.Nlectura);
            $scope.Memoria.Nlectura++;
        });

        console.log('OF: ' + $scope.Scrap.NumOrdr);
        if ($scope.Scrap.NumOrdr === "") {
            $scope.Alertas('w', "Ingrese Orden de Fabricación");
            $scope.Scrap.NumOrdr = "";
            return;
        }

        //Validar si es correcta la etiqueta
        //if ($scope.Lectura.Etiqueta.indexOf("&") !== -1) {
        //Verificar si la etiqueta leida es Permitida
        //if ($scope.EsPermitida($scope.Lectura.Etiqueta)) {
        //Obtener OF de la etiqueta
        //$scope.Memoria.Batch = $scope.Lectura.Etiqueta.split('&')[0];
        //$scope.Memoria.Item = $scope.Lectura.Etiqueta.split('&')[1];                
        //$scope.Memoria.NumOrdr = $scope.Lectura.Etiqueta.split('&')[3];

        //$scope.Scrap.NumOrdr === "";
        //$scope.Scrap.Molde = "";
        //$scope.Scrap.Maquina = "";
        //$scope.Memoria.EstatusOF = "";
        console.log('OF: ' + $scope.Scrap.NumOrdr);

        //if ($scope.Memoria.Batch === "" || $scope.Memoria.Item === "" || $scope.Memoria.NumOrdr === "") {
        //    $scope.Alertas('w', "La lectura fue incorrecta, vuelva a leer");
        //    $scope.Lectura.Etiqueta = '';
        //    $scope.Memoria.Batch = "";
        //    $scope.Memoria.Item = "";
        //    $scope.Memoria.NumOrdr = "";

        //    $scope.Scrap.NumOrdr = "";
        //    $scope.Scrap.DocNum = "";
        //    $scope.Scrap.DocEntry = "";
        //    $scope.Scrap.Almacen = "";
        //    $scope.Scrap.Molde = "";
        //    $scope.Scrap.Maquina = "";
        //    $scope.Memoria.EstatusOF = "";
        //    return;
        //}

        //if ($scope.Memoria.NumOrdr !== "" && $scope.Memoria.NumOrdr !== $scope.Scrap.DocNum) {
        //$scope.Scrap.NumOrdr = "";
        $scope.Scrap.DocNum = "";
        $scope.Scrap.DocEntry = "";
        $scope.Scrap.Almacen = "";
        $scope.Scrap.Molde = "";
        $scope.Scrap.Maquina = "";
        $scope.Memoria.EstatusOF = "";
        $scope.Memoria.Item = "";

        Servicio.ValidaOF($scope.Scrap.NumOrdr).then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.EstatusOF = response.mensaje[0].Status; //SOLO procesar R: Liberado/Abierta | C=Canceled, L=Closed, P=Planned, R=Released

                    if ($scope.Memoria.EstatusOF === "R") {
                        //$scope.Scrap.NumOrdr = $scope.Memoria.NumOrdr;
                        $scope.Scrap.DocNum = response.mensaje[0].DocNum;
                        $scope.Scrap.DocEntry = response.mensaje[0].DocEntry;
                        $scope.Scrap.Almacen = response.mensaje[0].Almacen;
                        $scope.Scrap.Maquina = response.mensaje[0].Maquina;
                        $scope.Scrap.Molde = response.mensaje[0].Molde;
                        $scope.Memoria.Item = response.mensaje[0].ItemCode;

                        console.log('OF OK: ' + $scope.Scrap.DocNum +
                            ' | NumOrdr: ' + $scope.Scrap.NumOrdr +
                            ' | Entry: ' + $scope.Scrap.DocEntry +
                            ' | Item: ' + $scope.Memoria.Item +
                            ' | Almacen: ' + $scope.Scrap.Almacen +
                            ' | Maquina: ' + $scope.Scrap.Maquina +
                            ' | Molde: ' + $scope.Scrap.Molde);
                    }
                    else {
                        if ($scope.Memoria.EstatusOF === "C") {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.Scrap.NumOrdr + " se encuentra Cancelada");
                        }
                        else if ($scope.Memoria.EstatusOF === "L") {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.Scrap.NumOrdr + " se encuentra Cerrada");
                        }
                        else {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.Scrap.NumOrdr + " se encuentra Planificada");
                        }

                        $scope.Memoria.NumOrdr = "";
                        $scope.Scrap.NumOrdr = "";
                        $scope.Memoria.EstatusOF = "";
                    }

                    $scope.Lectura.Etiqueta = '';
                    break;

                case -1:
                    $scope.Alertas('w', "La Orden de Fabricación " + $scope.Scrap.NumOrdr + " no existe");
                    $scope.Scrap.NumOrdr = "";
                    $scope.Memoria.EstatusOF = "";
                    $scope.Lectura.Etiqueta = '';
                    $scope.Memoria.Item = "";
                    break;

                case -2:
                    $scope.Alertas('e', "Error al validar Orden de Fabricación " + $scope.Scrap.NumOrdr + ", vuelva a intentar");
                    $scope.Scrap.NumOrdr = "";
                    $scope.Memoria.EstatusOF = "";
                    $scope.Lectura.Etiqueta = '';
                    $scope.Memoria.Item = "";
                    break;
            }
        });
        //}

        console.log('Doc: ' + $scope.Scrap.DocNum + ' | Entry: ' + $scope.Scrap.DocEntry + ' | Almacen: ' + $scope.Scrap.Almacen);
        //console.log('Lote: ' + $scope.Memoria.Batch + ' | Item: ' + $scope.Memoria.Item);
        //$scope.Lectura.Etiqueta = '';
        //$("#Entradas").focus();
        //}
        //}
        //else {
        //    $scope.Alertas('e', "La etiqueta leída no corresponde a ninguna de las permitidas");
        //    $scope.Lectura.Etiqueta = '';
        //    $("#Entradas").focus();
        //}
    }

    //Valida el tipo de Eiqueta LEIDA, esto con el 1er caracter del LOTE L000000 => L etiqueta PT, T0000 => T etiqueta traspaleo, U000 => U etiqueta traspaleo, S0000 => S etiqueta Scrap
    $scope.EsPermitida = function (etqs) {
        switch (etqs.substring(0, 1)) {
            case 'S':
            case 'L':
                return true;

            case 'T':
            case 'U':
                $scope.Alertas('e', 'La etiqueta de Traspaleo no es permitida');
                $scope.Lectura.Etiqueta = '';
                return false;

            default:
                $scope.Alertas('e', 'La etiqueta no es permitida');
                $scope.Lectura.Etiqueta = '';
                return false;
        }
    }

    /****************************************************************/
    //Listar Maquinas
    $scope.ListarMaquinas = function () {
        Servicio.ListaMaquinas().then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.Maquinas = response.mensaje;
                    console.log($scope.Memoria.Maquinas);
                    $scope.AbrirModalMaquinas($scope.Memoria.Maquinas);
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

    //Validar Maquina de forma manual
    $("#Maquina").blur(function () {
        console.log('Validar maquina: ' + $scope.Scrap.Maquina);
        if ($scope.Scrap.Maquina !== "") {
            $scope.ValMaquina($scope.Scrap.Maquina);
        }
    });

    //Proceso de validación de maquina
    $scope.ValMaquina = function (Maquina) {
        if (Maquina !== "") {
            Servicio.ValidaMaquina(Maquina).then(function (response) {
                switch (response.codigo) {
                    case 0:
                        $scope.Scrap.Maquina = response.mensaje[0].Recurso;
                        console.log('Maquina validada OK: ' + $scope.Scrap.Maquina);
                        break;

                    case -1:
                        $scope.Alertas('w', "La maquina ingresada no existe");
                        $scope.Scrap.Maquina = "";
                        break;

                    case -2:
                        $scope.Alertas('e', "Error al validar maquina ingresada, vuelva a intentar");
                        $scope.Scrap.Maquina = "";
                        break;
                }
            });
        }
    }

    /**************************************************************/
    //Listar Tipos de Scrap
    $scope.ListarTipoScrap = function () {
        Servicio.ListaTipoScrap().then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.TiposScrap = response.mensaje;
                    console.log($scope.Memoria.TiposScrap);
                    $scope.AbrirModalTScrap($scope.Memoria.TiposScrap);
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

    /*
    //Validar Tipo de Scrap de forma manual
    $("#TScrap").blur(function () {
        console.log('Validar scrap: ' + $scope.Scrap.DescScrap);
        if ($scope.Scrap.DescScrap !== "") {
            $scope.ValTScrap($scope.Scrap.DescScrap);
        }
    });

    //Proceso de validación de tipo de scrap
    $scope.ValTScrap = function (DescScrap) {
        if (DescScrap !== "") {
            Servicio.ValidaTScrap(DescScrap).then(function (response) {
                switch (response.codigo) {
                    case 0:
                        $scope.Scrap.Maquina = response.mensaje[0].Recurso;
                        console.log('Maquina validada OK: ' + $scope.Scrap.Maquina);
                        break;

                    case -1:
                        $scope.Alertas('w', "La maquina ingresada no existe");
                        $scope.Scrap.Maquina = "";
                        break;

                    case -2:
                        $scope.Alertas('e', "Error al validar maquina ingresada, vuelva a intentar");
                        $scope.Scrap.Maquina = "";
                        break;
                }
            });
        }
    }
    */

    /*
    *****************************************************************
    Validaciones
    - OF
    - Maquina
    - Operador
    - Scrap
    - Cantidad > 0
    */
    $scope.AgregarRegistro = function () {
        if ($scope.Scrap.NumOrdr === "") {
            $scope.Alertas('w', "No ha leído etiqueta para captura de OF");
            return;
        }

        if ($scope.Scrap.Fecha === "") {
            $scope.Alertas('w', "No ha seleccionado Fecha");
            return;
        }

        if ($scope.Scrap.Maquina === "") {
            $scope.Alertas('w', "No ha capturado Maquina");
            return;
        }

        if ($scope.Scrap.Operador === "") {
            $scope.Alertas('w', "No ha Seleccionado Operador");
            return;
        }

        if ($scope.Scrap.DescScrap === "") {
            $scope.Alertas('w', "No ha seleccionado Tipo Scrap");
            return;
        }

        if ($scope.Scrap.Cantidad === "" || $scope.Scrap.Cantidad === 0) {
            $scope.Alertas('w', "No ha capturado Cantidad");
            return;
        }

        /************************************************************/
        /*
        if ($scope.Scrap.Fecha === "") {
            if ($scope.Memoria.ErrorTT === -1) {
                $scope.Alertas('w', $scope.Memoria.msgErrorTT + ", corregir información y recargar módulo");
                return;
            }

            // Crear una instancia de Date
            const now = new Date();

            // Obtener la fecha y hora actual
            const year = now.getFullYear();
            const month = String(now.getMonth() + 1).padStart(2, "0"); // Los meses comienzan en 0
            const day = now.getDate();
            const hours = now.getHours();
            const minutes = now.getMinutes();
            const seconds = now.getSeconds();

            // Formatear la fecha y hora
            const formattedDate = `${day}/${month}/${year}`;
            const formattedTime = `${hours}:${minutes}:${seconds}`;

            console.log("Fecha actual:", formattedDate);
            console.log("Hora actual:", formattedTime);
            
            //Actual
            console.log("Fecha y hora actual: " + now);

            //Ayer (actual - 1 dia)
            const ayer = new Date();
            ayer.setDate(ayer.getDate() - 1)
            // Obtener la fecha y hora actual
            const year2 = ayer.getFullYear();
            const month2 = String(ayer.getMonth() + 1).padStart(2, "0"); // Los meses comienzan en 0
            const day2 = ayer.getDate();

            // Formatear la fecha y hora
            const formattedDate2 = `${day2}/${month2}/${year2}`;
            console.log("Fecha ayer: " + formattedDate2);
            
            const timeIni = $scope.Memoria.HoraIni;
            const timeFin = $scope.Memoria.HoraFin;

            //Inicio Turno
            // Dividir las cadenas y convertir a números
            const [hours1, minutes1, seconds1] = timeIni.split(":").map(Number);
            const time1 = new Date();
            time1.setHours(hours1, minutes1, seconds1); // Establecer hora, minutos y segundos
            time1.setMilliseconds(0);
            console.log("Inicio Turno 3: " + time1);

            //Fin Turno
            // Dividir las cadenas y convertir a números
            const [hours2, minutes2, seconds2] = timeFin.split(":").map(Number);
            const time2 = new Date();
            //time2.setDate(time2.getDate() + 1);
            time2.setHours(hours2, minutes2, seconds2); // Establecer hora, minutos y segundos
            time2.setMilliseconds(0);
            console.log("Fin Turno 3: " + time2);

            //Inicio Turno 1
            // Dividir las cadenas y convertir a números
            const [hours3, minutes3, seconds3] = timeFin.split(":").map(Number);
            const time3 = new Date();
            time3.setHours(hours3, minutes3, seconds3); // Establecer hora, minutos y segundos
            time3.setMilliseconds(0);
            time3.setSeconds(time3.getSeconds() + 1);
            console.log("Inicio Turno 1: " + time3);

            //Media noche
            // Dividir las cadenas y convertir a números
            const time4 = new Date();
            time4.setHours(0, 0, 0); // Establecer hora, minutos y segundos
            time4.setMilliseconds(0);
            console.log("Media noche: " + time4);

            //Pasada Media noche
            // Dividir las cadenas y convertir a números
            const time5 = new Date();
            //time5.setDate(time5.getDate() + 1);
            time5.setHours(0, 0, 1); // Establecer hora, minutos y segundos
            time5.setMilliseconds(0);
            console.log("Pasado media noche: " + time5);
            
            if (now > time5 && now < time2) {
                console.log("Hora actual es mayor a pasada media noche y menor a fin de 3er turno: Fecha es ayer => " + formattedDate2);
                $scope.Scrap.Fecha = formattedDate2;
            }
            else {
                console.log("Hora actual es menor a media noche o mayor a fin de 3er turno: Fecha de mantiene => " + formattedDate);
                $scope.Scrap.Fecha = formattedDate;
            }
        }
        */
        /************************************************************/

        //**********************************************************/
        //Obtener Nombre operador
        //**********************************************************/
        let index = -1;
        let Ls = $scope.Memoria.Operadores;
        index = Ls.findIndex(linea => linea.empID === $scope.Scrap.Operador);
        $scope.Scrap.NomOperador = $scope.Memoria.Operadores[index].Operador;
        console.log("Operador: " + $scope.Scrap.NomOperador);

        //Agregar registro
        $scope.Scrap.Lineas.push({
            "NumOrdr": $scope.Scrap.NumOrdr
            , "DocNum": $scope.Scrap.DocNum
            , "DocEntry": $scope.Scrap.DocEntry
            , "Almacen": $scope.Scrap.Almacen
            , "Maquina": $scope.Scrap.Maquina
            , "Molde": $scope.Scrap.Molde
            , "IdOperador": $scope.Scrap.Operador
            , "Operador": $scope.Scrap.NomOperador
            , "Fecha": $scope.Scrap.Fecha
            , "Turno": $scope.Scrap.Turno
            , "TipoScrap": $scope.Scrap.TipoScrap
            , "DescScrap": $scope.Scrap.DescScrap
            , "Cantidad": $scope.Scrap.Cantidad
            , "DistNumber": $scope.Memoria.Batch
            , "ItemCode": $scope.Memoria.Item
        });

        console.log('Registro agregado');
        console.log($scope.Scrap.Lineas);
        $scope.LimpiarReg();
    }

    $scope.LimpiarReg = function () {
        //$scope.Scrap.Maquina = "";
        //$scope.Scrap.Molde = "";
        $scope.Scrap.TipoScrap = "";
        $scope.Scrap.DescScrap = "";
        $scope.Scrap.Cantidad = "";
        $scope.Turnos();
        $scope.Operadores();
    }

    $scope.Eliminar = function (i) {
        $scope.Scrap.Lineas.splice(i, 1);

        if ($scope.Scrap.Lineas.length === 0) {
            $scope.Scrap.Fecha = "";
        }
    }

    //********************************************************************************************/
    //Generar Movimiento
    $scope.Crear = function () {
        console.log('Registros a procesar...');
        console.log($scope.Scrap.Lineas);
        $scope.Lectura.Movimiento = true;

        Servicio.Crear($scope.Scrap.Lineas).then(function (response) {
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

    //Recargar
    $scope.Limpiar = function () {
        $scope.Lectura = {
            Etiqueta: ''
            , Movimiento: false
        }

        $scope.Scrap = {
            NumOrdr: ''
            , DocNum: ''
            , DocEntry: ''
            , Almacen: ''
            , Maquina: ''
            , Molde: ''
            , Fecha: ''
            , Turno: ''
            , Operador: ''
            , NomOperador: ''
            , TipoScrap: ''
            , DescScrap: ''
            , Cantidad: ''
            , Lineas: []
        }

        $scope.Memoria = {
            Nlectura: 1
            , NumOrdr: ''
            , Item: ''
            , Batch: ''
            , EstatusOF: ''
            , Maquinas: []
            , Turnos: []
            , Operadores: []
            , TiposScrap: []
        }

        $scope.Turnos();
        $scope.Operadores();
    }

    //********************************************************************************************/
    $scope.AbrirModalMaquinas = function (ListMaquinas) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaMaquinas.html',
            controller: ListMaquinasController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                MaquinasList: function () {
                    return ListMaquinas;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result !== null && result !== '' && result !== undefined) {
                $scope.Scrap.Maquina = result.Recurso;
                console.log('Maquina seleccionada: ' + result.Recurso);
            }
        });
    }
    var ListMaquinasController = function (MaquinasList, $scope, $uibModalInstance, $log) {
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion !== null ? $scope.Seleccion : "");
        }

        $scope.MaqList = MaquinasList;

        $scope.MaqSeleccionado = function (val) {
            $scope.Seleccion = val;
            $scope.ok();
        }
    }

    $scope.AbrirModalTScrap = function (ListTScrap) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaTipoScrap.html',
            controller: ListTScrapController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                TScrapList: function () {
                    return ListTScrap;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result !== null && result !== '' && result !== undefined) {
                $scope.Scrap.TipoScrap = result.IdS;
                $scope.Scrap.DescScrap = result.TipoS;
                console.log('Scrap seleccionado: ' + $scope.Scrap.TipoScrap + ' | ' + $scope.Scrap.DescScrap);
            }
        });
    }
    var ListTScrapController = function (TScrapList, $scope, $uibModalInstance, $log) {
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion !== null ? $scope.Seleccion : "");
        }

        $scope.ScrapList = TScrapList;

        $scope.RegSeleccionado = function (val) {
            $scope.Seleccion = val;
            $scope.ok();
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

    //******************************************************************************/
    //Carga de turnos
    $scope.Turnos();
    //Carga de operadores
    $scope.Operadores();

});