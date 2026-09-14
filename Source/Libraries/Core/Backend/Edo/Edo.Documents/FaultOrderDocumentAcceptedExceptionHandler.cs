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
	public class FaultOrderDocumentAcceptedExceptionHandler
	{
		private readonly ILogger<FaultOrderDocumentAcceptedExceptionHandler> _logger;
		private readonly IMassTransitExceptionInfoFactory _exceptionInfoFactory;
		private readonly IGenericRepository<OrderEdoDocument> _edoDocumentRepository;
		private readonly IGenericRepository<DocumentEdoTask> _edoTaskRepository;
		private readonly FaultEdoProblemRegistrar _problemRegistrar;

		public FaultOrderDocumentAcceptedExceptionHandler(
			ILogger<FaultOrderDocumentAcceptedExceptionHandler> logger,
			IMassTransitExceptionInfoFactory exceptionInfoFactory,
			IGenericRepository<OrderEdoDocument> edoDocumentRepository,
			IGenericRepository<DocumentEdoTask> edoTaskRepository,
			FaultEdoProblemRegistrar problemRegistrar
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_exceptionInfoFactory = exceptionInfoFactory ?? throw new ArgumentNullException(nameof(exceptionInfoFactory));
			_edoDocumentRepository = edoDocumentRepository ?? throw new ArgumentNullException(nameof(edoDocumentRepository));
			_edoTaskRepository = edoTaskRepository ?? throw new ArgumentNullException(nameof(edoTaskRepository));
			_problemRegistrar = problemRegistrar ?? throw new ArgumentNullException(nameof(problemRegistrar));
		}
		
		public async Task HandleAsync(
			IUnitOfWork uow,
			Fault<OrderDocumentAcceptedEvent> fault,
			CancellationToken cancellationToken
		)
		{
			var documentId = fault.Message.DocumentId;
			if(!fault.Exceptions.Any())
			{
				_logger.LogInformation(
					"Упавшее событие о подтверждении клиентского документа {DocumentId} без ошибок, пропускаем",
					documentId);

				return;
			}
			
			DocumentEdoTask edoTask = null;

			try
			{
				var document = _edoDocumentRepository.GetFirstOrDefault(uow, x => x.Id == documentId);
				if(document is null)
				{
					_logger.LogWarning("Документ №{DocumentId} не найден", documentId);
					return;
				}

				edoTask = _edoTaskRepository.GetFirstOrDefault(uow, x => x.Id == document.DocumentTaskId);
				
				if(edoTask is null)
				{
					_logger.LogWarning("Задача ЭДО №{EdoTaskId} не найдена", document.DocumentTaskId);
					return;
				}

				if(edoTask.FormalEdoRequest is null)
				{
					_logger.LogInformation("Задача Id {EdoTaskId} не имеет связи с ЭДО заявкой", document.DocumentTaskId);
					return;
				}
				
				var exceptionInfos = _exceptionInfoFactory.Create(fault.Exceptions);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, exceptionInfos, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке упавшего события о подтверждении клиентского документа {DocumentId}",
					documentId);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, ex, cancellationToken);
			}
		}
	}
}
