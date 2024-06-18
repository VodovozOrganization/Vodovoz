using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[ToolboxItem(true)]
	public partial class PacsOperatorView : WidgetViewBase<PacsOperatorViewModel>
	{
		public PacsOperatorView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			labelOperatorId.Binding
				.AddBinding(ViewModel, vm => vm.CurrentOperatorId, w => w.LabelProp)
				.InitializeFromSource();

			labelWorkshiftId.Binding
				.AddBinding(ViewModel, vm => vm.WorkShiftId, w => w.LabelProp)
				.InitializeFromSource();

			labelOperatorStatus.Binding
				.AddBinding(ViewModel, vm => vm.CurrentOperatorStatus, w => w.LabelProp)
				.InitializeFromSource();

			comboboxPhone.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.AvailablePhones, w => w.ItemsList)
				.AddBinding(vm => vm.PhoneNumber, w => w.SelectedItem)
				.InitializeFromSource();

			labelShortBreaksUsed.Binding
				.AddBinding(ViewModel, vm => vm.ShortBreaksUsedCount, w => w.LabelProp)
				.InitializeFromSource();

			// Временно скрыто, до доработки с количеством использованных перерывов
			labelShortBreaksUsed.Visible = false;

			labelBreakInfo.Binding
				.AddBinding(ViewModel, vm => vm.BreakInfo, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.ShowBreakInfo, w => w.Visible)
				.InitializeFromSource();

			treeviewOperatorsOnBreak.ColumnsConfig = FluentColumnsConfig<DashboardOperatorOnBreakViewModel>.Create()
				.AddColumn("Перерыв").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Break)
				.AddColumn("Имя").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Name)
				.AddColumn("Доб. тел.").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.Phone)
				.AddColumn("Осталось").HeaderAlignment(0.5f)
					.AddReadOnlyTextRenderer(x => x.TimeRemains)
				.RowCells()
					.AddSetter<CellRenderer>((cell, vm) =>
					{
						if(vm.BreakTimeGone)
						{
							cell.CellBackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							cell.CellBackgroundGdk = GdkColors.PrimaryBG;
						}
					})
				.Finish();

			treeviewOperatorsOnBreak.ColumnsAutosize();

			treeviewOperatorsOnBreak.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.OperatorsOnBreak, w => w.ItemsDataSource)
				.InitializeFromSource();

			vboxWorkshiftReason.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.EndWorkShiftReasonRequired, w => w.Visible)
				.InitializeFromSource();

			textWorkshiftReason.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.EndWorkShiftReason, w => w.Buffer.Text)
				.InitializeFromSource();

			frameHelp.Visible = false;

			buttonLongBreak.BindCommand(ViewModel.StartLongBreakCommand);
			buttonShortBreak.BindCommand(ViewModel.StartShortBreakCommand);
			buttonEndBreak.BindCommand(ViewModel.EndBreakCommand);
			buttonChangePhone.BindCommand(ViewModel.ChangePhoneCommand);
			buttonStartWorkshift.BindCommand(ViewModel.StartWorkShiftCommand);
			buttonEndWorkshift.BindCommand(ViewModel.EndWorkShiftCommand);
			buttonWorkshiftReasonOk.BindCommand(ViewModel.EndWorkShiftCommand);
			buttonWorkshiftReasonCancel.BindCommand(ViewModel.CancelEndWorkShiftReasonCommand);
		}
	}
}
