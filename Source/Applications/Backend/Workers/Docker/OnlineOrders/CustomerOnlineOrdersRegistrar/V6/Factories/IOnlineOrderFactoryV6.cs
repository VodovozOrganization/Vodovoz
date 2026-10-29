using CustomerOrdersApi.Library.V6.Dto.Orders;
using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V6.Factories
{
	public interface IOnlineOrderFactoryV6
	{
		/// <summary>
		/// Создание онлайн заказа на основе данных из ИПЗ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="creatingOnlineOrder">Информация для создания онлайн заказа из ИПЗ</param>
		/// <param name="fastDeliveryScheduleId">Id графика доставки</param>
		/// <param name="selfDeliveryDiscountReasonId">Id основания скидки на самовывоз</param>
		/// <returns>Онлайн заказ</returns>
		OnlineOrderV1 CreateOnlineOrder(
			IUnitOfWork uow, ICreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId, int selfDeliveryDiscountReasonId);
	}
}
