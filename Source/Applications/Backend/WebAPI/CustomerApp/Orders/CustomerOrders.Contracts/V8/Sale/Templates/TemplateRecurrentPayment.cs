namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public class TemplateRecurrentPayment
	{
		/// <summary>
		/// Токен
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// ID аккаунта
		/// </summary>
		public string AccountId { get; set; }

		/// <summary>
		/// Последние 4 цифры карты
		/// </summary>
		public string CardLastFour { get; set; }

		/// <summary>
		/// Тип карты
		/// </summary>
		public string CardType { get; set; }
	}
}
