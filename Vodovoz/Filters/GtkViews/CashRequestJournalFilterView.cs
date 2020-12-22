using System;
using FluentNHibernate.Data;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
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
						var employeeFilter = new EmployeeFilterViewModel{
							Status = EmployeeStatus.IsWorking,
							Category = EmployeeCategory.office
						};
						return new EmployeesJournalViewModel(
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
				w => w.SelectedItem);

			UserRole? userRole = ViewModel.GetUserRole();
			userRole = UserRole.Other;
			//Для Роли Согласователя по-умолчанию Создана Подана,
			//для Роли Финансиста - Согласована,
			//для Кассира - Передана на Выдачу,
			//для остальных - все кроме Закрыта и Отменена
			
			//Иные роли - только видят только свои заявки, поэтому нужно скрытиь фильтр по авторам
			if (userRole == UserRole.Coordinator){
				yenumcomboStatus.SelectedItem = CashRequest.States.Submited;
			} else if (userRole == UserRole.Financier){
				yenumcomboStatus.SelectedItem = CashRequest.States.Agreed;
			} else if (userRole == UserRole.Cashier){
				yenumcomboStatus.SelectedItem = CashRequest.States.GivenForTake;
			} else if (userRole == UserRole.Other){
				// yenumcomboStatus.ItemsEnum = typeof(CashRequest.StatesForOthersFilter); //cannot be converted to type
				AuthorEntityviewmodelentry.Visible = false;
				label3.Visible = false;
			}
		}
	}

}
