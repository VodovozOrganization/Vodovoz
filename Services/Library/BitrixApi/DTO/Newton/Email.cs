using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class Email
    {
        [JsonProperty("ID")] public int ID { get; set; }
        [JsonProperty("VALUE_TYPE")] public string VALUE_TYPE { get; set; }
        [JsonProperty("VALUE")] public string VALUE { get; set; }
        [JsonProperty("TYPE_ID")] public string TYPE_ID { get; set; }
    }
}