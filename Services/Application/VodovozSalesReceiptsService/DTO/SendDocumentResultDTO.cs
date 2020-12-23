using Newtonsoft.Json;

namespace VodovozSalesReceiptsService.DTO
{
    public class SendDocumentResultDTO
    {
        [JsonProperty("timestamp")]
        public string TimeStampString { get; set; }
        
        [JsonProperty("status")] 
        public int StatusCode { get; set; }
        
        [JsonProperty("error")] 
        public string ErrorStatusCodeName { get; set; }
        
        [JsonProperty("message")] 
        public string Message { get; set; }

        [JsonProperty("code")] 
        public string InternalCode { get; set; }
        
        [JsonProperty("path")]
        public string AddressPath { get; set; }
    }
}