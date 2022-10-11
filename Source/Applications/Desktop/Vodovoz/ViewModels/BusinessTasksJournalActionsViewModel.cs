using System;
using QS.Project.Journal;
using QS.Commands;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.BusinessTasks;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Filters.ViewModels;
using QS.DomainModel.UoW;
using QS.Project.Services;
using System.Linq;
using QS.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels
{
	public class BusinessTasksJournalActionsViewModel : WidgetViewModelBase//JournalActionsViewModel
	{
		public object[] SelectedObjs;

		private bool hBoxChangeTasksVisibility;
		public bool HBoxChangeTasksVisibility {
			get => hBoxChangeTasksVisibility;
			set => SetField(ref hBoxChangeTasksVisibility, value);
		}

		private BusinessTaskStatus taskStatus;
		public BusinessTaskStatus TaskStatus {
			get => taskStatus;
			set => SetField(ref taskStatus, value);
		}

		private DateTime deadlineDate;
		public DateTime DeadlineDate {
			get => deadlineDate;
			set => SetField(ref deadlineDate, value);
		}

		private Employee employee;
		public Employee Employee {
			get => employee;
			set => SetField(ref employee, value);
		}

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; private set; }

		public Action<object[], Employee> ChangeAssignedEmployeeAction { get; set; }
		public Action<object[]> CompleteSelectedTasksAction { get; set; }
		public Action<object[], BusinessTaskStatus> ChangeTasksStateAction { get; set; }
		public Action<object[], DateTime> ChangeDeadlineDateAction { get; set; }

		public BusinessTasksJournalActionsViewModel(IEmployeeJournalFactory employeeJournalFactory)
		{
			CreateCommands();

			EmployeeSelectorFactory = employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
		}

		private void CreateCommands()
		{
			CreateNewClientTaskCommand();
			CreateNewPaymentTaskCommand();
			CreateChangeDeadlineDateCommand();
			CreateChangeTasksStateCommand();
			CreateCompleteSelectedTasksCommand();
			CreateChangeAssignedEmployeeCommand();
		}

		public DelegateCommand NewClientTaskCommand { get; private set; }
		private void CreateNewClientTaskCommand()
		{
			/*NewClientTaskCommand = new DelegateCommand(
				() => JournalActions.SingleOrDefault(x => x.Title == title).ExecuteAction?.Invoke(SelectedObjs),
				() => true
			);*/
		}

		public DelegateCommand NewPaymentTaskCommand { get; private set; }
		private void CreateNewPaymentTaskCommand()
		{
			/*
			NewPaymentTaskCommand = new DelegateCommand(
				() => JournalActions.SingleOrDefault(x => x.Title == title).ExecuteAction?.Invoke(SelectedObjs),
				() => true
			);
			*/
		}

		public DelegateCommand ChangeDeadlineDateCommand { get; private set; }
		private void CreateChangeDeadlineDateCommand()
		{
			ChangeDeadlineDateCommand = new DelegateCommand(
				() => ChangeDeadlineDateAction?.Invoke(SelectedObjs, DeadlineDate),
				() => SelectedObjs.Any()
			);
		}

		public DelegateCommand ChangeTasksStateCommand { get; private set; }
		private void CreateChangeTasksStateCommand()
		{
			ChangeTasksStateCommand = new DelegateCommand(
				() => ChangeTasksStateAction?.Invoke(SelectedObjs, TaskStatus),
				() => SelectedObjs.Any()
			);
		}

		public DelegateCommand ChangeAssignedEmployeeCommand { get; private set; }
		private void CreateChangeAssignedEmployeeCommand()
		{
			ChangeAssignedEmployeeCommand = new DelegateCommand(
				() => ChangeAssignedEmployeeAction?.Invoke(SelectedObjs, Employee),
				() => SelectedObjs.Any()
			);
		}

		public DelegateCommand CompleteSelectedTasksCommand { get; private set; }
		private void CreateCompleteSelectedTasksCommand()
		{
			CompleteSelectedTasksCommand = new DelegateCommand(
				() => CompleteSelectedTasksAction?.Invoke(SelectedObjs),
				() => SelectedObjs.Any()
			);
		}
	}
}
