using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public interface ITrueMarkCodesValidator
	{
		Task<TrueMarkTaskValidationResult> ValidateAsync(OrderEdoTask edoTask, EdoTaskItemTrueMarkStatusProvider edoTaskItemTrueMarkStatusProvider, CancellationToken cancellationToken);

		Task<IEnumerable<TrueMarkCodeValidationResult>> ValidateAsync(
			IEnumerable<ProductInstanceStatus> productInstanceStatuses,
			string organizationInn,
			CancellationToken cancellationToken);
		
		Task<TrueMarkTaskValidationResult> ValidateAsync(IEnumerable<TrueMarkWaterIdentificationCode> codes, string organizationInn, CancellationToken cancellationToken);
		Task<TrueMarkTaskValidationResult> ValidateAsync(IEnumerable<string> codes, string organizationInn, CancellationToken cancellationToken);
	}
}
