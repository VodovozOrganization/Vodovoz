using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Dialogs.Counterparty;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CloseSupplyToCounterpartyView : WidgetViewBase<CounterpartyFilesViewModel>
	{
		public CloseSupplyToCounterpartyView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			if(ViewModel == null)
			{
				return;
			}


		}
	}
}
