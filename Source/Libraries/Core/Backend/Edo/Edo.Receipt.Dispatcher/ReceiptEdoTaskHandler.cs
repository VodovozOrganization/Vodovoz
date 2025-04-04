﻿using Edo.Problems;
using Edo.Problems.Exception;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Receipt.Dispatcher
{
	public class ReceiptEdoTaskHandler : IDisposable
	{
		private readonly ILogger<ReceiptEdoTaskHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ForOwnNeedsReceiptEdoTaskHandler _forOwnNeedsReceiptEdoTaskHandler;
		private readonly ResaleReceiptEdoTaskHandler _resaleReceiptEdoTaskHandler;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;

		public ReceiptEdoTaskHandler(
			ILogger<ReceiptEdoTaskHandler> logger,
			IUnitOfWork uow,
			ForOwnNeedsReceiptEdoTaskHandler forOwnNeedsReceiptEdoTaskHandler,
			ResaleReceiptEdoTaskHandler resaleReceiptEdoTaskHandler,
			EdoProblemRegistrar edoProblemRegistrar
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_forOwnNeedsReceiptEdoTaskHandler = forOwnNeedsReceiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(forOwnNeedsReceiptEdoTaskHandler));
			_resaleReceiptEdoTaskHandler = resaleReceiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(resaleReceiptEdoTaskHandler));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
		}

		public async Task HandleNew(int receiptEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<ReceiptEdoTask>(receiptEdoTaskId, cancellationToken);
			try
			{
				if(edoTask.OrderEdoRequest.Order.Client.ReasonForLeaving == ReasonForLeaving.Resale)
				{
					await _resaleReceiptEdoTaskHandler.HandleResaleReceipt(edoTask, cancellationToken);
				}
				else
				{
					await _forOwnNeedsReceiptEdoTaskHandler.HandleForOwnNeedsReceipt(edoTask, cancellationToken);
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

		public async Task HandleTransfered(int transferIterationId, CancellationToken cancellationToken)
		{
			var transferIteration = await _uow.Session.GetAsync<TransferEdoRequestIteration>(transferIterationId, cancellationToken);

			if(transferIteration.Status != TransferEdoRequestIterationStatus.Completed)
			{
				_logger.LogWarning($"Пришло событие завершения трансфера, но трансфер не завершен, " +
					$"статус: {transferIteration.Status}.");
				return;
			}

			if(transferIteration.Initiator != TransferInitiator.Receipt)
			{
				_logger.LogWarning($"Пришло событие завершения трансфера, но инициатор трансфера не чек, " +
					$"инициатор: {transferIteration.Initiator}.");
				return;
			}

			var receiptEdoTask = transferIteration.OrderEdoTask.As<ReceiptEdoTask>();
			if(receiptEdoTask.OrderEdoRequest.Order.Client.ReasonForLeaving == ReasonForLeaving.Resale)
			{
				await _resaleReceiptEdoTaskHandler.HandleTransferComplete(receiptEdoTask, cancellationToken);
			}
			else
			{
				await _forOwnNeedsReceiptEdoTaskHandler.HandleTransferComplete(receiptEdoTask, cancellationToken);
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
