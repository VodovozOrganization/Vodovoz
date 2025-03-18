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
			_logger.LogInformation($"Consuming {nameof(WithdrawalTaskCreatedEvent)} with Id {context.Message.Id}");

			await _withdrawalTaskCreatedHandler.HandleWithdrawal(context.Message.Id, context.CancellationToken);
		}
	}
}
