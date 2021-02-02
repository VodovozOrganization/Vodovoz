using System;
using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class DealRequest
    {
        [JsonProperty("result")] public Deal Result { get; set; }
        [JsonProperty("time")] public ResponseTime ResponseTime { get; set; }
    }
    
    public class Deal
    {
        
        [JsonProperty("ID")]  public int ID { get; set; }
        [JsonProperty("TITLE")]  public string Title { get; set; }
        [JsonProperty("TYPE_ID")] public string TipeId { get; set; }
        [JsonProperty("STAGE_ID")] public string StageId { get; set; }
        [JsonProperty("CURRENCY_ID")]  public string CurrencyId { get; set; }
        [JsonProperty("OPPORTUNITY")]  public decimal Opportunity { get; set; }
        [JsonProperty("IS_MANUAL_OPPORTUNITY")]  public string IsManualOpportunity { get; set; }
        [JsonProperty("TAX_VALUE")]  public decimal TaxValue { get; set; }
        [JsonProperty("LEAD_ID")] public string LeadId { get; set; }
        [JsonProperty("COMPANY_ID")] public uint CompanyId { get; set; }
        [JsonProperty("CONTACT_ID")] public uint ContancId { get; set; }
        [JsonProperty("QUOTE_ID")]  public string QuioteId { get; set; }
        [JsonProperty("BEGINDATE")]  public DateTime BegunDate { get; set; }
        [JsonProperty("CLOSEDATE")]  public DateTime CloseDate { get; set; }
        [JsonProperty("ASSIGNED_BY_ID")]  public int AssignedById { get; set; }
        [JsonProperty("CREATED_BY_ID")] public int CreatedById { get; set; } 
        [JsonProperty("MODIFY_BY_ID")]  public int ModifyById { get; set; }
        [JsonProperty("DATE_CREATE")]  public DateTime DateCreate { get; set; }
        [JsonProperty("DATE_MODIFY")]  public DateTime DateModyfy { get; set; }
        [JsonProperty("OPENED")]  public string Opened { get; set; }
        [JsonProperty("CLOSED")]  public string Closed { get; set; }
        [JsonProperty("CATEGORY_ID")]  public int CategoryId { get; set; }
        [JsonProperty("UF_CRM_1603522128")]  public string Status { get; set; }
        [JsonProperty("UF_CRM_1611649517604")]  public string Coordinates { get; set; }
        [JsonProperty("UF_CRM_5DA85CF9E13B9")]  public string DeliveryAddressWithoutHouse { get; set; }
        [JsonProperty("UF_CRM_5DA85CFA4B2FD")]  public string RoomNumber { get; set; }
        [JsonProperty("UF_CRM_5DADB4A25AFE5")]  public string HouseAndBuilding { get; set; }
        [JsonProperty("UF_CRM_1593010244990")]  public string PaymentStatus { get; set; }
    }
}