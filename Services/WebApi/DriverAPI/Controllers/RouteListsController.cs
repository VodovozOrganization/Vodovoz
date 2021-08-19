using DriverAPI.Library.Models;
using DriverAPI.Library.DTOs;
using DriverAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
		private readonly IRouteListModel _aPIRouteListData;
		private readonly IOrderModel _aPIOrderData;
		private readonly UserManager<IdentityUser> _userManager;

		public RouteListsController(
			IRouteListModel aPIRouteListData,
			IOrderModel aPIOrderData,
			UserManager<IdentityUser> userManager)
		{
			_aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			_aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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
		public async Task RollbackRouteListAddressStatusEnRoute([FromBody]int routelistAddressId)
		{
			_aPIRouteListData.RollbackRouteListAddressStatusEnRoute(routelistAddressId);
		}
	}
}
