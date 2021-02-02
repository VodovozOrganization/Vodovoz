using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class CustomFileldDealItem
    {
        [DataMember(Name="result")] public CustomField Result { get; set; }
        [DataMember(Name="time")] public DTO.ResponseTime ResponseTime { get; set; }
    }
    [DataContract]
    public class CustomField
    {
        [DataMember(Name="ID")]  public int ID { get; set; }
        [DataMember(Name="FIELD_NAME")]  public string ShitName { get; set; }
        [DataMember(Name="USER_TYPE_ID")]  public string UserTypeId { get; set; }
        [DataMember(Name="EDIT_FORM_LABEL")]  public RussianFieldName Russian { get; set; }
    }
    [DataContract]
    public class RussianFieldName
    {
        [DataMember(Name="ru")]  public string Name { get; set; }
    }
}