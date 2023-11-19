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
		public virtual int Id { get; set; }
		public virtual int AdministratorId { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual int MaxOperatorsOnBreak { get; set; }
		public virtual TimeSpan MaxBreakTime { get; set; }
	}
}
