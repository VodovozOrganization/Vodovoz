using DriverAPI.Library.DataAccess;
using DriverAPI.Library.Models;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IAPIRouteListData aPIRouteListData;
        private readonly IAPIOrderData aPIOrderData;

        public RouteListsController(IAPIRouteListData aPIRouteListData, IAPIOrderData aPIOrderData)
        {
            this.aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
            this.aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
        }

        /// <summary>
        /// Эндпоинт получения информации о маршрутных листах (МЛ) и его не зввершенных заказах
        /// В ответе сервера будет JSON объект с полями соответствующими GetRouteListsDetailsResponseModel и статусом 200
        /// </summary>
        /// <param name="routeListsIds">Список идентификаторов МЛ</param>
        /// <returns>GetRouteListsDetailsResponseModel</returns>
        [HttpGet]
        [Route("/api/GetRouteListsDetails")]
        public GetRouteListsDetailsResponseModel Get([FromBody] int[] routeListsIds)
        {
            var routeLists = aPIRouteListData.Get(routeListsIds);
            var ordersIds = routeLists.Where(x => x.CompletionStatusEnum == APIRouteListCompletionStatus.Incompleted)
                .SelectMany(x => x.IncompletedRouteList.RouteListAddresses.Select(x => x.OrderId));

            var orders = aPIOrderData.Get(ordersIds.ToArray());

            return new GetRouteListsDetailsResponseModel()
                {
                    RouteLists = routeLists,
                    Orders = orders
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
        public APIRouteList Get([FromBody] int routeListId)
        {
            return aPIRouteListData.Get(routeListId);
        }

        /// <summary>
        /// Эндпоинт получения МЛ текущего водителя
        /// </summary>
        /// <returns>IEnumerable<int> - список идентификаторов МЛ</returns>
        [HttpGet]
        [Route("/api/GetRouteListsIds")]
        public IEnumerable<int> GetIds()
        {
            return new int[] { 1, 2, 3, 4, 5 };
        }
    }
}
