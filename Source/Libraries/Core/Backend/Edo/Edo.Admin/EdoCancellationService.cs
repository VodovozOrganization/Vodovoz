using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Admin
{
	public class EdoCancellationService
	{
		private readonly ILogger<EdoCancellationService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IEdoCancellationValidator _edoCancellationValidator;
		private readonly IBus _messageBus;

		public EdoCancellationService(
			ILogger<EdoCancellationService> logger,
			IUnitOfWork uow,
			IEdoCancellationValidator edoCancellationValidator,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoCancellationValidator = edoCancellationValidator ?? throw new ArgumentNullException(nameof(edoCancellationValidator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task CancelTask(int taskId, string reason, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<EdoTask>(taskId, cancellationToken);
			if(edoTask == null)
			{
				_logger.LogWarning("Задача №{TaskId} не найдена.", taskId);
				return;
			}

			var canCancel = _edoCancellationValidator.CanCancelEdoTask(edoTask);
			if(!canCancel)
			{
				return;
			}

			if(edoTask.TaskType == EdoTaskType.Transfer)
			{
				await CancelTransferTask((TransferEdoTask)edoTask, reason, cancellationToken);
			}
			else
			{
				await CancelOrderTask((OrderEdoTask)edoTask, reason, cancellationToken);
			}

			await _uow.CommitAsync(cancellationToken);
		}

		private async Task CancelOrderTask(OrderEdoTask edoTask, string reason, CancellationToken cancellationToken)
		{
			var relatedTransfers = edoTask.TransferIterations
				.SelectMany(x => x.TransferEdoRequests.Select(t => t.TransferEdoTask));

			foreach(var transfer in relatedTransfers)
			{
				await CancelTransferTask(transfer, reason, cancellationToken);
			}

			if(edoTask.Status == EdoTaskStatus.New)
			{
				edoTask.Status = EdoTaskStatus.Cancelled;
			}
			else
			{
				edoTask.Status = EdoTaskStatus.InCancellation;
			}

			edoTask.CancellationReason = reason;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
		}

		private async Task CancelTransferTask(
			TransferEdoTask transferEdoTask,
			string reason,
			CancellationToken cancellationToken
			)
		{
			if(transferEdoTask.Status.IsIn(
				EdoTaskStatus.Completed,
				EdoTaskStatus.Cancelled,
				EdoTaskStatus.InCancellation
				))
			{
				return;
			}

			transferEdoTask.Status = EdoTaskStatus.InCancellation;
			transferEdoTask.CancellationReason = reason;

			await _uow.SaveAsync(transferEdoTask, cancellationToken: cancellationToken);

			var message = new RequestDocflowCancellationEvent
			{
				TaskId = transferEdoTask.Id,
				Reason = reason
			};

			await _messageBus.Publish(message, cancellationToken);
		}

		public async Task AcceptTransferTaskCancellation(int transferDocumentId, CancellationToken cancellationToken)
		{
			var transferDocument = await _uow.Session.GetAsync<TransferEdoDocument>(transferDocumentId, cancellationToken);
			if(transferDocument == null)
			{
				_logger.LogWarning("Документ №{TransferDocumentId} не найден.", transferDocumentId);
				return;
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(transferDocument.TransferTaskId, cancellationToken);
			if(transferTask == null)
			{
				_logger.LogWarning("Задача №{TaskId} не найдена.", transferDocument.TransferTaskId);
				return;
			}

			if(transferTask.Status != EdoTaskStatus.InCancellation)
			{
				_logger.LogWarning("Задача №{TaskId} не находится в процессе отмены.", transferDocument.TransferTaskId);
				return;
			}

			transferTask.Status = EdoTaskStatus.Cancelled;
			transferTask.EndTime = DateTime.Now;

			await UpdateInCancellationOrderTasks(transferTask, cancellationToken);

			await _uow.SaveAsync(transferTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		private async Task UpdateInCancellationOrderTasks(TransferEdoTask transferTask, CancellationToken cancellationToken)
		{
			var orderTasks = transferTask.TransferEdoRequests
				.SelectMany(x => x.TransferedItems.Select(t => t.CustomerEdoTask));

			var inCancellationTasks = orderTasks.Where(x => x.Status == EdoTaskStatus.InCancellation);
			foreach(var inCancellationTask in inCancellationTasks)
			{
				await TryAcceptCancellationOrderTask(inCancellationTask, cancellationToken);
			}
		}

		private async Task TryAcceptCancellationOrderTask(
			OrderEdoTask edoTask,
			CancellationToken cancellationToken
			)
		{
			var relatedTransfers = edoTask.TransferIterations
				.SelectMany(x => x.TransferEdoRequests.Select(t => t.TransferEdoTask));

			if(relatedTransfers.All(x => x.Status == EdoTaskStatus.Cancelled))
			{
				edoTask.Status = EdoTaskStatus.Cancelled;
				edoTask.EndTime = DateTime.Now;

				await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			}
		}
	}
}
