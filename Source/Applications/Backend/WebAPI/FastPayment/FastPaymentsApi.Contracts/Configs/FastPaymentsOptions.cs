namespace FastPaymentsApi.Contracts.Configs
{
	/// <summary>
	/// Конфигурация для интеграции с FastPayment API
	/// </summary>
	public class FastPaymentsOptions
	{
		/// <summary>
		/// Ссылка на API банка для интеграции с FastPayment Order API
		/// </summary>
		public string ApiUrl { get; set; }
	}
}
