using DeliveryRulesService.V2.DTO;
using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders.Cart;

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
		ICartItem CreateCartItem(IUnitOfWork uow, SaleItemDto saleItem);
		/// <summary>
		/// Создание пакета аренды как позиции корзины
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция заказа</param>
		/// <returns></returns>
		ICartItem FreeRentPackageCartItem(IUnitOfWork uow, SaleItemDto saleItem);
		/// <summary>
		/// Создание номенклатурной позиции корзины
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция заказа</param>
		/// <returns></returns>
		ICartItem NomenclatureCartItem(IUnitOfWork uow, SaleItemDto saleItem);
		/// <summary>
		/// Создание промонабора как позиции корзины
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция заказа</param>
		/// <returns></returns>
		ICartItem PromoSetCartItem(IUnitOfWork uow, SaleItemDto saleItem);
	}
}
