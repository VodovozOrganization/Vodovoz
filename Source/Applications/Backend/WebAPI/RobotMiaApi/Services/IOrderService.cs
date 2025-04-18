using RobotMiaApi.Contracts.Responses.V1;

namespace RobotMiaApi.Services
{
	/// <summary>
	/// Сервис заказов Api робота Мия
	/// </summary>
	public interface IOrderService
	{
		/// <summary>
		/// Получение последнего заказа контрагента
		/// </summary>
		/// <param name="counterpartyId"></param>
		/// <returns></returns>
		LastOrderResponse GetLastOrderByCounterpartyId(int counterpartyId);

		/// <summary>
		/// Получение последнего заказа точки доставки
		/// </summary>
		/// <param name="deliveryPointId"></param>
		/// <returns></returns>
		LastOrderResponse GetLastOrderByDeliveryPointId(int deliveryPointId);
	}
}
