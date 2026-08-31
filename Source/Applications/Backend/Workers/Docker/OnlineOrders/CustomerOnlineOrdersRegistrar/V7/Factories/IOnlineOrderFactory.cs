using CustomerOrdersApi.Library.V7.Dto.Orders;
using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders;

namespace CustomerOnlineOrdersRegistrar.V7.Factories
{
	public interface IOnlineOrderFactory
	{
		/// <summary>
		/// Создание онлайн заказа на основе данных из ИПЗ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="creatingOnlineOrder">Информация для создания онлайн заказа из ИПЗ</param>
		/// <param name="fastDeliveryScheduleId">Id графика доставки</param>
		/// <returns>Онлайн заказ</returns>
		OnlineOrderV2 CreateOnlineOrder(
			IUnitOfWork uow, ICreatingOnlineOrder creatingOnlineOrder, int fastDeliveryScheduleId);
	}
}
