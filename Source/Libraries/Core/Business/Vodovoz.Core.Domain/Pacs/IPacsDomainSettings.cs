using System;

namespace Vodovoz.Core.Domain.Pacs
{
	public interface IPacsDomainSettings
	{
		DateTime Timestamp { get; set; }
		int AdministratorId { get; set; }
		int OperatorsOnLongBreak { get; set; }
		TimeSpan LongBreakDuration { get; set; }
		int LongBreakCountPerDay { get; set; }
		int OperatorsOnShortBreak { get; set; }
		TimeSpan ShortBreakDuration { get; set; }
		TimeSpan ShortBreakInterval { get; set; }
	}
}
