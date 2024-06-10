using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Тип действия в мобильном приложении водителей
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ActionDtoType
	{
		/// <summary>
		/// Открытие инфопанели
		/// </summary>
		OpenOrderInfoPanel,
		/// <summary>
		/// Открытие панели зоставки
		/// </summary>
		OpenOrderDeliveryPanel,
		/// <summary>
		/// Открытие панели приема оборудования
		/// </summary>
		OpenOrderReceiptionPanel,
		/// <summary>
		/// Завершение заказа
		/// </summary>
		CompleteOrderClicked
	}
}
