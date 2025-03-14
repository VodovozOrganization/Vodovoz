using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transfer.Dispatcher
{
	public class TransferEdoHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<TransferEdoHandler> _logger;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly TransferTaskRepository _transferTaskRepository;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferDispatcher _transferDispatcher;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly IBus _messageBus;

		public TransferEdoHandler(
			ILogger<TransferEdoHandler> logger,
			IUnitOfWork uow,
			EdoTaskValidator edoTaskValidator,
			TransferTaskRepository transferTaskRepository,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferDispatcher transferDispatcher,
			EdoProblemRegistrar edoProblemRegistrar,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_transferDispatcher = transferDispatcher ?? throw new ArgumentNullException(nameof(transferDispatcher));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task HandleNewTransfer(int transferIterationId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var transferIteration = await _uow.Session.GetAsync<TransferEdoRequestIteration>(transferIterationId, cancellationToken);

			if(transferIteration == null)
			{
				throw new InvalidOperationException($"Итерация трансфера с Id {transferIterationId} не найдена");
			}

			if(transferIteration.Status != TransferEdoRequestIterationStatus.InProgress)
			{
				_logger.LogWarning("На трансфер можно принять только не завершенную интерацию");
				return;
			}

			var hasAssignedTransferTask = transferIteration.TransferEdoRequests.Any(x => x.TransferEdoTask != null);
			if(hasAssignedTransferTask)
			{
				_logger.LogWarning("При первичной обработки на трансфер итерации Id {transferIterationId} " +
					"обнаружены уже обработанные заявки на трансфер. Возможно задача попала на обработку второй раз.",
					transferIterationId);
			}

			var newTransferRequests = transferIteration.TransferEdoRequests.Where(x => x.TransferEdoTask == null);
			if(!newTransferRequests.Any())
			{
				_logger.LogError("При первичной обработки на трансфер клиентской задачи Id {transferIterationId} " +
					"не обнаружены новые заявки на трансфер.", transferIterationId);
				return;
			}

			var requestsGroups = newTransferRequests.GroupBy(x => new TransferDirection(
				x.FromOrganizationId, 
				x.ToOrganizationId
			));

			var addRequestsTasks = requestsGroups.Select(requestsGroup => 
				_transferDispatcher.AddRequestsToTask(
					_uow, 
					requestsGroup, 
					cancellationToken
				)
			);

			var transferTasks = await Task.WhenAll(addRequestsTasks);
			var sentTasks = transferTasks.Where(x => x.TransferStatus == TransferEdoTaskStatus.InProgress);
			await _uow.CommitAsync(cancellationToken);

			var events = sentTasks.Select(x => new TransferTaskReadyToSendEvent { Id = x.Id });
			var publishTasks = events.Select(message => _messageBus.Publish(message, cancellationToken));

			await Task.WhenAll(publishTasks);
		}

		public async Task HandleTransferDocumentAcceptance(int documentId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var document = await _uow.Session.GetAsync<TransferEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке подтверждения документа №{documentId} не найден документ.", documentId);
			}

			if(document.AcceptTime == null)
			{
				_logger.LogError("Невозможно завершить трансфер, так как документ №{documentId} еще не принят.", documentId);
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);
			if(transferTask.Status == EdoTaskStatus.Completed)
			{
				_logger.LogWarning("При обработке принятия документа трансфера №{documentId} обнаружено, что трансфер уже завершен.", documentId);
				_uow.Dispose();
				return;
			}


			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(transferTask);
			var isValid = await _edoTaskValidator.Validate(transferTask, cancellationToken, trueMarkCodesChecker);
			if(!isValid)
			{
				return;
			}

			transferTask.TransferStatus = TransferEdoTaskStatus.Completed;
			transferTask.Status = EdoTaskStatus.Completed;
			transferTask.EndTime = DateTime.Now;

			await _uow.SaveAsync(transferTask, cancellationToken: cancellationToken);
			_uow.Commit();

			await UpdateIterationsAndNotifyOnCompleted(transferTask, cancellationToken);
		}

		private async Task UpdateIterationsAndNotifyOnCompleted(TransferEdoTask transferTask, CancellationToken cancellationToken)
		{
			var messages = new List<TransferCompleteEvent>();

			var transferIterations = transferTask.TransferEdoRequests.Select(x => x.Iteration).Distinct();
			foreach(var transferIteration in transferIterations)
			{
				var iterationCompleted = await _transferTaskRepository.IsTransferIterationCompletedAsync(
					_uow, transferIteration.Id, cancellationToken);

				if(!iterationCompleted)
				{
					continue;
				}

				transferIteration.Status = TransferEdoRequestIterationStatus.Completed;
				await _uow.SaveAsync(transferIteration, cancellationToken: cancellationToken);

				messages.Add(new TransferCompleteEvent
				{
					TransferIterationId = transferIteration.Id,
					TransferInitiator = transferIteration.Initiator
				});
			}

			await _uow.CommitAsync(cancellationToken);

			var notificationTasks = messages.Select(message => _messageBus.Publish(message, cancellationToken));
			await Task.WhenAll(notificationTasks);
		}

		public async Task HandleTransferDocumentCancelled(int documentId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var document = await _uow.Session.GetAsync<TransferEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке отмены документа №{documentId} не найден документ.", documentId);
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);

			await _edoProblemRegistrar.RegisterCustomProblem<DocflowCouldNotBeCompleted>(
				transferTask, cancellationToken, "Документооборот был отменен");
		}

		public async Task HandleTransferDocumentProblem(int documentId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var document = await _uow.Session.GetAsync<TransferEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке отмены документа №{documentId} не найден документ.", documentId);
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);

			await _edoProblemRegistrar.RegisterCustomProblem<DocflowCouldNotBeCompleted>(
				transferTask, cancellationToken, "Возникла проблема с документооборотом, не завершился на стороне ЭДО провайдера");
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
