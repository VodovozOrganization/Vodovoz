using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Withdrawal.Consumers
{
	public class WithdrawalTaskCreatedConsumer : IConsumer<WithdrawalTaskCreatedEvent>
	{
		private readonly ILogger<WithdrawalTaskCreatedConsumer> _logger;
		private readonly WithdrawalTaskCreatedHandler _withdrawalTaskCreatedHandler;

		public WithdrawalTaskCreatedConsumer(
			ILogger<WithdrawalTaskCreatedConsumer> logger,
			WithdrawalTaskCreatedHandler withdrawalTaskCreatedHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_withdrawalTaskCreatedHandler = withdrawalTaskCreatedHandler ?? throw new ArgumentNullException(nameof(withdrawalTaskCreatedHandler));
		}

		public async Task Consume(ConsumeContext<WithdrawalTaskCreatedEvent> context)
		{
			_logger.LogInformation("Consuming {EventName} with Id {WithdrawalEdoTaskId}",
				nameof(WithdrawalTaskCreatedEvent),
				context.Message.WithdrawalEdoTaskId);

			await _withdrawalTaskCreatedHandler.HandleWithdrawal(context.Message.WithdrawalEdoTaskId, context.CancellationToken);

			_logger.LogInformation("Successfully consumed {EventName} with Id {WithdrawalEdoTaskId}",
				nameof(WithdrawalTaskCreatedEvent),
				context.Message.WithdrawalEdoTaskId);
		}
	}
}
