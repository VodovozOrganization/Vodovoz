using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.TaskValidation;
using MassTransit;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	public class DocumentEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly EdoTaskMainValidator _validator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly IBus _messageBus;
		private readonly IUnitOfWork _uow;

		public DocumentEdoTaskHandler(
			IUnitOfWorkFactory uowFactory,
			EdoTaskMainValidator validator,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferRequestCreator transferRequestCreator,
			IBus messageBus
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
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
			// TEST
			// проверяем все коды как МН
			var trueMarkApiClient = new TrueMarkApiClient("https://test-mn-truemarkapi.dev.vod.qsolution.ru/", "test");
			var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
			if(!valid)
			{
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			//Необходимо проверять принадлежность кодов, возможно перемещение не нужно
			object message = null;
			if(IsFormalDocument(edoTask))
			{
				await CreateTransferRequest(edoTask, trueMarkCodeChecker, cancellationToken);
				message = new TransferRequestCreatedEvent { EdoTaskId = edoTask.Id };
			}
			else
			{
				var customerDocument = await SendDocument(edoTask, cancellationToken);
				message = new OrderDocumentSendEvent { OrderDocumentId = customerDocument.Id };
			}

			edoTask.Status = EdoTaskStatus.InProgress;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

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
			await _transferRequestCreator.CreateTransferRequests(_uow, edoTask, trueMarkCodeChecker, cancellationToken);
			edoTask.Stage = DocumentEdoTaskStage.Transfering;
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
		public async Task HandleTransfered(int documentEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<DocumentEdoTask>(documentEdoTaskId, cancellationToken);
			// TEST
			// проверяем все коды как ВВ
			var trueMarkApiClient = new TrueMarkApiClient("https://test-vv-truemarkapi.dev.vod.qsolution.ru/", "test");
			var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
			if(!valid)
			{
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			var codeStatuses = await trueMarkCodeChecker.GetItemsStatusesAsync(cancellationToken);
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
			// TEST
			// ТУТ НЕ ВАЖНО КАКОЙ АПИ
			var trueMarkApiClient = new TrueMarkApiClient("https://test-vv-truemarkapi.dev.vod.qsolution.ru/", "test");
			var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
			if(!valid)
			{
				await _uow.CommitAsync(cancellationToken);
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

			// TEST
			// ТУТ НЕ ВАЖНО КАКОЙ АПИ
			var trueMarkApiClient = new TrueMarkApiClient("https://test-vv-truemarkapi.dev.vod.qsolution.ru/", "test");
			var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
			if(!valid)
			{
				await _uow.CommitAsync(cancellationToken);
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

		private async Task<bool> Validate(DocumentEdoTask edoTask, EdoTaskItemTrueMarkStatusProvider itemStatusProvider, CancellationToken cancellationToken)
		{
			var context = new EdoTaskValidationContext();
			context.AddService(itemStatusProvider);
			var results = await _validator.ValidateAsync(edoTask, cancellationToken, context);
			await UpdateValidationResults(edoTask, results, cancellationToken);

			if(results.IsValid)
			{
				return true;
			}

			switch(results.Importance)
			{
				case EdoValidationImportance.Waiting:
					edoTask.Status = EdoTaskStatus.Waiting;
					break;
				case EdoValidationImportance.Problem:
					edoTask.Status = EdoTaskStatus.Problem;
					break;
				case null:
					throw new InvalidOperationException($"Результаты валидации обязаны содержать {nameof(EdoValidationImportance)}, если результаты не валидны.");
				default:
					throw new InvalidOperationException($"Неизвестное значение важности валидации {nameof(EdoValidationImportance)}.");
			}

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			return false;
		}

		private async Task UpdateValidationResults(
			DocumentEdoTask edoTask,
			EdoValidationResults validationResults,
			CancellationToken cancellationToken
			)
		{
			foreach(var validationResult in validationResults.Results)
			{
				var problem = edoTask.Problems.FirstOrDefault(x => x.ValidatorName == validationResult.Validator.Name);
				if(problem == null && validationResult.IsValid)
				{
					continue;
				}

				if(problem == null)
				{
					problem = new EdoTaskProblem
					{
						ValidatorName = validationResult.Validator.Name,
						State = TaskProblemState.Active,
						EdoTask = edoTask,
						TaskItems = new ObservableList<EdoTaskItem>(validationResult.ProblemItems)
					};
				}

				if(validationResult.IsValid)
				{
					problem.State = TaskProblemState.Solved;
				}
				else
				{
					problem.TaskItems.Clear();
					foreach(var problemItem in validationResult.ProblemItems)
					{
						problem.TaskItems.Add(problemItem);
					}
				}

				await _uow.SaveAsync(problem, cancellationToken: cancellationToken);
			}
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
