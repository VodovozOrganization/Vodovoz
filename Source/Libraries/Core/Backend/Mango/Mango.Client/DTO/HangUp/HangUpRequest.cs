using System;
namespace Mango.Client.DTO.HangUp
{
	public class HangUpRequest
	{
		public string command_id { get; set; }
		public string call_id { get; set; }
	}
}
