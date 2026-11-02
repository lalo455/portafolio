var appTab = angular.module('corteCajaApp', []);

appTab.factory('Ruta', function () {
    return { path: '' }
});
appTab.service('Servicio', function ($http, Ruta, $log) {

    this.ObtenerInfoCorteCajaPorRuta = function (fecha) {
        return $http.get(Ruta.path + "/CorteCaja/ObtenerInfoCorteCajaPorRuta?fecha=" + encodeURIComponent(fecha))
            .then(function (respuesta) {
                return respuesta.data;
            })
            .catch(function (error) {
                $log.error("Error al obtener pedidos", error);
                return [];
            });
    };

    this.GuardarCorteCaja = function (model) {
        return $http({
            url: Ruta.path + '/CorteCaja/GuardarCorteCaja',
            method: 'POST',
            data: {
                model: model
            },
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -3, mensajes: "No se puede cominicar con el Servidor." }
            return error;
        });
    }

    //this.ExportarCorteCaja = function (ruta, fecha) {
    //    return $http.get(Ruta.path + "/CorteCaja/ExportarCorteCaja?ruta=" + encodeURIComponent(ruta) + "&fecha=" + encodeURIComponent(fecha))
    //        .then(function (respuesta) {
    //            return respuesta.data;
    //        })
    //        .catch(function (error) {
    //            $log.error("Error al obtener pedidos", error);
    //            return [];
    //        });
    //};
    this.ExportarCorteCaja = function (fecha) {
        return $http({
            method: "GET",
            url: Ruta.path + "/CorteCaja/ExportarCorteCaja",
            params: { fecha: fecha },
            responseType: "arraybuffer" // Necesario para manejar archivos binarios
        });
    };

});

appTab.controller('corteCajaController', function ($scope, Ruta, $log, Servicio, $timeout) {
    $scope.totales;
    $scope.entregas = [];
    $scope.fechaActual = new Date().toLocaleDateString('es-MX');
    $scope.cargando = true;
    $scope.registros = true;
    $scope.liquido = '';
    $scope.mensajeVisible = true;

    var urlParams = new URLSearchParams(window.location.search);
    $scope.fecha = urlParams.get('fecha');
    // Función para obtener la hora en formato "HH:mm hrs"
    function obtenerHora(ahora) {
        let horas = ("0" + ahora.getHours()).slice(-2); // Asegura formato 2 dígitos
        let minutos = ("0" + ahora.getMinutes()).slice(-2);
        return `${horas}:${minutos} hrs`;
    }

    // Inicializar la hora actual
    $scope.horaActual = obtenerHora(new Date());

    $scope.cargarCierreCaja = function () {
        if ($scope.fecha) {
            $scope.cargando = true;
            Servicio.ObtenerInfoCorteCajaPorRuta($scope.fecha).then(function (data) {
                if (data.Detalle === null) {
                    $scope.registros = false;
                } else {
                    $scope.totales = data.Encabezado;
                    $scope.entregas = data.Detalle; // Guardar la lista de órdenes
                    if (data.Encabezado.EsRegistrado) {
                        $scope.horaActual = data.Encabezado.Hora;
                        var f = data.Encabezado.Fecha.split('-');
                        $scope.fecha = `${f[2]}-${f[1]}-${f[0]}`;
                        $scope.liquido = data.Encabezado.Liquido;
                    }
                }
            }).catch(function (error) {
                $scope.mostrarMensaje('danger', error);
            }).finally(function () {
                $scope.cargando = false; // Ocultar el spinner al terminar
            });
        }
    };

    // Cargar automáticamente al abrir la página
    $scope.cargarCierreCaja();

    $scope.generarCorteCaja = function () {
        $scope.cargando = true;
        $scope.model = {
            Cabecera: '',
            Encabezado: {
                TotalPiezasContado: $scope.totales.TotalPiezasContado,
                TotalPiezasCredito: $scope.totales.TotalPiezasCredito,
                TotalImporteContado: $scope.totales.TotalImporteContado,
                TotalImporteCredito: $scope.totales.TotalImporteCredito
            },
            Detalle: $scope.entregas
        };

        Servicio.GuardarCorteCaja($scope.model).then(function (respuesta) {
            //$scope.cargarCierreCaja();
            $scope.totales.EsRegistrado = true;
            $scope.mostrarMensaje('success', respuesta);
        }).catch(function (error) {
            $scope.mostrarMensaje('danger', error);
        }).finally(function () {
            $scope.cargando = false; // Ocultar el spinner al terminar
        });
    };

    $scope.exportarCorteCaja = function () {
        if ($scope.fecha) {
            $scope.cargando = true;
            Servicio.ExportarCorteCaja($scope.fecha).then(function (response) {
                var blob = new Blob([response.data], { type: response.headers('Content-Type') });
                var link = document.createElement("a");
                link.href = window.URL.createObjectURL(blob);
                link.download = "CorteCaja_Diario.pdf"; // Nombre del archivo
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }).catch(function (error) {
                $scope.mostrarMensaje('danger', error);
            }).finally(function () {
                $scope.cargando = false; // Ocultar el spinner al terminar
            });
        }
    };

    $scope.mostrarMensaje = function (tipo, texto) {
        $scope.tipoMensaje = tipo; // success, danger, warning, info
        $scope.mensajeTexto = texto;
        $scope.mensajeVisible = true;


        // 🔹 Ocultar después de 3 segundos
        $timeout(function () {
            $scope.mensajeVisible = false;
        }, 3000);
    };

    $scope.cerrarMensaje = function () {
        $scope.mensajeVisible = false;
    };
});