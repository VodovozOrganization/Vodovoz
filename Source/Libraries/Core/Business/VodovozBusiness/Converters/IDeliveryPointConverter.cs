using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;

namespace Vodovoz.Converters
{
	public interface IDeliveryPointConverter
	{
		/// <summary>
		/// Конвертация точки доставки <see cref="DeliveryPoint"/> в информацию о ней для ЭДО <see cref="DeliveryPointInfoForEdo"/>
		/// </summary>
		/// <param name="deliveryPoint">Конвертируемая ТД</param>
		/// <returns>Информация о ТД для ЭДО</returns>
		DeliveryPointInfoForEdo ConvertDeliveryPointToDeliveryPointInfoForEdo(DeliveryPoint deliveryPoint);
	}
}
