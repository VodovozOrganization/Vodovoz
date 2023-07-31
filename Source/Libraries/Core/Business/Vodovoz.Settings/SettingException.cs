using System;
using System.Runtime.Serialization;

namespace Vodovoz.Settings
{
	public class SettingException : Exception
	{
		public SettingException()
		{
		}

		public SettingException(string message) : base(message)
		{
		}

		public SettingException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SettingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
