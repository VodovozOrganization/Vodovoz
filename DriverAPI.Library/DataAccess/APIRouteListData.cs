using DriverAPI.Library.Converters;
using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Logistic;

namespace DriverAPI.Library.DataAccess
{
    public class APIRouteListData : IAPIRouteListData
    {
        private readonly ILogger<APIRouteListData> logger;
        private readonly IRouteListRepository routeListRepository;
        private readonly RouteListConverter routeListConverter;
        private readonly IUnitOfWork unitOfWork;

        public APIRouteListData(ILogger<APIRouteListData> logger,
            IRouteListRepository routeListRepository,
            RouteListConverter routeListConverter,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
            this.routeListConverter = routeListConverter ?? throw new ArgumentNullException(nameof(routeListConverter));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public APIRouteList Get(int routeListId) => Get(new[] { routeListId }).FirstOrDefault();

        public IEnumerable<APIRouteList> Get(int[] routeListsIds)
        {
            var vodovozRouteLists = routeListRepository.GetRouteLists(unitOfWork, routeListsIds);

            var routeLists = new List<APIRouteList>();

            foreach (var routelist in vodovozRouteLists)
            {
                try
                {
                    routeLists.Add(routeListConverter.convertToAPIRouteList(routelist));
                }
                catch (ArgumentException e)
                {
                    logger.LogWarning(e, $"Ошибка конвертирования маршрутного листа {routelist.Id}");
                }
            }

            return routeLists;
        }
    }
}
