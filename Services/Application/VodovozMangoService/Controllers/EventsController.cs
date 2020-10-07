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
using VodovozMangoService.Calling;
using VodovozMangoService.DTO;

namespace VodovozMangoService.Controllers
{
	[ApiController]
    [Route("mango/[controller]")]
    public class EventsController : ControllerBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
#if DEBUG
        private FileInfo file =new FileInfo("/var/log/VodovozMangoService/DebugReport.txt");
        private StreamWriter stream = null;
        EventsController()
        {
            stream = new StreamWriter(file.Open(FileMode.Append,FileAccess.Write));
        }

        ~EventsController()
        {
            if(file.Exists)
                file.Delete();
        }
#endif
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
#if DEBUG
            string debugParseMessage = null;   
#endif
            lock (Calls)
            {
                if (!Calls.TryGetValue(message.call_id, out call))
                {
#if DEBUG
                    if (message.CallState == CallState.Disconnected)
                        debugParseMessage += "У звонка не было Appeared/Connect"+ $"|{message.call_id}|"+eventRequest.Json + "\n";
#endif

                    Calls[message.call_id] = call = new CallInfo();
                    call.LastEvent = message;
                }
                if (call.Seq > message.seq) //Пришло старое сообщение
                    return;
                if (message.CallState == CallState.Disconnected)
                {
                    Calls.Remove(message.call_id);
                }
#if DEBUG
                foreach (var item in Calls)
                {
                    Console.Write("Item" + item + "\n" );
                    Console.Write("Key" + item.Key + "\n");
                    Console.Write("Value" + item.Value + "\n");
                    Console.Write("LastEvent" + item.Value.LastEvent + "\n" );
                    Console.Write("Time" + item.Value.LastEvent.Time.Minute + "\n");
                }
                var longerCallIds = Calls.Where(c => c.Value.LastEvent.Time.Minute > 60)
                    .Select(e => e.Value.LastEvent.call_id).ToList();
                if (longerCallIds != null && longerCallIds.Count > 0)
                {
                    debugParseMessage += "Эти звонки не закрыты больше 1 часа:\n";
                    longerCallIds.ForEach(str => debugParseMessage += "| Call Id:" + str + "\n");
                    debugParseMessage += "Всего таких звонков: " + longerCallIds.Count + "\n";
                    debugParseMessage += "Удаляем их!" + "\n";
                    foreach (string id in longerCallIds)
                    {
                        Calls.Remove(id);
                    }
                }
#endif

            }
            call.LastEvent = message;
            if (!String.IsNullOrEmpty(message.from.taken_from_call_id))
            {
                if (Calls.ContainsKey(message.from.taken_from_call_id))
                    call.OnHoldCall = Calls[message.from.taken_from_call_id];
                else
                {
                    logger.Warn($"Информация о звонке {message.from.taken_from_call_id} отсутствет, но на него ссылается текущий звонок как переадресация.");
#if DEBUG
                    debugParseMessage += "Попытка перевода звонка, первоначального события которого нет:" +
                                         eventRequest.Json +"\n";
#endif

                }
            }
            Program.NotificationServiceInstance.NewEvent(call);
            logger.Debug($"Сервер отслеживает {Calls.Count} звонков");
#if DEBUG
            if (!String.IsNullOrEmpty(debugParseMessage))
            {
                try
                {
                    lock (stream)
                    {
                        stream.Write(debugParseMessage);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
#endif
        }
    }
}