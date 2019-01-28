using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.Dialogs.Store
{
	public static class CarLoadRepository
	{
		public static bool IsUniqDocument(IUnitOfWork UoW, RouteList routeList, Warehouse warehouse,int id)
		{
			if(id != 0)
				return true;

			CarLoadDocument carLoadDocument = null;
			var getSimilarCarUnloadDoc = QueryOver.Of<CarLoadDocument>(() => carLoadDocument)
									.Where(() => carLoadDocument.RouteList.Id == routeList.Id)
									.Where(() => carLoadDocument.Warehouse.Id == warehouse.Id);
			IList<CarLoadDocument> documents = getSimilarCarUnloadDoc.GetExecutableQueryOver(UoW.Session)
				.List();
				
			if(documents.Count > 0)
				return false;
			else
				return true;
		}
	}
}
