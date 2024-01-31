using System;
using System.Threading.Tasks;
using EventsApi.Library.Dtos;
using EventsApi.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Vodovoz.Core.Domain.Employees;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер событий нахождения водителя на складе
	/// </summary>
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class DriverWarehouseEventsController : VersionedController
	{
		private readonly ILogger<DriverWarehouseEventsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ILogisticsEventsService _driverWarehouseEventsService;

		public DriverWarehouseEventsController(
			ILogger<DriverWarehouseEventsController> logger,
			UserManager<IdentityUser> userManager,
			ILogisticsEventsService driverWarehouseEventsService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_driverWarehouseEventsService = driverWarehouseEventsService ?? throw new ArgumentNullException(nameof(driverWarehouseEventsService));
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
				qrData = _driverWarehouseEventsService.ConvertAndValidateQrData(eventData.QrData);
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
			var driver = _driverWarehouseEventsService.GetEmployeeProxyByApiLogin(
				user.UserName, ExternalApplicationType.DriverApp);
			
			try
			{
				var completedEvent = _driverWarehouseEventsService.CompleteDriverWarehouseEvent(qrData, eventData, driver);
				
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
			var driver = _driverWarehouseEventsService.GetEmployeeProxyByApiLogin(
				user.UserName, ExternalApplicationType.DriverApp);
			
			try
			{
				return Ok(_driverWarehouseEventsService.GetTodayCompletedEvents(driver));
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
