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

		public WithdrawalTaskCreatedConsumer(
			ILogger<WithdrawalTaskCreatedConsumer> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<WithdrawalTaskCreatedEvent> context)
		{
			try
			{

			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error while processing WithdrawalTaskCreatedEvent");
				await Task.CompletedTask;
			}
		}
	}
}
