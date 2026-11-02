var appTab = angular.module('corteCajaAdminApp', []);

appTab.factory('Ruta', function () {
    return { path: '' } // Asegúrate de configurar esto si la aplicación no corre en la raíz
});

appTab.service('ServicioAdmin', function ($http, Ruta, $log) {
    this.ObtenerRepartidores = function () {
        return $http.get(Ruta.path + "/CorteCaja/ObtenerRepartidores")
            .then(function (respuesta) {
                return respuesta.data;
            })
            .catch(function (error) {
                $log.error("Error al obtener repartidores", error);
                return [];
            });
    };

    this.ExportarCorteCajaAdmin = function (fecha, idRepartidor) {
        return $http({
            method: "GET",
            url: Ruta.path + "/CorteCaja/ExportarCorteCajaAdmin",
            params: { fecha: fecha, idRepartidor: idRepartidor },
            responseType: "arraybuffer" // Necesario para manejar archivos binarios
        });
    };
});

appTab.controller('corteCajaAdminController', function ($scope, Ruta, $log, ServicioAdmin, $timeout) {
    $scope.repartidores = [];
    $scope.repartidorSeleccionado = "";
    $scope.fecha = new Date(); // Fecha actual por defecto
    $scope.cargando = false;
    $scope.mensajeVisible = false;

    $scope.cargarRepartidores = function () {
        ServicioAdmin.ObtenerRepartidores().then(function (data) {
            $scope.repartidores = data;
        }).catch(function (error) {
            $scope.mostrarMensaje('danger', 'Error al cargar repartidores.');
        });
    };

    // Inicializar cargando repartidores
    $scope.cargarRepartidores();

    $scope.generarReporte = function () {
        if ($scope.fecha && $scope.repartidorSeleccionado) {
            $scope.cargando = true;
            
            // Formatear fecha a dd-MM-yyyy para el backend
            let dia = ("0" + $scope.fecha.getDate()).slice(-2);
            let mes = ("0" + ($scope.fecha.getMonth() + 1)).slice(-2);
            let anio = $scope.fecha.getFullYear();
            let fechaFormateada = `${dia}-${mes}-${anio}`;

            ServicioAdmin.ExportarCorteCajaAdmin(fechaFormateada, $scope.repartidorSeleccionado).then(function (response) {
                var blob = new Blob([response.data], { type: response.headers('Content-Type') });
                var link = document.createElement("a");
                link.href = window.URL.createObjectURL(blob);
                link.download = "CorteCajaAdmin_" + $scope.repartidorSeleccionado + "_" + fechaFormateada + ".pdf";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }).catch(function (error) {
                $scope.mostrarMensaje('danger', 'Error al generar el reporte.');
            }).finally(function () {
                $scope.cargando = false;
            });
        }
    };

    $scope.mostrarMensaje = function (tipo, texto) {
        $scope.tipoMensaje = tipo; // success, danger, warning, info
        $scope.mensajeTexto = texto;
        $scope.mensajeVisible = true;

        $timeout(function () {
            $scope.mensajeVisible = false;
        }, 3000);
    };

    $scope.cerrarMensaje = function () {
        $scope.mensajeVisible = false;
    };
});
