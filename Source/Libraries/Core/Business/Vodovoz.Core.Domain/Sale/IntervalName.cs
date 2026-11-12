using System;

namespace Vodovoz.Core.Domain.Sale
{
	public static class IntervalName
	{
		public static string CustomerOrdersApiIntervalName(TimeSpan from, TimeSpan to)
		{
			return $"{from:hh\\:mm} - {to:hh\\:mm}";
		}
	}
}
