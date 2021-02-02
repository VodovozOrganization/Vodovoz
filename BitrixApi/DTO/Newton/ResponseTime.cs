using System;
using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class ResponseTime
    {
        [JsonProperty("date_start")] public DateTime DateStart { get; set; }
        [JsonProperty("date_finish")] public DateTime DateFinish { get; set; }
        [JsonProperty("duration")] public double Duration { get; set; }
        [JsonProperty("processing")] public double Processing { get; set; }
    }
}