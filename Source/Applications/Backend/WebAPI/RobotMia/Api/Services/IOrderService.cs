using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors;
using Vodovoz.RobotMia.Contracts.Requests.V1;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Services
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
		Task<Result<int>> CreateAndAcceptOrderAsync(CreateOrderRequest createOrderRequest);

		/// <summary>
		/// Создание заказа без принятия
		/// </summary>
		/// <param name="createOrderRequest"></param>
		Task<Result<int>> CreateIncompleteOrderAsync(CreateOrderRequest createOrderRequest);

		/// <summary>
		/// Получение цены заказа, доставки и неустойки
		/// </summary>
		/// <param name="calculatePriceRequest"></param>
		/// <returns></returns>
		Result<CalculatePriceResponse> GetOrderAndDeliveryPrices(CalculatePriceRequest calculatePriceRequest);
	}
}
