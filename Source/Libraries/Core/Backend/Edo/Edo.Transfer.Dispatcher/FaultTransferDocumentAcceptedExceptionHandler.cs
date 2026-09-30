using System;
using System.Collections.Generic;
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
	public class FaultTransferDocumentAcceptedExceptionHandler
	{
		private readonly ILogger<FaultTransferDocumentAcceptedExceptionHandler> _logger;
		private readonly IGenericRepository<TransferEdoTask> _transferTaskRepository;
		private readonly IMassTransitExceptionInfoFactory _exceptionInfoFactory;
		private readonly FaultEdoProblemRegistrar _problemRegistrar;

		public FaultTransferDocumentAcceptedExceptionHandler(
			ILogger<FaultTransferDocumentAcceptedExceptionHandler> logger,
			IGenericRepository<TransferEdoTask> transferTaskRepository,
			IMassTransitExceptionInfoFactory exceptionInfoFactory,
			FaultEdoProblemRegistrar problemRegistrar
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_exceptionInfoFactory = exceptionInfoFactory ?? throw new ArgumentNullException(nameof(exceptionInfoFactory));
			_problemRegistrar = problemRegistrar ?? throw new ArgumentNullException(nameof(problemRegistrar));
		}
		
		public async Task HandleAsync(
			IUnitOfWork uow,
			Fault<TransferDocumentAcceptedEvent> fault,
			CancellationToken cancellationToken
		)
		{
			var documentId = fault.Message.DocumentId;
			
			if(!fault.Exceptions.Any())
			{
				_logger.LogInformation(
					"Упавшее событие подтверждения трансфера по документу {TransferDocumentId} без ошибок, пропускаем",
					documentId);
				
				return;
			}

			IEnumerable<int> orderTaskIds = null;
			
			try
			{
				var transferTasks = await _transferTaskRepository
					.GetAsync(uow, x => x.Id == documentId, cancellationToken: cancellationToken);
				
				var transferTask = transferTasks.Value.FirstOrDefault();

				if(transferTask is null)
				{
					_logger.LogWarning("Не обнаружена задача по трансферу для документа трансфера №{TransferDocumentId}", documentId);
					return;
				}
				
				if(transferTask.Status == EdoTaskStatus.Completed)
				{
					_logger.LogWarning("При обработке принятия документа трансфера №{documentId} обнаружено, что трансфер уже завершен", documentId);
					return;
				}
				
				orderTaskIds = transferTask.TransferEdoRequests
					.Select(x => x.Iteration.OrderEdoTask.Id)
					.Distinct();
				
				//TODO может нужно проверять еще и таски на возможность перевода в проблему
				var exceptionInfos = _exceptionInfoFactory.Create(fault.Exceptions);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, orderTaskIds, exceptionInfos, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке упавшего события подтверждения трансфера по документу {TransferDocumentId}",
					documentId);
				
				await _problemRegistrar.TryRegisterExceptionProblem(uow, orderTaskIds, new []{ _exceptionInfoFactory.Create(ex) }, cancellationToken);
			}
		}
	}
}
