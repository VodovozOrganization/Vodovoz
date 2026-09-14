using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Transport.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Documents
{
	public class FaultTransferCompleteExceptionHandler
	{
		private readonly ILogger<FaultTransferCompleteExceptionHandler> _logger;
		private readonly IGenericRepository<TransferEdoRequestIteration> _transferIterationRepository;
		private readonly IMassTransitExceptionInfoFactory _exceptionInfoFactory;
		private readonly FaultEdoProblemRegistrar _problemRegistrar;

		public FaultTransferCompleteExceptionHandler(
			ILogger<FaultTransferCompleteExceptionHandler> logger,
			IGenericRepository<TransferEdoRequestIteration> transferIterationRepository,
			IMassTransitExceptionInfoFactory exceptionInfoFactory,
			FaultEdoProblemRegistrar problemRegistrar
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferIterationRepository = transferIterationRepository ?? throw new ArgumentNullException(nameof(transferIterationRepository));
			_exceptionInfoFactory = exceptionInfoFactory ?? throw new ArgumentNullException(nameof(exceptionInfoFactory));
			_problemRegistrar = problemRegistrar ?? throw new ArgumentNullException(nameof(problemRegistrar));
		}

		public async Task HandleAsync(
			IUnitOfWork uow,
			Fault<TransferCompleteEvent> fault,
			CancellationToken cancellationToken
		)
		{
			var transferIterationId = fault.Message.TransferIterationId;
			
			if(!fault.Exceptions.Any())
			{
				_logger.LogInformation(
					"Упавшее событие завершения трансфера с итерацией {TransferIterationId} без ошибок, пропускаем",
					transferIterationId);

				return;
			}

			EdoTask edoTask = null;
			
			try
			{
				var transferIterations = await _transferIterationRepository
					.GetAsync(uow, x => x.Id == transferIterationId, cancellationToken: cancellationToken);

				var transferIteration = transferIterations.Value.FirstOrDefault();

				//TODO вынести условия в отдельные или совместный обработчик, т.к. они есть также в DocumentEdoTaskHandler
				if(transferIteration is null)
				{
					_logger.LogInformation("Итерация трансфера Id {TransferIterationId} не найдена", transferIterationId);
					return;
				}

				if(!TryGetTaskAndCheck(fault, transferIteration, ref edoTask))
				{
					return;
				}

				if(edoTask is null)
				{
					_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
						"так как задача Id {EdoTaskId} не найдена", transferIteration.OrderEdoTask.Id);

					return;
				}
				
				if(edoTask.Status is EdoTaskStatus.Completed)
				{
					_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
						"так как задача Id {EdoTaskId} уже завершена", edoTask.Id);
					
					return;
				}

				var exceptionInfos = _exceptionInfoFactory.Create(fault.Exceptions);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, exceptionInfos, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке упавшего события завершения трансфера с итерацией {TransferIterationId}",
					transferIterationId);
				
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, ex, cancellationToken);
			}
		}

		private bool TryGetTaskAndCheck(
			Fault<TransferCompleteEvent> fault,
			TransferEdoRequestIteration transferIteration,
			ref EdoTask edoTask)
		{
			switch(fault.Message.TransferInitiator)
			{
				case TransferInitiator.Document:
					var documentEdoTask = transferIteration.OrderEdoTask.As<DocumentEdoTask>();
					if(!CheckDocumentTask(documentEdoTask))
					{
						return false;
					};
					edoTask = documentEdoTask;
					break;
				case TransferInitiator.Receipt:
					var receiptEdoTask = transferIteration.OrderEdoTask.As<ReceiptEdoTask>();
					if(!CheckReceiptTask(receiptEdoTask))
					{
						return false;
					};
					edoTask = receiptEdoTask;
					break;
				case TransferInitiator.Tender:
					var tenderEdoTask = transferIteration.OrderEdoTask.As<TenderEdoTask>();
					if(!CheckTenderTask(tenderEdoTask))
					{
						return false;
					};
					edoTask = tenderEdoTask;
					break;
			}

			return true;
		}

		private bool CheckTenderTask(TenderEdoTask tenderEdoTask)
		{
			if(tenderEdoTask is null)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, т.к. не удалось определить задачу");
				return false;
			}
			
			if(tenderEdoTask.Stage != TenderEdoTaskStage.Transfering)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, " +
					"так как задача Id {TenderEdoTaskId} находится не на стадии трансфера, " +
					"а на стадии {DocumentEdoTaskStage}",
					tenderEdoTask.Id, tenderEdoTask.Stage);
				
				return false;
			}

			return true;
		}

		private bool CheckReceiptTask(ReceiptEdoTask receiptEdoTask)
		{
			if(receiptEdoTask is null)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, т.к. не удалось определить задачу");
				return false;
			}
			
			if(receiptEdoTask.ReceiptStatus != EdoReceiptStatus.Transfering)
			{
				_logger.LogInformation(
					"Невозможно выполнить завершение трансфера, так как задача Id {ReceiptEdoTaskId} " +
					"находится не на стадии трансфера, а на стадии {ReceiptEdoTaskReceiptStatus}",
					receiptEdoTask.Id,
					receiptEdoTask.ReceiptStatus);

				return false;
			}
			
			return true;
		}

		private bool CheckDocumentTask(DocumentEdoTask documentEdoTask)
		{
			if(documentEdoTask is null)
			{
				_logger.LogInformation("Невозможно выполнить завершение трансфера, т.к. не удалось определить задачу");
				return false;
			}
			
			if(documentEdoTask.Stage != DocumentEdoTaskStage.Transfering)
			{
				_logger.LogInformation(
					"Невозможно выполнить завершение трансфера, так как задача Id {EdoTaskId} " +
					"находится не на стадии трансфера, а на стадии {DocumentEdoTaskStage}",
					documentEdoTask.Id,
					documentEdoTask.Stage);

				return false;
			}

			return true;
		}
	}
}
