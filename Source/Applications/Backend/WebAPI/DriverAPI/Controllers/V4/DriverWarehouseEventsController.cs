using System;
using System.Threading.Tasks;
using DriverAPI.DTOs.V4;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер событий нахождения водителя на складе
	/// </summary>
	[Authorize]
	public class DriverWarehouseEventsController : VersionedController
	{
		private readonly ILogger<DriverWarehouseEventsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IEmployeeModel _employeeModel;
		private readonly IDriverWarehouseEventsModel _driverWarehouseEventsModel;
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordModel;

		public DriverWarehouseEventsController(
			ILogger<DriverWarehouseEventsController> logger,
			UserManager<IdentityUser> userManager,
			IEmployeeModel employeeModel,
			IDriverWarehouseEventsModel driverWarehouseEventsModel,
			IDriverMobileAppActionRecordModel driverMobileAppActionRecordModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_employeeModel = employeeModel ?? throw new ArgumentNullException(nameof(employeeModel));
			_driverWarehouseEventsModel = driverWarehouseEventsModel ?? throw new ArgumentNullException(nameof(driverWarehouseEventsModel));
			_driverMobileAppActionRecordModel =
				driverMobileAppActionRecordModel ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordModel));
		}
		
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <param name="eventData">данные для завершения события</param>
		/// <returns>Http status OK или ошибка</returns>
		/// <exception cref="Exception">ошибка</exception>
		[HttpPost("CompleteDriverWarehouseEvent")]
		public async Task CompleteDriverWarehouseEventAsync(DriverWarehouseEventData eventData)
		{
			var eventId = eventData.DriverWarehouseEventId;
			var userName = HttpContext.User.Identity?.Name ?? "Unknown";
			
			_logger.LogInformation("Попытка завершения события {EventId} пользователем {Username} User token: {AccessToken}",
				eventId,
				userName,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeModel.GetByAPILogin(user.UserName);
			
			try
			{
				_driverWarehouseEventsModel.CompleteDriverWarehouseEvent(eventData, driver);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при попытке завершить событие {EventId} пользователем {Username}",
					eventId,
					userName);
				throw;
			}
		}
	}
}
