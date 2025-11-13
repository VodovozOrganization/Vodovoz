using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Edo.Docflow.Converters
{
	public interface IDeliveryPointConverter
	{
		/// <summary>
		/// Конвертация точки доставки <see cref="DeliveryPointEntity"/> в информацию о ней для ЭДО <see cref="DeliveryPointInfoForEdo"/>
		/// </summary>
		/// <param name="deliveryPoint">Конвертируемая ТД</param>
		/// <returns>Информация о ТД для ЭДО</returns>
		DeliveryPointInfoForEdo ConvertDeliveryPointToDeliveryPointInfoForEdo(DeliveryPointEntity deliveryPoint);
	}
}
