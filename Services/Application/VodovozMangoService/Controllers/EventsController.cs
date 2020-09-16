using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VodovozMangoService.Calling;
using VodovozMangoService.DTO;

namespace VodovozMangoService.Controllers
{
	[ApiController]
    [Route("mango/[controller]")]
    public class EventsController : ControllerBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

        public static Dictionary<string, CallInfo> Calls = new Dictionary<string, CallInfo>();

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
            CallInfo call;
            lock (Calls)
            {
                if(!Calls.TryGetValue(message.call_id, out call))
                    Calls[message.call_id] = call = new CallInfo();

                if (call.Seq > message.seq) //Пришло старое сообщение
                    return;
                if (message.CallState == CallState.Disconnected)
                    Calls.Remove(message.call_id);
                
                call.LastEvent = message;
                if (!String.IsNullOrEmpty(message.from.taken_from_call_id)) {
                    if (Calls.ContainsKey(message.from.taken_from_call_id))
                        call.OnHoldCall = Calls[message.from.taken_from_call_id];
                    else
                        logger.Warn($"Информация о звонке {message.from.taken_from_call_id} отсутствет, но на него ссылается текущий звонок как переадресация.")
                }
            Program.NotificationServiceInstance.NewEvent(call);
        }
    }
}