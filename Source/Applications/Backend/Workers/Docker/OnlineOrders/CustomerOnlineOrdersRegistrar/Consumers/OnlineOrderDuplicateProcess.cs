namespace CustomerOnlineOrdersRegistrar.Consumers
{
	/// <summary>
	/// Действие над пришедшим онлайн заказом, возможным дублем
	/// </summary>
	public enum OnlineOrderDuplicateProcess
	{
		/// <summary>
		/// Нужно отменить
		/// </summary>
		NeedCancel,
		/// <summary>
		/// Отправить на ручную обработку
		/// </summary>
		ToManualProcessing
	}
}
