using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
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
		/// Выводит список id товара в заказе и количество отгруженное со склада 
		/// по документам отгрузки, исключая указанный документ открузки
		/// </summary>
		/// <returns>Id товара и сколько его отгружено</returns>
		/// <param name="order">Заказ по которому необходимо найти отгрузку товаров</param>
		/// <param name="excludeDoc">Исключаемый документ отгрузки</param>
		public static Dictionary<int, decimal> OrderItemUnloaded(IUnitOfWork UoW, Order order, SelfDeliveryDocument excludeDoc)
		{
			SelfDeliveryDocument docAlias = null;
			SelfDeliveryDocumentItem docItemsAlias = null;

			ItemInStock inUnload = null;
			var unloadedQuery = UoW.Session.QueryOver<SelfDeliveryDocument>(() => docAlias)
								  .JoinAlias(d => d.Items, () => docItemsAlias)
								  .Where(d => d.Order.Id == order.Id)
								  .WhereRestrictionOn(() => docItemsAlias.OrderItem).IsNotNull;
			if(excludeDoc != null && excludeDoc.Id != 0) {
				unloadedQuery.Where(d => d.Id != excludeDoc.Id);
			}

			var unloadedlist = unloadedQuery.SelectList(list => list
				   .SelectGroup(() => docItemsAlias.OrderItem.Id).WithAlias(() => inUnload.Id)
				   .SelectSum(() => docItemsAlias.Amount).WithAlias(() => inUnload.Added)
				).TransformUsing(Transformers.PassThrough).List<object[]>();

			var result = new Dictionary<int, decimal>();
			foreach(var unloadedItem in unloadedlist) {
				result.Add((int)unloadedItem[0], (decimal)unloadedItem[1]);
			}
			return result;
		}

		public static Dictionary<int, decimal> OrderEquipmentsUnloaded(IUnitOfWork UoW, Order order, SelfDeliveryDocument excludeDoc)
		{
			SelfDeliveryDocument docAlias = null;
			SelfDeliveryDocumentItem docItemsAlias = null;

			ItemInStock inUnload = null;
			var unloadedQuery = UoW.Session.QueryOver<SelfDeliveryDocument>(() => docAlias)
								  .JoinAlias(d => d.Items, () => docItemsAlias)
								  .Where(d => d.Order.Id == order.Id)
								  .WhereRestrictionOn(() => docItemsAlias.OrderEquipment).IsNotNull;
			if(excludeDoc != null && excludeDoc.Id != 0) {
				unloadedQuery.Where(d => d.Id != excludeDoc.Id);
			}
			var unloadedlist = unloadedQuery.SelectList(list => list
			                                  .SelectGroup(() => docItemsAlias.OrderEquipment.Id).WithAlias(() => inUnload.Id)
			                                  .SelectSum(() => docItemsAlias.Amount).WithAlias(() => inUnload.Added)
			                                 ).TransformUsing(Transformers.PassThrough).List<object[]>();
			var result = new Dictionary<int, decimal>();
			foreach(var unloadedItem in unloadedlist) {
				result.Add((int)unloadedItem[0], (decimal)unloadedItem[1]);
			}
			return result;
		}
	}
}

