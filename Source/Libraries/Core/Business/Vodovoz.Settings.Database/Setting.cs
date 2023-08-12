using System;

namespace Vodovoz.Settings.Database
{
	public class Setting
	{
		#region MappedProperties

		public virtual string Name { get; set; }
		public virtual string StrValue { get; set; }
		public virtual TimeSpan CacheTimeout { get; set; }

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
	}
}
