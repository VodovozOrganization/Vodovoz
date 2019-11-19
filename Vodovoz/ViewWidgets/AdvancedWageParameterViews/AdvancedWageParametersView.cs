using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.ViewWidgets.AdvancedWageParameterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AdvancedWageParametersView : TabViewBase<AdvancedWageParametersViewModel>
	{
		public AdvancedWageParametersView(AdvancedWageParametersViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
