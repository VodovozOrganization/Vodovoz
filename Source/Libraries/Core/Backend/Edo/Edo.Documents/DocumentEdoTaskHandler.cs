using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Documents
{
	public class DocumentEdoTaskHandler : IDisposable
	{
		private readonly ILogger<DocumentEdoTaskHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ForOwnNeedDocumentEdoTaskHandler _forOwnNeedDocumentEdoTaskHandler;
		private readonly ForResaleDocumentEdoTaskHandler _forResaleDocumentEdoTaskHandler;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly IBus _messageBus;

		public DocumentEdoTaskHandler(
			ILogger<DocumentEdoTaskHandler> logger,
			IUnitOfWork uow,
			ForOwnNeedDocumentEdoTaskHandler forOwnNeedDocumentEdoTaskHandler,
			ForResaleDocumentEdoTaskHandler forResaleDocumentEdoTaskHandler,
			EdoTaskValidator edoTaskValidator,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			EdoProblemRegistrar edoProblemRegistrar,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_forOwnNeedDocumentEdoTaskHandler = forOwnNeedDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(forOwnNeedDocumentEdoTaskHandler));
			_forResaleDocumentEdoTaskHandler = forResaleDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(forResaleDocumentEdoTaskHandler));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		// handle new
		// Entry stage: New
		// Validated stage: New
		// Changed to: Transfering, Sending
		// [событие от scheduler]
		// (проверяет нужен ли перенос, или сразу отправляет)
		public async Task HandleNew(int documentEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<DocumentEdoTask>(documentEdoTaskId, cancellationToken);

			if(edoTask == null)
			{
				_logger.LogInformation("Задача Id {DocumentEdoTaskId} не найдена", documentEdoTaskId);
				return;
			}

			if(edoTask.Stage != DocumentEdoTaskStage.New)
			{
				_logger.LogInformation("Задача Id {DocumentEdoTaskId} уже в работе", documentEdoTaskId);
				return;
			}

			if(edoTask.OrderEdoRequest == null)
			{
				_logger.LogInformation("Задача Id {DocumentEdoTaskId} не имеет связи с ЭДО заявкой", documentEdoTaskId);
				return;
			}

			// предзагрузка для ускорения
			var productCodes = await _uow.Session.QueryOver<TrueMarkProductCode>()
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoRequest.Id == edoTask.OrderEdoRequest.Id)
				.ListAsync(cancellationToken);

			var taskCodes = await _uow.Session.QueryOver<EdoTaskItem>()
				.Fetch(SelectMode.Fetch, x => x.ProductCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ProductCode.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoTask.Id == edoTask.Id)
				.ListAsync(cancellationToken);

			var totalProductCodes = productCodes
				.Union(taskCodes.Select(x => x.ProductCode));

			var sourceCodes = totalProductCodes
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = totalProductCodes
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			try
			{
				var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);
				var isValid = await _edoTaskValidator.Validate(edoTask, cancellationToken, trueMarkCodesChecker);
				if(!isValid)
				{
					return;
				}

				if(IsFormalDocument(edoTask))
				{
					await HandleFormalDocument(edoTask, trueMarkCodesChecker, cancellationToken);
				}
				else
				{
					await HandleInformalDocument(edoTask, cancellationToken);
				}
			}
			catch(EdoProblemException ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}
			catch(Exception ex) when (ex.InnerException is MySqlException)
			{
				var mysqlException = (MySqlException)ex.InnerException;
				if(mysqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
				{
					var edoException = new CodeDuplicatedException(mysqlException.Message);
					var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, edoException, cancellationToken);
					if(!registered)
					{
						throw;
					}
				}
			}
			catch(Exception ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}
		}
		public async Task HandleManualTask(int documentEdoTaskId, CancellationToken cancellationToken)
		{
			// Получаем таску
			// ищем по таске исходящий документ
			// Если он есть, в таком случае в новой отправке будут обновлены поля контрагента и заказа, не влияющие на товары и сумму заказа и коды маркировки.
			var edoTask = await _uow.Session.GetAsync<DocumentEdoTask>(documentEdoTaskId, cancellationToken);

			if(edoTask == null)
			{
				_logger.LogInformation("Задача Id {DocumentEdoTaskId} не найдена", documentEdoTaskId);
				return;
			}

			if(edoTask.Stage != DocumentEdoTaskStage.New)
			{
				_logger.LogInformation("Задача Id {DocumentEdoTaskId} уже в работе", documentEdoTaskId);
				return;
			}

			if(edoTask.OrderEdoRequest == null)
			{
				_logger.LogInformation("Задача Id {DocumentEdoTaskId} не имеет связи с ЭДО заявкой", documentEdoTaskId);
				return;
			}

			try
			{
				var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);
				var isValid = await _edoTaskValidator.Validate(edoTask, cancellationToken, trueMarkCodesChecker);
				if(!isValid)
				{
					return;
				}

				if(IsFormalDocument(edoTask))
				{
					await HandleFormalDocument(edoTask, trueMarkCodesChecker, cancellationToken);
				}
				else
				{
					await HandleInformalDocument(edoTask, cancellationToken);
				}
			}
			catch(EdoProblemException ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}
			catch(Exception ex) when (ex.InnerException is MySqlException)
			{
				var mysqlException = (MySqlException)ex.InnerException;
				if(mysqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
				{
					var edoException = new CodeDuplicatedException(mysqlException.Message);
					var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, edoException, cancellationToken);
					if(!registered)
					{
						throw;
					}
				}
			}
			catch(Exception ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}
		}

		private async Task HandleFormalDocument(
			DocumentEdoTask edoTask,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken)
		{
			var reasonForLeaving = edoTask.OrderEdoRequest.Order.Client.ReasonForLeaving;
			if(reasonForLeaving == ReasonForLeaving.Resale)
			{
				await _forResaleDocumentEdoTaskHandler.HandleNewForResaleFormalDocument(
					edoTask,
					trueMarkCodesChecker,
					cancellationToken
				);
			}
			else
			{
				await _forOwnNeedDocumentEdoTaskHandler.HandleNewForOwnNeedsFormalDocument(
					edoTask,
					trueMarkCodesChecker,
					cancellationToken
				);
			}
		}

		private async Task HandleInformalDocument(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			var customerDocument = await SendDocument(edoTask, cancellationToken);

			edoTask.Status = EdoTaskStatus.InProgress;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			var message = new OrderDocumentSendEvent { OrderDocumentId = customerDocument.Id };
			await _messageBus.Publish(message, cancellationToken);
		}

		private bool IsFormalDocument(DocumentEdoTask edoTask)
		{
			switch(edoTask.DocumentType)
			{
				case EdoDocumentType.UPD:
					return true;
				case EdoDocumentType.Bill:
					return false;
				default:
					throw new EdoException($"Неизвестный тип документа {edoTask.DocumentType}.");
			}
		}

		private async Task<OrderEdoDocument> SendDocument(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			edoTask.Stage = DocumentEdoTaskStage.Sending;

			var customerEdoDocument = new OrderEdoDocument
			{
				DocumentTaskId = edoTask.Id,
				DocumentType = edoTask.DocumentType,
				Status = EdoDocumentStatus.NotStarted,
				EdoType = EdoType.Taxcom,
				Type = OutgoingEdoDocumentType.Order
			};

			await _uow.SaveAsync(customerEdoDocument, cancellationToken: cancellationToken);
			return customerEdoDocument;
		}

		// handle transfered
		// Entry stage: Transfering
		// Validated stage: Transfering
		// Changed to: Sending
		// [событие от transfer]
		// (проверяет выполнены ли переносы и отправляет)
		public async Task HandleTransfered(int transferIterationId, CancellationToken cancellationToken)
		{
			var transferIteration = await _uow.Session.GetAsync<TransferEdoRequestIteration>(transferIterationId, cancellationToken);
			if(transferIteration == null)
			{
				_logger.LogInformation("Итерация трансфера Id {TransferIterationId} не найдена", transferIterationId);
				return;
			}

			var edoTask = transferIteration.OrderEdoTask.As<DocumentEdoTask>();
			if(edoTask == null)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
					"так как задача Id {DocumentEdoTaskId} не найдена", transferIteration.OrderEdoTask.Id);
				return;
			}

			if(edoTask.OrderEdoRequest == null)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
					"так как задача Id {DocumentEdoTaskId} не связана ни с одной клиенсткой заявкой", 
					transferIteration.OrderEdoTask.Id);
				return;
			}

			if(edoTask.Status == EdoTaskStatus.Completed)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
					"так как задача Id {DocumentEdoTaskId} уже завершена", edoTask.Id);
				return;
			}

			if(edoTask.Stage != DocumentEdoTaskStage.Transfering)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
					"так как задача Id {DocumentEdoTaskId} находится не на стадии трансфера, " +
					"а на стадии {DocumentEdoTaskStage}",
					edoTask.Id, edoTask.Stage);
				return;
			}

			var productCodes = await _uow.Session.QueryOver<TrueMarkProductCode>()
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Where(x => x.CustomerEdoRequest.Id == edoTask.OrderEdoRequest.Id)
				.ListAsync();

			var sourceCodes = productCodes
				.Where(x => x.SourceCode != null)
				.Select(x => x.SourceCode);

			var resultCodes = productCodes
				.Where(x => x.ResultCode != null)
				.Select(x => x.ResultCode);

			var codesToPreload = sourceCodes.Union(resultCodes).Distinct();
			await _trueMarkCodeRepository.PreloadCodes(codesToPreload, cancellationToken);

			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			try
			{
				var reasonForLeaving = edoTask.OrderEdoRequest.Order.Client.ReasonForLeaving;
				if(reasonForLeaving == ReasonForLeaving.Resale)
				{
					await _forResaleDocumentEdoTaskHandler.HandleTransferedForResaleFormalDocument(
						edoTask,
						trueMarkCodesChecker,
						cancellationToken
					);
				}
				else
				{
					await _forOwnNeedDocumentEdoTaskHandler.HandleTransferedForOwnNeedsFormalDocument(
						edoTask,
						trueMarkCodesChecker,
						cancellationToken
					);
				}
			}
			catch(EdoProblemException ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}
			catch(Exception ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}
		}

		// handle sent
		// Entry stage: Sending
		// Validated stage: Sending
		// Changed to: Sent
		// [событие от docflow]
		// (проверяет отправлен ли документ)
		public async Task HandleSent(int documentEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<DocumentEdoTask>(documentEdoTaskId, cancellationToken);

			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);
			var isValid = await _edoTaskValidator.Validate(edoTask, cancellationToken, trueMarkCodesChecker);
			if(!isValid)
			{
				return;
			}

			SentDocument(edoTask, cancellationToken);

			edoTask.Status = EdoTaskStatus.InProgress;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		private void SentDocument(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			edoTask.Stage = DocumentEdoTaskStage.Sent;
		}

		// handle accepted
		// Entry stage: Sent
		// Validated stage: Sent
		// Changed to: Accepted
		// [событие от docflow]
		// (проверяет принят ли документ)
		public async Task HandleAccepted(int documentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<OrderEdoDocument>(documentId, cancellationToken);
			if(document == null)
			{
				_logger.LogWarning("Документ №{DocumentId} не найден.", documentId);
				return;
			}

			var edoTask = await _uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);
			if(edoTask == null)
			{
				_logger.LogWarning("Задача ЭДО №{DocumentEdoTaskId} не найдена.", document.DocumentTaskId);
				return;
			}

			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);
			var isValid = await _edoTaskValidator.Validate(edoTask, cancellationToken, trueMarkCodesChecker);
			if(!isValid)
			{
				return;
			}

			AcceptDocument(edoTask, cancellationToken);

			edoTask.Status = EdoTaskStatus.Completed;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		private void AcceptDocument(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			edoTask.Stage = DocumentEdoTaskStage.Completed;
		}

		public async Task HandleCancelled(int documentId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var document = await _uow.Session.GetAsync<OrderEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке отмены документа №{DocumentId} не найден документ.", documentId);
			}

			var documentTask = await _uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);

			await _edoProblemRegistrar.RegisterCustomProblem<DocflowCouldNotBeCompleted>(
				documentTask, cancellationToken, "Документооборот был отменен");
		}

		public async Task HandleProblem(int documentId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var document = await _uow.Session.GetAsync<OrderEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке отмены документа №{DocumentId} не найден документ.", documentId);
			}

			var documentTask = await _uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);

			await _edoProblemRegistrar.RegisterCustomProblem<DocflowCouldNotBeCompleted>(
				documentTask, cancellationToken, "Возникла проблема с документооборотом, не завершился на стороне ЭДО провайдера");
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
