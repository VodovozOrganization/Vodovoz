namespace CustomerOrders.Contracts.V8.Sale
{
	public class UserCardInfo
	{
		/// <summary>
		/// Последние 4 цифры карты
		/// </summary>
		public string CardLastFour { get; set; }

		/// <summary>
		/// Тип карты
		/// </summary>
		public string CardType { get; set; }
		
		public static UserCardInfo Create(string cardLastFour, string cardType) =>
			new UserCardInfo
			{
				CardLastFour = cardLastFour,
				CardType = cardType
			};
	}
}
