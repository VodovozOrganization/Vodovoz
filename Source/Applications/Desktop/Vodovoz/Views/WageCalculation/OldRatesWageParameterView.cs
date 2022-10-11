using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OldRatesWageParameterView : WidgetViewBase<OldRatesWageParameterItemViewModel>
	{
		public OldRatesWageParameterView(OldRatesWageParameterItemViewModel itemViewModel) : base(itemViewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
		}
	}
}
