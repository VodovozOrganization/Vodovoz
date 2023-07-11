﻿using QS.DomainModel.UoW;
using QS.RepresentationModel.GtkUI;
using QS.Tools;
using Vodovoz.Domain.Cash;
using Vodovoz.Filters;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using Vodovoz.TempAdapters;

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

			var employeeFactory = new EmployeeJournalFactory();
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Binding.AddBinding(Filter, x => x.Employee, w => w.Subject).InitializeFromSource();
			evmeEmployee.ChangedByUser += (sender, e) => Refilter();

			var incomeCategoryVM = new EntityCommonRepresentationModelConstructor<IncomeCategory>(UoW)
				.AddColumn("Имя", x => x.Name).AddSearch(x => x.Name)
				.AddColumn("Тип документа", x => x.IncomeDocumentType.GetEnumTitle())
				.OrderBy(x => x.Name)
				.Finish();
			entryIncomeCategory.RepresentationModel = incomeCategoryVM;
			//entryIncomeCategory.Binding.AddBinding(Filter, x => x.IncomeCategory, w => w.Subject).InitializeFromSource();
			entryIncomeCategory.ChangedByUser += (sender, e) => Refilter();

			var expenseCategoryVM = new EntityCommonRepresentationModelConstructor<ExpenseCategory>(UoW)
				.AddColumn("Имя", x => x.Name).AddSearch(x => x.Name)
				.AddColumn("Тип документа", x => x.ExpenseDocumentType.GetEnumTitle())
				.OrderBy(x => x.Name)
				.Finish();
			entryExpenseCategory.RepresentationModel = expenseCategoryVM;
			//entryExpenseCategory.Binding.AddBinding(Filter, x => x.ExpenseCategory, w => w.Subject).InitializeFromSource();
			entryExpenseCategory.ChangedByUser += (sender, e) => Refilter();
		}

		public override IQueryFilter GetQueryFilter()
		{
			return Filter;
		}
	}
}
