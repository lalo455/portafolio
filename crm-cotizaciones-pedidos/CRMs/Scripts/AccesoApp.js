var accesoApp = angular.module('accesoApp', []);

accesoApp.factory('Ruta', function () {
    return { path: '' };//AQUI VA ALGO carpeta de proyecto en IIS
});

accesoApp.service('Servicio', function ($http, Ruta, $log) {
    this.Login = function (CredencialesUsuario) {
        return $http({
            url: Ruta.path + '/Login/Acceso',
            method: 'POST',
            data: {
                Credenciales: CredencialesUsuario
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -3, mensajes: "No se puede cominicar con el Servidor."}
            return error;
        });
    }
});

accesoApp.controller('accesoController', function ($scope, $window, Ruta, $log, Servicio) {
    $scope.Credenciales = {
        Usuario: '',
        Password: ''
    }

    $scope.mensajesError = {
        MensajeGeneral: '',
        MensajeUsuario: '',
        MensajePass: '',
        MostrarMensaje: false,
        Propiedad: ''
    }    

    //*************************************************************
    $scope.Navegador = function () {
        const ua = navigator.userAgent;

        const isEdge = /Edg\/|EdgA\/|EdgiOS\//.test(ua);
        const isChrome = /Chrome/.test(ua) && !isEdge;
        const isFirefox = /Firefox/.test(ua);
        const isSafari = /^((?!Chrome|Chromium|Edg).)*Safari/.test(ua);

        const elemento = document.getElementById("togglePassword");

        // Si NO es Chrome, Firefox o Safari => Ocultar con display: none
        if (!(isChrome || isFirefox || isSafari)) {
            elemento.style.display = 'none';
        }
    }

    $scope.BtnLogin = function () {        
        $scope.mensajesError.MensajeGeneral = "";
        $scope.mensajesError.MensajeUsuario = "";
        $scope.mensajesError.MensajePass = "";
        $scope.mensajesError.MostrarMensaje = false;
        $scope.mensajesError.Propiedad = "";

        Servicio.Login($scope.Credenciales).then(function (response) {
            //debugger
            if (response.codigo == -1) {

                if (response.mensajes == 'Password') {

                    $scope.mensajesError.Propiedad = response.mensajes;
                    $scope.mensajesError.MensajePass = "Ingrese la contraseña";
                } else if (response.mensajes == 'Usuario') {

                    $scope.mensajesError.Propiedad = response.mensajes;
                    $scope.mensajesError.MensajeUsuario= "Ingrese código de usuario";
                } else {

                    $scope.mensajesError.MensajeGeneral = response.mensajes;
                    $scope.mensajesError.MostrarMensaje = true;
                }
            }
            else
            {
                $window.location = response.urlRetorno;
            }
        })
    }

    $scope.Navegador();
});

//Directiva para detectar enter en input
accesoApp.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });
                event.preventDefault();
            }
        });
    };
});