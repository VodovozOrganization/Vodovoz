using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MovementWagonJournalFilterView : FilterViewBase<MovementWagonJournalFilterViewModel>
	{
		public MovementWagonJournalFilterView(MovementWagonJournalFilterViewModel movementWagonJournalFilterViewModel)
			: base(movementWagonJournalFilterViewModel)
		{
			this.Build();
		}
	}
}
