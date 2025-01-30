using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.Organizations
{
	[ToolboxItem(true)]
	public partial class AccountJournalFilterView : FilterViewBase<AccountJournalFilterViewModel>
	{
		public AccountJournalFilterView(AccountJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
