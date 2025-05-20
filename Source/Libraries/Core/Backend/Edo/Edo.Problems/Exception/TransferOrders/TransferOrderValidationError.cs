using Edo.Problems.Exception.Validation;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception.TransferOrders
{
	public class TransferOrderValidationError : ValidationError<TransferOrder>
	{
		public TransferOrderValidationError(TransferOrder transferOrder)
		{
			TransferOrder = transferOrder;
		}

		public TransferOrder TransferOrder { get; }
	}
}
