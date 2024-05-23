using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[ToolboxItem(true)]
	public partial class OperatorDetailsView : WidgetViewBase<DashboardOperatorDetailsViewModel>
	{
		public OperatorDetailsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			labelInfo.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Title, w => w.LabelProp)
				.InitializeFromSource();

			treeViewOperatorHistory.ColumnsConfig = FluentColumnsConfig<OperatorState>.Create()
				.AddColumn("Статус").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.State.GetEnumTitle())
				.AddColumn("Начало").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Started.ToString("dd.MM HH:mm:ss"))
				.AddColumn("Конец").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Ended.HasValue ? x.Ended.Value.ToString("dd.MM HH:mm:ss") : "")
				.AddColumn("Доб. тел.").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.PhoneNumber)
				.AddColumn("")
				.Finish();

			treeViewOperatorHistory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.States, w => w.ItemsDataSource)
				.InitializeFromSource();

			textviewBreakReason.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.BreakReason, w => w.Buffer.Text)
				.InitializeFromSource();

			buttonStartLongBreak.BindCommand(ViewModel.StartLongBreakCommand);
			buttonStartShortBreak.BindCommand(ViewModel.StartShortBreakCommand);
			buttonEndBreak.BindCommand(ViewModel.EndBreakCommand);

			textviewWorkShiftEndReason.Binding
				.AddBinding(ViewModel, vm => vm.EndWorkShiftReason, w => w.Buffer.Text)
				.InitializeFromSource();

			ybuttonEndWorkshift.BindCommand(ViewModel.EndWorkShiftCommand);
		}
	}
}
