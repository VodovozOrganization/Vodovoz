using System;
using DeliveryRulesService.V2.DTO;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Cart;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders.Cart;

namespace DeliveryRulesService.Factories
{
	/// <inheritdoc/>
	public class CartItemFactory : ICartItemFactory
	{
		/// <inheritdoc/>
		public ICartItem CreateCartItem(IUnitOfWork uow, SaleItemDto saleItem)
		{
			switch(saleItem.Type)
			{
				case SaleItemType.Water:
				case SaleItemType.Service:
				case SaleItemType.Equipment:
				case SaleItemType.Other:
					return NomenclatureCartItem(uow, saleItem);
				case SaleItemType.PromoSet:
					return PromoSetCartItem(uow, saleItem);
				case SaleItemType.RentPackage:
					return FreeRentPackageCartItem(uow, saleItem);
				default:
					throw new ArgumentOutOfRangeException(nameof(saleItem.Type), "Пришло неизвестное значение типа товара корзины");
			}
		}
		
		/// <inheritdoc/>
		public ICartItem FreeRentPackageCartItem(IUnitOfWork uow, SaleItemDto saleItem)
		{
			ThrowIfErpIdIsNull(saleItem, "Пакет аренды");

			return new FreeRentPackageCartItem
			{
				Count = saleItem.Amount,
				RentPackage = uow.GetById<FreeRentPackage>(saleItem.ErpId.Value)
					?? throw new ArgumentNullException(nameof(saleItem), "Пришел пакет аренды с неизвестным значением id")
			};
		}

		/// <inheritdoc/>
		public ICartItem NomenclatureCartItem(IUnitOfWork uow, SaleItemDto saleItem)
		{
			ThrowIfErpIdIsNull(saleItem, "Номенклатура");
			
			return new NomenclatureCartItem
			{
				Count = saleItem.Amount,
				Nomenclature = uow.GetById<Nomenclature>(saleItem.ErpId.Value)
					?? throw new ArgumentNullException(nameof(saleItem), "Пришла номенклатура с неизвестным значением id")
			};
		}
		
		/// <inheritdoc/>
		public ICartItem PromoSetCartItem(IUnitOfWork uow, SaleItemDto saleItem)
		{
			ThrowIfErpIdIsNull(saleItem, "Промонабор");

			return new PromoSetCartItem
			{
				Count = saleItem.Amount,
				PromoSet = uow.GetById<PromotionalSet>(saleItem.ErpId.Value)
					?? throw new ArgumentNullException(nameof(saleItem), "Пришел промонабор с неизвестным значением id")
			};
		}
		
		private static void ThrowIfErpIdIsNull(SaleItemDto saleItem, string itemType)
		{
			if(!saleItem.ErpId.HasValue)
			{
				throw new ArgumentNullException(nameof(saleItem), $"{itemType} с пустым значением id");	
			}
		}
	}
}
