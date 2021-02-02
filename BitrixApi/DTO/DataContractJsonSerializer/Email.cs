using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class Email
    {
        [DataMember(Name="ID")] public int ID { get; set; }
        [DataMember(Name="VALUE_TYPE")] public string VALUE_TYPE { get; set; }
        [DataMember(Name="VALUE")] public string VALUE { get; set; }
        [DataMember(Name="TYPE_ID")] public string TYPE_ID { get; set; }
    }
}