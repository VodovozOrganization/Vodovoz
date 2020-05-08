using System;
using QS.Project.Journal;
using QS.Commands;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.BusinessTasks;
using QS.Project.Journal.EntitySelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Filters.ViewModels;
using QS.DomainModel.UoW;
using QS.Project.Services;
using System.Linq;
using QS.Tdi;

namespace Vodovoz.ViewModels
{
	public class BusinessTasksJournalActionsViewModel : JournalActionsViewModel
	{
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

		public Action<object[], Employee> ChangeAssignedEmployeeAction;
		public Action<object[]> CompleteSelectedTasksAction;
		public Action<object[], BusinessTaskStatus> ChangeTasksStateAction;
		public Action<object[], DateTime> ChangeDeadlineDateAction;

		public BusinessTasksJournalActionsViewModel()
		{
			CreateCommands();

			EmployeeSelectorFactory =
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() => {
						var filter = new EmployeeFilterViewModel { Status = EmployeeStatus.IsWorking, RestrictCategory = EmployeeCategory.office };
						return new EmployeesJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
					});
		}

		private void CreateCommands()
		{
			CreateChangeDeadlineDateCommand();
			CreateChangeTasksStateCommand();
			CreateCompleteSelectedTasksCommand();
			CreateChangeAssignedEmployeeCommand();
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
