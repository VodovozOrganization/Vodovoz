using System;
using System.Linq;
using Vodovoz.RobotMia.Contracts.Requests.V1;
using ApiCreateOrderRequest = Vodovoz.RobotMia.Contracts.Requests.V1.CreateOrderRequest;
using CreateOrderRequest = VodovozBusiness.Services.Orders.CreateOrderRequest;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
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
			CounterpartyId = postOrderRequest.CounterpartyId, //Обязательное поле
			DeliveryPointId = postOrderRequest.DeliveryPointId, //Обязательное поле
			Date = postOrderRequest.DeliveryDate ?? DateTime.Today,
			DeliveryScheduleId = postOrderRequest.DeliveryIntervalId ?? default,
			PaymentType = postOrderRequest.PaymentType?.MapToPaymentType() ?? Domain.Client.PaymentType.Cash,
			PaymentByTerminalSource =
				postOrderRequest.PaymentType == PaymentType.TerminalQR
				? Vodovoz.Domain.Client.PaymentByTerminalSource.ByQR
				: postOrderRequest.PaymentType == Vodovoz.RobotMia.Contracts.Requests.V1.PaymentType.TerminalCard
					? Vodovoz.Domain.Client.PaymentByTerminalSource.ByCard
					: null,
			BanknoteForReturn = postOrderRequest.Trifle,
			BottlesReturn = postOrderRequest.BottlesReturn ?? default,
			SaleItems = postOrderRequest.SaleItems?.MapToSaleItem()
				?? Enumerable.Empty<CreateOrderRequest.SaleItem>(),
			TareNonReturnReasonId = postOrderRequest.TareNonReturnReasonId,
		};
	}
}
