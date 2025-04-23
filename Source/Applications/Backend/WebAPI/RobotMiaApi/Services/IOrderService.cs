using RobotMiaApi.Contracts.Requests.V1;
using RobotMiaApi.Contracts.Responses.V1;
using System.Threading.Tasks;

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

		/// <summary>
		/// Создание и принятие заказа
		/// </summary>
		/// <param name="createOrderRequest"></param>
		/// <returns></returns>
		Task<int> CreateAndAcceptOrder(CreateOrderRequest createOrderRequest);

		/// <summary>
		/// Создание заказа без принятия
		/// </summary>
		/// <param name="createOrderRequest"></param>
		void CreateIncompleteOrder(CreateOrderRequest createOrderRequest);

		/// <summary>
		/// Получение цены заказа, доставки и неустойки
		/// </summary>
		/// <param name="calculatePriceRequest"></param>
		/// <returns></returns>
		(decimal orderPrice, decimal deliveryPrice, decimal forfeitPrice) GetOrderAndDeliveryPrices(CalculatePriceRequest calculatePriceRequest);
	}
}
