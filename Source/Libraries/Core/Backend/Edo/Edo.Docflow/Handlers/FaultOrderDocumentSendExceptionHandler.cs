using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Transport.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Docflow.Handlers
{
	public class FaultOrderDocumentSendExceptionHandler
	{
		private readonly ILogger<FaultOrderDocumentSendExceptionHandler> _logger;
		private readonly IMassTransitExceptionInfoFactory _exceptionInfoFactory;
		private readonly IGenericRepository<OrderEdoDocument> _edoDocumentRepository;
		private readonly IGenericRepository<DocumentEdoTask> _edoTaskRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly FaultEdoProblemRegistrar _problemRegistrar;

		public FaultOrderDocumentSendExceptionHandler(
			ILogger<FaultOrderDocumentSendExceptionHandler> logger,
			IMassTransitExceptionInfoFactory exceptionInfoFactory,
			IGenericRepository<OrderEdoDocument> edoDocumentRepository,
			IGenericRepository<DocumentEdoTask> edoTaskRepository,
			IEdoRepository edoRepository,
			FaultEdoProblemRegistrar problemRegistrar)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_exceptionInfoFactory = exceptionInfoFactory ?? throw new ArgumentNullException(nameof(exceptionInfoFactory));
			_edoDocumentRepository = edoDocumentRepository ?? throw new ArgumentNullException(nameof(edoDocumentRepository));
			_edoTaskRepository = edoTaskRepository ?? throw new ArgumentNullException(nameof(edoTaskRepository));
			_problemRegistrar = problemRegistrar ?? throw new ArgumentNullException(nameof(problemRegistrar));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
		}

		public async Task HandleAsync(
			IUnitOfWork uow,
			Fault<OrderDocumentSendEvent> fault,
			CancellationToken cancellationToken
		)
		{
			var documentId = fault.Message.OrderDocumentId;
			if(!fault.Exceptions.Any())
			{
				_logger.LogInformation(
					"Упавшее событие об отправке клиентского документа {DocumentId} без ошибок, пропускаем",
					documentId);

				return;
			}

			DocumentEdoTask documentTask = null;

			try
			{
				var document = _edoDocumentRepository.GetFirstOrDefault(uow, x => x.Id == documentId);
				if(document is null)
				{
					_logger.LogWarning("Документ {DocumentId} не найден", documentId);
					return;
				}

				var isNotValidStatus = _edoRepository.GetInProgressOrCompletedStatuses().Contains(document.Status);
				if(isNotValidStatus)
				{
					_logger.LogError("Документ {DocumentId} уже в работе, повторно отправить нельзя", documentId);
					return;
				}

				documentTask = _edoTaskRepository.GetFirstOrDefault(uow, x => x.Id == document.DocumentTaskId);
				if(documentTask is null)
				{
					_logger.LogWarning("Задача для документа {DocumentId} не найдена", documentId);
					return;
				}
				
				var exceptionInfos = _exceptionInfoFactory.Create(fault.Exceptions);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, documentTask, exceptionInfos, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке упавшего события отправки клиентского документа {DocumentId}",
					documentId);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, documentTask, ex, cancellationToken);
			}
		}
	}
}
