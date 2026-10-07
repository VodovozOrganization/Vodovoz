using CustomerApp.Contracts.Sale;
using DeliveryRulesService.V2.DTO;
using QS.DomainModel.UoW;

namespace DeliveryRulesService.Factories
{
	/// <summary>
	/// Фабрика для создания позиций корзины
	/// </summary>
	public interface ICartItemFactory
	{
		/// <summary>
		/// Создание конкретной позиции корзины
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция заказа</param>
		/// <returns></returns>
		ICartItemBase CreateCartItem(IUnitOfWork uow, SaleItemDto saleItem);
		/// <summary>
		/// Создание пакета аренды как позиции корзины
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция заказа</param>
		/// <returns></returns>
		ICartItemBase FreeRentPackageCartItem(IUnitOfWork uow, SaleItemDto saleItem);
		/// <summary>
		/// Создание номенклатурной позиции корзины
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция заказа</param>
		/// <returns></returns>
		ICartItemBase NomenclatureCartItem(IUnitOfWork uow, SaleItemDto saleItem);
		/// <summary>
		/// Создание промонабора как позиции корзины
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция заказа</param>
		/// <returns></returns>
		ICartItemBase PromoSetCartItem(IUnitOfWork uow, SaleItemDto saleItem);
	}
}
