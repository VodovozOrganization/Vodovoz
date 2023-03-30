using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCheckResult
	{
		public TrueMarkWaterIdentificationCode Code { get; set; }

		/// <summary>
		/// В обороте
		/// </summary>
		public bool Introduced { get; set; }
	}
}
