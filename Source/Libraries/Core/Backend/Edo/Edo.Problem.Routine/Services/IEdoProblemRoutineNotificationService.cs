using System.Threading;
using System.Threading.Tasks;
using Edo.Problems.Validation;
using EdoNotifications.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	public interface IEdoProblemRoutineNotificationService
	{
		Task<bool> NotifyAsync(
			IUnitOfWork unitOfWork,
			OrderEdoTask edoTask,
			EdoNotificationType notificationType,
			IEdoTaskValidator validator,
			CancellationToken cancellationToken);
	}
}
