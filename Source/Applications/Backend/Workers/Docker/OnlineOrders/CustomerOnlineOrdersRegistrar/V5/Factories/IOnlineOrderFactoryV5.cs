using CustomerOrders.Contracts.V5.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V5.Factories
{
	/// <summary>
	/// Фабрика для создания онлайн заказа из данных ИПЗ
	/// </summary>
	public interface IOnlineOrderFactoryV5
	{
		/// <summary>
		/// Создание сущности онлайн заказ из данных, пришедших с ИПЗ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="creatingOnlineOrder">Данные из ИПЗ</param>
		/// <param name="fastDeliveryScheduleId">Идентификатор быстрой доставки(доставка за час)</param>
		/// <param name="selfDeliveryDiscountReasonId">Идентификатор основания скидки за самовывоз</param>
		/// <returns></returns>
		OnlineOrder CreateOnlineOrder(
			IUnitOfWork uow, ICreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
	}
}
