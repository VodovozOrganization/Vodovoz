using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using VodovozMangoService.DTO;

namespace VodovozMangoService.Controllers
{
    [ApiController]
    [Route("mango/[controller]")]
    public class EventsController : ControllerBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger ();

        [HttpPost("call")]
        public async Task<string> Call([FromForm] EventRequest eventRequest)
        {
#if DEBUG
            String message = $"vpbx_api_key={eventRequest.Vpbx_Api_Key}\nsign={eventRequest.Sign}\njson={eventRequest.Json}";
            logger.Debug(message);
#endif
            if (!eventRequest.ValidateSign())
            {
                logger.Warn("Запрос с некорретной подписью пропускаем...");
                return null;
            }
            return String.Empty;
        }
    }
}