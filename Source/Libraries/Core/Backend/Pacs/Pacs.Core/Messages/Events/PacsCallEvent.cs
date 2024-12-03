using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	/// <summary>
	/// Событие звонка
	/// </summary>
	public class PacsCallEvent : EventBase
	{
		/// <summary>
		/// Звонок
		/// </summary>
		public Call Call { get; set; }
	}
}
