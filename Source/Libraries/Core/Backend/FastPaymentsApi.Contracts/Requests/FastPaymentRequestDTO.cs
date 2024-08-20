namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Информация для регистрации заказа в Авангарде для оплаты из ДВ
	/// </summary>
	public class FastPaymentRequestDTO
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Номер телефона
		/// </summary>
		public string PhoneNumber { get; set; }
		/// <summary>
		/// Оплата по Qr или карте
		/// </summary>
		public bool IsQr { get; set; } = true;
	}
}
