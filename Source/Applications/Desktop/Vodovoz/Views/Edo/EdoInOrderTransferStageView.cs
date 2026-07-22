using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoInOrderTransferStageView : WidgetViewBase<EdoInOrderTransferStageViewModel>
	{
		public EdoInOrderTransferStageView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			var monospacedFont = Pango.FontDescription.FromString("Consolas 9");

			ytreeviewTransferTasks.ColumnsConfig = FluentColumnsConfig<EdoInOrderTransferRowViewModel>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TimeString).Editable(false)
					.XAlign(0.5f)
				.AddColumn("Откуда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.From).Editable(false)
					.XAlign(0.5f)
				.AddColumn("Куда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.To).Editable(false)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TransferStatusString).Editable(false)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeviewTransferTasks.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Transfers, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedTransfer, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeviewTransferedCodes.ColumnsConfig = FluentColumnsConfig<string>.Create()
				.AddColumn("Перемещаемые коды")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x).Editable(false)
					.AddSetter((c, node) => {
						c.Xpad = 5;
						c.FontDesc = monospacedFont;
					})
					.XAlign(0f)
				.Finish();
			ytreeviewTransferedCodes.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.TransferedCodes, w => w.ItemsDataSource)
				.InitializeFromSource();

			pipelineTransferStages.PipelineVerticalPadding = 5;
			pipelineTransferStages.PipelineSidePadding = 10;
			pipelineTransferStages.HorizontalAlignment = 0f;
			pipelineTransferStages.VerticalAlignment = 0f;
			pipelineTransferStages.HeightRequest = 0;
			pipelineTransferStages.StageCircleRadius = 16;
			pipelineTransferStages.StageAdditionalInfoHeight = 14;
			pipelineTransferStages.TitleHeight = 12;
			pipelineTransferStages.TitleBottomSpacing = 4;
			pipelineTransferStages.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.PipelineViewModel, w => w.ViewModel)
				.InitializeFromSource();

			yhboxTransferContent.HeightRequest = 140;
		}
	}
}
