using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "топливные лимиты",
		Nominative = "топливный лимит",
		Genitive = "топливного лимита",
		GenitivePlural = "топливных лимитов")]
	[HistoryTrace]
	public class FuelLimit : PropertyChangedBase, IDomainObject
	{
		private string _limitId;
		private string _cardId;
		private string _contractId;
		private string _productGroup;
		private string _productType;
		private decimal? _amount;
		private decimal? _usedAmount;
		private decimal? _sum;
		private decimal? _usedSum;
		private FuelLimitUnit _unit;
		private int _transactionsCount;
		private int? _transactionsOccured;
		private int _period;
		private FuelLimitPeriodUnit _periodUnit;
		private FuelLimitTermType? _termType;
		private DateTime _createDate;
		private DateTime? _lastEditDate;
		private FuelLimitStatus _status;

		public virtual int Id { get; set; }

		[Display(Name = "Id лимита в Газпромнефть (выдается сервисом)")]
		public virtual string LimitId
		{
			get => _limitId;
			set => SetField(ref _limitId, value);
		}

		[Display(Name = "Id карты на которую выдается лимит")]
		public virtual string CardId
		{
			get => _cardId;
			set => SetField(ref _cardId, value);
		}

		[Display(Name = "ID договора организации")]
		public virtual string ContractId
		{
			get => _contractId;
			set => SetField(ref _contractId, value);
		}

		[Display(Name = "ID группы продукта в Газпромнефть")]
		public virtual string ProductGroup
		{
			get => _productGroup;
			set => SetField(ref _productGroup, value);
		}

		[Display(Name = "ID типа продукта в Газпромнефть")]
		public virtual string ProductType
		{
			get => _productType;
			set => SetField(ref _productType, value);
		}

		[Display(Name = "Ограничение по количеству")]
		public virtual decimal? Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Использовано из ограничения по количеству")]
		public virtual decimal? UsedAmount
		{
			get => _usedAmount;
			set => SetField(ref _usedAmount, value);
		}

		[Display(Name = "Ограничение по сумме")]
		public virtual decimal? Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		[Display(Name = "Использовано из ограничения по сумме")]
		public virtual decimal? UsedSum
		{
			get => _usedSum;
			set => SetField(ref _usedSum, value);
		}

		[Display(Name = "Единица измерения ограничения")]
		public virtual FuelLimitUnit Unit
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

		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		[Display(Name = "Дата последнего изменения")]
		public virtual DateTime? LastEditDate
		{
			get => _lastEditDate;
			set => SetField(ref _lastEditDate, value);
		}

		[Display(Name = "Статус лимита")]
		public virtual FuelLimitStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		public virtual string Title => $"Топливный лимит на {Amount} л. №{Id} (Id={LimitId}) Id карты: {CardId}";
	}
}
