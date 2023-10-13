using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics
{
	public class DriversWarehousesEventsReportViewModel : DialogViewModelBase
	{
		private readonly ILifetimeScope _scope;
		private readonly IUnitOfWork _unitOfWork;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private Car _car;
		private DriverWarehouseEventName _startEventName;
		private DriverWarehouseEventName _endEventName;

		public DriversWarehousesEventsReportViewModel(
			ILifetimeScope scope,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation) : base(navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_unitOfWork = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot();
				
			ConfigureEntryViewModels();
		}
		
		public IEntityEntryViewModel StartEventNameViewModel { get; private set; }
		public IEntityEntryViewModel EndEventNameViewModel { get; private set; }
		public IEntityEntryViewModel DriverViewModel { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}
		
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}
		
		public DriverWarehouseEventName StartEventName
		{
			get => _startEventName;
			set => SetField(ref _startEventName, value);
		}
		
		public DriverWarehouseEventName EndEventName
		{
			get => _endEventName;
			set => SetField(ref _endEventName, value);
		}
		
		public Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}
		
		public Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}
		
		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<DriversWarehousesEventsReportViewModel>(
				this, this, _unitOfWork, NavigationManager, _scope);
			
			StartEventNameViewModel =
				builder.ForProperty(x => x.StartEventName)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsNamesJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventNameViewModel>()
					.Finish();
			
			EndEventNameViewModel =
				builder.ForProperty(x => x.EndEventName)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsNamesJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventNameViewModel>()
					.Finish();

			DriverViewModel =
				builder.ForProperty(x => x.Driver)
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();
			
			CarViewModel =
				builder.ForProperty(x => x.Car)
					.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
					.UseViewModelDialog<CarViewModel>()
					.Finish();
		}

		public void GenerateReport()
		{
			if(StartEventName is null || EndEventName is null)
			{
				return;
			}
			
			CompletedDriverWarehouseEvent completedEventAlias = null;
			DriverWarehouseEvent eventAlias = null;
			DriverWarehouseEventName eventNameAlias = null;
			Employee driverAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			DriversWarehousesEventsReportNode resultAlias = null;

			var eventNameIds = new int[] { StartEventName.Id, EndEventName.Id };

			var query = _unitOfWork.Session.QueryOver(() => completedEventAlias)
				.JoinAlias(() => completedEventAlias.DriverWarehouseEvent, () => eventAlias)
				.JoinAlias(() => eventAlias.EventName, () => eventNameAlias)
				.JoinAlias(() => completedEventAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => completedEventAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.WhereRestrictionOn(() => eventNameAlias.Id).IsIn(eventNameIds);

			if(Car != null)
			{
				query.And(() => completedEventAlias.Car.Id == Car.Id);
			}
			
			if(Driver != null)
			{
				query.And(() => completedEventAlias.Driver.Id == Driver.Id);
			}
			
			query.SelectList(list => list
				.Select(() => completedEventAlias.CompletedDate).WithAlias(() => resultAlias.Date)
				.Select())
		}
	}

	public class DriversWarehousesEventsReportNode
	{
		public DateTime Date { get; set; }
		public string DriverFio { get; set; }
		public string CarModelWithNumber { get; set; }
		public string FirstEventName { get; set; }
		public string SecondEventName { get; set; }
		public decimal FirstDistance { get; set; }
		public decimal SecondDistance { get; set; }
	}
}
