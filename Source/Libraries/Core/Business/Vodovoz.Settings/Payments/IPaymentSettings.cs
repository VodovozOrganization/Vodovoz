namespace Vodovoz.Services
{
	public interface IPaymentSettings
	{
		#region Payment ProfitCategory

		/// <summary>
		/// Категория дохода по умолчанию
		/// </summary>
		int DefaultProfitCategoryId { get; }
		/// <summary>
		/// Категория Прочий доход
		/// </summary>
		int OtherProfitCategoryId { get; }

		#endregion
	}
}
