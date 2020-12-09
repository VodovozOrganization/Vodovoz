using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VodovozSalesReceiptsService.DTO
{
    public class SalesDocumentsStatusDTO
    {
        [JsonProperty("fiscalInfo")] 
        public FiscalInfo FiscalInfo { get; set; }

        [JsonProperty("fnState")] 
        public string FnState { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")] 
        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentStatus Status { get; set; }
    }
    
    public class FiscalInfo
    {
        [JsonProperty("checkNumber")] 
        public int CheckNumber { get; set; }

        [JsonProperty("checkType")] 
        public string CheckType { get; set; }

        [JsonProperty("date")] 
        public string Date { get; set; }

        [JsonProperty("fnDocMark")] 
        public int FnDocMark { get; set; }

        [JsonProperty("fnDocNumber")] 
        public int FnDocNumber { get; set; }

        [JsonProperty("fnNumber")] 
        public string FnNumber { get; set; }

        [JsonProperty("kktNumber")] 
        public string KktNumber { get; set; }
        
        [JsonProperty("qr")] 
        public string Qr { get; set; }

        [JsonProperty("shiftNumber")] 
        public int ShiftNumber { get; set; }

        [JsonProperty("sum")]
        public double Sum { get; set; }
    }
}