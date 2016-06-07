using System;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using NHibernate.Transform;

namespace Vodovoz.Repository.Store
{
	public static class SelfDeliveryRepository
	{
		public static Dictionary<int,decimal> NomenclatureUnloaded(IUnitOfWork UoW, Order order, SelfDeliveryDocument excludeDoc)
		{
			SelfDeliveryDocument docAlias = null;
			SelfDeliveryDocumentItem docItemsAlias = null;

			ItemInStock inUnload = null;
			var unliadedlist = UoW.Session.QueryOver<SelfDeliveryDocument> (() => docAlias)
				.Where (d => d.Order.Id == order.Id)
				.Where(d => d.Id != excludeDoc.Id)
				.JoinAlias(d => d.Items, () => docItemsAlias)
				.SelectList (list => list
					.SelectGroup (() => docItemsAlias.Nomenclature.Id).WithAlias (() => inUnload.Id)
					.SelectSum (() => docItemsAlias.Amount).WithAlias (() => inUnload.Added)
				).TransformUsing (Transformers.PassThrough).List<object[]> ();
			var result = new Dictionary<int,decimal> ();
			foreach (var unloadedItem in unliadedlist) {
				result.Add ((int) unloadedItem[0], (decimal) unloadedItem[1]);
			}
			return result;			      
		}
	}
}

