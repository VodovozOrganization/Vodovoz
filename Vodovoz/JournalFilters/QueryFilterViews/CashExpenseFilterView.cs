using System;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.RepresentationModel.GtkUI;
using QS.Tools;
using Vodovoz.Domain.Cash;
using Vodovoz.Filters;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.JournalFilters.QueryFilterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashExpenseFilterView : QueryFilterWidgetBase
	{
		public ExpenseFilter Filter { get; set; }

		public CashExpenseFilterView()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Filter = new ExpenseFilter();

			ydateperiodPicker.Binding.AddBinding(Filter, x => x.StartDate, w => w.StartDate).InitializeFromSource();
			ydateperiodPicker.Binding.AddBinding(Filter, x => x.EndDate, w => w.EndDate).InitializeFromSource();
			ydateperiodPicker.PeriodChanged += (sender, e) => Refilter();

			var employeeFilter = new EmployeeRepresentationFilterViewModel
			{
				Status = Domain.Employees.EmployeeStatus.IsWorking
			};
			entryEmployee.RepresentationModel = new EmployeesVM(employeeFilter);
			entryEmployee.Binding.AddBinding(Filter, x => x.Employee, w => w.Subject).InitializeFromSource();
			entryEmployee.ChangedByUser += (sender, e) => Refilter();

			var expenseCategoryVM = new EntityCommonRepresentationModelConstructor<ExpenseCategory>(UoW)
				.AddColumn("Имя", x => x.Name).AddSearch(x => x.Name)
				.AddColumn("Тип документа", x => x.ExpenseDocumentType.GetEnumTitle())
				.OrderBy(x => x.Name)
				.Finish();
			entryExpenseCategory.RepresentationModel = expenseCategoryVM;
			entryExpenseCategory.Binding.AddBinding(Filter, x => x.ExpenseCategory, w => w.Subject).InitializeFromSource();
			entryExpenseCategory.ChangedByUser += (sender, e) => Refilter();
		}

		public override IQueryFilter GetQueryFilter()
		{
			return Filter;
		}
	}
}
