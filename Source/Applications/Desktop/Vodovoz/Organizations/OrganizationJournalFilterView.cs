using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.Organizations
{
	[ToolboxItem(true)]
	public partial class OrganizationJournalFilterView : FilterViewBase<OrganizationJournalFilterViewModel>
	{
		public OrganizationJournalFilterView(OrganizationJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
