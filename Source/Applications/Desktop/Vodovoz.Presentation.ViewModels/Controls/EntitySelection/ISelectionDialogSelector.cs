using QS.DomainModel.Entity;
using System;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface ISelectionDialogSelector<TEntity>
		where TEntity : class, IDomainObject
	{
		void OpenSelector();
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
		event EventHandler SelectEntityFromJournalSelected;
	}
}
