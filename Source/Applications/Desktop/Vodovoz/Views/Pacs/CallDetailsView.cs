using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Presentation.ViewModels.Pacs;
using Gamma.Utilities;

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

			treeViewHistory.ColumnsConfig = FluentColumnsConfig<CallEvent>.Create()
				.AddColumn("Время").AddReadOnlyTextRenderer(x => x.EventTime.ToString("MM.dd HH:mm:ss"))
				.AddColumn("Статус").AddReadOnlyTextRenderer(x => x.CallState.GetEnumTitle())
				.AddColumn("От кого").AddReadOnlyTextRenderer(x => x.FromNumber)
				.AddColumn("Кому").AddReadOnlyTextRenderer(x => x.ToExtension)
				.AddColumn("Причина отключения").AddReadOnlyTextRenderer(x => x.DisconnectReason > 0 ? x.DisconnectReason.ToString() : "")
				.AddColumn("")
				.Finish();

			treeViewHistory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CallEvents, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
	}
}
