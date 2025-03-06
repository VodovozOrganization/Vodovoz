using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public interface ITrueMarkCodesValidator
	{
		Task<TrueMarkTaskValidationResult> ValidateAsync(OrderEdoTask edoTask, EdoTaskItemTrueMarkStatusProvider edoTaskItemTrueMarkStatusProvider, CancellationToken cancellationToken);
		Task<TrueMarkTaskValidationResult> ValidateAsync(IEnumerable<TrueMarkWaterIdentificationCode> codes, string organizationInn, CancellationToken cancellationToken);
	}
}
