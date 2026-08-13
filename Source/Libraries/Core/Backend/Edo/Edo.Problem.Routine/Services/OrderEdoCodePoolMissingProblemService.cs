using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using Edo.Problems.Validation;
using Edo.Transport;
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
		private readonly MessageService _messageService;

		public OrderEdoCodePoolMissingProblemService(
			ILogger<OrderEdoCodePoolMissingProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEnumerable<IEdoTaskValidator> validators,
			IServiceProvider serviceProvider,
			IBus messageBus,
			MessageService messageService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_edoCodePoolValidator = (validators ?? throw new ArgumentNullException(nameof(validators)))
				.FirstOrDefault(v => v.Name == _problemSourceName);
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
		}

		public async Task TryResumeTask(OrderEdoTask edoTask, CancellationToken cancellationToken)
		{
			await TryResumeTaskAsync(edoTask, cancellationToken);
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
					
					throw new ArgumentException(
						$"Задача ЭДО {edoTask.Id}: пул кодов не прошел проверку по заказу №{edoTask.FormalEdoRequest.Order.Id}");
				}
			}

			_logger.LogInformation(
				"Задача ЭДО {EdoTaskId}: пул кодов прошел проверку по заказу №{OrderId}",
				edoTask.Id,
				edoTask.FormalEdoRequest.Order.Id);

			await _messageService.PublishTaskCreatedEvent(edoTask, cancellationToken);
		}
	}
}
