using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitrixApi.DTO {
    public class ListofDealsRequest {
        [JsonProperty("result")] public IList<DealFromList> Result { get; set; }
        [JsonProperty("next")]  public uint? Next { get; set; }
        [JsonProperty("total")]  public uint Total { get; set; }
        [JsonProperty("time")]  public ResponseTime ResponseTime { get; set; }
    }

    public class DealFromList {
        [JsonProperty("ID")]  public uint Id { get; set; }
        // [JsonProperty("TITLE")]  public string Title { get; set; }
        // [JsonProperty("TYPE_ID")] public string TipeId { get; set; }
        // [JsonProperty("STAGE_ID")] public string StageId { get; set; }
        // [JsonProperty("CURRENCY_ID")]  public string CurrencyId { get; set; }
        // [JsonProperty("OPPORTUNITY")]  public decimal Opportunity { get; set; }
        // [JsonProperty("IS_MANUAL_OPPORTUNITY")]  public string IsManualOpportunity { get; set; }
        // [JsonProperty("TAX_VALUE")]  public decimal TaxValue { get; set; }
        // [JsonProperty("LEAD_ID")] public string LeadId { get; set; }
        // [JsonProperty("COMPANY_ID")] public uint CompanyId { get; set; }
        // [JsonProperty("CONTACT_ID")] public uint ContancId { get; set; }
        // [JsonProperty("COMMENTS")] public string Comment { get; set; }
        // [JsonProperty("QUOTE_ID")]  public string QuioteId { get; set; }
        // [JsonProperty("BEGINDATE")]  public DateTime BeginDate { get; set; }
        // // [JsonProperty("CLOSEDATE")]  public DateTime CloseDate { get; set; }
        // [JsonProperty("ASSIGNED_BY_ID")]  public int AssignedById { get; set; }
        // [JsonProperty("CREATED_BY_ID")] public int CreatedById { get; set; } 
        // [JsonProperty("MODIFY_BY_ID")]  public int ModifyById { get; set; }
        // [JsonProperty("DATE_CREATE")]  public DateTime CreateDate { get; set; }
        // [JsonProperty("DATE_MODIFY")]  public DateTime ModifyDate { get; set; }
    }
    
    
}