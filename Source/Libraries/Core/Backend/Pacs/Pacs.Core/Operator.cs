using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core
{
	/*public class Operator
	{
		public int Id { get; set; }
		public int OperatorId { get; set; }
		public DateTime Timestamp { get; set; }
		public OperatorTrigger Trigger { get; set; }
		public OperatorState State { get; set; }
		public string PhoneNumber { get; set; }
		public string CallId { get; set; }
	}*/

	/*public enum OperatorState
	{
		Connected,
		WaitingForCall,
		Talk,
		Break,
		Disconnected
	}

	public enum OperatorTrigger
	{
		Connect,
		StartWorkShift,
		TakeCall,
		EndCall,
		TakeBreak,
		EndBreak,
		ChangePhone,
		EndWorkShift,
		Disconnect
	}*/

	public class Administrator
	{

	}



	public class Call
	{
		public int Id { get; set; }
		public string CallId { get; set; }
		public string CallerNumber { get; set; }
		public DateTime CallTime { get; set; }
		public DateTime? ConnectedTime { get; set; }
		public DateTime? DisconnectedTime { get; set; }
		public CallState State { get; set; }
		public string OperatorId { get; set; }
		public int DisconnectedReason { get; set; }
		public string DisconnectedReasonDescription { get; set; }
		public string ForwardedFromCallId { get; set; }
	}
}
