using Mango.Core.Dto;
using Mango.Service.Calling;
using Mango.Service.HostedServices;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Mango.Service.Consumers
{
	public class MangoCallEventConsumer : IConsumer<MangoCallEvent>
	{
		private readonly ILogger<MangoCallEventConsumer> _logger;
		private readonly CallsHostedService _callsService;
		private readonly NotificationHostedService _notificationHostedService;

		public MangoCallEventConsumer(
			ILogger<MangoCallEventConsumer> logger, 
			CallsHostedService callsService, 
			NotificationHostedService notificationHostedService
			)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_callsService = callsService ?? throw new System.ArgumentNullException(nameof(callsService));
			_notificationHostedService = notificationHostedService ?? throw new System.ArgumentNullException(nameof(notificationHostedService));
		}

		public Task Consume(ConsumeContext<MangoCallEvent> context)
		{
			var callEvent = context.Message;
			var call = _callsService.Calls.GetOrAdd(callEvent.CallId, id => new CallInfo(callEvent));
			lock(call)
			{
				call.Events[callEvent.Seq] = callEvent;
				if(call.Seq > callEvent.Seq)
				{
					//Пришло старое сообщение
					_logger.LogWarning("Пропускаем обработку сообщения с номером {Seq} " +
						"так как уже получили сообщение с номером {Seq}", callEvent.Seq, callEvent.Seq);
					return Task.CompletedTask;
				}

				if(!string.IsNullOrEmpty(callEvent.From.TakenFromCallId))
				{
					if(_callsService.Calls.TryGetValue(callEvent.From.TakenFromCallId, out var takenFrom))
					{
						if((int)takenFrom.LastEvent.CallState == (int)CallState.OnHold)
						{
							call.OnHoldCall = _callsService.Calls[callEvent.From.TakenFromCallId];
						}
					}
				}

				_notificationHostedService.NewEvent(call);
			}

			return Task.CompletedTask;
		}
	}
}
