using EventsApi.Library.Models;
using LogisticsEventsApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Logistics;
using Vodovoz.Core.Domain.Employees;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер событий нахождения водителя на складе
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class DriverWarehouseEventsController : VersionedController
	{
		private readonly ILogger<DriverWarehouseEventsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ILogisticsEventsService _driverWarehouseEventsService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="driverWarehouseEventsService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public DriverWarehouseEventsController(
			ILogger<DriverWarehouseEventsController> logger,
			UserManager<IdentityUser> userManager,
			ILogisticsEventsService driverWarehouseEventsService) : base(logger)
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
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompletedDriverWarehouseEventDto))]
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
				_logger.LogError(e, "Ошибка при валидации данных Qr кода от {Username}: {ExceptionMessage}",
					userName,
					e.Message);
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
				var completedEvent = _driverWarehouseEventsService.CompleteDriverWarehouseEvent(
					qrData, eventData, driver, out var distanceMetersFromScanningLocation);

				if(completedEvent is null)
				{
					return Problem($"Слишком большое расстояние от Qr кода: {distanceMetersFromScanningLocation}м", statusCode: StatusCodes.Status403Forbidden);
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
				_logger.LogError(e, "Ошибка при попытке завершить событие {EventId} пользователем {Username}: {ExceptionMessage}",
					qrData.EventId,
					userName,
					e.Message);

				return Problem("Сервис не доступен. Обратитесь в техподдержку");
			}
		}

		/// <summary>
		/// Получение списка завершенных событий за день
		/// </summary>
		/// <returns>список завершенных событий</returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompletedEventDto>))]
		public async Task<IActionResult> GetTodayCompletedEvents()
		{
			var userName = HttpContext.User.Identity?.Name ?? "Unknown";

			_logger.LogInformation("Попытка получения завершенных событий за день пользователем {Username} User token: {AccessToken}",
				userName,
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _driverWarehouseEventsService.GetEmployeeProxyByApiLogin(
				user.UserName, ExternalApplicationType.DriverApp);

			return Ok(_driverWarehouseEventsService.GetTodayCompletedEvents(driver));
		}
	}
}
