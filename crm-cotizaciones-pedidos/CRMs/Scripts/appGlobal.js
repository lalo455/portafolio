angular.module('services.core', ['angular-growl']) // Módulo global reutilizable
    .factory('Ruta', function () {
        return { path: '' };
    })
    .factory('RCtrl', function () {
        return { path: '' }
    })
    .config(['growlProvider', function (growlProvider) {
        growlProvider.globalTimeToLive(15000);
        growlProvider.globalPosition('top-center');
    }])
    .service('ServicesGlobal', ['growl', function (growl) {
        this.mostrarAlerta = function (tipo, texto) {
            if (!growl) {
                console.error("Growl no está definido");
                return;
            }

            const opciones = { title: 'Aviso de sistema' };

            switch (tipo) {
                case 'w': growl.warning(texto, opciones); break;
                case 'i': growl.info(texto, { title: 'Información' }); break;
                case 's': growl.success(texto, { title: '- Completado -' }); break;
                case 'e': growl.error(texto, { title: '¡¡Error!!' }); break;
                default: console.warn("Tipo de alerta no válido:", tipo);
            }
        },
        this.codigoGuia = function () {
            var today = new Date();
            var dd = String(today.getDate()).padStart(2, '0');
            var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
            var yyyy = today.getFullYear();
            var curHour = today.getHours(); // > 12 ? today.getHours() - 12 : (today.getHours() < 10 ? "0" + today.getHours() : today.getHours());
            var curMinute = today.getMinutes(); // < 10 ? "0" + today.getMinutes() : today.getMinutes();
            var curSeconds = today.getSeconds(); // < 10 ? "0" + today.getSeconds() : today.getSeconds();
            var curMiliSec = today.getMilliseconds();

            const codeGuia = dd + mm + yyyy + curHour + curMinute + curSeconds + curMiliSec;
            return codeGuia;
        };
    }]);