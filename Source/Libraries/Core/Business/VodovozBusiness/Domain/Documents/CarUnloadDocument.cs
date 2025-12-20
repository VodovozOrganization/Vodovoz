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
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Service;
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
		private string _comment;
		private RouteList _routeList;
		private Warehouse _warehouse;
		private IList<CarUnloadDocumentItem> _items = new List<CarUnloadDocumentItem>();
		private GenericObservableList<CarUnloadDocumentItem> _observableItems;
		public const string DocumentRdlPath = "Reports/Store/CarUnloadDoc.rdl";

		#region Сохраняемые свойства

		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;
				if(!NHibernateUtil.IsInitialized(Items))
				{
					return;
				}

				UpdateOperationsTime();
			}
		}

		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				if(SetField(ref _warehouse, value, () => Warehouse))
				{
					UpdateWarehouse();
				}
			}
		}

		[Display(Name = "Строки")]
		public virtual IList<CarUnloadDocumentItem> Items
		{
			get => _items;
			set
			{
				if(SetField(ref _items, value))
				{
					_observableItems = null;
				}
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CarUnloadDocumentItem> ObservableItems =>
			_observableItems ?? (_observableItems = new GenericObservableList<CarUnloadDocumentItem>(Items));

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
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
			{
				throw new ArgumentNullException(nameof(nomenclatureRepository));
			}

			DefBottleId = nomenclatureRepository.GetDefaultBottleNomenclature(uow).Id;
		}

		public virtual void AddItem(CarUnloadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add(item);
		}

		public virtual void AddItem(
			ReciveTypes reciveType, 
			NomenclatureEntity nomenclature, 
			Equipment equipment, 
			decimal amount, 
			ServiceClaim serviceClaim,
			int terminalId,
			string redhead = null, 
			DefectSource source = DefectSource.None, 
			CullingCategory typeOfDefect = null)
		{
			var warehouseMovementOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Amount = amount,
				Nomenclature = nomenclature,
				Warehouse = Warehouse,
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
				GoodsAccountingOperation = warehouseMovementOperation,
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
			{
				return;
			}

			foreach(var item in Items)
			{
				if(item.GoodsAccountingOperation != null)
				{
					item.GoodsAccountingOperation.Warehouse = Warehouse;
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
			if(item.GoodsAccountingOperation?.Nomenclature.Id != DefBottleId)
				return false;
			TareToReturn += (int)(item.GoodsAccountingOperation?.Amount ?? 0);
			return true;
		}

		#endregion

		#region Приватные функции

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
			if(AuthorId == null)
			{
				yield return new ValidationResult("Не указан кладовщик.",
					new[] { nameof(AuthorId) });
			}

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
				if(item.GoodsAccountingOperation.Nomenclature.Category == NomenclatureCategory.bottle && item.GoodsAccountingOperation.Amount < 0) {
					yield return new ValidationResult(
						$"Для оборудования {item.GoodsAccountingOperation.Nomenclature.Name}, нельзя указывать отрицательное значение.",
						new[] { nameof(Items) }
					);
				}
				if(item.GoodsAccountingOperation.Nomenclature.IsDefectiveBottle && item.TypeOfDefect == null) {
					yield return new ValidationResult(
						$"Для брака {item.GoodsAccountingOperation.Nomenclature.Name} необходимо указать его вид",
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
				.Select(item => item.GoodsAccountingOperation.Nomenclature)
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
