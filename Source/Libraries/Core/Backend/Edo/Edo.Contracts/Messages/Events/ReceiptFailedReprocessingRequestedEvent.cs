using System;

namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие запроса внеплановой переобработки чеков со статусом Failed
	/// </summary>
	public class ReceiptFailedReprocessingRequestedEvent
	{
		/// <summary>
		/// Инициатор запроса (имя пользователя, отправившего команду)
		/// </summary>
		public string Initiator { get; set; }

		/// <summary>
		/// Время инициации запроса
		/// </summary>
		public DateTime RequestedAt { get; set; } = DateTime.Now;
	}
}
