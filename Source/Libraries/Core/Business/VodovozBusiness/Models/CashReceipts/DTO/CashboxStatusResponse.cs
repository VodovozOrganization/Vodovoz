using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Vodovoz.Models.CashReceipts.DTO
{
	public class CashboxStatusResponse
	{
		[JsonProperty("dateTime")]
		public string DateTime { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		public FiscalRegistratorStatus CashboxStatus {
			get {
				switch(Status.ToLower()) {
					case "ready": return FiscalRegistratorStatus.Ready;
					case "failed": return FiscalRegistratorStatus.Failed;
					case "associated": return FiscalRegistratorStatus.Associated;
					default: return FiscalRegistratorStatus.Unknown;
				}
			}
		}
	}
}
