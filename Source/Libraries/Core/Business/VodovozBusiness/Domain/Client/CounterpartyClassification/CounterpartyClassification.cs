using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client.CounterpartyClassification
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "классификации контрагентов",
		Nominative = "классификация контрагента")]
	public class CounterpartyClassification : PropertyChangedBase, IDomainObject
	{
		private ClientClassificationByBottlesCount _classificationByBottlesCount;
		private ClientClassificationByOrdersCount _classificationByOrdersCount;
		private int _bottlesPerMonthAverageCount;
		private int _ordersPerMonthAverageCount;
		private decimal _moneyTurnoverPerMonthAverageSum;
		private DateTime _classificationCalculationDate;

		public virtual int Id { get; }

		[Display(Name = "Классификация по среднему количеству бутылей")]
		public virtual ClientClassificationByBottlesCount ClassificationByBottlesCount
		{
			get => _classificationByBottlesCount;
			set => SetField(ref _classificationByBottlesCount, value);
		}

		[Display(Name = "Классификация по среднему количеству заказов")]
		public virtual ClientClassificationByOrdersCount ClassificationByOrdersCount
		{
			get => _classificationByOrdersCount;
			set => SetField(ref _classificationByOrdersCount, value);
		}

		[Display(Name = "Среднее количество бутылей в месяц")]
		public virtual int BottlesPerMonthAverageCount
		{
			get => _bottlesPerMonthAverageCount;
			set => SetField(ref _bottlesPerMonthAverageCount, value);
		}

		[Display(Name = "Среднее количество заказов в месяц")]
		public virtual int OrdersPerMonthAverageCount
		{
			get => _ordersPerMonthAverageCount;
			set => SetField(ref _ordersPerMonthAverageCount, value);
		}

		[Display(Name = "Средний оборот (сумма всех заказов) в месяц")]
		public virtual decimal MoneyTurnoverPerMonthAverageSum
		{
			get => _moneyTurnoverPerMonthAverageSum;
			set => SetField(ref _moneyTurnoverPerMonthAverageSum, value);
		}

		[Display(Name = "Дата выполнения расчета классификации")]
		public virtual DateTime ClassificationCalculationDate
		{
			get => _classificationCalculationDate;
			set => SetField(ref _classificationCalculationDate, value);
		}
	}

	public enum ClientClassificationByBottlesCount
	{
		[Display(Name = "A")]
		A,
		[Display(Name = "B")]
		B,
		[Display(Name = "C")]
		C
	}

	public enum ClientClassificationByOrdersCount
	{
		[Display(Name = "X")]
		X,
		[Display(Name = "Y")]
		Y,
		[Display(Name = "Z")]
		Z
	}
}
