namespace VodovozBusiness.Services.Clients.DeliveryPoints
{
	/// <summary>
	/// Интерфейс обработчика строки с номером дома
	/// </summary>
	public interface IDeliveryPointBuildingNumberHandler
	{
		/// <summary>
		/// Конвертирование строки с номером дома под формат Erp
		/// </summary>
		/// <param name="building">Строка с номером дома</param>
		/// <returns>Номер дома в формате Erp</returns>
		string TryConvertBuildingStringToErpFormat(string building);
	}
}
