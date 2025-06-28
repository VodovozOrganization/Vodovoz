using Vodovoz.RobotMia.Contracts.Requests.V1;
using CreateOrderRequest = VodovozBusiness.Services.Orders.CreateOrderRequest;
using PaymentType = Vodovoz.Domain.Client.PaymentType;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="CalculatePriceRequest"/>
	/// </summary>
	public static class CalculatePriceRequestExtensions
	{
		/// <summary>
		/// Маппинг запроса цены заказа в <see cref="CreateOrderRequest"/>
		/// </summary>
		/// <param name="calculatePriceRequest"></param>
		/// <returns></returns>
		public static CreateOrderRequest MapToCreateOrderRequest(this CalculatePriceRequest calculatePriceRequest)
			=> new CreateOrderRequest
			{
				CounterpartyId = calculatePriceRequest.CounterpartyId,
				DeliveryPointId = calculatePriceRequest.DeliveryPointId,
				Date = calculatePriceRequest.DeliveryDate,
				DeliveryScheduleId = calculatePriceRequest.DeliveryIntervalId,
				PaymentType = PaymentType.Cash,
				BanknoteForReturn = 0,
				BottlesReturn = calculatePriceRequest.BottlesReturn,
				SaleItems = calculatePriceRequest.OrderSaleItems.MapToCreateOrderRequestSaleItem()
			};
	}
}
