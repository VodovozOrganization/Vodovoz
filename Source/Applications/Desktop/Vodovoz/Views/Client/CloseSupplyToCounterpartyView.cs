using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Dialogs.Counterparty;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CloseSupplyToCounterpartyView : TabViewBase<CloseSupplyToCounterpartyViewModel>
	{
		public CloseSupplyToCounterpartyView(CloseSupplyToCounterpartyViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		protected void ConfigureDlg()
		{
			if(ViewModel == null)
			{
				return;
			}


		}
	}
}
