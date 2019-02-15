using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repository.Store
{
	public static class CarUnloadRepository
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public static Dictionary<int, decimal> NomenclatureUnloaded(IUnitOfWork UoW, RouteList routeList, Warehouse warehouse, CarUnloadDocument excludeDoc)
		{
			CarUnloadDocument docAlias = null;
			CarUnloadDocumentItem docItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;


			var unloadedlist = UoW.Session.QueryOver<CarUnloadDocument>(() => docAlias)
								  .Where(d => d.RouteList.Id == routeList.Id)
								  .Where(d => d.Warehouse.Id == warehouse.Id)
								  .Where(d => d.Id != excludeDoc.Id)
								  .JoinAlias(d => d.Items, () => docItemsAlias)
								  .JoinAlias(() => docItemsAlias.MovementOperation, () => movementOperationAlias)
				.SelectList(list => list
							.SelectGroup(() => movementOperationAlias.Nomenclature.Id)
							.SelectSum(() => movementOperationAlias.Amount)
						   ).List<object[]>();
			return unloadedlist.ToDictionary(r => (int)r[0], r => (decimal)r[1]);
		}

		public static bool IsUniqDocument(IUnitOfWork UoW, RouteList routeList, Warehouse warehouse,int documentId)
		{
			if(documentId != 0)
				return true;

			CarUnloadDocument carUnloadDocument = null;
			var getSimilarCarUnloadDoc = QueryOver.Of<CarUnloadDocument>(() => carUnloadDocument)
									.Where(() => carUnloadDocument.RouteList.Id == routeList.Id)
									.Where(() => carUnloadDocument.Warehouse.Id == warehouse.Id);
			IList<CarUnloadDocument> documents = getSimilarCarUnloadDoc.GetExecutableQueryOver(UoW.Session)
				.List();

			if(documents.Count > 0)
				return false;
			else
				return true;
		}
	}
}
