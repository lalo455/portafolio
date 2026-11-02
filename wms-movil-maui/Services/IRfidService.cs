using Com.Rscja.Deviceapi.Entity;

namespace WmsMovil.Services
{
    public interface IRfidService
    {
        int GetPower(int tries = 3);
        bool StartInventoryTag(int flagAnti, int q);
        bool SetPower(int potencia);
        bool Init();
        bool Free();
        string InventorySingleTag(bool tryGetTid = false);
        bool StopInventory();
        UHFTAGInfo ReadTagFromBuffer();
        bool SetFilter(int ptr, int cnt, string data);
        bool setEPCandTIDMode();
        bool isPowerOn();
        Task<UHFTAGInfo> ReadTagFromBufferAsync();
        Task<string> ReadEpcFromBufferAsync();
    }
}