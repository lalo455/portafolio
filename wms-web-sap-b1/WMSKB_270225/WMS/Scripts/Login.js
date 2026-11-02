var accesoApp = angular.module('accesoApp', ['angular-growl']);

accesoApp.config(['growlProvider', function (growlProvider) {
    growlProvider.globalTimeToLive(5000);
    growlProvider.globalPosition('top-center');
}]);

accesoApp.factory('Ruta', function () {
    return { path: '' }
});

accesoApp.service('Servicio', function ($http, Ruta, $log) {
    this.Login = function (Credenciales) {
        return $http({
            url: Ruta.path + '/Login/Ingresar',
            method: 'POST',
            data: {
                Usuario: Credenciales
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio de Login ": response } }
            return error;
        });
    }
});

accesoApp.controller('accesoController', function ($scope, $window, Ruta, $log, Servicio, growl) {
    $scope.Credenciales = {
        Usuario: '',
        Password: ''
    }
    $scope.Lectura = {
        Movimiento: false
    }

    $scope.Ingresar = function ()
    {
        $scope.Lectura.Movimiento = true;
        $("#btnLogin").css({'display': 'none'});

        Servicio.Login($scope.Credenciales).then(function (response)
        {
            if (response.codigo === "0") //Acceso Exitoso 
            {
                $window.location = response.mensaje;
                $scope.Lectura.Movimiento = false;
                $("#btnLogin").css({'display': 'block'});
            }
            else //Error en Entrada a WMS
            {
                $scope.Alertas('e', response.mensaje);
                $scope.Lectura.Movimiento = false;
                $("#btnLogin").css({'display': 'block'});
            }
        });
    }

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