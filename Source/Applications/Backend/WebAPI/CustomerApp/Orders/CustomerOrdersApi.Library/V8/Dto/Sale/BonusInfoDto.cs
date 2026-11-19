namespace CustomerOrdersApi.Library.V8.Dto.Sale
{
	/// <summary>
	/// Данные по бонусам клиента в заказе
	/// </summary>
	public class BonusInfoDto
	{
		/// <summary>
		/// Признак того, что расчёт выполнен
		/// </summary>
		public bool IsAvailable { get; set; }

		/// <summary>
		/// Причина, по которой расчёт недоступен
		/// </summary>
		public BonusUnavailableReason? UnavailableReason { get; set; }

		/// <summary>
		/// Доступно бонусов на счёте клиента
		/// </summary>
		public int? Balance { get; set; }

		/// <summary>
		/// Сколько бонусов можно списать в этом заказе
		/// </summary>
		public int MaxBonusesToWriteOff { get; set; }

		/// <summary>
		/// Сколько бонусов списано в расчёте
		/// </summary>
		public int BonusesWrittenOff { get; set; }

		/// <summary>
		/// Сколько бонусов начислится за заказ
		/// </summary>
		public int BonusesToEarn { get; set; }

		/// <summary>
		/// Создаёт данные о недоступности расчёта бонусов
		/// </summary>
		/// <param name="reason">Причина недоступности</param>
		/// <returns>Данные о недоступности расчёта</returns>
		public static BonusInfoDto CreateUnavailable(BonusUnavailableReason reason)
		{
			return new BonusInfoDto
			{
				IsAvailable = false,
				UnavailableReason = reason,
				Balance = null,
				MaxBonusesToWriteOff = 0,
				BonusesWrittenOff = 0,
				BonusesToEarn = 0
			};
		}
	}
}
