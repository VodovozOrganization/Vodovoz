using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bitrix.DTO 
{
    public class DealsResponse 
	{
        [JsonProperty("result")] 
		public IList<Deal> Result { get; set; }

        [JsonProperty("next")]  
		public int? Next { get; set; }

        [JsonProperty("total")]  
		public int Total { get; set; }

        [JsonProperty("time")] 
		public ResponseTime ResponseTime { get; set; }
    }
}