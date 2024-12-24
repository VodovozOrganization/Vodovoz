using Edo.TaskValidation;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	public class DocumentEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly EdoTaskMainValidator _validator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly IUnitOfWork _uow;

		public DocumentEdoTaskHandler(
			IUnitOfWorkFactory uowFactory,
			EdoTaskMainValidator validator,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			TransferRequestCreator transferRequestCreator
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
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
			var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
			if(!valid)
			{
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			if(IsFormalDocument(edoTask))
			{
				await TransferDocument(edoTask, trueMarkCodeChecker, cancellationToken);
			}
			else
			{
				SendDocument(edoTask, cancellationToken);
			}

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
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

		private async Task TransferDocument(DocumentEdoTask edoTask, EdoTaskItemTrueMarkStatusProvider trueMarkCodeChecker, CancellationToken cancellationToken)
		{
			await _transferRequestCreator.CreateTransferRequests(edoTask, trueMarkCodeChecker, cancellationToken);
			edoTask.Stage = DocumentEdoTaskStage.Transfering;
		}

		private void SendDocument(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			edoTask.Stage = DocumentEdoTaskStage.Sending;
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
			var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
			if(!valid)
			{
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			var orderEdoRequest = edoTask.CustomerEdoRequest as OrderEdoRequest;
			if(orderEdoRequest == null)
			{
				throw new EdoException($"Трансфер кодов недоступен для заявки типа {edoTask.TaskType}. " +
					$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
			}

			var codeStatuses = await trueMarkCodeChecker.GetItemsStatusesAsync(cancellationToken);
			var organizationTo = orderEdoRequest.Order.Contract.Organization;

			foreach(var codeStatus in codeStatuses.Values)
			{
				if(codeStatus.Status == null)
				{
					throw new EdoException($"Коды в задаче №{edoTask.Id} не были проверены в честном знаке после перемещения. " +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}

				if(codeStatus.Status.Status == null || codeStatus.Status.Status.Value != ProductInstanceStatusEnum.Introduced)
				{
					throw new EdoException($"Все коды в задаче №{edoTask.Id} должны иметь статус {ProductInstanceStatusEnum.Introduced}. " +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}

				if(codeStatus.Status.OwnerInn != organizationTo.INN)
				{
					throw new EdoException($"Все коды в задаче №{edoTask.Id} должны быть на балансе организации из клиенской заявки. " +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}
			}

			SendDocument(edoTask, cancellationToken);

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
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
			var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
			if(!valid)
			{
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			SentDocument(edoTask, cancellationToken);

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
		public async Task HandleAccepted(int documentEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<DocumentEdoTask>(documentEdoTaskId, cancellationToken);
			var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
			if(!valid)
			{
				await _uow.CommitAsync(cancellationToken);
				return;
			}

			AcceptDocument(edoTask, cancellationToken);

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		private void AcceptDocument(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			edoTask.Stage = DocumentEdoTaskStage.Done;
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
					problem.TaskItems.AddRange(validationResult.ProblemItems);
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
