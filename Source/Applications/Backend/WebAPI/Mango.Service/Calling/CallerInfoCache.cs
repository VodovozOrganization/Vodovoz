using System;

namespace Mango.Service.Calling
{
	public class CallerInfoCache
	{
		public string Number => Caller.Number;

		public Caller Caller { get; private set; }

		private DateTime created = DateTime.Now;

		public CallerInfoCache(Caller caller)
		{
			Caller = caller;
		}

		public TimeSpan LiveTime => DateTime.Now - created;
	}
}