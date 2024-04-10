using System.Collections.Generic;

namespace Pacs.Core.Messages.Events
{
	public class GlobalBreakAvailabilityEvent
	{
		public bool LongBreakAvailable { get; set; } = true;
		public string LongBreakDescription { get; set; } = "";

		public bool ShortBreakAvailable { get; set; } = true;
		public string ShortBreakDescription { get; set; } = "";

		public override bool Equals(object obj)
		{
			return obj is GlobalBreakAvailabilityEvent availability &&
				   LongBreakAvailable == availability.LongBreakAvailable &&
				   LongBreakDescription == availability.LongBreakDescription &&
				   ShortBreakAvailable == availability.ShortBreakAvailable &&
				   ShortBreakDescription == availability.ShortBreakDescription;
		}

		public override int GetHashCode()
		{
			int hashCode = 622147706;
			hashCode = hashCode * -1521134295 + LongBreakAvailable.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LongBreakDescription);
			hashCode = hashCode * -1521134295 + ShortBreakAvailable.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortBreakDescription);
			return hashCode;
		}
	}
}
