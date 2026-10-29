using System.Threading;
using System.Threading.Tasks;
using Edo.Problems.Validation;
using EdoNotifications.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.Common
{
	public interface IEdoProblemRoutineNotificationService
	{
		/// <summary>
		/// Публикует уведомление <typeparamref name="TSetting"/> о проблеме задачи ЭДО в транзакцию <paramref name="unitOfWork"/>
		/// </summary>
		/// <typeparam name="TSetting">Класс настройки уведомления</typeparam>
		Task<bool> NotifyAsync<TSetting>(
			IUnitOfWork unitOfWork,
			OrderEdoTask edoTask,
			IEdoTaskValidator validator,
			CancellationToken cancellationToken)
			where TSetting : EdoNotificationSetting;
	}
}
