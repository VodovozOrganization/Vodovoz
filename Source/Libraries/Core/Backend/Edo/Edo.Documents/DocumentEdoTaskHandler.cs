using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems.Validation;
using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	public class DocumentEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ForOwnNeedDocumentEdoTaskHandler _forOwnNeedDocumentEdoTaskHandler;
		private readonly ForResaleDocumentEdoTaskHandler _forResaleDocumentEdoTaskHandler;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly TrueMarkTaskCodesValidator _trueMarkTaskCodesValidator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly IBus _messageBus;

		public DocumentEdoTaskHandler(
			IUnitOfWork uow,
			ForOwnNeedDocumentEdoTaskHandler forOwnNeedDocumentEdoTaskHandler,
			ForResaleDocumentEdoTaskHandler forResaleDocumentEdoTaskHandler,
			EdoTaskValidator edoTaskValidator,
			TrueMarkTaskCodesValidator trueMarkTaskCodesValidator,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferRequestCreator transferRequestCreator,
			IBus messageBus
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_forOwnNeedDocumentEdoTaskHandler = forOwnNeedDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(forOwnNeedDocumentEdoTaskHandler));
			_forResaleDocumentEdoTaskHandler = forResaleDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(forResaleDocumentEdoTaskHandler));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
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
			var edoTask = transferIteration.OrderEdoTask.As<DocumentEdoTask>();

			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

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
			var edoTask = await _uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);

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

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
