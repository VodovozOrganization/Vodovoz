using QS.Views.GtkUI;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MissedCallDetailsView : WidgetViewBase<DashboardMissedCallDetailsViewModel>
	{
		public MissedCallDetailsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			textDetails.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Details, w => w.Buffer.Text)
				.InitializeFromSource();
		}
	}
}
