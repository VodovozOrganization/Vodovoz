using Newtonsoft.Json;

namespace OnlineStoreImportService
{
    [JsonObject]
    public class Product
    {
        [JsonProperty(PropertyName = "ID")]
        public string Id { get; set; }
        
        [JsonProperty(PropertyName = "NAME")]
        public string Name { get; set; }
        
        [JsonProperty(PropertyName = "SECTION_ID")]
        public string GroupId { get; set; }
        
        [JsonProperty(PropertyName = "PRICE")]
        public decimal Price { get; set; }
        
        [JsonProperty(PropertyName = "QUANTITY")]
        public string Quantity { get; set; }
        
        [JsonProperty(PropertyName = "PHOTO")]
        public string Photo { get; set; }
        
        [JsonProperty(PropertyName = "MANUFACTURER")]
        public string Manufacturer { get; set; }
        
        [JsonProperty(PropertyName = "WEIGHT")]
        public string Weight { get; set; }
    }
}