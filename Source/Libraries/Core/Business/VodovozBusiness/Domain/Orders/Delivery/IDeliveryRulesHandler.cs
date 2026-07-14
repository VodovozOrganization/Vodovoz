using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Domain.Orders.Cart;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	public interface IDeliveryRulesHandler
	{
		/// <summary>
		/// Получение данных о стоимости доставки и сообщения по получению бесплатной доставки
		/// </summary>
		/// <param name="context">Данные для расчета</param>
		/// <returns></returns>
		Result<(decimal? DeliveryPrice, string Message)> GetDeliveryCost(IDeliveryRulesRequestContext context);
	}
}
