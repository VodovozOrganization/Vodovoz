using Vodovoz.Core.Application.Common;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class OrderRecalculateDiscount : CommonRecalculateDiscount
	{
		protected OrderRecalculateDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue discountValue, bool orderInUndeliveredStatus)
			: base(saleItem, discountValue)
		{
			OrderInUndeliveredStatus = orderInUndeliveredStatus;
		}

		public new IPreserveDiscount SaleItem => base.SaleItem as IPreserveDiscount;
		public bool OrderInUndeliveredStatus { get; }
		
		public static IDataContext CreateDataContext(IPreserveDiscount saleItem, IDiscountValue discountValue, bool orderInUndeliveredStatus) =>
			DataContext.Create(Create(saleItem, discountValue, orderInUndeliveredStatus));
		
		public static IDataContext CreateDataContext(IPreserveDiscount saleItem, bool orderInUndeliveredStatus) =>
			DataContext.Create(Create(saleItem, saleItem.DiscountData, orderInUndeliveredStatus));
		
		public static OrderRecalculateDiscount Create(IPreserveDiscount saleItem, IDiscountValue discountValue, bool orderInUndeliveredStatus) =>
			new OrderRecalculateDiscount(saleItem, discountValue, orderInUndeliveredStatus);
	}
}
