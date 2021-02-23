using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BitrixApi.DTO.DataContractJsonSerializer {
    [DataContract]
    public class BitrixPostResponse {
        // [DataMember(Name="event")] public string Event { get; set; }
        // [DataMember(Name="data")] public Data Data { get; set; }
        // [DataMember(Name="ts")] public string Ts { get; set; }
        
        [DataMember(Name="FIELDS")] public Field Fields { get; set; }
    }

    [DataContract]
    public class Data {
        [DataMember(Name="FIELDS")] public IList<Field> Fields { get; set; }
    }
    
    [DataContract]
    public class Field {
        [DataMember(Name="ID")]  public uint Id { get; set; }
    }
}