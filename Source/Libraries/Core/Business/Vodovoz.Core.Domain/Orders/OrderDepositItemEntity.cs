using QS.DomainModel.Entity;
using QS.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Core.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "залоги в заказе",
		Nominative = "залог в заказе"
	)]
	public class OrderDepositItemEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderEntity _order;
		private int _count;
		private int? _actualCount;
		private DepositType _depositType;
		private decimal _deposit;


		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Количество")]
		public virtual int Count
		{
			get => _count;
			set => SetField(ref _count, value);
		}

		[Display(Name = "Фактическое количество")]
		public virtual int? ActualCount
		{
			get => _actualCount;
			set => SetField(ref _actualCount, value);
		}

		[Display(Name = "Тип залога")]
		public virtual DepositType DepositType
		{
			get => _depositType;
			set => SetField(ref _depositType, value);
		}

		[Display(Name = "Залог")]
		public virtual decimal Deposit
		{
			get => _deposit;
			set => SetField(ref _deposit, value);
		}

		/// <summary>
		/// Свойство возвращает подходяшее значение Count или ActualCount в зависимости от статуса заказа.
		/// </summary>
		public int CurrentCount => ActualCount ?? Count;

		public virtual decimal ActualSum => Math.Round(Deposit * CurrentCount, 2);

		public string Title => string.Format("{0} на сумму {1}", DepositTypeString, CurrencyWorks.GetShortCurrencyString(ActualSum));

		public virtual string DepositTypeString
		{
			get
			{
				switch(DepositType)
				{
					case DepositType.Bottles:
						return "Возврат залога за бутыли";
					case DepositType.Equipment:
						return "Возврат залога за оборудование";
					default:
						return "Не определено";
				}
			}
		}
	}
}
