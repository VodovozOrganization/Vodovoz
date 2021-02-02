using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class CustomFileldDealItem
    {
        [JsonProperty("result")] public CustomField Result { get; set; }
        [JsonProperty("time")] public ResponseTime ResponseTime { get; set; }
    }
    
    public class CustomField
    {
        [JsonProperty("ID")]  public int ID { get; set; }
        [JsonProperty("FIELD_NAME")]  public string ShitName { get; set; }
        [JsonProperty("USER_TYPE_ID")]  public string UserTypeId { get; set; }
        [JsonProperty("EDIT_FORM_LABEL")]  public RussianFieldName Russian { get; set; }
    }

    public class RussianFieldName
    {
        [JsonProperty("ru")]  public string Name { get; set; }
    }
}