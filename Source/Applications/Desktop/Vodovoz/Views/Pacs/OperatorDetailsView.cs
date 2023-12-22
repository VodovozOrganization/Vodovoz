using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Presentation.ViewModels.Pacs;
using Gamma.Utilities;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OperatorDetailsView : WidgetViewBase<DashboardOperatorDetailsViewModel>
	{
		public OperatorDetailsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			labelInfo.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Tittle, w => w.LabelProp)
				.InitializeFromSource();

			treeViewOperatorHistory.ColumnsConfig = FluentColumnsConfig<OperatorState>.Create()
				.AddColumn("Статус").AddReadOnlyTextRenderer(x => x.State.GetEnumTitle())
				.AddColumn("Начало").AddReadOnlyTextRenderer(x => x.Started.ToString("dd.MM HH:mm:ss"))
				.AddColumn("Конец").AddReadOnlyTextRenderer(x => x.Ended.HasValue ? x.Ended.Value.ToString("dd.MM HH:mm:ss") : "")
				.AddColumn("Доб. тел.").AddReadOnlyTextRenderer(x => x.PhoneNumber)
				.Finish();

			treeViewOperatorHistory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.States, w => w.ItemsDataSource)
				.InitializeFromSource();

			textviewBreakReason.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.BreakReason, w => w.Buffer.Text)
				.InitializeFromSource();

			/*buttonStartBreak.BindCommand(ViewModel.StartLongBreakCommand);
			buttonEndBreak.BindCommand(ViewModel.EndBreakCommand);*/
		}
	}
}
