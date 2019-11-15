using System;
using System.Collections;
using Gamma.Binding;
using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParametersViewModel;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AdvancedWageParameterListView : TabViewBase<AdvancedWageParameterListViewModel>
	{
		public AdvancedWageParameterListView(AdvancedWageParameterListViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytreeviewWageParam.YTreeModel = new RecursiveTreeConfig<AdvancedWageParameter>
								(x => x.ParentParameter, x => x.ChildParameters)
								.CreateModel(ViewModel.RootParameters as IList);
		}
	}
}
