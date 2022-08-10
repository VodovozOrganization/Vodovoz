using System.Collections.Generic;
using QS.HistoryLog.Domain;

namespace Vodovoz.Domain.HistoryChanges
{
	public class ArchiveChangedEntity : ChangedEntityBase
	{
		#region Свойства

		public virtual ArchiveChangeSet ChangeSet { get; set; }

		public virtual IList<ArchiveFieldChange> Changes { get; set; }

		#endregion
	}
}
