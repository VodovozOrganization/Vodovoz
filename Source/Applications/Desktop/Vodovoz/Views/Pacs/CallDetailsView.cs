using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallDetailsView : WidgetViewBase<DashboardCallDetailsViewModel>
	{
		public CallDetailsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			textDetails.Editable = false;
			textDetails.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DetailsInfo, w => w.Buffer.Text)
				.InitializeFromSource();

			treeViewHistory.ColumnsConfig = FluentColumnsConfig<SubCall>.Create()
				.AddColumn("Время").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.StartTime.ToString("MM.dd HH:mm:ss"))
				.AddColumn("От кого").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.FromNumber)
				.AddColumn("Кому").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.ToExtension)
				.AddColumn("Принят").HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.WasConnected).Editing(false)
				.AddColumn("")
				.Finish();

			treeViewHistory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CallDetails, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
	}
}
