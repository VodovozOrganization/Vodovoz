namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Статус попытки отправки смс для оплаты
	/// </summary>
	public enum SendPaymentResponseDtoMessageStatus
	{
		/// <summary>
		/// Успешно
		/// </summary>
		Ok,
		/// <summary>
		/// Ошибка
		/// </summary>
		Error
	}
}
