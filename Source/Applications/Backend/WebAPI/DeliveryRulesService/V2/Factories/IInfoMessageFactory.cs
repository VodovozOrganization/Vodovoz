using Vodovoz.Core.Data.InfoMessages;
using VodovozBusiness.Domain.Orders.Delivery;

namespace DeliveryRulesService.Factories
{
	/// <summary>
	/// Фабрика создания информационных сообщений
	/// </summary>
	public interface IInfoMessageFactory
	{
		/// <summary>
		/// Создание информационного сообщения по доставке
		/// </summary>
		/// <param name="deliveryCostData">Данные по доставке</param>
		/// <returns></returns>
		InfoMessage CreateDeliveryMessage(IDeliveryCostData deliveryCostData);
	}
}
