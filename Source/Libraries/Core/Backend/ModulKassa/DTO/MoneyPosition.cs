using Newtonsoft.Json;

namespace ModulKassa.DTO
{
	public class MoneyPosition
	{
		[JsonProperty("paymentType", Required = Required.Always)]
		public string PaymentType { get; set; }

		[JsonProperty("sum", Required = Required.Always)]
		public decimal Sum { get; set; }
	}
}
