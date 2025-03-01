using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCheckResult
	{
		public TrueMarkWaterIdentificationCode Code { get; set; }

		/// <summary>
		/// В обороте
		/// </summary>
		public bool Introduced { get; set; }

		public string OwnerInn { get; set; }
		public string OwnerName { get; set; }
	}
}
