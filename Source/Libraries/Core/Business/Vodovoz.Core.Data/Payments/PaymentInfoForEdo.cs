namespace Vodovoz.Core.Data.Payments
{
	/// <summary>
	/// Информация об оплате для ЭДО(электронного документооборота)
	/// </summary>
	public class PaymentInfoForEdo
	{
		protected PaymentInfoForEdo(string paymentNum, string paymentDate)
		{
			PaymentNum = paymentNum;
			PaymentDate = paymentDate;
		}
		
		/// <summary>
		/// номер платежа
		/// </summary>
		public string PaymentNum { get; }
		/// <summary>
		/// Дата платежа, в формате dd.MM.yyyy
		/// </summary>
		public string PaymentDate { get; }

		public static PaymentInfoForEdo Create(string paymentNum, string paymentDate) => new PaymentInfoForEdo(paymentNum, paymentDate);
	}
}
