using System;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface IEntityJournalSelector
	{
		void OpenSelector(string dialogTitle = null);
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
	}
}
