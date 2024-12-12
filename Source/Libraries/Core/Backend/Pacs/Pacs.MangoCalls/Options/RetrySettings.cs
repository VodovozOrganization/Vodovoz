using System;

namespace Pacs.MangoCalls.Options
{
	public class RetrySettings
	{
		public int RetryCount { get; set; }
		public TimeSpan Delay { get; set; }
	}
}
