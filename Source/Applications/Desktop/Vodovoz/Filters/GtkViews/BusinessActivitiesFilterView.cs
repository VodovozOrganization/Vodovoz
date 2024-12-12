using System;
using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.Presentation.ViewModels.Organisations.Journals;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class BusinessActivitiesFilterView : FilterViewBase<BusinessActivitiesFilterViewModel>
	{
		public BusinessActivitiesFilterView(BusinessActivitiesFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			chkShowArchived.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchived, w => w.Active)
				.InitializeFromSource();
		}
	}
}
