var menu_submenuApp = angular.module('menu_submenuApp', []);

menu_submenuApp.factory('Ruta', function () {
    return { path: '' }
});

menu_submenuApp.factory('RCtrl', function () {
    return { path: '' }
});

menu_submenuApp.service('Servicio', function ($http, Ruta, $log) {
    this.CargarModulos = function () {
        return $http({
            url: Ruta.path + '/Menu/Modulos',
            method: 'GET',
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            }
        }).then(function (response) {
            return response.data;
        }, function (response) {
            var error = { codigo: -1, mensaje: { "Error en Servicio 1 ": response } }
            return error;
        });
    }
});

menu_submenuApp.controller('menu_submenuController', function ($scope, Servicio, $log, Ruta, RCtrl) {
    $scope.ListaMenuPrincipal = [
        {   //0
            Vista: 'TrsManual',
            Controlador: RCtrl.path + 'TraspasoManual',
            NombreMenu: 'Traspaso Manual',
            imagen: Ruta.path + '/Assets/Traspasos.png',
            color: 'mr-2 menu-yellow',
            id: 'Traspasos',
            Activo: ''
        },
        {   //1
            Vista: 'Remision',
            Controlador: RCtrl.path +'Remision',
            NombreMenu: 'Remisión',
            imagen: Ruta.path +'/Assets/Remision.png',
            color: 'mr-2 menu-green',
            id: 'Remision',
            Activo: ''
        },
        {   //2
            Vista: 'Produccion',
            Controlador: RCtrl.path +'Produccion',
            NombreMenu: 'Env. Producción',
            imagen: Ruta.path +'/Assets/Env_Produccion.png',
            color: 'mr-2 mt-2 menu-red',
            id: 'Env_Produccion',
            Activo: ''
        },
        {   //3
            Vista: 'ReciboProduccion',
            Controlador: RCtrl.path +'Produccion',
            NombreMenu: 'Validacion Prod',
            imagen: Ruta.path +'/Assets/Validacion_Prod.png',
            color: 'mr-2 mt-2 menu-grey',
            id: 'Validacion_Prod',
            Activo: ''
        },
        {   //4
            Vista: 'RecuentoInv',
            Controlador: RCtrl.path +'Recuento',
            NombreMenu: 'Recuento Inv',
            imagen: Ruta.path +'/Assets/Recuento_Inv.png',
            color: 'mr-2 mt-2 menu-magenta',
            id:'Recuento_Inv',
            Activo: ''
        },
        {   //5
            Vista: 'ValidacionEmbarque',
            Controlador: RCtrl.path +'Embarque',
            NombreMenu: 'Val de Embarque',
            imagen: Ruta.path +'/Assets/Val_de_Embarque.png',
            color: 'mr-2 mt-2 menu-aqua',
            id:'Val_de_Embarque',
            Activo: ''
        },
        {   //6
            Vista: 'Calidad',
            Controlador: RCtrl.path + 'Calidad',
            NombreMenu: 'Calidad',
            imagen: Ruta.path + '/Assets/Calidad.png',
            color: 'mr-2 menu-sea',
            id: 'Calidad',
            Activo: ''
        },
        {   //7
            Vista: 'Traspaleo',
            Controlador: RCtrl.path +'Traspaleo',
            NombreMenu: 'Traspaleo',
            imagen: Ruta.path +'/Assets/Traspaleo.png',
            color: 'mr-2 mt-2 menu-sunset',
            id:'Traspaleo',
            Activo: ''
        },
        {   //8
            Vista: 'EmisionReciboProduccion',
            Controlador: RCtrl.path + 'Produccion',
            NombreMenu: 'Recibo Prod',
            imagen: Ruta.path + '/Assets/Recibo_Prod.png',
            color: 'mr-2 mt-2 menu-blue',
            id: 'Recibo_Prod',
            Activo: ''
        }, 
        {   //9
            Vista: 'EtiquetaMaster',
            Controlador: RCtrl.path + 'EtiquetaMaster',
            NombreMenu: 'Etq. Master',
            imagen: Ruta.path + '/Assets/EtqMaster.png',
            color: 'mr-2 menu-yellow',
            id: 'Etq_Master',
            Activo: ''
        },
        {   //10
            Vista: 'Consulta',
            Controlador: RCtrl.path + 'Consulta',
            NombreMenu: 'Consulta Art.',
            imagen: Ruta.path + '/Assets/Consulta.png',
            color: 'mr-2 menu-glacier',
            id: 'Consulta',
            Activo: ''
        },
        {   //11
            Vista: 'SubMenuScrap',
            Controlador: RCtrl.path + 'Menu',
            NombreMenu: 'Scrap',
            imagen: Ruta.path + '/Assets/Scrap.png',
            color: 'mr-2 mt-2 menu-red',
            id: 'Scrap',
            Activo: ''
        },
        {   //12
            Vista: 'SubMenuTM',
            Controlador: RCtrl.path + 'Menu',
            NombreMenu: 'Tiempo Muerto',
            imagen: Ruta.path + '/Assets/TiempoMuerto.png',
            color: 'mr-2 mt-2 menu-red',
            id: 'TiempoMuerto',
            Activo: ''
        }
    ];

    $scope.ListaMenuScrap = [
        {
            Vista: 'RegistroScrap',
            Controlador: RCtrl.path + 'Scrap',
            NombreMenu: 'Registrar',
            imagen: Ruta.path + '/Assets/RegistroScrap.png',
            color: 'mr-2 menu-red'
        },
        {
            Vista: 'EdicionScrap',
            Controlador: RCtrl.path + 'Scrap',
            NombreMenu: 'Editar',
            imagen: Ruta.path + '/Assets/EditarScrap.png',
            color: 'mr-2 menu-marmalade'
        }
    ];

    $scope.ListaMenuTM = [
        {
            Vista: 'RegistroTM',
            Controlador: RCtrl.path + 'TiempoMuerto',
            NombreMenu: 'Registrar TPM',
            imagen: Ruta.path + '/Assets/RegistroTM.png',
            color: 'mr-2 menu-red'
        },
        {
            Vista: 'EdicionTM',
            Controlador: RCtrl.path + 'TiempoMuerto',
            NombreMenu: 'Editar TPM',
            imagen: Ruta.path + '/Assets/EditarTM.png',
            color: 'mr-2 menu-marmalade'
        }
    ];

    $scope.Cargar = function () {
        Servicio.CargarModulos().then(function (response) {
            if (response.codigo == 0) {
                for (var m = 0; m < $scope.ListaMenuPrincipal.length; m++) {
                    $scope.ListaMenuPrincipal[0].Activo = response.mensaje[0].Traspasos;
                    $scope.ListaMenuPrincipal[1].Activo = response.mensaje[0].Remision;
                    $scope.ListaMenuPrincipal[2].Activo = response.mensaje[0].Env_Produccion;
                    $scope.ListaMenuPrincipal[3].Activo = response.mensaje[0].Validacion_Prod;
                    $scope.ListaMenuPrincipal[4].Activo = response.mensaje[0].Recuento_Inv;
                    $scope.ListaMenuPrincipal[5].Activo = 'N'; // response.mensaje[0].Val_de_Embarque;
                    $scope.ListaMenuPrincipal[6].Activo = response.mensaje[0].Calidad;
                    $scope.ListaMenuPrincipal[7].Activo = response.mensaje[0].Traspaleo;
                    $scope.ListaMenuPrincipal[8].Activo = response.mensaje[0].Recibo_Prod;
                    $scope.ListaMenuPrincipal[9].Activo = response.mensaje[0].Etq_Master;
                    $scope.ListaMenuPrincipal[10].Activo = response.mensaje[0].Consulta; 
                    $scope.ListaMenuPrincipal[11].Activo = response.mensaje[0].Scrap;
                    $scope.ListaMenuPrincipal[12].Activo = response.mensaje[0].TiempoMuerto;
                }
            }
        });
    }

    $scope.Cargar();
});