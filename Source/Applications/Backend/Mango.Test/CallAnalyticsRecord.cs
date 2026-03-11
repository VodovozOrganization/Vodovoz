using System;

namespace Mango.Test
{
	public class CallAnalyticsRecord
	{
		public DateTime Date { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public DateTime? AnswerTime { get; set; }

		public string Direction { get; set; }

		public bool IsMissed { get; set; }
	}
}
