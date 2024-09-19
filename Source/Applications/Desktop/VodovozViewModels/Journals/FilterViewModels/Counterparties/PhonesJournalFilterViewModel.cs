using QS.DomainModel.Entity;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties
{
	public class PhonesJournalFilterViewModel : FilterViewModelBase<PhonesJournalFilterViewModel>
	{
		private readonly ViewModelEEVMBuilder<Employee> _employeeViewModelEEVMBuilder;
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private bool _showArchive;
		private Employee _restrictEmployee;
		private JournalViewModelBase _journal;

		public PhonesJournalFilterViewModel(ViewModelEEVMBuilder<Employee> employeeViewModelEEVMBuilder)
		{
			_employeeViewModelEEVMBuilder = employeeViewModelEEVMBuilder ?? throw new System.ArgumentNullException(nameof(employeeViewModelEEVMBuilder));
		}

		public Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		public DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		public bool ShowArchive
		{
			get => _showArchive;
			set => SetField(ref _showArchive, value);
		}

		public Employee Employee
		{
			get => _restrictEmployee;
			set => SetField(ref _restrictEmployee, value);
		}

		[PropertyChangedAlso(nameof(CanChangeEmployee))]
		public Employee RestrictEmployee
		{
			get => _restrictEmployee;
			set
			{
				if(SetField(ref _restrictEmployee, value))
				{
					if(RestrictEmployee != null)
					{
						Employee = RestrictEmployee;
					}
				}
			}
		}

		public JournalViewModelBase Journal
		{
			get => _journal;
			set
			{
				_journal = value;

				EmployeeViewModel = _employeeViewModelEEVMBuilder
					.SetUnitOfWork(_journal.UoW)
					.SetViewModel(_journal)
					.ForProperty(this, x => x.Employee)
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();
			}
		}

		public bool CanChangeEmployee => RestrictEmployee != null;

		public IEntityEntryViewModel EmployeeViewModel { get; set; }
	}
}
