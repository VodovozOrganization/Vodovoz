using System.Collections.Generic;
using System.Linq;
using Vodovoz.RobotMia.Contracts.Requests.V1;
using CreateOrderRequest = VodovozBusiness.Services.Orders.CreateOrderRequest;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="SaleItem"/>
	/// </summary>
	public static class SaleItemExtensions
	{
		/// <summary>
		/// Маппинг строки заказа в <see cref="CreateOrderRequest.SaleItem"/>
		/// </summary>
		/// <param name="orderSaleItem"></param>
		/// <returns></returns>
		public static CreateOrderRequest.SaleItem MapToSaleItem(this SaleItem orderSaleItem)
			=> new CreateOrderRequest.SaleItem(orderSaleItem.NomenclatureId, (int)orderSaleItem.Count);

		/// <summary>
		/// Маппинг строк заказа в IEnumerable <see cref="CreateOrderRequest.SaleItem"/>
		/// </summary>
		/// <param name="orderSaleItems"></param>
		/// <returns></returns>
		public static IEnumerable<CreateOrderRequest.SaleItem> MapToSaleItem(this IEnumerable<SaleItem> orderSaleItems)
			=> orderSaleItems.Select(MapToSaleItem);
	}
}
