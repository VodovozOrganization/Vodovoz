namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Интерфейс, представляющий информацию о доставке заказа водителем
	/// </summary>
	public interface IDriverOrderShipmentInfo : ITrueMarkOrderScannedInfo
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		int OrderId { get; }

		/// <summary>
		/// Количество возвращенных бутылок
		/// </summary>
		int BottlesReturnCount { get; }

		/// <summary>
		/// Комментарий водителя
		/// </summary>
		string DriverComment { get; }
	}
}
