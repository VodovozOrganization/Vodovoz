﻿using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.EntityRepositories.Store
{
	public class CarLoadDocumentRepository : ICarLoadDocumentRepository
    {
		private readonly IRouteListRepository _routeListRepository;

		public CarLoadDocumentRepository(IRouteListRepository routeListRepository)
		{
			_routeListRepository = routeListRepository ?? throw new System.ArgumentNullException(nameof(routeListRepository));
		}

        public decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId)
        {
            CarLoadDocument carLoadDocumentAlias = null;
            CarLoadDocumentItem carLoadDocumentItemAlias = null;

            var query = uow.Session.QueryOver(() => carLoadDocumentAlias)
                                    .JoinAlias(c => c.Items, () => carLoadDocumentItemAlias)
                                    .Where(() => carLoadDocumentAlias.RouteList.Id == routelistId)
                                    .And(() => carLoadDocumentItemAlias.Nomenclature.Id == terminalId)
                                    .Select(Projections.Sum(() => carLoadDocumentItemAlias.Amount))
                                    .SingleOrDefault<decimal>()
						+ _routeListRepository.TerminalTransferedCountToRouteList(uow, uow.GetById<RouteList>(routelistId));

            return query;
        }
    }
}
