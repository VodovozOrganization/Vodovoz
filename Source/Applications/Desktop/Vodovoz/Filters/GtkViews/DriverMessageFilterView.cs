using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels;

namespace Vodovoz.Filters.GtkViews
{
	public partial class DriverMessageFilterView : FilterViewBase<DriverMessageFilterViewModel>
	{
		public DriverMessageFilterView(DriverMessageFilterViewModel driverMessageFilterViewModel) : base(driverMessageFilterViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			daterangepicker.Binding.AddBinding(ViewModel, x => x.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			daterangepicker.Binding.AddBinding(ViewModel, x => x.EndDate, w => w.EndDateOrNull).InitializeFromSource();
		}
	}
}
