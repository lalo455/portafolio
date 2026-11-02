var appTab = angular.module('rutasAsignadasApp', []);

appTab.factory('Ruta', function () {
    return { path: '' }
});
appTab.service('Servicio', function ($http, Ruta, $log) {

    return {
        ObtenerRutasAsignadasPorRepartidor: function (id) {
            return $http.get(Ruta.path + "/Rutas/ObtenerRutasAsignadasPorRepartidor?estatus=" + encodeURIComponent(id))
                .then(function (respuesta) {
                    return respuesta.data; // Devuelve la lista de pedidos
                })
                .catch(function (error) {
                    $log.error("Error al obtener rutas", error);
                    return [];
                });
        }
    };
});

appTab.controller('rutasAsignadasController', function ($scope, Ruta, $log, Servicio) {
    $scope.rutas = [];
    $scope.cargando = true;
    $scope.fecha = new Date(); // Fecha inicial
    var fechaFormateada = '';

    let hoy = new Date();
    let year = hoy.getFullYear();
    let month = ("0" + (hoy.getMonth() + 1)).slice(-2);
    let day = ("0" + hoy.getDate()).slice(-2);
    $scope.hoy = `${day}-${month}-${year}`;

    $scope.cambiarFecha = function () {
        console.log($scope.fecha);
        if ($scope.fecha !== null && $scope.fecha !== undefined) {
            fechaFormateada = formatFecha($scope.fecha);
        } else {

        }
    };

    var urlParams = new URLSearchParams(window.location.search);
    var estatus = urlParams.get('estatus');

    $scope.cargarRutas = function () {
        $scope.cargando = true; // Mostrar el spinner mientras carga

        Servicio.ObtenerRutasAsignadasPorRepartidor(estatus).then(function (data) {
            $scope.rutas = data;
        }).catch(function (error) {
            $log.error("Error al cargar:", error);
        }).finally(function () {
            $scope.cargando = false; // Ocultar el spinner al terminar
        });
    };

    // 🔹 Cargar rutas automáticamente cuando se abre la página
    $scope.cargarRutas();

    $scope.irARuta = function (rutaNombre) {
        if (estatus === 'O') {
            var url = "/Pedido/RutasPedidos?nombreRuta=" + encodeURIComponent(rutaNombre) +
                "&estatus=" + encodeURIComponent(estatus);
            window.location.href = url;
        } else {
            if (fechaFormateada === '')
                fechaFormateada = $scope.hoy;
            var url = "/CorteCaja/CorteCaja?nombreRuta=" + encodeURIComponent(rutaNombre) +
                "&estatus=" + encodeURIComponent(estatus) + "&fecha=" + encodeURIComponent(fechaFormateada);
            window.location.href = url;
        }
    };

    function formatFecha(fecha) {
        let day = ("0" + fecha.getDate()).slice(-2);
        let month = ("0" + (fecha.getMonth() + 1)).slice(-2);
        let year = fecha.getFullYear();
        return `${day}-${month}-${year}`;
    }
});