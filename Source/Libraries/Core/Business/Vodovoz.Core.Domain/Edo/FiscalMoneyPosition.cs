using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Позиция оплаты в фискальном документе
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "оплата в фискальном документе",
		NominativePlural = "оплата в фискальном документе"
	)]
	public class FiscalMoneyPosition : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private FiscalPaymentType _paymentType;
		private decimal _sum;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Тип оплаты
		/// </summary>
		[Display(Name = "Тип оплаты")]
		public virtual FiscalPaymentType PaymentType
		{
			get => _paymentType;
			set => SetField(ref _paymentType, value);
		}

		/// <summary>
		/// Сумма
		/// </summary>
		[Display(Name = "Сумма")]
		public virtual decimal Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}
	}
}
