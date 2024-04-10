using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Pacs
{
	/// <summary>
	/// Основной звонок, создается при начале звонка одной из сторон
	/// сохраняет информацию о состоянии звонка на всем его протяжении
	/// не зависимо от того, кто из операторов на него ответил, 
	/// или сколько раз его могли перевести
	/// </summary>
	public class Call
	{
		public virtual string EntryId { get; set; }
		public virtual string CallId { get; set; }
		public virtual DateTime? StartTime { get; set; }
		public virtual DateTime? EndTime { get; set; }
		public virtual string FromNumber { get; set; }
		public virtual string FromExtension { get; set; }
		public virtual string ToNumber { get; set; }
		public virtual string ToExtension { get; set; }
		public virtual string ToLineNumber { get; set; }
		public virtual int? DisconnectReason { get; set; }
		public virtual CallDirection? CallDirection { get; set; }
		public virtual CallEntryResult? EntryResult { get; set; }

		public virtual DateTime CreationTime { get; set; }
		public virtual IList<SubCall> SubCalls { get; set; } = new List<SubCall>();
		public virtual CallStatus Status { get; set; }
	}
}
