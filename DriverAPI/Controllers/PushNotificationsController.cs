using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PushNotificationsController : ControllerBase
    {
        /// <summary>
        /// Эндпоинт включения PUSH уведомлений
        /// </summary>
        /// <param name="enablePushNotificationsRequest"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Route("/api/EnablePushNotifications")]
        public IActionResult EnablePushNotifications([FromBody] EnablePushNotificationsRequestModel enablePushNotificationsRequest)
        {
            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Эндпоинт отключения PUSH уведомлений
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Route("/api/DisablePushNotifications")]
        public IActionResult DisablePushNotifications()
        {
            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
