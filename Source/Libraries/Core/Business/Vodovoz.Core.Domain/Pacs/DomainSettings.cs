using QS.DomainModel.Entity;
using System;

namespace Vodovoz.Core.Domain.Pacs
{
	public class DomainSettings : IDomainObject, IPacsDomainSettings
	{
		public virtual int Id { get; set; }
		public virtual int AdministratorId { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual int MaxOperatorsOnBreak { get; set; }
		public virtual TimeSpan MaxBreakTime { get; set; }

		public override bool Equals(object obj)
		{
			return obj is DomainSettings settings &&
				   MaxOperatorsOnBreak == settings.MaxOperatorsOnBreak &&
				   MaxBreakTime.Equals(settings.MaxBreakTime);
		}
	}
}
