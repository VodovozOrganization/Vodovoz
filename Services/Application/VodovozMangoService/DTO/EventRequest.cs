using System;

namespace VodovozMangoService.DTO
{
    public class EventRequest
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public string Vpbx_Api_Key { get; set; }

        public string Sign{ get; set; }

        public string Json { get; set; }

        public bool ValidateSign()
        {
            if (Vpbx_Api_Key != Program.VpbxApiKey)
            {
                logger.Error($"Сервис работает с VpbxApiKey={Program.VpbxApiKey}, а запрос пришел для VpbxApiKey={Vpbx_Api_Key}");
                return false;
            }
            
            var testSign = Mango.MangoSignHelper.GetSign(Program.VpbxApiKey, Json, Program.VpbxApiSalt);
            return testSign == Sign;
        }
    }
}