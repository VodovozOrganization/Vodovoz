using System;

namespace Pacs.Server
{
	public class PacsStaticSettings : IPacsSettings
	{
		public TimeSpan OperatorInactivityTimeout => TimeSpan.FromMinutes(60);

		public TimeSpan OperatorKeepAliveInterval => TimeSpan.FromMinutes(1);
	}
}
