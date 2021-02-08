using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class ProductRequest
    {
        [DataMember(Name="result")]public Product Result { get; set; }
        [DataMember(Name="time")] public DTO.ResponseTime ResponseTime { get; set; }
    }
    
    [DataContract]
    public class Product
    {
        [DataMember(Name="ID")]  public int Id { get; set; }
        [DataMember(Name="NAME")]  public string NAME { get; set; }
        [DataMember(Name="ACTIVE")] public string ACTIVE { get; set; } //TODO gavr в bool
        [DataMember(Name="TIMESTAMP_X")] public DateTime TIMESTAMP_X { get; set; }
        [DataMember(Name="DATE_CREATE")] public DateTime DATE_CREATE { get; set; }
        [DataMember(Name="MODIFIED_BY")] public int MODIFIED_BY { get; set; }
        [DataMember(Name="CREATED_BY")] public int CREATED_BY { get; set; }
        [DataMember(Name="CATALOG_ID")] public int CATALOG_ID { get; set; }
        [DataMember(Name="SECTION_ID")] public int SECTION_ID { get; set; }
        [DataMember(Name="PRICE")] public decimal PRICE { get; set; }
        [DataMember(Name="CURRENCY_ID")] public string CURRENCY_ID { get; set; }
        [DataMember(Name="VAT_INCLUDED")] public string VAT_INCLUDED { get; set; } //TODO gavr в bool
        [DataMember(Name="MEASURE")] public int MEASURE { get; set; } //TODO gavr узнать че ето у товара, может быть в битриксе есть таблица соответствий мер типа грам это id 9
        [DataMember(Name="PROPERTY_174")] public ProductCategory CategoryObj { get; set; }
    }
    
    [DataContract]
    public class ProductCategory {
        [DataMember(Name="valueId")] public string valueId { get; set; }
        [DataMember(Name="value")] public string Category { get; set; }
    }
}