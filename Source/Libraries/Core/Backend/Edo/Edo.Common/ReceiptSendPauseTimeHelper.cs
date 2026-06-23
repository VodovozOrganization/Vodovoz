using System;

namespace Edo.Common
{
	public static class ReceiptSendPauseTimeHelper
	{
		public static bool IsNightPauseTime(TimeSpan currentTime, TimeSpan pauseStart, TimeSpan pauseEnd)
		{
			if(pauseStart == pauseEnd)
			{
				return false;
			}

			if(pauseStart < pauseEnd)
			{
				return currentTime >= pauseStart
					&& currentTime < pauseEnd;
			}

			return currentTime >= pauseStart
				|| currentTime < pauseEnd;
		}
	}
}
