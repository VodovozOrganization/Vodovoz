using System;

namespace DriverAPI.Library.Helpers
{
	public class FCMException : Exception
	{
		public FCMException(string message) : base(message) { }
	}
}
