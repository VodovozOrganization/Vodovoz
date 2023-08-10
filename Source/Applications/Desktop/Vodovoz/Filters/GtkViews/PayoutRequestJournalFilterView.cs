using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using static Vodovoz.ViewModels.Journals.FilterViewModels.PayoutRequestJournalFilterViewModel;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PayoutRequestJournalFilterView : FilterViewBase<PayoutRequestJournalFilterViewModel>
	{
		public PayoutRequestJournalFilterView(PayoutRequestJournalFilterViewModel journalFilterViewModel) : base(journalFilterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			evmeAuthor.SetEntityAutocompleteSelectorFactory(
				ViewModel.EmployeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory());
			evmeAuthor.Binding.AddBinding(ViewModel, vm => vm.Author, w => w.Subject).InitializeFromSource();

			daterangepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull);

			evmeAccountable.SetEntityAutocompleteSelectorFactory(
				ViewModel.EmployeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeAccountable.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.AccountableEmployee, w => w.Subject)
				.AddBinding(vm => vm.CanSetAccountable, w => w.Sensitive)
				.InitializeFromSource();

			yenumcomboStatus.ItemsEnum = typeof(PayoutRequestState);
			yenumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.State, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboStatus.ShowSpecialStateAll = true;

			comboRequestType.ItemsEnum = typeof(PayoutRequestDocumentType);
			comboRequestType.Binding.AddBinding(ViewModel, vm => vm.DocumentType, w => w.SelectedItemOrNull).InitializeFromSource();

			evmeCounterparty.SetEntityAutocompleteSelectorFactory(
				ViewModel.CounterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory());
			evmeCounterparty.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Counterparty, w => w.Subject)
				.AddBinding(vm => vm.CanSetCounterparty, w => w.Sensitive)
				.InitializeFromSource();

			yenumcomboboxSortBy.ItemsEnum = typeof(PayoutDocumentsSortOrder);
			yenumcomboboxSortBy.Binding.AddBinding(ViewModel, vm => vm.DocumentsSortOrder, w => w.SelectedItemOrNull).InitializeFromSource();


			PayoutRequestUserRole? userRole = ViewModel.GetUserRole();
			//Для Роли Согласователя по-умолчанию Создана Подана,
			//для Роли Финансиста - Согласована,
			//для Кассира - Передана на Выдачу,

			//Иные роли - только видят только свои заявки, поэтому нужно скрытиь фильтр по авторам
			if(userRole == PayoutRequestUserRole.Coordinator)
			{
				yenumcomboStatus.SelectedItem = PayoutRequestState.Submited;
			}
			else if(userRole == PayoutRequestUserRole.Financier)
			{
				yenumcomboStatus.SelectedItem = PayoutRequestState.Agreed;
			}
			else if(userRole == PayoutRequestUserRole.Cashier || userRole == PayoutRequestUserRole.Accountant)
			{
				yenumcomboStatus.SelectedItem = PayoutRequestState.GivenForTake;
			}
			else if(userRole == PayoutRequestUserRole.Other)
			{
				evmeAuthor.Visible = false;
				label3.Visible = false;
			}
		}
	}
}
