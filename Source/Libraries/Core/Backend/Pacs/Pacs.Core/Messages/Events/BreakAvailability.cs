using System;
using System.Collections.Generic;

namespace Pacs.Core.Messages.Events
{
	public class OperatorBreakAvailability
	{
		public int OperatorId { get; set; }

        public bool LongBreakAvailable { get; set; } = true;
        public string LongBreakDescription { get; set; } = "";

		public bool ShortBreakAvailable { get; set; } = true;
        public DateTime? ShortBreakSupposedlyAvailableAfter { get; set; } = null;
		public string ShortBreakDescription { get; set; } = "";

		public override bool Equals(object obj)
		{
			return obj is OperatorBreakAvailability availability &&
				   LongBreakAvailable == availability.LongBreakAvailable &&
				   LongBreakDescription == availability.LongBreakDescription &&
				   ShortBreakAvailable == availability.ShortBreakAvailable &&
				   ShortBreakSupposedlyAvailableAfter == availability.ShortBreakSupposedlyAvailableAfter &&
				   ShortBreakDescription == availability.ShortBreakDescription;
		}

		public override int GetHashCode()
		{
			int hashCode = -1461885315;
			hashCode = hashCode * -1521134295 + OperatorId.GetHashCode();
			hashCode = hashCode * -1521134295 + LongBreakAvailable.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LongBreakDescription);
			hashCode = hashCode * -1521134295 + ShortBreakAvailable.GetHashCode();
			hashCode = hashCode * -1521134295 + ShortBreakSupposedlyAvailableAfter.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortBreakDescription);
			return hashCode;
		}
	}

	public class GlobalBreakAvailability
	{
		public bool LongBreakAvailable { get; set; } = true;
		public string LongBreakDescription { get; set; } = "";

		public bool ShortBreakAvailable { get; set; } = true;
		public string ShortBreakDescription { get; set; } = "";

		public override bool Equals(object obj)
		{
			return obj is GlobalBreakAvailability availability &&
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
