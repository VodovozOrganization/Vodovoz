using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.BusinessTasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.JournalViewModels.Organization;
using Vodovoz.FilterViewModels.Organization;

namespace Vodovoz.ViewModels.BusinessTasks
{
	public class PaymentTaskViewModel : EntityTabViewModelBase<PaymentTask>
	{
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; private set; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; private set; }
		public IEntityAutocompleteSelectorFactory OrderSelectorFactory { get; private set; }
		public IEntityAutocompleteSelectorFactory SubdivisionSelectorFactory { get; private set; }

		public readonly IEmployeeRepository employeeRepository;

		public PaymentTaskViewModel(
			IEmployeeRepository employeeRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(uowBuilder.IsNewEntity) {
				TabName = "Новая задача";
				Entity.CreationDate = DateTime.Now;
				Entity.Source = Domain.BusinessTasks.TaskSource.Handmade;
				Entity.TaskCreator = employeeRepository.GetEmployeeForCurrentUser(UoW);
				Entity.EndActivePeriod = DateTime.Now;
			} else {
				TabName = Entity.Counterparty?.Name;
			}

			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

			Initialize();
			CreateCommands();
		}

		private void Initialize()
		{
			CounterpartySelectorFactory = new DefaultEntityAutocompleteSelectorFactory<Counterparty,
																						CounterpartyJournalViewModel,
																						CounterpartyJournalFilterViewModel>(CommonServices);

			EmployeeSelectorFactory =
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() => {
						var filter = new EmployeeFilterViewModel { Status = EmployeeStatus.IsWorking, RestrictCategory = EmployeeCategory.office };
						return new EmployeesJournalViewModel(filter, UnitOfWorkFactory, CommonServices);
					});

			OrderSelectorFactory = new DefaultEntityAutocompleteSelectorFactory<Order,
																				OrderJournalViewModel,
																				OrderJournalFilterViewModel>(CommonServices);
			/*
			SubdivisionSelectorFactory = new DefaultEntityAutocompleteSelectorFactory<Subdivision,
																						SubdivisionsJournalViewModel,
																						SubdivisionFilterViewModel>(CommonServices);
																						*/																					
		}

		private void CreateCommands()
		{
			CreateSaveCommand();
			CreateCancelCommand();
		}

		public DelegateCommand SaveCommand { get; private set; }
		private void CreateSaveCommand()
		{
			SaveCommand = new DelegateCommand(
				() => Save(true),
				() => true
			);
		}

		public DelegateCommand CancelCommand { get; private set; }
		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(
				() => Close(false, QS.Navigation.CloseSource.Cancel),
				() => true
			);
		}
	}
}
