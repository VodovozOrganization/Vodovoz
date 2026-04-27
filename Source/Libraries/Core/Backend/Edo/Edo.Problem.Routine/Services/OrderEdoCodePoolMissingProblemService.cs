using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	public class OrderEdoCodePoolMissingProblemService
	{
		private const string _problemSourceName = "EdoCodePoolMissingCodeException";

		private readonly ILogger<OrderEdoCodePoolMissingProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IServiceProvider _serviceProvider;
		private readonly IBus _messageBus;
		private readonly IEdoTaskValidator _edoCodePoolValidator;

		public OrderEdoCodePoolMissingProblemService(
			ILogger<OrderEdoCodePoolMissingProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEnumerable<IEdoTaskValidator> validators,
			IServiceProvider serviceProvider,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoCodePoolValidator = (validators ?? throw new ArgumentNullException(nameof(validators)))
				.FirstOrDefault(v => v.Name == _problemSourceName);
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task TryResumeTask(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			try
			{
				await TryResumeTaskAsync(edoTask, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке задачи ЭДО {EdoTaskId}", edoTask.Id);
			}
		}

		private async Task TryResumeTaskAsync(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(_edoCodePoolValidator != null)
			{
				var validationResult = await _edoCodePoolValidator.ValidateAsync(edoTask, _serviceProvider, cancellationToken);

				if(!validationResult.IsValid)
				{
					_logger.LogDebug(
						"Задача ЭДО {EdoTaskId}: пул кодов не прошел проверку по заказу №{OrderId}",
						edoTask.Id,
						edoTask.FormalEdoRequest.Order.Id);
					return;
				}
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId}: пул кодов не прошел проверку по заказу №{OrderId}",
				edoTask.Id,
				edoTask.FormalEdoRequest.Order.Id);

			await PublishResumeEvent(edoTask, cancellationToken);
		}

		private async Task PublishResumeEvent(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			switch(edoTask)
			{
				case DocumentEdoTask documentTask:
					await PublishDocumentResumeEvent(documentTask, cancellationToken);
					break;
				case TenderEdoTask tenderTask:
					await PublishTenderResumeEvent(tenderTask, cancellationToken);
					break;
				case ReceiptEdoTask receiptTask:
					await PublishReceiptResumeEvent(receiptTask, cancellationToken);
					break;
				default:
					throw new ArgumentOutOfRangeException(
						$"Задача ЭДО {edoTask.Id}: неизвестный тип задачи {edoTask.GetType().Name}, не удалось определить событие для возобновления");
			}
		}

		private async Task PublishDocumentResumeEvent(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Stage != DocumentEdoTaskStage.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (DocumentEdoTask) находится на стадии {Stage}. Возобновление возможно только на стадии New",
					edoTask.Id,
					edoTask.Stage);

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (DocumentEdoTask) находится на стадии {edoTask.Stage}. Возобновление возможно только на стадии New");
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (DocumentEdoTask) находится на стадии {Stage}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.Stage,
				nameof(DocumentTaskCreatedEvent));

			await _messageBus.Publish(new DocumentTaskCreatedEvent { Id = edoTask.Id }, cancellationToken);
		}

		private async Task PublishTenderResumeEvent(TenderEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.Stage != TenderEdoTaskStage.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (TenderEdoTask) находится на стадии {Stage}. Возобновление возможно только на стадии New",
					edoTask.Id,
					edoTask.Stage);

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (TenderEdoTask) находится на стадии {edoTask.Stage}. Возобновление возможно только на стадии New");
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (TenderEdoTask) находится на стадии {Stage}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.Stage,
				nameof(TenderTaskCreatedEvent));

			await _messageBus.Publish(new TenderTaskCreatedEvent { TenderEdoTaskId = edoTask.Id }, cancellationToken);
		}

		private async Task PublishReceiptResumeEvent(ReceiptEdoTask edoTask, CancellationToken cancellationToken)
		{
			if(edoTask.ReceiptStatus != EdoReceiptStatus.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Возобновление возможно только в статусе New",
					edoTask.Id,
					edoTask.ReceiptStatus);

				throw new ArgumentException(
					$"Задача ЭДО {edoTask.Id} (ReceiptEdoTask) находится в статусе {edoTask.ReceiptStatus}. Возобновление возможно только на стадии New");
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId} (ReceiptEdoTask) находится в статусе {ReceiptStatus}. Публикуем событие {EventName}",
				edoTask.Id,
				edoTask.ReceiptStatus,
				nameof(ReceiptTaskCreatedEvent));

			await _messageBus.Publish(new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = edoTask.Id }, cancellationToken);
		}
	}
}
