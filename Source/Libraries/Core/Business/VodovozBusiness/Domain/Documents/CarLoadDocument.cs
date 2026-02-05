using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "талоны погрузки автомобилей",
		Nominative = "талон погрузки автомобиля")]
	[EntityPermission]
	[HistoryTrace]
	public class CarLoadDocument : Document, IValidatableObject, IWarehouseBoundedDocument
	{
		private const int _commentLimit = 150;

		private RouteList _routeList;
		private Warehouse _warehouse;
		private IList<CarLoadDocumentItem> _items = new List<CarLoadDocumentItem>();
		private GenericObservableList<CarLoadDocumentItem> _observableItems;
		private string _comment;
		private CarLoadDocumentLoadOperationState _loadOperationState;

		#region Сохраняемые свойства

		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}
		
		[Display(Name = "Строки")]
		public virtual IList<CarLoadDocumentItem> Items {
			get => _items;
			set
			{
				SetField(ref _items, value);
				_observableItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CarLoadDocumentItem> ObservableItems {
			get
			{
				if(_observableItems == null)
				{
					_observableItems = new GenericObservableList<CarLoadDocumentItem>(Items);
				}

				return _observableItems;
			}
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Статус талона погрузки")]
		public virtual CarLoadDocumentLoadOperationState LoadOperationState
		{
			get => _loadOperationState;
			set => SetField(ref _loadOperationState, value);
		}


		#endregion

		#region Не сохраняемые свойства

		public virtual string Title => $"Талон погрузки №{Id} от {TimeStamp:d}";

		public virtual IEnumerable<int> SeparateTableOrderIds =>
			Items
			.Where(item => item.OrderId.HasValue && item.IsIndividualSetForOrder)
			.Select(item => item.OrderId.Value)
			.Distinct()
			.ToList();

		public virtual bool IsDocumentHasCommonOrders =>
			Items.Any(x => !x.IsIndividualSetForOrder);

		#endregion

		#region Публичные функции

		public virtual void FillFromRouteList(IUnitOfWork uow, IRouteListRepository routeListRepository, ISubdivisionRepository subdivisionRepository, bool warehouseOnly)
		{
			if(routeListRepository == null)
			{
				throw new ArgumentNullException(nameof(routeListRepository));
			}

			ObservableItems.Clear();
			if(RouteList == null || (Warehouse == null && warehouseOnly))
			{
				return;
			}

			var carLoadDocumentItems = GetCarLoadDocumentItemsFromRouteList(uow, routeListRepository, subdivisionRepository, warehouseOnly);

			foreach(var item in carLoadDocumentItems)
			{
				ObservableItems.Add(item);
			}
		}

		public virtual IEnumerable<CarLoadDocumentItem> GetCarLoadDocumentItemsFromRouteList(
			IUnitOfWork uow,
			IRouteListRepository routeListRepository,
			ISubdivisionRepository subdivisionRepository,
			bool isWarehouseOnly)
		{
			var goodsAndEquips = routeListRepository
				.GetGoodsAndEquipsInRLWithSpecialRequirements(uow, RouteList, subdivisionRepository, isWarehouseOnly ? Warehouse : null);

			var nomenclatures = uow.GetById<Nomenclature>(goodsAndEquips.Select(x => x.NomenclatureId).ToArray());

			var items = new List<CarLoadDocumentItem>();

			foreach(var inRoute in goodsAndEquips)
			{
				var amountInRouteList = goodsAndEquips
					.Where(x =>
						x.NomenclatureId == inRoute.NomenclatureId
						&& x.ExpireDatePercent == inRoute.ExpireDatePercent
						&& x.OrderId == inRoute.OrderId
						&& x.OwnType == inRoute.OwnType)
					.Sum(x => x.Amount);

				items.Add(new CarLoadDocumentItem
				{
					Document = this,
					Nomenclature = nomenclatures.First(x => x.Id == inRoute.NomenclatureId),
					OwnType = inRoute.OwnType,
					ExpireDatePercent = inRoute.ExpireDatePercent,
					AmountInRouteList = amountInRouteList,
					Amount = inRoute.Amount,
					OrderId = inRoute.OrderId,
					IsIndividualSetForOrder = inRoute.IsNeedIndividualSetOnLoad
				});
			}

			return items;
		}

		public virtual void UpdateInRouteListAmount(IUnitOfWork uow, IRouteListRepository routeListRepository)
		{
			if(routeListRepository == null)
			{
				throw new ArgumentNullException(nameof(routeListRepository));
			}

			if(RouteList == null)
			{
				return;
			}

			var goodsAndEquips = routeListRepository.GetGoodsAndEquipsInRLWithSpecialRequirements(uow, RouteList, null);

			foreach(var item in Items)
			{
				var aGoods = goodsAndEquips.Where(x =>
					x.NomenclatureId == item.Nomenclature.Id
					&& x.ExpireDatePercent == item.ExpireDatePercent
					&& x.OrderId == item.OrderId
					&& x.OwnType == item.OwnType);

				if(aGoods != null)
				{
					item.AmountInRouteList = aGoods.Sum(x => x.Amount);
				}
			}
		}

		public virtual void UpdateStockAmount(IUnitOfWork uow, IStockRepository stockRepository)
		{
			if(!Items.Any() || Warehouse == null)
			{
				return;
			}

			var inStock = stockRepository.NomenclatureInStock(
				uow,
				Items.Select(x => x.Nomenclature.Id).ToArray(),
				new []{ Warehouse.Id },
				TimeStamp
			);

			foreach(var item in Items)
			{
				if(inStock.ContainsKey(item.Nomenclature.Id))
				{
					item.AmountInStock = inStock[item.Nomenclature.Id];
				}
			}
		}

		public virtual void UpdateAlreadyLoaded(IUnitOfWork uow, IRouteListRepository routeListRepository)
		{
			if(routeListRepository == null)
			{
				throw new ArgumentNullException(nameof(routeListRepository));
			}

			if(!Items.Any() || Warehouse == null)
			{
				return;
			}

			var inLoaded = routeListRepository.AllGoodsLoadedDivided(uow, RouteList, this);
			if(inLoaded.Count == 0)
			{
				return;
			}
			
			foreach(var item in Items)
			{
				var found = inLoaded.FirstOrDefault(x => 
					x.NomenclatureId == item.Nomenclature.Id 
					&& x.OwnType == item.OwnType 
					&& x.ExpireDatePercent == item.ExpireDatePercent
					&& x.OrderId == item.OrderId);

				if(found != null)
				{
					item.AmountLoaded = found.Amount;
				}
			}
		}

		public virtual void UpdateAmounts()
		{
			foreach(var item in Items)
			{
				item.Amount = item.AmountInRouteList - item.AmountLoaded;

				var alreadyCounted = Items
					.Where(x => x.Nomenclature == item.Nomenclature
						&& Items.IndexOf(x) < Items.IndexOf(item))
					.Sum(x => x.Amount);

				var calculatedAvailableCount = item.AmountInStock - alreadyCounted;

				if(item.Amount > calculatedAvailableCount)
				{
					item.Amount = calculatedAvailableCount;
				}
			}
		}

		public virtual void UpdateOperations(IUnitOfWork uow, int terminalId)
		{
			foreach(var item in Items) {
				if(item.Amount == 0) {
					if(item.GoodsAccountingOperation != null) {
						uow.Delete(item.GoodsAccountingOperation);
						item.GoodsAccountingOperation = null;
					}
					if(item.EmployeeNomenclatureMovementOperation != null) {
						uow.Delete(item.EmployeeNomenclatureMovementOperation);
						item.EmployeeNomenclatureMovementOperation = null;
					}

					if(item.DeliveryFreeBalanceOperation != null)
					{
						uow.Delete(item.DeliveryFreeBalanceOperation);
						item.DeliveryFreeBalanceOperation = null;
					}
				}
				else {
					if(item.GoodsAccountingOperation != null) {
						item.UpdateWarehouseMovementOperation(Warehouse);
					} else {
						item.CreateWarehouseMovementOperation(Warehouse, TimeStamp);
					}
					
					if(item.EmployeeNomenclatureMovementOperation != null) {
						item.UpdateEmployeeNomenclatureMovementOperation();
					} else {
						item.CreateEmployeeNomenclatureMovementOperation(TimeStamp);
					}

					if(item.Nomenclature.Id != terminalId)
					{
						item.CreateOrUpdateDeliveryFreeBalanceOperation();
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

		public override void SetTimeStamp(DateTime value)
		{
			base.TimeStamp = value;
			if(!NHibernate.NHibernateUtil.IsInitialized(Items))
			{
				return;
			}

			UpdateOperationsTime();
		}

		#endregion

		#region Приватные функии

		private void UpdateOperationsTime()
		{
			foreach(var item in Items) {
				if(item.GoodsAccountingOperation != null && item.GoodsAccountingOperation.OperationTime != TimeStamp)
					item.GoodsAccountingOperation.OperationTime = TimeStamp;
				if(item.EmployeeNomenclatureMovementOperation != null && item.EmployeeNomenclatureMovementOperation.OperationTime != TimeStamp)
					item.EmployeeNomenclatureMovementOperation.OperationTime = TimeStamp;
			}
		}

		#endregion
		
		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(AuthorId == null) {
				yield return new ValidationResult("Не указан кладовщик.",
					new[] { nameof(AuthorId) });
			}
			if(RouteList == null) {
				yield return new ValidationResult("Не указан маршрутный лист, по которому осуществляется отгрузка.",
					new[] { nameof(RouteList) });
			}
			if(Warehouse == null) {
				yield return new ValidationResult("Не указан склад погрузки.",
					new[] { nameof(Warehouse) });
			}
			
			if(Comment?.Length > _commentLimit)
			{
				yield return new ValidationResult($"Длина комментария превышена на {Comment.Length - _commentLimit}",
					new[] { nameof(Comment) });
			}

			var uniqueNomenclaturesIds = Items.Select(x => x.Nomenclature.Id).Distinct();

			foreach(var nomenclatureId in uniqueNomenclaturesIds)
			{
				var amountInStock = Items.Where(x => x.Nomenclature.Id == nomenclatureId).First().AmountInStock;
				var amountToLoad = Items.Where(x => x.Nomenclature.Id == nomenclatureId).Sum(x => x.Amount);
				var nomenclatureName = Items.Where(x => x.Nomenclature.Id == nomenclatureId).First().Nomenclature.Name;

				if(amountToLoad > amountInStock)
				{
					yield return new ValidationResult($"На складе недостаточное количество <{nomenclatureName}>",
						new[] { nameof(Items) });
				}
			}

			foreach(var item in Items) {
				if(item.Equipment != null && !(item.Amount == 0 || item.Amount == 1) && item.Equipment.Nomenclature.IsSerial) {
					yield return new ValidationResult(
						$"Оборудование <{item.Nomenclature.Name}> сн: {item.Equipment.Serial} нельзя отгружать в количестве отличном от 0 или 1",
						new[] { nameof(Items) });
				}
				if(item.Amount + item.AmountLoaded > item.AmountInRouteList) {
					yield return new ValidationResult(
						$"Номенклатура <{item.Nomenclature.Name}> отгружается в большем количестве чем указано в маршрутном листе. " +
						$"Отгружается:{item.Amount}, По другим документам:{item.AmountLoaded}, Всего нужно отгрузить:{item.AmountInRouteList}",
						new[] { nameof(Items) }
					);
				}
			}
		}

		#endregion
	}
}

