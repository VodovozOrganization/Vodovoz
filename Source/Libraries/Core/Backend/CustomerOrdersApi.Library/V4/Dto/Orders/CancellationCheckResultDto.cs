namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// DTO для результата проверки возможности отмены заказа
	/// </summary>
	public class CancellationCheckResultDto
	{
		/// <summary>
		/// Возможна ли отмена заказа
		/// </summary>
		public bool CanCancel { get; set; }

		/// <summary>
		/// Требуется ли контакт с менеджером (для установленного маршрута)
		/// </summary>
		public bool RequireManagerContact { get; set; }

		/// <summary>
		/// Причина, если отмена невозможна
		/// </summary>
		public string ReasonMessage { get; set; }
	}
}
