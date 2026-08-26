using Vodovoz.Core.Domain.Interfaces;

namespace VodovozBusiness.Domain.Orders
{
	/// <inheritdoc/>
	public class DiscountValue : IDiscountValue
	{
		//Для Nhibernate, т.к. он используется как компонент в PersonalDiscountMap
		public DiscountValue() { }
		
		private DiscountValue(bool isDiscountMoney, decimal discount, decimal discountMoney)
		{
			IsDiscountMoney = isDiscountMoney;
			Discount = discount;
			DiscountMoney = discountMoney;
		}
		
		/// <inheritdoc/>
		public virtual bool IsDiscountMoney { get; protected set; }
		/// <inheritdoc/>
		public virtual decimal Discount { get; set; }
		/// <inheritdoc/>
		public virtual decimal DiscountMoney { get; protected set; }
		
		public virtual bool IsZeroDiscount => IsDiscountMoney
			? DiscountMoney <= 0
			: Discount <= 0;

		public virtual decimal GetDiscount => IsDiscountMoney
			? DiscountMoney
			: Discount;

		public virtual void AddDiscountValue(IDiscountValue discountValue)
		{
			if(discountValue.IsDiscountMoney)
			{
				IsDiscountMoney = discountValue.IsDiscountMoney;
			}
			
			Discount += discountValue.Discount;
			DiscountMoney += discountValue.DiscountMoney;
		}
		
		public virtual void SetDiscount(decimal discount, bool isDiscountMoney)
		{
			if(isDiscountMoney)
			{
				Discount = discount;
			}
			else
			{
				DiscountMoney = discount;
			}
		}

		public static IDiscountValue Create(bool isDiscountMoney, decimal discount, decimal discountMoney) =>
			new DiscountValue(isDiscountMoney, discount, discountMoney);

		public static IDiscountValue CreateZero(bool isDiscountMoney = false) =>
			new DiscountValue(false, 0, 0);
	}
}
