using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	public interface IReceiptContactProblemNotificationService
	{
		Task<bool> TryNotifyAsync(
			IUnitOfWork unitOfWork,
			ReceiptEdoTask receiptTask,
			EdoTaskProblem problem,
			int retryCount,
			CancellationToken cancellationToken);
	}
}
