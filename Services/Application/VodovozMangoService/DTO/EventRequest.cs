using System;
using System.Text.Json;

namespace VodovozMangoService.DTO
{
    public class EventRequest
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private CallEvent callEvent;
        public string Vpbx_Api_Key { get; set; }

        public string Sign{ get; set; }

        public string Json { get; set; }

        public CallEvent CallEvent
        {
            get
            {
                if(callEvent == null && !String.IsNullOrWhiteSpace(Json))
                    callEvent = JsonSerializer.Deserialize<CallEvent>(Json);
                return callEvent;
            }
        }

        public bool ValidateSign()
        {
            if (Vpbx_Api_Key != Program.VpbxApiKey)
            {
                logger.Error($"Сервис работает с VpbxApiKey={Program.VpbxApiKey}, а запрос пришел для VpbxApiKey={Vpbx_Api_Key}");
                return false;
            }
            
            var testSign = MangoService.MangoSignHelper.GetSign(Program.VpbxApiKey, Json, Program.VpbxApiSalt);
            return testSign == Sign;
        }
    }
}