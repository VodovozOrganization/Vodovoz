using Vodovoz.Core.Domain.Interfaces.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkWaterCode : ITrueMarkWaterCode
	{
		public string SourceCode { get; set; }
		public string GTIN { get; set; }
		public string SerialNumber { get; set; }
		public string CheckCode { get; set; }
	}
}
