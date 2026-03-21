namespace VodovozHealthCheck.Providers
{
	/// <summary>
	/// Провайдер информации о сервисе для HealthCheck
	/// </summary>
	public interface IHealthCheckServiceInfoProvider
	{
		/// <summary>
		/// Название сервиса
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Подробное описание всего, что делает сервис
		/// </summary>
		string DetailedDescription { get; }
	}
}
