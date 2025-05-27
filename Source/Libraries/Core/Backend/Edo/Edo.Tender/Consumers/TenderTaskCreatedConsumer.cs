using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Edo.Tender.Consumers
{
	/// <summary>
	/// Настройка MassTransit для события создания задачи по Тендеру
	/// </summary>
	public class TenderTaskCreatedConsumer : IConsumer<TenderTaskCreatedEvent>
	{
		private readonly ILogger<TenderTaskCreatedConsumer> _logger;
		private readonly TenderEdoTaskHandler _tenderEdoTaskHandler;

		public TenderTaskCreatedConsumer(
			ILogger<TenderTaskCreatedConsumer> logger,
			TenderEdoTaskHandler tenderEdoTaskHandler
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenderEdoTaskHandler = tenderEdoTaskHandler ?? throw new ArgumentNullException(nameof(tenderEdoTaskHandler));
		}
		
		public async Task Consume(ConsumeContext<TenderTaskCreatedEvent> context)
		{
			await _tenderEdoTaskHandler.HandleNew(context.Message.TenderEdoTaskId, context.CancellationToken);
		}
	}
}
