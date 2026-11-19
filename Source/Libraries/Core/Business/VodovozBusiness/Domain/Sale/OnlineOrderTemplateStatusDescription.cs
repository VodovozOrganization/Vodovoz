namespace VodovozBusiness.Domain.Sale
{
	public static class OnlineOrderTemplateStatusDescription
	{
		/// <summary>
		/// Детализации для статуса ошибка оплаты
		/// </summary>
		public static class PaymentProblemDescription
		{
			/// <summary>
			/// Карта не привязана
			/// </summary>
			public static string CardNotLinked => "Карта не привязана";

			/// <summary>
			/// Недостаточно средств
			/// </summary>
			public static string NotEnoughFunds => "Не удалось списать оплату: недостаточно средств";
		}

		/// <summary>
		/// Детализации для статуса ручной обработки
		/// </summary>
		public static class ManualProcessingDescription
		{
			/// <summary>
			/// Автозаказ не оплачен
			/// </summary>
			/// <param name="autoOrderId">Идентификатор автозаказа</param>
			/// <returns></returns>
			public static string AutoOrderUnPaid(int autoOrderId) =>
				$"Заказ №{autoOrderId} не оплачен. Менеджер свяжется с вами";
		}
	}
}
