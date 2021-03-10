using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BitrixApi.DTO.DataContractJsonSerializer
{
    [DataContract]
    public class ResponseTime
    {
        [DataMember(Name="date_start")] public DateTime DateStart { get; set; }
        [DataMember(Name="date_finish")] public DateTime DateFinish { get; set; }
        [DataMember(Name="duration")] public double Duration { get; set; }
        [DataMember(Name="processing")] public double Processing { get; set; }
    }
}