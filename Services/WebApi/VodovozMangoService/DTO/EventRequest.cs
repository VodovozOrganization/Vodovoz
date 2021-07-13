using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

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

        public bool ValidateSign(IConfiguration configuration)
        {
            if (Vpbx_Api_Key != configuration["Mango:vpbx_api_key"])
            {
                logger.Error($"Сервис работает с VpbxApiKey={configuration["Mango:vpbx_api_key"]}, а запрос пришел для VpbxApiKey={Vpbx_Api_Key}");
                return false;
            }
            
            var testSign = MangoService.MangoSignHelper.GetSign(configuration["Mango:vpbx_api_key"], Json, configuration["Mango:vpbx_api_salt"]);
            return testSign == Sign;
        }
    }
}