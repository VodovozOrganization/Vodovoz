using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "документы выдачи топлива",
		Nominative = "документ выдачи топлива")]
	public class FuelDocument: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set {SetField(ref date, value, () => Date);}
		}

		private Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set {SetField(ref driver, value, () => Driver);}
		}

		private Car car;

		[Display(Name = "Транспортное средство")]
		public virtual Car Car
		{
			get { return car; }
			set { SetField(ref car, value, () => Car); }
		}

		private RouteList routeList;

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get { return routeList; }
			set { SetField(ref routeList, value, () => RouteList); }
		}

		private FuelOperation operation;

		[Display (Name = "Операции выдачи")]
		public virtual FuelOperation Operation {
			get { return operation; }
			set {SetField(ref operation, value, () => Operation);}
		}

		private decimal? payedForLiter;


		[Display (Name = "Топливо, оплаченное деньгами")]
		public virtual decimal? PayedForFuel {
			get { return payedForLiter; }
			set {SetField(ref payedForLiter, value, () => PayedForFuel);}
		}

		private decimal literCost;


		[Display (Name = "Стоимость литра топлива")]
		public virtual decimal LiterCost {
			get { return literCost; }
			set { SetField(ref literCost, value, () => LiterCost);}
		}

		private FuelType fuel;

		[Display (Name = "Вид топлива")]
		public virtual FuelType Fuel {
			get { return fuel; }
			set { SetField(ref fuel, value, () => Fuel);}
		}

		private int fuelCoupons;

		[Display (Name = "Литры выданные талонами")]
		public virtual int FuelCoupons {
			get { return fuelCoupons; }
			set { SetField (ref fuelCoupons, value, () => FuelCoupons); }
		}

		private Expense fuelCashExpense;

		[Display(Name = "Оплата топлива")]
		public virtual Expense FuelCashExpense {
			get { return fuelCashExpense; }
			set { SetField(ref fuelCashExpense, value, () => FuelCashExpense); }
		}

		private Employee author;

		[Display(Name = "Автор документа")]
		public virtual Employee Author{
			get { return author; }
			set { SetField(ref author, value, () => Author);}
		}

		private Employee lastEditor;

		[Display(Name = "Автор последней правки")]
		public virtual Employee LastEditor {
			get { return lastEditor; }
			set { SetField(ref lastEditor, value, () => LastEditor); }
		}

		private DateTime lastEditDate;

		[Display(Name = "Дата последней правки")]
		public virtual DateTime LastEditDate{
			get { return lastEditDate; }
			set { SetField(ref lastEditDate, value, () => LastEditDate);}
		}

		public FuelDocument()
		{
		}

		public virtual void UpdateOperation() 
		{
			decimal litersByMoney = 0;
			if(Fuel.Cost > 0 && PayedForFuel.HasValue)
				litersByMoney = PayedForFuel.Value / Fuel.Cost;
			if (Operation == null)
				Operation = new FuelOperation();

			Car car = Car;
			Employee driver = Driver;
			if (car.IsCompanyHavings)
				driver = null;
			else
				car = null;
			
			Operation.Driver 		 = driver;
			Operation.Car			 = car;
			Operation.Fuel 			 = Fuel;
			Operation.LitersGived 	 = fuelCoupons + litersByMoney;
			Operation.LitersOutlayed = 0;
			Operation.OperationTime  = Date;
		}

		public virtual void UpdateFuelCashExpense(IUnitOfWork uow, Employee cashier)
		{
			if (PayedForFuel.HasValue) {
				if (FuelCashExpense == null) {
					FuelCashExpense = new Expense
					{
						ExpenseCategory = Repository.Cash.CategoryRepository.FuelDocumentExpenseCategory(uow),
						TypeOperation 	= ExpenseType.Expense,
						Date 			= DateTime.Now,
						Casher 			= cashier,
						Employee 		= Driver,
						Description 	=$"Оплата топлива по МЛ №{RouteList.Id}",
					};
				}
				FuelCashExpense.Money = Math.Round(PayedForFuel.Value, 0, MidpointRounding.AwayFromZero);
			}
			else
				FuelCashExpense = null;
		}

		public virtual Employee GetActualCashier(IUnitOfWork uow)
		{
			if(RouteList.Cashier == null) {
				return EmployeeRepository.GetEmployeeForCurrentUser(uow);
			}
			return RouteList.Cashier;
		}

		public virtual void UpdateDocument(IUnitOfWork uow)
		{
			Car = RouteList.Car;
			Driver = RouteList.Driver;

			var cashier = GetActualCashier(uow);
			if(cashier == null) {
				return;
			}

			UpdateFuelCashExpense(uow, cashier);
			UpdateOperation();
		}


#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Operation == null || operation.LitersGived <= 0)
			{
				yield return new ValidationResult("Документ должен выдавать литры топлива. Сейчас их не более нуля.",
					new[] {Gamma.Utilities.PropertyUtil.GetPropertyName(this, o=>o.Operation)});
			}
		}

		#endregion
	}
}

