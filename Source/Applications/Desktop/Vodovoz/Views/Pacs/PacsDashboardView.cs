using QS.Views.GtkUI;
using System;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsDashboardView : WidgetViewBase<PacsDashboardViewModel>
	{
		public PacsDashboardView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();
			
			//treeViewAllCalls
			//treeViewMissedCalls
			//treeViewOperatorsOnBreak
			//treeViewOperatorsOnWorkshift

			//ДЕТАЛИ

			//ФИЛЬТР

			//АКТИВАЦИЯ СТРОК

			//buttonRefresh.BindCommand();
		}
	}
}
