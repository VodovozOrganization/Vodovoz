using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems.Validation;
using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	public class DocumentEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly TrueMarkTaskCodesValidator _trueMarkTaskCodesValidator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly IBus _messageBus;
		private readonly IUnitOfWork _uow;

		public DocumentEdoTaskHandler(
			IUnitOfWorkFactory uowFactory,
			EdoTaskValidator edoTaskValidator,
			TrueMarkTaskCodesValidator trueMarkTaskCodesValidator,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferRequestCreator transferRequestCreator,
			IBus messageBus
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_uow = uowFactory.CreateWithoutRoot();
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
			var taskValidationResult = await _trueMarkTaskCodesValidator.ValidateAsync(
					edoTask,
					trueMarkCodesChecker,
					cancellationToken
				);

			if(!taskValidationResult.IsAllValid)
			{
				throw new InvalidOperationException("Не все коды в задаче прошли проверку в честном знаке.");
			}

			var transferRequired = !taskValidationResult.ReadyToSell;

			object message = null;
			if(transferRequired)
			{
				var iteration = await _transferRequestCreator.CreateTransferRequests(
					_uow,
					edoTask,
					trueMarkCodesChecker,
					cancellationToken
				);
				edoTask.Status = EdoTaskStatus.InProgress;
				edoTask.Stage = DocumentEdoTaskStage.Transfering;
				message = new TransferRequestCreatedEvent { TransferIterationId = iteration.Id };
			}
			else
			{
				var customerDocument = await SendDocument(edoTask, cancellationToken);
				edoTask.Status = EdoTaskStatus.InProgress;
				edoTask.Stage = DocumentEdoTaskStage.Sending;
				message = new OrderDocumentSendEvent { OrderDocumentId = customerDocument.Id };
			}

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(message, cancellationToken);
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

		private async Task CreateTransferRequest(
			DocumentEdoTask edoTask, 
			EdoTaskItemTrueMarkStatusProvider trueMarkCodeChecker, 
			CancellationToken cancellationToken)
		{
			
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
			var edoTask = (DocumentEdoTask)transferIteration.OrderEdoTask;

			var trueMarkCodesChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);
			var isValid = await _edoTaskValidator.Validate(edoTask, cancellationToken, trueMarkCodesChecker);
			if(!isValid)
			{
				return;
			}

			var codeStatuses = await trueMarkCodesChecker.GetItemsStatusesAsync(cancellationToken);
			var organizationTo = edoTask.OrderEdoRequest.Order.Contract.Organization;

			foreach(var codeStatus in codeStatuses.Values)
			{
				if(codeStatus.ProductInstanceStatus == null)
				{
					throw new EdoException($"Коды в задаче №{edoTask.Id} не были проверены в честном знаке после перемещения. " +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}

				if(codeStatus.ProductInstanceStatus.Status == null || codeStatus.ProductInstanceStatus.Status.Value != ProductInstanceStatusEnum.Introduced)
				{
					throw new EdoException($"Все коды в задаче №{edoTask.Id} должны иметь статус {ProductInstanceStatusEnum.Introduced}. " +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}

				if(codeStatus.ProductInstanceStatus.OwnerInn != organizationTo.INN)
				{
					throw new EdoException($"Все коды в задаче №{edoTask.Id} должны быть на балансе организации из клиенской заявки. " +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}
			}

			// Проверка через ЧЗ всех кодов, с отметкой о прохождении проверки

			var customerDocument = await SendDocument(edoTask, cancellationToken);
			var message = new OrderDocumentSendEvent { OrderDocumentId = customerDocument.Id };

			edoTask.Status = EdoTaskStatus.InProgress;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(message, cancellationToken);
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
