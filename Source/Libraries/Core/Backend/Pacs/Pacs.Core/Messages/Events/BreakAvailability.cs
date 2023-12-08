using System;

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
	}
}
