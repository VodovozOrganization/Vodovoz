using System;
using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ResidueFilterView : FilterViewBase<ResidueFilterViewModel>
	{
		public ResidueFilterView(ResidueFilterViewModel residueFilterViewModel) : base(residueFilterViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			dateperiodOrders.Binding.AddBinding(ViewModel, x => x.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			dateperiodOrders.Binding.AddBinding(ViewModel, x => x.EndDate, w => w.EndDateOrNull).InitializeFromSource();
		}
	}
}
