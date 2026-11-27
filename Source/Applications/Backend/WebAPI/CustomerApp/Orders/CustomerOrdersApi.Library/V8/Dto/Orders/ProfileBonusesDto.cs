namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Бонусный счёт клиента в профиле
	/// </summary>
	public sealed class ProfileBonusesDto
	{
		/// <summary>
		/// Удалось ли получить данные из Mindbox
		/// </summary>
		public bool IsAvailable { get; set; }

		/// <summary>
		/// Доступно бонусов
		/// </summary>
		public int? Balance { get; set; }

		/// <summary>
		/// Ожидает начисления
		/// </summary>
		public int? Pending { get; set; }

		/// <summary>
		/// Ближайшее сгорание
		/// </summary>
		public ProfileBonusesExpiringDto Expiring { get; set; }

		/// <summary>
		/// Нулевой бонусный счёт для клиента без регистрации в Mindbox
		/// </summary>
		public static ProfileBonusesDto Zero()
		{
			return new ProfileBonusesDto
			{
				IsAvailable = true,
				Balance = 0,
				Pending = 0
			};
		}

		/// <summary>
		/// Недоступность данных Mindbox
		/// </summary>
		public static ProfileBonusesDto Unavailable()
		{
			return new ProfileBonusesDto { IsAvailable = false };
		}
	}
}
