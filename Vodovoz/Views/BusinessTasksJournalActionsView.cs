using System;
using Vodovoz.ViewModels;
using QS.Views;
using Vodovoz.Domain.BusinessTasks;
namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BusinessTasksJournalActionsView : ViewBase<BusinessTasksJournalActionsViewModel>
	{
		public BusinessTasksJournalActionsView(BusinessTasksJournalActionsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			btnAddClientTask.Clicked += (sender, e) => ViewModel.NewClientTaskCommand.Execute();
			btnAddPaymentTask.Clicked += (sender, e) => ViewModel.NewPaymentTaskCommand.Execute();
			//btnEdit.Clicked += (sender, e) => ViewModel.EditCommand.Execute();
			//btnDelete.Clicked += (sender, e) => ViewModel.DeleteCommand.Execute();
			btnChangeTasks.Clicked += (sender, e) => { HBoxChangeVisibility(); };
			btnChangeEmployee.Clicked += (sender, e) => entityVMEmployee.OpenSelectDialog("Ответственный :");
			btnCompleteSelected.Clicked += (sender, e) => ViewModel.CompleteSelectedTasksCommand.Execute();
			cmbBoxTaskStatus.ChangedByUser += (sender, e) => ViewModel.ChangeTasksStateCommand.Execute();
			entityVMEmployee.ChangedByUser += (sender, e) => ViewModel.ChangeAssignedEmployeeCommand.Execute();
			datepickerDeadlineChange.DateChangedByUser += (sender, e) => ViewModel.ChangeDeadlineDateCommand.Execute();

			//btnEdit.Binding.AddBinding(ViewModel, vm => vm.EditButtonSensitivity, v => v.Sensitive).InitializeFromSource();
			//btnDelete.Binding.AddBinding(ViewModel, vm => vm.DeleteButtonSensitivity, v => v.Sensitive).InitializeFromSource();
			cmbBoxTaskStatus.ItemsEnum = typeof(BusinessTaskStatus);
			cmbBoxTaskStatus.Binding.AddBinding(ViewModel, vm => vm.TaskStatus, v => v.SelectedItem).InitializeFromSource();
			datepickerDeadlineChange.Binding.AddBinding(ViewModel, vm => vm.DeadlineDate, v => v.Date).InitializeFromSource();
			entityVMEmployee.Binding.AddBinding(ViewModel, vm => vm.Employee, v => v.Subject).InitializeFromSource();
			entityVMEmployee.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
		}

		public void HBoxChangeVisibility() => hboxChangeTasks.Visible = ViewModel.HBoxChangeTasksVisibility = !ViewModel.HBoxChangeTasksVisibility;
	}
}
