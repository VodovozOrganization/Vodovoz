using NLog;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Organizations;
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
		Logger _logger = LogManager.GetCurrentClassLogger();

		private DateTime _date;
		private Employee _driver;
		private Car _car;
		private RouteList _routeList;
		private FuelOperation _operation;
		private FuelExpenseOperation _fuelExpenseOperation;
		private decimal? _payedForLiter;
		private FuelPaymentType? _fuelPaymentType;
		private decimal _literCost;
		private FuelType _fuel;
		private decimal _fuelLimitsLitersAmount;
		private Expense _fuelCashExpense;
		private Employee _author;
		private Employee _lastEditor;
		private DateTime _lastEditDate;
		private Subdivision _subdivision;
		private string _fuelCardNumber;
		private FuelLimit _fuelLimit;

		public virtual int Id { get; set; }

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Транспортное средство")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[Display(Name = "Операции выдачи")]
		public virtual FuelOperation FuelOperation
		{
			get => _operation;
			set => SetField(ref _operation, value);
		}

		[Display(Name = "Операции списания топлива")]
		public virtual FuelExpenseOperation FuelExpenseOperation
		{
			get => _fuelExpenseOperation;
			set => SetField(ref _fuelExpenseOperation, value);
		}

		[Display(Name = "Топливо, оплаченное деньгами")]
		public virtual decimal? PayedForFuel
		{
			get => _payedForLiter;
			set => SetField(ref _payedForLiter, value);
		}

		public virtual FuelPaymentType? FuelPaymentType
		{
			get => _fuelPaymentType;
			set => SetField(ref _fuelPaymentType, value);
		}

		[IgnoreHistoryTrace]
		[Display(Name = "Стоимость литра топлива")]
		public virtual decimal LiterCost
		{
			get => _literCost;
			set => SetField(ref _literCost, value);
		}

		[Display(Name = "Вид топлива")]
		public virtual FuelType Fuel
		{
			get => _fuel;
			set => SetField(ref _fuel, value);
		}

		[Display(Name = "Литры выданные лимитами")]
		public virtual decimal FuelLimitLitersAmount
		{
			get => _fuelLimitsLitersAmount;
			set => SetField(ref _fuelLimitsLitersAmount, value);
		}

		[Display(Name = "Оплата топлива")]
		public virtual Expense FuelCashExpense
		{
			get => _fuelCashExpense;
			set => SetField(ref _fuelCashExpense, value);
		}

		[Display(Name = "Автор документа")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Автор последней правки")]
		public virtual Employee LastEditor
		{
			get => _lastEditor;
			set => SetField(ref _lastEditor, value);
		}

		[Display(Name = "Дата последней правки")]
		public virtual DateTime LastEditDate
		{
			get => _lastEditDate;
			set => SetField(ref _lastEditDate, value);
		}

		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}

		[Display(Name = "Номер топливной карты")]
		public virtual string FuelCardNumber
		{
			get => _fuelCardNumber;
			set => SetField(ref _fuelCardNumber, value);
		}

		[Display(Name = "Лимит по топливу")]
		public virtual FuelLimit FuelLimit
		{
			get => _fuelLimit;
			set => SetField(ref _fuelLimit, value);
		}

		public virtual decimal PayedLiters
		{
			get
			{
				if(Fuel == null || Fuel.Cost <= 0 || !PayedForFuel.HasValue)
				{
					return 0;
				}

				return Math.Round(PayedForFuel.Value / Fuel.Cost, 2, MidpointRounding.AwayFromZero);
			}
		}

		public FuelDocument() { }

		public virtual string Title => string.Format("{0} №{1}", this.GetType().GetSubjectName(), Id);

		public virtual void CreateOperations(IFuelRepository fuelRepository,
			IOrganizationRepository organizationRepository,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings)
		{
			if(fuelRepository == null)
			{
				throw new ArgumentNullException(nameof(fuelRepository));
			}

			if(organizationRepository == null)
			{
				throw new ArgumentNullException(nameof(organizationRepository));
			}

			ValidationContext context = new ValidationContext(this, new Dictionary<object, object>() {
				{"Reason", nameof(CreateOperations)}
			});
			context.InitializeServiceProvider(type => { if (type == typeof(IFuelRepository)) { return fuelRepository; } return null; });
			string validationMessage = this.RaiseValidationAndGetResult(context);
			if(!string.IsNullOrWhiteSpace(validationMessage))
			{
				throw new ValidationException(validationMessage);
			}

			try
			{
				CreateFuelOperation();
				CreateFuelCashExpense(financialCategoriesGroupsSettings.FuelFinancialExpenseCategoryId, organizationRepository);
			}
			catch(Exception ex)
			{
				//восстановление исходного состояния
				FuelOperation = null;
				FuelExpenseOperation = null;
				FuelCashExpense = null;
				_logger.Error(ex, "Ошибка при создании операций для выдачи топлива");
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
			FuelOperation.LitersGived = FuelLimitLitersAmount + FuelOperation.PayedLiters;
		}

		private void CreateFuelOperation()
		{
			if(FuelOperation != null)
			{
				_logger.Warn("Попытка создания операции выдачи топлива при уже имеющейся операции");
				return;
			}

			var litersPaid = PayedForFuel.HasValue ? PayedLiters : 0;

			var activeCarVersion = Car.GetActiveCarVersionOnDate(RouteList.Date);

			FuelOperation = new FuelOperation()
			{
				Driver = activeCarVersion.IsCompanyCar ? null : Driver,
				Car = activeCarVersion.IsCompanyCar ? Car : null,
				Fuel = Fuel,
				LitersGived = FuelLimitLitersAmount + litersPaid,
				LitersOutlayed = 0,
				PayedLiters = litersPaid,
				OperationTime = Date
			};
		}

		private void CreateFuelCashExpense(int financialExpenseCategory,
			IOrganizationRepository organizationRepository)
		{
			if(FuelPaymentType.HasValue && FuelPaymentType.Value == Logistic.FuelPaymentType.Cashless)
				return;

			if(!PayedForFuel.HasValue || (PayedForFuel.HasValue && PayedForFuel.Value <= 0))
				return;

			if(FuelCashExpense != null)
			{
				_logger.Warn("Попытка создания операции оплаты топлива при уже имеющейся операции");
				return;
			}

			FuelCashExpense = new Expense
			{
				ExpenseCategoryId = financialExpenseCategory,
				TypeOperation = ExpenseType.Advance,
				Date = Date,
				Casher = Author,
				Employee = Driver,
				Organisation = organizationRepository.GetCommonOrganisation(UoW),
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
			SetFuelCardNumberByDocumentDate();
		}

		public virtual void SetFuelCardNumberByDocumentDate()
		{
			FuelCardNumber = Car?.GetActiveFuelCardVersionOnDate(Date)?.FuelCard?.CardNumber;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Subdivision == null && PayedForFuel > 0m)
			{
				yield return new ValidationResult("Необходимо выбрать кассу, с которой будет списываться топливо");
			}

			if(Fuel == null)
			{
				yield return new ValidationResult("Топливо должно быть заполнено");
			}

			if(FuelLimitLitersAmount <= 0 && PayedLiters <= 0)
			{
				yield return new ValidationResult("Не указано сколько топлива выдается.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.PayedLiters) });
			}

			if(Id <= 0 && PayedForFuel > 0m && FuelPaymentType == null)
			{
				yield return new ValidationResult("Не указан тип оплаты топлива.",
					new[] { Gamma.Utilities.PropertyUtil.GetPropertyName(this, o => o.FuelPaymentType) });
			}

			if(FuelLimitLitersAmount > 0 && FuelCardNumber is null)
			{
				yield return new ValidationResult(
					"При выдаче топливных лимитов у авто должна быть указана топливная карта",
					new[] { nameof(FuelCardNumber) });
			}

			if(FuelLimitLitersAmount > 0 && PayedForFuel.HasValue && PayedForFuel.Value > 0)
			{
				yield return new ValidationResult(
					"Нельзя выдавать топливо лимитами и деньгами одновременно",
					new[] { nameof(FuelLimitLitersAmount), nameof(PayedForFuel) });
			}

			if(Id > 0 && FuelLimitLitersAmount > 0)
			{
				yield return new ValidationResult(
					"Нельзя вносить изменения в документ по которому уже выдано топливо лимитами",
					new[] { nameof(FuelLimitLitersAmount) });
			}

			if(Id == 0 && FuelLimitLitersAmount > 0 && RouteList?.Date < DateTime.Today)
			{
				yield return new ValidationResult(
					"Нельзя выдавать топливо лимитами на маршрутные листы за предыдущие дни",
					new[] { nameof(RouteList) });
			}

			if(Id == 0 && RouteList?.HasAddressesOrAdditionalLoading == false)
			{
				yield return new ValidationResult(
					"Запрещено выдавать топливо для МЛ без адресов или без погруженного запаса",
					new[] { nameof(RouteList) });
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
}
