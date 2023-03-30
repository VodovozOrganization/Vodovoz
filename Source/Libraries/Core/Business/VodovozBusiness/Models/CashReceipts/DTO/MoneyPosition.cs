using Newtonsoft.Json;

namespace Vodovoz.Models.CashReceipts.DTO
{
	public class MoneyPosition
	{
		[JsonProperty("paymentType", Required = Required.Always)]
		public string PaymentType { get; set; }

		[JsonProperty("sum", Required = Required.Always)]
		public decimal Sum { get; set; }
	}
}
