using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ViewModels.ReportsParameters
{
	public class OrderCreationDateReportViewModel : ReportParametersViewModelBase, IDisposable
	{		
		private DateTime? _startDate;
		private DateTime? _endDate;

		public OrderCreationDateReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IEmployeeRepository employeeRepository,
			ICommonServices commonServices,
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
			): base(rdlViewerViewModel, reportInfoFactory)
		{
			UoW = unitOfWorkFactory.CreateWithoutRoot(Title);

			Title = "Отчёт по дате создания заказа";
			Identifier = "Sales.OrderCreationDateReport";

			LoadReportCommand = new DelegateCommand(LoadReport);
			CanEditEmployee = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CanEditEmployeeInOrderCreationDateReport);

			var officeFilter = new EmployeeFilterViewModel();
			officeFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking);

			employeeJournalFactory.SetEmployeeFilterViewModel(officeFilter);
			EmployeeAutocompleteSelectorFactory = employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();

			Employee = employeeRepository.GetEmployeeForCurrentUser(UoW);
		}

		public IEntityAutocompleteSelectorFactory EmployeeAutocompleteSelectorFactory { get; }

		public Employee Employee { get; set; }

		public bool CanEditEmployee { get; }

		public DelegateCommand LoadReportCommand { get; }

		[PropertyChangedAlso(nameof(CanCreateReport))]
		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[PropertyChangedAlso(nameof(CanCreateReport))]
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public bool CanCreateReport => EndDate.HasValue && StartDate.HasValue;

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>
		{
				{ "start_date", StartDate },
				{ "end_date", EndDate },
				{ "employee_id", Employee?.Id ?? 0 }
		};

		public IUnitOfWork UoW { get; }

		public void Dispose()
		{
			UoW.Dispose();			
		}
	}
}
