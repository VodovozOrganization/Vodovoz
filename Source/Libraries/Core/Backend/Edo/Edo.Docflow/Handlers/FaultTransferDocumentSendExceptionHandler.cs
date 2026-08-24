using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Transport.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Docflow.Handlers
{
	public class FaultTransferDocumentSendExceptionHandler
	{
		private readonly ILogger<FaultTransferDocumentSendExceptionHandler> _logger;
		private readonly IMassTransitExceptionInfoFactory _exceptionInfoFactory;
		private readonly IGenericRepository<TransferEdoDocument> _edoDocumentRepository;
		private readonly IGenericRepository<TransferEdoTask> _transferTaskRepository;
		private readonly IGenericRepository<DocumentEdoTask> _edoTaskRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly FaultEdoProblemRegistrar _problemRegistrar;

		public FaultTransferDocumentSendExceptionHandler(
			ILogger<FaultTransferDocumentSendExceptionHandler> logger,
			IMassTransitExceptionInfoFactory exceptionInfoFactory,
			IGenericRepository<TransferEdoDocument> edoDocumentRepository,
			IGenericRepository<TransferEdoTask> transferTaskRepository,
			IGenericRepository<DocumentEdoTask> edoTaskRepository,
			IEdoRepository edoRepository,
			FaultEdoProblemRegistrar problemRegistrar
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_exceptionInfoFactory = exceptionInfoFactory ?? throw new ArgumentNullException(nameof(exceptionInfoFactory));
			_edoDocumentRepository = edoDocumentRepository ?? throw new ArgumentNullException(nameof(edoDocumentRepository));
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_edoTaskRepository = edoTaskRepository ?? throw new ArgumentNullException(nameof(edoTaskRepository));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_problemRegistrar = problemRegistrar ?? throw new ArgumentNullException(nameof(problemRegistrar));
		}

		public async Task HandleAsync(
			IUnitOfWork uow,
			Fault<TransferDocumentSendEvent> fault,
			CancellationToken cancellationToken
		)
		{
			var documentId = fault.Message.TransferDocumentId;
			if(!fault.Exceptions.Any())
			{
				_logger.LogInformation(
					"Упавшее событие об отправке трансфера {DocumentId} без ошибок, пропускаем",
					documentId);

				return;
			}

			IEnumerable<int> orderTaskIds = null;

			try
			{
				var document = _edoDocumentRepository.GetFirstOrDefault(uow, x => x.Id == documentId);
				if(document is null)
				{
					_logger.LogWarning("Трансфер документ {documentId} не найден", documentId);
					return;
				}

				var inProgress = _edoRepository.GetInProgressEdoDocumentStatuses().Contains(document.Status);
				if(inProgress)
				{
					_logger.LogError("Документ {DocumentId} уже в работе, повторно отправить нельзя", documentId);
					return;
				}

				var transferTask = _transferTaskRepository.GetFirstOrDefault(uow, x => x.Id == document.TransferTaskId);
				if(transferTask is null)
				{
					_logger.LogWarning("Трансфер задача для документа {DocumentId} не найдена", documentId);
					return;
				}

				orderTaskIds = transferTask.TransferEdoRequests
					.Select(x => x.Iteration.OrderEdoTask.Id)
					.Distinct();
				
				var exceptionInfos = _exceptionInfoFactory.Create(fault.Exceptions);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, orderTaskIds, exceptionInfos, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке упавшего события об отправке трансфера {DocumentId}",
					documentId);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, orderTaskIds, new []{ _exceptionInfoFactory.Create(ex) }, cancellationToken);
			}
		}
	}
}
