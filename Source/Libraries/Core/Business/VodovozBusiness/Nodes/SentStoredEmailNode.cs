using System;
using Vodovoz.Core.Domain.StoredEmails;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Данные оь отправленных электронных письмах
	/// </summary>
	public class SentStoredEmailNode
	{
		private SentStoredEmailNode(DateTime sentDate, string recipientAddress, StoredEmailStates state)
		{
			SentDate = sentDate;
			RecipientAddress = recipientAddress;
			State = state;
		}
		
		/// <summary>
		/// Дата отправки
		/// </summary>
		public DateTime SentDate { get; set; }
		/// <summary>
		/// Адрес получателя
		/// </summary>
		public string RecipientAddress { get; set; }
		/// <summary>
		/// Состояние письма
		/// </summary>
		public StoredEmailStates State { get; set; }

		public static SentStoredEmailNode Create(DateTime sentDate, string recipientAddress, StoredEmailStates state) =>
			new SentStoredEmailNode(sentDate, recipientAddress, state);
	}
}
