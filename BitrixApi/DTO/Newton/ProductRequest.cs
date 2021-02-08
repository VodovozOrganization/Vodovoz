using System;
using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class ProductRequest
    {
        [JsonProperty("result")] public Product Result { get; set; }
        [JsonProperty("time")] public ResponseTime ResponseTime { get; set; }
    }
    
    public class Product
    {
        [JsonProperty("ID")]  public int Id { get; set; }
        [JsonProperty("NAME")]  public string NAME { get; set; }
        [JsonProperty("ACTIVE")] public string ACTIVE { get; set; } //TODO gavr в bool
        [JsonProperty("TIMESTAMP_X")] public DateTime TIMESTAMP_X { get; set; }
        [JsonProperty("DATE_CREATE")] public DateTime DATE_CREATE { get; set; }
        [JsonProperty("MODIFIED_BY")] public int MODIFIED_BY { get; set; }
        [JsonProperty("CREATED_BY")] public int CREATED_BY { get; set; }
        [JsonProperty("CATALOG_ID")] public int CATALOG_ID { get; set; }
        [JsonProperty("SECTION_ID")] public int SECTION_ID { get; set; }
        [JsonProperty("PRICE")] public decimal PRICE { get; set; }
        [JsonProperty("CURRENCY_ID")] public string CURRENCY_ID { get; set; }
        [JsonProperty("VAT_INCLUDED")] public string VAT_INCLUDED { get; set; } //TODO gavr в bool
        [JsonProperty("MEASURE")] public int MEASURE { get; set; } //TODO gavr узнать че ето у товара, может быть в битриксе есть таблица соответствий мер типа грам это id 9
        [JsonProperty("PROPERTY_174")] public ProductCategory CategoryObj { get; set; }
    }

    public class ProductCategory {
        [JsonProperty("valueId")] public string valueId { get; set; }
        [JsonProperty("value")] public string Category { get; set; }
    }
}