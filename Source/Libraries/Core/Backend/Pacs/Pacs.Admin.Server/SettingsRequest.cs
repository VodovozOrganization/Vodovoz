using System;

namespace Pacs.Admin.Server
{
	public class SettingsRequest
	{
		public int AdministratorId { get; set; }
		public int OperatorsOnLongBreak { get; set; }
		public TimeSpan LongBreakDuration { get; set; }
		public int LongBreakCountPerDay { get; set; }
		public int OperatorsOnShortBreak { get; set; }
		public TimeSpan ShortBreakDuration { get; set; }
		public TimeSpan ShortBreakInterval { get; set; }
	}
}
