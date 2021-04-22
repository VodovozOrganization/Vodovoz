#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class CompanyRequest
    {
        [JsonProperty("result")] 
        public Company Result { get; set; }
        
        [JsonProperty("time")]  
        public ResponseTime ResponseTime { get; set; }
    }
}