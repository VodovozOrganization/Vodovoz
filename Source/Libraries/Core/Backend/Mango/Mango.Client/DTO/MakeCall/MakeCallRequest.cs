using Mango.Client.DTO.Common;

namespace Mango.Client.DTO.MakeCall
{
	public class MakeCallRequest
	{
		public string command_id { get; set; }
		public From from { get; set; }
		public string to_number { get; set; }
		public string line_number { get; set; }
		public string sip_headers { get; set; }
	}
}
