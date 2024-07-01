using Gdk;
using Gtk;
using QS.Utilities.Text;
using QS.Views.Dialog;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Counterparties;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Counterparties
{
	[ToolboxItem(true)]
	public partial class CallTaskMassEditView : DialogViewBase<CallTaskMassEditViewModel>
	{
		private static readonly Pixbuf _emptyImg = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.empty16.png");
		private static readonly Pixbuf _fire = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.fire16.png");

		public CallTaskMassEditView(CallTaskMassEditViewModel viewModel)
			: base(viewModel)
		{
			Build();
			ConfigureTreeView();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseCommand);

			entityentryAttachedEmployee.ViewModel = ViewModel.AttachedEmployeeViewModel;

			ComboBoxCallTaskStatus.ItemsEnum = typeof(CallTaskStatus);
			ComboBoxCallTaskStatus.ShowSpecialStateNot = true;
			ComboBoxCallTaskStatus.Binding
				.AddBinding(ViewModel, vm => vm.CallTaskStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			IsTaskCompleteButton.Binding
				.AddBinding(ViewModel, vm => vm.IsTaskComplete, w => w.Active)
				.InitializeFromSource();

			ydatepickerEndActivePeriod.Binding
				.AddBinding(ViewModel, vm => vm.EndActivePeriod, w => w.DateOrNull)
				.InitializeFromSource();

			buttonResetAssignedEmployee.BindCommand(ViewModel.ResetChangeAssignedEmployeeCommand);
			buttonSaveResetCallTaskStatus.BindCommand(ViewModel.ResetCallTaskStatusCommand);
			buttonResetIsTaskComplete.BindCommand(ViewModel.ResetIsTaskCompleteCommand);
			buttonResetEndActivePeriod.BindCommand(ViewModel.ResetEndActivePeriodCommand);

			buttonShowInformation.BindCommand(ViewModel.ShowInformationCommand);
		}

		private void ConfigureTreeView()
		{
			ytreeviewTasks.CreateFluentColumnsConfig<CallTask>()
				.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Срочность").AddPixbufRenderer(node =>
					node.ImportanceDegree == ImportanceDegreeType.Important
						&& !node.IsTaskComplete
					? _fire
					: _emptyImg)
				.AddColumn("Статус").AddEnumRenderer(node => node.TaskState)
				.AddColumn("Клиент").AddTextRenderer(node =>
					node.Counterparty != null
					? node.Counterparty.Name
					: "").WrapWidth(500).WrapMode(WrapMode.WordChar)
				.AddColumn("Адрес").AddTextRenderer(node =>
					node.DeliveryPoint != null
					? node.DeliveryPoint.ShortAddress
					: "Самовывоз").WrapWidth(500).WrapMode(WrapMode.WordChar)
				.AddColumn("Телефоны").AddTextRenderer(node =>
					node.DeliveryPoint != null
					&& node.DeliveryPoint.Phones != null
					&& node.DeliveryPoint.Phones.Any()
					? string.Join("\n", node.DeliveryPoint.Phones.Select(p => $"+7{p.DigitsNumber}"))
					: "")
			.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Ответственный").AddTextRenderer(node => node.AssignedEmployee != null 
					? PersonHelper.PersonNameWithInitials(node.AssignedEmployee.LastName, node.AssignedEmployee.Name, node.AssignedEmployee.Patronymic)
					: "")
				.AddColumn("Выполнить до").AddTextRenderer(node => node.EndActivePeriod.ToString("dd / MM / yyyy  HH:mm"))
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					var color = GdkColors.PrimaryText;

					if(n.IsTaskComplete)
					{
						color = GdkColors.SuccessText;
					}

					if(DateTime.Now > n.EndActivePeriod)
					{
						color = GdkColors.DangerText;
					}

					c.ForegroundGdk = color;
				})
				.Finish();

			ytreeviewTasks.Binding
				.AddBinding(ViewModel, vm => vm.Tasks, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
	}
}
