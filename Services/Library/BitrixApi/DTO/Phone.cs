using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class Phone
    {
        [JsonProperty("ID")] 
		public int Id { get; set; }

        [JsonProperty("VALUE_TYPE")]
		public string ValueType { get; set; }

        [JsonProperty("VALUE")]
		public string Value { get; set; }

        [JsonProperty("TYPE_ID")]
		public string TypeId { get; set; }
    }
}