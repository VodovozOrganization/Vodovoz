using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
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
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly IPublishEndpoint _publishEndpoint;

		public EdoCancellationService(
			ILogger<EdoCancellationService> logger,
			IUnitOfWork uow,
			IEdoCancellationValidator edoCancellationValidator,
			EdoProblemRegistrar edoProblemRegistrar,
			IPublishEndpoint publishEndpoint
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoCancellationValidator = edoCancellationValidator ?? throw new ArgumentNullException(nameof(edoCancellationValidator));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public async Task CancelTask(
			int taskId, 
			string reason,
			CancellationToken cancellationToken
		)
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

		private async Task CancelOrderTask(
			OrderEdoTask edoTask, 
			string reason,
			CancellationToken cancellationToken
			)
		{
			var orderDocument = await _uow.Session.QueryOver<OrderEdoDocument>()
				.Where(x => x.DocumentTaskId == edoTask.Id)
				.SingleOrDefaultAsync(cancellationToken);

			if(orderDocument == null || orderDocument.Status == EdoDocumentStatus.Cancelled)
			{
				edoTask.Status = EdoTaskStatus.Cancelled;
				edoTask.CancellationReason = reason;

				await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
				return;
			}

			edoTask.Status = EdoTaskStatus.InCancellation;
			edoTask.CancellationReason = reason;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);

			var message = new RequestDocflowCancellationEvent
			{
				TaskId = edoTask.Id,
				Reason = reason
			};

			await _publishEndpoint.Publish(message, cancellationToken);
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

			var transferDocument = await _uow.Session.QueryOver<TransferEdoDocument>()
				.Where(x => x.TransferTaskId == transferEdoTask.Id)
				.SingleOrDefaultAsync(cancellationToken);

			if(transferDocument == null || transferDocument.Status == EdoDocumentStatus.Cancelled)
			{
				transferEdoTask.Status = EdoTaskStatus.Cancelled;
				transferEdoTask.CancellationReason = reason;

				await _uow.SaveAsync(transferEdoTask, cancellationToken: cancellationToken);
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

			await _publishEndpoint.Publish(message, cancellationToken);
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

			await _uow.SaveAsync(transferTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		public async Task AcceptOrderTaskCancellation(int orderDocumentId, CancellationToken cancellationToken)
		{
			var orderDocument = await _uow.Session.GetAsync<OrderEdoDocument>(orderDocumentId, cancellationToken);
			if(orderDocument == null)
			{
				_logger.LogWarning("Документ №{OrderDocumentId} не найден.", orderDocumentId);
				return;
			}

			var orderTask = await _uow.Session.GetAsync<OrderEdoTask>(orderDocument.DocumentTaskId, cancellationToken);
			if(orderTask == null)
			{
				_logger.LogWarning("Задача №{TaskId} не найдена.", orderDocument.DocumentTaskId);
				return;
			}

			if(orderTask.Status == EdoTaskStatus.Cancelled)
			{
				_logger.LogWarning("Задача №{TaskId} уже отменена.", orderDocument.DocumentTaskId);
				return;
			}

			if(orderTask.Status != EdoTaskStatus.InCancellation)
			{
				_logger.LogWarning("Задача №{TaskId} не находится в процессе отмены.", orderDocument.DocumentTaskId);
				await _edoProblemRegistrar.RegisterCustomProblem<DocflowCouldNotBeCompleted>(
					orderTask, cancellationToken, "Документооборот был отменен");
				return;
			}

			orderTask.Status = EdoTaskStatus.Cancelled;
			orderTask.EndTime = DateTime.Now;

			await _uow.SaveAsync(orderTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}
	}
}
