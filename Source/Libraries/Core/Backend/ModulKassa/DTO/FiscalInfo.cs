using Newtonsoft.Json;

namespace ModulKassa.DTO
{
	public class FiscalInfo
	{
		[JsonProperty("checkNumber")]
		public long CheckNumber { get; set; }

		[JsonProperty("checkType")]
		public string CheckType { get; set; }

		[JsonProperty("date")]
		public string Date { get; set; }

		[JsonProperty("fnDocMark")]
		public long FnDocMark { get; set; }

		[JsonProperty("fnDocNumber")]
		public long FnDocNumber { get; set; }

		[JsonProperty("fnNumber")]
		public string FnNumber { get; set; }

		[JsonProperty("kktNumber")]
		public string KktNumber { get; set; }

		[JsonProperty("qr")]
		public string Qr { get; set; }

		[JsonProperty("shiftNumber")]
		public long ShiftNumber { get; set; }

		[JsonProperty("sum")]
		public double Sum { get; set; }

		[JsonProperty("ecrRegistrationNumber")]
		public string EcrRegistrationNumber { get; set; }
	}
}
