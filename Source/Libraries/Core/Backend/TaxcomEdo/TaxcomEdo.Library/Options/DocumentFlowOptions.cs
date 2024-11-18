namespace TaxcomEdo.Library.Options
{
	/// <summary>
	/// Настройки для обработки документооборота
	/// </summary>
	public sealed class DocumentFlowOptions
	{
		/// <summary>
		/// Название секции в конфиге с настройками
		/// </summary>
		public const string Path = nameof(DocumentFlowOptions); 
		
		/// <summary>
		/// Количество добавляемых месяцев для выборки заказов для отправки УПД
		/// </summary>
		public int AddMonthForUpdPreparing { get; set; }
		/// <summary>
		/// Количество добавляемых дней для выборки заказов для отправки счетов
		/// </summary>
		public int AddDaysForBillsPreparing { get; set; }
		/// <summary>
		/// Задержка между итерациями подготовки документов в секундах
		/// </summary>
		public int DelayBetweenPreparingInSeconds { get; set; }
	}
}
