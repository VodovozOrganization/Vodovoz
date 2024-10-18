using EventsApi.Library.Models;
using LogisticsEventsApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;

namespace LogisticsEventsApi.Controllers
{
	[Authorize(Roles = _rolesToAccess)]
	[ApiController]
	[OnlyOneSession]
	[Route("/api/")]
	public class WarehouseEventsController : ControllerBase
	{
		private const string _rolesToAccess =
			nameof(ApplicationUserRole.WarehousePicker) + "," + nameof(ApplicationUserRole.WarehouseDriver);
		private readonly ILogger<WarehouseEventsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ILogisticsEventsService _warehouseEventsService;

		public WarehouseEventsController(
			ILogger<WarehouseEventsController> logger,
			UserManager<IdentityUser> userManager,
			ILogisticsEventsService warehouseEventsService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_warehouseEventsService = warehouseEventsService ?? throw new ArgumentNullException(nameof(warehouseEventsService));
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
				qrData = _warehouseEventsService.ConvertAndValidateQrData(eventData.QrData);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при валидации данных Qr кода от {Username}", userName);
			}

			if(qrData is null)
			{
				return ValidationProblem("Неправильный QR код");
			}

			_logger.LogInformation("Попытка завершения события {EventId} пользователем {Username}",
				qrData.EventId,
				userName);

			var user = await _userManager.GetUserAsync(User);
			var employee = _warehouseEventsService.GetEmployeeProxyByApiLogin(user.UserName);

			try
			{
				var completedEvent = _warehouseEventsService.CompleteDriverWarehouseEvent(
					qrData, eventData, employee, out var distanceMetersFromScanningLocation);

				if(completedEvent is null)
				{
					return Problem($"Слишком большое расстояние от Qr кода: {distanceMetersFromScanningLocation}м", statusCode: 550);
				}

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
		/// Завершение события нахождения на складе
		/// </summary>
		/// <param name="qrData">данные Qr для завершения события</param>
		/// <returns>Http status OK или ошибка</returns>
		/// <exception cref="Exception">ошибка</exception>
		[HttpPost("CompleteDriverWarehouseEventWithoutCoordinates")]
		public async Task<IActionResult> CompleteDriverWarehouseEventWithoutCoordinates(DriverWarehouseEventQrData qrData)
		{
			var userName = HttpContext.User.Identity?.Name ?? "Unknown";

			_logger.LogInformation("Попытка завершения события {EventId} пользователем {Username}",
				qrData.EventId,
				userName);

			var user = await _userManager.GetUserAsync(User);
			var employee = _warehouseEventsService.GetEmployeeProxyByApiLogin(user.UserName);

			try
			{
				var completedEvent = _warehouseEventsService.CompleteWarehouseEventWithoutCoordinates(
					qrData, employee, out var distanceMetersFromScanningLocation);

				if(completedEvent is null)
				{
					return Problem($"При завершении события возникла непредвиденная ошибка. Создать событие не удалось. Обратитесь в техподдержку", statusCode: 550);
				}

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

			_logger.LogInformation("Попытка получения завершенных событий за день пользователем {Username}",
				userName);

			var user = await _userManager.GetUserAsync(User);
			var employee = _warehouseEventsService.GetEmployeeProxyByApiLogin(user.UserName);

			try
			{
				return Ok(_warehouseEventsService.GetTodayCompletedEvents(employee));
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
