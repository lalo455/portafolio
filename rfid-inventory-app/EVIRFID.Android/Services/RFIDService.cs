using EVIRFID.Droid.Services;
using EVIRFID.Services;
using Xamarin.Forms;
using Com.Rscja.Deviceapi;
using Com.Rscja.Deviceapi.Entity;
using System.Threading.Tasks;
using System;

[assembly: Dependency(typeof(RfidService))]
namespace EVIRFID.Droid.Services
{
    public class RfidService : IRfidService
    {
        bool loopFlag = false;
        // emptyTag es el objeto que contiene todos losd atos de la etiqueta como el EPC
        UHFTAGInfo emptyTag = new UHFTAGInfo(); //Tag

        private readonly RFIDWithUHFUART uhfApi = RFIDWithUHFUART.Instance; //Reader

        /// <summary>
        /// Initialize UHF module.
        /// </summary>
        /// <remarks>
        /// Switches on the device before operating the device.
        /// </remarks>
        public bool Init()
        {
#if NO_RFID
            return true;
#else
            
            return uhfApi.Init();
            
#endif
        }

        /// <summary>
        /// Sets the reader to also report back the TID memory block
        /// </summary>
        /// <remarks>
        /// Puede ralentizar la lectura de tags que NO sean Impinj
        /// </remarks>
        public bool setEPCandTIDMode()
        {
#if NO_RFID
            return true;
#else
            return uhfApi.SetEPCAndTIDMode();
#endif
        }


        /// <summary>
        /// Switch off UHF module
        /// </summary>
        /// <remarks>
        /// Switch off device after using.
        /// </remarks>
        public bool Free()
        {
#if NO_RFID
            return true;
#else
            return uhfApi.Free();
#endif
        }

        /// <summary>
        /// Gets the current RF power setting from the reader
        /// </summary>
        /// <remarks>
        /// Goes from 5 to 30 dBm
        /// </remarks>

        public int GetPower(int tries = 3)
        {
            int iPower = uhfApi.Power;
            if (iPower > -1)
            {
                return iPower;
            }
            else
            {
                if(tries > 0)
                {
                    tries--;
                    return GetPower(tries);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Sets the current RF power setting from the reader
        /// </summary>
        /// <remarks>
        /// Goes from 5 to 30 dBm
        /// </remarks>
        public bool SetPower(int potencia)
        {
            return uhfApi.SetPower(potencia);
        }

        /// <summary>
        /// Start the identification tag cycle and then the identified tag number to the buffer.
        /// </summary>
        /// <remarks>
        /// Used for reading one tag data from buffer zone, after starting circular identification, the module will respond to formula only.
        ///
        /// TODO docs recommend calling readUidFromBuffer() instead. 
        /// </remarks>
        /// <param name="flagAnti">Use anti-collision identification function or not，default 0</param>
        /// <param name="q">Initial Q value of anti-collision process, it will be valid if flagAnti is 1.</param>
        /// <returns>Activate is successful or not</returns>
        public bool StartInventoryTag(int flagAnti, int q, int session)
        {
#if NO_RFID
            return true;
#else
            return uhfApi.StartInventoryTag(flagAnti, q, session);
#endif
        }

        /// <summary>
        /// Stop circular identification
        /// </summary>
        public bool StopInventory()
        {
#if NO_RFID
            return true;
#else
            //Stop loopflag??
            return uhfApi.StopInventory();
#endif
        }

        /// <summary>
        /// Read the return tag UII in buffer zone.
        /// </summary>
        public UHFTAGInfo ReadTagFromBuffer()
        {
#if NO_RFID
            return new string[] { string.Empty };
# else
            return uhfApi.ReadTagFromBuffer();
            //UHFTAGInfo empty = new UHFTAGInfo();
            //empty.EPC = "";

            //UHFTAGInfo res = uhfApi.ReadTagFromBuffer();

            //if(res != null)
            //{
            //    return (res);
            //}
            //else
            //{
            //    return empty;
            //}
#endif
        }

        public async Task<UHFTAGInfo> ReadTagFromBufferAsync()
        {
            return await Task.Run(() => uhfApi.ReadTagFromBuffer());           
        }
        public async Task<string> ReadEpcFromBufferAsync()
        {
            return await Task.Run(() =>
            {
                var tag = uhfApi.ReadTagFromBuffer();
                return tag?.EPC;
            });
        }

        /// <summary>
        /// Setup scanning filter.
        /// </summary>
        /// <param name="ptr">Filter start address</param>
        /// <param name="cnt">Filter data length</param>
        /// <param name="data">Filter data</param>
        /// <returns>True is success, false is failed</returns>

        [System.Obsolete]
        public bool SetFilter(int ptr, int cnt, string data)
        {
#if NO_RFID
            return new string[] { string.Empty };
# else
            return uhfApi.SetFilter(Com.Rscja.Deviceapi.Interfaces.UHFConsts.BankEPC, ptr, cnt, data);
#endif
        }

        /// <summary>
        /// This formula identify tag in single step, return EPC / EPC+TID for only one time.
        /// </summary>
        /// <returns>
        /// Return EPC / EPC/TID, null is identified tag failure.
        /// </returns>
        public string InventorySingleTag(bool tryGetTid = false)
        {
#if NO_RFID
            return "X9999ASDFZZZZZ";
#else
            //UHFTAGInfo tag = uhfApi.InventorySingleTag();
            if (tryGetTid)
            {
                return uhfApi.ReadData("00000000", 1, 2, 6);
                //return (string)tag.EPC + "," + (string)tag.Tid;
            }
            else
            {
                return uhfApi.ReadData("00000000", 1, 2, 6);
                //return (string)tag.EPC;
            }
#endif
        }

        public bool isPowerOn()
        {
            return uhfApi.IsPowerOn;
        }
    }
}