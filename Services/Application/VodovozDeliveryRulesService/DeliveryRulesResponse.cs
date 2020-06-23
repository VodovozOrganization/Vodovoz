using System;
using System.Runtime.Serialization;
namespace VodovozDeliveryRulesService
{
	[DataContract]
	public class DeliveryRulesResponse
	{
		private DeliveryRulesResponseStatus statusEnum;

		public DeliveryRulesResponseStatus StatusEnum {
			get { return statusEnum; }
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
		public DeliveryRuleDTO DeliveryRule { get; set; }
	}
}
