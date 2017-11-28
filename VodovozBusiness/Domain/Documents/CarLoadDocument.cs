using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "документы погрузки автомобилей",
		Nominative = "документ погрузки автомобиля")]
	public class CarLoadDocument: Document, IValidatableObject
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				if (!NHibernate.NHibernateUtil.IsInitialized(Items))
					return;
				foreach (var item in Items) {
					if (item.MovementOperation != null && item.MovementOperation.OperationTime != TimeStamp)
						item.MovementOperation.OperationTime = TimeStamp;
				}
			}
		}
			
		RouteList routeList;

		public virtual RouteList RouteList {
			get { return routeList; }
			set { SetField (ref routeList, value, () => RouteList); }
		}

		Warehouse warehouse;

		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField (ref warehouse, value, () => Warehouse); }
		}


		IList<CarLoadDocumentItem> items = new List<CarLoadDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<CarLoadDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<CarLoadDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CarLoadDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<CarLoadDocumentItem> (Items);
				return observableItems;
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		public virtual string Title { 
			get { return String.Format ("Талон погрузки №{0} от {1:d}", Id, TimeStamp); }
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Author == null)
				yield return new ValidationResult ("Не указан кладовщик.",
					new[] { this.GetPropertyName (o => o.Author) });
			if (RouteList == null)
				yield return new ValidationResult ("Не указан маршрутный лист, по которому осуществляется отгрузка.",
					new[] { this.GetPropertyName (o => o.RouteList)});

			if(Items.All(x => x.Amount == 0))
				yield return new ValidationResult (String.Format("В документе нет позиций с количеством больше нуля."),
					new[] { this.GetPropertyName (o => o.Items) });

			foreach(var item in Items)
			{
				if(item.Amount > item.AmountInStock)
					yield return new ValidationResult (String.Format("На складе недостаточное количество <{0}>", item.Nomenclature.Name),
						new[] { this.GetPropertyName (o => o.Items) });
				if(item.Equipment != null && !(item.Amount == 0 || item.Amount == 1))
					yield return new ValidationResult (String.Format("Оборудование <{0}> сн: {1} нельзя отгружать в количестве отличном от 0 или 1", item.Nomenclature.Name, item.Equipment.Serial),
						new[] { this.GetPropertyName (o => o.Items) });
				if(item.Amount + item.AmountLoaded > item.AmountInRouteList)
					yield return new ValidationResult (String.Format("Номенклатура <{0}> отгружается в большем количестве чем указано в маршрутном листе. Отгружается:{1}, По другим документам:{2}, Всего нужно отгрузить:{3}", 
						item.Nomenclature.Name,
						item.Amount,
						item.AmountLoaded,
						item.AmountInRouteList
					),
						new[] { this.GetPropertyName (o => o.Items) });
			}

		}

		#endregion

		#region Функции

		public virtual void AddItem (CarLoadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add (item);
		}

		public virtual void FillFromRouteList(IUnitOfWork uow, bool warehouseOnly)
		{
			ObservableItems.Clear();
			if (RouteList == null || (Warehouse == null && warehouseOnly))
				return;

			var goods = Repository.Logistics.RouteListRepository.GetGoodsInRLWithoutEquipments(uow, 
				            RouteList, warehouseOnly ? Warehouse : null);
			var nomenclatures = uow.GetById<Nomenclature>(goods.Select(x => x.NomenclatureId).ToArray());

			foreach(var inRoute in goods)
			{
				ObservableItems.Add(new CarLoadDocumentItem(){
					Document = this,
					Nomenclature = nomenclatures.First(x => x.Id == inRoute.NomenclatureId),
					AmountInRouteList = inRoute.Amount,
					Amount = inRoute.Amount
				});
			}

			var equipmentsInRoute = Repository.Logistics.RouteListRepository.GetEquipmentsInRL(uow, 
				RouteList, warehouseOnly ? Warehouse : null);
			nomenclatures = uow.GetById<Nomenclature>(equipmentsInRoute.Select(x => x.NomenclatureId).ToArray());
			var equipments = uow.GetById<Equipment>(equipmentsInRoute.Select(x => x.EquipmentId).ToArray());

			foreach(var inRoute in equipmentsInRoute)
			{
				ObservableItems.Add(new CarLoadDocumentItem(){
					Document = this,
					Nomenclature = nomenclatures.First(x => x.Id == inRoute.NomenclatureId),
					//FIXME запуск оборудования - временный фикс
					//Equipment = equipments.First(x => x.Id == inRoute.EquipmentId),
					AmountInRouteList = 1,
					Amount = 1
				});
			}
		}

		public virtual void UpdateInRouteListAmount(IUnitOfWork uow)
		{
			if (RouteList == null)
				return;

			var goods = Repository.Logistics.RouteListRepository.GetGoodsInRLWithoutEquipments(uow, 
				RouteList, null);

			var equipmentsInRoute = Repository.Logistics.RouteListRepository.GetEquipmentsInRL(uow, 
				RouteList, null);
			
			foreach(var item in Items)
			{
				var aGoods = goods.FirstOrDefault(x => x.NomenclatureId == item.Nomenclature.Id);
				if (aGoods != null)
					item.AmountInRouteList = aGoods.Amount;
				else
				{
					var equipment = equipmentsInRoute.FirstOrDefault(x => x.EquipmentId == item.Equipment.Id);
					if (equipment != null)
						item.AmountInRouteList = equipment.Amount;
				}
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

		public virtual void UpdateAlreadyLoaded(IUnitOfWork uow)
		{
			if (Items.Count == 0 || RouteList == null)
				return;

			var inLoaded = Repository.Logistics.RouteListRepository.AllGoodsLoaded(uow, RouteList, this);

			foreach(var item in Items)
			{
				Repository.Logistics.RouteListRepository.GoodsLoadedListResult found;
				if (item.Equipment == null)
					found = inLoaded.FirstOrDefault(x => x.NomenclatureId == item.Nomenclature.Id);
				else
					found = inLoaded.FirstOrDefault(x => x.EquipmentId == item.Equipment.Id);
				if(found != null)
					item.AmountLoaded = found.Amount;
			}
		}

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			foreach(var item in Items)
			{
				if(item.Amount == 0 && item.MovementOperation != null)
				{
					uow.Delete(item.MovementOperation);
					item.MovementOperation = null;
				}
				if(item.Amount != 0)
				{
					if(item.MovementOperation != null)
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
			
		public virtual void ClearItemsFromZero()
		{
			foreach(var item in Items.Where(x => x.Amount == 0).ToList())
			{
				ObservableItems.Remove(item);
			}
		}

		#endregion
	}
}

