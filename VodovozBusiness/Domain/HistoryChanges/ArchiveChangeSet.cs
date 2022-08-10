using System.Collections.Generic;
using QS.HistoryLog.Domain;
using QS.Project.Domain;

namespace Vodovoz.Domain.HistoryChanges
{
	public class ArchiveChangeSet : ChangeSetBase
	{
		public virtual IList<ArchiveChangedEntity> Entities { get; set; } = new List<ArchiveChangedEntity>();
		public virtual UserBase User { get; set; }
		
		public virtual string UserName {
			get {
				return User?.Name ?? UserLogin;
			}
		}
	}
}
