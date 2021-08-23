using System;
using Gamma.Widgets;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.JournalFilters;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using VodovozInfrastructure.Interfaces;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	public partial class CashDocumentsFilter : SubdivisionsAccessJournalFilterBase<CashDocumentsFilter>
	{
		protected override void ConfigureWithUow()
		{
			enumcomboDocumentType.ItemsEnum = typeof(CashDocumentType);
			entryEmployee.RepresentationModel = new ViewModel.EmployeesVM();

			ConfigureEntityViewModelEntry();

			//Последние 30 дней.
			dateperiodDocs.StartDateOrNull = DateTime.Today.AddDays(-30);
			dateperiodDocs.EndDateOrNull = DateTime.Today.AddDays(1);
		}

		private void ConfigureEntityViewModelEntry()
		{
			var incomeCategoryFilter = new IncomeCategoryJournalFilterViewModel();
			var expenseCategoryFilter = new ExpenseCategoryJournalFilterViewModel {
				ExcludedIds = new CategoryRepository(new ParametersProvider()).ExpenseSelfDeliveryCategories(UoW).Select(x => x.Id),
				HidenByDefault = true
			};
			
			var commonServices = ServicesConfig.CommonServices;
			IFileChooserProvider chooserIncomeProvider = new FileChooser("Приход " + DateTime.Now + ".csv");
			IFileChooserProvider chooserExpenseProvider = new FileChooser("Расход " + DateTime.Now + ".csv");
			var employeeJournalFactory = new EmployeeJournalFactory();
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			
			var incomeCategoryAutocompleteSelectorFactory =
				new SimpleEntitySelectorFactory<IncomeCategory, IncomeCategoryViewModel>(
					() =>
					{
						var incomeCategoryJournalViewModel =
							new SimpleEntityJournalViewModel<IncomeCategory, IncomeCategoryViewModel>(
								x => x.Name,
								() => new IncomeCategoryViewModel(
									EntityUoWBuilder.ForCreate(),
									UnitOfWorkFactory.GetDefaultFactory,
									commonServices,
									chooserIncomeProvider,
									incomeCategoryFilter,
									employeeJournalFactory,
									subdivisionJournalFactory
								),
								node => new IncomeCategoryViewModel(
									EntityUoWBuilder.ForOpen(node.Id),
									UnitOfWorkFactory.GetDefaultFactory,
									commonServices,
									chooserIncomeProvider,
									incomeCategoryFilter,
									employeeJournalFactory,
									subdivisionJournalFactory
								),
								UnitOfWorkFactory.GetDefaultFactory,
								commonServices
							)
							{
								SelectionMode = JournalSelectionMode.Single
							};
						return incomeCategoryJournalViewModel;
					});
			

			var expenseCategoryAutocompleteSelectorFactory =
				new SimpleEntitySelectorFactory<ExpenseCategory, ExpenseCategoryViewModel>(
					() =>
					{
						var expenseCategoryJournalViewModel =
							new SimpleEntityJournalViewModel<ExpenseCategory, ExpenseCategoryViewModel>(
								x => x.Name,
								() => new ExpenseCategoryViewModel(
									EntityUoWBuilder.ForCreate(),
									UnitOfWorkFactory.GetDefaultFactory,
									ServicesConfig.CommonServices,
									chooserExpenseProvider,
									expenseCategoryFilter,
									employeeJournalFactory,
									subdivisionJournalFactory
								),
								node => new ExpenseCategoryViewModel(
									EntityUoWBuilder.ForOpen(node.Id),
									UnitOfWorkFactory.GetDefaultFactory,
									ServicesConfig.CommonServices,
									chooserExpenseProvider,
									expenseCategoryFilter,
									employeeJournalFactory,
									subdivisionJournalFactory
								),
								UnitOfWorkFactory.GetDefaultFactory,
								ServicesConfig.CommonServices
							)
							{
								SelectionMode = JournalSelectionMode.Single
							};
						expenseCategoryJournalViewModel.SetFilter(expenseCategoryFilter,
							filter => Restrictions.Not(Restrictions.In("Id", filter.ExcludedIds.ToArray())));

						return expenseCategoryJournalViewModel;
					});
			
			entityVMEntryCashIncomeCategory.SetEntityAutocompleteSelectorFactory(incomeCategoryAutocompleteSelectorFactory);
			entityVMEntryCashExpenseCategory.SetEntityAutocompleteSelectorFactory(expenseCategoryAutocompleteSelectorFactory);
		}

		public CashDocumentsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public CashDocumentsFilter()
		{
			this.Build();
			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;
		}

		private IEnumerable<Subdivision> allowedSubdivisions;
		protected override IEnumerable<Subdivision> AllowedSubdivisions {
			get { return allowedSubdivisions; }
			set {
				allowedSubdivisions = value;
				UpdateSubdivisionsWidget();
			}
		}

		private void UpdateSubdivisionsWidget()
		{
			accessfilteredsubdivisionselectorwidget.NeedChooseSubdivision = ShowSubdivisions;
			accessfilteredsubdivisionselectorwidget.Configure(UoW, AllowedSubdivisions);
		}

		public IEnumerable<Subdivision> SelectedSubdivisions => Subdivisions.Distinct();

		protected override IEnumerable<Subdivision> Subdivisions { 
			get {
				if(!ShowSubdivisions) {
					return base.Subdivisions;
				}
				if(accessfilteredsubdivisionselectorwidget.SelectedSubdivision != null) {
					return new Subdivision[] { accessfilteredsubdivisionselectorwidget.SelectedSubdivision };
				}
				if(accessfilteredsubdivisionselectorwidget.AllSelected) {
					return AllowedSubdivisions;
				}
				return new Subdivision[0];
			} 
		}

		public CashDocumentType? RestrictDocumentType {
			get { return enumcomboDocumentType.SelectedItem as CashDocumentType?; }
			set {
				enumcomboDocumentType.SelectedItem = value;
				enumcomboDocumentType.Sensitive = false;
			}
		}

		public ExpenseCategory RestrictExpenseCategory {
			get => entityVMEntryCashExpenseCategory.Subject as ExpenseCategory;
			set {
				entityVMEntryCashExpenseCategory.Subject = value;
				entityVMEntryCashExpenseCategory.Sensitive = false;
			}
		}

		public IncomeCategory RestrictIncomeCategory {
			get => entityVMEntryCashIncomeCategory.Subject as IncomeCategory;
			set {
				entityVMEntryCashIncomeCategory.Subject = value;
				entityVMEntryCashIncomeCategory.Sensitive = false;
			}
		}

		public Employee RestrictEmployee {
			get { return entryEmployee.Subject as Employee; }
			set {
				entryEmployee.Subject = value;
				entryEmployee.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return dateperiodDocs.StartDateOrNull; }
			set {
				dateperiodDocs.StartDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return dateperiodDocs.EndDateOrNull; }
			set {
				dateperiodDocs.EndDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		protected void OnEnumcomboDocumentTypeEnumItemSelected(object sender, ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodDocsPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntryEmployeeChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		void Accessfilteredsubdivisionselectorwidget_OnSelected(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntityVMEntryCashIncomeCategoryChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntityVMEntryCashExpenseCategoryChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

