using System;
using System.Threading.Tasks;
using DriverAPI.DTOs.V4;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Vodovoz.Domain.Logistic.Drivers;

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

		public DriverWarehouseEventsController(
			ILogger<DriverWarehouseEventsController> logger,
			UserManager<IdentityUser> userManager,
			IEmployeeModel employeeModel,
			IDriverWarehouseEventsModel driverWarehouseEventsModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_employeeModel = employeeModel ?? throw new ArgumentNullException(nameof(employeeModel));
			_driverWarehouseEventsModel = driverWarehouseEventsModel ?? throw new ArgumentNullException(nameof(driverWarehouseEventsModel));
		}
		
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <param name="eventData">данные для завершения события</param>
		/// <returns>Http status OK или ошибка</returns>
		/// <exception cref="Exception">ошибка</exception>
		[HttpPost("CompleteDriverWarehouseEvent")]
		public async Task<IActionResult> CompleteDriverWarehouseEventAsync(DriverWarehouseEventData eventData)
		{
			var userName = HttpContext.User.Identity?.Name ?? "Unknown";
			DriverWarehouseEventQrData qrData = null; 
			
			try
			{
				qrData = _driverWarehouseEventsModel.ConvertAndValidateQrData(eventData.QrData);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при валидации данных Qr кода от {Username}", userName);
			}

			if(qrData is null)
			{
				return ValidationProblem("Неправильный QR код");
			}
			
			_logger.LogInformation("Попытка завершения события {EventId} пользователем {Username} User token: {AccessToken}",
				qrData.EventId,
				userName,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeModel.GetByAPILogin(user.UserName);

			try
			{
				var completedEvent = _driverWarehouseEventsModel.CompleteDriverWarehouseEvent(qrData, eventData, driver);
				
				return Ok(
					new CompletedDriverWarehouseEventDto
					{
						EventName = completedEvent.DriverWarehouseEvent.EventName,
						CompletedDate = completedEvent.CompletedDate,
						EmployeeName = completedEvent.Employee.ShortName
					});
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при попытке завершить событие {EventId} пользователем {Username}",
					qrData.EventId,
					userName);

				return Problem("Сервис не доступен. Обратитесь в техподдержку");
			}
		}

		/// <summary>
		/// Получение списка завершенных событий за день
		/// </summary>
		/// <returns>список завершенных событий</returns>
		[HttpGet("GetTodayCompletedEvents")]
		public async Task<IActionResult> GetTodayCompletedEvents()
		{
			var userName = HttpContext.User.Identity?.Name ?? "Unknown";

			_logger.LogInformation("Попытка получения завершенных событий за день пользователем {Username} User token: {AccessToken}",
				userName,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeModel.GetByAPILogin(user.UserName);
			
			try
			{
				return Ok(_driverWarehouseEventsModel.GetTodayCompletedEventsForDriver(driver));
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при попытке получения завершенных событий за день пользователем {Username}",
					userName);

				return Problem("Внутренняя ошибка сервера");
			}
		}
	}
}
