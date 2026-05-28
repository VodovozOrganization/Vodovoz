using System;

namespace Vodovoz.EntityRepositories.Payments
{
	/// <summary>
	/// Данные списания с баланса клиента для сверки с актом 1С.
	/// </summary>
	public class PaymentWriteOffNode
	{
		/// <summary>
		/// Идентификатор списания.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Номер платежного документа списания.
		/// </summary>
		public int PaymentNumber { get; set; }

		/// <summary>
		/// Дата списания.
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// Сумма списания.
		/// </summary>
		public decimal Sum { get; set; }

		/// <summary>
		/// Причина списания.
		/// </summary>
		public string Reason { get; set; }
	}
}
