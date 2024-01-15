using System;
using System.Threading.Tasks;
using EventsApi.Library.Dtos;
using EventsApi.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace LogisticsEventsApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("/api/")]
	public class WarehouseEventsController : ControllerBase
	{
		private readonly ILogger<WarehouseEventsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ILogisticsEventsModel _warehouseEventsModel;

		public WarehouseEventsController(
			ILogger<WarehouseEventsController> logger,
			UserManager<IdentityUser> userManager,
			ILogisticsEventsModel warehouseEventsModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_warehouseEventsModel = warehouseEventsModel ?? throw new ArgumentNullException(nameof(warehouseEventsModel));
		}
		
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <param name="eventData">данные для завершения события</param>
		/// <returns>Http status OK или ошибка</returns>
		/// <exception cref="Exception">ошибка</exception>
		[HttpPost("CompleteDriverWarehouseEvent")]
		public async Task<IActionResult> CompleteWarehouseEventAsync(DriverWarehouseEventData eventData)
		{
			var userName = HttpContext.User.Identity?.Name ?? "Unknown";
			DriverWarehouseEventQrData qrData = null; 
			
			try
			{
				qrData = _warehouseEventsModel.ConvertAndValidateQrData(eventData.QrData);
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
			var employee = _warehouseEventsModel.GetEmployeeProxyByApiLogin(user.UserName);

			try
			{
				var completedEvent = _warehouseEventsModel.CompleteDriverWarehouseEvent(qrData, eventData, employee);
				
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
			var employee = _warehouseEventsModel.GetEmployeeProxyByApiLogin(user.UserName);
			
			try
			{
				return Ok(_warehouseEventsModel.GetTodayCompletedEvents(employee));
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
