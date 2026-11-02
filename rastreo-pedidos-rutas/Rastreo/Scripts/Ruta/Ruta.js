var appTab = angular.module('Ruta', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

appTab.factory('Ruta', function () {
    return { path: '' }
});

appTab.service('Servicio', function ($http, Ruta, $log) {

    return {

        //ObtenerRuta: function () {
        //    return $http.post(Ruta.path + "/LogicaPedido/ObtenerRuta")
        //        .then(function (respuesta) {
        //            return respuesta.data; // Devuelve la lista de rutas
        //        })
        //        .catch(function (error) {
        //            $log.error("Error al obtener las rutas", error);
        //            return [];
        //        });

        //},
        //ObtenerRepartidores: function () {
        //    return $http.post(Ruta.path + "/LogicaPedido/ObtenerRepartidor").then(function (respuestaDefault) {
        //        return respuestaDefault.data;
        //    }, function (error) {
        //        var data = {
        //            codigo: -1, mensaje: { "Error": "Error al obtener los repartidores" }
        //        }
        //    })
        //},
        //VerificarRuta: function () {
        //    return $http.post(Ruta.path + "/LogicaPedido/VerificarRuta").then(function (respuestaDefault) {
        //        return respuestaDefault.data;
        //    }, function (error) {
        //        var data = {
        //            codigo: -1, mensaje: { "Error": "Error al obtener las rutas" }
        //        }
        //    })
        //},
        ObtenerRutasYRepartidores: function () {
            return $http.post(Ruta.path + "/LogicaPedido/ObtenerRutasYRepartidores")
                .then(function (respuesta) {
                    return respuesta.data; // Devuelve la lista de rutas y repartidores
                })
                .catch(function (error) {
                    $log.error("Error al obtener las rutas y repartidores", error);
                    return [];
                });
        },
        GuardarRutas: function (cambios) {
            return $http.post(Ruta.path + "/LogicaPedido/GuardarRutas", cambios)
                .then(function (respuesta) {
                    return respuesta.data;
                })
                .catch(function (error) {
                    $log.error("Error al guardar las rutas", error);
                    throw error;
                });
        }
    };
});
appTab.controller('TableController', function ($scope, Servicio) {
    // Lista de rutas (columnas)
    $scope.routes = [];

    // Lista de repartidores (filas)
    $scope.deliveries = [];

    // Función para cargar las rutas y repartidores
    $scope.cargarRutasYRepartidores = function () {
        Servicio.ObtenerRutasYRepartidores().then(function (data) {
            // Llenar las columnas de rutas (omitir duplicados basados en el nombre de la ruta)
            const rutasUnicas = [];
            const rutasMap = {};

            data.forEach(item => {
                if (item.RutaName && !rutasMap[item.RutaName]) {  // Evitar valores vacíos y duplicados
                    rutasMap[item.RutaName] = true;  // Marcar la ruta como agregada
                    rutasUnicas.push({
                        code: item.RutaCode,  // Conservar el código de la ruta
                        name: item.RutaName   // Usar el nombre de la ruta para evitar duplicados
                    });
                }
            });

            $scope.routes = rutasUnicas;

            // Llenar las filas de repartidores (omitir duplicados)
            const repartidoresMap = {};
            data.forEach(item => {
                if (!repartidoresMap[item.RepartidorCode]) {

                    repartidoresMap[item.RepartidorCode] = {
                        code: item.RepartidorCode,  // Conservar el código del repartidor
                        name: item.RepartidorName,  // Mostrar el nombre del repartidor (U_SlpName)
                        routes: {}  // Inicializar routes vacío
                    };
                }


                if (item.RepartidorCode === repartidoresMap[item.RepartidorCode].code) {
                    // Interpretar Y como true (activo) y N como false (inactivo)
                    repartidoresMap[item.RepartidorCode].routes[item.RutaName] = item.Estatus === "Y";
                }
            });


            $scope.deliveries = Object.values(repartidoresMap);

            console.log("Rutas cargadas:", $scope.routes);
            console.log("Repartidores cargados:", $scope.deliveries);
        });
    };


    $scope.toggleRoute = function (index, routeName) {

        $scope.deliveries[index].routes[routeName] = !$scope.deliveries[index].routes[routeName];


        console.log("Estado actualizado:", $scope.deliveries[index].routes[routeName]);


    };
    $scope.guardarCambios = function () {
        const cambios = [];

        $scope.deliveries.forEach(delivery => {
            for (const routeName in delivery.routes) {
                cambios.push({
                    RepartidorCode: delivery.code,
                    RutaName: routeName,
                    Estatus: delivery.routes[routeName] ? "Y" : "N"
                });
            }
        });

        Servicio.GuardarRutas(cambios)
            .then(function (respuesta) {
                if (respuesta.success) {
                    mostrarAlerta("Exito", respuesta.message, "success");
                } else {
                    mostrarAlerta("Error", respuesta.message, "error");
                }
            })
            .catch(function (error) {
                mostrarAlerta("Error", "Error al guardar los cambios: " + error.data.message, "error");
            });
    };

    function mostrarAlerta(titulo, mensaje, tipo) {
        const alerta = document.createElement("div");
        alerta.className = `alert alert-${tipo} fixed-top text-center`;
        alerta.style.marginTop = "20px";
        alerta.style.zIndex = "1000";
        alerta.innerHTML = `<strong>${titulo}:</strong> ${mensaje}`;

        document.body.appendChild(alerta);

        setTimeout(() => {
            alerta.remove();
        }, 3000);
    }

    // Llamar la función al iniciar
    $scope.cargarRutasYRepartidores();

});
