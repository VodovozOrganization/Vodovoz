using System.Collections.Generic;
using QS.HistoryLog.Domain;
using QS.Project.Domain;

namespace Vodovoz.Domain.HistoryChanges
{
	public class ArchivedChangeSet : ChangeSetBase
	{
		public virtual IList<ArchivedChangedEntity> Entities { get; set; } = new List<ArchivedChangedEntity>();
		public virtual UserBase User { get; set; }
		
		public virtual string UserName {
			get {
				return User?.Name ?? UserLogin;
			}
		}
	}
}
