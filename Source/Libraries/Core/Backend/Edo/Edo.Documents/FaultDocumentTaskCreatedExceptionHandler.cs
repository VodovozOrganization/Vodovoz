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
	public class FaultDocumentTaskCreatedExceptionHandler
	{
		private readonly ILogger<FaultDocumentTaskCreatedExceptionHandler> _logger;
		private readonly IMassTransitExceptionInfoFactory _exceptionInfoFactory;
		private readonly IGenericRepository<DocumentEdoTask> _edoTaskRepository;
		private readonly FaultEdoProblemRegistrar _problemRegistrar;

		public FaultDocumentTaskCreatedExceptionHandler(
			ILogger<FaultDocumentTaskCreatedExceptionHandler> logger,
			IMassTransitExceptionInfoFactory exceptionInfoFactory,
			IGenericRepository<DocumentEdoTask> edoTaskRepository,
			FaultEdoProblemRegistrar problemRegistrar
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_exceptionInfoFactory = exceptionInfoFactory ?? throw new ArgumentNullException(nameof(exceptionInfoFactory));
			_edoTaskRepository = edoTaskRepository ?? throw new ArgumentNullException(nameof(edoTaskRepository));
			_problemRegistrar = problemRegistrar ?? throw new ArgumentNullException(nameof(problemRegistrar));
		}
		
		public async Task HandleAsync(
			IUnitOfWork uow,
			Fault<DocumentTaskCreatedEvent> fault,
			CancellationToken cancellationToken
		)
		{
			var edoTaskId = fault.Message.Id;
			if(!fault.Exceptions.Any())
			{
				_logger.LogInformation(
					"Упавшее событие о созданной задаче {EdoTaskId} по ЭДО документу без ошибок, пропускаем",
					edoTaskId);

				return;
			}
			
			DocumentEdoTask edoTask = null;

			try
			{
				edoTask = _edoTaskRepository.GetFirstOrDefault(uow, x => x.Id == edoTaskId);

				if(edoTask is null)
				{
					_logger.LogInformation("Задача Id {EdoTaskId} не найдена", edoTaskId);
					return;
				}

				if(edoTask.Stage != DocumentEdoTaskStage.New)
				{
					_logger.LogInformation("Задача Id {EdoTaskId} уже в работе", edoTaskId);
					return;
				}

				if(edoTask.FormalEdoRequest is null)
				{
					_logger.LogInformation("Задача Id {EdoTaskId} не имеет связи с ЭДО заявкой", edoTaskId);
					return;
				}
				
				var exceptionInfos = _exceptionInfoFactory.Create(fault.Exceptions);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, exceptionInfos, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке упавшего события о созданной задаче {EdoTaskId}",
					edoTaskId);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, ex, cancellationToken);
			}
		}
	}
}
