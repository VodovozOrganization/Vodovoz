using System;
using System.Runtime.Serialization;
namespace VodovozDeliveryRulesService
{
	[DataContract]
	public class DeliveryRuleDTO
	{
		[DataMember]
		public int MinBottles { get; set; }

		[DataMember]
		public decimal DeliveryPrice { get; set; }

		[DataMember]
		public string DeliveryRuleTitle { get; set; }

		[DataMember]
		public string DeliverySchedule { get; set; }

	}
}
