using Edo.Common;
using Edo.Problems;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Receipt.Dispatcher
{
	public class ReceiptEdoTaskHandler : IDisposable
	{
		private readonly ILogger<ReceiptEdoTaskHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ForOwnNeedsReceiptEdoTaskHandler _forOwnNeedsReceiptEdoTaskHandler;
		private readonly ResaleReceiptEdoTaskHandler _resaleReceiptEdoTaskHandler;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;

		public ReceiptEdoTaskHandler(
			ILogger<ReceiptEdoTaskHandler> logger,
			IUnitOfWork uow,
			ForOwnNeedsReceiptEdoTaskHandler forOwnNeedsReceiptEdoTaskHandler,
			ResaleReceiptEdoTaskHandler resaleReceiptEdoTaskHandler,
			EdoProblemRegistrar edoProblemRegistrar,
			ITrueMarkCodeRepository trueMarkCodeRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_forOwnNeedsReceiptEdoTaskHandler = forOwnNeedsReceiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(forOwnNeedsReceiptEdoTaskHandler));
			_resaleReceiptEdoTaskHandler = resaleReceiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(resaleReceiptEdoTaskHandler));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
		}

		public async Task HandleNew(int receiptEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<ReceiptEdoTask>(receiptEdoTaskId, cancellationToken);
			if(edoTask == null)
			{
				_logger.LogWarning("Задача Id {ReceiptEdoTaskId} не найдена.", receiptEdoTaskId);
				return;
			}

			try
			{
				if(edoTask.OrderEdoRequest.Order.Client.ReasonForLeaving == ReasonForLeaving.Resale)
				{
					await _resaleReceiptEdoTaskHandler.HandleNewReceipt(edoTask, cancellationToken);
				}
				else
				{
					await _forOwnNeedsReceiptEdoTaskHandler.HandleNewReceipt(edoTask, cancellationToken);
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

		public async Task HandleTransfered(int transferIterationId, CancellationToken cancellationToken)
		{
			var transferIteration = await _uow.Session.GetAsync<TransferEdoRequestIteration>(transferIterationId, cancellationToken);

			if(transferIteration == null)
			{
				_logger.LogWarning("Итерация трансфера Id {TransferIterationId} не найдена.", transferIterationId);
				return;
			}

			if(transferIteration.Status != TransferEdoRequestIterationStatus.Completed)
			{
				_logger.LogWarning("Пришло событие завершения трансфера, но трансфер не завершен, " +
					"статус: {TransferIterationStatus}.", transferIteration.Status);
				return;
			}

			if(transferIteration.Initiator != TransferInitiator.Receipt)
			{
				_logger.LogWarning("Пришло событие завершения трансфера, но инициатор трансфера не чек, " +
					"инициатор: {TransferIterationInitiator}.", transferIteration.Initiator);
				return;
			}

			var edoTask = transferIteration.OrderEdoTask.As<ReceiptEdoTask>();

			if(edoTask.Status == EdoTaskStatus.Completed)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
					"так как задача Id {ReceiptEdoTaskId} уже завершена", edoTask.Id);
				return;
			}

			if(edoTask.ReceiptStatus != EdoReceiptStatus.Transfering)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
					"так как задача Id {ReceiptEdoTaskId} находится не на стадии трансфера, " +
					"а на стадии {ReceiptEdoTaskReceiptStatus}",
					edoTask.Id, edoTask.ReceiptStatus);
				return;
			}

			// предзагрузка для ускорения
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

			try
			{
				if(edoTask.OrderEdoRequest.Order.Client.ReasonForLeaving == ReasonForLeaving.Resale)
				{
					await _resaleReceiptEdoTaskHandler.HandleTransferComplete(edoTask, cancellationToken);
				}
				else
				{
					await _forOwnNeedsReceiptEdoTaskHandler.HandleTransferComplete(edoTask, cancellationToken);
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

		public async Task HandleCompleted(int receiptEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<ReceiptEdoTask>(receiptEdoTaskId, cancellationToken);
			edoTask.Status = EdoTaskStatus.Completed;
			edoTask.ReceiptStatus = EdoReceiptStatus.Completed;
			
			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
