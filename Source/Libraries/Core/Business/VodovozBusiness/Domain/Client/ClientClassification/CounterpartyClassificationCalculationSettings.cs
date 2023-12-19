using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client.ClientClassification
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "параметры расчета классификаций контрагентов",
		Nominative = "параметры расчета классификации контрагента")]
	[HistoryTrace]
	public class CounterpartyClassificationCalculationSettings : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _periodInMonths;
		private int _bottlesCountAClassificationFrom;
		private int _bottlesCountCClassificationTo;
		private int _ordersCountXClassificationFrom;
		private int _ordersCountZClassificationTo;
		private DateTime _settingsCreationDate;

		public virtual int Id { get; }

		public virtual string Title => "Параметры расчета классификации контрагента";

		[Display(Name = "Значение периода в месяцах для выполнения расчета")]
		public virtual int PeriodInMonths
		{
			get => _periodInMonths;
			set => SetField(ref _periodInMonths, value);
		}

		//Если у контрагента среднее кол-во бутылей в месяц больше BottlesCountCClassificationTo, но меньше BottlesCountAClassificationFrom,
		//то контрагенту присваивается категория 'B'

		[Display(Name = "Среднее кол-во бутылей в месяц, начиная от которого присваивается категория 'A'")]
		[PropertyChangedAlso(nameof(BottlesCountClassificationSettingsSummary))]
		public virtual int BottlesCountAClassificationFrom
		{
			get => _bottlesCountAClassificationFrom;
			set => SetField(ref _bottlesCountAClassificationFrom, value);
		}

		[Display(Name = "Среднее кол-во бутылей в месяц, до которого присваивается категория 'C'")]
		[PropertyChangedAlso(nameof(BottlesCountClassificationSettingsSummary))]
		public virtual int BottlesCountCClassificationTo
		{
			get => _bottlesCountCClassificationTo;
			set => SetField(ref _bottlesCountCClassificationTo, value);
		}

		//Если у контрагента среднее кол-во заказов в месяц больше OrdersCountZClassificationTo, но меньше OrdersCountXClassificationFrom,
		//то контрагенту присваивается категория 'Y'

		[Display(Name = "Среднее кол-во заказов в месяц, начиная от которого присваивается категория 'X'")]
		[PropertyChangedAlso(nameof(OrdersCountClassificationSettingsSummary))]
		public virtual int OrdersCountXClassificationFrom
		{
			get => _ordersCountXClassificationFrom;
			set => SetField(ref _ordersCountXClassificationFrom, value);
		}

		[Display(Name = "Среднее кол-во заказов в месяц, до которого присваивается категория 'Z'")]
		[PropertyChangedAlso(nameof(OrdersCountClassificationSettingsSummary))]
		public virtual int OrdersCountZClassificationTo
		{
			get => _ordersCountZClassificationTo;
			set => SetField(ref _ordersCountZClassificationTo, value);
		}

		[Display(Name = "Дата создания настройки параметров для расчета классификации контрагентов")]
		public virtual DateTime SettingsCreationDate
		{
			get => _settingsCreationDate;
			set => SetField(ref _settingsCreationDate, value);
		}

		public virtual string BottlesCountClassificationSettingsSummary =>
			$"A: От {BottlesCountAClassificationFrom} и более\n" +
			$"B: От {BottlesCountCClassificationTo + 0.01} до {BottlesCountAClassificationFrom - 0.01}\n" +
			$"C: От 0 до {BottlesCountCClassificationTo}";

		public virtual string OrdersCountClassificationSettingsSummary =>
			$"X: От {OrdersCountXClassificationFrom} и более\n" +
			$"Y: От {OrdersCountZClassificationTo + 0.01} до {OrdersCountXClassificationFrom - 0.01}\n" +
			$"Z: От 0 до {OrdersCountZClassificationTo}";

		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(PeriodInMonths < 1)
			{
				yield return new ValidationResult(string.Format("Введенное значение периода не должно быть меньше 1"));
			}

			if(BottlesCountAClassificationFrom < 0
				|| BottlesCountCClassificationTo < 0
				|| OrdersCountXClassificationFrom < 0
				|| OrdersCountZClassificationTo < 0)
			{
				yield return new ValidationResult(string.Format("Введенные значения среднего кол-ва бутылей и среднего кол-ва заказов в месяц " +
					"не должно быть меньше 0"));
			}

			if(BottlesCountAClassificationFrom <= BottlesCountCClassificationTo)
			{
				yield return new ValidationResult(string.Format("Значение среднего кол-ва бутылей в месяц для присвоения классификации 'A' " +
					"должно быть больше среднего значения для присвоения классификации 'С'"));
			}

			if(OrdersCountXClassificationFrom <= OrdersCountZClassificationTo)
			{
				yield return new ValidationResult(string.Format("Значение среднего кол-ва заказов в месяц для присвоения классификации 'X' " +
					"должно быть больше среднего значения для присвоения классификации 'Z'"));
			}
		}
		#endregion
	}
}
