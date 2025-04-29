using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public interface ISaveCodesService
	{
		Task SaveCodesToPool(SaveCodesEdoTask edoTask, CancellationToken cancellationToken);
		Task SaveCodesToPool(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken);
	}
}
