using ApiCreateOrderRequest = RobotMiaApi.Contracts.Requests.V1.CreateOrderRequest;
using CreateOrderRequest = VodovozBusiness.Services.Orders.CreateOrderRequest;

namespace RobotMiaApi.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="ApiCreateOrderRequest"/>
	/// </summary>
	public static class PostOrderRequestExtensions
	{
		/// <summary>
		/// Маппинг запроса на создание заказа в <see cref="CreateOrderRequest.SaleItem"/>
		/// </summary>
		/// <param name="postOrderRequest"></param>
		/// <returns></returns>
		public static CreateOrderRequest MapToCreateOrderRequest(this ApiCreateOrderRequest postOrderRequest) => new CreateOrderRequest
		{
			CounterpartyId = postOrderRequest.CounterpartyId,
			DeliveryPointId = postOrderRequest.DeliveryPointId,
			Date = postOrderRequest.DeliveryDate,
			DeliveryScheduleId = postOrderRequest.DeliveryIntervalId,
			PaymentType = postOrderRequest.PaymentType.MapToPaymentType(),
			PaymentByTerminalSource =
				postOrderRequest.PaymentType == Contracts.Requests.V1.PaymentType.TerminalQR
				? Vodovoz.Domain.Client.PaymentByTerminalSource.ByQR
				: postOrderRequest.PaymentType == Contracts.Requests.V1.PaymentType.TerminalCard
					? Vodovoz.Domain.Client.PaymentByTerminalSource.ByCard
					: null,
			BanknoteForReturn = postOrderRequest.Trifle,
			BottlesReturn = postOrderRequest.BottlesReturn,
			SaleItems = postOrderRequest.SaleItems.MapToSaleItem()
		};
	}
}
