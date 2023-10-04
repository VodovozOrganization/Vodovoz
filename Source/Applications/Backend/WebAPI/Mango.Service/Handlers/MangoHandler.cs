using Mango.Core.Dto;
using Mango.Core.Handlers;
using Mango.Service.Extensions;
using Mango.Service.HostedServices;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Mango.Service.Calling;

namespace Mango.Service.Handlers
{
	public class MangoHandler : ICallEventHandler
	{
		private readonly ILogger<MangoHandler> _logger;
		private readonly CallsHostedService _callsService;
		private readonly NotificationHostedService _notificationHostedService;

		public MangoHandler(ILogger<MangoHandler> logger, CallsHostedService callsService, NotificationHostedService notificationHostedService)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_callsService = callsService ?? throw new System.ArgumentNullException(nameof(callsService));
			_notificationHostedService = notificationHostedService ?? throw new System.ArgumentNullException(nameof(notificationHostedService));
		}

		public Task HandleAsync(CallEvent callEvent)
		{
			var call = _callsService.Calls.GetOrAdd(callEvent.CallId, id => new CallInfo(callEvent));
			lock(call)
			{
				call.Events[callEvent.Seq] = callEvent;
				if(call.Seq > callEvent.Seq)
				{
					//Пришло старое сообщение
					_logger.LogWarning($"Пропускаем обработку сообщения с номером {callEvent.Seq} " +
						$"так как уже получили сообщение с номером {call.Seq}");
					return Task.CompletedTask;
				}

				if(!string.IsNullOrEmpty(callEvent.From.TakenFromCallId))
				{
					if(_callsService.Calls.TryGetValue(callEvent.From.TakenFromCallId, out var takenFrom))
					{
						if(takenFrom.LastEvent.CallState.ParseCallState() == CallState.OnHold)
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
