class PingClassJS {
    static async isValid() {
        try {
            const resp = await fetch('/api/ping-check', { cache: 'no-store' });
            if (!resp.ok) {
                this._lastError = 'Error al llamar al servicio de ping.';
                return false;
            }
            const data = await resp.json();
            this._lastError = data.error || '';
            return data.ok === true;
        } catch (e) {
            this._lastError = 'Error de red: ' + e.message;
            return false;
        }
    }

    static GetError() {
        return this._lastError || '';
    }
}
