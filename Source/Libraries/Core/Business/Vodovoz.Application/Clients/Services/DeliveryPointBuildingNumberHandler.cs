using System;
using VodovozBusiness.Services.Clients.DeliveryPoints;

namespace Vodovoz.Application.Clients.Services
{
	/// <summary>
	/// Обработчик поля Дом/Строение точки доставки
	/// </summary>
	public class DeliveryPointBuildingNumberHandler : IDeliveryPointBuildingNumberHandler
	{
		private readonly IDeliveryPointBuildingNumberParser _buildingNumberParser;

		public DeliveryPointBuildingNumberHandler(IDeliveryPointBuildingNumberParser numberParser)
		{
			_buildingNumberParser = numberParser ?? throw new ArgumentNullException(nameof(numberParser));
		}
		
		/// <summary>
		/// Конвертирование строки с номером дома под формат Erp,
		/// если не удалось распарсить входящую строку - возвращаем исходный вариант
		/// </summary>
		/// <param name="building">Строка с номером дома</param>
		/// <returns>Номер дома в формате Erp, либо исходная строка</returns>
		public string TryConvertBuildingStringToErpFormat(string building)
		{
			var parsedBuilding = _buildingNumberParser.ParseBuildingNumber(building);
			return string.IsNullOrWhiteSpace(parsedBuilding.BuildingNumber) ? building : parsedBuilding.ToString();
		}
	}
}
