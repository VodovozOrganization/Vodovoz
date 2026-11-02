using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public class DiscountApplicability : PropertyChangedBase, IDomainObject
	{
		private DiscountType _discountType;
		private UseDiscountType _useDiscountType;
		private DiscountReasonBase _discountReason;
		
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Тип скидки
		/// </summary>
		public virtual DiscountType DiscountType
		{
			get => _discountType;
			set => SetField(ref _discountType, value);
		}

		/// <summary>
		/// Тип применения скидки
		/// </summary>
		public virtual UseDiscountType UseDiscountType
		{
			get => _useDiscountType;
			set => SetField(ref _useDiscountType, value);
		}

		/// <summary>
		/// Основание скидки
		/// </summary>
		public virtual DiscountReasonBase DiscountReason
		{
			get => _discountReason;
			protected set => SetField(ref _discountReason, value);
		}

		public static DiscountApplicability Create(DiscountType discountType, UseDiscountType useDiscountType, DiscountReasonBase discountReason) =>
			new DiscountApplicability
			{
				DiscountType = discountType,
				UseDiscountType = useDiscountType,
				DiscountReason = discountReason
			};
	}
}
