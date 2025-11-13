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
			Configure();
		}

		private void Configure()
		{
			chkHasAvangardShopId.Binding
				.AddBinding(ViewModel, vm => vm.HasAvangardShopId, w => w.Active)
				.InitializeFromSource();

			chkHasCashBoxId.Binding
				.AddBinding(ViewModel, vm => vm.HasCashBoxId, w => w.Active)
				.InitializeFromSource();

			chkHasTaxcomEdoAccountId.Binding
				.AddBinding(ViewModel, vm => vm.HasTaxcomEdoAccountId, w => w.Active)
				.InitializeFromSource();
		}
	}
}
