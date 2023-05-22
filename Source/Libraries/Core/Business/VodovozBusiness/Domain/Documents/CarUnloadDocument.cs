using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "талоны разгрузки автомобилей",
		Nominative = "талон разгрузки автомобиля")]
	[EntityPermission]
	[HistoryTrace]
	public class CarUnloadDocument : Document, IValidatableObject, IWarehouseBoundedDocument
	{
		private const int _commentLimit = 150;
		
		#region Сохраняемые свойства

		public override DateTime TimeStamp {
			get => base.TimeStamp;
			set {
				base.TimeStamp = value;
				if(!NHibernateUtil.IsInitialized(Items))
					return;
				UpdateOperationsTime();
			}
		}

		private RouteList routeList;
		public virtual RouteList RouteList {
			get => routeList;
			set => SetField(ref routeList, value, () => RouteList);
		}

		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set {
				if(SetField(ref warehouse, value, () => Warehouse))
					UpdateWarehouse();
			}
		}

		private IList<CarUnloadDocumentItem> items = new List<CarUnloadDocumentItem>();
		[Display(Name = "Строки")]
		public virtual IList<CarUnloadDocumentItem> Items {
			get => items;
			set {
				if(SetField(ref items, value, () => Items))
					observableItems = null;
			}
		}

		private GenericObservableList<CarUnloadDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CarUnloadDocumentItem> ObservableItems {
			get {
				if(observableItems == null)
					observableItems = new GenericObservableList<CarUnloadDocumentItem>(Items);
				return observableItems;
			}
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		#endregion

		#region Не сохраняемые свойства

		public virtual int DefBottleId { get; protected set; }

		public virtual string Title => $"Разгрузка автомобиля №{Id} от {TimeStamp:d}";

		private int returnedTareBefore;
		[PropertyChangedAlso(nameof(ReturnedTareBeforeText))]
		public virtual int ReturnedTareBefore {
			get => returnedTareBefore;
			set => SetField(ref returnedTareBefore, value, () => ReturnedTareBefore);
		}

		public virtual string ReturnedTareBeforeText => ReturnedTareBefore > 0 ? $"Возвращено другими разгрузками: {ReturnedTareBefore} бут."
			: string.Empty;

		private int tareToReturn;
		public virtual int TareToReturn {
			get => tareToReturn;
			set => SetField(ref tareToReturn, value, () => TareToReturn);
		}

		#endregion

		#region Публичные функции

		public virtual void InitializeDefaultValues(IUnitOfWork uow, INomenclatureRepository nomenclatureRepository)
		{
			if(nomenclatureRepository == null)
				throw new ArgumentNullException(nameof(nomenclatureRepository));

			DefBottleId = nomenclatureRepository.GetDefaultBottleNomenclature(uow).Id;
		}

		public virtual void AddItem(CarUnloadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add(item);
		}

		public virtual void AddItem(
			ReciveTypes reciveType, 
			Nomenclature nomenclature, 
			Equipment equipment, 
			decimal amount, 
			ServiceClaim serviceClaim,
			int terminalId,
			string redhead = null, 
			DefectSource source = DefectSource.None, 
			CullingCategory typeOfDefect = null)
		{
			var warehouseMovementOperation = new WarehouseMovementOperation {
				Amount = amount,
				Nomenclature = nomenclature,
				IncomingWarehouse = Warehouse,
				Equipment = equipment,
				OperationTime = TimeStamp
			};
			
			var employeeNomenclatureMovementOperation = new EmployeeNomenclatureMovementOperation {
				Amount = -amount,
				Nomenclature = nomenclature,
				Employee = RouteList.Driver,
				OperationTime = TimeStamp
			};

			var item = new CarUnloadDocumentItem {
				ReciveType = reciveType,
				WarehouseMovementOperation = warehouseMovementOperation,
				EmployeeNomenclatureMovementOperation = employeeNomenclatureMovementOperation,
				ServiceClaim = serviceClaim,
				Redhead = redhead,
				DefectSource = source,
				TypeOfDefect = typeOfDefect
			};

			AddItem(item);

			item.CreateOrUpdateDeliveryFreeBalanceOperation(terminalId);
		}

		public virtual void UpdateWarehouse()
		{
			if(Warehouse == null)
				return;
			
			foreach(var item in Items) {
				if(item.WarehouseMovementOperation != null) {
					item.WarehouseMovementOperation.IncomingWarehouse = Warehouse;
				}
			}
		}

		public virtual void ReturnedEmptyBottlesBefore(IUnitOfWork uow, IRouteListRepository routeListRepository)
		{
			if(routeListRepository == null)
				throw new ArgumentNullException(nameof(routeListRepository));

			ReturnedTareBefore = routeListRepository.BottlesUnloadedByCarUnloadedDocuments(uow, DefBottleId, RouteList.Id, Id);
		}

		public virtual bool IsDefaultBottle(CarUnloadDocumentItem item)
		{
			if(item.WarehouseMovementOperation?.Nomenclature.Id != DefBottleId)
				return false;
			TareToReturn += (int)(item.WarehouseMovementOperation?.Amount ?? 0);
			return true;
		}

		#endregion

		#region Приватные функции

		private void UpdateOperationsTime()
		{
			foreach(var item in Items) {
				if(item.WarehouseMovementOperation != null && item.WarehouseMovementOperation.OperationTime != TimeStamp)
					item.WarehouseMovementOperation.OperationTime = TimeStamp;
				if(item.EmployeeNomenclatureMovementOperation != null && item.EmployeeNomenclatureMovementOperation.OperationTime != TimeStamp)
					item.EmployeeNomenclatureMovementOperation.OperationTime = TimeStamp;
			}
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Author == null)
				yield return new ValidationResult("Не указан кладовщик.",
					new[] { nameof(Author) });
			if(RouteList == null)
				yield return new ValidationResult("Не указан маршрутный лист, по которому осуществляется разгрузка.",
					new[] { nameof(RouteList) });

			if(Warehouse == null)
				yield return new ValidationResult("Не указан склад разгрузки.",
					new[] { nameof(Warehouse) });

			if(Comment?.Length > _commentLimit)
			{
				yield return new ValidationResult($"Длина комментария превышена на {Comment.Length - _commentLimit}",
					new[] { nameof(Comment) });
			}

			foreach(var item in Items) {
				if(item.WarehouseMovementOperation.Nomenclature.Category == NomenclatureCategory.bottle && item.WarehouseMovementOperation.Amount < 0) {
					yield return new ValidationResult(
						$"Для оборудования {item.WarehouseMovementOperation.Nomenclature.Name}, нельзя указывать отрицательное значение.",
						new[] { nameof(Items) }
					);
				}
				if(item.WarehouseMovementOperation.Nomenclature.IsDefectiveBottle && item.TypeOfDefect == null) {
					yield return new ValidationResult(
						$"Для брака {item.WarehouseMovementOperation.Nomenclature.Name} необходимо указать его вид",
						new[] { nameof(Items) }
					);
				}
			}

			var hasDublicateServiceClaims = Items
				.Where(item => item.ServiceClaim != null)
				.GroupBy(item => item.ServiceClaim)
				.Any(g => g.Count() > 1);

			if(hasDublicateServiceClaims) {
				yield return new ValidationResult(
					"Имеются продублированные заявки на сервис.",
					new[] { nameof(Items) }
				);
			}

			var needWeightOrVolume = Items
				.Select(item => item.WarehouseMovementOperation.Nomenclature)
				.Where(nomenclature =>
					Nomenclature.CategoriesWithWeightAndVolume.Contains(nomenclature.Category)
					&& (nomenclature.Weight == default
						|| nomenclature.Length == default
						|| nomenclature.Width == default
						|| nomenclature.Height == default))
				.ToList();
			if(needWeightOrVolume.Any())
			{
				yield return new ValidationResult(
					"Для всех добавленных на возврат номенклатур должны быть заполнены вес и объём.\n" +
					"Список номенклатур, в которых не заполнен вес или объём:\n" +
					$"{string.Join("\n", needWeightOrVolume.Select(x => $"({x.Id}) {x.Name}"))}",
					new[] { nameof(Items) });
			}
		}

		#endregion
	}
}
