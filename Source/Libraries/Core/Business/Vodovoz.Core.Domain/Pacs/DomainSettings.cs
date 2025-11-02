using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;

namespace Vodovoz.Core.Domain.Pacs
{
	/// <summary>
	/// Настройки СКУД (операторов КЦ)
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки СКУД",
		Nominative = "настройки СКУД")]
	[EntityPermission]
	[HistoryTrace]
	public class DomainSettings : IDomainObject, IPacsDomainSettings
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Идентификатор администратора, который последним обновил настройки
		/// </summary>
		public virtual int AdministratorId { get; set; }

		/// <summary>
		/// Время обновления настроек
		/// </summary>
		public virtual DateTime Timestamp { get; set; }

		/// <summary>
		/// Количество операторов которые одновременно могут быть на большом перерыве
		/// </summary>
		public virtual int OperatorsOnLongBreak { get; set; }

		/// <summary>
		/// Длительность большого перерыва (максимальная)
		/// </summary>
		public virtual TimeSpan LongBreakDuration { get; set; }

		/// <summary>
		/// Количество больших перерывов в день
		/// </summary>
		public virtual int LongBreakCountPerDay { get; set; }

		/// <summary>
		/// Количество операторов которые одновременно могут быть на коротком перерыве
		/// </summary>
		public virtual int OperatorsOnShortBreak { get; set; }

		/// <summary>
		/// Длительность короткого перерыва (максимальная)
		/// </summary>
		public virtual TimeSpan ShortBreakDuration { get; set; }

		/// <summary>
		/// Интервал между короткими перерывами
		/// </summary>
		public virtual TimeSpan ShortBreakInterval { get; set; }

		public override bool Equals(object obj)
			=> obj is DomainSettings settings
			&& OperatorsOnLongBreak == settings.OperatorsOnLongBreak
			&& LongBreakDuration.Equals(settings.LongBreakDuration)
			&& LongBreakCountPerDay == settings.LongBreakCountPerDay
			&& OperatorsOnShortBreak == settings.OperatorsOnShortBreak
			&& ShortBreakDuration.Equals(settings.ShortBreakDuration)
			&& ShortBreakInterval.Equals(settings.ShortBreakInterval);
	}
}
