using System;
using System.Collections.Generic;
using NHibernate.Proxy;
using QS.DomainModel.Entity;
using QS.HistoryLog.Domain;

namespace Vodovoz.Domain.HistoryChanges
{
	public class OldChangedEntity : ChangedEntityBase
	{
		#region Свойства

		public virtual OldChangeSet ChangeSet { get; set; }

		public virtual IList<OldFieldChange> Changes { get; set; }

		#endregion

		public OldChangedEntity() { }

		public OldChangedEntity(EntityChangeOperation operation, object entity, List<OldFieldChange> changes)
		{
			Operation = operation;
			var type = NHibernateProxyHelper.GuessClass(entity);

			EntityClassName = type.Name;
			EntityTitle = DomainHelper.GetTitle(entity);
			//Обрезаем так как в базе данных поле равно 200.
			if(EntityTitle != null && EntityTitle.Length > 200)
			{
				EntityTitle = EntityTitle.Substring(0, 197) + "...";
			}

			EntityId = DomainHelper.GetId(entity);
			ChangeTime = DateTime.Now;

			changes.ForEach(f => f.Entity = this);
			Changes = changes;
		}

		public virtual void AddFieldChange(OldFieldChange change)
		{
			change.Entity = this;
			Changes.Add(change);
		}
	}
}