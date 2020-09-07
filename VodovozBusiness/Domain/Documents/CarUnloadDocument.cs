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
using Vodovoz.Domain.Operations;
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
	public class CarUnloadDocument : Document, IValidatableObject
	{
		public CarUnloadDocument() { }

		public override DateTime TimeStamp {
			get => base.TimeStamp;
			set {
				base.TimeStamp = value;
				if(!NHibernate.NHibernateUtil.IsInitialized(Items))
					return;
				foreach(var item in Items) {
					if(item.MovementOperation.OperationTime != TimeStamp)
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
			set {
				if(SetField(ref warehouse, value, () => Warehouse))
					UpdateWarehouse();
			}
		}

		IList<CarUnloadDocumentItem> items = new List<CarUnloadDocumentItem>();

		[Display(Name = "Строки")]
		public virtual IList<CarUnloadDocumentItem> Items {
			get => items;
			set {
				if(SetField(ref items, value, () => Items))
					observableItems = null;
			}
		}

		GenericObservableList<CarUnloadDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CarUnloadDocumentItem> ObservableItems {
			get {
				if(observableItems == null)
					observableItems = new GenericObservableList<CarUnloadDocumentItem>(Items);
				return observableItems;
			}
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		#region Не сохраняемые

		public virtual int DefBottleId { get; protected set; }

		public virtual string Title => string.Format("Разгрузка автомобиля №{0} от {1:d}", Id, TimeStamp);

		int returnedTareBefore;
		[PropertyChangedAlso("ReturnedTareBeforeText")]
		public virtual int ReturnedTareBefore {
			get => returnedTareBefore;
			set => SetField(ref returnedTareBefore, value, () => ReturnedTareBefore);
		}

		public virtual string ReturnedTareBeforeText => ReturnedTareBefore > 0 ? string.Format("Возвращено другими разгрузками: {0} бут.", ReturnedTareBefore) : string.Empty;

		int tareToReturn;
		public virtual int TareToReturn {
			get => tareToReturn;
			set => SetField(ref tareToReturn, value, () => TareToReturn);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Author == null)
				yield return new ValidationResult("Не указан кладовщик.",
					new[] { this.GetPropertyName(o => o.Author) });
			if(RouteList == null)
				yield return new ValidationResult("Не указан маршрутный лист, по которому осуществляется разгрузка.",
					new[] { this.GetPropertyName(o => o.RouteList) });

			if(Warehouse == null)
				yield return new ValidationResult("Не указан склад разгрузки.",
					new[] { this.GetPropertyName(o => o.Warehouse) });

			foreach(var item in Items) {
				if(item.MovementOperation.Nomenclature.Category == NomenclatureCategory.bottle && item.MovementOperation.Amount < 0)
					yield return new ValidationResult(
						string.Format("Для оборудования {0}, нельзя указывать отрицательное значение.", item.MovementOperation.Nomenclature.Name),
						new[] { this.GetPropertyName(o => o.Items) }
					);

				if(item.MovementOperation.Nomenclature.IsDefectiveBottle && item.TypeOfDefect == null)
					yield return new ValidationResult(
						string.Format("Для брака {0} необходимо указать его вид", item.MovementOperation.Nomenclature.Name),
						new[] { this.GetPropertyName(o => o.Items) }
					);
			}

			var hasDublicateServiceClaims = Items
				.Where(item => item.ServiceClaim != null)
				.GroupBy(item => item.ServiceClaim)
				.Any(g => g.Count() > 1);

			if(hasDublicateServiceClaims)
				yield return new ValidationResult(
					"Имеются продублированные заявки на сервис.",
					new[] { this.GetPropertyName(o => o.Items) }
				);
		}

		#endregion

		public virtual void InitializeDefaultValues(IUnitOfWork uow, INomenclatureRepository nomenclatureRepository)
		{
			if(nomenclatureRepository == null)
				throw new ArgumentNullException(nameof(nomenclatureRepository));

			DefBottleId = nomenclatureRepository.GetDefaultBottle(uow).Id;
		}

		public virtual void AddItem(CarUnloadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add(item);
		}

		public virtual void AddItem(ReciveTypes reciveType, Nomenclature nomenclature, Equipment equipment, 
		                            decimal amount, Service.ServiceClaim serviceClaim, int terminalId, string redhead = null, 
		                            DefectSource source = DefectSource.None, CullingCategory typeOfDefect = null)
		{
			var operation = new WarehouseMovementOperation {
				Amount = amount,
				Nomenclature = nomenclature,
				IncomingWarehouse = Warehouse,
				Equipment = equipment,
				OperationTime = TimeStamp
			};

			var item = new CarUnloadDocumentItem {
				ReciveType = reciveType,
				MovementOperation = operation,
				ServiceClaim = serviceClaim,
				Redhead = redhead,
				Source = source,
				TypeOfDefect = typeOfDefect
			};

			if (nomenclature.Id == terminalId) {
				var terminalMovementOperation = new EmployeeNomenclatureMovementOperation {
					Amount = -(int)amount,
					Nomenclature = nomenclature,
					Employee = RouteList.Driver,
					OperationTime = TimeStamp
				};

				item.EmployeeNomenclatureMovementOperation = terminalMovementOperation;
			}
			
			AddItem(item);
		}

		public virtual void UpdateWarehouse()
		{
			if(Warehouse != null) {
				foreach(var item in Items) {
					if(item.MovementOperation != null) {
						item.MovementOperation.IncomingWarehouse = Warehouse;
					}
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
			if(item.MovementOperation?.Nomenclature.Id != DefBottleId)
				return false;
			TareToReturn += (int)(item.MovementOperation?.Amount ?? 0);
			return true;
		}
	}
}