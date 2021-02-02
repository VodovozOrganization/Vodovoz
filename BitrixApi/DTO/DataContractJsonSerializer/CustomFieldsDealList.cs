using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class CustomFieldsDealList
    {
        [DataMember(Name="result")] public IList<CustomFieldFromList> Result { get; set; }
        [DataMember(Name="time")] public DTO.ResponseTime ResponseTime { get; set; }
    }
    
    [DataContract]
    public class CustomFieldFromList
    {
        [DataMember(Name="ID")]  public int ID { get; set; }
        [DataMember(Name="FIELD_NAME")]  public string ShitName { get; set; }
        [DataMember(Name="USER_TYPE_ID")]  public string UserTypeId { get; set; }
    }
}