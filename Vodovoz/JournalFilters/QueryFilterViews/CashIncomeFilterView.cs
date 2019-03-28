using System;
using QS.DomainModel.UoW;
using QS.RepresentationModel;
using QS.RepresentationModel.GtkUI;
using QS.Tools;
using Vodovoz.Domain.Cash;
using Vodovoz.Filters;
using Vodovoz.ViewModel;
using Gamma.Utilities;
using Gamma.Widgets;
using QS.DomainModel.Config;
using QS.Dialog.GtkUI;

namespace Vodovoz.JournalFilters.QueryFilterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashIncomeFilterView : QueryFilterWidgetBase
	{
		public IncomeFilter Filter { get; set; }

		public CashIncomeFilterView()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Filter = new IncomeFilter();

			ydateperiodPicker.Binding.AddBinding(Filter, x => x.StartDate, w => w.StartDate).InitializeFromSource();
			ydateperiodPicker.Binding.AddBinding(Filter, x => x.EndDate, w => w.EndDate).InitializeFromSource();
			ydateperiodPicker.PeriodChanged += (sender, e) => Refilter();

			var counterpartyFilter = new EmployeeFilter(UoW);
			entryEmployee.RepresentationModel = new EmployeesVM(counterpartyFilter);
			entryEmployee.Binding.AddBinding(Filter, x => x.Employee, w => w.Subject).InitializeFromSource();
			entryEmployee.ChangedByUser += (sender, e) => Refilter();

			var incomeCategoryVM = new EntityCommonRepresentationModelConstructor<IncomeCategory>(UoW)
				.AddSearchColumn("Имя", x => x.Name)
				.AddColumn("Тип документа", x => x.IncomeDocumentType.GetEnumTitle())
				.OrderBy(x => x.Name)
				.Finish();
			entryIncomeCategory.RepresentationModel = incomeCategoryVM;
			entryIncomeCategory.Binding.AddBinding(Filter, x => x.IncomeCategory, w => w.Subject).InitializeFromSource();
			entryIncomeCategory.ChangedByUser += (sender, e) => Refilter();

			var expenseCategoryVM = new EntityCommonRepresentationModelConstructor<ExpenseCategory>(UoW)
				.AddSearchColumn("Имя", x => x.Name)
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
