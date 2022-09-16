using System.Collections.Generic;
using System.Runtime.Serialization;
using Vodovoz.Domain.Sale;

namespace DeliveryRulesService.DTO
{
    [DataContract]
    public class WeekDayDeliveryRuleDTO
    {
        private WeekDayName weekDayEnum;
        public WeekDayName WeekDayEnum {
            get => weekDayEnum;
            set {
                weekDayEnum = value;
                WeekDay = weekDayEnum.ToString();
            }
        }

        [DataMember]
        public string WeekDay { get; set; }
		
        [DataMember]
        public IList<string> DeliveryRules { get; set; }
		
        [DataMember]
        public IList<string> ScheduleRestrictions { get; set; }
    }
}
