using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
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
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IBus _messageBus;

		public TransferEdoHandler(
			ILogger<TransferEdoHandler> logger,
			IUnitOfWork uow,
			EdoTaskValidator edoTaskValidator,
			TransferTaskRepository transferTaskRepository,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferDispatcher transferDispatcher,
			EdoProblemRegistrar edoProblemRegistrar,
			ITrueMarkCodeRepository trueMarkCodeRepository,
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
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task HandleNewTransfer(int transferIterationId, CancellationToken cancellationToken)
		{
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

			var transferTasks = new List<TransferEdoTask>();
			foreach(var requestsGroup in requestsGroups)
			{
				var transferTask = await _transferDispatcher.AddRequestsToTask(
					requestsGroup,
					cancellationToken
				);
				transferTasks.Add(transferTask);
			}

			var sentTasks = transferTasks.Where(x => x.TransferStatus == TransferEdoTaskStatus.PreparingToSend);
			await _uow.CommitAsync(cancellationToken);

			var events = sentTasks.Select(x => new TransferTaskPrepareToSendEvent { TransferTaskId = x.Id });
			var publishTasks = events.Select(message => _messageBus.Publish(message, cancellationToken));

			await Task.WhenAll(publishTasks);
		}

		public async Task HandleTransferDocumentAcceptance(int documentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<TransferEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке подтверждения документа №{documentId} не найден документ.", documentId);
				return;
			}

			if(document.AcceptTime == null)
			{
				_logger.LogError("Невозможно завершить трансфер, так как документ №{documentId} еще не принят.", documentId);
				return;
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);

			if(transferTask.Status == EdoTaskStatus.Completed)
			{
				_logger.LogWarning("При обработке принятия документа трансфера №{documentId} обнаружено, что трансфер уже завершен.", documentId);
				return;
			}

			var requests = await _uow.Session.QueryOver<TransferEdoRequest>()
				.Fetch(SelectMode.Fetch, x => x.Iteration)
				.Fetch(SelectMode.Fetch, x => x.Iteration.OrderEdoTask)
				.Where(x => x.TransferEdoTask.Id == document.TransferTaskId)
				.ListAsync();

			await _uow.Session.QueryOver<TransferEdoRequest>()
				.Fetch(SelectMode.Fetch, x => x.TransferedItems)
				.Where(x => x.TransferEdoTask.Id == document.TransferTaskId)
				.ListAsync();

			var orderTaskIds = transferTask.TransferEdoRequests.Select(x => x.Iteration.OrderEdoTask.Id);

			var taskItems = await _uow.Session.QueryOver<EdoTaskItem>()
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode.Tag1260CodeCheckResult)
				.WhereRestrictionOn(x => x.CustomerEdoTask.Id).IsIn(orderTaskIds.ToArray())
				.ListAsync(cancellationToken);

			var sourceCodes = taskItems.Select(x => x.ProductCode)
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = taskItems.Select(x => x.ProductCode)
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			try
			{
				var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(transferTask);
				var isValid = await _edoTaskValidator.Validate(transferTask, cancellationToken, trueMarkCodesChecker);
				if(!isValid)
				{
					return;
				}
			}
			catch(EdoProblemException ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(transferTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
				return;
			}
			catch(Exception ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(transferTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
				return;
			}

			transferTask.TransferStatus = TransferEdoTaskStatus.Completed;
			transferTask.Status = EdoTaskStatus.Completed;
			transferTask.EndTime = DateTime.Now;

			await _uow.SaveAsync(transferTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

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

		public async Task UpdateTransferCompletion(int documentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<TransferEdoDocument>(documentId, cancellationToken);
			if(document == null)
			{
				_logger.LogError("При обработке отмены документа №{documentId} не найден документ.", documentId);
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);

			var requests = await _uow.Session.QueryOver<TransferEdoRequest>()
				.Fetch(SelectMode.Fetch, x => x.Iteration)
				.Fetch(SelectMode.Fetch, x => x.Iteration.OrderEdoTask)
				.Where(x => x.TransferEdoTask.Id == document.TransferTaskId)
				.ListAsync();

			await _uow.Session.QueryOver<TransferEdoRequest>()
				.Fetch(SelectMode.Fetch, x => x.TransferedItems)
				.Where(x => x.TransferEdoTask.Id == document.TransferTaskId)
				.ListAsync();

			var orderTaskIds = transferTask.TransferEdoRequests.Select(x => x.Iteration.OrderEdoTask.Id);

			var taskItems = await _uow.Session.QueryOver<EdoTaskItem>()
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode.Tag1260CodeCheckResult)
				.WhereRestrictionOn(x => x.CustomerEdoTask.Id).IsIn(orderTaskIds.ToArray())
				.ListAsync(cancellationToken);

			var sourceCodes = taskItems.Select(x => x.ProductCode)
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = taskItems.Select(x => x.ProductCode)
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			await UpdateIterationsAndNotifyOnCompleted(transferTask, cancellationToken);
		}

		public async Task HandleTransferDocumentCancelled(int documentId, CancellationToken cancellationToken)
		{
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
			var document = await _uow.Session.GetAsync<TransferEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке отмены документа №{documentId} не найден документ.", documentId);
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);

			await _edoProblemRegistrar.RegisterCustomProblem<DocflowCouldNotBeCompleted>(
				transferTask, cancellationToken, "Возникла проблема с документооборотом, не завершился на стороне ЭДО провайдера");
		}

		public async Task MoveToPrepareToSend(int transferTaskId, CancellationToken cancellationToken)
		{
			var transferEdoTask = await _uow.Session.GetAsync<TransferEdoTask>(transferTaskId, cancellationToken);
			await _transferDispatcher.MoveToPrepareToSend(transferEdoTask, cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(
				new TransferTaskPrepareToSendEvent { TransferTaskId = transferTaskId },
				cancellationToken
			);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
