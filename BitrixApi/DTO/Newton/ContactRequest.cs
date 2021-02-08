#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class ContactRequest
    {
        [JsonProperty("result")]public Contact Result { get; set; }
        [JsonProperty("time")] public ResponseTime ResponseTime { get; set; }
    }
    public class Contact
    {

        [JsonProperty("ID")] public uint Id { get; set; }
        [JsonProperty("COMMENTS")] public string? COMMENTS { get; set; }
        [JsonProperty("NAME")] public string? NAME { get; set; }
        [JsonProperty("SECOND_NAME")] public string? SECOND_NAME { get; set; }
        [JsonProperty("LAST_NAME")] public string? LAST_NAME { get; set; }
        [JsonProperty("LEAD_ID")] public string LEAD_ID { get; set; }
        [JsonProperty("TYPE_ID")] public string TYPE_ID { get; set; }
        [JsonProperty("SOURCE_ID")] public string SOURCE_ID { get; set; }
        [JsonProperty("COMPANY_ID")] public string COMPANY_ID { get; set; }
        [JsonProperty("ASSIGNED_BY_ID")] public int ASSIGNED_BY_ID { get; set; }
        [JsonProperty("CREATED_BY_ID")] public int CREATED_BY_ID { get; set; }
        [JsonProperty("MODIFY_BY_ID")] public int MODIFY_BY_ID { get; set; }
        [JsonProperty("OPENED")] public string OPENED { get; set; }
        
        [JsonProperty("PHONE")] public IList<Phone> PHONE { get; set; }
        [JsonProperty("EMAIL")] public IList<Email> EMAIL { get; set; }
    }
}