using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Util;
using Pacs.Core.Messages.Events;
using Pacs.Server.Operators;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Consumers
{
	public class PacsServerCallEventConsumer : IConsumer<PacsCallEvent>
	{
		private readonly ILogger<PacsServerCallEventConsumer> _logger;
		private readonly IOperatorStateService _operatorStateService;
		private static ConcurrentQueue<Guid> _processedEventsPool = new ConcurrentQueue<Guid>();
		private const int MaxEventsPoolSize = 100;

		public PacsServerCallEventConsumer(
			ILogger<PacsServerCallEventConsumer> logger,
			IOperatorStateService operatorControllerProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_operatorStateService = operatorControllerProvider ?? throw new ArgumentNullException(nameof(operatorControllerProvider));
		}

		public async Task Consume(ConsumeContext<PacsCallEvent> context)
		{
			_logger.LogInformation("Обрабатывается событие {@PacsCallEvent}", context.Message);

			var call = context.Message.Call;

			if(call.CallDirection.HasValue && call.CallDirection != CallDirection.Incoming)
			{
				return;
			}

			var connectedSubCall = call.SubCalls.FirstOrDefault(x => x.WasConnected);

			if(connectedSubCall == null)
			{
				return;
			}

			if(call.Status == CallStatus.Connected)
			{
				if(WasProcessedBefore(context.Message.EventId))
				{
					return;
				}

				await _operatorStateService.TakeCall(connectedSubCall.ToExtension, connectedSubCall.CallId);

				RegisterProcessed(context.Message.EventId);
			}
			else
			{
				await _operatorStateService.EndCall(connectedSubCall.ToExtension, connectedSubCall.CallId);
			}
		}

		public static bool WasProcessedBefore(Guid eventId) => _processedEventsPool.Any(x => x == eventId);

		public static void RegisterProcessed(Guid eventId)
		{
			while(_processedEventsPool.Count >= MaxEventsPoolSize)
			{
				_processedEventsPool.TryDequeue(out _);
			}

			_processedEventsPool.Enqueue(eventId);
		}
	}
}
