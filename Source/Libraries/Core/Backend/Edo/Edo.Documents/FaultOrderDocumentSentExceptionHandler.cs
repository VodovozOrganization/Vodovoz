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

namespace Edo.Documents
{
	public class FaultOrderDocumentSentExceptionHandler
	{
		private readonly ILogger<FaultOrderDocumentSentExceptionHandler> _logger;
		private readonly IMassTransitExceptionInfoFactory _exceptionInfoFactory;
		private readonly FaultEdoProblemRegistrar _problemRegistrar;

		public FaultOrderDocumentSentExceptionHandler(
			ILogger<FaultOrderDocumentSentExceptionHandler> logger,
			IMassTransitExceptionInfoFactory exceptionInfoFactory,
			FaultEdoProblemRegistrar problemRegistrar
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_exceptionInfoFactory = exceptionInfoFactory ?? throw new ArgumentNullException(nameof(exceptionInfoFactory));
			_problemRegistrar = problemRegistrar ?? throw new ArgumentNullException(nameof(problemRegistrar));
		}
		
		public async Task HandleAsync(
			IUnitOfWork uow,
			Fault<OrderDocumentSentEvent> fault,
			CancellationToken cancellationToken
		)
		{
			var documentId = fault.Message.Id;
			if(!fault.Exceptions.Any())
			{
				_logger.LogInformation(
					"Упавшее событие об отправке документа {DocumentId} без ошибок, пропускаем",
					documentId);

				return;
			}
			
			DocumentEdoTask edoTask = null;

			try
			{
				var document = await uow.Session.GetAsync<OrderEdoDocument>(documentId, cancellationToken);
				if(document is null)
				{
					_logger.LogWarning("Документ №{DocumentId} не найден", documentId);
					return;
				}

				edoTask = await uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);
				
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
					"Ошибка при обработке упавшего события об отправке документа {DocumentId}",
					documentId);
				await _problemRegistrar.TryRegisterExceptionProblem(uow, edoTask, ex, cancellationToken);
			}
		}
	}
}
