var app = angular.module('PedidosApp', ['ui.bootstrap', 'ui.utils', 'xeditable', 'angularUtils.directives.dirPagination', 'angular-growl']);

app.controller('PedidosController', function ($scope, $http, $timeout) {
    $scope.pedidos = []; // Aquí se almacenarán los datos del pedido
    $scope.deshabilitarBoton = true; // Inicialmente deshabilitado

    // Obtener el DocNum desde la URL
    var urlParams = new URLSearchParams(window.location.search);
    var docNum = urlParams.get('docNum');

    if (!docNum) {
        console.error("No se proporcionó un número de documento (DocNum).");
        return;
    }

    // Obtener la ruta base (incluyendo el nombre de la aplicación)
    var baseUrl = window.location.origin /*+ "/entregasPruebas";*/;
    // 🔹 Función para limpiar los arreglos y variables al actualizar la página
    function limpiarDatos() {
        $scope.pedidos = [];
        $scope.deshabilitarBoton = true;
        $scope.procesando = false;
    }

    // 🔹 Evento que se ejecuta al recargar la página
    window.addEventListener("beforeunload", function () {
        limpiarDatos();
    });
    // Función para verificar si hay artículos marcados
    $scope.verificarArticulosMarcados = function () {
        if ($scope.pedidos.length === 0) {
            // No hay artículos cargados
            $scope.deshabilitarBoton = true;
        } else {
            // Verificar si al menos un artículo está marcado como "Entregado"
            const hayArticulosMarcados = $scope.pedidos.some(item => item.Entregado);
            $scope.deshabilitarBoton = !hayArticulosMarcados;
        }
        console.log("Deshabilitar botón:", $scope.deshabilitarBoton); // Depuración
    };

    // Obtener los datos del pedido desde el backend
    $scope.cargarPedido = function () {
        $http.post(baseUrl + '/Pedido/ObtenerDetallePedido', { docNum: docNum })
            .then(function (response) {
                // FORZAR REFRESCO DE VISTA CON $timeout
                $timeout(function () {
                    $scope.pedidos = response.data.map(item => {
                        item.CantidadEditada = item.CantidadPendiente;
                        return item;
                    });
                    $scope.verificarArticulosMarcados();
                }, 0);
            })
            .catch(function (error) {
                console.error("Error al cargar el pedido:", error);
                $scope.deshabilitarBoton = true;
            });
    };

    // Actualizar el estado de entrega de un artículo
    $scope.actualizarEstado = function (item) {
        $http.post(baseUrl + '/Pedido/ActualizarEstadoEntrega', item)
            .then(function (response) {
                console.log("Estado actualizado:", response.data);
                $scope.verificarArticulosMarcados(); // Verificar artículos marcados después de actualizar
            })
            .catch(function (error) {
                console.error("Error al actualizar el estado:", error);
            });
    };

    $scope.validarCantidad = function (item) {
        if (item.CantidadEditada < 0 || item.CantidadEditada > item.CantidadPendiente) {
            item.CantidadEditada = item.CantidadPendiente; // Restablecer a la cantidad pendiente
            Swal.fire('Error', 'La cantidad no puede ser menor que 0 ni mayor que la cantidad pendiente.', 'error');
        }
    };

    $scope.calcularSubtotal = function () {
        return $scope.pedidos
            .filter(item => item.Entregado) // Filtra solo los artículos marcados como "Entregado"
            .reduce((total, item) => total + (item.CantidadEditada * item.Precio), 0);
    };

    $scope.calcularIVA = function () {
        return $scope.calcularSubtotal() * 0.16; // Suponiendo un IVA del 16%
    };

    $scope.calcularTotal = function () {
        return $scope.calcularSubtotal() + $scope.calcularIVA();
    };

    // Mostrar confirmación antes de generar el PDF
    $scope.procesando = false;

    $scope.mostrarConfirmacion = function () {
        Swal.fire({
            title: 'Confirmas la entrega?',
            text: "Esta acción no se puede deshacer.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#28a745', // Verde
            cancelButtonColor: '#dc3545',  // Rojo
            confirmButtonText: 'Si, entregado',
            cancelButtonText: 'No, cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                // Validar que ninguna CantidadEditada sea 0
                const hayCantidadesInvalidas = $scope.pedidos.some(item => item.Entregado && item.CantidadEditada <= 0);

                if (hayCantidadesInvalidas) {
                    Swal.fire('Error', 'La cantidad no puede ser menor o igual a 0.', 'error');
                    return;
                }
                console.log($scope.pedidos);
                $scope.procesando = true; // Activar animación

                $http.post(baseUrl + '/Pedido/EntregarPedido', {
                    docEntryPedido: $scope.pedidos[0].DocEntry,
                    detalles: $scope.pedidos
                })
                    .then(function (response) {
                        $scope.procesando = false;

                        if (response.data.success) {
                            Swal.fire('Éxito', 'El pedido se entregó correctamente.', 'success');

                            // FORZAR REFRESCO DE VISTA CON $timeout
                            $timeout(function () {
                                // Actualizar CantidadPendiente en el frontend
                                $scope.pedidos.forEach(item => {
                                    if (item.Entregado) {
                                        item.CantidadPendiente -= item.CantidadEditada;
                                        // Opcional: Desmarcar el check después de entregar
                                        item.Entregado = false;
                                    }
                                });
                                $scope.verificarArticulosMarcados();
                            }, 0);

                            // Abrir el PDF en una nueva ventana
                            if (response.data.pdfUrl) {
                                window.open(response.data.pdfUrl, '_blank');
                            }
                        } else {
                            Swal.fire('Error', response.data.message, 'error');
                        }
                    })
                    .catch(function (error) {
                        $scope.procesando = false; // Desactivar animación
                        Swal.fire('Error', 'Ocurrió un error al procesar la entrega.', 'error');
                    });
            }
        });
    };
    // Generar el PDF de confirmación de entrega
    $scope.generarPDF = function () {
        // Mostrar un mensaje de carga
        Swal.fire({
            title: 'Generando PDF...',
            text: 'Por favor, espera un momento.',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        // Generar el PDF
        const doc = new jsPDF();
        doc.text("Detalle del Pedido Entregado", 10, 10);

        // Abrir el PDF en una nueva pestaña
        const pdfData = doc.output('datauristring'); // Genera el PDF como una cadena de datos
        const newWindow = window.open(); // Abre una nueva pestaña
        newWindow.document.write(`<iframe width='100%' height='100%' src='${pdfData}'></iframe>`); // Muestra el PDF en la nueva pestaña

        // Cerrar el mensaje de carga
        Swal.close();
        Swal.fire('Éxito', 'El PDF se ha generado correctamente.', 'success');
    };
   


    // Cargar el pedido al iniciar la vista
    $scope.cargarPedido();
});