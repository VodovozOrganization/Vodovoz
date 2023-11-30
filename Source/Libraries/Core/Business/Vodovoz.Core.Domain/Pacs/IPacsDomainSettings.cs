using System;

namespace Vodovoz.Core.Domain.Pacs
{
	public interface IPacsDomainSettings
	{
		DateTime Timestamp { get; set; }
		int AdministratorId { get; set; }
		TimeSpan MaxBreakTime { get; set; }
		int MaxOperatorsOnBreak { get; set; }
	}
}
