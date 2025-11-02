namespace DeliveryRulesService.Options
{
	/// <summary>
	/// Уровни логирования предзагрузки сущностей
	/// </summary>
	public enum PreloadLoggingLevel
	{
		/// <summary>
		/// Нет
		/// </summary>
		None,
		/// <summary>
		/// Только базовая информация
		/// </summary>
		Simple,
		/// <summary>
		/// Детальная информация<br/>
		/// Для районов выводятся границы
		/// </summary>
		Detailed
	}
}
