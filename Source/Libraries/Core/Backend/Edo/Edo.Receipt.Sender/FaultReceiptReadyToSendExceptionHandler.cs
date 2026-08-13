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

namespace Edo.Receipt.Sender
{
	public class FaultReceiptReadyToSendExceptionHandler
	{
		private readonly ILogger<FaultReceiptReadyToSendExceptionHandler> _logger;
		private readonly IMassTransitExceptionInfoFactory _exceptionInfoFactory;
		private readonly IGenericRepository<ReceiptEdoTask> _receiptTaskRepository;
		private readonly FaultEdoProblemRegistrar _problemRegistrar;

		public FaultReceiptReadyToSendExceptionHandler(
			ILogger<FaultReceiptReadyToSendExceptionHandler> logger,
			IMassTransitExceptionInfoFactory exceptionInfoFactory,
			IGenericRepository<ReceiptEdoTask> receiptTaskRepository,
			FaultEdoProblemRegistrar problemRegistrar
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_exceptionInfoFactory = exceptionInfoFactory ?? throw new ArgumentNullException(nameof(exceptionInfoFactory));
			_receiptTaskRepository = receiptTaskRepository ?? throw new ArgumentNullException(nameof(receiptTaskRepository));
			_problemRegistrar = problemRegistrar ?? throw new ArgumentNullException(nameof(problemRegistrar));
		}

		public async Task HandleAsync(
			IUnitOfWork uow,
			Fault<ReceiptReadyToSendEvent> fault,
			CancellationToken cancellationToken
		)
		{
			var edoTaskId = fault.Message.ReceiptEdoTaskId;
			if(!fault.Exceptions.Any())
			{
				_logger.LogInformation(
					"Упавшее событие о готовности чека к отправке по задаче {EdoTaskId} без ошибок, пропускаем",
					edoTaskId);

				return;
			}
			
			ReceiptEdoTask edoTask = null;

			try
			{
				edoTask = _receiptTaskRepository.GetFirstOrDefault(uow, x => x.Id == edoTaskId);
				
				if(edoTask is null)
				{
					_logger.LogWarning("Задача №{EdoTaskId} не найдена", edoTaskId);
					return;
				}
				
				var exceptionInfos = _exceptionInfoFactory.Create(fault.Exceptions);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, exceptionInfos, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке упавшего события о готовности чека к отправке по задаче {EdoTaskId}",
					edoTaskId);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, ex, cancellationToken);
			}
		}
	}
}
