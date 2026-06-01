using System.Text.Json.Serialization;

namespace CloudPaymentsApi.Library.Models
{
	/// <summary>
	/// Статусы транзакций CloudPayments
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CloudPaymentsTransactionStatus
	{
		/// <summary>
		/// Создана (начальный статус для возвратов)
		/// </summary>
		Created = 0, 

		/// <summary>
		/// Ожидает аутентификации
		/// Возможные действия: Нет
		/// </summary>
		AwaitingAuthentication = 1,

		/// <summary>
		/// Авторизована
		/// Возможные действия: Подтверждение, Отмена (void)
		/// </summary>
		Authorized = 2,

		/// <summary>
		/// Завершена
		/// Возможные действия: Возврат денег (refund)
		/// </summary>
		Completed = 3,

		/// <summary>
		/// Отменена
		/// Возможные действия: Нет
		/// </summary>
		Cancelled = 4,

		/// <summary>
		/// Отклонена банком/платежной системой
		/// Возможные действия: Нет
		/// </summary>
		Declined = 5,
	}
}
