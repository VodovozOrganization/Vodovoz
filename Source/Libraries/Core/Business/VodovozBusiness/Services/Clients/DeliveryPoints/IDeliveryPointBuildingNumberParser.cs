using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace VodovozBusiness.Services.Clients.DeliveryPoints
{
	/// <summary>
	/// Интерфейс парсера строки с номером дома
	/// </summary>
	public interface IDeliveryPointBuildingNumberParser
	{
		/// <summary>
		/// Парсинг строки с номером дома
		/// </summary>
		/// <param name="building">Строка с номером дома</param>
		/// <returns>Разделенные данные с номером дома, корпусом, литерой, строением</returns>
		BuildingNumberDetails ParseBuildingNumber(string building);
	}
}
