using System.Collections.Generic;
using QS.HistoryLog.Domain;

namespace Vodovoz.Domain.HistoryChanges
{
	public class ArchivedChangedEntity : ChangedEntityBase
	{
		#region Свойства

		public virtual ArchivedChangeSet ChangeSet { get; set; }

		public virtual IList<ArchivedFieldChange> Changes { get; set; }

		#endregion
	}
}
