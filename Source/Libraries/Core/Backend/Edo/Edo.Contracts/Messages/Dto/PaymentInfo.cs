namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Информация об оплатах
	/// </summary>
	public class PaymentInfo
	{
		/// <summary>
		/// Номер платежа
		/// </summary>
		public string PaymentNum { get; set; }
		/// <summary>
		/// Дата платежа, в формате dd.MM.yyyy
		/// </summary>
		public string PaymentDate { get; set; }
	}
}
