using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Domain.Fuel;
using NLog;
using Vodovoz.Tools;
using Vodovoz.Repository.Cash;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Repositories;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "документы выдачи топлива",
		Nominative = "документ выдачи топлива")]
	public class FuelDocument : BusinessObjectBase<FuelDocument>, IDomainObject, IValidatableObject
	{
		Logger logger = LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		private DateTime date;

		[Display(Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField(ref date, value, () => Date); }
		}

		private Employee driver;

		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		private Car car;

		[Display(Name = "Транспортное средство")]
		public virtual Car Car {
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

		[Display(Name = "Операции выдачи")]
		public virtual FuelOperation FuelOperation {
			get { return operation; }
			set { SetField(ref operation, value, () => FuelOperation); }
		}

		private FuelExpenseOperation fuelExpenseOperation;
		[Display(Name = "Операции списания топлива")]
		public virtual FuelExpenseOperation FuelExpenseOperation {
			get => fuelExpenseOperation;
			set => SetField(ref fuelExpenseOperation, value, () => FuelExpenseOperation);
		}

		private decimal? payedForLiter;

		[Display(Name = "Топливо, оплаченное деньгами")]
		public virtual decimal? PayedForFuel {
			get { return payedForLiter; }
			set { SetField(ref payedForLiter, value, () => PayedForFuel); }
		}

		private decimal literCost;


		[Display(Name = "Стоимость литра топлива")]
		public virtual decimal LiterCost {
			get { return literCost; }
			set { SetField(ref literCost, value, () => LiterCost); }
		}

		private FuelType fuel;

		[Display(Name = "Вид топлива")]
		public virtual FuelType Fuel {
			get { return fuel; }
			set { SetField(ref fuel, value, () => Fuel); }
		}

		private int fuelCoupons;

		[Display(Name = "Литры выданные талонами")]
		public virtual int FuelCoupons {
			get { return fuelCoupons; }
			set { SetField(ref fuelCoupons, value, () => FuelCoupons); }
		}

		private Expense fuelCashExpense;

		[Display(Name = "Оплата топлива")]
		public virtual Expense FuelCashExpense {
			get { return fuelCashExpense; }
			set { SetField(ref fuelCashExpense, value, () => FuelCashExpense); }
		}

		private Employee author;

		[Display(Name = "Автор документа")]
		public virtual Employee Author {
			get { return author; }
			set { SetField(ref author, value, () => Author); }
		}

		private Employee lastEditor;

		[Display(Name = "Автор последней правки")]
		public virtual Employee LastEditor {
			get { return lastEditor; }
			set { SetField(ref lastEditor, value, () => LastEditor); }
		}

		private DateTime lastEditDate;

		[Display(Name = "Дата последней правки")]
		public virtual DateTime LastEditDate {
			get { return lastEditDate; }
			set { SetField(ref lastEditDate, value, () => LastEditDate); }
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value, () => Subdivision);
		}


		public virtual decimal PayedLiters {
			get {
				if(Fuel == null || Fuel.Cost <= 0 || !PayedForFuel.HasValue) {
					return 0;
				}

				return Math.Round(PayedForFuel.Value / Fuel.Cost, 2, MidpointRounding.AwayFromZero);
			}
		}

		public FuelDocument()
		{ }

		public virtual void CreateOperations(IFuelRepository fuelRepository)
		{
			if(fuelRepository == null) {
				throw new ArgumentNullException(nameof(fuelRepository));
			}

			ExpenseCategory expenseCategory = CategoryRepository.FuelDocumentExpenseCategory(UoW);
			if(expenseCategory == null) {
				throw new InvalidProgramException("Не возможно найти подходящую статью расхода, возможно в параметрах базы не настроена статья расхода по умолчанию.");
			}

			ValidationContext context = new ValidationContext(this, new Dictionary<object, object>() {
				{"Reason", nameof(CreateOperations)}
			});
			context.ServiceContainer.AddService(typeof(IFuelRepository), fuelRepository);
			string validationMessage = this.RaiseValidationAndGetResult(context);
			if(!string.IsNullOrWhiteSpace(validationMessage)) {
				throw new ValidationException(validationMessage);
			}

			try {
				CreateFuelOperation();
				CreateFuelExpenseOperation();
				CreateFuelCashExpense(expenseCategory);
			} catch(Exception ex) {
				//восстановление исходного состояния
				FuelOperation = null;
				FuelExpenseOperation = null;
				FuelCashExpense = null;
				logger.Error(ex, "Ошибка при создании операций для выдачи топлива");
				throw;
			}
		}

		private void CreateFuelOperation()
		{
			if(FuelOperation != null) {
				logger.Warn("Попытка создания операции выдачи топлива при уже имеющейся операции");
				return;
			}

			FuelOperation = new FuelOperation() {
				Driver = Car.IsCompanyHavings ? null : Driver,
				Car = Car.IsCompanyHavings ? Car : null,
				Fuel = Fuel,
				LitersGived = FuelCoupons,
				LitersOutlayed = 0,
				PayedLiters = PayedForFuel.HasValue ? PayedLiters : 0,
				OperationTime = Date
			};
		}

		private void CreateFuelExpenseOperation()
		{
			if(FuelCoupons <= 0) {
				return;
			}

			if(FuelExpenseOperation != null) {
				logger.Warn("Попытка создания операции списания топлива при уже имеющейся операции");
				return;
			}

			FuelExpenseOperation = new FuelExpenseOperation() {
				FuelDocument = this,
				FuelType = Fuel,
				FuelLiters = FuelCoupons,
				RelatedToSubdivision = Subdivision,
				СreationTime = Date
			};
		}

		private void CreateFuelCashExpense(ExpenseCategory expenseCategory)
		{
			if(!PayedForFuel.HasValue || (PayedForFuel.HasValue && PayedForFuel.Value <= 0)) {
				return;
			}

			if(FuelCashExpense != null) {
				logger.Warn("Попытка создания операции оплаты топлива при уже имеющейся операции");
				return;
			}

			FuelCashExpense = new Expense {
				ExpenseCategory = expenseCategory,
				TypeOperation = ExpenseType.Expense,
				Date = Date,
				Casher = Author,
				Employee = Driver,
				RelatedToSubdivision = Subdivision,
				Description = $"Оплата топлива по МЛ №{RouteList.Id}",
				Money = Math.Round(PayedForFuel.Value, 2, MidpointRounding.AwayFromZero)
			};
		}

		public virtual Employee GetActualCashier(IUnitOfWork uow)
		{
			return EmployeeRepository.GetEmployeeForCurrentUser(uow);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(RouteList.ClosingSubdivision == null) {
				yield return new ValidationResult("Касса в маршрутном листе должна быть заполнена");
			}

			if(Subdivision == null) {
				yield return new ValidationResult("Необходимо выбрать кассу, с которой будет списываться топливо");
			}

			if(Fuel == null) {
				yield return new ValidationResult("Топливо должно быть заполнено");
			}

			if(FuelCoupons <= 0 && PayedLiters <= 0) {
				yield return new ValidationResult("Не указано сколько топлива выдается.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.PayedLiters) });
			}


			if(validationContext.Items.ContainsKey("Reason") && (validationContext.Items["Reason"] as string) == nameof(CreateOperations)) {
				if(!(validationContext.GetService(typeof(IFuelRepository)) is IFuelRepository fuelRepository)) {
					throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IFuelRepository)}");
				}

				if(Subdivision != null && Fuel != null) {
					decimal balance = fuelRepository.GetFuelBalanceForSubdivision(UoW, Subdivision, Fuel);
					if(FuelCoupons > balance && FuelCoupons > 0) {
						yield return new ValidationResult("На балансе недостаточно топлива для выдачи");
					}
				}
			}
		}

		#endregion
	}
}

