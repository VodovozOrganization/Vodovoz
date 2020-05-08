using System;
using QS.Project.Journal;
using QS.Commands;
using Vodovoz.ViewModels.BusinessTasks;
using QS.Project.Domain;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories;
using QS.Project.Services;
using Vodovoz.JournalNodes;
using Vodovoz.Domain.BusinessTasks;
using System.Collections.Generic;
using Vodovoz.JournalViewModels;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels
{
	public class BusinessTasksJournalActionsViewModel : JournalActionsViewModel
	{
		readonly IEmployeeRepository employeeRepository;
		readonly IBottlesRepository bottleRepository;
		readonly ICallTaskRepository callTaskRepository;
		readonly IPhoneRepository phoneRepository;

		BusinessTasksJournalViewModel ViewModel { get; set; }
		public BusinessTaskJournalNode SelectedTask { get; set; }
		public List<BusinessTaskJournalNode> SelectedTasks { get; set; }

		private bool hBoxChangeTasksVisibility;
		public bool HBoxChangeTasksVisibility {
			get => hBoxChangeTasksVisibility;
			set => SetField(ref hBoxChangeTasksVisibility, value);
		}

		private Employee employee;
		public Employee Employee {
			get => employee;
			set => SetField(ref employee, value);
		}

		public BusinessTasksJournalActionsViewModel()
		{
			CreateCommands();
		}

		private void CreateCommands()
		{
			CreateClientTaskCommand();
			CreatePaymentTaskCommand();
			CreateEditTaskCommand();
			CreateChangeTasksCommand();
		}

		public DelegateCommand CreateNewClientTaskCommand { get; private set; }
		private void CreateClientTaskCommand()
		{
			CreateNewClientTaskCommand = new DelegateCommand(
				() => {
					var clientTaskVM = new ClientTaskViewModel(
						employeeRepository,
						bottleRepository,
						callTaskRepository,
						phoneRepository,
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices
					);
					ViewModel.TabParent.AddTab(clientTaskVM, ViewModel, false);
				},
				() => true
			);
		}

		public DelegateCommand CreateNewPaymentTaskCommand { get; private set; }
		private void CreatePaymentTaskCommand()
		{
			CreateNewPaymentTaskCommand = new DelegateCommand(
				() => {
					var paymentTaskVM = new ClientTaskViewModel(
						employeeRepository,
						bottleRepository,
						callTaskRepository,
						phoneRepository,
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices
					);
					ViewModel.TabParent.AddTab(paymentTaskVM, ViewModel, false);
				},
				() => true
			);
		}

		public DelegateCommand EditTaskCommand { get; private set; }
		private void CreateEditTaskCommand()
		{
			EditTaskCommand = new DelegateCommand(
				() => {

					if(SelectedTask.NodeType == typeof(ClientTask)) {
						var clientTaskVM = new ClientTaskViewModel(
								employeeRepository,
								bottleRepository,
								callTaskRepository,
								phoneRepository,
								EntityUoWBuilder.ForOpen(SelectedTask.Id),
								UnitOfWorkFactory.GetDefaultFactory,
								ServicesConfig.CommonServices);
						ViewModel.TabParent.AddTab(clientTaskVM, ViewModel, false);

					} else if(SelectedTask.NodeType == typeof(PaymentTask)) {
						var paymentTaskVM = new ClientTaskViewModel(
								employeeRepository,
								bottleRepository,
								callTaskRepository,
								phoneRepository,
								EntityUoWBuilder.ForOpen(SelectedTask.Id),
								UnitOfWorkFactory.GetDefaultFactory,
								ServicesConfig.CommonServices);
						ViewModel.TabParent.AddTab(paymentTaskVM, ViewModel, false);
					}
				},
				() => SelectedTask != null
			);
		}

		public DelegateCommand ChangeTasksCommand { get; private set; }
		private void CreateChangeTasksCommand()
		{
			ChangeTasksCommand = new DelegateCommand(
				() => HBoxChangeTasksVisibility = !HBoxChangeTasksVisibility,
				() => true
			);
		}

		public DelegateCommand ChangeAssignedEmployeeCommand { get; private set; }
		private void CreateChangeAssignedEmployeeCommand()
		{
			ChangeAssignedEmployeeCommand = new DelegateCommand(
				() => HBoxChangeTasksVisibility = !HBoxChangeTasksVisibility,
				() => true
			);
		}

		public DelegateCommand CompleteTaskCommand { get; private set; }
		private void CreateCompleteTaskCommand()
		{
			CompleteTaskCommand = new DelegateCommand(
				() => HBoxChangeTasksVisibility = !HBoxChangeTasksVisibility,
				() => true
			);
		}

		/*private void ChangeEnitity(Action<CallTask> action, CallTaskVMNode[] tasks)
		{
			if(action == null)
				return;

			tasks.ToList().ForEach((taskNode) => {
				CallTask task = UoW.GetById<CallTask>(taskNode.Id);
				action(task);
				UoW.Save(task);
				UoW.Commit();
			});
		}
		*/
	}
}
