using System;
using System.Collections.Generic;

namespace Mango.Client.DTO.User
{
	public class User
	{
		public General general { get; set; }
		public Telephony telephony { get; set; }
	}
	public class General
	{
		public string name { get; set; }
		public string email { get; set; }
		public string department { get; set; }
		public string position { get; set; }
	}
	public class Telephony
	{
		public string extension { get; set; }
		public string outgoingline { get; set; }
		public IEnumerable<Number> numbers { get; set; }

	}
	public class Number
	{
		public string number { get; set; }
		public string protocol { get; set; }
		public string order { get; set; }
		public string wait_sec { get; set; }
		public string status { get; set; }

	}
}
