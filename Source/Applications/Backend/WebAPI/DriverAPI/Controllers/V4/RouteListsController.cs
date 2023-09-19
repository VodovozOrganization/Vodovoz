using DriverAPI.DTOs.V4;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Контроллер маршрутных листов
	/// </summary>
	[Authorize]
	public class RouteListsController : VersionedController
	{
		private readonly ILogger<RouteListsController> _logger;
		private readonly IRouteListModel _aPIRouteListData;
		private readonly IOrderModel _aPIOrderData;
		private readonly IEmployeeModel _employeeData;
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordModel;
		private readonly IActionTimeHelper _actionTimeHelper;
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="aPIRouteListData"></param>
		/// <param name="aPIOrderData"></param>
		/// <param name="employeeData"></param>
		/// <param name="driverMobileAppActionRecordModel"></param>
		/// <param name="actionTimeHelper">Хелпер-класс для времени</param>
		/// <param name="userManager">Менеджер пользователей</param>
		/// <exception cref="ArgumentNullException"></exception>

		public RouteListsController(
			ILogger<RouteListsController> logger,
			IRouteListModel aPIRouteListData,
			IOrderModel aPIOrderData,
			IEmployeeModel employeeData,
			IDriverMobileAppActionRecordModel driverMobileAppActionRecordModel,
			IActionTimeHelper actionTimeHelper,
			UserManager<IdentityUser> userManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			_aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_driverMobileAppActionRecordModel = driverMobileAppActionRecordModel ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordModel));
			_actionTimeHelper = actionTimeHelper ?? throw new ArgumentNullException(nameof(actionTimeHelper));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
		}

		/// <summary>
		/// Получения детализированнной информации о маршрутных листах и связанных заказах
		/// </summary>
		/// <param name="routeListsIds">Список номеров маршрутных листов</param>
		/// <returns>GetRouteListsDetailsResponseModel</returns>
		[HttpPost("GetRouteListsDetails")]
		[Produces("application/json", Type = typeof(GetRouteListsDetailsResponseDto))]
		public IActionResult Get([FromBody] int[] routeListsIds)
		{
			_logger.LogInformation("Запрос МЛ-ов с деталями: {@RouteListIds} пользователем {Username} User token: {AccessToken}",
				routeListsIds,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var routeLists = _aPIRouteListData.Get(routeListsIds);
			var ordersIds = routeLists.Where(x => x.CompletionStatus == RouteListDtoCompletionStatus.Incompleted)
				.SelectMany(x => x.IncompletedRouteList.RouteListAddresses.Select(x => x.OrderId));

			var orders = _aPIOrderData.Get(ordersIds.ToArray());

			var resortedOrders = new List<OrderDto>();

			foreach(var orderId in ordersIds)
			{
				resortedOrders.Add(orders.Where(o => o.OrderId == orderId).First());
			}

			return Ok(new GetRouteListsDetailsResponseDto()
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
		[HttpGet("GetRouteList")]
		[Produces("application/json", Type = typeof(RouteListDto))]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IActionResult Get(int routeListId)
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			_logger.LogInformation("Запрос информации о МЛ {RouteListId} пользователем {Username} User token: {AccessToken}",
				routeListId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				tokenStr);

			return Ok(_aPIRouteListData.Get(routeListId));
		}

		/// <summary>
		/// Получение номеров маршрутных листов текущего водителя
		/// </summary>
		/// <returns><see cref="IEnumerable{int}"/>Cписок номеров маршрутных листов водителя</returns>
		[HttpGet("GetRouteListsIds")]
		[Produces("application/json", Type = typeof(IEnumerable<int>))]
		public async Task<IActionResult> GetIds()
		{
			_logger.LogInformation("Запрос доступных МЛ пользователем {Username} User token: {AccessToken}",
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var userName = await _userManager.GetUserNameAsync(user);

			return Ok(_aPIRouteListData.GetRouteListsIdsForDriverByAndroidLogin(userName));
		}

		/// <summary>
		/// Возвращения адреса маршрутного листа в статус В пути
		/// </summary>
		/// <returns></returns>
		[HttpPost("RollbackRouteListAddressStatusEnRoute")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> RollbackRouteListAddressStatusEnRouteAsync([FromBody] RollbackRouteListAddressStatusEnRouteRequestDto requestDto)
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			_logger.LogInformation("Запрос возврата в путь адреса МЛ {RoutelistAddressId} пользователем {Username} User token: {AccessToken}",
				requestDto.RoutelistAddressId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;
			var resultMessage = "OK";

			var localActionTime = requestDto.ActionTimeUtc.ToLocalTime();

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, localActionTime);
				_aPIRouteListData.RollbackRouteListAddressStatusEnRoute(requestDto.RoutelistAddressId, driver.Id);

				return Ok(resultMessage);
			}
			catch(Exception ex)
			{
				resultMessage = ex.Message;
				throw;
			}
			finally
			{
				_driverMobileAppActionRecordModel.RegisterAction(driver,
					DriverMobileAppActionType.RollbackRouteListAddressStatusEnRouteClicked,
					localActionTime,
					recievedTime,
					resultMessage);
			}
		}
	}
}
