using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "талоны погрузки автомобилей",
		Nominative = "талон погрузки автомобиля")]
	[EntityPermission]
	[HistoryTrace]
	public class CarLoadDocument : Document, IValidatableObject
	{
		DateTime version;
		[Display(Name = "Версия")]
		public virtual DateTime Version {
			get => version;
			set => SetField(ref version, value, () => Version);
		}

		public override DateTime TimeStamp {
			get => base.TimeStamp;
			set {
				base.TimeStamp = value;
				if(!NHibernate.NHibernateUtil.IsInitialized(Items))
					return;
				foreach(var item in Items) {
					if(item.MovementOperation != null && item.MovementOperation.OperationTime != TimeStamp)
						item.MovementOperation.OperationTime = TimeStamp;
				}
			}
		}

		RouteList routeList;

		public virtual RouteList RouteList {
			get => routeList;
			set => SetField(ref routeList, value, () => RouteList);
		}

		Warehouse warehouse;

		public virtual Warehouse Warehouse {
			get => warehouse;
			set => SetField(ref warehouse, value, () => Warehouse);
		}


		IList<CarLoadDocumentItem> items = new List<CarLoadDocumentItem>();

		[Display(Name = "Строки")]
		public virtual IList<CarLoadDocumentItem> Items {
			get => items;
			set {
				SetField(ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<CarLoadDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CarLoadDocumentItem> ObservableItems {
			get {
				if(observableItems == null)
					observableItems = new GenericObservableList<CarLoadDocumentItem>(Items);
				return observableItems;
			}
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		public virtual string Title => $"Талон погрузки №{Id} от {TimeStamp:d}";

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Author == null)
				yield return new ValidationResult("Не указан кладовщик.",
					new[] { this.GetPropertyName(o => o.Author) });
			if(RouteList == null)
				yield return new ValidationResult("Не указан маршрутный лист, по которому осуществляется отгрузка.",
					new[] { this.GetPropertyName(o => o.RouteList) });
			if(Warehouse == null)
				yield return new ValidationResult("Не указан склад погрузки.",
					new[] { this.GetPropertyName(o => o.Warehouse) });

			foreach(var item in Items) {
				if(item.Amount > item.AmountInStock)
					yield return new ValidationResult(string.Format("На складе недостаточное количество <{0}>", item.Nomenclature.Name),
						new[] { this.GetPropertyName(o => o.Items) });
				if(item.Equipment != null && !(item.Amount == 0 || item.Amount == 1)
				   && item.Equipment.Nomenclature.IsSerial // I-407
				  )
					yield return new ValidationResult(
						$"Оборудование <{item.Nomenclature.Name}> сн: {item.Equipment.Serial} нельзя отгружать в количестве отличном от 0 или 1",
						new[] { this.GetPropertyName(o => o.Items) });
				if(item.Amount + item.AmountLoaded > item.AmountInRouteList)
					yield return new ValidationResult(
						$"Номенклатура <{item.Nomenclature.Name}> отгружается в большем количестве чем указано в маршрутном листе. " +
						$"Отгружается:{item.Amount}, По другим документам:{item.AmountLoaded}, Всего нужно отгрузить:{item.AmountInRouteList}",
						new[] { this.GetPropertyName(o => o.Items) }
					);
			}
		}

		#endregion

		#region Функции

		public virtual void AddItem(CarLoadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add(item);
		}

		public virtual void FillFromRouteList(IUnitOfWork uow, IRouteListRepository routeListRepository, bool warehouseOnly)
		{
			if(routeListRepository == null)
				throw new ArgumentNullException(nameof(routeListRepository));

			ObservableItems.Clear();
			if(RouteList == null || (Warehouse == null && warehouseOnly))
				return;

			var goodsAndEquips = routeListRepository.GetGoodsAndEquipsInRL(uow, RouteList, warehouseOnly ? Warehouse : null);
			var nomenclatures = uow.GetById<Nomenclature>(goodsAndEquips.Select(x => x.NomenclatureId).ToArray());

			foreach(var inRoute in goodsAndEquips) {
				ObservableItems.Add(
					new CarLoadDocumentItem {
						Document = this,
						Nomenclature = nomenclatures.First(x => x.Id == inRoute.NomenclatureId),
						AmountInRouteList = inRoute.Amount,
						Amount = inRoute.Amount
					}
				);
			}
		}

		public virtual void UpdateInRouteListAmount(IUnitOfWork uow, IRouteListRepository routeListRepository)
		{
			if(routeListRepository == null)
				throw new ArgumentNullException(nameof(routeListRepository));

			if(RouteList == null)
				return;
			var goodsAndEquips = routeListRepository.GetGoodsAndEquipsInRL(uow, RouteList, null);

			foreach(var item in Items) {
				var aGoods = goodsAndEquips.FirstOrDefault(x => x.NomenclatureId == item.Nomenclature.Id);
				if(aGoods != null) {
					item.AmountInRouteList = aGoods.Amount;
				}
			}
		}

		public virtual void UpdateStockAmount(IUnitOfWork uow)
		{
			if(!Items.Any() || Warehouse == null)
				return;
			var nomenclatureIds = Items.Select(x => x.Nomenclature.Id).ToArray();
			var inStock = Repositories.StockRepository.NomenclatureInStock(
														uow,
														Warehouse.Id,
														nomenclatureIds,
														TimeStamp
													);

			foreach(var item in Items)
				item.AmountInStock = inStock[item.Nomenclature.Id];
		}

		public virtual void UpdateAlreadyLoaded(IUnitOfWork uow, IRouteListRepository routeListRepository)
		{
			if(routeListRepository == null)
				throw new ArgumentNullException(nameof(routeListRepository));

			if(!Items.Any() || Warehouse == null)
				return;

			var inLoaded = routeListRepository.AllGoodsLoaded(uow, RouteList, this);

			foreach(var item in Items) {
				GoodsLoadedListResult found;
				found = inLoaded.FirstOrDefault(x => x.NomenclatureId == item.Nomenclature.Id);
				if(found != null)
					item.AmountLoaded = found.Amount;
			}
		}

		public virtual void UpdateOperations(IUnitOfWork uow)
		{
			foreach(var item in Items) {
				if(item.Amount == 0 && item.MovementOperation != null) {
					uow.Delete(item.MovementOperation);
					item.MovementOperation = null;
				}
				if(item.Amount != 0) {
					if(item.MovementOperation != null) {
						item.UpdateOperation(Warehouse);
					} else {
						item.CreateOperation(Warehouse, TimeStamp);
					}
				}
			}
		}

		public virtual void ClearItemsFromZero()
		{
			foreach(var item in Items.Where(x => x.Amount == 0).ToList()) {
				ObservableItems.Remove(item);
			}
		}

		#endregion
	}
}

