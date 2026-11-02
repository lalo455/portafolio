// Definir el módulo de AngularJS
var app = angular.module('PedidosApp', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

appTab.factory('Ruta', function () {
    return { path: '' }
});
// Controlador para la vista de pedidos
app.controller('PedidosController', function ($scope, $http) {
    $scope.pedidos = []; // Aquí se almacenarán los datos del pedido


    // Obtener los datos del pedido desde el backend
    $scope.cargarPedido = function () {
        var docNum = 59454; // Cambiar por el número de documento del pedido
        $http.post('/Pedido/ObtenerDetallePedido', { docNum: docNum })
            .then(function (response) {
                $scope.pedidos = response.data;
                console.log("Datos recibidos:", response.data); // Verifica en la consola
            })
            .catch(function (error) {
                console.error("Error al cargar el pedido:", error);
            });
    };

    // Actualizar el estado de entrega de un artículo
    $scope.actualizarEstado = function (item) {
        $http.post('/Pedidos/ActualizarEstadoEntrega', item)
            .then(function (response) {
                console.log("Estado actualizado:", response.data);
            })
            .catch(function (error) {
                console.error("Error al actualizar el estado:", error);
            });
    };

    $scope.calcularSubtotal = function () {
        return $scope.pedidos
            .filter(item => item.Entregado) // Filtra solo los artículos marcados como "Entregado"
            .reduce((total, item) => total + item.TotalLinea, 0);
    };

    $scope.calcularIVA = function () {
        return $scope.calcularSubtotal() * 0.16; // IVA del 16%
    };

    $scope.calcularTotal = function () {
        return $scope.calcularSubtotal() + $scope.calcularIVA();
    };
    $scope.mostrarConfirmacion = function () {
        Swal.fire({
            title: 'Confirmas la entrega?',
            text: "Esta accion no se puede deshacer.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#28a745', // Verde
            cancelButtonColor: '#dc3545',  // Rojo
            confirmButtonText: 'Si, entregado',
            cancelButtonText: 'No, cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                // Si el usuario confirma, generar el PDF
                $scope.generarPDF();
            } else {
                // Si el usuario cancela, no hacer nada
                Swal.fire('Cancelado', 'La entrega no fue confirmada.', 'info');
            }
        });
    };
    //pdf de prueba
    $scope.generarPDF = function () {
        // Crear un nuevo documento PDF
        const doc = new jsPDF();

        // Agregar contenido al PDF (por ejemplo, un título)
        doc.text("Detalle del Pedido Entregado", 10, 10);

        // Guardar el PDF
        doc.save("pedido_entregado.pdf");

        // Mostrar una alerta de éxito
        Swal.fire('Exito', 'El PDF se ha generado correctamente.', 'success');
    };

    // Cargar el pedido al iniciar la vista
    $scope.cargarPedido();
});