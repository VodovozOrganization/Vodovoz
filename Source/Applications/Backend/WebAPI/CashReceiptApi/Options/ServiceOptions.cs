namespace CashReceiptApi.Options
{
	public class ServiceOptions
	{
		/// <summary>
		/// Ключ API
		/// </summary>
		public string ApiKey { get; set; }

		/// <summary>
		/// Id чека для проверки здоровья службы
		/// </summary>
		public int HealthCheckCashReceiptId { get; set; }
	}
}
