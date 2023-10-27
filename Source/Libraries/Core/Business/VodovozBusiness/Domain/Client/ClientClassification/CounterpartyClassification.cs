using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client.ClientClassification
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "классификации контрагентов",
		Nominative = "классификация контрагента")]
	public class CounterpartyClassification : PropertyChangedBase, IDomainObject
	{
		private int _counterpartyId;
		private CounterpartyClassificationByBottlesCount _classificationByBottlesCount;
		private CounterpartyClassificationByOrdersCount _classificationByOrdersCount;
		private decimal _bottlesPerMonthAverageCount;
		private decimal _ordersPerMonthAverageCount;
		private decimal _moneyTurnoverPerMonthAverageSum;
		private DateTime _classificationCalculationDate;

		public virtual int Id { get; }

		[Display(Name = "Id контрагента")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		[Display(Name = "Классификация по среднему количеству бутылей")]
		public virtual CounterpartyClassificationByBottlesCount ClassificationByBottlesCount
		{
			get => _classificationByBottlesCount;
			set => SetField(ref _classificationByBottlesCount, value);
		}

		[Display(Name = "Классификация по среднему количеству заказов")]
		public virtual CounterpartyClassificationByOrdersCount ClassificationByOrdersCount
		{
			get => _classificationByOrdersCount;
			set => SetField(ref _classificationByOrdersCount, value);
		}

		[Display(Name = "Среднее количество бутылей в месяц")]
		public virtual decimal BottlesPerMonthAverageCount
		{
			get => _bottlesPerMonthAverageCount;
			set => SetField(ref _bottlesPerMonthAverageCount, value);
		}

		[Display(Name = "Среднее количество заказов в месяц")]
		public virtual decimal OrdersPerMonthAverageCount
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
}
