using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoInOrderDocflowsStageView : WidgetViewBase<EdoInOrderDocflowsStageViewModel>
	{
		public EdoInOrderDocflowsStageView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			yentryCreationTime.Binding
				.AddBinding(ViewModel, vm => vm.CreationTime, w => w.Text)
				.InitializeFromSource();

			yentryStatus.Binding
				.AddBinding(ViewModel, vm => vm.Status, w => w.Text)
				.InitializeFromSource();

			ytreeviewTaxcomDocflows.ColumnsConfig = FluentColumnsConfig<EdoInOrderTaxcomDocflowViewModel>.Create()
				.AddColumn("Время отправки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.SendTime)
					.XAlign(0.5f)
				.AddColumn("Идентификатор в Такском")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TaxcomDocflowId)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Status)
					.XAlign(0.5f)
				.AddColumn("Последнее изменение")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.LastUpdateTime)
					.XAlign(0.5f)
				.AddColumn("Статус в ГИС МТ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TrueMarkTraceabilityStatus)
					.XAlign(0.5f)
				.AddColumn("Ошибка")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.ErrorMessage)
					.XAlign(0.5f)
				.Finish();
			ytreeviewTaxcomDocflows.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.TaxcomDocflows, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedTaxcomDocflow, w => w.SelectedRow)
				.InitializeFromSource();

			ybuttonRefreshFromTaxcom.Visible = false;
		}
	}
}
