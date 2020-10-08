using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using NLog.Fluent;
using VodovozMangoService.Calling;
using VodovozMangoService.DTO;

namespace VodovozMangoService.Controllers
{
	[ApiController]
    [Route("mango/[controller]")]
    public class EventsController : ControllerBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static NLog.Logger loggerLostEvents = NLog.LogManager.GetLogger("LostEvents");
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
                if (!Calls.TryGetValue(message.call_id, out call))
                {
                    if (message.CallState == CallState.Disconnected)
                        loggerLostEvents.Error( $"У звонка не было Appeared/Connect |{message.call_id}|{eventRequest.Json}");
                    Calls[message.call_id] = call = new CallInfo();
                    call.LastEvent = message;
                }
                if (call.Seq > message.seq) //Пришло старое сообщение
                    return;
                if (message.CallState == CallState.Disconnected)
                {
                    Calls.Remove(message.call_id);
                }

                var longerCallIds = Calls.Where(c => c.Value.LastEvent.Time.Minute > 60)
                    .Select(e => e.Value.LastEvent.call_id).ToList();
                if (longerCallIds.Count > 0)
                {
                    var text = "Эти звонки не получили события Desconnected более 1 часа:\n";
                    longerCallIds.ForEach(str => text += "| Call Id:" + str + "\n");
                    text += "Всего таких звонков: " + longerCallIds.Count + "\n";
                    text += "Удаляем их!";
                    loggerLostEvents.Error(text);
                    foreach (string id in longerCallIds)
                    {
                        Calls.Remove(id);
                    }
                }
            }
            call.LastEvent = message;
            if (!String.IsNullOrEmpty(message.from.taken_from_call_id))
            {
                //Если звонок не нашли это нормально, так как он может ссылаться на IVR который был уже удланен.
                if (Calls.ContainsKey(message.from.taken_from_call_id) && Calls[message.from.taken_from_call_id].LastEvent.location != "ivr")
                    call.OnHoldCall = Calls[message.from.taken_from_call_id];
            }
            Program.NotificationServiceInstance.NewEvent(call);
            logger.Debug($"Сервер отслеживает {Calls.Count} звонков");
        }
    }
}