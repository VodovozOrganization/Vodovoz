using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services
{
	public class CodeDuplicatedProblemService
	{
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _codeDuplicatedMessagePublisher;
		private readonly IEdoNotificationMessageFactory _edoNotificationMessageFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public CodeDuplicatedProblemService(
			IOutboxNotificationPublisher<EdoNotificationMessage> codeDuplicatedMessagePublisher,
			IEdoNotificationMessageFactory edoNotificationMessageFactory,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_codeDuplicatedMessagePublisher = codeDuplicatedMessagePublisher ?? throw new ArgumentNullException(nameof(codeDuplicatedMessagePublisher));
			_edoNotificationMessageFactory = edoNotificationMessageFactory ?? throw new ArgumentNullException(nameof(edoNotificationMessageFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public async Task ProcessProblemTasks(
			CancellationToken cancellationToken)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(nameof(CodeDuplicatedProblemService)))
			{
				// var problemTasks = await GetProblemTasks(unitOfWork, cancellationToken);
				// var notSuccessfullyProcessedProblemTasks = await ProcessProblems(unitOfWork, cancellationToken);
				// ...

				var message = _edoNotificationMessageFactory.Create(
					EdoNotificationType.CodeDuplicated,
					("EdoTaskId", "123"),
					("Codes", "\u001d0104602009723094215H>Dw?JPSUOdS\u001d, \u001d0104602009723094215jtGj2dt:d&Zb\u001d93cdfg"));

				await _codeDuplicatedMessagePublisher.TryPublishAsync(unitOfWork, message, cancellationToken);

				await unitOfWork.CommitAsync(cancellationToken);
			}
		}
	}
}
