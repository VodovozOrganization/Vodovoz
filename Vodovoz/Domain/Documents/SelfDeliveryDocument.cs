using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using System.Linq;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "отпуски самовывоза",
		Nominative = "отпуск самовывоза")]
	public class SelfDeliveryDocument: Document
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				if (!NHibernate.NHibernateUtil.IsInitialized(Items))
					return;
				foreach (var item in Items) {
					if (item.WarehouseMovementOperation != null && item.WarehouseMovementOperation.OperationTime != TimeStamp)
						item.WarehouseMovementOperation.OperationTime = TimeStamp;
				}
			}
		}

		Order order;

		public virtual Order Order {
			get { return order; } 
			set { SetField (ref order, value, () => Order); }
		}

		Warehouse warehouse;

		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField (ref warehouse, value, () => Warehouse); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		IList<SelfDeliveryDocumentItem> items = new List<SelfDeliveryDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<SelfDeliveryDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<SelfDeliveryDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SelfDeliveryDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<SelfDeliveryDocumentItem> (Items);
				return observableItems;
			}
		}

		IList<SelfDeliveryDocumentReturned> returnedItems = new List<SelfDeliveryDocumentReturned> ();

		[Display (Name = "Строки возврата")]
		public virtual IList<SelfDeliveryDocumentReturned> ReturnedItems {
			get { return returnedItems; }
			set {
				SetField (ref returnedItems, value, () => ReturnedItems);
			}
		}

		#region Не сохраняемые

		public virtual string Title { 
			get { return String.Format ("Самовывоз №{0} от {1:d}", Id, TimeStamp); }
		}

		#endregion

		#region Функции

		public virtual void AddItem (SelfDeliveryDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add (item);
		}

		public virtual void FillByOrder()
		{
			ObservableItems.Clear();
			if (Order == null)
				return;

			foreach(var orderItem in Order.OrderItems)
			{
				if (!Nomenclature.GetCategoriesForSale().Contains(orderItem.Nomenclature.Category) 
					&& orderItem.Nomenclature.Category != NomenclatureCategory.equipment)
					continue;
				ObservableItems.Add(new SelfDeliveryDocumentItem(){
					Document = this,
					Nomenclature = orderItem.Nomenclature,
					OrderItem = orderItem,
					Amount = orderItem.Count
				});
			}

			foreach(var orderItem in Order.OrderEquipments.Where(x => x.Direction == Direction.Deliver))
			{
				ObservableItems.Add(new SelfDeliveryDocumentItem(){
					Document = this,
					Nomenclature = orderItem.Equipment.Nomenclature,
					Equipment = orderItem.Equipment,
					Amount = 1
				});
			}
		}

		public virtual void UpdateStockAmount(IUnitOfWork uow)
		{
			if (Items.Count == 0 || Warehouse == null)
				return;
			var nomenclatureIds = Items.Select(x => x.Nomenclature.Id).ToArray();
			var inStock = Repository.StockRepository.NomenclatureInStock(uow, Warehouse.Id, 
				nomenclatureIds, TimeStamp);

			foreach(var item in Items)
			{
				item.AmountInStock = inStock[item.Nomenclature.Id];
			}
		}

		public virtual void UpdateAlreadyUnloaded(IUnitOfWork uow)
		{
			if (Items.Count == 0 || Order == null)
				return;
			
			var inUnloaded = Repository.Store.SelfDeliveryRepository.NomenclatureUnloaded(uow, Order, this);

			foreach(var item in Items)
			{
				if(inUnloaded.ContainsKey(item.Nomenclature.Id))
					item.AmountUnloaded = inUnloaded[item.Nomenclature.Id];
			}
		}

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			foreach(var item in Items)
			{
				if(item.Amount == 0 && item.WarehouseMovementOperation != null)
				{
					uow.Delete(item.WarehouseMovementOperation);
					item.WarehouseMovementOperation = null;
				}
				if(item.Amount != 0)
				{
					if(item.WarehouseMovementOperation != null)
					{
						item.UpdateOperation(Warehouse);
					}
					else
					{
						item.CreateOperation(Warehouse, TimeStamp);
					}
				}
			}
		}

		public virtual void UpdateReturnedOperations(IUnitOfWork uow, Dictionary<int, decimal> returnedNomenclatures)
		{
			foreach(var returned in returnedNomenclatures)
			{
				var item = ReturnedItems.FirstOrDefault(x => x.Nomenclature.Id == returned.Key);
				if(item == null && returned.Value != 0)
				{
					item = new SelfDeliveryDocumentReturned()
					{
						Amount = returned.Value,
						Document = this,
						Nomenclature = uow.GetById<Nomenclature>(returned.Key)
					};
					item.CreateOperation(Warehouse, TimeStamp);
					ReturnedItems.Add(item);
				}
				else if(item != null && returned.Value == 0)
				{
					ReturnedItems.Remove(item);
				}
				else if(item != null && returned.Value != 0)
				{
					item.UpdateOperation(Warehouse);
				}
			}
		}

		public virtual bool ShipIfCan()
		{
			bool closed = Items.All(x => (x.OrderItem != null ? x.OrderItem.Count : 1) == x.Amount + x.AmountUnloaded);
			if (closed)
				Order.Close();
			return closed;
		}

		#endregion
	}
}

