using System;
using System.Runtime.Serialization;

namespace Android.DTO
{
	[DataContract]
	public class PaymentInfoDTO
	{
		[DataMember]
		public int OrderId { get; set; }

		[DataMember]
		public PaymentStatus Status { get; set; }

		public PaymentInfoDTO(int orderId, PaymentStatus status)
		{
			OrderId = orderId;
			Status = status;
		}
	}
}
