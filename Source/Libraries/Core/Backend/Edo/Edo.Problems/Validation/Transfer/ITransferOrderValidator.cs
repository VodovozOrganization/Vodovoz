using Edo.Problems.Exception.TransferOrders;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Results;

namespace Edo.Problems.Validation.Transfer
{
	public interface ITransferOrderValidator
	{
		bool IsApplicable(TransferOrder transferOrder);
		Task<Result<TransferOrder, TransferOrderValidationError>> Validate(TransferOrder transferOrder);
	}
}
