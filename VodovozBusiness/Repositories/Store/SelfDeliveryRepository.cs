using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Repository.Store
{
	public static class SelfDeliveryRepository
	{
		public static Dictionary<int,decimal> NomenclatureUnloaded(IUnitOfWork UoW, Order order, SelfDeliveryDocument excludeDoc)
		{
			SelfDeliveryDocument docAlias = null;
			SelfDeliveryDocumentItem docItemsAlias = null;

			ItemInStock inUnload = null;
			var unloadedlist = UoW.Session.QueryOver<SelfDeliveryDocument> (() => docAlias)
				.Where (d => d.Order.Id == order.Id)
				.Where(d => d.Id != excludeDoc.Id)
				.JoinAlias(d => d.Items, () => docItemsAlias)
				.SelectList (list => list
					.SelectGroup (() => docItemsAlias.Nomenclature.Id).WithAlias (() => inUnload.Id)
					.SelectSum (() => docItemsAlias.Amount).WithAlias (() => inUnload.Added)
				).TransformUsing (Transformers.PassThrough).List<object[]> ();
			var result = new Dictionary<int,decimal> ();
			foreach (var unloadedItem in unloadedlist) {
				result.Add ((int) unloadedItem[0], (decimal) unloadedItem[1]);
			}
			return result;
		}

		public static Dictionary<int, decimal> OrderNomenclaturesLoaded(IUnitOfWork UoW, Order order)
		{
			SelfDeliveryDocument docAlias = null;
			SelfDeliveryDocumentItem docItemsAlias = null;

			ItemInStock inLoaded = null;
			var loadedlist = UoW.Session.QueryOver<SelfDeliveryDocument>(() => docAlias)
				.Where(d => d.Order.Id == order.Id)
				.JoinAlias(d => d.Items, () => docItemsAlias)
				.SelectList(list => list
				   .SelectGroup(() => docItemsAlias.Nomenclature.Id).WithAlias(() => inLoaded.Id)
				   .SelectSum(() => docItemsAlias.Amount).WithAlias(() => inLoaded.Added)
				).TransformUsing(Transformers.PassThrough).List<object[]>();
			var result = new Dictionary<int, decimal>();
			foreach(var loadedItem in loadedlist) {
				result.Add((int)loadedItem[0], (decimal)loadedItem[1]);
			}
			return result;
		}

		/// <summary>
		/// Выводит список id номенклатур в заказе и количество отгруженное со склада 
		/// по документам отгрузки, включая документ отгрузки, который ещё не сохранён
		/// </summary>
		/// <returns>Id товара и сколько его отгружено</returns>
		/// <param name="order">Заказ по которому необходимо найти отгрузку товаров</param>
		/// <param name="notSavedDoc">Не сохранённый документ для включения в расчёт</param>
		public static Dictionary<int, decimal> OrderNomenclaturesUnloaded(IUnitOfWork UoW, Order order, SelfDeliveryDocument notSavedDoc = null)
		{
			SelfDeliveryDocumentItem docItemsAlias = null;
			ItemInStock inUnload = null;

			var unloadedQuery = UoW.Session.QueryOver<SelfDeliveryDocument>()
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

			if(notSavedDoc != null && notSavedDoc.Id <= 0) {
				foreach(var i in notSavedDoc.Items) {
					if(unloadedDict.ContainsKey(i.Nomenclature.Id))
						unloadedDict[i.Nomenclature.Id] += i.Amount;
					else
						unloadedDict.Add(i.Nomenclature.Id, i.Amount);
				}
			}

			return unloadedDict;
		}
	}
}