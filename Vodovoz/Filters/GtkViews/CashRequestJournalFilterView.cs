using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashRequestJournalFilterView : FilterViewBase<CashRequestJournalFilterViewModel>
	{
		public CashRequestJournalFilterView(CashRequestJournalFilterViewModel journalFilterViewModel) : base(journalFilterViewModel)
		{
			this.Build();
			this.Configure();
		}

		private void Configure()
		{
			AuthorEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() =>
					{
						var employeeFilter = new EmployeeFilterViewModel
						{
							Status = EmployeeStatus.IsWorking,
							Category = EmployeeCategory.office
						};
						var journalActions = 
							new EmployeesJournalActionsViewModel(ServicesConfig.InteractiveService, UnitOfWorkFactory.GetDefaultFactory);
						
						return new EmployeesJournalViewModel(
							journalActions,
							employeeFilter,
							UnitOfWorkFactory.GetDefaultFactory, 
							ServicesConfig.CommonServices);
					})
			);

			daterangepicker.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull);
			daterangepicker.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull);
			
			AuthorEntityviewmodelentry.Binding.AddBinding(
				ViewModel, 
				vm => vm.Author,
				w => w.Subject
			).InitializeFromSource();

			
			AccountableEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>
					(ServicesConfig.CommonServices)
			);
			
			AccountableEntityviewmodelentry.Binding.AddBinding(
				ViewModel,
				vm => vm.AccountableEmployee,
				w => w.Subject
			).InitializeFromSource();

			yenumcomboStatus.ItemsEnum = typeof(CashRequest.States);
			yenumcomboStatus.Binding.AddBinding(
				ViewModel,
				e => e.State,
				w => w.SelectedItemOrNull);
			yenumcomboStatus.ShowSpecialStateAll = true;
			

			CashRequestUserRole? userRole = ViewModel.GetUserRole();
			//Для Роли Согласователя по-умолчанию Создана Подана,
			//для Роли Финансиста - Согласована,
			//для Кассира - Передана на Выдачу,
			
			//Иные роли - только видят только свои заявки, поэтому нужно скрытиь фильтр по авторам
			if (userRole == CashRequestUserRole.Coordinator){
				yenumcomboStatus.SelectedItem = CashRequest.States.Submited;
			} else if (userRole == CashRequestUserRole.Financier){
				yenumcomboStatus.SelectedItem = CashRequest.States.Agreed;
			} else if (userRole == CashRequestUserRole.Cashier){
				yenumcomboStatus.SelectedItem = CashRequest.States.GivenForTake;
			} else if (userRole == CashRequestUserRole.Other){
				AuthorEntityviewmodelentry.Visible = false;
				label3.Visible = false;
			}
		}
	}

}
