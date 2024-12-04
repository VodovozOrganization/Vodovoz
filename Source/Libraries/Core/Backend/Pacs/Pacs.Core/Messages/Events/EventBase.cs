using System;

namespace Pacs.Core.Messages.Events
{
	/// <summary>
	/// Событие
	/// </summary>
	public abstract class EventBase
	{
		/// <summary>
		/// Идентификатор события
		/// </summary>
		public Guid EventId { get; set; }
	}
}
