using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public class OnlineOrderTemplateSaleItemFactory : ISaleItemFactory
	{
		public IProduct Create(
			object source,
			decimal count,
			decimal price,
			Nomenclature nomenclature
		)
		{
			return OnlineOrderTemplateProduct.Create(
				(source as OnlineOrderTemplate).Id, //templateId,
				count,
				price,
				nomenclature,
				null,//proSet,
				new ObservableList<OnlineOrderTemplateProductDiscount>()
			);
		}
		
		public IProduct CreateDeliveryOrderItem(
			object source,
			Nomenclature nomenclature,
			decimal price
			)
		{
			return OnlineOrderTemplateProduct.Create(
				(source as OnlineOrderTemplate).Id, //templateId,
				1,
				price,
				nomenclature,
				null,//proSet,
				new ObservableList<OnlineOrderTemplateProductDiscount>()
			);
		}

		public IProduct CreateNewNonFreeRentDepositItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OnlineOrderTemplateProduct.CreateNewNonFreeRentDepositItem(
				(source as OnlineOrderTemplate).Id, //templateId,
				paidRentPackage
			);
		}

		public IProduct CreateNewNonFreeRentServiceItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OnlineOrderTemplateProduct.CreateNewNonFreeRentServiceItem(
				(source as OnlineOrderTemplate).Id, //templateId,
				paidRentPackage
			);
		}

		public IProduct CreateNewDailyRentDepositItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OnlineOrderTemplateProduct.CreateNewDailyRentDepositItem(
				(source as OnlineOrderTemplate).Id, //templateId,
				paidRentPackage
			);
		}

		public IProduct CreateNewDailyRentServiceItem(
			object source,
			PaidRentPackage paidRentPackage
			)
		{
			return OnlineOrderTemplateProduct.CreateNewDailyRentServiceItem(
				(source as OnlineOrderTemplate).Id, //templateId,
				paidRentPackage
			);
		}

		public IProduct CreateNewFreeRentDepositItem(
			object source,
			FreeRentPackage freeRentPackage
			)
		{
			return OnlineOrderTemplateProduct.CreateNewFreeRentDepositItem(
				(source as OnlineOrderTemplate).Id, //templateId,
				freeRentPackage
			);
		}
	}
}
