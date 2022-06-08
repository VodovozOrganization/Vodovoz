using System.Collections.Generic;
using QS.HistoryLog.Domain;
using QS.Project.Domain;

namespace Vodovoz.Domain.HistoryChanges
{
	public class OldChangeSet : ChangeSetBase
	{
		public virtual IList<OldChangedEntity> Entities { get; set; } = new List<OldChangedEntity>();

		public OldChangeSet() { }

		public OldChangeSet(string actionName, UserBase user = null, string login = null)
		{
			SetActionAndUserProperties(actionName, user, login);
		}

		public virtual void AddChange(params OldChangedEntity[] changes)
		{
			foreach(var entity in changes)
			{
				entity.ChangeSet = this;
				Entities.Add(entity);
			}
		}
	}
}
