using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Handlers
{
	public interface IOrderProductHandler : IProductHandler
	{
		void RemoveEquipment(OrderEquipment item);

		/// <summary>
		/// Добавить товары из выбранного предыдущего заказа.
		/// </summary>
		/// <param name="addingItem">Элемент заказа.</param>
		void AddNomenclatureForSaleFromPreviousOrder(IProduct addingItem);

		void AddFastDeliveryNomenclatureIfNeeded();

		void UpdateDeliveryItem(
			Nomenclature nomenclature,
			decimal price);

		/// <summary>
		/// Добавление/удаление номенклатуры для вызова мастера в зависимости от типа адреса
		/// </summary>
		void UpdateMasterCallNomenclatureIfNeeded();
		void RemoveItemFromClosingOrder(IProduct removableItem);
		void RemoveFastDeliveryNomenclature();
	}
}
