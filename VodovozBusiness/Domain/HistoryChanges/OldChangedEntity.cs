using System.Collections.Generic;
using QS.HistoryLog.Domain;

namespace Vodovoz.Domain.HistoryChanges
{
	public class OldChangedEntity : ChangedEntityBase
	{
		#region Свойства

		public virtual OldChangeSet ChangeSet { get; set; }

		public virtual IList<OldFieldChange> Changes { get; set; }

		#endregion
	}
}
