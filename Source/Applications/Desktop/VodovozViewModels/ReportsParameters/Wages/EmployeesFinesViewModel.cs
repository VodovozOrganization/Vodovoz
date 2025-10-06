using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.Reports;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics;

namespace Vodovoz.ViewModels.ReportsParameters.Wages
{
	public class EmployeesFinesViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly EmployeeFilterViewModel _employeeFilter;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private bool _categoryDriver;
		private bool _categoryForwarder;
		private bool _categoryOffice;
		private List<FineTypes> _selectedFineTypes = new List<FineTypes>();

		public EmployeesFinesViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			EmployeeFilterViewModel employeeFilterViewModel,
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, uowFactory, reportInfoFactory, validator)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeFilter = employeeFilterViewModel ?? throw new ArgumentNullException(nameof(employeeFilterViewModel));

			Title = "Штрафы сотрудников";
			Identifier = "Employees.Fines";

			_employeeFilter.Status = EmployeeStatus.IsWorking;
			_employeeJournalFactory.SetEmployeeFilterViewModel(_employeeFilter);
			DriverSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
			AllStatusCommand = new DelegateCommand(AllStatus);
			NoneStatusCommand = new DelegateCommand(NoneStatus);

			FineCategories = new GenericObservableList<EmployeeFineCategoryNode>();

			foreach(var fine in Enum.GetValues(typeof(FineTypes)))
			{
				var fineNode = new EmployeeFineCategoryNode((FineTypes)fine) { Selected = true };
				FineCategories.Add(fineNode);
			}
		}

		public DelegateCommand GenerateReportCommand;
		public DelegateCommand AllStatusCommand;
		public DelegateCommand NoneStatusCommand;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		public virtual bool CategoryDriver
		{
			get => _categoryDriver;
			set
			{
				SetField(ref _categoryDriver, value);
				UpdateEmployeeCategoryFilter();
			}
		}

		public virtual bool CategoryForwarder
		{
			get => _categoryForwarder;
			set
			{
				SetField(ref _categoryForwarder, value);
				UpdateEmployeeCategoryFilter();
			}
		}

		public virtual bool CategoryOffice
		{
			get => _categoryOffice;
			set
			{
				SetField(ref _categoryOffice, value);
				UpdateEmployeeCategoryFilter();
			}
		}

		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }

		public GenericObservableList<EmployeeFineCategoryNode> FineCategories { get; private set; }

		public List<FineTypes> SelectedFineCategory
		{
			get => _selectedFineTypes;
			set => SetField(ref _selectedFineTypes, value);
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>();
				if(Driver != null)
				{
					parameters.Add("drivers", Driver.Id);
					parameters.Add("driverName", " " + Driver.FullName);
				}
				else
				{
					parameters.Add("drivers", -1);
				}
				if(StartDate != null && EndDate != null)
				{
					parameters.Add("startDate", StartDate);
					parameters.Add("endDate", EndDate);
				}
				else
				{
					parameters.Add("startDate", 0);
					parameters.Add("endDate", 0);
				}

				parameters.Add("showbottom", false);
				parameters.Add("routelist", 0);
				parameters.Add("category", GetCategory());

				parameters.Add("fineCategories", FineCategories.Any(x => x.Selected) 
					? string.Join(",", FineCategories.Where(x => x.Selected).Select(x => x.FineCategory)) 
					: "");

				return parameters;
			}
		}

		private void UpdateEmployeeCategoryFilter() 
		{
			if(CategoryDriver)
			{
				_employeeFilter.RestrictCategory = EmployeeCategory.driver;
			}

			if(CategoryForwarder)
			{
				_employeeFilter.RestrictCategory = EmployeeCategory.forwarder;
			}

			if(CategoryOffice)
			{
				_employeeFilter.RestrictCategory = EmployeeCategory.office;
			}

			if(!CategoryDriver && !CategoryForwarder && !CategoryOffice)
			{
				_employeeFilter.RestrictCategory = null;
			}
		}

		private string GetCategory()
		{
			string cat = "-1";

			if(CategoryDriver)
			{
				cat = EmployeeCategory.driver.ToString();
			}
			else if(CategoryForwarder)
			{
				cat = EmployeeCategory.forwarder.ToString();
			}
			else if(CategoryOffice)
			{
				cat = EmployeeCategory.office.ToString();
			}

			return cat;
		}

		private void NoneStatus()
		{
			foreach(var fineCategory in FineCategories)
			{
				var item = fineCategory;
				item.Selected = false;
			}
		}

		private void AllStatus()
		{
			foreach(var fineCategory in FineCategories)
			{
				var item = fineCategory;
				item.Selected = true;
			}
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var datePeriodSelected = StartDate != null && EndDate != null;
			var driverSelected = Driver != null;
			if (!datePeriodSelected && !driverSelected)
			{
				yield return new ValidationResult("Необходимо выбрать период или сотрудника.", new[] { nameof(StartDate), nameof(EndDate), nameof(Driver) });
			}
			
		}
	}
}
