using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VodovozMangoService.Calling;
using VodovozMangoService.DTO;
using VodovozMangoService.HostedServices;

namespace VodovozMangoService.Controllers
{
	[ApiController]
    [Route("mango/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly CallsHostedService _callsService;
        private readonly NotificationHostedService _notificationHostedService;
        private readonly IConfiguration _configuration;
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public EventsController(CallsHostedService callsService, NotificationHostedService notificationHostedService, IConfiguration configuration)
        {
            _callsService = callsService ?? throw new ArgumentNullException(nameof(callsService));
            _notificationHostedService = notificationHostedService ?? throw new ArgumentNullException(nameof(notificationHostedService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("call")]
        public async Task Call([FromForm] EventRequest eventRequest)
        {
#if DEBUG
            _logger.Debug($"message={eventRequest.Json}");
#endif
            if (!eventRequest.ValidateSign(_configuration))
            {
                _logger.Warn("Запрос с некорретной подписью пропускаем...");
                return;
            }
            //Обработка события.
            var message = eventRequest.CallEvent;
            CallInfo call = _callsService.Calls.GetOrAdd(message.CallId, id => new CallInfo(message));
            lock (call)
            {
                call.Events[message.Seq] = message;
                if (call.Seq > message.Seq) //Пришло старое сообщение
                {
                    _logger.Warn(
                        $"Пропускаем обработку сообщения с номером {message.Seq} так как уже получили сообщение с номером {call.Seq}");
                    return;
                }
                
                if (!string.IsNullOrEmpty(message.From.TakenFromCallId))
                {
                    if (_callsService.Calls.TryGetValue(message.From.TakenFromCallId, out var takenFrom))
                    {
                        if(takenFrom.LastEvent.CallStateEnum == CallState.OnHold)
						{
							call.OnHoldCall = _callsService.Calls[message.From.TakenFromCallId];
						}
					}
                }
                _notificationHostedService.NewEvent(call);
            }
        }
    }
}
