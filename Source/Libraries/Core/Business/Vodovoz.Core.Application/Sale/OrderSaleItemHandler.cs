using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class OrderSaleItemHandler : SaleItemWithTaxHandler
	{
		private readonly IOrderDiscountsController _discountController;
		
		public OrderSaleItemHandler(
			IOrderDiscountsController discountController,
			ISaleItemTaxHandler saleItemTaxHandler
		) : base(discountController, saleItemTaxHandler)
		{
			_discountController = discountController;
		}
		
		internal virtual bool SetRentCount(IRecalculateRentCount saleItem, int rentCount)
		{
			if(saleItem.RentCount == rentCount)
			{
				return false;
			}

			saleItem.RentCount = rentCount;
			return true;
		}

		internal void CopyDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IApplyDiscountReasonItem copyingSaleItem)
		{
			_discountController.CopyDiscounts(uow, saleItem, copyingSaleItem);
			RecalculateTaxSum(saleItem as IRecalculateTax);
		}
		
		internal void CopyOriginalDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IPreserveDiscount copyingSaleItem)
		{
			_discountController.CopyOriginalDiscounts(uow, saleItem, copyingSaleItem);
			RecalculateTaxSum(saleItem as IRecalculateTax);
		}

		internal void RecalculateDiscountWithPreserveOrRestoreDiscount(IPreserveDiscount discountItem)
		{
			_discountController.RecalculateDiscountWithPreserveOrRestoreDiscount(discountItem);
			RecalculateTaxSum(discountItem as IRecalculateTax);
		}

		internal void TryRestoreOriginalDiscount(IPreserveDiscount discountItem)
		{
			_discountController.TryRestoreOriginalDiscount(discountItem);
		}
	}
}
