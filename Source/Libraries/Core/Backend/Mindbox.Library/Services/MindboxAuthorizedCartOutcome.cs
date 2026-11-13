namespace Mindbox.Library.Services
{
	/// <summary>
	/// Результат расчёта бонусов заказа в Mindbox
	/// </summary>
	public enum MindboxAuthorizedCartOutcome
	{
		/// <summary>
		/// Расчёт выполнен
		/// </summary>
		Success = 0,

		/// <summary>
		/// Расчёт временно недоступен
		/// </summary>
		ServiceUnavailable = 1
	}
}
