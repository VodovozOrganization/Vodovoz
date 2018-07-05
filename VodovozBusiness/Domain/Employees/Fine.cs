using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;
using QSProjectsLib;
using System.Linq;
using Gamma.Utilities;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "штрафы сотрудникам",
		Nominative = "штраф сотрудникам")]
	public class Fine: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		FineTypes fineType;

		[Display(Name = "Тип штрафа")]
		public virtual FineTypes FineType {
			get { return fineType; }
			set { SetField(ref fineType, value, () => FineType); }
		}

		private DateTime date = DateTime.Today;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set {
				SetField (ref date, value, () => Date);
			}
		}

		decimal totalMoney;

		[Display (Name = "Всего денег")]
		public virtual decimal TotalMoney {
			get { return totalMoney; }
			set { SetField (ref totalMoney, value, () => TotalMoney); }
		}

		decimal litersOverspending;
		/// <summary>
		/// Перерасходовано литров.
		/// Свойство без маппинга, данные записываются в FineItem
		/// </summary>
		[Display(Name = "Перерасходовано литров")]
		public virtual decimal LitersOverspending {
			get { return litersOverspending; }
			set { SetField(ref litersOverspending, value, () => LitersOverspending); }
		}

		private string fineReasonString;

		[Display(Name = "Причина штрафа")]
		public virtual string FineReasonString
		{
			get { return fineReasonString; }
			set { SetField(ref fineReasonString, value, () => FineReasonString); }
		}

		private RouteList routeList;

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get { return routeList; }
			set { SetField(ref routeList, value, () => RouteList); }
		}

        private Employee author;

        [Display(Name = "Автор штрафа")]
        public virtual Employee Author
        {
            get { return author; }
            set { SetField(ref author, value, () => Author); }
        }

		IList<FineItem> items = new List<FineItem> ();

		[Display (Name = "Строки")]
		public virtual IList<FineItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<FineItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FineItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<FineItem> (Items);
				return observableItems;
			}
		}

		IList<FineNomenclature> nomenclatures = new List<FineNomenclature> ();

		[Display (Name = "Номенклатура")]
		public virtual IList<FineNomenclature> Nomenclatures {
			get { return nomenclatures; }
			set {
				SetField (ref nomenclatures, value, () => Nomenclatures);
			}
		}

		public virtual void UpdateFuelOperations(IUnitOfWork uow)
		{
			if(FineType == FineTypes.FuelOverspending && ObservableItems.Count() > 0) {
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
				}else {
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

		public virtual string Title { 
			get { return String.Format ("Штраф №{0} от {1:d}", Id, Date); }
		}

		public virtual string Description
		{
			get
			{
				if (Items.Count == 0)
					return CurrencyWorks.GetShortCurrencyString(TotalMoney);
				string persons;
				if (Items.Count <= 3)
					persons = String.Join(", ", Items.Select(x => x.Employee.ShortName));
				else
					persons = RusNumber.FormatCase(Items.Count, "{0} сотрудник", "{0} сотрудника", "{0} сотрудников");
				return String.Format("({0}) = {1}", persons,
					CurrencyWorks.GetShortCurrencyString(TotalMoney));
			}
		}

		#endregion

		public Fine ()
		{
		}

		#region Методы

		public virtual void AddItem (Employee employee)
		{
			var item = new FineItem()
				{
					Employee = employee,
					Fine = this
				};
			ObservableItems.Add (item);
		}

		public virtual void AddNomenclature (Dictionary<Nomenclature, decimal> nomenclatureAmounts)
		{
			foreach(var nom in nomenclatureAmounts)
			{
				Nomenclatures.Add(new FineNomenclature{
					Fine = this,
					Nomenclature = nom.Key,
					Amount = nom.Value
				});
			}
		}

		public virtual void UpdateNomenclature (Dictionary<Nomenclature, decimal> nomenclatureAmounts)
		{
			foreach(var nom in Nomenclatures.ToList())
			{
				if (nomenclatureAmounts.All(x => x.Key.Id != nom.Nomenclature.Id))
					Nomenclatures.Remove(nom);
			}

			foreach(var nom in nomenclatureAmounts)
			{
				var item = Nomenclatures.FirstOrDefault(x => x.Nomenclature.Id == nom.Key.Id);
				if (item == null)
				{
					Nomenclatures.Add(new FineNomenclature
						{
							Fine = this,
							Nomenclature = nom.Key,
							Amount = nom.Value
						});
				}
				else
					item.Amount = nom.Value;
			}
		}

		public virtual void DivideAtAll()
		{
			if (Items.Count == 0)
				return;
			var part = Math.Round(TotalMoney / Items.Count, 2);
			foreach(var item in Items)
			{
				item.Money = part;
			}
		}

		public virtual void UpdateWageOperations(IUnitOfWork uow)
		{
			foreach (var item in Items)
			{
				if(item.WageOperation == null)
				{
					item.WageOperation = new WagesMovementOperations
						{
							OperationType = WagesType.HoldedFine,
							Employee 	  = item.Employee,
							Money 		  = item.Money * (-1),
							OperationTime = this.Date
						};
				} else {
					item.WageOperation.OperationType = WagesType.HoldedFine;
					item.WageOperation.Employee 	 = item.Employee;
					item.WageOperation.Money 		 = item.Money * (-1);
				}
				uow.Save(item.WageOperation);
			}
		}

		public virtual void Fill (decimal money, RouteList routeList, string reasonString, DateTime date, params Employee[] employees)
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
				yield return new ValidationResult (String.Format("Отсутствуют сотрудники на которых назначен штраф."),
					new[] { this.GetPropertyName (o => o.Items) });

			var totalSum = Items.Sum(x => x.Money);
			if(totalSum != TotalMoney)
				yield return new ValidationResult (String.Format("Общая сумма штрафа {0:C}, отличается от суммы штрафов всех сотрудников {1:C}.",
					TotalMoney, totalSum),
					new[] { this.GetPropertyName (o => o.Items) });

			if(string.IsNullOrWhiteSpace(FineReasonString))
				yield return new ValidationResult (String.Format("Отсутствует причина выдачи штрафа."),
					new[] { this.GetPropertyName (o => o.FineReasonString) });
			
			if(FineType == FineTypes.FuelOverspending && RouteList == null) {
				yield return new ValidationResult(String.Format("Не выбран маршрутный лист, при типе штрафа \"{0}\"", FineType.GetEnumTitle()));
			}

			if(!QSMain.User.Permissions["can_delete_fines"] && Id > 0){
				yield return new ValidationResult(String.Format("Недостаточно прав для изменения штрафа!"));
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
		public FineTypeStringType() : base(typeof(FineTypes))
		{
		}
	}
}

