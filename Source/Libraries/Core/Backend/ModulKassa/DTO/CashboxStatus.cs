using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ModulKassa.DTO
{
	public class CashboxStatus
	{
		[JsonProperty("dateTime")]
		public string DateTime { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		public FiscalRegistratorStatus FiscalRegistratorStatus
		{
			get
			{
				switch(Status.ToLower())
				{
					case "ready": return FiscalRegistratorStatus.Ready;
					case "failed": return FiscalRegistratorStatus.Failed;
					case "associated": return FiscalRegistratorStatus.Associated;
					default: return FiscalRegistratorStatus.Unknown;
				}
			}
		}
	}
}
