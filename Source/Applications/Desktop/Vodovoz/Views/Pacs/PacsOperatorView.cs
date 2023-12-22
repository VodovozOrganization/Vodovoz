using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsOperatorView : WidgetViewBase<PacsOperatorViewModel>
	{
		public PacsOperatorView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			comboboxPhone.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.AvailablePhones, w => w.ItemsList)
				.AddBinding(vm => vm.PhoneNumber, w => w.SelectedItem)
				.InitializeFromSource();
			/*
			buttonBreak.Binding
				.AddBinding(ViewModel, vm => vm.CanStartBreak, w => w.Visible)
				.InitializeFromSource();

			buttonEndBreak.Binding
				.AddBinding(ViewModel, vm => vm.CanEndBreak, w => w.Visible)
				.InitializeFromSource();

			buttonStartWorkshift.Binding
				.AddBinding(ViewModel, vm => vm.CanStartWorkShift, w => w.Visible)
				.InitializeFromSource();

			buttonEndWorkshift.Binding
				.AddBinding(ViewModel, vm => vm.CanEndWorkShift, w => w.Visible)
				.InitializeFromSource();

			buttonChangePhone.Binding
				.AddBinding(ViewModel, vm => vm.CanChangePhone, w => w.Visible)
				.InitializeFromSource();*/

			labelBreakInfo.Binding
				.AddBinding(ViewModel, vm => vm.BreakInfo, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.HasBreakInfo, w => w.Visible)
				.InitializeFromSource();

			treeviewOperatorsOnBreak.ColumnsConfig = FluentColumnsConfig<DashboardOperatorOnBreakViewModel>.Create()
				.AddColumn("Перерыв").AddReadOnlyTextRenderer(x => x.Break)
				.AddColumn("Имя").AddReadOnlyTextRenderer(x => x.Name)
				.AddColumn("Доб. тел.").AddReadOnlyTextRenderer(x => x.Phone)
				.AddColumn("Осталось").AddReadOnlyTextRenderer(x => x.TimeRemains)
				.AddColumn("")
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
			treeviewOperatorsOnBreak.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OperatorsOnBreak, w => w.ItemsDataSource)
				.InitializeFromSource();

			vboxWorkshiftReason.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.EndWorkShiftReasonRequired, w => w.Visible)
				.InitializeFromSource();

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
