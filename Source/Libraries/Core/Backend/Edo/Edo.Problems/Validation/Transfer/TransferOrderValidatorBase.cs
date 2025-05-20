using Edo.Problems.Exception.TransferOrders;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Results;

namespace Edo.Problems.Validation.Transfer
{
	public abstract class TransferOrderValidatorBase : EdoTaskProblemValidatorSourceEntity, ITransferOrderValidator
	{
		public abstract bool IsApplicable(TransferOrder transferOrder);
		public abstract Task<Result<TransferOrder, TransferOrderValidationError>> Validate(TransferOrder transferOrder);
	}
}
