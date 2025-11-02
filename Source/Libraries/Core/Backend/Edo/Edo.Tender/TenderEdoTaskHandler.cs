using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Tender
{
	/// <summary>
	/// Обработчик ЭДО задач по Тендеру
	/// </summary>
	public class TenderEdoTaskHandler : IDisposable
	{
		private readonly ILogger<TenderEdoTaskHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly ITrueMarkCodesValidator _trueMarkTaskCodesValidator;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly IBus _messageBus;

		public TenderEdoTaskHandler(
			ILogger<TenderEdoTaskHandler> logger,
			IUnitOfWork uow,
			EdoTaskValidator edoTaskValidator,
			EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
			EdoProblemRegistrar edoProblemRegistrar,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			ITrueMarkCodesValidator trueMarkTaskCodesValidator,
			TransferRequestCreator transferRequestCreator,
			IBus messageBus
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ??
			                                     throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		/// <summary>
		/// Обработка задачи по Тендеру и создание заявки на трансфер кодов
		/// </summary>
		/// <param name="tenderEdoTaskId"></param>
		/// <param name="cancellationToken"></param>
		public async Task HandleNew(int tenderEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<TenderEdoTask>(tenderEdoTaskId, cancellationToken);

			if(edoTask == null)
			{
				_logger.LogInformation("Задача Id {TenderEdoTaskId} не найдена", tenderEdoTaskId);
				return;
			}

			if(edoTask.Stage != TenderEdoTaskStage.New)
			{
				_logger.LogInformation("Задача Id {TenderEdoTaskId} уже в работе", tenderEdoTaskId);
				return;
			}

			if(edoTask.OrderEdoRequest == null)
			{
				_logger.LogInformation("Задача Id {TenderEdoTaskId} не имеет связи с ЭДО заявкой", tenderEdoTaskId);
				return;
			}

			var reasonForLeaving = edoTask.OrderEdoRequest.Order.Client.ReasonForLeaving;

			if(reasonForLeaving != ReasonForLeaving.Tender)
			{
				_logger.LogInformation("Задача Id {TenderEdoTaskId} не для Тендера", tenderEdoTaskId);
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

				await HandleNewTenderDocument(edoTask, trueMarkCodesChecker, cancellationToken);
			}
			catch(EdoProblemException ex)
			{
				var registered = await _edoProblemRegistrar.TryRegisterExceptionProblem(edoTask, ex, cancellationToken);
				if(!registered)
				{
					throw;
				}
			}
			catch(Exception ex) when(ex.InnerException is MySqlException)
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

		private async Task HandleNewTenderDocument(
			TenderEdoTask tenderEdoTask,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken
		)
		{
			foreach(var taskItem in tenderEdoTask.Items)
			{
				if(taskItem.ProductCode.ResultCode == null)
				{
					taskItem.ProductCode.ResultCode = taskItem.ProductCode.SourceCode;
					taskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
				}
			}

			trueMarkCodesChecker.ClearCache();
			var taskValidationResult = await _trueMarkTaskCodesValidator.ValidateAsync(
				tenderEdoTask,
				trueMarkCodesChecker,
				cancellationToken
			);

			if(!taskValidationResult.IsAllValid)
			{
				var affectedCodes = taskValidationResult
					.CodeResults.Where(x => !x.IsValid)
					.Select(x => x.EdoTaskItem);

				throw new EdoProblemException(new ResaleHasInvalidCodesException(), affectedCodes);
			}

			// создать трансфер
			var iteration = await _transferRequestCreator.CreateTransferRequests(
				_uow,
				tenderEdoTask,
				trueMarkCodesChecker,
				cancellationToken
			);

			tenderEdoTask.Status = EdoTaskStatus.InProgress;
			tenderEdoTask.Stage = TenderEdoTaskStage.Transfering;

			var message = new TransferRequestCreatedEvent { TransferIterationId = iteration.Id };

			await _uow.SaveAsync(tenderEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(message, cancellationToken);
		}

		// handle transfered
		// Entry stage: Transfering
		// Validated stage: Transfering
		// Changed to: Sending
		// [событие от transfer]
		// (проверяет выполнены ли переносы и переводит в Sending)
		public async Task HandleTransfered(int transferIterationId, CancellationToken cancellationToken)
		{
			var transferIteration = await _uow.Session.GetAsync<TransferEdoRequestIteration>(transferIterationId, cancellationToken);
			if(transferIteration == null)
			{
				_logger.LogInformation("Итерация трансфера Id {TransferIterationId} не найдена", transferIterationId);
				return;
			}

			var edoTask = transferIteration.OrderEdoTask.As<TenderEdoTask>();
			if(edoTask == null)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
				                       "так как задача Id {TenderEdoTaskId} не найдена", transferIteration.OrderEdoTask.Id);
				return;
			}

			if(edoTask.OrderEdoRequest == null)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
				                       "так как задача Id {TenderEdoTaskId} не связана ни с одной клиенсткой заявкой",
					transferIteration.OrderEdoTask.Id);
				return;
			}

			if(edoTask.Status == EdoTaskStatus.Completed)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
				                       "так как задача Id {TenderEdoTaskId} уже завершена", edoTask.Id);
				return;
			}

			if(edoTask.Stage != TenderEdoTaskStage.Transfering)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
				                       "так как задача Id {TenderEdoTaskId} находится не на стадии трансфера, " +
				                       "а на стадии {DocumentEdoTaskStage}",
					edoTask.Id, edoTask.Stage);
				return;
			}

			try
			{
				edoTask.Status = EdoTaskStatus.InProgress;
				edoTask.Stage = TenderEdoTaskStage.Sending;

				await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
				await _uow.CommitAsync(cancellationToken);
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

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
