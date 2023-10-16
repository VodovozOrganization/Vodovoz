using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.BusinessTasks;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.BusinessTasks
{
	public class PaymentTaskViewModel : EntityTabViewModelBase<PaymentTask>
	{
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; private set; }
		public IEntityAutocompleteSelectorFactory OrderSelectorFactory { get; private set; }
		public IEntityAutocompleteSelectorFactory SubdivisionSelectorFactory { get; private set; }

		public readonly IEmployeeRepository employeeRepository;

		public PaymentTaskViewModel(
			IEmployeeRepository employeeRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICounterpartyJournalFactory counterpartyJournalFactory) : base(uowBuilder, unitOfWorkFactory, commonServices)
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

			if(counterpartyJournalFactory is null)
			{
				throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			}

			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

			Initialize();
			CreateCommands();
			CounterpartySelectorFactory = counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory();
		}

		private void Initialize()
		{
			EmployeeSelectorFactory = new EmployeeJournalFactory(NavigationManager).CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();

			OrderSelectorFactory = new DefaultEntityAutocompleteSelectorFactory<Order,
																				OrderJournalViewModel,
																				OrderJournalFilterViewModel>(CommonServices);
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
				() => Close(true, QS.Navigation.CloseSource.Cancel),
				() => true
			);
		}
	}
}
