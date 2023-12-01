using System;

namespace Sms.External.SmsRu
{
	internal class TimeHelper
	{
		public static int GetCurrentUnixTime()
		{
			return GetUnixTime(DateTime.Now);
		}

		public static int GetUnixTime(DateTime dateTime)
		{
			dateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime);
			return (int)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
		}
	}
}
