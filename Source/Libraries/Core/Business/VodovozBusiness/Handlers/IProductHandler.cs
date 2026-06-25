using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Handlers
{
	public interface IProductHandler
	{
		void Initialize(
			IUnitOfWork uow,
			IAddSaleItemSource saleItemSource);
		
		Result TryAddProduct(
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			IEnumerable<DiscountReason> discountReasons = null
		);

		void AddSaleItem(
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			bool discountInMoney = false,
			bool needGetFixedPrice = true,
			IEnumerable<DiscountReason> discountReasons = null,
			PromotionalSet proSet = null
		);

		void AddWaterForSale(
			Nomenclature nomenclature,
			decimal count,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			bool needGetFixedPrice = true,
			DiscountReason reason = null,
			PromotionalSet proSet = null
		);

		void AddMasterNomenclature(
			decimal count,
			Nomenclature nomenclature,
			int quantityOfFollowingNomenclatures = 0);

		void AddAnyGoodsNomenclatureForSale(
			Nomenclature nomenclature,
			bool isChangeOrder = false,
			int? cnt = null);

		void AddSaleItem(
			IProduct addingItem,
			bool forceUseAlternativePrice = false);
		
		void AddNonFreeRent(
			PaidRentPackage paidRentPackage,
			Nomenclature equipmentNomenclature
		);

		void AddDailyRent(
			PaidRentPackage paidRentPackage,
			Nomenclature equipmentNomenclature
		);

		void AddFreeRent(
			FreeRentPackage freeRentPackage,
			Nomenclature equipmentNomenclature
		);
		
		void RemoveSaleItem(IProduct removableProduct);
		void RemoveItem(IProduct removableItem);
	}
}
