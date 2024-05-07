using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	public class FuelLimit : PropertyChangedBase, IDomainObject
	{
		private string _limitId;
		private string _contractId;
		private string _serviceProductGroup;
		private string _serviceProductType;
		private decimal? _amount;
		private decimal? _sum;
		private FuelLimitUnit _unit;
		private int _transactionsCount;
		private int? _transactionsOccured;
		private int _period;
		private FuelLimitPeriodUnit _periodUnit;
		private FuelLimitTermType? _termType;
		private DateTime? _lastEditDate;
		private FuelLimitStatus _status;

		public virtual int Id { get; set; }

		[Display(Name = "Id лимита (выдается сервисом)")]
		public virtual string LimitId
		{
			get => _limitId;
			set => SetField(ref _limitId, value);
		}

		[Display(Name = "ID договора организации")]
		public virtual string ContractId
		{
			get => _contractId;
			set => SetField(ref _contractId, value);
		}

		[Display(Name = "ID группы продукта")]
		public virtual string ServiceProductGroup
		{
			get => _serviceProductGroup;
			set => SetField(ref _serviceProductGroup, value);
		}

		[Display(Name = "ID типа продукта")]
		public virtual string ServiceProductType
		{
			get => _serviceProductType;
			set => SetField(ref _serviceProductType, value);
		}

		[Display(Name = "Ограничение по количеству")]
		public virtual decimal? Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Ограничение по сумме")]
		public virtual decimal? Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		[Display(Name = "Единица измерения ограничения")]
		public FuelLimitUnit Unit
		{
			get => _unit;
			set => SetField(ref _unit, value);
		}

		[Display(Name = "Количество транзакций по услуге")]
		public virtual int TransctionsCount
		{
			get => _transactionsCount;
			set => SetField(ref _transactionsCount, value);
		}

		[Display(Name = "Количество проведенных транзакций по ограничению")]
		public virtual int? TransactionsOccured
		{
			get => _transactionsOccured;
			set => SetField(ref _transactionsOccured, value);
		}

		[Display(Name = "Ограничение по периоду использования (лительность ограничения)")]
		public virtual int Period
		{
			get => _period;
			set => SetField(ref _period, value);
		}

		[Display(Name = "Единица изменения длительности ограничения")]
		public virtual FuelLimitPeriodUnit PeriodUnit
		{
			get => _periodUnit;
			set => SetField(ref _periodUnit, value);
		}

		[Display(Name = "Ограничение по времени использования")]
		public virtual FuelLimitTermType? TermType
		{
			get => _termType;
			set => SetField(ref _termType, value);
		}

		[Display(Name = "Дата последнего изменения")]
		public virtual DateTime? LastEditDate
		{
			get => _lastEditDate;
			set => SetField(ref _lastEditDate, value);
		}

		[Display(Name = "Статус лимита")]
		public FuelLimitStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}
	}

	public enum FuelLimitStatus
	{
		[Display(Name = "Не указано")]
		None,
		[Display(Name = "Выдан успешно")]
		Success,
		[Display(Name = "Отменен")]
		Canceled,
		[Display(Name = "Ошибка на стороне сервиса при выдаче")]
		ServiceError,
		[Display(Name = "Ошибка на стороне ДВ при выдаче")]
		LocalError
	}

	public enum FuelLimitUnit
	{
		[Display(Name = "Литр")]
		Liter,
		[Display(Name = "Рубль")]
		Ruble
	}

	public enum FuelLimitPeriodUnit
	{
		[Display(Name = "Разовый")]
		OneTime = 2,
		[Display(Name = "Сутки")]
		Day = 3,
		[Display(Name = "Неделя")]
		Week = 4,
		[Display(Name = "Месяц")]
		Month = 5,
		[Display(Name = "Квартал")]
		Quarter = 6,
		[Display(Name = "Год")]
		Year = 7
	}

	public enum FuelLimitTermType
	{
		[Display(Name = "Все дни")]
		AllDays = 1,
		[Display(Name = "Рабочие дни")]
		WorkingDays = 2,
		[Display(Name = "Выходные дни")]
		DaysOff = 3,
	}
}
