using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class ContactRequest
    {
        [DataMember(Name="result")]public Contact Result { get; set; }
        [DataMember(Name="time")] public ResponseTime ResponseTime { get; set; }
    }
    
    [DataContract]
    public class Contact
    {

        [DataMember(Name="ID")] public int Id { get; set; }
        [DataMember(Name="COMMENTS")] public string COMMENTS { get; set; }
        [DataMember(Name="NAME")] public string NAME { get; set; }
        [DataMember(Name="SECOND_NAME")] public string SECOND_NAME { get; set; }
        [DataMember(Name="LAST_NAME")] public string LAST_NAME { get; set; }
        [DataMember(Name="LEAD_ID")] public string LEAD_ID { get; set; }
        [DataMember(Name="TYPE_ID")] public string TYPE_ID { get; set; }
        [DataMember(Name="SOURCE_ID")] public string SOURCE_ID { get; set; }
        [DataMember(Name="COMPANY_ID")] public string COMPANY_ID { get; set; }
        [DataMember(Name="ASSIGNED_BY_ID")] public int ASSIGNED_BY_ID { get; set; }
        [DataMember(Name="CREATED_BY_ID")] public int CREATED_BY_ID { get; set; }
        [DataMember(Name="MODIFY_BY_ID")] public int MODIFY_BY_ID { get; set; }
        [DataMember(Name="OPENED")] public string OPENED { get; set; }
        
        [DataMember(Name="PHONE")] public IList<Phone> PHONE { get; set; }
        [DataMember(Name="EMAIL")] public IList<Email> EMAIL { get; set; }
    }
}