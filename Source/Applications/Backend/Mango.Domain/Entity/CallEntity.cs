using System;
using Mango.Domain.Enums;

namespace Mango.Domain.Entity
{
	public class CallEntity
	{
		public string EntryId { get; set; } = string.Empty;
		
		public string GroupName { get; set; }
		
		public string UnicHash { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public DateTime? AnswerTime { get; set; }

		public CallDirect CallDirect { get; set; }

		public bool IsMissed { get; set; }
	}
}
