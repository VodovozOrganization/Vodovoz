using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NLog;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Parameters;
using Vodovoz.Settings.Cash;
using Vodovoz.Tools;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "документы выдачи топлива",
		Nominative = "документ выдачи топлива")]
	[HistoryTrace]
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

		private Car _car;

		[Display(Name = "Транспортное средство")]
		public virtual Car Car {
			get { return _car; }
			set { SetField(ref _car, value, () => Car); }
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

		private FuelPaymentType? fuelPaymentType;
		public virtual FuelPaymentType? FuelPaymentType {
			get => fuelPaymentType;
			set => SetField(ref fuelPaymentType, value);
		}

		private decimal literCost;

		[IgnoreHistoryTrace]
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

		string fuelCardNumber;
		[Display(Name = "Номер топливной карты")]
		public virtual string FuelCardNumber {
			get => fuelCardNumber;
			set => SetField(ref fuelCardNumber, value, () => FuelCardNumber);
		}

		public virtual decimal PayedLiters {
			get {
				if(Fuel == null || Fuel.Cost <= 0 || !PayedForFuel.HasValue)
					return 0;
				return Math.Round(PayedForFuel.Value / Fuel.Cost, 2, MidpointRounding.AwayFromZero);
			}
		}

		public FuelDocument() { }

		public virtual string Title => string.Format("{0} №{1}", this.GetType().GetSubjectName(), Id);

		public virtual void CreateOperations(IFuelRepository fuelRepository, 
			CashDistributionCommonOrganisationProvider commonOrganisationProvider,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings)
		{
			if(fuelRepository == null) {
				throw new ArgumentNullException(nameof(fuelRepository));
			}
			
			if(commonOrganisationProvider == null) {
				throw new ArgumentNullException(nameof(commonOrganisationProvider));
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
				CreateFuelCashExpense(financialCategoriesGroupsSettings.FuelExpenseCategoryId, commonOrganisationProvider);
			} catch(Exception ex) {
				//восстановление исходного состояния
				FuelOperation = null;
				FuelExpenseOperation = null;
				FuelCashExpense = null;
				logger.Error(ex, "Ошибка при создании операций для выдачи топлива");
				throw;
			}
		}

		public virtual void UpdateFuelOperation()
		{
			if(FuelOperation.PayedLiters <= 0m)
			{
				return;
			}

			FuelOperation.PayedLiters = Math.Round(PayedForFuel.Value / LiterCost, 2, MidpointRounding.AwayFromZero);
			FuelOperation.LitersGived = FuelCoupons + FuelOperation.PayedLiters;
		}

		private void CreateFuelOperation()
		{
			if(FuelOperation != null) {
				logger.Warn("Попытка создания операции выдачи топлива при уже имеющейся операции");
				return;
			}

			var litersPaid = PayedForFuel.HasValue ? PayedLiters : 0;

			var activeCarVersion = Car.GetActiveCarVersionOnDate(RouteList.Date);
			
			FuelOperation = new FuelOperation() {
				Driver = activeCarVersion.IsCompanyCar ? null : Driver,
				Car = activeCarVersion.IsCompanyCar ? Car : null,
				Fuel = Fuel,
				LitersGived = FuelCoupons + litersPaid,
				LitersOutlayed = 0,
				PayedLiters = litersPaid,
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

		private void CreateFuelCashExpense(int financialExpenseCategory, 
			CashDistributionCommonOrganisationProvider commonOrganisationProvider)
		{
			if(FuelPaymentType.HasValue && FuelPaymentType.Value == Logistic.FuelPaymentType.Cashless)
				return;

			if(!PayedForFuel.HasValue || (PayedForFuel.HasValue && PayedForFuel.Value <= 0))
				return;

			if(FuelCashExpense != null) {
				logger.Warn("Попытка создания операции оплаты топлива при уже имеющейся операции");
				return;
			}

			FuelCashExpense = new Expense {
				ExpenseCategoryId = financialExpenseCategory,
				TypeOperation = ExpenseType.Advance,
				Date = Date,
				Casher = Author,
				Employee = Driver,
				Organisation = commonOrganisationProvider.GetCommonOrganisation(UoW),
				RelatedToSubdivision = Subdivision,
				Description = $"Оплата топлива по МЛ №{RouteList.Id}",
				Money = Math.Round(PayedForFuel.Value, 2, MidpointRounding.AwayFromZero)
			};
		}

		public virtual void FillEntity(RouteList rl)
		{
			Date = DateTime.Now;
			Car = rl.Car;
			Driver = rl.Driver;
			Fuel = rl.Car.FuelType;
			LiterCost = rl.Car.FuelType.Cost;
			RouteList = rl;
			FuelCardNumber = rl.Car.FuelCardNumber;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
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

			if(Id <= 0 && PayedForFuel > 0m && FuelPaymentType == null) {
				yield return new ValidationResult("Не указан тип оплаты топлива.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.FuelPaymentType) });
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

	public enum FuelPaymentType
	{
		[Display(Name = "нал")]
		Cash,
		[Display(Name = "безнал")]
		Cashless
	}

	public class FuelPaymentTypeStringType : NHibernate.Type.EnumStringType
	{
		public FuelPaymentTypeStringType() : base(typeof(FuelPaymentType))
		{
		}
	}
}

