using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VodovozSalesReceiptsService.DTO
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DocumentStatus
    {
        [EnumMember(Value = "QUEUED")]
        Queued,
        
        [EnumMember(Value = "PENDING")]
        Pending,
        
        [EnumMember(Value = "PRINTED")]
        Printed,
        
        [EnumMember(Value = "WAIT_FOR_CALLBACK")]
        WaitForCallback,
        
        [EnumMember(Value = "COMPLETED")]
        Completed,
        
        [EnumMember(Value = "FAILED")]
        Failed
    }
}