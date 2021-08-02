using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitrixApi.DTO 
{
    public class DealProductItemResponse 
	{
        [JsonProperty("result")] 
		public IList<DealProductItem> Result { get; set; }

        [JsonProperty("time")] 
		public ResponseTime ResponseTime { get; set; }
    }
}