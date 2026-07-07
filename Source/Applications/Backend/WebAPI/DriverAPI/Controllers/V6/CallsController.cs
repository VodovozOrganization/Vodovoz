using DriverApi.Contracts.V6.Requests;
using DriverAPI.Library.V6.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Errors.Logistics;
using Vodovoz.Presentation.WebApi.Common;

namespace DriverAPI.Controllers.V6
{
	/// <summary>
	/// Контроллер звонков Манго
	/// </summary>
	[ApiVersion("6.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class CallsController : VersionedController
	{
		private readonly ICallsService _callsService;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IEmployeeService _employeeService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="callsService">Сервис для работы со звонками</param>
		/// <param name="userManager">Сервис для работы с пользователями</param>
		/// <param name="employeeService">Сервис для работы с сотрудниками</param>
		public CallsController(
			ILogger<ApiControllerBase> logger,
			ICallsService callsService,
			UserManager<IdentityUser> userManager,
			IEmployeeService employeeService
			) : base(logger)
		{
			_callsService = callsService ?? throw new System.ArgumentNullException(nameof(callsService));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
		}

		/// <summary>
		/// Отправка запроса на звонок через вебхук Манго
		/// </summary>
		/// <param name="getCallRequest">Модель данных входящего запроса</param>
		/// <param name="cancellationToken">Токен отмены</param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> GetCall([FromBody] GetCallRequest getCallRequest, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Запрос на звонок на номер: {ClientPhoneNumber}, маршрутный лист {RouteListId} от пользователя {Username} | X-Idempotency-Key: {XIdempotencyKey} | X-Action-Time-Utc: {XActionTimeUtc}",
				getCallRequest.Number,
				getCallRequest.RouteListId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				HttpContext.Request.Headers["X-Idempotency-Key"],
				HttpContext.Request.Headers["X-Action-Time-Utc"]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			try
			{
				var result =
					await _callsService.MakeWebhookCall(getCallRequest.RouteListId, driver, getCallRequest.Number, cancellationToken);

				return MapResult(
					result,
					result =>
					{
						if(result.IsSuccess)
						{
							return StatusCodes.Status204NoContent;
						}

						var firstError = result.Errors.First();

						if(firstError.Code == RouteListErrors.NotFound)
						{
							return StatusCodes.Status404NotFound;
						}

						if(firstError.Code == RouteListErrors.NotEnRouteState
							|| firstError.Code == Library.Errors.PhoneNumberErrors.InvalidFormat
							|| firstError.Code == Library.Errors.PhoneNumberErrors.ActiveMangoExtensionNumberNotFound)
						{
							return StatusCodes.Status400BadRequest;
						}

						if(firstError.Code == Library.Errors.Security.Authorization.RouteListAccessDenied)
						{
							return StatusCodes.Status403Forbidden;
						}

						return StatusCodes.Status500InternalServerError;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Произошла ошибка при запросе звонка на номер клиента {ClientPhoneNumber} для маршрутного листа {RouteListId}: {ExceptionMessage}",
					getCallRequest.Number,
					getCallRequest.RouteListId,
					ex.Message);

				return Problem($"Произошла ошибка при запросе звонка на номер клиента {getCallRequest.Number} для маршрутного листа {getCallRequest.RouteListId}");
			}
		}
	}
}
