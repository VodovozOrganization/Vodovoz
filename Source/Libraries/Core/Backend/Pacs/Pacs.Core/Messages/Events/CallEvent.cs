using System;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	public class CallEvent
	{
		public int Id { get; set; }
		public string CallId { get; set; }
		public DateTime Timestamp { get; set; }
		public uint Seq { get; set; }
		public CallState CallState { get; set; }
		public string FromNumber { get; set; }
		public string FromExtension { get; set; }
		public string TakenFromCallId { get; set; }
		public string ToNumber { get; set; }
		public string ToExtension { get; set; }
		public string OperatorId { get; set; }
		public int DisconnectReason { get; set; }
	}
}
