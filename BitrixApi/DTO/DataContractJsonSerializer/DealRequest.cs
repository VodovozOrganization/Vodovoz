using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class DealRequest
    {
        [DataMember(Name="result")] public Deal Result { get; set; }
        [DataMember(Name="time")] public DTO.ResponseTime ResponseTime { get; set; }
    }
    
    [DataContract]
    public class Deal
    {
        
        [DataMember(Name="ID")]  public uint Id { get; set; }
        [DataMember(Name="TITLE")]  public string Title { get; set; }
        [DataMember(Name="TYPE_ID")] public string TipeId { get; set; }
        [DataMember(Name="STAGE_ID")] public string StageId { get; set; }
        [DataMember(Name="CURRENCY_ID")]  public string CurrencyId { get; set; }
        [DataMember(Name="OPPORTUNITY")]  public decimal Opportunity { get; set; }
        [DataMember(Name="IS_MANUAL_OPPORTUNITY")]  public string IsManualOpportunity { get; set; }
        [DataMember(Name="TAX_VALUE")]  public decimal TaxValue { get; set; }
        [DataMember(Name="LEAD_ID")] public string LeadId { get; set; }
        [DataMember(Name="COMPANY_ID")] public string CompanyId { get; set; }
        [DataMember(Name="CONTACT_ID")] public string ContancId { get; set; }
        [DataMember(Name="QUOTE_ID")]  public string QuioteId { get; set; }
        [DataMember(Name="BEGINDATE")]  public DateTime BegunDate { get; set; }
        [DataMember(Name="CLOSEDATE")]  public DateTime CloseDate { get; set; }
        [DataMember(Name="ASSIGNED_BY_ID")]  public int AssignedById { get; set; }
        [DataMember(Name="CREATED_BY_ID")] public int CreatedById { get; set; } 
        [DataMember(Name="MODIFY_BY_ID")]  public int ModifyById { get; set; }
        [DataMember(Name="DATE_CREATE")]  public DateTime DateCreate { get; set; }
        [DataMember(Name="DATE_MODIFY")]  public DateTime DateModyfy { get; set; }
        [DataMember(Name="OPENED")]  public string Opened { get; set; }
        [DataMember(Name="CLOSED")]  public string Closed { get; set; }
        [DataMember(Name="UF_CRM_1603522128")]  public string Status { get; set; }
        //2 значения разделены запятой, пример: 59.852624,30.226881
        [DataMember(Name="UF_CRM_1611649517604")]  public string Coordinates { get; set; }
        [DataMember(Name="UF_CRM_5DA85CF9E13B9")]  public string DeliveryAddressWithoutHouse { get; set; }
        [DataMember(Name="UF_CRM_5DA85CFA4B2FD")]  public string RoomNumber { get; set; }
        [DataMember(Name="UF_CRM_5DADB4A25AFE5")]  public string HouseAndBuilding { get; set; }
        [DataMember(Name="UF_CRM_1593010244990")]  public string PaymentStatus { get; set; }
    }
}