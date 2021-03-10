using Newtonsoft.Json;

namespace BitrixApi.DTO {
    public class ChangeStatusResult {
        [JsonProperty("result")] public bool Result { get; set; }
        [JsonProperty("time")]  public ResponseTime ResponseTime { get; set; }

    }
}