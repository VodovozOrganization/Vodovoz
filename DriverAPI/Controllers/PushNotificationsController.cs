using DriverAPI.Library.DataAccess;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PushNotificationsController : ControllerBase
    {
        private readonly ILogger<PushNotificationsController> logger;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IEmployeeData employeeData;

        public PushNotificationsController(ILogger<PushNotificationsController> logger, UserManager<IdentityUser> userManager, IEmployeeData employeeData)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
        }

        /// <summary>
        /// Эндпоинт включения PUSH уведомлений
        /// </summary>
        /// <param name="enablePushNotificationsRequest"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Route("/api/EnablePushNotifications")]
        public IActionResult EnablePushNotifications([FromBody] EnablePushNotificationsRequestModel enablePushNotificationsRequest)
        {
            try
            {
                var user = userManager.GetUserAsync(User).Result;
                var driver = employeeData.GetByAPILogin(user.UserName);
                employeeData.EnablePushNotifications(driver, enablePushNotificationsRequest.Token);
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return BadRequest(new ErrorResponseModel(e.Message));
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
            try
            {
                var user = userManager.GetUserAsync(User).Result;
                var driver = employeeData.GetByAPILogin(user.UserName);
                employeeData.DisablePushNotifications(driver);
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return BadRequest(new ErrorResponseModel(e.Message));
            }
        }
    }
}
