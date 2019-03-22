using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repository.Store
{
	public static class CarLoadRepository
	{
		public static bool IsUniqDocument(IUnitOfWork UoW, RouteList routeList, Warehouse warehouse, int documentId)
		{
			if(documentId != 0)
				return true;

			var getSimilarCarUnloadDoc = QueryOver.Of<CarLoadDocument>()
												  .Where(d => d.RouteList.Id == routeList.Id)
												  .Where(d => d.Warehouse.Id == warehouse.Id);
			IList<CarLoadDocument> documents = getSimilarCarUnloadDoc.GetExecutableQueryOver(UoW.Session).List();

			return !documents.Any();
		}
	}
}
