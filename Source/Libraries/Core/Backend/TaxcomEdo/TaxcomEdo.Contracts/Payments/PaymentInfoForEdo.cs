namespace TaxcomEdo.Contracts.Payments
{
	/// <summary>
	/// Информация об оплате для ЭДО(электронного документооборота)
	/// </summary>
	public class PaymentInfoForEdo
	{
		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public PaymentInfoForEdo() { }
		
		protected PaymentInfoForEdo(string paymentNum, string paymentDate)
		{
			PaymentNum = paymentNum;
			PaymentDate = paymentDate;
		}
		
		/// <summary>
		/// Номер платежа
		/// </summary>
		public string PaymentNum { get; set; }
		/// <summary>
		/// Дата платежа, в формате dd.MM.yyyy
		/// </summary>
		public string PaymentDate { get; set; }

		public static PaymentInfoForEdo Create(string paymentNum, string paymentDate) => new PaymentInfoForEdo(paymentNum, paymentDate);
	}
}
