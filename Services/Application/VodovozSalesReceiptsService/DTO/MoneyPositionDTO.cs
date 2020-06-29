using System.Runtime.Serialization;

namespace VodovozSalesReceiptsService.DTO
{
	[DataContract]
	public class MoneyPositionDTO
	{
		public MoneyPositionDTO(decimal sum)
		{
			Sum = sum;
		}

		[DataMember(IsRequired = true)]
		string paymentType = "CASH";

		[DataMember(IsRequired = true)]
		decimal sum;
		public decimal Sum {
			get => sum;
			set => sum = value;
		}
	}
}