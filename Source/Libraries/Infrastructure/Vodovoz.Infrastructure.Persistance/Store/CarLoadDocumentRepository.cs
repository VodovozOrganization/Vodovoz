using Core.Infrastructure;
using NHibernate.Criterion;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.Infrastructure.Persistance.Store
{
	internal sealed class CarLoadDocumentRepository : ICarLoadDocumentRepository
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

		public async Task<IEnumerable<CarLoadDocument>> GetCarLoadDocumentsById(IUnitOfWork uow, int carLoadDocumentId)
		{
			var documents = uow.Session.Query<CarLoadDocument>()
				.Where(d => d.Id == carLoadDocumentId);

			return await documents.ToListAsync();
		}

		public async Task<IEnumerable<CarLoadDocumentItem>> GetAccountableInTrueMarkHavingGtinItemsByCarLoadDocumentId(
			IUnitOfWork uow,
			int orderId)
		{
			var documentItems =
				from documentItem in uow.Session.Query<CarLoadDocumentItem>()
				join nomenclature in uow.Session.Query<Nomenclature>() on documentItem.Nomenclature.Id equals nomenclature.Id
				where
					documentItem.OrderId == orderId
					&& nomenclature.IsAccountableInTrueMark
				select documentItem;

			return await documentItems.ToListAsync();
		}

		public CarLoadDocumentLoadingProcessAction GetLastLoadingProcessActionByDocumentId(IUnitOfWork uow, int documentId)
		{
			var documentActions =
				from action in uow.Session.Query<CarLoadDocumentLoadingProcessAction>()
				where action.CarLoadDocumentId == documentId
				select action;

			var lastAction =
				documentActions
				.OrderByDescending(x => x.Id)
				.FirstOrDefault();

			return lastAction;
		}
	}
}
