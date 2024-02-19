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
using QS.Project.Services;
using QS.Utilities;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "штрафы сотрудникам",
		Nominative = "штраф сотрудникам",
		GenitivePlural = "штрафов")]
	[EntityPermission]
	[HistoryTrace]
	public class Fine : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		FineTypes fineType;

		[Display(Name = "Тип штрафа")]
		public virtual FineTypes FineType {
			get => fineType;
			set => SetField(ref fineType, value, () => FineType);
		}

		private DateTime date = DateTime.Today;

		[Display(Name = "Дата")]
		public virtual DateTime Date {
			get => date;
			set => SetField(ref date, value, () => Date);
		}

		decimal totalMoney;

		[Display(Name = "Всего денег")]
		public virtual decimal TotalMoney {
			get => totalMoney;
			set => SetField(ref totalMoney, value, () => TotalMoney);
		}

		decimal litersOverspending;
		/// <summary>
		/// Перерасходовано литров.
		/// Свойство без маппинга, данные записываются в FineItem
		/// </summary>
		[Display(Name = "Перерасходовано литров")]
		public virtual decimal LitersOverspending {
			get => litersOverspending;
			set => SetField(ref litersOverspending, value, () => LitersOverspending);
		}

		private string fineReasonString;

		[Display(Name = "Причина штрафа")]
		public virtual string FineReasonString {
			get => fineReasonString;
			set => SetField(ref fineReasonString, value, () => FineReasonString);
		}

		private RouteList routeList;

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get => routeList;
			set => SetField(ref routeList, value, () => RouteList);
		}

		private UndeliveredOrder undeliveredOrder;

		[Display(Name = "Недовоз")]
		public virtual UndeliveredOrder UndeliveredOrder {
			get => undeliveredOrder;
			set {
				if(SetField(ref undeliveredOrder, value, () => UndeliveredOrder))
					FineReasonString = string.Format(
						"{0}, {1}, {2}",
						UndeliveredOrder.Title,
						UndeliveredOrder.OldOrder.Client.Name,
						UndeliveredOrder.OldOrder.DeliveryPoint != null ? UndeliveredOrder.OldOrder.DeliveryPoint.ShortAddress : "Самовывоз"
					);
			}
		}

		private Employee author;

		[Display(Name = "Автор штрафа")]
		public virtual Employee Author {
			get => author;
			set => SetField(ref author, value, () => Author);
		}

		IList<FineItem> items = new List<FineItem>();

		[Display(Name = "Строки")]
		public virtual IList<FineItem> Items {
			get => items;
			set {
				SetField(ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<FineItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FineItem> ObservableItems {
			get {
				if(observableItems == null)
					observableItems = new GenericObservableList<FineItem>(Items);
				return observableItems;
			}
		}

		IList<FineNomenclature> nomenclatures = new List<FineNomenclature>();

		[Display(Name = "Номенклатура")]
		public virtual IList<FineNomenclature> Nomenclatures {
			get => nomenclatures;
			set => SetField(ref nomenclatures, value, () => Nomenclatures);
		}

		IList<RouteListItem> routeListItems = new List<RouteListItem>();
		[Display(Name = "Адрес МЛ")]
		public virtual IList<RouteListItem> RouteListItems {
			get => routeListItems;
			set => SetField(ref routeListItems, value);
		}
		
		GenericObservableList<RouteListItem> observableRouteListItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RouteListItem> ObservableRouteListItems {
			get {
				if(observableRouteListItems == null)
					observableRouteListItems = new GenericObservableList<RouteListItem>(RouteListItems);
				return observableRouteListItems;
			}
		}

		public virtual void UpdateFuelOperations(IUnitOfWork uow)
		{
			if(FineType == FineTypes.FuelOverspending && ObservableItems.Any()) {
				var item = ObservableItems.FirstOrDefault();
				if(item.FuelOutlayedOperation == null) {
					item.FuelOutlayedOperation = new FuelOperation() {
						Car = item.Fine.RouteList.Car,
						Fuel = item.Fine.RouteList.Car.FuelType,
						Driver = item.Employee,
						LitersGived = 0,
						LitersOutlayed = item.LitersOverspending,
						OperationTime = DateTime.Now,
						IsFine = true
					};
				} else {
					item.FuelOutlayedOperation.Car = item.Fine.RouteList.Car;
					item.FuelOutlayedOperation.Fuel = item.Fine.RouteList.Car.FuelType;
					item.FuelOutlayedOperation.Driver = item.Employee;
					item.FuelOutlayedOperation.LitersGived = 0;
					item.FuelOutlayedOperation.LitersOutlayed = item.LitersOverspending;
					item.FuelOutlayedOperation.OperationTime = DateTime.Now;
					item.FuelOutlayedOperation.IsFine = true;
				}

				uow.Save(item.FuelOutlayedOperation);
			}
		}

		#endregion

		#region Расчетные

		public virtual string Title => string.Format("Штраф №{0} от {1:d}", Id, Date);

		public virtual string Description {
			get {
				if(Items.Count == 0)
					return CurrencyWorks.GetShortCurrencyString(TotalMoney);
				string persons;
				if(Items.Count <= 3)
					persons = string.Join(", ", Items.Select(x => x.Employee.ShortName));
				else
					persons = NumberToTextRus.FormatCase(Items.Count, "{0} сотрудник", "{0} сотрудника", "{0} сотрудников");
				return string.Format("({0}) = {1}", persons,
					CurrencyWorks.GetShortCurrencyString(TotalMoney));
			}
		}

		#endregion

		public Fine() { }

		#region Методы

		public virtual void UpdateItems()
		{
			Employee driver = RouteList?.Driver;
			FineItem item = null;
			ObservableItems.Clear();
			if(driver != null) {
				item = ObservableItems.FirstOrDefault(x => x.Employee == driver);
				if(item != null) {
					ObservableItems.Add(item);
				} else {
					AddItem(driver);
				}
			}
		}

		public virtual void AddItem(Employee employee)
		{
			ObservableItems.Add(
				new FineItem {
					Employee = employee,
					Fine = this
				}
			);
		}

		public virtual void RemoveItem(FineItem item)
		{
			if(ObservableItems.Contains(item))
				ObservableItems.Remove(item);
		}
		
		public virtual void AddAddress(RouteListItem address)
		{
			if(!ObservableRouteListItems.Contains(address))
				ObservableRouteListItems.Add(address);
		}
		
		public virtual void AddNomenclature(Dictionary<Nomenclature, decimal> nomenclatureAmounts)
		{
			foreach(var nom in nomenclatureAmounts) {
				Nomenclatures.Add(
					new FineNomenclature {
						Fine = this,
						Nomenclature = nom.Key,
						Amount = nom.Value
					}
				);
			}
		}

		public virtual void UpdateNomenclature(Dictionary<Nomenclature, decimal> nomenclatureAmounts)
		{
			var nomenclaturesToRemove =
				Nomenclatures.Where(nom =>
					nomenclatureAmounts.All(x => x.Key.Id != nom.Nomenclature.Id));
			
			foreach(var nom in nomenclaturesToRemove)
			{
				Nomenclatures.Remove(nom);
			}

			foreach(var nom in nomenclatureAmounts) {
				var item = Nomenclatures.FirstOrDefault(x => x.Nomenclature.Id == nom.Key.Id);
				if(item == null) {
					Nomenclatures.Add(
						new FineNomenclature {
							Fine = this,
							Nomenclature = nom.Key,
							Amount = nom.Value
						}
					);
				} else
					item.Amount = nom.Value;
			}
		}

		public virtual void DivideAtAll()
		{
			if(!Items.Any())
				return;
			var part = Math.Round(TotalMoney / Items.Count, 2);
			foreach(var item in Items) {
				item.Money = part;
			}
		}

		public virtual void UpdateWageOperations(IUnitOfWork uow)
		{
			foreach(var item in Items) {
				if(item.WageOperation == null) {
					item.WageOperation = new WagesMovementOperations {
						OperationType = WagesType.HoldedFine,
						Employee = item.Employee,
						Money = item.Money * (-1),
						OperationTime = this.Date
					};
				} else {
					item.WageOperation.OperationType = WagesType.HoldedFine;
					item.WageOperation.Employee = item.Employee;
					item.WageOperation.Money = item.Money * (-1);
				}
				uow.Save(item.WageOperation);
			}
		}

		public virtual void Fill(decimal money, RouteList routeList, string reasonString, DateTime date, params Employee[] employees)
		{
			employees.ToList().ForEach(this.AddItem);
			this.TotalMoney = money;
			this.DivideAtAll();
			this.FineReasonString = reasonString;
			this.Date = date;
			this.RouteList = routeList;
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Items.Count == 0)
				yield return new ValidationResult(string.Format("Отсутствуют сотрудники на которых назначен штраф."),
					new[] { this.GetPropertyName(o => o.Items) });

			var totalSum = Items.Sum(x => x.Money);
			if(totalSum != TotalMoney)
				yield return new ValidationResult(string.Format("Общая сумма штрафа {0:C}, отличается от суммы штрафов всех сотрудников {1:C}.",
					TotalMoney, totalSum),
					new[] { this.GetPropertyName(o => o.Items) });

			if(string.IsNullOrWhiteSpace(FineReasonString))
				yield return new ValidationResult(string.Format("Отсутствует причина выдачи штрафа."),
					new[] { this.GetPropertyName(o => o.FineReasonString) });

			if(FineType == FineTypes.FuelOverspending && RouteList == null) {
				yield return new ValidationResult(string.Format("Не выбран маршрутный лист, при типе штрафа \"{0}\"", FineType.GetEnumTitle()));
			}

			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete_fines") && Id > 0) {
				yield return new ValidationResult(string.Format("Недостаточно прав для изменения штрафа!"));
			}

			if(Id == 0 && (Date < DateTime.Today || Date > DateTime.Today.AddDays(28)) && RouteList == null)
			{
				yield return new ValidationResult(string.Format("Дату штрафа можно менять только в пределах 28 дней от даты создания."));
			}
		}


		#endregion
	}

	public enum FineTypes
	{
		[Display(Name = "Стандартный")]
		Standart,
		[Display(Name = "Перерасход топлива")]
		FuelOverspending
	}

	public class FineTypeStringType : NHibernate.Type.EnumStringType
	{
		public FineTypeStringType() : base(typeof(FineTypes)) { }
	}
}

