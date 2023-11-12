using QS.DomainModel.Entity;
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

	public class PacsDomainSettings : IDomainObject, IPacsDomainSettings
	{
		public int Id { get; set; }
		public int AdministratorId { get; set; }
		public DateTime Timestamp { get; set; }
		public int MaxOperatorsOnBreak { get; set; }
		public TimeSpan MaxBreakTime { get; set; }
	}
}
