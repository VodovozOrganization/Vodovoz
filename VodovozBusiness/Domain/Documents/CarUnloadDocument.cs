using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы разгрузки автомобилей",
		Nominative = "документ разгрузки автомобиля")]
	[EntityPermission]
	public class CarUnloadDocument : Document, IValidatableObject
	{
		public CarUnloadDocument() { }

		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				if (!NHibernate.NHibernateUtil.IsInitialized(Items))
					return;
				foreach (var item in Items) {
					if (item.MovementOperation.OperationTime != TimeStamp)
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

		IList<CarUnloadDocumentItem> items = new List<CarUnloadDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<CarUnloadDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<CarUnloadDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CarUnloadDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<CarUnloadDocumentItem> (Items);
				return observableItems;
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		public virtual string Title => String.Format("Разгрузка автомобиля №{0} от {1:d}", Id, TimeStamp);

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Author == null)
				yield return new ValidationResult ("Не указан кладовщик.",
					new[] { this.GetPropertyName (o => o.Author) });
			if (RouteList == null)
				yield return new ValidationResult ("Не указан маршрутный лист, по которому осуществляется разгрузка.",
					new[] { this.GetPropertyName (o => o.RouteList)});
			
			if(Warehouse == null)
				yield return new ValidationResult ("Не указан склад разгрузки.",
					new[] { this.GetPropertyName (o => o.Warehouse)});
			
			foreach(var item in Items)
			{
				if (item.MovementOperation.Nomenclature.Category == NomenclatureCategory.bottle)
				{
					if (item.MovementOperation.Amount < 0)
					{
						yield return new ValidationResult (String.Format("Для оборудования {0}, нельзя указывать отрицательное значение.", item.MovementOperation.Nomenclature.Name),
							new[] { this.GetPropertyName (o => o.Items)});
					}
				}

				if(item.MovementOperation.Nomenclature.IsDefectiveBottle && item.TypeOfDefect == null)
					yield return new ValidationResult(
						String.Format("Для брака {0} необходимо указать его вид", item.MovementOperation.Nomenclature.Name),
						new[] { this.GetPropertyName(o => o.Items) }
					);
			}

			var hasDublicateServiceClaims = Items
				.Where(item => item.ServiceClaim != null)
				.GroupBy(item => item.ServiceClaim)
				.Any(g => g.Count() > 1);

			if(hasDublicateServiceClaims)
				yield return new ValidationResult ("Имеются продублированные заявки на сервис.",
					new[] { this.GetPropertyName (o => o.Items)});
			
		}

		#endregion

		public virtual void AddItem (CarUnloadDocumentItem item)
		{
			item.Document = this;
			ObservableItems.Add (item);
		}

		public virtual void AddItem(ReciveTypes reciveType, Nomenclature nomenclature, Equipment equipment, decimal amount, Service.ServiceClaim serviceClaim, string redhead = null, DefectSource source = DefectSource.None, CullingCategory typeOfDefect = null)
		{
			var operation = new WarehouseMovementOperation();
			operation.Amount = amount;
			operation.Nomenclature = nomenclature;
			operation.IncomingWarehouse = Warehouse;
			operation.Equipment = equipment;
			operation.OperationTime = TimeStamp;
			AddItem(new CarUnloadDocumentItem {
				ReciveType = reciveType,
				MovementOperation = operation,
				ServiceClaim = serviceClaim,
				Redhead = redhead,
				Source = source,
				TypeOfDefect = typeOfDefect
			});
		}
	}
}