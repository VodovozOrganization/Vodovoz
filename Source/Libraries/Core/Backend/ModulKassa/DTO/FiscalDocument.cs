using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModulKassa.DTO
{
	public class FiscalDocument
	{
		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("docNum", Required = Required.Always)]
		public string DocNum { get; set; }

		[JsonProperty("docType", Required = Required.Always)]
		public string DocType { get; set; }

		[JsonProperty("checkoutDateTime", Required = Required.Always)]
		public string CheckoutDateTime { get; set; }

		[JsonProperty("email", Required = Required.Always)]
		public string Email { get; set; }

		[JsonProperty("clientInn")]
		public string ClientINN { get; set; }

		[JsonProperty("cashierName")]
		public string CashierName { get; set; }

		[JsonProperty("printReceipt")]
		public bool PrintReceipt { get; set; }

		[JsonProperty("taxMode")]
		public string TaxMode { get; set; }

		[JsonProperty("responseURL")]
		public string ResponseURL { get; set; }

		[JsonProperty("inventPositions", Required = Required.Always)]
		public List<InventPosition> InventPositions { get; set; } = new List<InventPosition>();

		[JsonProperty("moneyPositions", Required = Required.Always)]
		public List<MoneyPosition> MoneyPositions { get; set; } = new List<MoneyPosition>();
	}
}
