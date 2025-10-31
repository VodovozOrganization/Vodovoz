using System;

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
		
		/// <summary>
		/// Дата начала работы контрольной точки по платежам из выписки(для контроля приходов и разносов с 1С)
		/// </summary>
		DateTime ControlPointStartDate { get; }
	}
}
