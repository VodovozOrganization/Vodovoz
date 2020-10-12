using System.Runtime.Serialization;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozSalesReceiptsService.DTO
{
	[DataContract]
	public class MoneyPositionDTO
	{
		public MoneyPositionDTO(Order order, decimal sum)
		{
			Sum = sum;
			FillPaymentType(order);
		}

		[DataMember(IsRequired = true)]
		string paymentType;

		[DataMember(IsRequired = true)]
		decimal sum;
		public decimal Sum {
			get => sum;
			set => sum = value;
		}

		private void FillPaymentType(Order order) {

			var cash = "CASH";
			var card = "CARD";
			
			switch (order.PaymentType) {
				case PaymentType.Terminal:
				case PaymentType.ByCard:
					paymentType = card;
					break;
				default:
					paymentType = cash;
					break;
			}
		}
	}
}