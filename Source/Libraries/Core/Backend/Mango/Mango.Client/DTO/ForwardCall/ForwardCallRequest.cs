using System;
namespace Mango.Client.DTO.ForwardCall
{
	public class ForwardCallRequest
	{
		public string command_id { get; set; }
		public string call_id { get; set; }
		public string method { get; set; }
		public string to_number { get; set; }
		public string initiator { get; set; }

	}
}
