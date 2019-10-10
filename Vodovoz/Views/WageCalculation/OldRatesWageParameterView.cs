using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OldRatesWageParameterView : EntityWidgetViewBase<OldRatesWageParameterViewModel>
	{
		public OldRatesWageParameterView(OldRatesWageParameterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
		}
	}
}
