using System;

namespace Vodovoz.Core.Domain.Pacs
{
	/// <summary>
	/// Подзвонок, является звонком внутри системы АТС, связанный с основным звонком
	/// Создается при дозвоне на каждого оператора, при переводе и тд
	/// </summary>
	public class SubCall
	{
		public virtual string CallId { get; set; }

		// Локальные свойства

		public virtual DateTime CreationTime { get; set; }
		public virtual CallState State { get; set; }
		public virtual bool WasConnected { get; set; }
		public virtual int LastSeq { get; set; }

		// Свойства из манго

		// Не изменяемые  свойства
		public virtual DateTime StartTime { get; set; }
		public virtual DateTime? EndTime { get; set; }
		public virtual string EntryId { get; set; }
		public virtual string FromNumber { get; set; }
		public virtual string FromExtension { get; set; }
		public virtual string ToNumber { get; set; }
		public virtual string ToExtension { get; set; }
		public virtual string ToLineNumber { get; set; }
		public virtual string ToAcdGroup { get; set; }
		public virtual string TakenFromCallId { get; set; }
		public virtual int? DisconnectReason { get; set; }
	}
}
