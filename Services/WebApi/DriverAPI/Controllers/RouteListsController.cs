using DriverAPI.DTOs;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class RouteListsController : ControllerBase
	{
		private readonly ILogger<RouteListsController> _logger;
		private readonly IRouteListModel _aPIRouteListData;
		private readonly IOrderModel _aPIOrderData;
		private readonly IEmployeeModel _employeeData;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly int _timeout;
		private readonly int _futureTimeout;

		public RouteListsController(
			ILogger<RouteListsController> logger,
			IConfiguration configuration,
			IRouteListModel aPIRouteListData,
			IOrderModel aPIOrderData,
			IEmployeeModel employeeData,
			UserManager<IdentityUser> userManager)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			_aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_timeout = configuration.GetValue<int>("PostActionTimeTimeOut");
			_futureTimeout = configuration.GetValue<int>("FutureAtionTimeTimeOut");
		}

		/// <summary>
		/// Эндпоинт получения информации о маршрутных листах (МЛ) и его не зввершенных заказах
		/// В ответе сервера будет JSON объект с полями соответствующими GetRouteListsDetailsResponseModel и статусом 200
		/// </summary>
		/// <param name="routeListsIds">Список идентификаторов МЛ</param>
		/// <returns>GetRouteListsDetailsResponseModel</returns>
		[HttpPost]
		[Route("/api/GetRouteListsDetails")]
		public GetRouteListsDetailsResponseDto Get([FromBody] int[] routeListsIds)
		{
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
		[Route("/api/GetRouteList")]
		public RouteListDto Get(int routeListId)
		{
			return _aPIRouteListData.Get(routeListId);
		}

		/// <summary>
		/// Эндпоинт получения МЛ текущего водителя
		/// </summary>
		/// <returns>IEnumerable<int> - список идентификаторов МЛ</returns>
		[HttpGet]
		[Route("/api/GetRouteListsIds")]
		public async Task<IEnumerable<int>> GetIds()
		{
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
		[Route("/api/RollbackRouteListAddressStatusEnRoute")]
		public void RollbackRouteListAddressStatusEnRoute([FromBody] RollbackRouteListAddressStatusEnRouteRequestDto requestDto)
		{
			_logger.LogInformation($"Попытка вернуть в путь адрес МЛ: { requestDto.RoutelistAddressId } пользователем {HttpContext.User.Identity?.Name ?? "Unknown"}");

			var recievedTime = DateTime.Now;

			if(requestDto.ActionTime < recievedTime.AddMinutes(-_futureTimeout))
			{
				throw new InvalidTimeZoneException("Нельзя отправлять запросы из будущего! Проверьте настройки системного времени вашего телефона");
			}

			if(recievedTime - requestDto.ActionTime > new TimeSpan(0, _timeout, 0))
			{
				throw new InvalidOperationException("Таймаут запроса операции");
			}

			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);

			_aPIRouteListData.RollbackRouteListAddressStatusEnRoute(requestDto.RoutelistAddressId, driver.Id);
		}
	}
}
