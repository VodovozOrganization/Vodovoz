using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public EventsController(CallsHostedService callsService)
        {
            this.callsService = callsService ?? throw new ArgumentNullException(nameof(callsService));
        }

        [HttpPost("call")]
        public async Task Call([FromForm] EventRequest eventRequest)
        {
#if DEBUG
            logger.Debug($"message={eventRequest.Json}");
#endif
            if (!eventRequest.ValidateSign())
            {
                logger.Warn("Запрос с некорретной подписью пропускаем...");
                return;
            }
            //Обработка события.
            var message = eventRequest.CallEvent;
            CallInfo call = callsService.Calls.GetOrAdd(message.call_id, id => new CallInfo(message));
            lock (call)
            {
                if (call.Seq > message.seq) //Пришло старое сообщение
                {
                    logger.Warn(
                        $"Пропускаем обработку сообщения с номером {message.seq} так как уже получили сообщение с номером {call.Seq}");
                    return;
                }
                call.Events[message.seq] = message;
                
                if (!String.IsNullOrEmpty(message.from.taken_from_call_id))
                {
                    if (callsService.Calls.TryGetValue(message.from.taken_from_call_id, out var takenFrom))
                    {
                        if(takenFrom.LastEvent.location == "abonent")
                            call.OnHoldCall = callsService.Calls[message.from.taken_from_call_id];
                    }
                }
                Program.NotificationServiceInstance.NewEvent(call);
            }
        }
    }
}