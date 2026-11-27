namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Результат создания онлайн заказа
	/// </summary>
	public class CreatedOnlineOrderResult
	{
		public CreatedOnlineOrderResult() { }
		private CreatedOnlineOrderResult((int OnlineOrderId, int Code) data)
		{
			OnlineOrderId = data.OnlineOrderId;
			Code = data.Code;
		}
		/// <summary>
		/// Id онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }
		/// <summary>
		/// Http код
		/// </summary>
		public int Code { get; set; } = 500;

		/// <summary>
		/// Списано бонусов
		/// </summary>
		public int BonusesWrittenOff { get; set; }

		/// <summary>
		/// Бонусов к начислению
		/// </summary>
		public int? BonusesToEarn { get; set; }

		/// <summary>
		/// Код ошибки бонусов
		/// </summary>
		public string ErrorCode { get; set; }

		public static CreatedOnlineOrderResult Create((int OnlineOrderId, int Code) data) => new CreatedOnlineOrderResult(data);

		public static CreatedOnlineOrderResult Create(
			int onlineOrderId,
			int code,
			int bonusesWrittenOff = 0,
			int? bonusesToEarn = null,
			string errorCode = null)
		{
			return new CreatedOnlineOrderResult
			{
				OnlineOrderId = onlineOrderId,
				Code = code,
				BonusesWrittenOff = bonusesWrittenOff,
				BonusesToEarn = bonusesToEarn,
				ErrorCode = errorCode
			};
		}
	}
}
