namespace DeliveryRulesService.Options
{
	public class DistrictCacheServiceSettings
	{
		/// <summary>
		/// Конфигурация логирования предзагрузки
		/// </summary>
		public PreloadLoggingLevel PreloadLoggingLevel { get; set; } = PreloadLoggingLevel.Simple;
	}
}
