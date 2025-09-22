using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client.ClientClassification
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "классификации контрагентов",
		Nominative = "классификация контрагента",
		GenitivePlural = "классификаций контрагентов")]
	public class CounterpartyClassification : PropertyChangedBase, IDomainObject
	{
		private int _counterpartyId;
		private CounterpartyClassificationByBottlesCount _classificationByBottlesCount;
		private CounterpartyClassificationByOrdersCount _classificationByOrdersCount;
		private decimal _bottlesPerMonthAverageCount;
		private decimal _ordersPerMonthAverageCount;
		private decimal _moneyTurnoverPerMonthAverageSum;
		private int _classificationCalculationSettingsId;

		public CounterpartyClassification(
			int counterpartyId,
			decimal bottlesCount,
			decimal ordersCount,
			decimal moneyTurnoverSum,
			CounterpartyClassificationCalculationSettings calculationSettings)
		{
			if(calculationSettings is null)
			{
				throw new ArgumentNullException(nameof(calculationSettings));
			}

			CounterpartyId = counterpartyId;

			ClassificationByBottlesCount = GetClassificationByBottlesCount(bottlesCount, calculationSettings);
			ClassificationByOrdersCount = GetClassificationByOrdersCount(ordersCount, calculationSettings);

			if(calculationSettings.PeriodInMonths > 0)
			{
				BottlesPerMonthAverageCount = bottlesCount / calculationSettings.PeriodInMonths;
				OrdersPerMonthAverageCount = ordersCount / calculationSettings.PeriodInMonths;
				MoneyTurnoverPerMonthAverageSum = moneyTurnoverSum / calculationSettings.PeriodInMonths;
			}

			ClassificationCalculationSettingsId = calculationSettings.Id;
		}

		public CounterpartyClassification()
		{
			ClassificationByBottlesCount = CounterpartyClassificationByBottlesCount.C;
			ClassificationByOrdersCount = CounterpartyClassificationByOrdersCount.Z;
		}

		#region Properties
		public virtual int Id { get; set; }

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

		[Display(Name = "Id записи настроек рассчета классификации")]
		public virtual int ClassificationCalculationSettingsId
		{
			get => _classificationCalculationSettingsId;
			set => SetField(ref _classificationCalculationSettingsId, value);
		}
		#endregion Properties

		public static CounterpartyClassificationByBottlesCount GetClassificationByBottlesCount(
			decimal bottlesCount,
			CounterpartyClassificationCalculationSettings calculationSettings)
		{
			var bottlesPerMonthAverageCount = (calculationSettings.PeriodInMonths > 0)
				? bottlesCount / calculationSettings.PeriodInMonths
				: 0;

			if(bottlesPerMonthAverageCount <= calculationSettings.BottlesCountCClassificationTo)
			{
				return CounterpartyClassificationByBottlesCount.C;
			}

			if(bottlesPerMonthAverageCount >= calculationSettings.BottlesCountAClassificationFrom)
			{
				return CounterpartyClassificationByBottlesCount.A;
			}

			return CounterpartyClassificationByBottlesCount.B;
		}

		public static CounterpartyClassificationByOrdersCount GetClassificationByOrdersCount(
			decimal ordersCount,
			CounterpartyClassificationCalculationSettings calculationSettings)
		{
			var ordersPerMonthAverageCount =
				(calculationSettings.PeriodInMonths > 0)
				? ordersCount / calculationSettings.PeriodInMonths
				: 0;

			if(ordersPerMonthAverageCount <= calculationSettings.OrdersCountZClassificationTo)
			{
				return CounterpartyClassificationByOrdersCount.Z;
			}

			if(ordersPerMonthAverageCount >= calculationSettings.OrdersCountXClassificationFrom)
			{
				return CounterpartyClassificationByOrdersCount.X;
			}

			return CounterpartyClassificationByOrdersCount.Y;
		}

		public static CounterpartyClassificationByBottlesCount? ConvertToClassificationByBottlesCount(CounterpartyCompositeClassification classification)
		{
			switch(classification)
			{
				case CounterpartyCompositeClassification.AX:
				case CounterpartyCompositeClassification.AY:
				case CounterpartyCompositeClassification.AZ:
					return CounterpartyClassificationByBottlesCount.A;

				case CounterpartyCompositeClassification.BX:
				case CounterpartyCompositeClassification.BY:
				case CounterpartyCompositeClassification.BZ:
					return CounterpartyClassificationByBottlesCount.B;

				case CounterpartyCompositeClassification.CX:
				case CounterpartyCompositeClassification.CY:
				case CounterpartyCompositeClassification.CZ:
					return CounterpartyClassificationByBottlesCount.C;

				case CounterpartyCompositeClassification.New:
					return null;

				default:
					throw new ArgumentException("Неизвестное значение классификации контрагента");
			}
		}

		public static CounterpartyClassificationByOrdersCount? ConvertToClassificationByOrdersCount(CounterpartyCompositeClassification classification)
		{
			switch(classification)
			{
				case CounterpartyCompositeClassification.AX:
				case CounterpartyCompositeClassification.BX:
				case CounterpartyCompositeClassification.CX:
					return CounterpartyClassificationByOrdersCount.X;

				case CounterpartyCompositeClassification.AY:
				case CounterpartyCompositeClassification.BY:
				case CounterpartyCompositeClassification.CY:
					return CounterpartyClassificationByOrdersCount.Y;

				case CounterpartyCompositeClassification.AZ:
				case CounterpartyCompositeClassification.BZ:
				case CounterpartyCompositeClassification.CZ:
					return CounterpartyClassificationByOrdersCount.Z;

				case CounterpartyCompositeClassification.New:
					return null;

				default:
					throw new ArgumentException("Неизвестное значение классификации контрагента");
			}
		}
	}
}
