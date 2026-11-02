using Com.Rscja.Deviceapi;
using Com.Rscja.Deviceapi.Entity;

namespace WmsMovil.Services
{
    public class RfidService : IRfidService
    {
        private RFIDWithUHFUART _uhfAPI;

        public bool Init()
        {
            try
            {
                _uhfAPI = RFIDWithUHFUART.Instance;
                return _uhfAPI.Init();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error iniciando antena: {ex.Message}");
                return false;
            }
        }

        public bool Free() => _uhfAPI?.Free() ?? false;

        public bool SetPower(int potencia) => _uhfAPI?.SetPower(potencia) ?? false;

        public int GetPower(int tries = 3)
        {
            if (_uhfAPI == null) return 0;
            return _uhfAPI.Power;
        }

        public bool StartInventoryTag(int flagAnti, int q)
        {
            if (_uhfAPI == null) return false;
            // Solución al error: Agregamos el tercer parámetro (0) que exige la DLL actual
            return _uhfAPI.StartInventoryTag(flagAnti, q, 0);
        }

        public bool StopInventory() => _uhfAPI?.StopInventory() ?? false;

        public string InventorySingleTag(bool tryGetTid = false)
        {
            if (_uhfAPI == null) return null;
            UHFTAGInfo tag = _uhfAPI.InventorySingleTag();
            return tag?.EPC;
        }

        public UHFTAGInfo ReadTagFromBuffer()
        {
            if (_uhfAPI == null) return null;
            return _uhfAPI.ReadTagFromBuffer();
        }

        public bool SetFilter(int ptr, int cnt, string data)
        {
            if (_uhfAPI == null) return false;
            // Asumiendo banco EPC (1) por defecto para el filtro
            return _uhfAPI.SetFilter(1, ptr, cnt, data);
        }

        public bool setEPCandTIDMode()
        {
            if (_uhfAPI == null) return false;
            return _uhfAPI.SetEPCAndTIDMode();
        }

        public bool isPowerOn()
        {
            return _uhfAPI != null; // Si la instancia existe, asumimos que está encendido
        }

        public async Task<UHFTAGInfo> ReadTagFromBufferAsync()
        {
            if (_uhfAPI == null) return null;
            return await Task.Run(() => _uhfAPI.ReadTagFromBuffer());
        }

        public async Task<string> ReadEpcFromBufferAsync()
        {
            var tag = await ReadTagFromBufferAsync();
            return tag?.EPC;
        }
    }
}