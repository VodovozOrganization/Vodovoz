﻿using DriverApi.Contracts.V5;
using DriverApi.Contracts.V5.Requests;
using DriverApi.Contracts.V5.Responses;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;
using IApiRouteListService = DriverAPI.Library.V5.Services.IRouteListService;
using IRouteListService = Vodovoz.Services.Logistics.IRouteListService;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Контроллер маршрутных листов
	/// </summary>
	[ApiVersion("5.0")]
	[Authorize(Roles = nameof(ApplicationUserRole.Driver))]
	public class RouteListsController : VersionedController
	{
		private readonly ILogger<RouteListsController> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IApiRouteListService _apiRouteListService;
		private readonly IOrderService _orderService;
		private readonly IEmployeeService _employeeService;
		private readonly IDriverMobileAppActionRecordService _driverMobileAppActionRecordService;
		private readonly IActionTimeHelper _actionTimeHelper;
		private readonly IRouteListService _routeListService;
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="unitOfWork"></param>
		/// <param name="apiRouteListService"></param>
		/// <param name="orderService"></param>
		/// <param name="employeeService"></param>
		/// <param name="driverMobileAppActionRecordService"></param>
		/// <param name="actionTimeHelper">Хелпер-класс для времени</param>
		/// <param name="routeListService">Сервис маршрутных листов</param>
		/// <param name="userManager">Менеджер пользователей</param>
		/// <exception cref="ArgumentNullException"></exception>

		public RouteListsController(
			ILogger<RouteListsController> logger,
			IUnitOfWork unitOfWork,
			IApiRouteListService apiRouteListService,
			IOrderService orderService,
			IEmployeeService employeeService,
			IDriverMobileAppActionRecordService driverMobileAppActionRecordService,
			IActionTimeHelper actionTimeHelper,
			IRouteListService routeListService,
			UserManager<IdentityUser> userManager) : base(logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_apiRouteListService = apiRouteListService;
			_orderService = orderService;
			_employeeService = employeeService;
			_driverMobileAppActionRecordService = driverMobileAppActionRecordService;
			_actionTimeHelper = actionTimeHelper ?? throw new ArgumentNullException(nameof(actionTimeHelper));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		/// <summary>
		/// Получения детализированнной информации о маршрутных листах и связанных заказах
		/// </summary>
		/// <param name="routeListsIds">Список номеров маршрутных листов</param>
		/// <returns>GetRouteListsDetailsResponseModel</returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetRouteListsDetailsResponse))]
		public IActionResult GetRouteListsDetails([FromBody] int[] routeListsIds)
		{
			_logger.LogInformation("Запрос МЛ-ов с деталями: {@RouteListIds} пользователем {Username} User token: {AccessToken}",
				routeListsIds,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var routeLists = _apiRouteListService.Get(routeListsIds);
			var ordersIds = routeLists.Where(x => x.CompletionStatus == RouteListDtoCompletionStatus.Incompleted)
				.SelectMany(x => x.IncompletedRouteList.RouteListAddresses.Select(x => x.OrderId));

			var orders = _orderService.Get(ordersIds.ToArray());

			var resortedOrders = new List<OrderDto>();

			foreach(var orderId in ordersIds)
			{
				resortedOrders.Add(orders.Where(o => o.OrderId == orderId).First());
			}

			return Ok(new GetRouteListsDetailsResponse()
			{
				RouteLists = routeLists,
				Orders = resortedOrders
			});
		}

		/// <summary>
		/// Получения информации о маршрутном листе
		/// </summary>
		/// <param name="routeListId">Номер маршрутного листа</param>
		/// <returns><see cref="RouteListDto"/></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RouteListDto))]
		public IActionResult GetRouteList(int routeListId)
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];

			_logger.LogInformation("Запрос информации о МЛ {RouteListId} пользователем {Username} User token: {AccessToken}",
				routeListId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				tokenStr);

			return Ok(_apiRouteListService.Get(routeListId));
		}

		/// <summary>
		/// Получение номеров маршрутных листов текущего водителя
		/// </summary>
		/// <returns><see cref="IEnumerable{int}"/>Cписок номеров маршрутных листов водителя</returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<int>))]
		public async Task<IActionResult> GetRouteListsIdsAsync()
		{
			_logger.LogInformation("Запрос доступных МЛ пользователем {Username} User token: {AccessToken}",
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var userName = await _userManager.GetUserNameAsync(user);

			return Ok(_apiRouteListService.GetRouteListsIdsForDriverByAndroidLogin(userName));
		}

		/// <summary>
		/// Возвращения адреса маршрутного листа в статус В пути
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> RollbackRouteListAddressStatusEnRouteAsync([FromBody] RollbackRouteListAddressStatusEnRouteRequest requestDto)
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			_logger.LogInformation("Запрос возврата в путь адреса МЛ {RoutelistAddressId} пользователем {Username} User token: {AccessToken}",
				requestDto.RoutelistAddressId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				tokenStr);

			var recievedTime = DateTime.Now;
			var resultMessage = "OK";

			var localActionTime = requestDto.ActionTimeUtc.ToLocalTime();

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			var timeCheckResult = _actionTimeHelper.CheckRequestTime(recievedTime, localActionTime);

			if(timeCheckResult.IsFailure)
			{
				return MapResult(HttpContext, timeCheckResult, errorStatusCode: StatusCodes.Status400BadRequest);
			}

			try
			{
				_apiRouteListService.RollbackRouteListAddressStatusEnRoute(requestDto.RoutelistAddressId, driver.Id);

				return NoContent();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при возврате в путь адреса МЛ {RoutelistAddressId}: {ExceptionMessage}",
					requestDto.RoutelistAddressId,
					ex.Message);

				resultMessage = ex.Message;

				return Problem("Произошла ошибка при возврате в путь адреса МЛ");
			}
			finally
			{
				_driverMobileAppActionRecordService.RegisterAction(driver,
					DriverMobileAppActionType.RollbackRouteListAddressStatusEnRouteClicked,
					localActionTime,
					recievedTime,
					resultMessage);
			}
		}

		/// <summary>
		/// Принятие условий МЛ
		/// </summary>
		/// <param name="specialConditionsIds">Идентификаторы специальных условий для принятия</param>
		/// <returns></returns>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> AcceptRouteListSpecialConditionsAsync([FromBody] IEnumerable<int> specialConditionsIds)
		{
			_logger.LogInformation("Попытка принятия условий МЛ пользователем {Username} User token: {AccessToken}",
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var userName = await _userManager.GetUserNameAsync(user);

			var employee = _employeeService.GetByAPILogin(userName);

			_routeListService.AcceptConditions(_unitOfWork, employee.Id, specialConditionsIds);

			return NoContent();
		}
	}
}
