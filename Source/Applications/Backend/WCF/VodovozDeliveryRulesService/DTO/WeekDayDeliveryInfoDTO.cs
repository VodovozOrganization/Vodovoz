using System.Collections.Generic;
using System.Runtime.Serialization;
using Vodovoz.Domain.Sale;

namespace VodovozDeliveryRulesService
{
	[DataContract]
	public class WeekDayDeliveryInfoDTO
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
		public IList<DeliveryRuleDTO> DeliveryRules { get; set; }
		
		[DataMember]
		public IList<string> ScheduleRestrictions { get; set; }
	}

	[DataContract]
	public class DeliveryRuleDTO
	{
		[DataMember]
		public string Bottles19l { get; set; }
		
		[DataMember]
		public string Bottles6l { get; set; }
		
		[DataMember]
		public string Bottles1500ml { get; set; }
		
		[DataMember]
		public string Bottles600ml { get; set; }
		
		[DataMember]
		public string Bottles500ml { get; set; }
		
		[DataMember]
		public string MinOrder { get; set; }
		
		[DataMember]
		public string Price { get; set; }
	}
}
