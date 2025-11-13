using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.Infrastructure.Persistance.Store
{
	internal sealed class SelfDeliveryRepository : ISelfDeliveryRepository
	{
		public Dictionary<int, decimal> NomenclatureUnloaded(IUnitOfWork uow, Order order, SelfDeliveryDocument excludeDoc)
		{
			SelfDeliveryDocument docAlias = null;
			SelfDeliveryDocumentItem docItemsAlias = null;

			ItemInStock inUnload = null;
			var unloadedlist = uow.Session.QueryOver(() => docAlias)
				.Where(d => d.Order.Id == order.Id)
				.Where(d => d.Id != excludeDoc.Id)
				.JoinAlias(d => d.Items, () => docItemsAlias)
				.SelectList(list => list
					.SelectGroup(() => docItemsAlias.Nomenclature.Id).WithAlias(() => inUnload.Id)
					.SelectSum(() => docItemsAlias.Amount).WithAlias(() => inUnload.Added)
				).TransformUsing(Transformers.PassThrough).List<object[]>();
			var result = new Dictionary<int, decimal>();
			foreach(var unloadedItem in unloadedlist)
			{
				result.Add((int)unloadedItem[0], (decimal)unloadedItem[1]);
			}
			return result;
		}

		public Dictionary<int, decimal> OrderNomenclaturesLoaded(IUnitOfWork uow, Order order)
		{
			SelfDeliveryDocument docAlias = null;
			SelfDeliveryDocumentItem docItemsAlias = null;

			ItemInStock inLoaded = null;
			var loadedlist = uow.Session.QueryOver(() => docAlias)
				.Where(d => d.Order.Id == order.Id)
				.JoinAlias(d => d.Items, () => docItemsAlias)
				.SelectList(list => list
				   .SelectGroup(() => docItemsAlias.Nomenclature.Id).WithAlias(() => inLoaded.Id)
				   .SelectSum(() => docItemsAlias.Amount).WithAlias(() => inLoaded.Added)
				).TransformUsing(Transformers.PassThrough).List<object[]>();
			var result = new Dictionary<int, decimal>();
			foreach(var loadedItem in loadedlist)
			{
				result.Add((int)loadedItem[0], (decimal)loadedItem[1]);
			}
			return result;
		}

		public Dictionary<int, decimal> OrderNomenclaturesUnloaded(IUnitOfWork uow, Order order, SelfDeliveryDocument notSavedDoc = null)
		{
			SelfDeliveryDocumentItem docItemsAlias = null;
			ItemInStock inUnload = null;

			var unloadedQuery = uow.Session.QueryOver<SelfDeliveryDocument>()
								  .JoinAlias(d => d.Items, () => docItemsAlias)
								  .Where(d => d.Order.Id == order.Id);

			var unloadedDict = unloadedQuery.SelectList(list => list
				   .SelectGroup(() => docItemsAlias.Nomenclature.Id).WithAlias(() => inUnload.Id)
				   .SelectSum(() => docItemsAlias.Amount).WithAlias(() => inUnload.Added)
				)
				.TransformUsing(Transformers.PassThrough)
				.List<object[]>()
				.GroupBy(o => (int)o[0], o => (decimal)o[1])
				.ToDictionary(g => g.Key, g => g.Sum());

			if(notSavedDoc != null && notSavedDoc.Id <= 0)
			{
				foreach(var i in notSavedDoc.Items)
				{
					if(unloadedDict.ContainsKey(i.Nomenclature.Id))
						unloadedDict[i.Nomenclature.Id] += i.Amount;
					else
						unloadedDict.Add(i.Nomenclature.Id, i.Amount);
				}
			}

			return unloadedDict;
		}

		public bool IsSelfDeliveryDocumentItemsUsedInEdoTasks(IUnitOfWork uow, int selfDeliveryDocumentId)
		{
			var edoTasks =
				(from selfDeliveryDocument in uow.Session.Query<SelfDeliveryDocument>()
				 join selfDeliveryDocumentItem in uow.Session.Query<SelfDeliveryDocumentItem>()
				 on selfDeliveryDocument.Id equals selfDeliveryDocumentItem.Document.Id
				 join trueMarkProductCode in uow.Session.Query<SelfDeliveryDocumentItemTrueMarkProductCode>()
				 on selfDeliveryDocumentItem.Id equals trueMarkProductCode.SelfDeliveryDocumentItem.Id
				 join edoTaskItem in uow.Session.Query<EdoTaskItem>() on trueMarkProductCode.Id equals edoTaskItem.ProductCode.Id
				 where selfDeliveryDocument.Id == selfDeliveryDocumentId
				 select edoTaskItem)
				 .ToList();

			return edoTasks.Any();
		}
	}
}
