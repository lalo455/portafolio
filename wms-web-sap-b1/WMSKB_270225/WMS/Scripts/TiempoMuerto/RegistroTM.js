var regTMApp = angular.module('regTMApp', ['xeditable', 'ui.bootstrap', 'angular-growl']);

regTMApp.factory('Ruta', function () {
    return { path: '' }
});

regTMApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(15000);
    growlProvider.globalPosition('top-center');
}]);

regTMApp.run(function (editableOptions) {
    editableOptions.theme = 'bs3';
});

regTMApp.service('Servicio', function ($http, Ruta, $log) {

    this.ListaTurnos = function () {
        return $http({
            url: Ruta.path + '/TiempoMuerto/Turnos',
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
                url: Ruta.path + '/TiempoMuerto/HorasTurno',
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
                url: Ruta.path + '/TiempoMuerto/ListaOperadores',
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
                url: Ruta.path + '/TiempoMuerto/GrabarLectura',
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

        this.ListaMaquinas = function () {
            return $http({
                url: Ruta.path + '/TiempoMuerto/Maquinas',
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
                url: Ruta.path + '/TiempoMuerto/ValidaMaquina',
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

        this.ListarTipoTPM = function () {
            return $http({
                url: Ruta.path + '/TiempoMuerto/TipoTPM',
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

        this.ObtenerTPM = function (DocEntry, Maquina, Turno, Fecha) {
            return $http({
                url: Ruta.path + '/TiempoMuerto/RegistrosTPM',
                method: 'GET',
                params: {
                    DocEntry: DocEntry
                    , Maquina: Maquina
                    , Turno: Turno
                    , Fecha: Fecha
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

        this.Crear = function (lineas) {
            return $http({
                url: Ruta.path + '/TiempoMuerto/CrearTPM',
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

regTMApp.controller('regTMController', function ($scope, Ruta, $log, Servicio, $uibModal, growl) {
    $scope.Lectura = {
        Etiqueta: ''
        , Movimiento: false
    }

    $scope.TPM = {
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
        , TipoTPM: ''
        , DescTPM: ''
        , Tiempo: ''
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
        , TiposTPM: []
        , ListaTPM: []

        , Categoria: ''
        , Correo: ''

        , HoraIni: ''
        , HoraFin: ''
        , ErrorTT: 0
        , msgErrorTT: ''
    }

    //************************************************************************/
    //Lista de Turnos
    $scope.Turnos = function () {
        $scope.Memoria.Turnos = [];
        Servicio.ListaTurnos().then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.Turnos = response.mensaje;
                    $scope.TPM.Turno = response.turno;
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
        $scope.Memoria.Operadores = [];
        Servicio.ListaOperadores().then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.Operadores = response.mensaje;
                    $scope.TPM.Operador = response.mensaje[0].empID;
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
        Servicio.GrabarLectura($scope.TPM.NumOrdr, $scope.Memoria.Nlectura).then(function (_response) {
            console.log(_response.mensaje.length + ' --- # ' + $scope.Memoria.Nlectura);
            $scope.Memoria.Nlectura++;
        });

        console.log('OF: ' + $scope.TPM.NumOrdr);
        if ($scope.TPM.NumOrdr === "") {
            $scope.Alertas('w', "Ingrese Orden de Fabricación");
            $scope.TPM.NumOrdr = "";
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

        //$scope.TPM.NumOrdr === "";
        //$scope.TPM.Molde = "";
        //$scope.TPM.Maquina = "";
        //$scope.Memoria.EstatusOF = "";
        console.log('OF: ' + $scope.TPM.NumOrdr);

        //if ($scope.Memoria.Batch === "" || $scope.Memoria.Item === "" || $scope.Memoria.NumOrdr === "") {
        //    $scope.Alertas('w', "La lectura fue incorrecta, vuelva a leer");
        //    $scope.Lectura.Etiqueta = '';
        //    $scope.Memoria.Batch = "";
        //    $scope.Memoria.Item = "";
        //    $scope.Memoria.NumOrdr = "";

        //    $scope.TPM.NumOrdr = "";
        //    $scope.TPM.DocNum = "";
        //    $scope.TPM.DocEntry = "";
        //    $scope.TPM.Almacen = "";
        //    $scope.TPM.Molde = "";
        //    $scope.TPM.Maquina = "";
        //    $scope.Memoria.EstatusOF = "";
        //    return;
        //}

        //if ($scope.Memoria.NumOrdr !== "" && $scope.Memoria.NumOrdr !== $scope.TPM.DocNum) {
        //$scope.TPM.NumOrdr = "";
        $scope.TPM.DocNum = "";
        $scope.TPM.DocEntry = "";
        $scope.TPM.Almacen = "";
        $scope.TPM.Molde = "";
        $scope.TPM.Maquina = "";
        $scope.Memoria.EstatusOF = "";
        $scope.Memoria.Item = "";

        Servicio.ValidaOF($scope.TPM.NumOrdr).then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.EstatusOF = response.mensaje[0].Status; //SOLO procesar R: Liberado/Abierta | C=Canceled, L=Closed, P=Planned, R=Released

                    if ($scope.Memoria.EstatusOF === "R") {
                        //$scope.TPM.NumOrdr = $scope.Memoria.NumOrdr;
                        $scope.TPM.DocNum = response.mensaje[0].DocNum;
                        $scope.TPM.DocEntry = response.mensaje[0].DocEntry;
                        $scope.TPM.Almacen = response.mensaje[0].Almacen;
                        $scope.TPM.Maquina = response.mensaje[0].Maquina;
                        $scope.TPM.Molde = response.mensaje[0].Molde;
                        $scope.Memoria.Item = response.mensaje[0].ItemCode;

                        console.log('OF OK: ' + $scope.TPM.DocNum +
                            ' | NumOrdr: ' + $scope.TPM.NumOrdr +
                            ' | Entry: ' + $scope.TPM.DocEntry +
                            ' | Item: ' + $scope.Memoria.Item +
                            ' | Almacen: ' + $scope.TPM.Almacen +
                            ' | Maquina: ' + $scope.TPM.Maquina +
                            ' | Molde: ' + $scope.TPM.Molde);
                    }
                    else {
                        if ($scope.Memoria.EstatusOF === "C") {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " se encuentra Cancelada");
                        }
                        else if ($scope.Memoria.EstatusOF === "L") {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " se encuentra Cerrada");
                        }
                        else {
                            $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " se encuentra Planificada");
                        }

                        $scope.Memoria.NumOrdr = "";
                        $scope.TPM.NumOrdr = "";
                        $scope.Memoria.EstatusOF = "";
                    }

                    $scope.Lectura.Etiqueta = '';
                    break;

                case -1:
                    $scope.Alertas('w', "La Orden de Fabricación " + $scope.TPM.NumOrdr + " no existe");
                    $scope.TPM.NumOrdr = "";
                    $scope.Memoria.EstatusOF = "";
                    $scope.Lectura.Etiqueta = '';
                    $scope.Memoria.Item = "";
                    break;

                case -2:
                    $scope.Alertas('e', "Error al validar Orden de Fabricación " + $scope.TPM.NumOrdr + ", vuelva a intentar");
                    $scope.TPM.NumOrdr = "";
                    $scope.Memoria.EstatusOF = "";
                    $scope.Lectura.Etiqueta = '';
                    $scope.Memoria.Item = "";
                    break;
            }
        });
        //}

        console.log('Doc: ' + $scope.TPM.DocNum + ' | Entry: ' + $scope.TPM.DocEntry + ' | Almacen: ' + $scope.TPM.Almacen);
        //console.log('Lote: ' + $scope.Memoria.Batch + ' | Item: ' + $scope.Memoria.Item);
        //$scope.Lectura.Etiqueta = '';
        //}
        //}
        //else {
        //    $scope.Alertas('e', "La etiqueta leída no corresponde a ninguna de las permitidas");
        //    $scope.Lectura.Etiqueta = '';
        //    $("#Entradas").focus();
        //}
    }

    //Valida el tipo de Eiqueta LEIDA, esto con el 1er caracter del LOTE L000000 => L etiqueta PT, T0000 => T etiqueta traspaleo, U000 => U etiqueta traspaleo, S0000 => S etiqueta TPM
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
        $scope.Memoria.Maquinas = [];
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
        console.log('Validar maquina: ' + $scope.TPM.Maquina);
        if ($scope.TPM.Maquina !== "") {
            $scope.ValMaquina($scope.TPM.Maquina);
        }
    });

    //Proceso de validación de maquina
    $scope.ValMaquina = function (Maquina) {
        if (Maquina !== "") {
            Servicio.ValidaMaquina(Maquina).then(function (response) {
                switch (response.codigo) {
                    case 0:
                        $scope.TPM.Maquina = response.mensaje[0].Recurso;
                        console.log('Maquina validada OK: ' + $scope.TPM.Maquina);
                        break;

                    case -1:
                        $scope.Alertas('w', "La maquina ingresada no existe");
                        $scope.TPM.Maquina = "";
                        break;

                    case -2:
                        $scope.Alertas('e', "Error al validar maquina ingresada, vuelva a intentar");
                        $scope.TPM.Maquina = "";
                        break;
                }
            });
        }
    }

    /**************************************************************/
    //Listar Tipos de TPM
    $scope.ListarTipoTPM = function () {
        $scope.Memoria.TiposTPM = [];
        Servicio.ListarTipoTPM().then(function (response) {
            switch (response.codigo) {
                case 0:
                    $scope.Memoria.TiposTPM = response.mensaje;
                    console.log($scope.Memoria.TiposTPM);
                    $scope.AbrirModalTPM($scope.Memoria.TiposTPM);
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
    *****************************************************************
    Validaciones
    - OF
    - Maquina
    - Operador
    - TPM
    - Tiempo > 0
    */
    $scope.AgregarRegistro = function () {
        if ($scope.TPM.NumOrdr === "") {
            $scope.Alertas('w', "No ha leído ingresado OF");
            return;
        }

        if ($scope.TPM.Fecha === "") {
            $scope.Alertas('w', "No ha seleccionado Fecha");
            return;
        }

        if ($scope.TPM.Maquina === "") {
            $scope.Alertas('w', "No ha capturado Maquina");
            return;
        }

        if ($scope.TPM.Operador === "") {
            $scope.Alertas('w', "No ha Seleccionado Operador");
            return;
        }

        if ($scope.TPM.DescTPM === "") {
            $scope.Alertas('w', "No ha seleccionado Tipo TPM");
            return;
        }

        if ($scope.TPM.Tiempo === "" || $scope.TPM.Tiempo === 0) {
            $scope.Alertas('w', "No ha capturado Tiempo");
            return;
        }

        /************************************************************/
        /*
        if ($scope.Memoria.ErrorTT === -1) {
            $scope.Alertas('w', $scope.Memoria.msgErrorTT + ", corregir información y recargar módulo");
            return;
        }
        */
        //********************************************/
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
        //********************************************/
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
        //********************************************/
        /*
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
        */
        //********************************************/
        //*************************************************/
        /*
        if (now > time5 && now < time2) {
            console.log("Hora actual es mayor a pasada media noche y menor a fin de 3er turno: Fecha es ayer => " + formattedDate2);
            $scope.TPM.Fecha = formattedDate2;
        }
        else {
            console.log("Hora actual es menor a media noche o mayor a fin de 3er turno: Fecha de mantiene => " + formattedDate);
            $scope.TPM.Fecha = formattedDate;
        }
        */
        /************************************************************/

        //**********************************************************/
        //Validar TPM | 1 solo código por OF, Maquina, Turno, Fecha
        //**********************************************************/
        let idx = -1;
        let Lt = $scope.TPM.Lineas;
        idx = Lt.findIndex(linea => linea.TipoTPM === $scope.TPM.TipoTPM && linea.NumOrdr === $scope.TPM.NumOrdr && linea.Maquina === $scope.TPM.Maquina && linea.Turno === $scope.TPM.Turno);
        if (idx > -1) {
            $scope.Alertas('w', "Ya se ha capturado el TPM " + $scope.TPM.DescTPM);
            proceder = false;
            return;
        }

        //Validar con registros existes en BD
        $scope.Memoria.ListaTPM = [];

        //Obtener TPM de la OF, Maquina, Turno, Fecha | Validar TPM a registrar
        Servicio.ObtenerTPM($scope.TPM.DocEntry, $scope.TPM.Maquina, $scope.TPM.Turno, $scope.TPM.Fecha).then(function (response) {
            if (response.codigo === 0) {
                $scope.Memoria.ListaTPM = response.mensaje;
                console.log($scope.Memoria.ListaTPM);
                console.log('TPM: ' + $scope.TPM.TipoTPM);

                let idx = -1;
                let Lt = $scope.Memoria.ListaTPM;
                idx = Lt.findIndex(linea => linea.TipoTPM === $scope.TPM.TipoTPM);

                if (idx > -1) {
                    //Hay registros | tpm ya existe | No se puede registrar
                    $scope.Alertas('w', "Ya existe el TPM " + $scope.TPM.DescTPM + " registrado en sistema anteriormente");
                    $scope.LimpiarReg();
                }
                else {
                    //Hay registros | tpm no existe | se puede registrar
                    //**********************************************************/
                    //Obtener Nombre operador
                    //**********************************************************/
                    let index = -1;
                    let Ls = $scope.Memoria.Operadores;
                    index = Ls.findIndex(linea => linea.empID === $scope.TPM.Operador);
                    $scope.TPM.NomOperador = $scope.Memoria.Operadores[index].Operador;
                    console.log("Operador: " + $scope.TPM.NomOperador);

                    //Agregar registro
                    $scope.TPM.Lineas.push({
                        "NumOrdr": $scope.TPM.NumOrdr
                        , "DocNum": $scope.TPM.DocNum
                        , "DocEntry": $scope.TPM.DocEntry
                        , "Almacen": $scope.TPM.Almacen
                        , "Maquina": $scope.TPM.Maquina
                        , "Molde": $scope.TPM.Molde
                        , "IdOperador": $scope.TPM.Operador
                        , "Operador": $scope.TPM.NomOperador
                        , "Fecha": $scope.TPM.Fecha
                        , "Turno": $scope.TPM.Turno
                        , "TipoTPM": $scope.TPM.TipoTPM
                        , "DescTPM": $scope.TPM.DescTPM
                        , "Tiempo": $scope.TPM.Tiempo
                        , "DistNumber": $scope.Memoria.Batch
                        , "ItemCode": $scope.Memoria.Item

                        , "Categoria": $scope.Memoria.Categoria
                        , "Correo": $scope.Memoria.Correo
                    });

                    console.log('Registro agregado');
                    console.log($scope.TPM.Lineas);
                    $scope.LimpiarReg();
                }
            }
            else if (response.codigo === -1) {
                //**********************************************************/
                //Obtener Nombre operador
                //**********************************************************/
                let index = -1;
                let Ls = $scope.Memoria.Operadores;
                index = Ls.findIndex(linea => linea.empID === $scope.TPM.Operador);
                $scope.TPM.NomOperador = $scope.Memoria.Operadores[index].Operador;
                console.log("Operador: " + $scope.TPM.NomOperador);

                //Agregar registro
                $scope.TPM.Lineas.push({
                    "NumOrdr": $scope.TPM.NumOrdr
                    , "DocNum": $scope.TPM.DocNum
                    , "DocEntry": $scope.TPM.DocEntry
                    , "Almacen": $scope.TPM.Almacen
                    , "Maquina": $scope.TPM.Maquina
                    , "Molde": $scope.TPM.Molde
                    , "IdOperador": $scope.TPM.Operador
                    , "Operador": $scope.TPM.NomOperador
                    , "Fecha": $scope.TPM.Fecha
                    , "Turno": $scope.TPM.Turno
                    , "TipoTPM": $scope.TPM.TipoTPM
                    , "DescTPM": $scope.TPM.DescTPM
                    , "Tiempo": $scope.TPM.Tiempo
                    , "DistNumber": $scope.Memoria.Batch
                    , "ItemCode": $scope.Memoria.Item

                    , "Categoria": $scope.Memoria.Categoria
                    , "Correo": $scope.Memoria.Correo
                });

                console.log('Registro agregado');
                console.log($scope.TPM.Lineas);
                $scope.LimpiarReg();
            }
            else {
                $scope.Alertas('e', 'Error al obtener registros previos de la OF, Maquina y Turno de hoy, revisar y volver a intentar');
                $scope.LimpiarReg();
            }
        });
    }

    $scope.LimpiarReg = function () {
        $scope.TPM.TipoTPM = "";
        $scope.TPM.DescTPM = "";
        $scope.TPM.Tiempo = "";
        //$scope.Turnos();
        //$scope.Operadores();
    }

    $scope.Eliminar = function (i) {
        $scope.TPM.Lineas.splice(i, 1);

        if ($scope.TPM.Lineas.length === 0) {
            $scope.TPM.Fecha = "";
        }
    }

    //********************************************************************************************/
    //Generar Movimiento
    $scope.Crear = function () {
        console.log('Registros a procesar...');
        console.log($scope.TPM.Lineas);
        $scope.Lectura.Movimiento = true;

        Servicio.Crear($scope.TPM.Lineas).then(function (response) {
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

        $scope.TPM = {
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
            , TipoTPM: ''
            , DescTPM: ''
            , Tiempo: ''
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
            , TiposTPM: []
            , ListaTPM: []

            , Categoria: ''
            , Correo: ''
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
                $scope.TPM.Maquina = result.Recurso;
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

    $scope.AbrirModalTPM = function (ListTPM) {
        var modalInstance = $uibModal.open({
            templateUrl: Ruta.path + '/Modales/ListaTipoTPM.html',
            controller: ListTPMController,
            keyboard: false,
            backdrop: "static",
            resolve: {
                TPMList: function () {
                    return ListTPM;
                }
            },
            windowClass: 'show',
            backdropClass: 'show'
        }).result.then(function (result) {
            if (result !== null && result !== '' && result !== undefined) {
                $scope.TPM.TipoTPM = result.IdS;
                $scope.TPM.DescTPM = result.TipoS;

                $scope.Memoria.Categoria = result.Categoria;
                $scope.Memoria.Correo = result.Correo;

                console.log('TPM seleccionado: ' + $scope.TPM.TipoTPM + ' | ' + $scope.TPM.DescTPM + ' |' + $scope.Memoria.Categoria + ' | ' + $scope.Memoria.Correo);
            }
        });
    }
    var ListTPMController = function (TPMList, $scope, $uibModalInstance, $log) {
        $scope.ok = function () {
            $uibModalInstance.close($scope.Seleccion !== null ? $scope.Seleccion : "");
        }

        $scope.TPMList = TPMList;

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
    //Carga de Operadores
    $scope.Operadores();

});