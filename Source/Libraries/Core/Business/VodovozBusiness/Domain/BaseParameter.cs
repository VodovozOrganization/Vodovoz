using System;
using Vodovoz.Settings.Database;

namespace Vodovoz.Domain
{
    public class BaseParameter
    {
        #region MappedProperties

        public virtual string Name { get; set; }
        public virtual string StrValue { get; set; }
        public virtual TimeSpan? CacheTimeout { get; set; }

        #endregion

        public virtual DateTime? CachedTime { get; set; }

        public virtual bool IsExpired {
            get {
                if(CacheTimeout == null) {
                    return true;
                }
                if(CachedTime == null) {
                    return false;
                }
                return DateTime.Now.Subtract(CachedTime.Value) > CacheTimeout;
            }
        }
    }
}
