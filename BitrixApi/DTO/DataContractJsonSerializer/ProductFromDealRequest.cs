using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BitrixApi.DTO.DataContractJsonSerializer {
    
    [DataContract]
    public class ProductFromDealRequest {
        [DataMember(Name="result")] public IList<ProductFromDeal> Result { get; set; }
        [DataMember(Name="time")] public ResponseTime ResponseTime { get; set; }
    }
    
    [DataContract]
    public class ProductFromDeal {
        [DataMember(Name="ID")]  public int Id { get; set; }
        [DataMember(Name="OWNER_ID")]  public string OWNER_ID { get; set; }
        [DataMember(Name="OWNER_TYPE")] public string OWNER_TYPE { get; set; } 
        [DataMember(Name="PRODUCT_ID_X")] public int PRODUCT_ID { get; set; }
        [DataMember(Name="PRODUCT_NAME")] public string PRODUCT_NAME { get; set; }
        [DataMember(Name="PRODUCT_DESCRIPTION")] public string PRODUCT_DESCRIPTION { get; set; }
        [DataMember(Name="PRICE")] public decimal PRICE { get; set; }
        [DataMember(Name="PRICE_EXCLUSIVE")] public decimal PRICE_EXCLUSIVE { get; set; }
        [DataMember(Name="PRICE_NETTO")] public decimal PRICE_NETTO { get; set; }
        [DataMember(Name="SECTION_ID")] public string PRICE_ACCOUNT { get; set; }
        
        [DataMember(Name="QUANTITY")] public int QUANTITY { get; set; }
        [DataMember(Name="DISCOUNT_TYPE_ID")] public int DISCOUNT_TYPE_ID { get; set; }
        [DataMember(Name="DISCOUNT_RATE")] public int DISCOUNT_RATE { get; set; }
        [DataMember(Name="DISCOUNT_SUM")] public int DISCOUNT_SUM { get; set; }
        [DataMember(Name="TAX_RATE")] public int TAX_RATE { get; set; }
        [DataMember(Name="TAX_INCLUDED")] public string TAX_INCLUDED { get; set; } // Bool
        [DataMember(Name="CUSTOMIZED")] public string CUSTOMIZED { get; set; }
        [DataMember(Name="MEASURE_CODE")] public string MEASURE_CODE { get; set; }
        [DataMember(Name="MEASURE_NAME")] public string MEASURE_NAME { get; set; } // шт
    }
}