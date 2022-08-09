using System.Collections.Generic;
using QS.HistoryLog.Domain;

namespace Vodovoz.Domain.HistoryChanges
{
	public class OldChangeSet : ChangeSetBase
	{
		public virtual IList<OldChangedEntity> Entities { get; set; } = new List<OldChangedEntity>();
		public virtual UserForOldMonitoring User { get; set; }
		
		public virtual string UserName {
			get {
				return User?.Name ?? UserLogin;
			}
		}
	}
}
