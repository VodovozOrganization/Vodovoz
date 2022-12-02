using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Profitability
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Рентабельности МЛ",
		Nominative = "Рентабельность МЛ")]
	public class RouteListProfitability : PropertyChangedBase, IDomainObject
	{
		private decimal _mileage;
		private decimal _amortisation;
		private decimal _repairCosts;
		private decimal _fuelCosts;
		private decimal _driverAndForwarderWages;
		private decimal _paidDelivery;
		private decimal _routeListExpenses;
		private decimal _totalGoodsWeight;
		private decimal _routeListExpensesPerKg;
		private decimal? _salesSum;
		private decimal? _expensesSum;
		private decimal? _grossMarginSum;
		private decimal? _grossMarginPercents;
		private DateTime? _profitabilityConstantsCalculatedMonth;
		
		public virtual int Id { get; set; }

		[Display(Name = "Пробег, км")]
		public virtual decimal Mileage
		{
			get => _mileage;
			set => SetField(ref _mileage, value);
		}

		[Display(Name = "Амортизация, руб")]
		public virtual decimal Amortisation
		{
			get => _amortisation;
			set => SetField(ref _amortisation, value);
		}

		[Display(Name = "Ремонт, руб")]
		public virtual decimal RepairCosts
		{
			get => _repairCosts;
			set => SetField(ref _repairCosts, value);
		}

		[Display(Name = "Топливо, руб")]
		public virtual decimal FuelCosts
		{
			get => _fuelCosts;
			set => SetField(ref _fuelCosts, value);
		}

		[Display(Name = "Зарплата водителя и экспедитора")]
		public virtual decimal DriverAndForwarderWages
		{
			get => _driverAndForwarderWages;
			set => SetField(ref _driverAndForwarderWages, value);
		}

		[Display(Name = "Оплата доставки клиентом")]
		public virtual decimal PaidDelivery
		{
			get => _paidDelivery;
			set => SetField(ref _paidDelivery, value);
		}

		[Display(Name = "Затраты на МЛ")]
		public virtual decimal RouteListExpenses
		{
			get => _routeListExpenses;
			set => SetField(ref _routeListExpenses, value);
		}

		[Display(Name = "Вывезено, кг")]
		public virtual decimal TotalGoodsWeight
		{
			get => _totalGoodsWeight;
			set => SetField(ref _totalGoodsWeight, value);
		}
		
		[Display(Name = "Затраты на кг")]
		public virtual decimal RouteListExpensesPerKg
		{
			get => _routeListExpensesPerKg;
			set => SetField(ref _routeListExpensesPerKg, value);
		}
		
		[Display(Name = "Дата рассчитанных констант")]
		public virtual DateTime? ProfitabilityConstantsCalculatedMonth
		{
			get => _profitabilityConstantsCalculatedMonth;
			set => SetField(ref _profitabilityConstantsCalculatedMonth, value);
		}
		
		[Display(Name = "Сумма продаж, руб")]
		public virtual decimal? SalesSum
		{
			get => _salesSum;
			set => SetField(ref _salesSum, value);
		}

		[Display(Name = "Сумма затрат, руб")]
		public virtual decimal? ExpensesSum
		{
			get => _expensesSum;
			set => SetField(ref _expensesSum, value);
		}

		[Display(Name = "Валовая маржа, руб")]
		public virtual decimal? GrossMarginSum
		{
			get => _grossMarginSum;
			set => SetField(ref _grossMarginSum, value);
		}

		[Display(Name = "Валовая маржа, %")]
		public virtual decimal? GrossMarginPercents
		{
			get => _grossMarginPercents;
			set => SetField(ref _grossMarginPercents, value);
		}
	}
}
