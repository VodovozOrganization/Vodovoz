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
			try
			{
				await _withdrawalTaskCreatedHandler.HandleWithdrawal(context.Message.Id, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error while processing WithdrawalTaskCreatedEvent");
				await Task.CompletedTask;
			}
		}
	}
}
