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
        private readonly CallsHostedService callsService;
        private readonly NotificationHostedService notificationHostedService;
        private readonly IConfiguration configuration;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public EventsController(CallsHostedService callsService, NotificationHostedService notificationHostedService, IConfiguration configuration)
        {
            this.callsService = callsService ?? throw new ArgumentNullException(nameof(callsService));
            this.notificationHostedService = notificationHostedService ?? throw new ArgumentNullException(nameof(notificationHostedService));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("call")]
        public async Task Call([FromForm] EventRequest eventRequest)
        {
#if DEBUG
            logger.Debug($"message={eventRequest.Json}");
#endif
            if (!eventRequest.ValidateSign(configuration))
            {
                logger.Warn("Запрос с некорретной подписью пропускаем...");
                return;
            }
            //Обработка события.
            var message = eventRequest.CallEvent;
            CallInfo call = callsService.Calls.GetOrAdd(message.call_id, id => new CallInfo(message));
            lock (call)
            {
                call.Events[message.seq] = message;
                if (call.Seq > message.seq) //Пришло старое сообщение
                {
                    logger.Warn(
                        $"Пропускаем обработку сообщения с номером {message.seq} так как уже получили сообщение с номером {call.Seq}");
                    return;
                }
                
                if (!String.IsNullOrEmpty(message.from.taken_from_call_id))
                {
                    if (callsService.Calls.TryGetValue(message.from.taken_from_call_id, out var takenFrom))
                    {
                        if(takenFrom.LastEvent.CallState == CallState.OnHold)
                            call.OnHoldCall = callsService.Calls[message.from.taken_from_call_id];
                    }
                }
                notificationHostedService.NewEvent(call);
            }
        }
    }
}