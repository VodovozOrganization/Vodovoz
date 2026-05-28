using System;

namespace Vodovoz.EntityRepositories.Payments
{
	/// <summary>
	/// Данные списания с баланса клиента для сверки с актом 1С.
	/// </summary>
	public class PaymentWriteOffNode
	{
		public int Id { get; set; }
		public int PaymentNumber { get; set; }
		public DateTime Date { get; set; }
		public decimal Sum { get; set; }
		public string Reason { get; set; }
	}
}
