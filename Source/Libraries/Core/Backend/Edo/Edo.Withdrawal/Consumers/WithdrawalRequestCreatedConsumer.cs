using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Withdrawal.Consumers
{
	public class WithdrawalRequestCreatedConsumer : IConsumer<WithdrawalRequestCreatedEvent>
	{
		private readonly ILogger<WithdrawalRequestCreatedConsumer> _logger;

		public WithdrawalRequestCreatedConsumer(
			ILogger<WithdrawalRequestCreatedConsumer> logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<WithdrawalRequestCreatedEvent> context)
		{
			try
			{

			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error while processing WithdrawalRequestCreatedEvent");
				await Task.CompletedTask;
			}
		}
	}
}
