namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public class TemplateRecurrentPaymentDto : UserCardInfo
	{
		/// <summary>
		/// Токен
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// ID аккаунта
		/// </summary>
		public string AccountId { get; set; }
	}
}
