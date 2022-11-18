using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VodovozDeliveryRulesService
{
    [DataContract]
    public class DeliveryInfoDTO
    {
        private DeliveryRulesResponseStatus statusEnum;
        public DeliveryRulesResponseStatus StatusEnum {
            get => statusEnum;
            set {
                statusEnum = value;
                Status = statusEnum.ToString();
            }
        }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Message { get; set; }
        
        [DataMember]
        public string GeoGroup { get; set; }

        [DataMember]
        public IList<WeekDayDeliveryInfoDTO> WeekDayDeliveryInfos { get; set; }
    }
}