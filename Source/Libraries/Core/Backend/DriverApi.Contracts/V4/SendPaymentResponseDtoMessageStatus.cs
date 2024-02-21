namespace DriverApi.Contracts.V4
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
