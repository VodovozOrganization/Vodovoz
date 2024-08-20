namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Данные для регистрации онлайн заказа в системе эквайринга банка Авангард
	/// </summary>
	public class RequestRegisterOnlineOrderDTO
	{
		/// <summary>
		/// Id заказа (Мобильное приложение: номер от 200_000_000, сайт: от 100_000_000)
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }
		/// <summary>
		/// Ссылка для возврата с платежной страницы
		/// Если не заполнены BackUrlOk и BackUrlFail будет использоваться только эта ссылка во всех случаях
		/// </summary>
		public string BackUrl { get; set; }
		/// <summary>
		/// Ссылка для возврата при успешной оплате
		/// </summary>
		public string BackUrlOk { get; set; }
		/// <summary>
		/// Ссылка для возврата при ошибке или отказа от оплаты
		/// </summary>
		public string BackUrlFail { get; set; }
		/// <summary>
		/// Ссылка для отправки уведомления о смене статуса оплаты для ИПЗ
		/// </summary>
		public string CallbackUrl { get; set; }
	}
}
