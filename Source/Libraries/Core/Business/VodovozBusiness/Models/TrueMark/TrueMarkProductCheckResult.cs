using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkProductCheckResult
	{
		public CashReceiptProductCode Code { get; set; }

		/// <summary>
		/// В обороте
		/// </summary>
		public bool Introduced { get; set; }

		public string OwnerInn { get; set; }

		public string OwnerName { get; set; }
	}
}
