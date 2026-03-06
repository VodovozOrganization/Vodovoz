using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Скидки на товары автозаказов",
		Nominative = "Скидка на товар автозаказа",
		Prepositional = "Скидке на товар автозаказа",
		PrepositionalPlural = "Скидках на товары автозаказов"
	)]
	[HistoryTrace]
	public class OnlineOrderTemplateProductDiscount : PropertyChangedBase, IDomainObject
	{
		private decimal _percentDiscount;
		private decimal _moneyDiscount;
		private bool _isDiscountInMoney;
		private DiscountReason _discountReason;
		private OnlineOrderTemplateProduct _templateProduct;
		
		public virtual int Id { get; set; }
		
		/// <summary>
		/// Товар из автозаказа
		/// </summary>
		[Display(Name = "Товар из автозаказа")]
		public virtual OnlineOrderTemplateProduct TemplateProduct
		{
			get => _templateProduct;
			set => SetField(ref _templateProduct, value);
		}
		
		/// <summary>
		/// Скидка
		/// </summary>
		[Display(Name = "Скидка в процентах")]
		public virtual decimal PercentDiscount
		{
			get => _percentDiscount;
			set => SetField(ref _percentDiscount, value);
		}
		
		/// <summary>
		/// Скидка
		/// </summary>
		[Display(Name = "Скидка в деньгах")]
		public virtual decimal MoneyDiscount
		{
			get => _moneyDiscount;
			set => SetField(ref _moneyDiscount, value);
		}
		
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		[Display(Name = "Скидка в деньгах")]
		public virtual bool IsDiscountInMoney
		{
			get => _isDiscountInMoney;
			set => SetField(ref _isDiscountInMoney, value);
		}
		
		/// <summary>
		/// Основание скидки на товар
		/// </summary>
		[Display(Name = "Основание скидки на товар")]
		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}
		
		protected virtual void CalculateDiscount(
			decimal price,
			decimal count,
			decimal discount)
		{
			if(price * count == 0)
			{
				MoneyDiscount = 0;
				PercentDiscount = 0;
				return;
			}
			
			if(IsDiscountInMoney)
			{
				MoneyDiscount = discount > price * count ? price * count : (discount < 0 ? 0 : discount);
				PercentDiscount = (100 * MoneyDiscount) / (price * count);
			}
			else
			{
				PercentDiscount = discount > 100 ? 100 : (discount < 0 ? 0 : discount);
				MoneyDiscount = price * count * PercentDiscount / 100;
			}
		}

		public static OnlineOrderTemplateProductDiscount Create(
			OnlineOrderTemplateProduct templateProduct,
			decimal count,
			decimal price,
			decimal discount,
			bool isDiscountInMoney,
			DiscountReason discountReason
		)
		{
			var productDiscount = new OnlineOrderTemplateProductDiscount
			{
				TemplateProduct = templateProduct,
				IsDiscountInMoney = isDiscountInMoney,
				DiscountReason = discountReason
			};

			productDiscount.CalculateDiscount(price, count, discount);

			return productDiscount;
		}
	}
}
