var appTab = angular.module('pedidosRutaApp', []);

appTab.factory('Ruta', function () {
    return { path: '' }
});

appTab.service('Servicio', function ($http, Ruta, $log) {
    return {
        ObtenerOrdenesVentaPorRepartidorYRuta: function (ruta, estatus) {
            return $http.get(Ruta.path + "/Pedido/ObtenerOrdenesVentaPorRepartidorYRuta?ruta=" + encodeURIComponent(ruta) + "&estatus=" + encodeURIComponent(estatus))
                .then(function (respuesta) {
                    return respuesta.data; // Devuelve la lista de pedidos
                })
                .catch(function (error) {
                    $log.error("Error al obtener pedidos", error);
                    return [];
                });
        }
    };
});

appTab.controller('pedidosRutaController', function ($scope, Ruta, $log, Servicio, $timeout) {
    $scope.pedidos = [];
    $scope.cargando = true;

    var urlParams = new URLSearchParams(window.location.search);
    var rutaNombre = urlParams.get('nombreRuta');
    var estatus = urlParams.get('estatus') || 'O'; // Valor predeterminado 'O'

    $scope.cargarOrdenes = function () {
        $scope.pedidos = []; // **Limpia la lista antes de cargar nuevos datos**
        $scope.cargando = true;

        if (rutaNombre) {
            Servicio.ObtenerOrdenesVentaPorRepartidorYRuta(rutaNombre, estatus)
                .then(function (data) {
                    $scope.pedidos = data;
                })
                .catch(function (error) {
                    $log.error("Error al cargar:", error);
                })
                .finally(function () {
                    $scope.cargando = false;
                });
        }
    };

    // **Cargar automáticamente cuando se abre la página**
    $scope.cargarOrdenes();

    $scope.irADetalle = function (docNum) {
        var url = Ruta.path + "/Pedido/DetallePedido?docNum=" + encodeURIComponent(docNum);
        window.location.href = url;
    };

    // **Opcional: Limpiar datos cuando la página se recarga o se cierra**
    window.onbeforeunload = function () {
        $scope.pedidos = [];
    };
});
