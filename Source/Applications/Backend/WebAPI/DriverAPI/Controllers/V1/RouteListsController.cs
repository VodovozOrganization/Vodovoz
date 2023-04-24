using DriverAPI.DTOs.V1;
using DriverAPI.Library.Deprecated.DTOs;
using DriverAPI.Library.Deprecated.Models;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic.Drivers;
using IOrderModel = DriverAPI.Library.Deprecated.Models.IOrderModel;
using OrderDto = DriverAPI.Library.Deprecated.DTOs.OrderDto;

namespace DriverAPI.Controllers.V1
{
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	[Authorize]
	public class RouteListsController : ControllerBase
	{
		private readonly ILogger<RouteListsController> _logger;
		private readonly IRouteListModel _aPIRouteListData;
		private readonly IOrderModel _aPIOrderData;
		private readonly IEmployeeModel _employeeData;
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordModel;
		private readonly IActionTimeHelper _actionTimeHelper;
		private readonly UserManager<IdentityUser> _userManager;

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
		/// Эндпоинт получения информации о маршрутных листах (МЛ) и его не зввершенных заказах
		/// В ответе сервера будет JSON объект с полями соответствующими GetRouteListsDetailsResponseModel и статусом 200
		/// </summary>
		/// <param name="routeListsIds">Список идентификаторов МЛ</param>
		/// <returns>GetRouteListsDetailsResponseModel</returns>
		[HttpPost]
		[Route("GetRouteListsDetails")]
		[Route("/api/GetRouteListsDetails")]
		public GetRouteListsDetailsResponseDto Get([FromBody] int[] routeListsIds)
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

			return new GetRouteListsDetailsResponseDto()
			{
				RouteLists = routeLists,
				Orders = resortedOrders
			};
		}

		/// <summary>
		/// Эндпоинт получения информации о маршрутном листе (МЛ)
		/// В ответе сервера будет JSON объект с полями соответствующими APIRouteList и статусом 200
		/// Или пустой ответ с кодом 204
		/// </summary>
		/// <param name="routeListId">Идентификатор МЛ</param>
		/// <returns>APIRouteList или null</returns>
		[HttpGet]
		[Route("GetRouteList")]
		[Route("/api/GetRouteList")]
		public RouteListDto Get(int routeListId)
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			_logger.LogInformation("Запрос информации о МЛ {RouteListId} пользователем {Username} User token: {AccessToken}",
				routeListId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			return _aPIRouteListData.Get(routeListId);
		}

		/// <summary>
		/// Эндпоинт получения МЛ текущего водителя
		/// </summary>
		/// <returns>IEnumerable<int> - список идентификаторов МЛ</returns>
		[HttpGet]
		[Route("GetRouteListsIds")]
		[Route("/api/GetRouteListsIds")]
		public async Task<IEnumerable<int>> GetIds()
		{
			_logger.LogInformation("Запрос доступных МЛ пользователем {Username} User token: {AccessToken}",
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var userName = await _userManager.GetUserNameAsync(user);

			return _aPIRouteListData.GetRouteListsIdsForDriverByAndroidLogin(userName);
		}

		/// <summary>
		/// Эндпоинт возвращения адреса МЛ в Путь
		/// </summary>
		/// <param name="routelistAddressId">идентификатор адреса МЛ</param>
		/// <returns></returns>
		[HttpPost]
		[Route("RollbackRouteListAddressStatusEnRoute")]
		[Route("/api/RollbackRouteListAddressStatusEnRoute")]
		public async Task RollbackRouteListAddressStatusEnRouteAsync([FromBody] RollbackRouteListAddressStatusEnRouteRequestDto requestDto)
		{
			var tokenStr = Request.Headers[HeaderNames.Authorization];
			_logger.LogInformation("Запрос возврата в путь адреса МЛ {RoutelistAddressId} пользователем {Username} User token: {AccessToken}",
				requestDto.RoutelistAddressId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;
			var resultMessage = "OK";

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			var actionTime = _actionTimeHelper.GetActionTime(requestDto);

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, actionTime);
				_aPIRouteListData.RollbackRouteListAddressStatusEnRoute(requestDto.RoutelistAddressId, driver.Id);
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
					actionTime,
					recievedTime,
					resultMessage);
			}
		}
	}
}
