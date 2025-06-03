using System.Collections.Generic;
using System.Linq;
using Vodovoz.RobotMia.Contracts.Requests.V1;
using CreateOrderRequest = VodovozBusiness.Services.Orders.CreateOrderRequest;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="OrderSaleItemExtensions"/>
	/// </summary>
	public static class OrderSaleItemExtensions
	{
		/// <summary>
		/// Маппинг строки заказа в <see cref="CreateOrderRequest.SaleItem"/>
		/// </summary>
		/// <param name="orderSaleItem"></param>
		/// <returns></returns>
		public static CreateOrderRequest.SaleItem MapToCreateOrderRequestSaleItem(this SaleItem orderSaleItem)
			=> new CreateOrderRequest.SaleItem(orderSaleItem.NomenclatureId, (int)orderSaleItem.Count);

		/// <summary>
		/// Маппинг строк заказа в IEnumerable <see cref="CreateOrderRequest.SaleItem"/>
		/// </summary>
		/// <param name="orderSaleItems"></param>
		/// <returns></returns>
		public static IEnumerable<CreateOrderRequest.SaleItem> MapToCreateOrderRequestSaleItem(this IEnumerable<SaleItem> orderSaleItems)
			=> orderSaleItems.Select(MapToCreateOrderRequestSaleItem);
	}
}
