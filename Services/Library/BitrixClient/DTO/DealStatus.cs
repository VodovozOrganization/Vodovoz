namespace Bitrix.DTO
{
	/// <summary>
	/// Стадии сделки
	/// </summary>
	public enum DealStatus
	{
		/// <summary>
		/// Завести в ДВ
		/// </summary>
		ToCreate,

		/// <summary>
		/// В обработке ДВ
		/// </summary>
		InProgress,

		/// <summary>
		/// Ошибка обработки ДВ
		/// </summary>
		Error,

		/// <summary>
		/// Успешна
		/// </summary>
		Success,

		/// <summary>
		/// Не успешна
		/// </summary>
		Fail
	}
}
