using Vodovoz.Domain.BusinessTasks;
using Vodovoz.Views.Comments;
using Vodovoz.ViewModels.BusinessTasks;
using QS.Views.GtkUI;
using QSWidgetLib;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Infrastructure.Converters;

namespace Vodovoz.Views.BusinessTasks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ClientTaskView : TabViewBase<ClientTaskViewModel>
	{
		readonly IUnitOfWork uow;

		public ClientTaskView(ClientTaskViewModel viewModel) : base(viewModel)
		{
			this.Build();
			uow = ViewModel.UoW;
			Configure();
		}

		private void Configure()
		{
			buttonSave.Clicked += (sender, e) => ViewModel.SaveCommand.Execute();
			buttonCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();
			buttonReportByDP.Clicked += (sender, e) => ViewModel.OpenReportByDPCommand.Execute();
			buttonReportByClient.Clicked += (sender, e) => ViewModel.OpenReportByClientCommand.Execute();
			createTaskButton.Clicked += (sender, e) => ViewModel.CreateNewTaskCommand.Execute();
			createOrderButton.Clicked += (sender, e) => ViewModel.CreateNewOrderCommand.Execute();

			buttonReportByClient.Binding.AddFuncBinding(ViewModel.Entity, vm => vm.Counterparty != null, v => v.Sensitive).InitializeFromSource();
			buttonReportByDP.Binding.AddFuncBinding(ViewModel.Entity, vm => vm.DeliveryPoint != null, v => v.Sensitive).InitializeFromSource();
			createTaskButton.Binding.AddBinding(ViewModel, vm => vm.TaskButtonVisibility, v => v.Sensitive).InitializeFromSource();
			debtByClientEntry.Binding.AddBinding(ViewModel, vm => vm.DebtByClient, v => v.Text).InitializeFromSource();
			debtByAddressEntry.Binding.AddBinding(ViewModel, vm => vm.DebtByAddress, v => v.Text).InitializeFromSource();
			entryReserve.Binding.AddBinding(ViewModel, vm => vm.BottleReserve, v => v.Text).InitializeFromSource();

			labelCreator.Binding.AddFuncBinding(ViewModel.Entity, 
												vm => vm.TaskCreator == null ? "" : vm.TaskCreator.ShortName, 
												v => v.Text).InitializeFromSource();

			ytextviewOldComments.Binding.AddBinding(ViewModel, vm => vm.OldComments, v => v.Buffer.Text).InitializeFromSource();

			comboboxImpotanceType.ItemsEnum = typeof(ImportanceDegreeType);
			comboboxImpotanceType.Binding.AddBinding(ViewModel.Entity, s => s.ImportanceDegree, w => w.SelectedItemOrNull).InitializeFromSource();
			TaskStateComboBox.ItemsEnum = typeof(BusinessTaskStatus);
			TaskStateComboBox.Binding.AddBinding(ViewModel.Entity, s => s.TaskState, w => w.SelectedItemOrNull).InitializeFromSource();
			IsTaskCompleteButton.Binding.AddBinding(ViewModel.Entity, s => s.IsTaskComplete, w => w.Active).InitializeFromSource();
			IsTaskCompleteButton.Label += ViewModel.Entity.CompleteDate?.ToString("dd / MM / yyyy  HH:mm");
			deadlineYdatepicker.Binding.AddBinding(ViewModel.Entity, s => s.EndActivePeriod, w => w.Date).InitializeFromSource();
			yentryTareReturn.ValidationMode = ValidationType.numeric;
			yentryTareReturn.Binding.AddBinding(ViewModel.Entity, s => s.TareReturn, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			employeeViewModelEntry.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			employeeViewModelEntry.CanEditReference = true;
			employeeViewModelEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.AssignedEmployee, v => v.Subject).InitializeFromSource();

			deliveryPointVMEntry.SetEntityAutocompleteSelectorFactory(ViewModel.DeliveryPointFactory);
			deliveryPointVMEntry.CanEditReference = true;
			deliveryPointVMEntry.Binding.AddBinding(ViewModel.Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();
			deliveryPointVMEntry.ChangedByUser += ViewModel.OnDeliveryPointVMEntryChangedByUser;

			counterpartyViewModelEntry.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			counterpartyViewModelEntry.CanEditReference = true;
			counterpartyViewModelEntry.Binding.AddBinding(ViewModel.Entity, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();
			counterpartyViewModelEntry.ChangedByUser += ViewModel.OnCounterpartyViewModelEntryChangedByUser;

			ClientPhonesView.ViewModel = ViewModel.ClientPhonesVM;
			DeliveryPointPhonesView.ViewModel = ViewModel.DeliveryPointPhonesVM;

			var docCommentView = new DocumentCommentView(ViewModel.Entity, ViewModel.employeeRepository, uow);
			vboxComments.Add(docCommentView);
			docCommentView.Show();
		}

		private void OnButtonSplitClicked(object sender, EventArgs e)
		{
			vboxOldComments.Visible = !vboxOldComments.Visible;
			buttonSplit.Label = vboxOldComments.Visible ? ">>" : "<<";
		}
	}
}
