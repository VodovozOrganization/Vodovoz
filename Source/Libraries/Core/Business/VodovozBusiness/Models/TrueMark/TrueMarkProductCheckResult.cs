using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkProductCheckResult
	{
		public TrueMarkCashReceiptProductCode Code { get; set; }

		/// <summary>
		/// В обороте
		/// </summary>
		public bool Introduced { get; set; }
	}
}
