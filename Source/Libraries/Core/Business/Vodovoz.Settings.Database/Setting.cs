using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;

namespace Vodovoz.Settings.Database
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "настройки параметов приложения",
		Nominative = "настройка параметров приложения"
	)]
	[HistoryTrace]
	public class Setting : IDomainObject
	{
		#region MappedProperties

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string StrValue { get; set; }
		public virtual TimeSpan CacheTimeout { get; set; }
		public virtual string Description { get; set; }

		#endregion

		public virtual DateTime? CachedTime { get; set; }

		public virtual bool IsExpired
		{
			get
			{
				if(CachedTime == null)
				{
					return false;
				}
				return DateTime.Now.Subtract(CachedTime.Value) > CacheTimeout;
			}
		}

		public virtual string Title => 
			string.IsNullOrWhiteSpace(Description)
			? Name 
			: Description;
	}
}
