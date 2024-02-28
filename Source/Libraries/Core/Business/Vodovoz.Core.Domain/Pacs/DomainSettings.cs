using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;

namespace Vodovoz.Core.Domain.Pacs
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки СКУД",
		Nominative = "настройки СКУД")]
	[EntityPermission]
	[HistoryTrace]
	public class DomainSettings : IDomainObject, IPacsDomainSettings
	{
		public virtual int Id { get; set; }
		public virtual int AdministratorId { get; set; }
		public virtual DateTime Timestamp { get; set; }
		public virtual int OperatorsOnLongBreak { get; set; }
		public virtual TimeSpan LongBreakDuration { get; set; }
		public virtual int LongBreakCountPerDay { get; set; }
		public virtual int OperatorsOnShortBreak { get; set; }
		public virtual TimeSpan ShortBreakDuration { get; set; }
		public virtual TimeSpan ShortBreakInterval { get; set; }

		public override bool Equals(object obj)
		{
			return obj is DomainSettings settings &&
				   OperatorsOnLongBreak == settings.OperatorsOnLongBreak &&
				   LongBreakDuration.Equals(settings.LongBreakDuration) &&
				   LongBreakCountPerDay == settings.LongBreakCountPerDay &&
				   OperatorsOnShortBreak == settings.OperatorsOnShortBreak &&
				   ShortBreakDuration.Equals(settings.ShortBreakDuration) &&
				   ShortBreakInterval.Equals(settings.ShortBreakInterval);
		}
	}
}
