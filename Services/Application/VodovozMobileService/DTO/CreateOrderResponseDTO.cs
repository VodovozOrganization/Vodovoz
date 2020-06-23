using System.Runtime.Serialization;

namespace VodovozMobileService.DTO
{
	[DataContract]
	public class CreateOrderResponseDTO
	{
		[DataMember]
		public bool Success { get; private set; }

		[DataMember]
		public int OrderId { get; private set; }

		[DataMember]
		public string UUID { get; private set; }

		public CreateOrderResponseDTO(int orderId, string uuid)
		{
			Success = orderId > 0;
			OrderId = orderId;
			UUID = uuid;
		}
	}
}

