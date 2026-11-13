namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Бонусы по заказу для карточки МП
	/// </summary>
	public sealed class OrderBonusesDto
	{
		/// <summary>
		/// Удалось ли получить данные из Mindbox
		/// </summary>
		public bool IsAvailable { get; set; }

		/// <summary>
		/// Списано бонусов
		/// </summary>
		public int? WrittenOff { get; set; }

		/// <summary>
		/// Начислено или будет начислено бонусов
		/// </summary>
		public int? Earned { get; set; }

		/// <summary>
		/// Статус начисления
		/// </summary>
		public OrderBonusesEarnStatus? EarnStatus { get; set; }
	}
}
